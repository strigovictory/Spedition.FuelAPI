namespace Spedition.Fuel.Shared.Entities;

[Table("kit_event_types", Schema = "dbo")]
public class KitEventType
{
    [Key]
    [Column("type_id")]
    public int Id { get; set; }

    [Column("type_name")]
    public string Name { get; set; }
}
