namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNDataBase<T>
{
    public int Total_count { get; set; }
    
    public List<T> Result { get; set; }
}
