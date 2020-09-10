using System;
using System.ComponentModel.DataAnnotations;

namespace stranitza.Models.ViewModels
{
    public class EPageDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Заглавие")]
        public string Title { get; set; }


        [Display(Name = "Съдържание")]
        public string Content { get; set; }
        
        [Display(Name = "Анотация")]
        public string Description { get; set; }
        
        [Display(Name = "Бележки")]
        public string Notes { get; set; }


        [Display(Name = "Превод")]
        public bool IsTranslation { get; set; }

        public int ReleaseNumber { get; set; }

        public int ReleaseYear { get; set; }
        

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public int CommentsCount { get; set; }

        public string AuthorId { get; set; }        

        public string AuthorNames { get; set; }        
        
        public string UploaderId { get; set; }

        public string UploaderNames { get; set; }

        public int? SourceId { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
