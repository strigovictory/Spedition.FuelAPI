namespace Spedition.Fuel.BusinessLayer.Models.GPN;

public class GPNStatus<T>
{
    public int Code { get; set; }

    public ICollection<T> Errors { get; set; }
}
