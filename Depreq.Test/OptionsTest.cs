using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CommandLine;
using Depreq;

namespace Depreq.Test
{
    public class OptionsTest
    {
        [Theory]
        // Use --manifest-tag-keys
        [InlineData(new object[] { new string[]{
            "--git-user",
            "amaya382",
            "--git-email",
            "mail@sapphire.in.net",
            "--git-token-user",
            "amaya382",
            "--git-token",
            "git-token",
            "--manifest-uri",
            "https://github.com/amaya382/manifests.git",
            "--manifest-base-branch",
            "dev",
            "--app-image",
            "gitopsdemo/app-a",
            "--app-uri",
            "https://github.com/gitops-demo/app-a",
            "--app-curr-commit",
            "cd398c7ce9d149884620a1a48764d567b853a8a0",
            "--github-user",
            "github-user",
            "--github-assignees",
            "amaya382",
            "--github-reviewers",
            "github-reviewers1:github-reviewers2",
            "--manifest-values-files",
            "app-a/values/dev-app-a.yaml:app-a/values/prd-app-a.yaml",
            "--manifest-tag-keys",
            "image.repository"}})]
        // Use --manifest-tag-pattern
        [InlineData(new object[] { new string[]{
            "--git-user",
            "amaya382",
            "--git-email",
            "mail@sapphire.in.net",
            "--git-token-user",
            "amaya382",
            "--git-token",
            "git-token",
            "--manifest-uri",
            "https://github.com/amaya382/manifests.git",
            "--manifest-base-branch",
            "dev",
            "--app-image",
            "gitopsdemo/app-a",
            "--app-uri",
            "https://github.com/gitops-demo/app-a",
            "--app-curr-commit",
            "cd398c7ce9d149884620a1a48764d567b853a8a0",
            "--github-user",
            "github-user",
            "--github-assignees",
            "amaya382",
            "--github-reviewers",
            "github-reviewers1:github-reviewers2",
            "--manifest-values-files",
            "app-a/values/dev-app-a.yaml:app-a/values/prd-app-a.yaml",
            "--manifest-tag-pattern",
            "(?<=repository: )([0-9a-f]+)$"}})]
        public async Task ShouldPassTest(string[] args)
        {
            var res = await Parser.Default.ParseArguments<Options>(args).MapResult(
                async (Options opts) =>
                {
                    try
                    {
                        opts.Prepare();
                    }
                    catch
                    {
                        return 1;
                    }
                    await Task.Delay(0); // dummy
                    return 0;
                }, async (IEnumerable<Error> er) =>
                {
                    await Task.Delay(0); // dummy
                    return 1;
                });

            Assert.Equal(0, res);
        }

        [Theory]
        // Both --manifest-tag-keys and --manifest-tag-pattern at once
        [InlineData(new object[]{ new string[]{
            "--git-user",
            "amaya382",
            "--git-email",
            "mail@sapphire.in.net",
            "--git-token-user",
            "amaya382",
            "--git-token",
            "git-token",
            "--manifest-uri",
            "https://github.com/amaya382/manifests.git",
            "--manifest-base-branch",
            "dev",
            "--app-image",
            "gitopsdemo/app-a",
            "--app-uri",
            "https://github.com/gitops-demo/app-a",
            "--app-curr-commit",
            "cd398c7ce9d149884620a1a48764d567b853a8a0",
            "--github-user",
            "github-user",
            "--github-assignees",
            "amaya382",
            "--github-reviewers",
            "github-reviewers1:github-reviewers2",
            "--manifest-values-files",
            "app-a/values/dev-app-a.yaml:app-a/values/prd-app-a.yaml",
            "--manifest-tag-keys",
            "image.repository",
            "--manifest-tag-pattern",
            "(?<=repository: )([0-9a-f]+)$"}})]
        // Neither --manifest-tag-keys nor --manifest-tag-pattern
        [InlineData(new object[]{ new string[]{
            "--git-user",
            "amaya382",
            "--git-email",
            "mail@sapphire.in.net",
            "--git-token-user",
            "amaya382",
            "--git-token",
            "git-token",
            "--manifest-uri",
            "https://github.com/amaya382/manifests.git",
            "--manifest-base-branch",
            "dev",
            "--app-image",
            "gitopsdemo/app-a",
            "--app-uri",
            "https://github.com/gitops-demo/app-a",
            "--app-curr-commit",
            "cd398c7ce9d149884620a1a48764d567b853a8a0",
            "--github-user",
            "github-user",
            "--github-assignees",
            "amaya382",
            "--github-reviewers",
            "github-reviewers1:github-reviewers2",
            "--manifest-values-files",
            "app-a/values/dev-app-a.yaml:app-a/values/prd-app-a.yaml"}})]
        // --manifest-tag-keys w/ unknown formatted file
        [InlineData(new object[]{ new string[]{
            "--git-user",
            "amaya382",
            "--git-email",
            "mail@sapphire.in.net",
            "--git-token-user",
            "amaya382",
            "--git-token",
            "git-token",
            "--manifest-uri",
            "https://github.com/amaya382/manifests.git",
            "--manifest-base-branch",
            "dev",
            "--app-image",
            "gitopsdemo/app-a",
            "--app-uri",
            "https://github.com/gitops-demo/app-a",
            "--app-curr-commit",
            "cd398c7ce9d149884620a1a48764d567b853a8a0",
            "--github-user",
            "github-user",
            "--github-assignees",
            "amaya382",
            "--github-reviewers",
            "github-reviewers1:github-reviewers2",
            "--manifest-values-files",
            "app-a/values/dev-app-a.unknown1:app-a/values/prd-app-a.unknown2",
            "--manifest-tag-keys",
            "image.repository"}})]
        // Missing requireds (--git-user)
        [InlineData(new object[]{ new string[]{
            "--git-email",
            "mail@sapphire.in.net",
            "--git-token-user",
            "amaya382",
            "--git-token",
            "git-token",
            "--manifest-uri",
            "https://github.com/amaya382/manifests.git",
            "--manifest-base-branch",
            "dev",
            "--app-image",
            "gitopsdemo/app-a",
            "--app-uri",
            "https://github.com/gitops-demo/app-a",
            "--app-curr-commit",
            "cd398c7ce9d149884620a1a48764d567b853a8a0",
            "--github-user",
            "github-user",
            "--github-assignees",
            "amaya382",
            "--github-reviewers",
            "github-reviewers1:github-reviewers2",
            "--manifest-values-files",
            "app-a/values/dev-app-a.yaml:app-a/values/prd-app-a.yaml",
            "--manifest-tag-keys",
            "image.repository"}})]
        // Invalid uri (Contains extra path)
        [InlineData(new object[]{ new string[]{
            "--git-user",
            "amaya382",
            "--git-email",
            "mail@sapphire.in.net",
            "--git-token-user",
            "amaya382",
            "--git-token",
            "git-token",
            "--manifest-uri",
            "https://github.com/invalid/uri/foo/bar",
            "--manifest-base-branch",
            "dev",
            "--app-image",
            "gitopsdemo/app-a",
            "--app-uri",
            "https://github.com/gitops-demo/app-a",
            "--app-curr-commit",
            "cd398c7ce9d149884620a1a48764d567b853a8a0",
            "--github-user",
            "github-user",
            "--github-assignees",
            "amaya382",
            "--github-reviewers",
            "github-reviewers1:github-reviewers2",
            "--manifest-values-files",
            "app-a/values/dev-app-a.yaml:app-a/values/prd-app-a.yaml",
            "--manifest-tag-keys",
            "image.repository"}})]
        // Invalid uri (Not uri)
        [InlineData(new object[]{ new string[]{
            "--git-user",
            "amaya382",
            "--git-email",
            "mail@sapphire.in.net",
            "--git-token-user",
            "amaya382",
            "--git-token",
            "git-token",
            "--manifest-uri",
            "invalid/uri",
            "--manifest-base-branch",
            "dev",
            "--app-image",
            "gitopsdemo/app-a",
            "--app-uri",
            "https://github.com/gitops-demo/app-a",
            "--app-curr-commit",
            "cd398c7ce9d149884620a1a48764d567b853a8a0",
            "--github-user",
            "github-user",
            "--github-assignees",
            "amaya382",
            "--github-reviewers",
            "github-reviewers1:github-reviewers2",
            "--manifest-values-files",
            "app-a/values/dev-app-a.yaml:app-a/values/prd-app-a.yaml",
            "--manifest-tag-keys",
            "image.repository"}})]

        public async Task ShouldFailTest(string[] args)
        {
            var res = await Parser.Default.ParseArguments<Options>(args).MapResult(
                async (Options opts) =>
                {
                    try
                    {
                        opts.Prepare();
                    }
                    catch
                    {
                        return 1;
                    }
                    await Task.Delay(0); // dummy
                    return 0;
                }, async (IEnumerable<Error> er) =>
                {
                    await Task.Delay(0); // dummy
                    return 1;
                });

            Assert.NotEqual(0, res);
        }
    }
}
