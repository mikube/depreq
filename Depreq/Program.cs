using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;

namespace Depreq
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args).MapResult(
                async (Options opts) =>
                {
                    try
                    {
                        opts.Prepare();
                        var updater =
                            opts.TagUpdater == Options.TagUpdaterType.Pattern
                                ? new PatternTagUpdater(opts)
                            : opts.TagUpdater == Options.TagUpdaterType.Key
                                ? opts.FileFormat == Options.FileFormatType.Yaml
                                    ? new YamlTagUpdater(opts)
                                    : throw new NotImplementedException("Json is not supported yet.")
                            : (ITagUpdater)null;
                        var creator =
                            opts.Repo == Options.RepoType.GitHub
                                ? new GitHubDeployRequestCreator(opts)
                            : opts.Repo == Options.RepoType.GitLab
                                ? throw new NotImplementedException("GitLab is not supported yet.")
                            : (IDeployRequestCreator)null;
                        var executor = new Executor(opts, updater, creator);
                        var res = await executor.Request();
                        Console.WriteLine(res);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    return 0;
                }, async (IEnumerable<Error> er) =>
                {
                    await Task.Delay(0); // dummy
                    Console.WriteLine("Failed to parse options.");
                    return 1;
                });
        }
    }
}
