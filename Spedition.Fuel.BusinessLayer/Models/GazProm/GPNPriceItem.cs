namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNPriceItem
{
    public int Id { get; set; }
    
    public string GoodsCode { get; set; }
    
    public decimal Price { get; set; }
    
    public string Currency { get; set; }
}
