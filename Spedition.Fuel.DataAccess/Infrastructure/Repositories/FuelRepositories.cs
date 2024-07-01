using Spedition.Fuel.Shared.Entities;

namespace Spedition.Fuel.DataAccess.Infrastructure.Repositories
{
    public class FuelRepositories
    {
        private readonly IServiceProvider serviceProvider;

        public FuelRepositories(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IRepository<FuelCard> Cards => serviceProvider.GetService<IRepository<FuelCard>>();

        public IRepository<FuelTransaction> Transactions => serviceProvider.GetService<IRepository<FuelTransaction>>();

        public IRepository<BPTransaction> BPTransactions => serviceProvider.GetService<IRepository<BPTransaction>>();

        public IRepository<FuelProvider> Providers => serviceProvider.GetService<IRepository<FuelProvider>>();

        public IRepository<FuelType> FuelTypes => serviceProvider.GetService<IRepository<FuelType>>();

        public IRepository<NotFoundFuelCard> NotFoundCards => serviceProvider.GetService<IRepository<NotFoundFuelCard>>();

        public IRepository<FuelCardsAlternativeNumber> CardsAlternativeNumbers => serviceProvider.GetService<IRepository<FuelCardsAlternativeNumber>>();

        public IRepository<FuelCardsEvent> CardsEvents => serviceProvider.GetService<IRepository<FuelCardsEvent>>();

        public IRepository<FuelCardsCountry> FuelCardsCountries => serviceProvider.GetService<IRepository<FuelCardsCountry>>();

        public IRepository<ProvidersAccount> ProvidersAccounts => serviceProvider.GetService<IRepository<ProvidersAccount>>();
    }
}
