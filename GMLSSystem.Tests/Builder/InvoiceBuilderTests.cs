using System;
using GMLSSystem.Models;
using GMLSSystem.Services.Builders;
using Xunit;

namespace GMLSSystem.Tests.Builder
{
    public class InvoiceBuilderTests
    {
        [Fact]
        public void Build_ValidInvoice_CreatesSuccessfully()
        {
            // Arrange
            var builder = new InvoiceBuilder();

            // Act
            var invoice = builder
                .SetClient("Test Client")
                .AddItem("Shipping Services", 1, 1000)
                .SetTaxRate(15)
                .SetDueDate(DateTime.UtcNow.AddDays(30))
                .Build();

            // Assert
            Assert.NotNull(invoice);
            Assert.Equal("Test Client", invoice.ClientName);
            Assert.Equal(1000, invoice.Subtotal);
            Assert.Equal(150, invoice.TaxAmount);
            Assert.Equal(1150, invoice.TotalAmount);
        }

        [Fact]
        public void Build_WithoutClient_ThrowsException()
        {
            // Arrange
            var builder = new InvoiceBuilder();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                builder.AddItem("Service", 1, 100).Build()
            );
        }

        [Fact]
        public void Build_WithoutItems_ThrowsException()
        {
            // Arrange
            var builder = new InvoiceBuilder();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                builder.SetClient("Test Client").Build()
            );
        }

        [Fact]
        public void AddItem_NegativeQuantity_ThrowsException()
        {
            // Arrange
            var builder = new InvoiceBuilder();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                builder.AddItem("Service", -1, 100)
            );
        }

        [Fact]
        public void SetTaxRate_InvalidRate_ThrowsException()
        {
            // Arrange
            var builder = new InvoiceBuilder();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                builder.SetTaxRate(150)
            );
        }

        [Fact]
        public void MultipleItems_CalculatesCorrectTotal()
        {
            // Arrange
            var builder = new InvoiceBuilder();

            // Act
            var invoice = builder
                .SetClient("Test Client")
                .AddItem("Item 1", 2, 100)
                .AddItem("Item 2", 1, 50)
                .AddItem("Item 3", 3, 25)
                .SetTaxRate(15)
                .SetDueDate(DateTime.UtcNow.AddDays(30))
                .Build();

            // Assert
            Assert.Equal(325, invoice.Subtotal); // (2*100) + 50 + (3*25) = 325
            Assert.Equal(48.75m, invoice.TaxAmount);
            Assert.Equal(373.75m, invoice.TotalAmount);
        }
    }
}