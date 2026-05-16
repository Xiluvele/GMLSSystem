using GMLSSystem.Models;

namespace GMLSSystem.Interfaces
{
    public interface IInvoiceBuilder
    {
        IInvoiceBuilder SetClient(string clientName);
        IInvoiceBuilder AddItem(string description, decimal quantity, decimal unitPrice);
        IInvoiceBuilder SetTaxRate(decimal taxRate);
        IInvoiceBuilder SetDueDate(DateTime dueDate);
        IInvoiceBuilder SetCurrency(string currency);
        Invoice Build();
    }
}
