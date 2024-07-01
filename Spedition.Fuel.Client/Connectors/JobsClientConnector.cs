using System;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Enums;

namespace Spedition.Fuel.Client.Connectors;

public class JobsClientConnector : ConnectorBase, IJobsClientConnector
{
    public JobsClientConnector(IHttpClientService httpClientService, IFuelApiConfigurationProvider apiVersionProvider)
        : base(httpClientService, apiVersionProvider)
    {
    }

    protected override UriSegment UriSegment => UriSegment.fueljobs;

    public async Task<ResponseGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> DoJob(ProvidersApiNames provider)
    {
        return await httpClientService?.SendRequestAsync<ResponseGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>>(
            Uri + $"/{provider.ToString()}", HttpMethod.Get);
    }

    public async Task<ResponseGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> DoJob(ProvidersApiNames provider, int periodity)
    {
        return await httpClientService?.SendRequestAsync<ResponseGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>>(
            Uri + $"/{provider.ToString()}/{periodity}", HttpMethod.Get);
    }
}
