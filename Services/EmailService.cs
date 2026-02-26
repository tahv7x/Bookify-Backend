using System.Net;
using System.Net.Mail;

namespace Bookify_API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void Send(string toEmail, string subject, string body)
        {
            var settings = _config.GetSection("EmailSettings");

            var smtp = new SmtpClient
            {
                Host = settings["SmtpServer"],
                Port = int.Parse(settings["Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    settings["Username"],
                    settings["Password"]
                )
            };

            var message = new MailMessage
            {
                From = new MailAddress(settings["From"], "Bookify"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            smtp.Send(message);
        }
    }
}
