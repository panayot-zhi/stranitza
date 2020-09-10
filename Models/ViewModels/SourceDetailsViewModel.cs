using System;
using System.ComponentModel.DataAnnotations;

namespace stranitza.Models.ViewModels
{
    public class SourceDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Произход")]
        public string Origin { get; set; }

        [Display(Name = "Заглавие")]
        public string Title { get; set; }

        [Display(Name = "Кратко описание")]
        public string Description { get; set; }

        [Display(Name = "Бележки")]
        public string Notes { get; set; }

        [Display(Name = "Пореден брой")]
        public int ReleaseNumber { get; set; }

        [Display(Name = "Година на издаване")]
        public int ReleaseYear { get; set; }

        [Display(Name = "Страници (от-до)")]
        public string Pages { get; set; }

        [Display(Name = "Начална страница №")]
        public int StartingPage { get; set; }

        [Display(Name = "Превод")]
        public bool IsTranslation { get; set; }

        [Display(Name = "Качил")]
        public string Uploader { get; set; }

        [Display(Name = "Категория")]
        public string CategoryName { get; set; }


        [Display(Name = "Категория №")]
        public int CategoryId { get; set; }

        [Display(Name = "Брой")]
        public int? IssueId { get; set; }

        [Display(Name = "е-страница")]
        public int? EPageId { get; set; }

        [Display(Name = "Автор")]
        public string AuthorId { get; set; }

        [Display(Name = "Свързан автор")]
        public string AuthorUserName { get; set; }


        [Display(Name = "Последна промяна")]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "Създаден")]
        public DateTime DateCreated { get; set; }
    }
}
