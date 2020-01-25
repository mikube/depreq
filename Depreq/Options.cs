using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using CommandLine;

namespace Depreq
{
    class Options
    {
        // Core
        [Option('u', "git-user", Required = true,
        HelpText = "A user name for creating a commit which changes an image tag in a manifest.")]
        public string GitUser { get; set; }

        [Option('e', "git-email", Required = true,
        HelpText = "An email for creating a commit which changes an image tag in a manifest.")]
        public string GitEmail { get; set; }

        [Option('m', "git-commit-msg", Default = "{AppImage}/{AppPrevCommit#8}...{AppCurrCommit#8}",
        HelpText = "A message for creating a commit which changes an image tag in a manifest.")]
        public string GitCommitMsgTemplate { get; set; }
        public string GitCommitMsg { get; set; }

        [Option('t', "git-token",
        HelpText = "A token for accessing a manifest repository.")]
        public string GitToken { get; set; }

        [Option('U', "git-token-user", Default = "",
        HelpText = "An owner of git-token.")]
        public string GitTokenUser { get; set; }

        [Option('r', "manifest-uri", Required = true,
        HelpText = "Manifest repository uri.")]
        public Uri ManifestUri { get; set; }

        [Option('b', "manifest-base-branch", Required = true,
        HelpText = "Manifest repository base branch name.")]
        public string ManifestBaseBranch { get; set; }

        [Option("manifest-deploy-branch", Default = "deploy/{AppImage}/{AppCurrCommit#8}",
        HelpText = "Manifest repository deploy branch name.")]
        public string ManifestDeployBranchTemplate { get; set; }
        public string ManifestDeployBranch { get; private set; }

        [Option("manifest-root", Default = ".",
        HelpText = "A path to the root of manifest in the manifest repository.")]
        public string ManifestRoot { get; set; }

        public enum FileFormatType { Yaml, Json, Unknown };
        public FileFormatType FileFormat { get; private set; }

        [Option('f', "manifest-values-files", Required = true, Separator = ':',
        HelpText = "Pathes to values files. (Separator: ':')")]
        public IEnumerable<string> ManifestValuesFiles { get; set; }

        public enum TagUpdaterType { Key, Pattern };
        public TagUpdaterType TagUpdater { get; private set; }

        [Option('k', "manifest-tag-keys", Separator = ':',
        HelpText = "Nested keys for a section which contains an image tag. Now only YAML is supported. (Separator: ':')")]
        public IEnumerable<string> ManifestTagKeys { get; set; }

        [Option('R', "manifest-tag-pattern",
        HelpText = "Regex pattern for finding a section which contains an image tag.")]
        public string ManifestTagPattern { get; set; }

        [Option("app-uri",
        HelpText = "Application repository uri.")]
        public Uri AppUri { get; set; }

        [Option('i', "app-image", Required = true,
        HelpText = "Docker image")]
        public string AppImage { get; set; }

        [Option('p', "app-prev-commit",
        HelpText = "Previous commit hash of the application repository. If empty, will try to extract from the old manifest.")]
        public string AppPrevCommit { get; set; }

        [Option('c', "app-curr-commit", Required = true,
        HelpText = "Current commit hash of the application repository.")]
        public string AppCurrCommit { get; set; }


        // Optional
        [Option("work-dir",
        HelpText = "Working directory.")]
        public string WorkDir { get; set; }


        // Repository
        public enum RepoType { GitHub, GitLab };
        public RepoType Repo { get; private set; }

        // GitHub-specific
        [Option("github-user", SetName = "github", Required = true,
        HelpText = "A GitHub user name for a pull request.")]
        public string GitHubUser { get; set; }

        [Option("github-pr-title", SetName = "github", Default = "[DEPLOY] {AppImage}",
        HelpText = "GitHub pull request title.")]
        public string GitHubPRTitleTemplate { get; set; }
        public string GitHubPRTitle { get; set; }

        [Option("github-pr-body", SetName = "github", Default = "[{AppImage}]({AppUri})\n[Application Diff ({AppPrevCommit#8}...{AppCurrCommit#8})]({AppUri}/compare/{AppPrevCommit}...{AppCurrCommit})",
        HelpText = "GitHub pull request body.")]
        public string GitHubPRBodyTemplate { get; set; }
        // Note: To use `AppPrevCommit`, have to wait for extracting a hash from manifest by ITagUpdater.Update()
        public string GitHubPRBody { get; set; }

        [Option("github-assignees", SetName = "github", Separator = ':',
        HelpText = "GitHub pull request assignees. (Separator: ':')")]
        public IEnumerable<string> GitHubAssignees { get; set; }

        [Option("github-reviewers", SetName = "github", Separator = ':',
        HelpText = "GitHub pull request reviewers. (Separator: ':')")]
        public IEnumerable<string> GitHubReviewers { get; set; }


        // GitLab-specific
        // [Option("gitlab-user", SetName = "gitlab", Required = true,
        // HelpText = "A GitLab user name for a merge request.")]
        // public string GitLabUser { get; set; }


        // Additional
        public Uri GitRepoUri { get; private set; }
        public string ManifestRepoOwner { get; private set; }
        public string ManifestRepoName { get; private set; }

        private void Extract()
        {
            var valuesFile = ManifestValuesFiles.First(); // Check first file
            if (valuesFile.EndsWith(".yaml") || valuesFile.EndsWith(".yml"))
            {
                FileFormat = FileFormatType.Yaml;
            }
            else if (valuesFile.EndsWith(".json"))
            {
                FileFormat = FileFormatType.Json;
            }
            else
            {
                FileFormat = FileFormatType.Unknown;
            }

            if ((new List<object> { ManifestTagKeys, ManifestTagPattern }).Count(x =>
                x != null && (x as IEnumerable<string>)?.Count() != 0) != 1)
            {
                throw new Exception("Specify either '--manifest-tag-keys' for Yaml/Json or '--manifest-tag-pattern' for any formats.");
            }
            if (ManifestTagKeys != null && ManifestTagKeys.Count() != 0)
            {
                TagUpdater = TagUpdaterType.Key;
            }
            else if (ManifestTagPattern != null)
            {
                TagUpdater = TagUpdaterType.Pattern;
            }

            if (FileFormat == FileFormatType.Unknown && TagUpdater != TagUpdaterType.Pattern)
            {
                throw new Exception("Use '--manifest-tag-pattern' except for Yaml/Json.");
            }

            if (GitHubUser != null)
            {
                Repo = RepoType.GitHub;
            }
            /*else if (GitLabUser != null)
            {
                Repo = RepoType.GitLab;
            }*/
        }

        private void Parse()
        {
            GitRepoUri = new Uri(
                ManifestUri.GetLeftPart(UriPartial.Authority));

            var ownerAndName = ManifestUri.AbsolutePath.Split("/");
            if (ownerAndName.Length != 3)
            {
                throw new Exception("Invalid manifest uri");
            }
            ManifestRepoOwner = ownerAndName[1];
            ManifestRepoName = Regex.Replace(ownerAndName[2], @"\.git$", "");
        }

        private string CreateTmpDir()
        {
            var tmpDir = Path.GetFullPath(
                Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            Directory.CreateDirectory(tmpDir);
            return tmpDir;
        }

        public void Prepare()
        {
            Extract();
            Parse();
            WorkDir = (WorkDir != null) ? WorkDir : CreateTmpDir();

            ManifestDeployBranch = TemplateUtil.Apply(this, ManifestDeployBranchTemplate);
        }
    }
}