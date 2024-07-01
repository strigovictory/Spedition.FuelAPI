using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;

namespace Spedition.Fuel.BFF.Retrievers.Interfaces.Office;

public interface IEmployeeRetriever
{
    public Task<List<EmployeeResponse>> GetEmployees();
}
