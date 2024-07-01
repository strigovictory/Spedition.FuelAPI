namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNAuthData
{
    public string Client_id { get; set; }
    
    public string Session_id { get; set; }
    
    public List<GPNContract> Contracts { get; set; }
}
