using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace GrznarAi.Web.Services;

public interface IGitHubService
{
    Task<GitHubRepositoryInfo?> GetRepositoryDetailsAsync(string owner, string repoName);
}

// DTO to hold repository details fetched from GitHub API
public class GitHubRepositoryInfo
{
    public string? Description { get; set; }
    public List<string> Languages { get; set; } = new List<string>();
    public int Stars { get; set; }
    public int Forks { get; set; }
    public string? HtmlUrl { get; set; } // The main URL to the repo
} 