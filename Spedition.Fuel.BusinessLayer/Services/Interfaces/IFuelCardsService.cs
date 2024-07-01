using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer
{
    public interface IFuelCardsService : IGroupAction<FuelCardShortResponse, FuelCardResponse>
    {
        #region GET
        Task<FuelCardResponse> GetCard(int cardId, CancellationToken token = default);

        Task<List<FuelCardFullResponse>> GetCards(CancellationToken token = default);

        Task<List<FuelCardNotFoundResponse>> GetNotFoundCards(CancellationToken token = default);

        Task<List<FuelCardsAlternativeNumberResponse>> GetCardsAlternativeNumbers(int cardId, CancellationToken token = default);
        #endregion

        #region PUT
        Task UpdateCard(FuelCardRequest card, string user, CancellationToken token = default);

        Task MoveCardsToArchive(List<FuelCardRequest> cards, string user, CancellationToken token = default);

        Task<FuelCardsAlternativeNumberResponse> UpdateCardsAlternativeNumber(
            FuelCardsAlternativeNumberRequest alternativeNumber, CancellationToken token = default);
        #endregion

        #region POST
        Task CreateCards(List<FuelCardRequest> cards, string user, CancellationToken token = default);

        Task<FuelCardsAlternativeNumberResponse> CreateCardsAlternativeNumber(
            FuelCardsAlternativeNumberRequest alternativeNumber, CancellationToken token = default);
        #endregion

        #region DELETE
        Task<bool> DeleteCards(List<int> cardsIds, CancellationToken token = default);

        bool DeleteCardsAlternativeNumbers(List<int> alternativeNumbersIds);

        bool DeleteNotFoundCards(List<int> notFoundCardsIds, CancellationToken token = default);
        #endregion
    }
}
