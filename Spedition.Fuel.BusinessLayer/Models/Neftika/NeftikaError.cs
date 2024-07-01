using Spedition.Fuel.BusinessLayer.Models.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Models.Neftika;

public class NeftikaError
{
    public string error { get; set; }

    public string error_description { get; set; }

    public int responseCode { get; set; }

    public string message { get; set; }
}
