using Spedition.Fuel.Shared.Enums;

namespace Spedition.Fuel.Client.Helpers
{
    public abstract class RoutesBase
    {
        private readonly string apiVersion;

        public RoutesBase(IFuelApiConfigurationProvider apiConfigProvider)
        {
            apiVersion = apiConfigProvider.GetApiVersion();
        }

        protected string Uri => $"/api/{apiVersion}/{UriSegment.ToString()}";

        protected abstract UriSegment UriSegment { get; }
    }
}
