using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Shared.Entities
{
    [Table("fuel_card_events", Schema = "dbo")]
    [AutoMap(typeof(FuelCardsEventResponse), ReverseMap = true)]
    [AutoMap(typeof(FuelCardsEventRequest), ReverseMap = true)]
    public partial class FuelCardsEvent : EntityModified
    {
        [Key]
        [Column("event_id")]
        public int Id { get; set; }

        [Column("card_id")]
        public int CardId { get; set; }

        [Column("car_id")]
        public int? CarId { get; set; }

        [Column("division_id")]
        public int? DivisionID { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Column("event_type_id")]
        public int EventTypeId { get; set; }

        [Column("event_start_date")]
        public DateTime StartDate { get; set; }

        [Column("finish_date")]
        public DateTime? FinishDate { get; set; }

        public string Comment { get; set; } = string.Empty;

        public virtual FuelCard Card { get; set; }
    }
}
