using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    [Index(nameof(NoteId), nameof(NoteCategoryId), IsUnique = true)]
    public class NoteCategoryRelation
    {
        public int Id { get; set; }
        
        public int NoteId { get; set; }
        public Note? Note { get; set; }
        
        public int NoteCategoryId { get; set; }
        public NoteCategory? NoteCategory { get; set; }
    }
} 