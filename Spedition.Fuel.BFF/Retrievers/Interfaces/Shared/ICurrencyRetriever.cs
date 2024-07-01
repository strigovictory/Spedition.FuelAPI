using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Finance;

namespace Spedition.Fuel.BFF.Retrievers.Interfaces.Shared
{
    public interface ICurrencyRetriever
    {
        Task<List<CurrencyResponse>> Get(CancellationToken token = default);
    }
}
