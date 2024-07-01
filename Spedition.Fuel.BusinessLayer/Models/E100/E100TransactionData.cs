using System.Text.Json.Serialization;

namespace Spedition.Fuel.BusinessLayer.Models.E100;

public class E100TransactionData
{
    public int Page_number { get; set; }

    public int Page_count { get; set; }

    public int Page_size { get; set; }

    public int Row_count { get; set; }

    public List<E100Transaction> Transactions { get; set; }
}
