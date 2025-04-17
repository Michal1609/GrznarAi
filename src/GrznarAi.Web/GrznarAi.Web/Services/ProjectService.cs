using Microsoft.EntityFrameworkCore;
using GrznarAi.Web.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
        return await context.Projects.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Projects.FindAsync(id);
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
        var existingProject = await context.Projects.FindAsync(project.Id);
        if (existingProject != null)
        {
            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.Content = project.Content;
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
        var project = await context.Projects.FindAsync(id);
        if (project != null)
        {
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException($"Project with ID {id} not found.");
        }
    }
} 