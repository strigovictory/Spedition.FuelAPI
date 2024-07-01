using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Shared.DTO.RequestModels.FuelModels
{
    [AutoMap(typeof(FuelCardShortResponse), ReverseMap = true)]
    [AutoMap(typeof(FuelCardResponse), ReverseMap = true)]
    public class FuelCardRequest
    {
        [JsonInclude]
        public int Id { get; set; }

        [JsonInclude]
        public string Number { get; set; }

        [JsonInclude]
        public DateTime? ExpirationDate { get; set; }

        [JsonInclude]
        public DateTime? ReceiveDate { get; set; }

        [JsonInclude]
        public DateTime? IssueDate { get; set; }

        [JsonInclude]
        public bool IsReserved { get; set; }

        [JsonInclude]
        public string Note { get; set; }

        [JsonInclude]
        public bool IsArchived { get; set; }

        [JsonInclude]
        public int? CarId { get; set; }

        [JsonInclude]
        public int DivisionID { get; set; }

        [JsonInclude]
        public int ProviderId { get; set; }

        [JsonInclude]
        public int? EmployeeId { get; set; }
    }
}
