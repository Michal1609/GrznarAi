using GrznarAi.Web.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetProjectsAsync();
        Task<Project?> GetProjectByIdAsync(int id);
        Task<Project> CreateProjectAsync(Project project);
        Task UpdateProjectAsync(Project project);
        Task DeleteProjectAsync(int id);

        // Metody pro práci s vícejazyčným obsahem
        Task<string?> GetProjectContentAsync(int projectId, string languageCode);
        Task<Dictionary<string, string>> GetAllProjectContentsAsync(int projectId);
        Task SetProjectContentAsync(int projectId, string languageCode, string content);
        Task DeleteProjectContentAsync(int projectId, string languageCode);
    }
} 