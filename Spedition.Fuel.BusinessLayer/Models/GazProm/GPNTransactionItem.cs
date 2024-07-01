namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNTransactionItem
{
    public string Id { get; set; }
    
    public string Product { get; set; }
    
    public decimal Amount { get; set; }
    
    public decimal Price { get; set; }
    
    public decimal Base_cost { get; set; }
    
    public decimal Cost { get; set; }
    
    public decimal Discount { get; set; }
    
    public decimal Discount_cost { get; set; }
    
    public int Currency { get; set; }
    
    public string Unit { get; set; }
}
