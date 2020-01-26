using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.RepresentationModel;

namespace Depreq
{
    // template-engine-specific
    interface ITagUpdater
    {
        void Update();
    }

    public static class TagUpdater
    {
        public static string TagReplacer(string input, string newTag)
        {
            // Supported formats:
            //   "host:port/owner/name:tag"
            //   "host/owner/name:tag"
            //   "owner/name:tag"
            //   "tag"
            return Regex.Replace(input, @"([^:]+)$", newTag);
        }

        public static string ExtractTag(string input)
        {
            return Regex.Replace(input, @"^.+:", "");
        }
    }

    class PatternTagUpdater : ITagUpdater
    {
        private Options opts { get; }
        public PatternTagUpdater(Options _opts)
        {
            this.opts = _opts;
        }

        // TODO: Testable
        public void Update()
        {
            var tagPattern = new Regex(opts.ManifestTagPattern, RegexOptions.Compiled);
            foreach (var file in opts.ManifestValuesFiles)
            {
                var path = Path.Combine(opts.WorkDir, opts.ManifestRepoName, opts.ManifestRoot, file);
                var lines = File.ReadAllLines(path);
                var updated = lines.Select(line =>
                    tagPattern.Replace(line, oldVal =>
                    {
                        opts.AppPrevCommit = TagUpdater.ExtractTag(oldVal.Value);
                        return TagUpdater.TagReplacer(oldVal.Value, opts.AppCurrCommit);
                    }));

                File.WriteAllLines(path, updated);
            }
        }
    }

    class YamlTagUpdater : ITagUpdater
    {
        private Options opts { get; }
        public YamlTagUpdater(Options _opts)
        {
            this.opts = _opts;
        }

        // TODO: Testable
        public void Update()
        {
            foreach (var file in opts.ManifestValuesFiles)
            {
                var ys = new YamlStream();

                var path = Path.Combine(opts.WorkDir, opts.ManifestRepoName, opts.ManifestRoot, file);
                using (var sr = new StreamReader(path))
                {
                    ys.Load(sr);
                    foreach (var doc in ys.Documents)
                    {
                        foreach (var nestedKey in opts.ManifestTagKeys)
                        {
                            var oldVal = YamlUtil.UpdateByNestedKey(
                                doc.RootNode,
                                nestedKey.Split("."),
                                oldVal => TagUpdater.TagReplacer(oldVal, opts.AppCurrCommit));
                            opts.AppPrevCommit = TagUpdater.ExtractTag(oldVal);
                        }
                    }
                }

                var tmpPath = Path.Combine(opts.WorkDir, Guid.NewGuid().ToString());
                using (var sw = new StreamWriter(tmpPath))
                {
                    ys.Save(sw, false);
                }

                var backupPath = Path.Combine(opts.WorkDir, Guid.NewGuid().ToString());
                File.Replace(tmpPath, path, backupPath);
            }
        }
    }
}