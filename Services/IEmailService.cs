public interface IEmailService
{
    Task SendEmailToIncomingSupervisorAsync();
    Task SendEmailToStorageSupervisorAsync(); // Cambiar a Task
}
