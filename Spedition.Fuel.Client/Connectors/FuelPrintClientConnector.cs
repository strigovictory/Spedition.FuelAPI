using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.RequestModels.Print;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Client.Connectors;

public class FuelPrintClientConnector : ConnectorBase, IFuelPrintClientConnector
{
    public FuelPrintClientConnector(IHttpClientService httpClientService, IFuelApiConfigurationProvider apiVersionProvider)
        : base(httpClientService, apiVersionProvider)
    {
    }

    protected override UriSegment UriSegment => UriSegment.fuelprint;

    public async Task<byte[]> PrintCards(List<CardPrintRequest> cards)
    {
        return await httpClientService?.SendRequestAsync<List<CardPrintRequest>, byte[]>(
            Uri + "/cards", HttpMethod.Post, cards);
    }

    public async Task<byte[]> PrintTransactions(List<TransactionPrintRequest> transactions)
    {
        return await httpClientService?.SendRequestAsync<List<TransactionPrintRequest>, byte[]>(
            Uri + "/transactions", HttpMethod.Post, transactions);
    }

    public async Task<byte[]> PrintNotFoundCards(List<CardNotFoundPrintRequest> cards)
    {
        return await httpClientService?.SendRequestAsync<List<CardNotFoundPrintRequest>, byte[]>(
            Uri + "/notfoundcards", HttpMethod.Post, cards);
    }
}
