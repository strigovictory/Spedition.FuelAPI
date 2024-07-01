namespace Spedition.Fuel.BusinessLayer.Models.E100;

public class E100Auth
{
    public string access_token { get; set; }

    public int expires_in { get; set; }

    public string token_type { get; set; }

    public object[] scope { get; set; }

    public string refresh_token { get; set; }

    public string user_id { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }

    public string code { get; set; }

    public string email { get; set; }

    public string tarif { get; set; }

    public string country { get; set; }

    public string fullname { get; set; }

    public int group_id { get; set; }

    public string defcur { get; set; }

    public int client_type { get; set; }

    public string REFNumber { get; set; }
}
