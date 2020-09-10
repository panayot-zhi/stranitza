using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace stranitza.Models.ViewModels
{
    public class PostCreateViewModel
    {        
        [Display(Name = "Заглавие")]
        [Required(ErrorMessage = "Заглавието е задължително поле. Моля, въведете заглавие.")]
        public string Title { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Източник")]
        [Required(ErrorMessage = "Моля, въведете източника на статията.")]
        public string Origin { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Моля, въведете съдържание.")]
        public string Content { get; set; }
        
        [Display(Name = "Анотация")]
        [Required(ErrorMessage = "Моля, въведете анотация.")]
        public string Description { get; set; }

        [Display(Name = "Снимка")]
        [Required(ErrorMessage = "Моля прикачете изображение.")]
        public IFormFile ImageFile { get; set; }       

        public int? ImageFileId { get; set; }       
    }
}
