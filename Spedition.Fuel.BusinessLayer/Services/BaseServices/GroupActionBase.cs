using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;

namespace Spedition.Fuel.BusinessLayer.Services.BaseServices;

public abstract class GroupActionBase<TSuccess, TNotSuccess, TSearch> : SearchBase<TSearch>, IGroupAction<TSuccess, TNotSuccess>
{
    protected readonly IMapper mapper;

    public GroupActionBase(
        IWebHostEnvironment env,
        IConfiguration configuration,
        IMapper mapper)
        : base(configuration, env)
    {
        this.mapper = mapper;
    }

    public List<TSuccess> SuccessItems { get; protected set; } = new();

    protected List<NotSuccessResponseItemDetailed<TNotSuccess>> ErrorItems { get; set; } = new();

    protected List<NotSuccessResponseItemDetailed<TNotSuccess>> notSuccessItems = new();

    public List<NotSuccessResponseItemDetailed<TNotSuccess>> NotSuccessItems => GetNotSuccessItems();

    protected virtual List<NotSuccessResponseItemDetailed<TNotSuccess>> GetNotSuccessItems()
    {
        List<NotSuccessResponseItemDetailed<TNotSuccess>> result = new(notSuccessItems);

        // добавить неуникальные
        ExistedInstances?.ForEach(existedItem =>
        result?.Add(new NotSuccessResponseItemDetailed<TNotSuccess>(
            mapper.Map<TNotSuccess>(existedItem),
            $"{existedItem?.ToString() ?? string.Empty} уже существует в БД")));

        // добавить с ошибкой
        result?.AddRange(ErrorItems);

        return result;
    }
}