﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace stranitza.Models.ViewModels
{
    public class SourceEditViewModel
    {
        public int Id { get; set; }

        [MaxLength(255, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [MaxLength(255, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Моля, попълнете полето.")]
        [MaxLength(512, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Произход")]
        public string Origin { get; set; }

        [MaxLength(512, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Заглавие")]
        public string Title { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Кратко описание")]
        public string Description { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Бележки")]
        public string Notes { get; set; }

        // NOTE: Since epages automatically create index entries
        // it is only sensible to add sources to an existing issue

        [Display(Name = "Брой №")]
        public int ReleaseNumber { get; set; }

        [Display(Name = "Година на издаване")]
        public int ReleaseYear { get; set; }

        [MaxLength(64, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Страници (от-до)")]
        public string Pages { get; set; }

        [Display(Name = "Начална")]
        public int StartingPage { get; set; }

        [Display(Name = "Превод")]
        public bool IsTranslation { get; set; }


        [Display(Name = "Качил")]
        public string Uploader { get; set; }


        [Required(ErrorMessage = "Моля, попълнете полето.")]
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        [Display(Name = "Категории")]
        public SelectList Categories { get; set; }


        [Display(Name = "Брой")]
        public int? IssueId { get; set; }

        [MaxLength(127, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
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
