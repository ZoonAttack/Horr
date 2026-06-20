using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Payment
{
    [Table("exchange_rates")]
    public class ExchangeRate
    {
        [Key]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,6)")]
        public decimal Rate { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
