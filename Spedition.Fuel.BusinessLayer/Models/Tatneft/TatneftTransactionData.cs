using Org.BouncyCastle.Asn1.X509;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Models.Tatneft;

public class TatneftTransactionData : TatneftTransactionDataBase
{
    public List<TatneftTransaction> response { get; set; }
}