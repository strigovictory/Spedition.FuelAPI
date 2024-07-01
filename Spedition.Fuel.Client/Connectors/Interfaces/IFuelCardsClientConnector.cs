using Spedition.Fuel.Shared.DTO;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;

namespace Spedition.Fuel.Client.Connectors.Interfaces
{
    public interface IFuelCardsClientConnector
    {
        Task<List<FuelCardFullResponse>> GetFuelCards(CancellationToken token = default);

        Task<FuelCardResponse> GetFuelCard(int cardId, CancellationToken token = default);

        Task<List<FuelCardNotFoundResponse>> GetFuelCardsNotFound(CancellationToken token = default);

        Task<List<FuelCardsAlternativeNumberResponse>> GetFuelCardsAlternativeNumbers(int cardId, CancellationToken token = default);

        Task<ResponseSingleAction<FuelCardShortResponse>> PutCard(RequestSingleAction<FuelCardRequest> card, CancellationToken token = default);

        Task<ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>> PutCards(
            RequestGroupAction<FuelCardRequest> cards, CancellationToken token = default);

        Task<ResponseSingleAction<FuelCardsAlternativeNumberResponse>> PutCardsAlternativeNumber(
            FuelCardsAlternativeNumberRequest alternativeNumber, CancellationToken token = default);

        Task<ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>> PostCards(
            RequestGroupAction<FuelCardRequest> cards, CancellationToken token = default);

        Task<ResponseSingleAction<FuelCardsAlternativeNumberResponse>> PostCardsAlternativeNumber(
            FuelCardsAlternativeNumberRequest alternativeNumber, CancellationToken token = default);

        Task<ResponseBase> DeleteCards(List<int> cardsIds, CancellationToken token = default);

        Task<ResponseBase> DeleteCardsAlternativeNumber(int alternativeNumbersId, CancellationToken token = default);

        Task<ResponseBase> DeleteNotFoundCards(List<int> notFoundCardsIds, CancellationToken token = default);
    }
}
