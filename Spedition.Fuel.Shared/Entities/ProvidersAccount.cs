namespace Spedition.Fuel.Shared.Entities
{
    [Table("FuelCardsAccounts", Schema = "dbo")]
    public partial class ProvidersAccount
    {
        [Key]
        public int Id { get; set; }

        public string BaseUrl { get; set; }

        public int DivisionId { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Key { get; set; }

        public int ProviderId { get; set; }

        public virtual FuelProvider Provider { get; set; }
    }
}
