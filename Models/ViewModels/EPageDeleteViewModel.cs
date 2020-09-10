using System;
using System.ComponentModel.DataAnnotations;

namespace stranitza.Models.ViewModels
{
    public class EPageDeleteViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Заглавие")]
        public string Title { get; set; }

//        [Required(ErrorMessage = "Моля, попълнете полето.")]
//        [Column(TypeName = "TEXT")]
//        [Display(Name = "Съдържание")]
//        public string Content { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Анотация")]
        public string Description { get; set; }

//        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
//        [Display(Name = "Бележки")]
//        public string Notes { get; set; }

        [Display(Name = "Превод")]
        public bool IsTranslation { get; set; }

        [Display(Name = "Пореден номер")]
        public int ReleaseNumber { get; set; }

        [Display(Name = "Година на публикуване")]
        public int ReleaseYear { get; set; }
        

        [Display(Name = "Свързан автор")]
        public string AuthorId { get; set; }

        [Display(Name = "Свързан автор")]
        public string AuthorUserName { get; set; }

        [Display(Name = "Качил")]
        public string UploaderId { get; set; }

        [Display(Name = "Качил")]
        public string UploaderUserName { get; set; }

        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        [Display(Name = "Категория")]
        public string CategoryName { get; set; }

        [Display(Name = "Източник")]
        public int? SourceId { get; set; }


        [Display(Name = "Дата на създаване")]
        public DateTime DateCreated { get; set; }
    }
}
