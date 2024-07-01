using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.BusinessLayer
{
    public interface IFuelTransactionService : IGroupAction<FuelTransactionShortResponse, FuelTransactionShortResponse>
    {
        #region GET
        long GetCount(CancellationToken token = default);

        Task<List<FuelTransactionFullResponse>> GetTransactions(CancellationToken token = default, int? take = null, int? skip = null);

        Task<FuelTransactionResponse> GetTransaction(int id, CancellationToken token = default);
        #endregion

        #region PUT
        Task<FuelTransactionShortResponse> UpdateTransaction(FuelTransactionRequest transaction);
        #endregion

        #region POST
        Task CreateTransactions(List<FuelTransactionRequest> transactions);
        #endregion

        #region DELETE
        bool DeleteTransactions(List<int> transactionsIds);

        Task<(int success, int notSuccess)> DeleteTransactionsDuplicates(int? fuelProvId, int? month = null, int? year = null, CancellationToken token = default);
        #endregion
    }
}
