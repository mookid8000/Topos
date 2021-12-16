using System;
using NUnit.Framework;
using Topos.Extensions;

namespace Topos.Tests.Extensions;

[TestFixture]
public class TestTypeExtensions
{
    [Test]
    public void CanGetTypeNameAsExpected()
    {
        CheckType<string>();
        CheckType<SomeMessageType>();
        CheckType<SomeGenericType<SomeMessageType>>();
    }

    static void CheckType<T>()
    {
        var actualTypeName = typeof(T).GetSimpleAssemblyQualifiedTypeName();
        var type = actualTypeName.ParseType();

        Console.WriteLine($"{typeof(T)} => {actualTypeName}");

        Assert.That(type, Is.EqualTo(typeof(T)));
    }
}

class SomeMessageType { }

class SomeGenericType<T> { }