using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace stranitza.Models.ViewModels
{
    public class PostEditViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Заглавие")]
        [Required(ErrorMessage = "Заглавието е задължително поле. Моля, въведете заглавие.")]
        public string Title { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Required(ErrorMessage = "Моля, въведете източника на статията.")]
        [Display(Name = "Източник")]
        public string Origin { get; set; }

        public int? ImageFileId { get; set; }

        public string ImageTitle { get; set; }

        public string ImageFileName { get; set; }

        [Display(Name = "Снимка")]
        public IFormFile ImageFile { get; set; }        

        [Display(Name = "Качил")]
        public string Uploader { get; set; }

        public string UploaderId { get; set; }
        
        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Моля, въведете съдържание.")]
        public string Content { get; set; }
        
        [Display(Name = "Анотация")]
        [Required(ErrorMessage = "Моля, въведете анотация.")]
        public string Description { get; set; }

        [Display(Name = "Избрано от редактора")]
        public bool EditorsPick { get; set; }

        [Display(Name = "Брой коментари")]
        public int CommentsCount { get; set; }


        [Display(Name = "Последна редакция")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "Дата на създаване")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime DateCreated { get; set; }
    }
}
