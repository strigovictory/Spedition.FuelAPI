namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNAzs
{
    public string Id { get; set; }
    
    public int? Status { get; set; }
    
    public string CountryCode { get; set; }
    
    public string Type { get; set; }
    
    public List<GPNPriceItem> Prices { get; set; }
}
