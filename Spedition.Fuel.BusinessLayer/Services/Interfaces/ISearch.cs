namespace Spedition.Fuel.BusinessLayer
{
    public interface ISearch<T>
    {
        /// <summary>
        /// Колллекция найденных экземпляров.
        /// </summary>
        List<T> ExistedInstances { get; }
    }
}
