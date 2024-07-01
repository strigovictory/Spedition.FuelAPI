using Microsoft.AspNetCore.Mvc;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.Shared.DTO.RequestModels.UploadedReports;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Helpers;

namespace Spedition.FuelAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/fueljobs")]
public class FuelJobsController : ControllerBase
{
    private IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction> providersApiService;

    private readonly List<IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>> providersApiServices;

    public FuelJobsController(IEnumerable<IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>> providersApiServices)
    {
        this.providersApiServices = providersApiServices?.ToList() ?? new();
    }

    /// <summary>
    /// Метод для запуска топливной работы по получению и обработке данных от провайдера топлива.
    /// </summary>
    /// <param name="provider">Провайдер топлива.</param>
    /// <returns>Детализованный результатвыполнения работы.</returns>
    [HttpGet("{provider}")]
    [ProducesResponseType(typeof(ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> DoJob(
        [FromRoute] string provider)
    {
        try
        {
            throw new Exception($"Test exception!");

            if (string.IsNullOrEmpty(provider) || string.IsNullOrWhiteSpace(provider))
            {
                return new ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>("Ошибка! Пустое наименование провайдера топлива!");
            }

            // Выбрать конкретную реализацию интерфейса
            providersApiService = ChoiceProvidersApiService(provider);
            if (providersApiService == null || providersApiService == default)
            {
                throw new NotImplementedException($"Для провайдера топлива {provider ?? string.Empty} сервис не реализован!");
            }

            await providersApiService.Do();

            return new ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>(
                result: providersApiService.NotifyMessage ?? string.Empty,
                successItems: providersApiService.SuccessItems ?? new(),
                secondarySuccessItems: providersApiService.SecondarySuccessItems ?? new(),
                notSuccessItems: providersApiService.NotSuccessItems ?? new());
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(DoJob), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Метод для запуска топливной работы по получению и обработке данных от провайдера топлива.
    /// </summary>
    /// <param name="provider">Провайдер топлива.</param>
    /// <param name="periodity">Период, за который нужно обработать данные.</param>
    /// <returns>Детализованный результатвыполнения работы.</returns>
    [HttpGet("{provider}/{periodity:int}")]
    [ProducesResponseType(typeof(ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> DoJob(
        [FromRoute] string provider, [FromRoute] int periodity)
    {
        try
        {
            if (string.IsNullOrEmpty(provider) || string.IsNullOrWhiteSpace(provider))
            {
                return new ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>("Ошибка! Пустое наименование провайдера топлива!");
            }

            // Выбрать конкретную реализацию интерфейса
            providersApiService = ChoiceProvidersApiService(provider);
            if (providersApiService == null || providersApiService == default || providersApiService is not IPeriodic periodicProvidersApiService)
            {
                return new ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>(
                    $"Для провайдера топлива {provider ?? string.Empty} сервис не реализован!");
            }

            periodicProvidersApiService.Periodicity = periodity;
            await (periodicProvidersApiService as IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>)?.Do();

            return new ResponseDoubleGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>(
                result: providersApiService?.NotifyMessage ?? string.Empty,
                successItems: providersApiService?.SuccessItems ?? new(), 
                secondarySuccessItems: providersApiService.SecondarySuccessItems ?? new(),
                notSuccessItems: providersApiService?.NotSuccessItems ?? new());
        }
        catch (Exception exc)
        {
            exc.LogError(nameof(DoJob), GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Вспомогательный метод для определения конкретной реализации job-сервиса.
    /// </summary>
    /// <param name="providerFromRoute">Наименование провайдера топлива из маршрута запроса.</param>
    /// <returns>Интерфейсная ссылка на конкретную реализацию job-сервиса</returns>
    private IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction> ChoiceProvidersApiService(string providerFromRoute)
    {
        Func<string, IProvidersApiBase<FuelTransactionShortResponse, NotParsedTransaction>> getProvidersApiService = (string providerFromRoute) =>
        providersApiServices?.FirstOrDefault(
            provider => !string.IsNullOrEmpty(provider.ProviderName.Trim())
            && provider.ProviderName.Trim().Equals(providerFromRoute.Trim(), StringComparison.InvariantCultureIgnoreCase));

        return getProvidersApiService.Invoke(providerFromRoute);
    }
}
