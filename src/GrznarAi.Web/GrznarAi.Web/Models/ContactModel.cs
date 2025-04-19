using System.ComponentModel.DataAnnotations;

namespace GrznarAi.Web.Models
{
    public class ContactModel
    {
        [Required(ErrorMessage = "Jméno je povinné")]
        [StringLength(100, ErrorMessage = "Jméno nesmí být delší než 100 znaků")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail je povinný")]
        [EmailAddress(ErrorMessage = "Neplatný formát e-mailu")]
        [StringLength(100, ErrorMessage = "E-mail nesmí být delší než 100 znaků")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Předmět je povinný")]
        [StringLength(100, ErrorMessage = "Předmět nesmí být delší než 100 znaků")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Zpráva je povinná")]
        [StringLength(1000, ErrorMessage = "Zpráva nesmí být delší než 1000 znaků")]
        public string Message { get; set; } = string.Empty;
    }
} 