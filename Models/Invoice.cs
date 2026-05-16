using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMLSSystem.Models
{
    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Paid,
        Overdue,
        Cancelled
    }

    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        public string InvoiceNumber { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required]
        public string ClientName { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string Currency { get; set; }

        // Add missing Status property
        public InvoiceStatus Status { get; set; }

        // Add missing Notes property
        public string Notes { get; set; }

        // Add missing CreatedAt property
        public DateTime CreatedAt { get; set; }

        // Add missing PaidAt property
        public DateTime? PaidAt { get; set; }

        // Navigation property - Contract (singular, not Contracts)
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; }

        // Collection of items
        public virtual ICollection<InvoiceItem> Items { get; set; }

        public Invoice()
        {
            Items = new List<InvoiceItem>();
            InvoiceNumber = GenerateInvoiceNumber();
            InvoiceDate = DateTime.UtcNow;
            DueDate = InvoiceDate.AddDays(30);
            CreatedAt = DateTime.UtcNow;
            Status = InvoiceStatus.Draft;
            Currency = "ZAR";
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }

        public void CalculateTotals()
        {
            Subtotal = 0;
            foreach (var item in Items)
            {
                Subtotal += item.Total;
            }
            TaxAmount = Subtotal * (TaxRate / 100);
            TotalAmount = Subtotal + TaxAmount;
        }
    }
}