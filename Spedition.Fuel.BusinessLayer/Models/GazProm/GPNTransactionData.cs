using Spedition.Fuel.BusinessLayer.Models.GazProm.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNTransactionData : GPNDataBase<GPNTransactionComplex>, IGPNRequestBase
{
    public string Url => "transactions";
    
    public string CountName => "count";
    
    public string ContractId => "contract_id";
    
    public string CardName => "card_id";
}
