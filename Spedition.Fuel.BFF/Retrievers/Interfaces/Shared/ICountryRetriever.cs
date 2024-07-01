using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Geo;

namespace Spedition.Fuel.BFF.Retrievers.Interfaces.Shared
{
    public interface ICountryRetriever
    {
        Task<List<CountryResponse>> Get(CancellationToken token = default);
    }
}
