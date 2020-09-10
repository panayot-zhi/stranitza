using System.Collections.Generic;
using System.Threading.Tasks;

namespace stranitza.Utility
{
    public interface IMailSender
    {
        Task SendMailAsync(string email, string subject, string template, dynamic viewModel);

        Task SendMailAsync(List<string> emails, List<string> ccEmails, List<string> bccEmails,
            string subject, string template, dynamic viewModel);

        string GetEmailPreviewLink(string templateName, dynamic viewModel);

        string GetLogoUri();
    }
}
