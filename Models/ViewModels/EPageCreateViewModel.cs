using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class EPageCreateViewModel
    {
        [MaxLength(255, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Име")]
        [Required(ErrorMessage = "Моля, попълнете полето.")]
        [RegularExpression(StranitzaConstants.CyrillicNamePattern, ErrorMessage = "Моля, въведете име на кирилица.")]
        public string FirstName { get; set; }

        [MaxLength(255, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Фамилия")]
        [Required(ErrorMessage = "Моля, попълнете полето.")]
        [RegularExpression(StranitzaConstants.CyrillicNamePattern, ErrorMessage = "Моля, въведете име на кирилица.")]
        public string LastName { get; set; }

        [MaxLength(512, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Required(ErrorMessage = "Моля, попълнете полето.")]
        [Display(Name = "Заглавие")]
        public string Title { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Моля, попълнете полето.")]
        public string Content { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Анотация")]
        public string Description { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Бележки")]
        public string Notes { get; set; }

        [Display(Name = "Превод")]
        public bool IsTranslation { get; set; }
        

        [Display(Name = "Категория")]
        [Required(ErrorMessage = "Моля, попълнете полето.")]
        public int CategoryId { get; set; }

        public SelectList Categories { get; set; }

//        [MaxLength(127, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
//        [ForeignKey("Author")]
//        public string AuthorId { get; set; }        
//        public ApplicationUser Author { get; set; }        
        
    }
}
