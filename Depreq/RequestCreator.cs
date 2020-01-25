using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;

namespace Depreq
{
    interface IDeployRequestCreator
    {
        Task<string> Create();
    }

    class GitHubDeployRequestCreator : IDeployRequestCreator
    {
        private Options opts { get; }

        private GitHubClient client { get; }
        private AssigneesClient aClient { get; }

        public GitHubDeployRequestCreator(Options _opts)
        {
            this.opts = _opts;

            var creds = new InMemoryCredentialStore(
                new Credentials(opts.GitToken));
            client = new GitHubClient(
                new ProductHeaderValue("Depreq"), creds, opts.GitRepoUri);
            var apiConn = new ApiConnection(client.Connection);
            aClient = new AssigneesClient(apiConn);
        }

        public async Task<string> Create()
        {
            // PullRequest
            opts.GitHubPRTitle = TemplateUtil.Apply(opts, opts.GitHubPRTitleTemplate);
            opts.GitHubPRBody = TemplateUtil.Apply(opts, opts.GitHubPRBodyTemplate);
            var newPr = new NewPullRequest(
                opts.GitHubPRTitle, opts.ManifestDeployBranch, opts.ManifestBaseBranch)
            {
                Body = opts.GitHubPRBody
            };
            var pr = await client.PullRequest.Create(
               opts.ManifestRepoOwner, opts.ManifestRepoName, newPr);

            // Add Reviewers
            var reviewers = new PullRequestReviewRequest(opts.GitHubReviewers.ToList());
            var pr1 = await client.PullRequest.ReviewRequest.Create(
                opts.ManifestRepoOwner, opts.ManifestRepoName, pr.Number, reviewers);

            // Add Assignees
            var assignees = new AssigneesUpdate(opts.GitHubAssignees.ToList());
            var pr2 = await aClient.AddAssignees(
                opts.ManifestRepoOwner, opts.ManifestRepoName, pr.Number, assignees);

            return $"Created deploy request: {pr.HtmlUrl}";
        }
    }
}