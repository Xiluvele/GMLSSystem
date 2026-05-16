using GMLSSystem.Interfaces;
using GMLSSystem.Models;

namespace GMLSSystem.Services.Builders
{
    // Builder Pattern Implementation
    public class InvoiceBuilder : IInvoiceBuilder
    {
        private readonly Invoice _invoice;

        public InvoiceBuilder()
        {
            _invoice = new Invoice();
        }

        public IInvoiceBuilder SetClient(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException("Client name is required");

            _invoice.ClientName = clientName;
            return this;
        }

        public IInvoiceBuilder AddItem(string description, decimal quantity, decimal unitPrice)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");
            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative");

            _invoice.Items.Add(new InvoiceItem
            {
                Description = description,
                Quantity = quantity,
                UnitPrice = unitPrice
            });
            return this;
        }

        public IInvoiceBuilder SetTaxRate(decimal taxRate)
        {
            if (taxRate < 0 || taxRate > 100)
                throw new ArgumentException("Tax rate must be between 0 and 100");

            _invoice.TaxRate = taxRate;
            return this;
        }

        public IInvoiceBuilder SetDueDate(DateTime dueDate)
        {
            if (dueDate < DateTime.UtcNow)
                throw new ArgumentException("Due date cannot be in the past");

            _invoice.DueDate = dueDate;
            return this;
        }

        public IInvoiceBuilder SetCurrency(string currency)
        {
            _invoice.Currency = currency;
            return this;
        }

        public Invoice Build()
        {
            if (string.IsNullOrWhiteSpace(_invoice.ClientName))
                throw new InvalidOperationException("Client name is required before building");
            if (_invoice.Items.Count == 0)
                throw new InvalidOperationException("At least one invoice item is required");

            _invoice.CalculateTotals();
            return _invoice;
        }
    }

    // Optional: Director class for complex invoice construction
    public class InvoiceDirector
    {
        public Invoice ConstructStandardInvoice(IInvoiceBuilder builder, string clientName, decimal amount)
        {
            return builder
                .SetClient(clientName)
                .AddItem("Logistics Services - Standard", 1, amount)
                .SetTaxRate(15)
                .SetDueDate(DateTime.UtcNow.AddDays(30))
                .SetCurrency("ZAR")
                .Build();
        }
    }
}
