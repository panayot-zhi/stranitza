namespace stranitza.Utility
{
    public class EmailSettings
    {
        public int MailPort { get; set; }

        public string MailServer { get; set; }
        
        public string MailAdmin { get; set; }

        public string SenderName { get; set; }

        public string Sender { get; set; }

        public string Password { get; set; }

        public bool Debug { get; set; }
    }
}
