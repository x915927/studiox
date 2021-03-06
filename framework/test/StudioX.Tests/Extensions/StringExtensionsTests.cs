﻿using System;
using System.Globalization;
using StudioX.Extensions;
using StudioX.Localization;
using Shouldly;
using Xunit;

namespace StudioX.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void EnsureEndsWithTest()
        {
            //Expected use-cases
            "Test".EnsureEndsWith('!').ShouldBe("Test!");
            "Test!".EnsureEndsWith('!').ShouldBe("Test!");
            @"C:\test\folderName".EnsureEndsWith('\\').ShouldBe(@"C:\test\folderName\");
            @"C:\test\folderName\".EnsureEndsWith('\\').ShouldBe(@"C:\test\folderName\");

            //Case differences
            "TurkeY".EnsureEndsWith('y').ShouldBe("TurkeYy");
            "TurkeY".EnsureEndsWith('y', StringComparison.OrdinalIgnoreCase).ShouldBe("TurkeY");

            //Edge cases for Turkish 'i'.
#if NET46
            "TAKSİ".EnsureEndsWith('i', true, new CultureInfo("tr-TR")).ShouldBe("TAKSİ");
            "TAKSİ".EnsureEndsWith('i', false, new CultureInfo("tr-TR")).ShouldBe("TAKSİi");
#else
            using (CultureInfoHelper.Use("tr-TR"))
            {
                "TAKSİ".EnsureEndsWith('i', StringComparison.CurrentCultureIgnoreCase).ShouldBe("TAKSİ");
                "TAKSİ".EnsureEndsWith('i', StringComparison.CurrentCulture).ShouldBe("TAKSİi");
            }
#endif
        }

        [Fact]
        public void EnsureStartsWithTest()
        {
            //Expected use-cases
            "Test".EnsureStartsWith('~').ShouldBe("~Test");
            "~Test".EnsureStartsWith('~').ShouldBe("~Test");

            //Case differences
            "Turkey".EnsureStartsWith('t').ShouldBe("tTurkey");
            "Turkey".EnsureStartsWith('t', StringComparison.OrdinalIgnoreCase).ShouldBe("Turkey");

            //Edge cases for Turkish 'i'.
#if NET46
            "İstanbul".EnsureStartsWith('i', true, new CultureInfo("tr-TR")).ShouldBe("İstanbul");
            "İstanbul".EnsureStartsWith('i', false, new CultureInfo("tr-TR")).ShouldBe("iİstanbul");
#else
            using (CultureInfoHelper.Use("tr-TR"))
            {
                "İstanbul".EnsureStartsWith('i', StringComparison.CurrentCultureIgnoreCase).ShouldBe("İstanbul");
                "İstanbul".EnsureStartsWith('i', StringComparison.CurrentCulture).ShouldBe("iİstanbul");
            }
#endif
        }

        [Fact]
        public void ToPascalCaseTest()
        {
            (null as string).ToPascalCase().ShouldBe(null);
            "helloWorld".ToPascalCase().ShouldBe("HelloWorld");
            "istanbul".ToPascalCase().ShouldBe("Istanbul");
#if NET46
            "istanbul".ToPascalCase(new CultureInfo("tr-TR")).ShouldBe("İstanbul");
#endif
        }

        [Fact]
        public void ToCamelCaseTest()
        {
            (null as string).ToCamelCase().ShouldBe(null);
            "HelloWorld".ToCamelCase().ShouldBe("helloWorld");
            "Istanbul".ToCamelCase().ShouldBe("istanbul");

#if NET46
            "Istanbul".ToCamelCase(new CultureInfo("tr-TR")).ShouldBe("ıstanbul");
            "İstanbul".ToCamelCase(new CultureInfo("tr-TR")).ShouldBe("istanbul");
#endif
        }

        [Fact]
        public void ToSentenceCaseTest()
        {
            (null as string).ToSentenceCase().ShouldBe(null);
            "HelloWorld".ToSentenceCase().ShouldBe("Hello world");

            using (CultureInfoHelper.Use("en-US"))
            {
                "HelloIsparta".ToSentenceCase().ShouldBe("Hello isparta");
            }

#if NET46
            "HelloIsparta".ToSentenceCase(new CultureInfo("tr-TR")).ShouldBe("Hello ısparta");
#endif
        }

        [Fact]
        public void RightTest()
        {
            const string str = "This is a test string";

            str.Right(3).ShouldBe("ing");
            str.Right(0).ShouldBe("");
            str.Right(str.Length).ShouldBe(str);
        }

        [Fact]
        public void LeftTest()
        {
            const string str = "This is a test string";

            str.Left(3).ShouldBe("Thi");
            str.Left(0).ShouldBe("");
            str.Left(str.Length).ShouldBe(str);
        }

        [Fact]
        public void NormalizeLineEndingsTest()
        {
            const string str = "This\r\n is a\r test \n string";
            var normalized = str.NormalizeLineEndings();
            var lines = normalized.SplitToLines();
            lines.Length.ShouldBe(4);
        }

        [Fact]
        public void NthIndexOfTest()
        {
            const string str = "This is a test string";

            str.NthIndexOf('i', 0).ShouldBe(-1);
            str.NthIndexOf('i', 1).ShouldBe(2);
            str.NthIndexOf('i', 2).ShouldBe(5);
            str.NthIndexOf('i', 3).ShouldBe(18);
            str.NthIndexOf('i', 4).ShouldBe(-1);
        }

        [Fact]
        public void TruncateTest()
        {
            const string str = "This is a test string";
            const string nullValue = null;

            str.Truncate(7).ShouldBe("This is");
            str.Truncate(0).ShouldBe("");
            str.Truncate(100).ShouldBe(str);

            nullValue.Truncate(5).ShouldBe(null);
        }

        [Fact]
        public void TruncateWithPostFixTest()
        {
            const string str = "This is a test string";
            const string nullValue = null;

            str.TruncateWithPostfix(3).ShouldBe("...");
            str.TruncateWithPostfix(12).ShouldBe("This is a...");
            str.TruncateWithPostfix(0).ShouldBe("");
            str.TruncateWithPostfix(100).ShouldBe(str);

            nullValue.Truncate(5).ShouldBe(null);

            str.TruncateWithPostfix(3, "~").ShouldBe("Th~");
            str.TruncateWithPostfix(12, "~").ShouldBe("This is a t~");
            str.TruncateWithPostfix(0, "~").ShouldBe("");
            str.TruncateWithPostfix(100, "~").ShouldBe(str);

            nullValue.TruncateWithPostfix(5, "~").ShouldBe(null);
        }

        [Fact]
        public void RemovePostFixTests()
        {
            //null case
            (null as string).RemovePreFix("Test").ShouldBeNull();

            //Simple case
            "MyTestAppService".RemovePostFix("AppService").ShouldBe("MyTest");
            "MyTestAppService".RemovePostFix("Service").ShouldBe("MyTestApp");

            //Multiple postfix (orders of postfixes are important)
            "MyTestAppService".RemovePostFix("AppService", "Service").ShouldBe("MyTest");
            "MyTestAppService".RemovePostFix("Service", "AppService").ShouldBe("MyTestApp");

            //Unmatched case
            "MyTestAppService".RemovePostFix("Unmatched").ShouldBe("MyTestAppService");
        }

        [Fact]
        public void RemovePreFixTests()
        {
            "Home.Index".RemovePreFix("NotMatchedPostfix").ShouldBe("Home.Index");
            "Home.About".RemovePreFix("Home.").ShouldBe("About");
        }

        [Fact]
        public void ToEnumTest()
        {
            "MyValue1".ToEnum<MyEnum>().ShouldBe(MyEnum.MyValue1);
            "MyValue2".ToEnum<MyEnum>().ShouldBe(MyEnum.MyValue2);
        }

        private enum MyEnum
        {
            MyValue1,
            MyValue2
        }
    }
}