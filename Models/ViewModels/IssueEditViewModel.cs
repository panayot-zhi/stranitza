using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class IssueEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "PDF файл")]
        public int? PdfFilePreviewId { get; set; }

        [Display(Name = "PDF файл")]
        public int? PdfFileDownloadId { get; set; }

        [Display(Name = "PDF файл")]
        public string PdfFileName { get; set; }

        [Display(Name = "Броят има PDF")]
        public bool HasPdf => PdfFilePreviewId.HasValue;

        public int? ZipFileId { get; set; }

        [Display(Name = "ZIP файл")]
        public string ZipFileName { get; set; }

        [Display(Name = "Брой коментари")]
        public int CommentsCount { get; set; }

        [Display(Name = "Свързани източника")]
        public int SourcesCount { get; set; }

        public PageViewModel CoverPage { get; set; }
        
//        public int CoverPageId { get; set; }

//        [Display(Name = "Корица на списанието №")]
//        [Required(ErrorMessage = "Моля, въведете номер на страница.")]
//        [Range(0, 999, ErrorMessage = "Моля, коригирайте номера на страницата.")]
//        public int CoverPageNumber { get; set; }        


        public PageViewModel IndexPage { get; set; }

//        public int IndexPageId { get; set; }

//        [Display(Name = "Страница със съдържание №")]
//        [Required(ErrorMessage = "Моля, въведете номер на страница.")]
//        [Range(0, 999, ErrorMessage = "Моля, коригирайте номера на страницата.")]
//        public int IndexPageNumber { get; set; }

        [Display(Name = "Брой №")]
        public int IssueNumber { get; set; }

        [Display(Name = "Номер")]
        public int ReleaseNumber { get; set; }

        [Display(Name = "Година на издаване")]
        public int ReleaseYear { get; set; }

        [Display(Name = "Допълнителна информация")]
        public string Description { get; set; }

        [Display(Name = "Броят е достъпен")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Брой на изображенията")]
        public int ImagePagesCount { get; set; }

        [Display(Name = "Брой на страниците (в PDF)")]
        public int PdfPagesCount { get; set; }

        [Display(Name = "Достъпни страници в PDF формат")]
        [BindProperty(BinderType = typeof(CsvToArrayModelBinder<int>))]
        public int[] AvailablePages { get; set; }

        public bool AvailablePagesChanged { get; set; }

        [Display(Name = "Ключови думи")]
        public string Tags { get; set; }

        [Display(Name = "Прикачете броят като документ (PDF)")]
        public IFormFile PdfFile { get; set; }

        [Display(Name = "Прикачете страниците, като изображения")]
        public Collection<IFormFile> PageFiles { get; set; }


        [Display(Name = "Дата на последна редакция")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "Дата на създаване")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime DateCreated { get; set; }
    }
}