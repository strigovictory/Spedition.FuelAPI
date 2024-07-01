using Spedition.Fuel.Client.Connectors.Interfaces;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Enums;

namespace Spedition.Fuel.Client.Connectors
{
    public class FuelCardsClientConnector : ConnectorBase, IFuelCardsClientConnector
    {
        public FuelCardsClientConnector(IHttpClientService httpClientService, IFuelApiConfigurationProvider apiVersionProvider)
            : base(httpClientService, apiVersionProvider)
        {
        }

        protected override UriSegment UriSegment => UriSegment.fuelcards;

        #region Get

        public async Task<List<FuelCardFullResponse>> GetFuelCards(CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<List<FuelCardFullResponse>>(Uri, HttpMethod.Get, token);
        }

        public async Task<FuelCardResponse> GetFuelCard(int cardId, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<FuelCardResponse>(
                Uri + $"/{cardId}", HttpMethod.Get, token);
        }

        public async Task<List<FuelCardNotFoundResponse>> GetFuelCardsNotFound(CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<List<FuelCardNotFoundResponse>>(
                Uri + "/notfoundcards", HttpMethod.Get, token);
        }

        public async Task<List<FuelCardsAlternativeNumberResponse>> GetFuelCardsAlternativeNumbers(int cardId, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<List<FuelCardsAlternativeNumberResponse>>(
                Uri + $"/alternativenumbers/{cardId}", HttpMethod.Get, token);
        }
        #endregion

        #region Put
        public async Task<ResponseSingleAction<FuelCardShortResponse>> PutCard(
            RequestSingleAction<FuelCardRequest> card, CancellationToken token = default)
        {
            return await httpClientService?
                .SendRequestAsync<RequestSingleAction<FuelCardRequest>, ResponseSingleAction<FuelCardShortResponse>>(
                    Uri, HttpMethod.Put, card, token);
        }

        public async Task<ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>> PutCards(
            RequestGroupAction<FuelCardRequest> cards, CancellationToken token = default)
        {
            return await httpClientService?
                .SendRequestAsync<RequestGroupAction<FuelCardRequest>, ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>>(
                    Uri + "/cards", HttpMethod.Put, cards, token);
        }

        public async Task<ResponseSingleAction<FuelCardsAlternativeNumberResponse>> PutCardsAlternativeNumber(
            FuelCardsAlternativeNumberRequest alternativeNumber, CancellationToken token = default)
        {
            return await httpClientService?
                .SendRequestAsync<FuelCardsAlternativeNumberRequest, ResponseSingleAction<FuelCardsAlternativeNumberResponse>>(
                    Uri + "/alternativenumber", HttpMethod.Put, alternativeNumber, token);
        }
        #endregion

        #region Post
        public async Task<ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>> PostCards(
            RequestGroupAction<FuelCardRequest> cards, CancellationToken token = default)
        {
            return await httpClientService?
                .SendRequestAsync<RequestGroupAction<FuelCardRequest>, ResponseGroupActionDetailed<FuelCardShortResponse, FuelCardResponse>>(
                    Uri, HttpMethod.Post, cards, token);
        }

        public async Task<ResponseSingleAction<FuelCardsAlternativeNumberResponse>> PostCardsAlternativeNumber(
            FuelCardsAlternativeNumberRequest alternativeNumber, CancellationToken token = default)
        {
            return await httpClientService?
                .SendRequestAsync<FuelCardsAlternativeNumberRequest, ResponseSingleAction<FuelCardsAlternativeNumberResponse>>(
                    Uri + "/alternativenumber", HttpMethod.Post, alternativeNumber, token);
        }
        #endregion

        #region Delete
        public async Task<ResponseBase> DeleteCards(List<int> cardsIds, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<List<int>, ResponseBase>(
                Uri + "/delete", HttpMethod.Post, cardsIds, token);
        }

        public async Task<ResponseBase> DeleteCardsAlternativeNumber(int alternativeNumbersId, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<ResponseBase>(
                Uri + $"/alternativenumbers/{alternativeNumbersId}", HttpMethod.Delete, token);
        }

        public async Task<ResponseBase> DeleteNotFoundCards(List<int> notFoundCardsIds, CancellationToken token = default)
        {
            return await httpClientService?.SendRequestAsync<List<int>, ResponseBase>(
                Uri + $"/notfoundcards/delete", HttpMethod.Post, notFoundCardsIds, token);
        }
        #endregion
    }
}
