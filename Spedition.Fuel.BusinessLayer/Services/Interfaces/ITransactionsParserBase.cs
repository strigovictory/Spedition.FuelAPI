using Microsoft.AspNetCore.Http;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;

namespace Spedition.Fuel.BusinessLayer.Services.Interfaces;

public interface ITransactionsParserBase<TSuccess, TNotSuccess> : IGroupDoubleAction<TSuccess, TNotSuccess>
{
    public int ProviderId { get; set; }

    public bool? IsMonthly { get; set; }

    /// <summary>
    /// Коллекция идентификаторов провайдеров топлива, отчеты которых могут быть обработаны парсером.
    /// </summary>
    List<int> ProvidersId { get; }

    Task ParseFile(UploadedContent content, CancellationToken token = default);

    Task SaveFilledTransactions(List<NotParsedTransactionFilled> transactions);
}
