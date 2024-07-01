using System.Collections.Generic;

namespace Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;

public class NotParsedTransactionsFilled : FuelParameters
{
    public NotParsedTransactionsFilled(int providerId, string userName, List<NotParsedTransactionFilled> transactions)
        : base(providerId, userName)
    {
        Transactions = transactions;
    }

    [JsonInclude]
    public List<NotParsedTransactionFilled> Transactions{ get; set; }
}
