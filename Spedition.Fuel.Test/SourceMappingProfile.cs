using AutoMapper;
using Spedition.Fuel.Shared.DTO.RequestModels.FuelModels;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spedition.Fuel.Test;

public class SourceMappingProfile : Profile
{
    public SourceMappingProfile()
    {
        CreateMap<FuelTransaction, NotParsedTransaction>().ReverseMap();
        CreateMap<FuelTransaction, FuelTransactionShortResponse>().ReverseMap(); 
        CreateMap<FuelTransaction, FuelTransactionResponse>().ReverseMap();
        CreateMap<FuelTransaction, FuelTransactionFullResponse>().ReverseMap();
        CreateMap<FuelTransaction, FuelTransactionRequest>().ReverseMap(); 
        CreateMap<FuelTransactionFullResponse, FuelTransactionRequest>().ReverseMap();
        CreateMap<FuelTransactionRequest, FuelTransactionShortResponse>().ReverseMap();
        CreateMap<FuelTransactionResponse, FuelTransactionShortResponse>().ReverseMap();
        //
        CreateMap<FuelType, FuelTypeResponse>().ReverseMap();
        CreateMap<FuelCard, FuelCardResponse>().ReverseMap();
        CreateMap<FuelCard, FuelCardFullResponse>().ReverseMap();
        CreateMap<FuelCard, FuelCardRequest>().ReverseMap();
        CreateMap<FuelCard, FuelCardShortResponse>().ReverseMap();
        CreateMap<FuelCardShortResponse, FuelCardFullResponse>().ReverseMap();
        CreateMap<FuelCardShortResponse, FuelCardRequest>().ReverseMap();
        CreateMap<FuelCardRequest, FuelCardFullResponse>().ReverseMap(); 
        //
        CreateMap<FuelProvider, FuelProviderResponse>().ReverseMap();
        CreateMap<FuelCardsAlternativeNumber, FuelCardsAlternativeNumberRequest>().ReverseMap();
        CreateMap<FuelCardsAlternativeNumber, FuelCardsAlternativeNumberResponse>().ReverseMap();
        CreateMap<FuelCardsAlternativeNumberRequest, FuelCardsAlternativeNumberResponse>().ReverseMap();
        CreateMap<FuelCardsEvent, FuelCardsEventResponse>().ReverseMap();
    }
}
