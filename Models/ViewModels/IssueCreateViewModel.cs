using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace stranitza.Models.ViewModels
{
    public class IssueCreateViewModel : IValidatableObject
    {
        [Display(Name = "Брой №")]
        [Required(ErrorMessage = "Моля, въведете пореден номер на изданието.")]        
        public int? IssueNumber { get; set; }
        
        [Display(Name = "Номер")]
        [Required(ErrorMessage = "Моля, въведете поредният номер на изданието за годината.")]
        public int? ReleaseNumber { get; set; }

        [Display(Name = "Година на издаване")]
        [Required(ErrorMessage = "Моля, въведете година на издаване.")]        
        public int? ReleaseYear { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Допълнителна информация")]
        public string Description { get; set; }

        [Display(Name = "Броят е достъпен")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Прикачете броят като документ (PDF)")]
        public IFormFile PdfFile { get; set; }

        [Display(Name = "Прикачете страниците, като изображения")]
        public Collection<IFormFile> PageFiles { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PageFiles == null || PageFiles.Count < 2)
            {
                yield return new ValidationResult("Моля прикачете страниците на списанието (поне две - за корица и съдържание) като изображения.", new [] { nameof(PageFiles) } );
            }

            // file checks
        }
    }
}
