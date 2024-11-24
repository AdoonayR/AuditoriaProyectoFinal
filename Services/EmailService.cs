using MailKit.Net.Smtp;
using MimeKit;
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

        public async Task SendEmailToIncomingSupervisorAsync()
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sistema de Auditoría", _configuration["EmailSettings:SenderEmail"]));
                message.To.Add(new MailboxAddress("Supervisor Incoming", _configuration["EmailSettings:IncomingSupervisorEmail"]));
                message.Subject = "Auditoría pendiente de revisión (Incoming)";
                message.Body = new TextPart("plain")
                {
                    Text = "Hay una auditoría pendiente de revisión para el supervisor de Incoming. Por favor, revise los detalles en el sistema."
                };

                using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
                await smtpClient.ConnectAsync(_configuration["EmailSettings:SMTPServer"], int.Parse(_configuration["EmailSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Correo enviado exitosamente al supervisor de Incoming.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error enviando correo al supervisor de Incoming: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmailToStorageSupervisorAsync()
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sistema de Auditoría", _configuration["EmailSettings:SenderEmail"]));
                message.To.Add(new MailboxAddress("Supervisor Storage", _configuration["EmailSettings:StorageSupervisorEmail"]));
                message.Subject = "Auditoría pendiente de revisión (Storage)";
                message.Body = new TextPart("plain")
                {
                    Text = "Hay una auditoría pendiente de revisión para el supervisor de Storage. Por favor, revise los detalles en el sistema."
                };

                using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
                await smtpClient.ConnectAsync(_configuration["EmailSettings:SMTPServer"], int.Parse(_configuration["EmailSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Correo enviado exitosamente al supervisor de Storage.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error enviando correo al supervisor de Storage: {ex.Message}");
                throw;
            }
        }
    }

}
