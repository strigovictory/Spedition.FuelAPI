using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Shared.Entities
{
    /// <summary>
    /// Дубликат номера топливной карты - из отчета по топливу.
    /// </summary>
    [Table("FuelCardsAlternativeNumbers", Schema = "dbo")]
    [AutoMap(typeof(FuelCardsAlternativeNumberResponse), ReverseMap = true)]
    [AutoMap(typeof(FuelCardsAlternativeNumberRequest), ReverseMap = true)]
    public partial class FuelCardsAlternativeNumber : EntityModified
    {
        [Key]
        public int Id { get; set; }

        [Column("AlternativeNumber")]
        public string Number { get; set; }

        [Column("FuelCardId")]
        public int CardId { get; set; }

        public virtual FuelCard Card { get; set; }
    }
}
