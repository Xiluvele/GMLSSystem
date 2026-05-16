using GMLSSystem.Interfaces;
using GMLSSystem.Models;

namespace GMLSSystem.Services.Factories
{
    // Abstract Factory Pattern - Concrete Factories
    public class LocalFactory : IGLMSFactory
    {
        public IInvoiceCreator CreateInvoiceCreator()
        {
            return new LocalInvoiceCreator();
        }

        public INotificationCreator CreateNotificationCreator()
        {
            return new LocalNotificationCreator();
        }
    }

    public class InternationalFactory : IGLMSFactory
    {
        public IInvoiceCreator CreateInvoiceCreator()
        {
            return new InternationalInvoiceCreator();
        }

        public INotificationCreator CreateNotificationCreator()
        {
            return new InternationalNotificationCreator();
        }
    }

    public class LocalInvoiceCreator : IInvoiceCreator
    {
        public Invoice CreateInvoice(Contract contract, decimal amount)
        {
            var invoice = new Invoice
            {
                ContractId = contract.ContractId,
                ClientName = contract.ClientName,
                Currency = "ZAR",
                TaxRate = 15 // VAT
            };

            invoice.Items.Add(new InvoiceItem
            {
                Description = "Logistics Services",
                Quantity = 1,
                UnitPrice = amount
            });
            invoice.CalculateTotals();

            return invoice;
        }
    }

    public class InternationalInvoiceCreator : IInvoiceCreator
    {
        public Invoice CreateInvoice(Contract contract, decimal amount)
        {
            var invoice = new Invoice
            {
                ContractId = contract.ContractId,
                ClientName = contract.ClientName,
                Currency = "USD",
                TaxRate = 0 // No VAT for international
            };

            invoice.Items.Add(new InvoiceItem
            {
                Description = "International Logistics Services",
                Quantity = 1,
                UnitPrice = amount
            });
            invoice.CalculateTotals();

            return invoice;
        }
    }

    public class LocalNotificationCreator : INotificationCreator
    {
        public object CreateNotification(string message, string recipient)
        {
            return new { Type = "Email", Message = message, Recipient = recipient };
        }
    }

    public class InternationalNotificationCreator : INotificationCreator
    {
        public object CreateNotification(string message, string recipient)
        {
            return new { Type = "Email+SMS", Message = message, Recipient = recipient };
        }
    }
}
