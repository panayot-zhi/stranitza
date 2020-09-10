namespace stranitza.Models.ViewModels
{
    public class SharerViewModel
    {
        public string Url { get; set; }

        public string Title { get; set; }

        public string Descritpion { get; set; }
        

        public string Locale { get; set; } = "bg_BG";

        public string Type { get; set; } = "article";


        public string Image { get; set; }
    }
}
