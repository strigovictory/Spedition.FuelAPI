using Spedition.Fuel.Shared.DTO.ResponseModels.OtherApiServicesModels.Office;

namespace Spedition.Fuel.BusinessLayer.Services.MicroservicesInteractions.Office;

public class EmployeeService: IEmployeeService
{
    private readonly IEmployeeRetriever retriever;

    public EmployeeService(IEmployeeRetriever retriever) => this.retriever = retriever;

    public async Task<List<EmployeeResponse>> GetEmployees()
    {
        return await retriever.GetEmployees();
    }
}
