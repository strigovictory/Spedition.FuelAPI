using Spedition.Fuel.Shared.DTO.ResponseModels;

namespace Spedition.Fuel.Shared.Entities;

[AutoMap(typeof(EntityModifiedResponse), ReverseMap = true)]
public abstract class EntityModified
{
    [Column("change_date")]
    public DateTime? ModifiedOn { get; set; }

    [Column("change_username")]
    public string ModifiedBy { get; set; }
}
