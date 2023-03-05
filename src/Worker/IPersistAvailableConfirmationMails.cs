namespace Worker
{
    public interface IPersistAvailableConfirmationMails
    {
        Task<ConfirmationMail?> GetAvailableConfirmationMail();

        Task MarkAsSent(ConfirmationMail mail);

        Task MarkAsPending(ConfirmationMail mail);
    }
}