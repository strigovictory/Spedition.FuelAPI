namespace Spedition.Fuel.BusinessLayer.Models.GazProm.Interfaces
{
    public interface IGPNRequestBase
    {
        string Url { get; }
        
        string CountName { get; }
        
        string CardName { get; }
        
        string ContractId { get; }
    }
}
