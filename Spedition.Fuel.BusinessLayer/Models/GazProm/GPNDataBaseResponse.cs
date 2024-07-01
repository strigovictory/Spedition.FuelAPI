using Spedition.Fuel.BusinessLayer.Models.GPN;

namespace Spedition.Fuel.BusinessLayer.Models.GazProm;

public class GPNDataBaseResponse<T>
{
    public GPNStatus<T> Status { get; set; }
    
    public T Data { get; set; }
}
