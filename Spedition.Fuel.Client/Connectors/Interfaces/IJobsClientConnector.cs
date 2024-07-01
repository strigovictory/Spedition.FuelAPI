using Spedition.Fuel.Shared.DTO;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.Entities;
using Spedition.Fuel.Shared.Enums;

namespace Spedition.Fuel.Client.Connectors.Interfaces;

public interface IJobsClientConnector
{
    Task<ResponseGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> DoJob(ProvidersApiNames provider);

    Task<ResponseGroupActionDetailed<FuelTransactionShortResponse, NotParsedTransaction>> DoJob(ProvidersApiNames provider, int periodity);
}
