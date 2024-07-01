namespace Spedition.Fuel.Shared.Entities;

[Table("FuelCardsCountry", Schema = "dbo")]
public class FuelCardsCountry
{
    [Column("FuelCardsCountryCode")]
    [Key]
    public int CountryCode { get; set; }

    public int CountryId { get; set; }

    public string Name { get; set; }
}