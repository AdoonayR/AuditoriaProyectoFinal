namespace AuditoriaQuimicos.Services
{
    public interface IEmailService
    {
        void SendEmailToIncomingSupervisor();
        void SendEmailToStorageSupervisor();
    }
}
