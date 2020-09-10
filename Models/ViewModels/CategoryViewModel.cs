using System;
using System.ComponentModel.DataAnnotations;

namespace stranitza.Models.ViewModels
{
    public class CategoryViewModel
    {
        [Display(Name = "№")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Моля, попълнете полето.")]
        [MaxLength(127, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [MaxLength(1024, ErrorMessage = "Надвишава позволения размер от {1} символа.")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Всички източници")]
        public int AllSourcesCount { get; set; }

        [Display(Name = "Брой източници")]        
        public int SourcesCount => AllSourcesCount - EPagesCount;

        [Display(Name = "Брой е-страници")]
        public int EPagesCount { get; set; }

        [Display(Name = "Последна промяна")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "Създадена")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime DateCreated { get; set; }

    }
}
