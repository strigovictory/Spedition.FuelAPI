using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Shared.Entities
{
    [Table("TFuelCardType", Schema = "dbo")]
    [AutoMap(typeof(FuelProviderResponse), ReverseMap = true)]
    public partial class FuelProvider
    {
        public FuelProvider()
        {
            FuelCardsAccounts = new HashSet<ProvidersAccount>();
            FuelTransactions = new HashSet<FuelTransaction>();
            NotFoundFuelCards = new HashSet<NotFoundFuelCard>();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? CountryId { get; set; }

        public virtual ICollection<ProvidersAccount> FuelCardsAccounts { get; set; }

        public virtual ICollection<FuelTransaction> FuelTransactions { get; set; }

        public virtual ICollection<NotFoundFuelCard> NotFoundFuelCards { get; set; }
    }
}
