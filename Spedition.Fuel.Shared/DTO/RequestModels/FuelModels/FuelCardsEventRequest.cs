using Spedition.Fuel.Shared.DTO.ResponseModels;

namespace Spedition.Fuel.Shared.DTO.RequestModels.FuelModels
{
    public partial class FuelCardsEventRequest : EntityModifiedRequest
    {
        [JsonInclude]
        public int Id { get; set; }

        [JsonInclude]
        public int CardId { get; set; }

        [JsonInclude]
        public int? CarId { get; set; }

        [JsonInclude]
        public int? DivisionID { get; set; }

        [JsonInclude]
        public int? EmployeeId { get; set; }

        [JsonInclude]
        public int EventTypeId { get; set; }

        [JsonInclude]
        public DateTime StartDate { get; set; }

        [JsonInclude]
        public DateTime? FinishDate { get; set; }

        [JsonInclude]
        public string Comment { get; set; }
    }
}
