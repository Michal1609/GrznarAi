using Microsoft.EntityFrameworkCore;
using GrznarAi.Web.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;

namespace GrznarAi.Web.Services;

public class ProjectService : IProjectService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ProjectService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Projects
            .Include(p => p.Contents)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Projects
            .Include(p => p.Contents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        return project;
    }

    public async Task UpdateProjectAsync(Project project)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingProject = await context.Projects
            .Include(p => p.Contents)
            .FirstOrDefaultAsync(p => p.Id == project.Id);

        if (existingProject != null)
        {
            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.GitHubUrl = project.GitHubUrl;
            
            // Aktualizace obsahu je řešena v samostatných metodách
            await context.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException($"Project with ID {project.Id} not found.");
        }
    }

    public async Task DeleteProjectAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var project = await context.Projects
            .Include(p => p.Contents)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project != null)
        {
            // Nejprve odstraníme všechny obsahy
            context.ProjectContents.RemoveRange(project.Contents);
            // Poté odstraníme projekt
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException($"Project with ID {id} not found.");
        }
    }

    // Metody pro práci s vícejazyčným obsahem
    public async Task<string?> GetProjectContentAsync(int projectId, string languageCode)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var content = await context.ProjectContents
            .FirstOrDefaultAsync(pc => pc.ProjectId == projectId && pc.LanguageCode == languageCode);
        
        return content?.Content;
    }

    public async Task<Dictionary<string, string>> GetAllProjectContentsAsync(int projectId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var contents = await context.ProjectContents
            .Where(pc => pc.ProjectId == projectId)
            .ToDictionaryAsync(pc => pc.LanguageCode, pc => pc.Content);
        
        return contents;
    }

    public async Task SetProjectContentAsync(int projectId, string languageCode, string content)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var projectContent = await context.ProjectContents
            .FirstOrDefaultAsync(pc => pc.ProjectId == projectId && pc.LanguageCode == languageCode);
        
        if (projectContent != null)
        {
            // Aktualizujeme existující obsah
            projectContent.Content = content;
        }
        else
        {
            // Vytvoříme nový obsah
            projectContent = new ProjectContent
            {
                ProjectId = projectId,
                LanguageCode = languageCode,
                Content = content
            };
            context.ProjectContents.Add(projectContent);
        }
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteProjectContentAsync(int projectId, string languageCode)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var projectContent = await context.ProjectContents
            .FirstOrDefaultAsync(pc => pc.ProjectId == projectId && pc.LanguageCode == languageCode);
        
        if (projectContent != null)
        {
            context.ProjectContents.Remove(projectContent);
            await context.SaveChangesAsync();
        }
    }
} 