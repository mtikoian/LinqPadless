namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using LinqPadless;
    using Markdig;
    using Markdig.Syntax;
    using Markdig.Syntax.Inlines;
    using MoreLinq.Extensions;
    using Xunit;
    using static MoreLinq.Extensions.ZipLongestExtension;

    public class ScriptTests
    {
        public enum QueryKind { Expression, Statements, Program }

        [Theory]
        [MemberData(nameof(TestSource))]
        public void Test(TestRecord record)
        {
            var kind = record.QueryKind;
            var source = record.Source;
            var expected = record.Expected;

            var language = kind switch
            {
                QueryKind.Expression => LinqPadQueryLanguage.Expression,
                QueryKind.Statements => LinqPadQueryLanguage.Statements,
                QueryKind.Program => LinqPadQueryLanguage.Program
            };

            var (meta, code) = Script.Transpile(language, source);

            foreach (var (i, (exp, act)) in
                Regex.Split(expected, @"\r?\n")
                     .ZipLongest(Regex.Split(string.Join(Environment.NewLine, meta, string.Empty, code), @"\r?\n"),
                                 ValueTuple.Create)
                     .Index(1))
            {
                Assert.Equal((i, exp), (i, act));
            }

            Assert.Equal(
                Regex.Split(expected, @"\r?\n"),
                Regex.Split(string.Join(Environment.NewLine, meta, string.Empty, code), @"\r?\n"));
        }

        public sealed class TestRecord
        {
            public string    Title     { get; }
            public QueryKind QueryKind { get; }
            public string    Source    { get; }
            public string    Expected  { get; }

            public TestRecord(string title, QueryKind queryKind, string source, string expected)
            {
                Title = title;
                QueryKind = queryKind;
                Source = source;
                Expected = expected;
            }

            public override string ToString() => Title;
        }

        public static TheoryData<TestRecord> TestSource()
        {
            using var stream = typeof(ScriptTests).Assembly.GetManifestResourceStream(typeof(ScriptTests), nameof(ScriptTests) + ".md");
            using var reader = new StreamReader(stream);
            var md = reader.ReadToEnd();
            var document = Markdown.Parse(md);

            var data = new TheoryData<TestRecord>();

            using var be = document.AsEnumerable().GetEnumerator();
            while (be.TryRead(out var block))
            {
                if (block is HeadingBlock heading && heading.Level == 2)
                {
                    var title = ((LiteralInline)heading.Inline.Single()).Content.ToString();
                    var kind = default(QueryKind?);
                    var source = default(string);
                    var expected = default(string);

                    while (be.TryRead(out block) && !(block is HeadingBlock))
                    {
                        if (block is ParagraphBlock paragraph
                            && paragraph.Inline.SingleOrDefault() is LiteralInline literal)
                        {
                            var i = block.Parent.IndexOf(block);
                            var nextBlock = i + 1 < block.Parent.Count ? block.Parent[i + 1] : null;
                            switch (literal.Content.ToString().Replace(" ", null).ToLowerInvariant())
                            {
                                case "suppose:" when nextBlock is ListBlock list:
                                {
                                    var kinds =
                                        from ListItemBlock item in list
                                        select item.SingleOrDefault() is ParagraphBlock paragraph
                                               && paragraph.Inline.SingleOrDefault() is LiteralInline literal
                                            ? Regex.Match(literal.Content.ToString(), @"(\w+)\s+is\s+(\w+)")
                                            : null
                                        into m
                                        where m?.Success ?? false
                                        select KeyValuePair.Create(m.Groups[1].Value, m.Groups[2].Value) into e
                                        where "kind".Equals(e.Key, StringComparison.OrdinalIgnoreCase)
                                        select e.Value;
                                    kind = Enum.Parse<QueryKind>(kinds.Last(), true);
                                    break;
                                }
                                case "source:" when nextBlock is FencedCodeBlock code:
                                {
                                    source = code.Lines.ToSlice().ToString();
                                    break;
                                }
                                case "expected:" when nextBlock is FencedCodeBlock code:
                                {
                                    expected = code.Lines.ToSlice().ToString();
                                    break;
                                }
                            }

                            if (kind is null || source is null || expected is null)
                                continue;

                            data.Add(new TestRecord(title, kind.Value, source, expected));
                            break;
                        }
                    }
                }
            }

            return data;
        }
    }
}
