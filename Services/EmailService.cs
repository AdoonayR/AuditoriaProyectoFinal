using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AuditoriaQuimicos.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void SendEmailToIncomingSupervisor()
        {
            try
            {
                var smtpClient = new SmtpClient(_configuration["EmailSettings:SMTPServer"])
                {
                    Port = int.Parse(_configuration["EmailSettings:Port"]),
                    Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
                    EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSSL"]),
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["EmailSettings:SenderEmail"]),
                    Subject = "Auditoría pendiente de revisión (Incoming)",
                    Body = "Hay una auditoría pendiente de revisión para el supervisor de Incoming. Por favor, revise los detalles.",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(_configuration["EmailSettings:IncomingSupervisorEmail"]);
                smtpClient.Send(mailMessage);

                _logger.LogInformation("Correo enviado al supervisor de Incoming.");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError($"Error SMTP enviando el correo al supervisor de Incoming: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error general enviando el correo al supervisor de Incoming: {ex.Message}");
            }
        }


        public void SendEmailToStorageSupervisor()
        {
            try
            {
                var smtpClient = new SmtpClient(_configuration["EmailSettings:SMTPServer"])
                {
                    Port = int.Parse(_configuration["EmailSettings:Port"]),
                    Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
                    EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSSL"]),
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["EmailSettings:SenderEmail"]),
                    Subject = "Auditoría pendiente de revisión (Storage)",
                    Body = "Hay una auditoría pendiente de revisión para el supervisor de Storage. Por favor, revise los detalles.",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(_configuration["EmailSettings:StorageSupervisorEmail"]);
                smtpClient.Send(mailMessage);

                _logger.LogInformation("Correo enviado al supervisor de Storage.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error enviando el correo al supervisor de Storage: {ex.Message}");
            }
        }
    }
}
