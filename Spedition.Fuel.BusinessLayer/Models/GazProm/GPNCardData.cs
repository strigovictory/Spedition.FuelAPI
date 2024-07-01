using Spedition.Fuel.BusinessLayer.Models.GazProm.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNCardData : GPNDataBase<GPNCard>, IGPNRequestBase
{
    public string Url => "cardsGpn";

    public string CountName => "";

    public string ContractId => "contract_id";

    public string CardName => "";
}
