using System;

namespace stranitza.Models.ViewModels
{
    public class PostIndexViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Origin { get; set; }

        public string ImageFileName { get; set; }

        public string ImageTitle { get; set; }        

        public string Description { get; set; }

        public bool EditorsPick { get; set; }

        public int ViewCount { get; set; }
       
        /*public string UploaderNames { get; set; }*/

        public int CommentsCount { get; set; }        

        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
