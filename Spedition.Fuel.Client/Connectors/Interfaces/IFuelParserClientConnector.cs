using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.Client.Connectors.Interfaces;

public interface IFuelParserClientConnector
{
    Task<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> ParseReport(
            FuelReport report, CancellationToken token = default);

    Task<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> PostFilledTransactions(
            NotParsedTransactionsFilled filledTransactions);
}
