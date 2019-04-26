using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Topos.Logging;

namespace Topos.Internals
{
    class FileEventBuffer : IDisposable
    {
        const string LineTerminator = "#";
        static readonly Encoding TextEncoding = Encoding.UTF8;
        readonly string _directory;
        readonly ILogger _logger;

        bool _disposed;

        public FileEventBuffer(string directory, ILoggerFactory loggerFactory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _logger = loggerFactory.GetLogger(typeof(FileEventBuffer));

            InitializeDirectory(_directory);
        }

        void InitializeDirectory(string directory)
        {
            CreateDirectory(directory);

            VerifyWritability(directory);
        }

        void VerifyWritability(string directory)
        {
            var testFilePath = Path.Combine(directory, "__test__.txt");

            try
            {
                const string sillyText = @"This file was created by the Fleet Manager client.

It's just here to verify that the current process has read/write access to the directory.

If it's still here, it is probably because something went wrong.

Please delete the file when you don't feel like looking at it anymore.";

                _logger.Debug("Verifying writability of directory {directoryPath}", directory);

                _logger.Debug("Writing text to file {testFilePath}", testFilePath);

                File.WriteAllText(testFilePath, sillyText, Encoding.UTF8);

                var roundtrippedText = File.ReadAllText(testFilePath, Encoding.UTF8);

                if (!string.Equals(roundtrippedText, sillyText))
                {
                    throw new IOException("Read/write test failed");
                }

                _logger.Debug("Written text successfully roundtripped");
            }
            catch (Exception exception)
            {
                throw new IOException($"Could not complete read/write test in directory {directory}", exception);
            }
            finally
            {
                try
                {
                    _logger.Debug("Deleting file {testFilePath}", testFilePath);

                    File.Delete(testFilePath);
                }
                catch { }
            }
        }

        void CreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                _logger.Debug("Directory {directoryPath} does not exist", directory);

                try
                {
                    _logger.Debug("Creating directory {directoryPath}", directory);

                    Directory.CreateDirectory(directory);
                }
                catch (IOException exception)
                {
                    if (!Directory.Exists(directory))
                    {
                        throw new IOException($"Could not create buffer directory {directory}", exception);
                    }
                }
            }
        }

        public void Append(IEnumerable<string> lines)
        {
            var writer = GetWriter();
            var linesWritten = 0;

            foreach (var line in lines)
            {
#if DEBUG
                if (line.Contains(Environment.NewLine))
                {
                    throw new ArgumentException($"Line contains illegal characters: {line}");
                }
#endif
                writer.WriteLine(line + LineTerminator);

                _linesWrittenWithCurrentWriter++;
                linesWritten++;

                if (_linesWrittenWithCurrentWriter >= MaxLinesPerFile)
                {
                    _logger.Debug("Flushing writer because max line count {maxLineCount} was reached", _linesWrittenWithCurrentWriter);

                    writer.Flush();
                    writer = GetWriter();
                }
            }

            _logger.Debug("Wrote {lineCount} lines", linesWritten);

            writer.Flush();
        }

        public int MaxLinesPerFile { get; set; } = 300000;

        public ReadResult Read(int maxLinesToRead = 10000)
        {
            while (true)
            {
                var reader = GetReader();

                if (reader == null) return ReadResult.Empty;

                var result = GetReadResult(reader, maxLinesToRead);

                if (!result.IsEmpty) return result;

                // is it time to advance to the next file?
                var previousFilePath = GetFilePath(_currentReadFileNumber);
                var nextFileNumber = _currentReadFileNumber + 1;
                var nextFilePath = GetFilePath(nextFileNumber);

                // if the file is not there yet, we need to stick around
                if (!File.Exists(nextFilePath)) return ReadResult.Empty;

                // move pointer to next file
                SavePosition(0, nextFilePath);

                // clean up old file
                File.Delete(previousFilePath);
            }
        }

        ReadResult GetReadResult(StreamReader reader, int maxLinesToRead)
        {
            using (reader)
            {
                var lines = new List<string>();
                var correction = 0;

                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    if (!line.EndsWith(LineTerminator))
                    {
                        // don't make the incomplete line count in the byte position
                        correction = TextEncoding.GetByteCount(line);
                        break;
                    }

                    var lineWithoutTrailingExclamation = line.Substring(0, line.Length - 1);

                    lines.Add(lineWithoutTrailingExclamation);

                    if (lines.Count == maxLinesToRead) break;
                }

                if (!lines.Any()) return ReadResult.Empty;

                var bytePosition = reader.GetBytePosition();
                var resumePosition = bytePosition - correction;
                var currentFileName = GetFilePath(_currentReadFileNumber);

                _logger.Debug("Reader retrieved {lineCount} lines", lines.Count);

                return new ReadResult(lines, () =>
                {
                    SavePosition(resumePosition, currentFileName);
                }, _currentReadFileNumber, resumePosition);
            }
        }

        void SavePosition(int bytePosition, string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var text = $"{fileName}:::{bytePosition}";

            File.WriteAllText(GetPositionFileName(), text);

            _logger.Debug("Updated reader position to {position}", text);
        }

        string GetPositionFileName()
        {
            return Path.Combine(_directory, "position.txt");
        }

        StreamReader GetReader()
        {
            var positionFileName = GetPositionFileName();

            if (File.Exists(positionFileName))
            {
                var text = File.ReadAllText(positionFileName);
                var parts = text.Split(new[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var filePath = Path.Combine(_directory, parts[0]);
                var bufferFile = new BufferFile(filePath);
                var bytePosition = int.Parse(parts[1]);

                _currentReadFileNumber = bufferFile.Number;

                return OpenReader(filePath, bytePosition);
            }

            var firstFile = Directory.GetFiles(_directory, "data-*.log")
                .Select(file => new BufferFile(file))
                .OrderBy(file => file.Number)
                .FirstOrDefault(file => file.Valid);

            if (firstFile == null) return null;

            _currentReadFileNumber = firstFile.Number;

            return OpenReader(firstFile.FilePath, 0);
        }

        static StreamReader OpenReader(string filePath, long initialPosition)
        {
            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            fileStream.Position = initialPosition;
            return new StreamReader(fileStream, TextEncoding);
        }

        int _linesWrittenWithCurrentWriter;
        StreamWriter _currentWriter;
        int _currentReadFileNumber;

        StreamWriter GetWriter()
        {
            if (_currentWriter != null)
            {
                if (_linesWrittenWithCurrentWriter < MaxLinesPerFile)
                {
                    return _currentWriter;
                }

                _currentWriter.Dispose();
                _linesWrittenWithCurrentWriter = 0;
            }

            var lastFile = Directory.GetFiles(_directory, "data-*.log")
                .Select(file => new BufferFile(file))
                .OrderBy(file => file.Number)
                .LastOrDefault(file => file.Valid);

            var number = lastFile?.Number + 1 ?? 0;
            var filePath = GetFilePath(number);

            _currentWriter = new StreamWriter(File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), TextEncoding);

            _logger.Debug("Creating new line writer for file {filePath}", filePath);

            _linesWrittenWithCurrentWriter = 0;

            return _currentWriter;
        }

        string GetFilePath(int number)
        {
            return Path.Combine(_directory, $"data-{number}.log");
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _currentWriter?.Dispose();
            }
            finally
            {
                _currentWriter = null;
                _disposed = true;
            }
        }

        class BufferFile
        {
            public BufferFile(string filePath)
            {
                FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

                var name = Path.GetFileNameWithoutExtension(filePath);
                var parts = name.Split('-');
                if (parts.Length != 2) return;

                if (parts[0] != "data") return;

                if (!int.TryParse(parts[1], out var number)) return;

                Valid = true;
                Number = number;
            }

            public bool Valid { get; }
            public int Number { get; }
            public string FilePath { get; }
        }
    }
}