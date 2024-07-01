namespace Spedition.Fuel.BusinessLayer.Models.Neftika;

public class NeftikaAuth : NeftikaError
{
    public string access_token { get; set; }

    public string token_type { get; set; } // "bearer"

    public int expires_in { get; set; } // 86399

    public string userName { get; set; } // "gml0500"

    public string language { get; set; } //"ru"

    public DateTime issued { get; set; } // "2022-05-12T10:15:58.000"

    public DateTime expires { get; set; } // "2022-05-13T10:15:58.000"

    public override string ToString()
    {
        return $"token: «{token_type} {access_token}», " +
               $"user: «{userName}», " +
               $"expired {expires}. ";
    }
}
