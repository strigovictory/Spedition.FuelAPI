using Spedition.Fuel.BusinessLayer;
using Spedition.Fuel.BusinessLayer.Models;
using Spedition.Fuel.BusinessLayer.Models.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.BaseServices;
using Spedition.Fuel.BusinessLayer.Services.Interfaces;
using Spedition.Fuel.BusinessLayer.Services.Parsers;
using Spedition.Fuel.BusinessLayer.Services.Parsers.ParserBaseServices;
using Spedition.Fuel.Shared.DTO.ResponseModels.FuelModels;
using Spedition.Fuel.Shared.Entities;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Spedition.Fuel.Test.Parsers;

public abstract class ParsersTestHelper : TestsHelper
{
    protected ParsersTestHelper(ITestOutputHelper output) : base(output)
    {
    }

    protected async Task MappingParsedToDBHelper(
        IMapperTestHelper<FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction> parserItem,
        object parsedReportsItemExistedCard,
        object parsedReportsItemNotExistedCard)
    {
        var taskInitDataForMapping = (Task)parserItem.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)?
            .Where(mi => mi.Name.Equals("InitDataForMapping"))?
            .FirstOrDefault(mi => mi.GetParameters().Count() == 0)?
            .Invoke(parserItem, new object[] { });
        taskInitDataForMapping.GetAwaiter().GetResult();

        // Action
        var methodToInvoke = parserItem.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)?
            .Where(mi => mi.Name.Equals("MappingParsedToDB"))?
            .FirstOrDefault(mi => mi.GetParameters().Count() == 1);

        // Assert
        var mappingResult = AssertMappingActionResultExisted(parserItem, () => methodToInvoke.Invoke(parserItem, new object[] { parsedReportsItemExistedCard }) as Task<FuelTransaction>);
        Output.WriteLine($"Mapped transaction: {mappingResult.ToString()}");

        AssertMappingActionResultNotExisted(parserItem, () => methodToInvoke.Invoke(parserItem, new object[] { parsedReportsItemNotExistedCard }) as Task<FuelTransaction>);
    }

    public FuelTransaction AssertMappingActionResultExisted(
        IMapperTestHelper<FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction> parser, Func<Task<FuelTransaction>> action)
    {
        var actionResultExisted = action().GetAwaiter().GetResult();
        Assert.NotNull(actionResultExisted);
        Assert.IsAssignableFrom<FuelTransaction>(actionResultExisted);
        Assert.True((parser?.NotSuccessItems?.Count ?? 0) == 0);
        return actionResultExisted;
    }

    public FuelTransaction AssertMappingActionResultNotExisted(
        IMapperTestHelper<FuelTransactionShortResponse, NotParsedTransaction, FuelTransaction> parser, Func<Task<FuelTransaction>> action)
    {
        var actionResultNotExisted = action().GetAwaiter().GetResult();
        Assert.True((parser?.NotSuccessItems?.Count ?? 0) == 1);
        return actionResultNotExisted;
    }
}
