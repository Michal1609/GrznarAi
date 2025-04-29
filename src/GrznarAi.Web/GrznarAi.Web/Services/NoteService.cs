using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public class NoteService : INoteService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IPermissionService _permissionService;

        public NoteService(IDbContextFactory<ApplicationDbContext> contextFactory, IPermissionService permissionService)
        {
            _contextFactory = contextFactory;
            _permissionService = permissionService;
        }

        public async Task<IEnumerable<Note>> GetUserNotesAsync(string userId, string? searchText = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var query = context.Notes
                .Include(n => n.Categories)
                .Where(n => n.ApplicationUserId == userId);
                
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(n => n.Title.ToLower().Contains(searchText) ||
                                         n.Content.ToLower().Contains(searchText));
            }
            
            return await query
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Note?> GetNoteAsync(int id, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Notes
                .Include(n => n.Categories)
                .FirstOrDefaultAsync(n => n.Id == id && n.ApplicationUserId == userId);
        }

        public async Task<Note> CreateNoteAsync(Note note)
        {
            if (!await _permissionService.UserHasPermissionAsync(note.ApplicationUserId, "App.Notes"))
                throw new UnauthorizedAccessException("Uživatel nemá oprávnění k používání aplikace Poznámky.");
                
            using var context = await _contextFactory.CreateDbContextAsync();
            
            note.CreatedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;
            
            context.Notes.Add(note);
            await context.SaveChangesAsync();
            
            return note;
        }

        public async Task<bool> UpdateNoteAsync(Note note)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var existingNote = await context.Notes
                    .FirstOrDefaultAsync(n => n.Id == note.Id && n.ApplicationUserId == note.ApplicationUserId);
                    
                if (existingNote == null)
                    return false;
                
                existingNote.Title = note.Title;
                existingNote.Content = note.Content;
                existingNote.UpdatedAt = DateTime.UtcNow;
                
                context.Update(existingNote);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteNoteAsync(int id, string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var note = await context.Notes
                    .FirstOrDefaultAsync(n => n.Id == id && n.ApplicationUserId == userId);
                    
                if (note == null)
                    return false;
                
                // Odstranění všech vazeb na kategorie
                var relations = await context.NoteCategoryRelations
                    .Where(ncr => ncr.NoteId == id)
                    .ToListAsync();
                    
                context.NoteCategoryRelations.RemoveRange(relations);
                context.Notes.Remove(note);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<NoteCategory>> GetUserCategoriesAsync(string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.NoteCategories
                .Where(nc => nc.ApplicationUserId == userId)
                .OrderBy(nc => nc.Name)
                .ToListAsync();
        }

        public async Task<NoteCategory?> GetCategoryAsync(int id, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.NoteCategories
                .Include(nc => nc.Notes)
                .FirstOrDefaultAsync(nc => nc.Id == id && nc.ApplicationUserId == userId);
        }

        public async Task<NoteCategory> CreateCategoryAsync(NoteCategory category)
        {
            if (!await _permissionService.UserHasPermissionAsync(category.ApplicationUserId, "App.Notes"))
                throw new UnauthorizedAccessException("Uživatel nemá oprávnění k používání aplikace Poznámky.");
                
            using var context = await _contextFactory.CreateDbContextAsync();
            
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            
            context.NoteCategories.Add(category);
            await context.SaveChangesAsync();
            
            return category;
        }

        public async Task<bool> UpdateCategoryAsync(NoteCategory category)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var existingCategory = await context.NoteCategories
                    .FirstOrDefaultAsync(nc => nc.Id == category.Id && nc.ApplicationUserId == category.ApplicationUserId);
                    
                if (existingCategory == null)
                    return false;
                
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.UpdatedAt = DateTime.UtcNow;
                
                context.Update(existingCategory);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id, string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var category = await context.NoteCategories
                    .FirstOrDefaultAsync(nc => nc.Id == id && nc.ApplicationUserId == userId);
                    
                if (category == null)
                    return false;
                
                // Odstranění všech vazeb z poznámek
                var relations = await context.NoteCategoryRelations
                    .Where(ncr => ncr.NoteCategoryId == id)
                    .ToListAsync();
                    
                context.NoteCategoryRelations.RemoveRange(relations);
                context.NoteCategories.Remove(category);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AddNoteToCategoryAsync(int noteId, int categoryId, string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // Ověření, že poznámka i kategorie patří danému uživateli
                var note = await context.Notes
                    .FirstOrDefaultAsync(n => n.Id == noteId && n.ApplicationUserId == userId);
                    
                var category = await context.NoteCategories
                    .FirstOrDefaultAsync(nc => nc.Id == categoryId && nc.ApplicationUserId == userId);
                    
                if (note == null || category == null)
                    return false;
                
                // Kontrola, zda již vazba neexistuje
                var existingRelation = await context.NoteCategoryRelations
                    .FirstOrDefaultAsync(ncr => ncr.NoteId == noteId && ncr.NoteCategoryId == categoryId);
                    
                if (existingRelation != null)
                    return true;
                
                // Vytvoření nové vazby
                var relation = new NoteCategoryRelation
                {
                    NoteId = noteId,
                    NoteCategoryId = categoryId
                };
                
                context.NoteCategoryRelations.Add(relation);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RemoveNoteFromCategoryAsync(int noteId, int categoryId, string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // Ověření, že poznámka i kategorie patří danému uživateli
                var note = await context.Notes
                    .FirstOrDefaultAsync(n => n.Id == noteId && n.ApplicationUserId == userId);
                    
                var category = await context.NoteCategories
                    .FirstOrDefaultAsync(nc => nc.Id == categoryId && nc.ApplicationUserId == userId);
                    
                if (note == null || category == null)
                    return false;
                
                // Nalezení a odstranění vazby
                var relation = await context.NoteCategoryRelations
                    .FirstOrDefaultAsync(ncr => ncr.NoteId == noteId && ncr.NoteCategoryId == categoryId);
                    
                if (relation == null)
                    return true;
                
                context.NoteCategoryRelations.Remove(relation);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 