using Microsoft.AspNetCore.Mvc;
using Spedition.Fuel.BusinessLayer;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.FuelAPI.Controllers;

/// <summary>
/// Информация по поставщикам/провайдерам топлива.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/fuelproviders")]
public class FuelProvidersController : ControllerBase
{
    private readonly IFuelProviderService fuelProviderService;

    public FuelProvidersController(IFuelProviderService fuelProviderService)
        => this.fuelProviderService = fuelProviderService;

    /// <summary>
    /// Метод для получения коллекции провайдеров/поставщиков топл.продукции.
    /// </summary>
    /// <param Name="token">Токен отмены.</param>
    /// <returns>Коллекция провайдеров/поставщиков топл.продукции.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<FuelProviderResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<FuelProviderResponse>> GetProviders([FromHeader] CancellationToken token = default)
    {
        return await fuelProviderService?.GetProviders(token);
    }
}
