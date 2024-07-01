using Spedition.Fuel.Client.Connectors.Interfaces;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Enums;

namespace Spedition.Fuel.Client.Connectors
{
    public class FuelTransactionsClientConnector : ConnectorBase, IFuelTransactionsClientConnector
    {
        public FuelTransactionsClientConnector(IHttpClientService httpClientService, IFuelApiConfigurationProvider apiVersionProvider)
        : base(httpClientService, apiVersionProvider)
        {
        }

        protected override UriSegment UriSegment => UriSegment.fueltransactions;

        public async Task<long> GetCount(CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<long>(
                Uri + "/count", HttpMethod.Get, token);
        }

        public async Task<List<FuelTransactionFullResponse>> GetTransactions(int? toTake, int? toSkip, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<List<FuelTransactionFullResponse>>(
                Uri + $"?toTake={toTake ?? 0}&toSkip={toSkip ?? 0}", HttpMethod.Get, token);
        }

        public async Task<FuelTransactionResponse> GetTransaction(int transactionsId, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<FuelTransactionResponse>(
                Uri + $"/{transactionsId}", HttpMethod.Get, token);
        }

        public async Task<ResponseSingleAction<FuelTransactionShortResponse>> PutTransaction(FuelTransactionRequest transaction, CancellationToken token = default)
        {
            return await httpClientService?
                .SendRequestAsync<FuelTransactionRequest, ResponseSingleAction<FuelTransactionShortResponse>>(
                    Uri, HttpMethod.Put, transaction, token);
        }

        public async Task<ResponseGroupActionDetailed<FuelTransactionShortResponse, FuelTransactionShortResponse>> PostTransactions(
            IEnumerable<FuelTransactionRequest> transactions, CancellationToken token = default)
        {
            return await httpClientService?
                .SendRequestAsync<IEnumerable<FuelTransactionRequest>, ResponseGroupActionDetailed<FuelTransactionShortResponse, FuelTransactionShortResponse>>(
                    Uri, HttpMethod.Post, transactions, token);
        }

        public async Task<ResponseBase> DeleteTransactions(List<int> transactionsIds, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<List<int>, ResponseBase>(
                Uri + "/delete", HttpMethod.Post, transactionsIds, token);
        }

        public async Task<ResponseBase> DeleteTransaction(int transactionsId, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<ResponseBase>(
                Uri + $"/{transactionsId}", HttpMethod.Delete, token);
        }

        public async Task<ResponseBase> DeleteTransactionsDuplicates(int? fuelProvId, int? month = null, int? year = null, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<ResponseBase>(
                Uri + $"/duplicates?{nameof(fuelProvId)}={fuelProvId}&{nameof(month)}={month}&{nameof(year)}={year}", HttpMethod.Delete, token);
        }
    }
}
