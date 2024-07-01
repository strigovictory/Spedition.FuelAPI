using Spedition.Fuel.BusinessLayer.Models.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Models.GazProm;

public class GPNTransaction : IParsedItem
{
    public string Id { get; set; }

    public DateTime Time { get; set; }

    public int Currency { get; set; }

    public string Card_id { get; set; }

    public string Service_center { get; set; }

    public bool Incoming { get; set; }

    public string Card_number { get; set; }

    public decimal Cost { get; set; }

    public decimal Discount_cost { get; set; }

    public string Product { get; set; }

    public decimal Amount { get; set; }

    public decimal Price { get; set; }
}
