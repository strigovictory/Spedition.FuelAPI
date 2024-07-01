using Spedition.Fuel.Shared.DTO.RequestModels.Print;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.Fuel.Client.Connectors.Interfaces;

public interface IFuelPrintClientConnector
{
    Task<byte[]> PrintTransactions(List<TransactionPrintRequest> transactions);

    Task<byte[]> PrintCards(List<CardPrintRequest> cards);

    Task<byte[]> PrintNotFoundCards(List<CardNotFoundPrintRequest> cards);
}
