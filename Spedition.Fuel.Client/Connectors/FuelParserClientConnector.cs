using Spedition.Fuel.Client.Connectors.Interfaces;
using Spedition.Fuel.Client.Helpers;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Enums;

namespace Spedition.Fuel.Client.Connectors
{
    public class FuelParserClientConnector : ConnectorBase, IFuelParserClientConnector
    {
        public FuelParserClientConnector(IHttpClientService httpClientService, IFuelApiConfigurationProvider apiVersionProvider)
            : base(httpClientService, apiVersionProvider)
        {
        }

        protected override UriSegment UriSegment => UriSegment.fuelparser;

        #region Post
        public async Task<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> ParseReport(
            FuelReport report, CancellationToken token = default)
        {
            return await httpClientService?
                .SendRequestAsync<FuelReport, ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>>(
                    Uri, HttpMethod.Post, report, token);
        }

        public async Task<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> PostFilledTransactions(
            NotParsedTransactionsFilled filledTransactions)
        {
            return await httpClientService?
                .SendRequestAsync<NotParsedTransactionsFilled, ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>>(
                    Uri + "/filled", HttpMethod.Post, filledTransactions);
        }
        #endregion
    }
}
