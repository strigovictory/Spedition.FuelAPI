using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Shared.Entities;

[Table("TRideCostCategories", Schema = "dbo")]
[AutoMap(typeof(FuelTypeResponse), ReverseMap = true)]
public partial class FuelType
{
    public FuelType()
    {
        FuelCardsEntries = new HashSet<FuelTransaction>();
    }

    [Key]
    public int Id { get; set; }

    [Column("CategoryName")]
    public string Name { get; set; }

    public virtual ICollection<FuelTransaction> FuelCardsEntries { get; set; }
}
