using Spedition.Fuel.Shared.DTO;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;

namespace Spedition.Fuel.Client.Connectors.Interfaces
{
    public interface IFuelTransactionsClientConnector
    {
        Task<long> GetCount(CancellationToken token = default);

        Task<List<FuelTransactionFullResponse>> GetTransactions(int? toTake, int? toSkip, CancellationToken token = default);

        Task<FuelTransactionResponse> GetTransaction(int transactionsId, CancellationToken token = default);

        Task<ResponseSingleAction<FuelTransactionShortResponse>> PutTransaction(FuelTransactionRequest transaction, CancellationToken token = default);

        Task<ResponseGroupActionDetailed<FuelTransactionShortResponse, FuelTransactionShortResponse>> PostTransactions(
            IEnumerable<FuelTransactionRequest> transactions, CancellationToken token = default);

        Task<ResponseBase> DeleteTransactions(List<int> transactionsIds, CancellationToken token = default);

        Task<ResponseBase> DeleteTransaction(int transactionsId, CancellationToken token = default);

        Task<ResponseBase> DeleteTransactionsDuplicates(int? fuelProvId, int? month = null, int? year = null, CancellationToken token = default);
    }
}
