namespace Spedition.Fuel.BusinessLayer.Services.Interfaces;

public interface IJobBaseService<TSuccess, TNotSuccess> : IGroupDoubleAction<TSuccess, TNotSuccess>
{
    Task Do();
}
