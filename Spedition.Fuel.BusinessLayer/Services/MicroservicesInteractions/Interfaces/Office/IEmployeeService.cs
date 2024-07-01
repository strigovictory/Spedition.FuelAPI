using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Interfaces.Office;

public interface IEmployeeService
{
    Task<List<EmployeeResponse>> GetEmployees();
}
