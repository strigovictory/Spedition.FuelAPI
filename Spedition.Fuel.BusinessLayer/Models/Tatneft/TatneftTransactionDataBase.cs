namespace Spedition.Fuel.BusinessLayer.Models.Tatneft;

public abstract class TatneftTransactionDataBase
{
    public int total { get; set; } // "3"

    public int limit { get; set; } // 100

    public int offset { get; set; } // 0
}