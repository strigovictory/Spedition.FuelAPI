using Spedition.Fuel.BusinessLayer.Models.GazProm.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNAzsData : GPNDataBase<GPNAzs>, IGPNRequestBase
{
    public string Url => "AZS";
    
    public string CountName => "onpage";
    
    public string ContractId => "";
    
    public string CardName => "";
}
