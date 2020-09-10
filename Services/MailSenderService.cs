using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using stranitza.Utility;

namespace stranitza.Services
{
    public class MailSenderService : IMailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly IRazorViewToStringRenderer _renderer;
        private readonly IHttpContextAccessor _;
        private readonly IHostingEnvironment _env;
        private readonly LinkGenerator _link;

        public MailSenderService(
            IOptions<EmailSettings> emailSettings,
            IRazorViewToStringRenderer renderer,
            IHttpContextAccessor httpContext,
            IHostingEnvironment env, LinkGenerator link)
        {
            _emailSettings = emailSettings.Value;
            _renderer = renderer;
            _env = env;
            _link = link;
            _ = httpContext;
        }

        public async Task SendMailAsync(string email, string subject, string template, dynamic viewModel)
        {
            await SendMailAsync(new List<string> { email }, null, null, subject, template, viewModel);
        }

        public async Task SendMailAsync(List<string> emails, List<string> ccEmails, List<string> bccEmails, 
            string subject, string template, dynamic viewModel)
        {
            // allow anonymous types to be passed
            viewModel = ToExpandoObject(viewModel);

            var mimeMessage = BuildMimeMessage(emails, ccEmails, bccEmails, subject);

            var builder = new BodyBuilder();

            //EmbedLogo(builder);

            builder.HtmlBody = await GetEmailTemplateAsync(template, viewModel);
            //builder.TextBody = await GetEmailTemplateAsync(template, viewModel);

            mimeMessage.Body = builder.ToMessageBody();

            await SendMimeMessage(mimeMessage);

        }

        private MimeMessage BuildMimeMessage(IEnumerable<string> emails, IReadOnlyCollection<string> ccEmails, IReadOnlyCollection<string> bccEmails, string subject)
        {
            try
            {
                var mimeMessage = new MimeMessage()
                {
                    Subject = subject
                };

                var senderName = _emailSettings.SenderName;
                var senderEmail = _emailSettings.Sender;
                mimeMessage.From.Add(new MailboxAddress(Encoding.UTF8, senderName, senderEmail));

                foreach (var email in emails)
                {
                    mimeMessage.To.Add(new MailboxAddress(email));
                }

                if (ccEmails != null)
                {
                    foreach (var email in ccEmails)
                    {
                        mimeMessage.Cc.Add(new MailboxAddress(email));
                    }
                }

                if (bccEmails != null)
                {
                    foreach (var email in bccEmails)
                    {
                        mimeMessage.Bcc.Add(new MailboxAddress(email));
                    }
                }

                return mimeMessage;
            }
            catch (Exception ex)
            {
                throw new StranitzaException("Възникна грешка при инициализирането на EMAIL съобщението.", ex);
            }
        }

        private async Task SendMimeMessage(MimeMessage mimeMessage)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    if (_env.IsDevelopment())
                    {
                        // The third parameter is useSSL (true if the client should make an SSL-wrapped
                        // connection to the server; otherwise, false).
                        await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort,
                            SecureSocketOptions.StartTlsWhenAvailable);
                    }
                    else
                    {
                        await client.ConnectAsync(_emailSettings.MailServer);
                    }

                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync(_emailSettings.Sender, _emailSettings.Password);

                    await client.SendAsync(mimeMessage);

                    await client.DisconnectAsync(quit: true);
                }
            }
            catch (Exception ex)
            {
                throw new StranitzaException("Възникна грешка при установяването на комуникация с SMTP сървъра.", ex);
            }
        }

        private async Task<string> GetEmailTemplateAsync(string templateName, dynamic viewModel) 
        {
            var view = $"/Views/Emails/{templateName}.cshtml";

            viewModel.Logo = GetLogoUri();
            viewModel.PreviewEmailLink = GetEmailPreviewLink(templateName, viewModel);

            var htmlMessage = await _renderer.RenderViewToStringAsync(view, viewModel);

            return htmlMessage;
        }

        private void EmbedLogo(BodyBuilder builder)
        {
            var logoPath = Path.Combine(_env.WebRootPath, "images\\main-logo.png"); ;
            var logoEntity = builder.LinkedResources.Add(logoPath);
            logoEntity.ContentId = "logo";
        }

        public string GetEmailPreviewLink(string templateName, dynamic viewModel)
        {
            var jsonViewModel = JsonConvert.SerializeObject(viewModel);
            var plainTextBytes = Encoding.UTF8.GetBytes(jsonViewModel);
            var data = Convert.ToBase64String(plainTextBytes);

            return _link.GetUriByAction(_.HttpContext, "Email", "Home", new { id = templateName, data = data },
                options: new LinkOptions()
                {
                    LowercaseUrls = true
                });
        }

        public string GetLogoUri()
        {
            return _link.GetUriByAction(_.HttpContext, "Image", "Home", new { id = "main-logo.png" },
                options: new LinkOptions()
                {
                    LowercaseUrls = true
                });
        }

        private static ExpandoObject ToExpandoObject(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is ExpandoObject expandoObject)
            {
                return expandoObject;
            }

            IDictionary<string, object> dictionary = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
            {
                dictionary.Add(property.Name, property.GetValue(obj));
            }

            return (ExpandoObject) dictionary;
        }
    }
}
