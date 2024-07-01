using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Shared.Entities
{
    [Table("TCarFuelCards", Schema = "dbo")]
    [AutoMap(typeof(FuelCardFullResponse), ReverseMap = true)]
    [AutoMap(typeof(FuelCardShortResponse), ReverseMap = true)]
    [AutoMap(typeof(FuelCardRequest), ReverseMap = true)]
    [AutoMap(typeof(FuelCardResponse), ReverseMap = true)]
    public partial class FuelCard
    {
        public FuelCard()
        {
            FuelCardsEntries = new HashSet<FuelTransaction>();
            FuelCardsAlternativeNumbers = new HashSet<FuelCardsAlternativeNumber>();
            FuelCardsEvents = new HashSet<FuelCardsEvent>();
        }

        [Key]
        public int Id { get; set; }

        [Column("FK_CarID")]
        public int? CarId { get; set; }

        public int DivisionID { get; set; }

        public int? EmployeeId { get; set; }

        [Column("FK_FuelCardTypeID")]
        public int ProviderId { get; set; }

        public string Number { get; set; } = string.Empty;

        public DateTime? ExpirationDate { get; set; }

        public int? ExpirationMonth { get; set; }

        public int? ExpirationYear { get; set; }

        public DateTime? ReceiveDate { get; set; }

        public DateTime? IssueDate { get; set; }

        [Column("IsReserve")]
        public bool IsReserved { get; set; }

        public string Note { get; set; }

        [Column("IsArchive")]
        public bool IsArchived { get; set; }

        public virtual ICollection<FuelTransaction> FuelCardsEntries { get; set; }

        public virtual ICollection<FuelCardsAlternativeNumber> FuelCardsAlternativeNumbers { get; set; }

        public virtual ICollection<FuelCardsEvent> FuelCardsEvents { get; set; }

        public override string ToString()
        {
            return $"Заправочная карта «{Number ?? string.Empty}»";
        }
    }
}
