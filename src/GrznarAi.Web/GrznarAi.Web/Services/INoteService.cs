using GrznarAi.Web.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface INoteService
    {
        Task<IEnumerable<Note>> GetUserNotesAsync(string userId, string? searchText = null);
        Task<Note?> GetNoteAsync(int id, string userId);
        Task<Note> CreateNoteAsync(Note note);
        Task<bool> UpdateNoteAsync(Note note);
        Task<bool> DeleteNoteAsync(int id, string userId);
        Task<IEnumerable<NoteCategory>> GetUserCategoriesAsync(string userId);
        Task<NoteCategory?> GetCategoryAsync(int id, string userId);
        Task<NoteCategory> CreateCategoryAsync(NoteCategory category);
        Task<bool> UpdateCategoryAsync(NoteCategory category);
        Task<bool> DeleteCategoryAsync(int id, string userId);
        Task<bool> AddNoteToCategoryAsync(int noteId, int categoryId, string userId);
        Task<bool> RemoveNoteFromCategoryAsync(int noteId, int categoryId, string userId);
    }
} 