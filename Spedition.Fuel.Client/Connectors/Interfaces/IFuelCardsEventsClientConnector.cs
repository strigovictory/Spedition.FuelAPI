using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.Client.Connectors.Interfaces;

public interface IFuelCardsEventsClientConnector
{
    Task<List<FuelCardsEventResponse>> GetFuelCardsEvents(int cardId, CancellationToken token = default);

    Task<FuelCardsEventResponse> GetFuelCardsEventPrevious(int eventId, CancellationToken token = default);

    Task<FuelCardsEventResponse> GetFuelCardsEventNext(int eventId, CancellationToken token = default);

    Task<ResponseSingleAction<FuelCardsEventResponse>> PutCardsEvent(RequestSingleAction<FuelCardsEventRequest> cardsEvent, CancellationToken token = default);

    Task<ResponseSingleAction<FuelCardsEventResponse>> PostCardsEvent(RequestSingleAction<FuelCardsEventRequest> cardsEvent, CancellationToken token = default);

    Task<ResponseBase> DeleteCardsEvent(FuelCardsEventRequest cardsEvent, CancellationToken token = default);
}
