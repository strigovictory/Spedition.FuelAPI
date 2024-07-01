using Microsoft.AspNetCore.Mvc;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;

namespace Spedition.FuelAPI.Controllers;

/// <summary>
/// Информация по типам топливных продуктов.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/fueltypes")]
public class FuelTypesController : ControllerBase
{
    private readonly IFuelTypesService fuelTypesService;

    public FuelTypesController(IFuelTypesService fuelTypesService)
        => this.fuelTypesService = fuelTypesService;

    /// <summary>
    /// Метод для получения коллекции предоставляемых провайдерами разновидностей топливных услуг.
    /// </summary>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Коллекция предоставляемых провайдерами разновидностей топливных услуг.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<FuelTypeResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<FuelTypeResponse>> GetFuelTypes([FromHeader] CancellationToken token = default)
    {
        return await fuelTypesService.GetFuelTypes(token);
    }
}
