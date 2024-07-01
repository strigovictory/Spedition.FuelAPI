namespace Spedition.Fuel.Shared.Entities
{
    [Table("TCarFuelCardNotFounds", Schema = "dbo")]
    public partial class NotFoundFuelCard
    {
        [Key]
        public int Id { get; set; }

        public string Number { get; set; } = string.Empty;

        [Column("UserId")]
        public string UserName { get; set; } = string.Empty;

        public DateTime ImportDate { get; set; }

        public int FuelProviderId { get; set; }

        public virtual FuelProvider FuelCardType { get; set; }
    }
}
