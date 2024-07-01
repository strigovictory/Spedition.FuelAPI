namespace Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

public class FuelCardsEventResponse : EntityModifiedResponse
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
