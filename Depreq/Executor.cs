using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Depreq
{
    class Executor
    {
        private Options opts { get; }
        private ITagUpdater referenceUpdater { get; }
        private IDeployRequestCreator deployRequestCreator { get; }

        public Executor(Options _opts,
        ITagUpdater _referneceUpdater,
        IDeployRequestCreator _deployRequestCreator)
        {
            this.opts = _opts;
            this.referenceUpdater = _referneceUpdater;
            this.deployRequestCreator = _deployRequestCreator;
        }

        public async Task<string> Request()
        {
            // Credentials
            LibGit2Sharp.Handlers.CredentialsHandler credsProvider = (_url, _user, _cred) =>
            new UsernamePasswordCredentials
            {
                Username = opts.GitTokenUser,
                Password = opts.GitToken,
            };

            // Clone
            var cloneOpts = new CloneOptions
            {
                CredentialsProvider = credsProvider,
                BranchName = opts.ManifestBaseBranch,
                RecurseSubmodules = true
            };
            var repoDir = Path.Combine(opts.WorkDir, opts.ManifestRepoName);
            Repository.Clone(opts.ManifestUri.ToString(), repoDir, cloneOpts);

            using (var repo = new Repository(repoDir))
            {
                // Branch for PR
                var branch = repo.CreateBranch(opts.ManifestDeployBranch);
                branch = Commands.Checkout(repo, opts.ManifestDeployBranch);

                // Update manifest
                referenceUpdater.Update();

                // Add
                Commands.Stage(repo, "*");

                // Commit
                opts.GitCommitMsg = TemplateUtil.Apply(opts, opts.GitCommitMsgTemplate);
                var author = new Signature(opts.GitUser, opts.GitEmail, DateTime.Now);
                var committer = author;
                var commit = repo.Commit(opts.GitCommitMsg, author, committer);

                // Push
                var pushOpts = new PushOptions
                {
                    CredentialsProvider = credsProvider
                };

                repo.Branches.Update(branch,
                    b => b.Remote = "origin",
                    b => b.UpstreamBranch = branch.CanonicalName);
                repo.Network.Push(branch, pushOpts);
            }

            return await deployRequestCreator.Create();
        }
    }
}