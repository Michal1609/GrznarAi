using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // For reading configuration
using Microsoft.Extensions.Logging;
using Octokit;

namespace GrznarAi.Web.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;
        private readonly ILogger<GitHubService> _logger;

        public GitHubService(IConfiguration configuration, ILogger<GitHubService> logger)
        {
            _logger = logger;
            _client = new GitHubClient(new ProductHeaderValue("GrznarAI-Website"));

            // Attempt to authenticate using a token from configuration
            var token = configuration["GitHub:AccessToken"]; // Expecting token in appsettings.json or user secrets
            if (!string.IsNullOrEmpty(token))
            {
                _client.Credentials = new Credentials(token);
                _logger.LogInformation("GitHub client configured with authentication token.");
            }
            else
            {
                _logger.LogWarning("GitHub AccessToken not found in configuration. Using unauthenticated requests (rate limits apply).");
            }
        }

        public async Task<GitHubRepositoryInfo?> GetRepositoryDetailsAsync(string owner, string repoName)
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repoName))
            {
                _logger.LogWarning("Owner or repo name is missing.");
                return null;
            }

            _logger.LogInformation("Fetching repository details and languages using Octokit for {Owner}/{RepoName}", owner, repoName);

            try
            {
                // Fetch main repository details
                var repository = await _client.Repository.Get(owner, repoName);
                if (repository == null)
                {
                    _logger.LogWarning("Octokit returned null repository for {Owner}/{RepoName}", owner, repoName);
                    return null;
                }

                // Fetch repository languages
                var languages = await _client.Repository.GetAllLanguages(owner, repoName);
                List<string> languageNames = languages?.Select(l => l.Name).ToList() ?? new List<string>();
                 _logger.LogInformation("Fetched {Count} languages for {Owner}/{RepoName}", languageNames.Count, owner, repoName);

                // Combine results
                return new GitHubRepositoryInfo
                {
                    Description = repository.Description,
                    Languages = languageNames,
                    Stars = repository.StargazersCount,
                    Forks = repository.ForksCount,
                    HtmlUrl = repository.HtmlUrl
                };
            }
            catch (ApiException ex)
            {
                 _logger.LogError(ex, "GitHub API error (ApiException) while fetching repository details/languages for {Owner}/{RepoName}. Status: {StatusCode}", 
                    owner, repoName, ex.StatusCode);
                 // Specific handling for rate limit
                 if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden && ex.Message.Contains("rate limit exceeded"))
                 {
                     _logger.LogWarning("GitHub API rate limit exceeded.");
                 }
                 return null;
            }
            catch (Exception ex) // Catch other unexpected errors
            {
                _logger.LogError(ex, "Unexpected error while fetching GitHub repository details/languages for {Owner}/{RepoName}", owner, repoName);
                return null;
            }
        }
        
        // Keep other methods from the original file if they existed and are needed
        // For example, if there was a method using Octokit:
        // public Task<Repository?> GetRepositoryDataAsync(string repositoryUrl) => Task.FromResult<Repository?>(null); // Placeholder implementation
        // public Task<string?> GetRepositoryImageUrlAsync(string repositoryUrl) => Task.FromResult<string?>(null); // Placeholder implementation
    }
} 