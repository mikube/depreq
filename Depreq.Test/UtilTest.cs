using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Xunit;
using Depreq;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

namespace Depreq.Test
{
    public class UtilTest
    {
        [Theory]
        [InlineData(
            @"image:
  repository: host:port/owner/name:1234567890abcdef",
            new string[] { "image.repository" },
            "123abc",
            @"image:
  repository: host:port/owner/name:123abc
...
")]
        [InlineData(
            @"image:
  repository: ""host:port/owner/name:1234567890abcdef""",
            new string[] { "image.repository" },
            "123abc",
            @"image:
  repository: ""host:port/owner/name:123abc""
...
")]
        [InlineData(
            @"image:
  repository: 'host:port/owner/name:1234567890abcdef'",
            new string[] { "image.repository" },
            "123abc",
            @"image:
  repository: 'host:port/owner/name:123abc'
...
")]
        [InlineData(
            @"image:
  repository: host/owner/name:1234567890abcdef",
            new string[] { "image.repository" },
            "123abc",
            @"image:
  repository: host/owner/name:123abc
...
")]
        [InlineData(
            @"image:
  repository: ""host/owner/name:1234567890abcdef""",
            new string[] { "image.repository" },
            "123abc",
            @"image:
  repository: ""host/owner/name:123abc""
...
")]
        [InlineData(
            @"image:
  repository: 'host/owner/name:1234567890abcdef'",
            new string[] { "image.repository" },
            "123abc",
            @"image:
  repository: 'host/owner/name:123abc'
...
")]
        [InlineData(
            @"image:
  repository: owner/name:1234567890abcdef",
            new string[] { "image.repository" },
            "123abc",
            @"image:
  repository: owner/name:123abc
...
")]
        [InlineData(
            @"image:
  repository: ""owner/name:1234567890abcdef""",
            new string[] { "image.repository" },
            "123abc",
            @"image:
  repository: ""owner/name:123abc""
...
")]
        [InlineData(
            @"image:
  repository: 'owner/name:1234567890abcdef'",
            new string[] { "image.repository" },
            "123abc",
            @"image:
  repository: 'owner/name:123abc'
...
")]
        [InlineData(
            @"tag: 1234567890abcdef",
            new string[] { "tag" },
            "123abc",
            @"tag: 123abc
...
")]
        [InlineData(
            @"tag: ""1234567890abcdef""",
            new string[] { "tag" },
            "123abc",
            @"tag: ""123abc""
...
")]
        [InlineData(
            @"tag: '1234567890abcdef'",
            new string[] { "tag" },
            "123abc",
            @"tag: '123abc'
...
")]
        [InlineData(
            @"spec:
  template:
    spec:
      containers:
      - name: app-1
        image: 'host:port/owner/name:1234567890abcdef'
      - name: app-2
        image: 'host:port/owner/name:1234567890abcdef'",
            new string[] { "spec.template.spec.containers[*].image" },
            "123abc",
            @"spec:
  template:
    spec:
      containers:
      - name: app-1
        image: 'host:port/owner/name:123abc'
      - name: app-2
        image: 'host:port/owner/name:123abc'
...
")]
        //         [InlineData(
        //             @"spec:
        //   template:
        //     spec:
        //       containers:
        //       - - image: 'host:port/owner/name:1234567890abcdef'
        //       - - image: 'host:port/owner/name:1234567890abcdef'",
        //             new string[] { "spec.template.spec.containers[*].[*].image" },
        //             "123abc",
        //             @"spec:
        //   template:
        //     spec:
        //       containers:
        //       - - image: 'host:port/owner/name:123abc'
        //       - - image: 'host:port/owner/name:123abc'
        // ...
        // ")] // Nested array is not supported
        public void UpdateByNestedKeyTest(string yaml, string[] nestedKeys, string newTag, string expected)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            var sr = new StringReader(yaml);
            var ys = new YamlStream();
            ys.Load(sr);

            foreach (var doc in ys.Documents)
            {
                foreach (var nestedKey in nestedKeys)
                {
                    var oldVal = YamlUtil.UpdateByNestedKey(
                        doc.RootNode,
                        nestedKey.Split("."),
                        oldVal =>
                            TagUpdater.TagReplacer(oldVal, newTag)
                    );
                }
            }
            ys.Save(sw, false);
            Assert.Equal(expected, sw.ToString());
        }


        class Piyo
        {
            public Uri Foo { get; set; }
            public string Bar { get; set; }
        }

        [Theory]
        [InlineData(@"aaa{Foo}bbb{Bar}ccc", "https://example.com", "nyan", "aaahttps://example.com/bbbnyanccc")]
        [InlineData(@"aaa{Foo}bbb{Bar}ccc", "https://example.com/", "nyan", "aaahttps://example.com/bbbnyanccc")]
        [InlineData(@"aaa{Foo}bbb{Bar}ccc", "https://example.com/foo", "nyan", "aaahttps://example.com/foobbbnyanccc")]
        [InlineData(@"aaa{Foo#3}bbb{Bar#3}ccc", "https://example.com", "nyan", "aaahttbbbnyaccc")]
        public void ApplyTest(string template, string foo, string bar, string expected)
        {
            var piyo = new Piyo
            {
                Foo = new Uri(foo),
                Bar = bar
            };

            Assert.Equal(expected, TemplateUtil.Apply(piyo, template));
        }
    }
}
