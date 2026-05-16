namespace GMLSSystem.Interfaces
{
    // Abstract Factory Pattern
    public interface IGLMSFactory
    {
        IInvoiceCreator CreateInvoiceCreator();
        INotificationCreator CreateNotificationCreator();
    }

    public interface IInvoiceCreator
    {
        Models.Invoice CreateInvoice(Models.Contract contract, decimal amount);
    }

    public interface INotificationCreator
    {
        object CreateNotification(string message, string recipient);
    }
}
