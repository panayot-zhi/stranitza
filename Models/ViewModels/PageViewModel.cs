using System;
using System.ComponentModel.DataAnnotations;
using stranitza.Utility;

namespace stranitza.Models.ViewModels
{
    public class PageViewModel
    {
        #region Display
        
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }

        [Display(Name = "Файлов идентификатор")]
        public int PageFileId { get; set; }

        [Display(Name = "Идентификатор на брой")]
        public int IssueId { get; set; }
        
        [Display(Name = "Тип на страницата")]
        [Required(ErrorMessage = "Моля, изберете тип на страницата.")]
        public StranitzaPageType? Type { get; set; }

        [Display(Name = "Номер")]
        [Required(ErrorMessage = "Моля, въведете номер на страницата.")]
        public int? PageNumber { get; set; }

        [Display(Name = "Пореден номер в галерия")]
        public int SlideNumber { get; set; }

        [Display(Name = "Разрешена")]
        public bool IsAvailable { get; set; }

        #endregion

        #region Details

        [Display(Name = "Брой №")]
        public int IssueNumber { get; set; }

        [Display(Name = "Пореден номер")]
        public int ReleaseNumber { get; set; }

        [Display(Name = "Година на издаване")]
        public int ReleaseYear { get; set; }

        [Display(Name = "Име на файл")]
        public string FileName { get; set; }

        [Display(Name = "Вид файл")]
        public string MimeType { get; set; }

        [Display(Name = "Заглавие на файл")]
        public string FileTitle { get; set; }

        [Display(Name = "Разширение")]
        public string FileExtension { get; set; }

        [Display(Name = "Път до изображение")]
        public string FilePath { get; set; }

        [Display(Name = "Път до смалено изображение")]
        public string ThumbPath { get; set; }

        [Display(Name = "Файл последна промяна на")]
        public DateTime FileLastUpdated { get; set; }

        [Display(Name = "Файл създаден на")]
        public DateTime FileDateCreated { get; set; }

        [Display(Name = "Страница последна промяна на")]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "Страница създадена на")]
        public DateTime DateCreated { get; set; }

        #endregion
    }
}
