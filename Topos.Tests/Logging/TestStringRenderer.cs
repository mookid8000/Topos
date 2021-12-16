using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Topos.Logging.Console;

namespace Topos.Tests.Logging;

[TestFixture]
public class TestStringRenderer : ToposFixtureBase
{
    StringRenderer _renderer;

    protected override void SetUp()
    {
        _renderer = new StringRenderer();
    }

    [TestCaseSource(nameof(GetCases))]
    public void CanRenderString(RenderCase renderCase)
    {
        var result = Render(renderCase.Template, renderCase.Args);

        Assert.That(result, Is.EqualTo(renderCase.ExpectedResult));
    }

    class RandomObject
    {
        public RandomObject(string text, int number)
        {
            Text = text;
            Number = number;
        }

        public string Text { get;  }
        public int Number { get;  }
    }

    static IEnumerable<RenderCase> GetCases()
    {
        yield return new RenderCase(@"object { Number = 23, Text = ""hej med dig"" }", "object {@obj}", new RandomObject("hej med dig", 23));
        yield return new RenderCase(@"stats { Elapsed = 00:00:02, Text = ""hej"", Whatever = 0.2 }", "stats {@stats}", new { Elapsed = TimeSpan.FromSeconds(2), Text = "hej", Whatever = 0.2});
        yield return new RenderCase(@"hej ""ven""", "hej {navn}", "ven");
        yield return new RenderCase(@"hej ""ven"" og ""igen""", "hej {navn} og {navn}", "ven", "igen");
    }

    public class RenderCase
    {
        public string ExpectedResult { get; }
        public string Template { get; }
        public object[] Args { get; }

        public RenderCase(string expectedResult, string template, params object[] args)
        {
            ExpectedResult = expectedResult;
            Template = template;
            Args = args;
        }

        public override string ToString() => $"{Template} + {string.Join(", ", Args)} = {ExpectedResult}";
    }

    string Render(string template, params object[] args)
    {
        var result = _renderer.RenderString(template, args);

        Console.WriteLine($@"
    {template}

    +

{string.Join(Environment.NewLine, args.Select(a => $"    {a}"))}

    => {result}");

        return result;
    }
}