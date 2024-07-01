using System.Collections.Generic;
using Spedition.Fuel.BusinessLayer.Models.GazProm;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNTransactionComplex
{
    public string Id { get; set; }
    
    public DateTime Time { get; set; }
    
    public int Currency { get; set; }
    
    public string Card_id { get; set; }
    
    public string Service_center { get; set; }
    
    public bool Incoming { get; set; }
    
    public string Card_number { get; set; }
    
    public decimal Base_cost { get; set; }
    
    public decimal Cost { get; set; }
    
    public decimal Discount { get; set; }
    
    public decimal Discount_cost { get; set; }
    
    public GPNTransactionRequest Request { get; set; }
    
    public List<GPNTransactionItem> Transaction_items { get; set; }

    public List<GPNTransaction> MapTransaction()
    {
        List<GPNTransaction> result = new();
        foreach (var transaction in Transaction_items)
        {
            result.Add(
                new GPNTransaction
                {
                    Id = transaction.Id,
                    Time = Time,
                    Currency = transaction.Currency,
                    Card_id = Card_id,
                    Service_center = Service_center,
                    Incoming = Incoming,
                    Card_number = Card_number,
                    Cost = Cost,
                    Discount_cost = Discount_cost,
                    Product = transaction.Product,
                    Amount = transaction.Amount,
                    Price = transaction.Price,
                });
        }

        return result;
    }
}
