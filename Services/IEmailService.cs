public interface IEmailService
{
    Task SendEmailToIncomingSupervisorAsync();
    Task SendEmailToStorageSupervisorAsync();
    Task SendDetailsPdfAsync(byte[] pdfBytes, string[] destinatarios, string subject);
}
