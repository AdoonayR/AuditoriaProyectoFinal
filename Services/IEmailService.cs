public interface IEmailService
{
    Task SendEmailToIncomingSupervisorAsync();
    Task SendEmailToStorageSupervisorAsync(); 
}
