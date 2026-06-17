using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GMLSSystem.API.Models
{
    public class InvoiceItem
    {
        [Key]
        public int InvoiceItemId { get; set; }  // Add primary key

        public int InvoiceId { get; set; }  // Foreign key
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal Total => Quantity * UnitPrice;

        // Navigation property
        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }
    }
}
