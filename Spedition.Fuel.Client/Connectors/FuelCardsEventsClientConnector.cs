using System.Net.Http;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.Client.Connectors;

public class FuelCardsEventsClientConnector  : ConnectorBase, IFuelCardsEventsClientConnector
{
    public FuelCardsEventsClientConnector(IHttpClientService httpClientService, IFuelApiConfigurationProvider apiVersionProvider)
        : base(httpClientService, apiVersionProvider)
    {
    }

    protected override UriSegment UriSegment => UriSegment.fuelcardsevents;

    public async Task<List<FuelCardsEventResponse>> GetFuelCardsEvents(int cardId, CancellationToken token = default)
    {
        return await httpClientService?.SendRequestAsync<List<FuelCardsEventResponse>>(
            Uri + $"/{cardId}", HttpMethod.Get, token);
    }

    public async Task<FuelCardsEventResponse> GetFuelCardsEventPrevious(int eventId, CancellationToken token = default)
    {
        return await httpClientService?.SendRequestAsync<FuelCardsEventResponse>(
            Uri + $"/previous/{eventId}", HttpMethod.Get, token);
    }

    public async Task<FuelCardsEventResponse> GetFuelCardsEventNext(int eventId, CancellationToken token = default)
    {
        return await httpClientService?.SendRequestAsync<FuelCardsEventResponse>(
            Uri + $"/next/{eventId}", HttpMethod.Get, token);
    }

    public async Task<ResponseSingleAction<FuelCardsEventResponse>> PutCardsEvent(RequestSingleAction<FuelCardsEventRequest> cardsEvent, CancellationToken token = default)
    {
        return await httpClientService?
            .SendRequestAsync<RequestSingleAction<FuelCardsEventRequest>, ResponseSingleAction<FuelCardsEventResponse>>(
                Uri, HttpMethod.Put, cardsEvent, token);
    }

    public async Task<ResponseSingleAction<FuelCardsEventResponse>> PostCardsEvent(RequestSingleAction<FuelCardsEventRequest> cardsEvent, CancellationToken token = default)
    {
        return await httpClientService?
            .SendRequestAsync<RequestSingleAction<FuelCardsEventRequest>, ResponseSingleAction<FuelCardsEventResponse>>(
                Uri, HttpMethod.Post, cardsEvent, token);
    }

    public async Task<ResponseBase> DeleteCardsEvent(FuelCardsEventRequest cardsEvent, CancellationToken token = default)
    {
        return await httpClientService?
            .SendRequestAsync<FuelCardsEventRequest, ResponseSingleAction<FuelCardsEventResponse>>(
                Uri + "/delete", HttpMethod.Post, cardsEvent, token);
    }
}
