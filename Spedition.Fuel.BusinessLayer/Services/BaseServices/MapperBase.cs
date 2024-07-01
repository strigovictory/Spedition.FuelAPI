using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BFF.Constants;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;

namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public interface IMapperTestHelper<TSuccess, TNotSuccess, TSearch> : IGroupDoubleAction<TSuccess, TNotSuccess>
{
}

public abstract class MapperBase<TParsed, TSuccess, TNotSuccess, TSearch> 
    : SavingBase<TSuccess, TNotSuccess, TSearch>, IMapperTestHelper<TSuccess, TNotSuccess, TSearch>
    where TParsed : class, IParsedItem
{
    protected MapperBase(IWebHostEnvironment env, FuelRepositories fuelRepositories, IConfiguration configuration, IMapper mapper) 
        : base(env, fuelRepositories, configuration, mapper)
    {
    }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    protected string UserName { get; set; }

    /// <summary>
    /// Коллекция экземпляров из обработанного отчета.
    /// </summary>
    protected List<TParsed> ItemsToMapping { get; set; } = new();

    protected virtual async Task MappingParsedToDB()
    {
        try
        {
            ConcurrentBag<TSearch> result = new();
            ConcurrentBag<byte> counter = new();
            await InitDataForMapping();

            await Parallel.ForEachAsync(
                new ConcurrentBag<TParsed>(ItemsToMapping ?? new()),
                new ParallelOptions { MaxDegreeOfParallelism = ConstantsList.MaxDegreeOfParallelism },
                async (item, token) =>
                {
                    counter?.Add(0);
                    var entry = await MappingParsedToDB(item);
                    if (entry != null)
                    {
                        result.Add(entry);
                        await Task.CompletedTask;
                    }
                });

            if (counter?.Count != (ItemsToMapping?.Count ?? 0))
            {
                $"Цикл не был пройден до конца (из {ItemsToMapping?.Count() ?? 0} итераций было выполнено только {counter.Count})"
                    .LogError(GetType().Name, nameof(MappingParsedToDB));
            }

            ItemsToSaveInDB = new(result ?? new());

            // Сохранить не найденные в БД сущности
            if ((NotSuccessItems?.Count ?? 0) > 0)
            {
                await SaveNotFoundItemsInDB();
            }
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(MappingParsedToDB), GetType().FullName);
            throw;
        }
    }

    protected abstract Task<TSearch> MappingParsedToDB(IParsedItem item);

    protected abstract Task InitDataForMapping();

    /// <summary>
    /// Метод для сохранения не обнаруженных в процессе маппинга сущностей.
    /// </summary>
    /// <returns>Задача по сохранению в БД новых сущностей.</returns>
    protected abstract Task SaveNotFoundItemsInDB();
}
