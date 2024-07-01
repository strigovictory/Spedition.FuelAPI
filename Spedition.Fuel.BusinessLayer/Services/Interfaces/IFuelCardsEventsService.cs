using Microsoft.AspNetCore.Http;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.BusinessLayer.Services.Interfaces
{
    public interface IFuelCardsEventsService
    {
        Task<List<FuelCardsEventResponse>> GetCardsEvents(int cardId, CancellationToken token = default);

        Task<FuelCardsEventResponse> GetCardsEventPrevious(int eventId, CancellationToken token = default);

        Task<FuelCardsEventResponse> GetCardsEventNext(int eventId, CancellationToken token = default);

        Task<FuelCardsEventResponse> UpdateCardsEvent(FuelCardsEventRequest cardsEvent, string userName, CancellationToken token = default);

        Task<FuelCardsEventResponse> CreateCardsEvent(FuelCardsEventRequest cardsEvent, string userName, CancellationToken token = default);

        Task<bool> DeleteLastCardsEvent(FuelCardsEventRequest cardsEvent, CancellationToken token = default);
    }
}
