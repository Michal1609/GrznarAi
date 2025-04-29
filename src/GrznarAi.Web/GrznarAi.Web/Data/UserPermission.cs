using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GrznarAi.Web.Data
{
    [Index(nameof(ApplicationUserId), nameof(ApplicationPermissionId), IsUnique = true)]
    public class UserPermission
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        public int ApplicationPermissionId { get; set; }
        public ApplicationPermission? ApplicationPermission { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 