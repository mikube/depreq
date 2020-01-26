using System;
using Depreq;
using Xunit;

namespace Depreq.Test
{
    public class TagUpdaterTest
    {
        [Theory]
        [InlineData("host:port/owner/name:tag", "newTag", "host:port/owner/name:newTag")]
        [InlineData("host/owner/name:tag", "newTag", "host/owner/name:newTag")]
        [InlineData("owner/name:tag", "newTag", "owner/name:newTag")]
        [InlineData("tag", "newTag", "newTag")]
        public void TagReplacerTest(string input, string newTag, string expected)
        {
            var replaced = TagUpdater.TagReplacer(input, newTag);
            Assert.Equal(expected, replaced);
        }

        [Theory]
        [InlineData("host:port/owner/name:tag", "tag")]
        [InlineData("host/owner/name:tag", "tag")]
        [InlineData("owner/name:tag", "tag")]
        [InlineData("tag", "tag")]
        public void ExtractTagTest(string input, string expected)
        {
            var tag = TagUpdater.ExtractTag(input);
            Assert.Equal(expected, tag);
        }
    }
}
