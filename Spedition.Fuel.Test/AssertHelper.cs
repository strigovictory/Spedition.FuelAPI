using Xunit.Abstractions;
using Xunit;
using Spedition.Fuel.Shared.DTO.ResponseModels.Generic;
using Spedition.Fuel.Shared.DTO.ResponseModels;
using System.Collections.Generic;

namespace Spedition.Fuel.Test;

public abstract class AssertHelper : TestsHelper
{
    protected AssertHelper(ITestOutputHelper output)
        : base(output)
    {
    }

    protected void AssertAction<T>(List<T> actionResult) where T : class
    {
        Assert.NotNull(actionResult);

        Output.WriteLine($"GetDivisions {actionResult?.Count ?? 0} items: ");
        actionResult?.ForEach(item => Output.WriteLine(item?.ToString() ?? string.Empty));

        Assert.IsAssignableFrom<IEnumerable<T>>(actionResult);
    }

    protected void AssertAction<T>(Func<List<T>> action, Func<T, int> selector) where T : class
    {
        var actionResult = action.Invoke();

        Assert.NotNull(actionResult);

        Output.WriteLine($"GetDivisions {actionResult?.Count ?? 0} items: ");
        actionResult?.ForEach(item => Output.WriteLine(item?.ToString() ?? string.Empty));

        Assert.IsAssignableFrom<IEnumerable<T>>(actionResult);

        Assert.True(actionResult?.All(item => selector(item) > 0));
    }

    protected async void AssertAction<T>(Func<Task<List<T>>> action, Func<T, int> selector) where T : class
    {
        var actionResult = await action.Invoke();

        Assert.NotNull(actionResult);

        Output.WriteLine($"GetDivisions {actionResult?.Count ?? 0} items: ");
        actionResult?.ForEach(item => Output.WriteLine(item?.ToString() ?? string.Empty));

        Assert.IsAssignableFrom<IEnumerable<T>>(actionResult);

        Assert.True(actionResult?.All(item => selector(item) > 0));
    }

    protected void AssertAction<T>(Func<List<T>> action, params Predicate<List<T>>[] predicates) where T : class
    {
        var actionResult = action.Invoke();

        Assert.NotNull(actionResult);

        Output.WriteLine($"GetDivisions {actionResult?.Count ?? 0} items: ");
        actionResult?.ForEach(item => Output.WriteLine(item?.ToString() ?? string.Empty));

        Assert.IsAssignableFrom<IEnumerable<T>>(actionResult);

        predicates?.ToList()?.ForEach(predicate => Assert.True(predicate(actionResult)));
    }

    protected async void AssertAction<T>(Func<Task<List<T>>> action, params Predicate<List<T>>[] predicates) where T : class
    {
        var actionResult = await action.Invoke();

        Assert.NotNull(actionResult);

        Output.WriteLine($"GetDivisions {actionResult?.Count ?? 0} items: ");
        actionResult?.ForEach(item => Output.WriteLine(item?.ToString() ?? string.Empty));

        Assert.IsAssignableFrom<IEnumerable<T>>(actionResult);

        predicates?.ToList()?.ForEach(predicate => Assert.True(predicate(actionResult)));
    }

    protected async void AssertAction<TRequest, TResponse, TCheck>(
        TRequest request,
        Func<ResponseSingleAction<TResponse>> action, 
        Func<Task<TCheck>> checkAction) 
        where TRequest : class
        where TResponse : class
        where TCheck : class
    {
        var actionResult = action.Invoke();

        Assert.NotNull(actionResult);

        Output.WriteLine($"Update 1 item: ");
        Output.WriteLine(actionResult.Item?.ToString() ?? string.Empty);

        Assert.IsAssignableFrom<ResponseSingleAction<TResponse>>(actionResult);

        var checkActionResult = await checkAction.Invoke();

        Assert.NotNull(checkActionResult);

        Assert.IsAssignableFrom<TCheck>(checkActionResult);

        Assert.Equal(Mapper.Map<TResponse>(request), actionResult.Item);

        Assert.Equal(Mapper.Map<TResponse>(checkActionResult), actionResult.Item);
    }

    protected async void AssertAction<TRequest, TResponse, TCheck>(
    TRequest request,
    Func<Task<ResponseSingleAction<TResponse>>> action,
    Func<Task<TCheck>> checkAction)
    where TRequest : class
    where TResponse : class
    where TCheck : class
    {
        var actionResult = await action.Invoke();

        Assert.NotNull(actionResult);

        Output.WriteLine($"Update 1 item: ");
        Output.WriteLine(actionResult.Item?.ToString() ?? string.Empty);

        Assert.IsAssignableFrom<ResponseSingleAction<TResponse>>(actionResult);

        var checkActionResult = await checkAction.Invoke();

        Assert.NotNull(checkActionResult);

        Assert.IsAssignableFrom<TCheck>(checkActionResult);

        var requestMapped = Mapper.Map<TResponse>(request);
        var checkResultMapped = Mapper.Map<TResponse>(checkActionResult);

        Assert.Equal(requestMapped, actionResult.Item);

        Assert.Equal(checkResultMapped, actionResult.Item);
    }

    protected async void AssertAction<TRequest, TResponse, TCheck>(
        RequestSingleAction<TRequest> request,
        Func<Task<ResponseSingleAction<TResponse>>> action,
        Func<Task<TCheck>> checkAction)
        where TRequest : class
        where TResponse : class
        where TCheck : class
    {
        var actionResult = await action.Invoke();

        Output.WriteLine($"Process 1 item: ");
        Output.WriteLine(actionResult.Item?.ToString() ?? string.Empty);

        Assert.IsAssignableFrom<ResponseSingleAction<TResponse>>(actionResult);

        Assert.NotNull(actionResult);

        Assert.NotNull(actionResult.Item);

        var checkActionResult = await checkAction.Invoke();

        Assert.NotNull(checkActionResult);

        Assert.IsAssignableFrom<TCheck>(checkActionResult);

        var requestMapped = Mapper.Map<TResponse>(request.Item);
        var checkResultMapped = Mapper.Map<TResponse>(checkActionResult);

        Assert.Equal(requestMapped, actionResult.Item);

        Assert.Equal(checkResultMapped, actionResult.Item);
    }

    protected async void AssertAction<TRequest, TResponse, TCheck>(
        RequestGroupAction<TRequest> request,
        Func<Task<ResponseGroupActionDetailed<TResponse, TCheck>>> action,
        Func<TRequest, Task<TCheck>> checkAction)
        where TRequest : class
        where TResponse : class
        where TCheck : class
    {
        var actionResult = await action.Invoke();

        Assert.NotNull(actionResult);
        Assert.NotNull(actionResult.SuccessItems);
        Assert.NotNull(actionResult.NotSuccessItems);
        Assert.True(actionResult.SuccessItems.Count + actionResult.NotSuccessItems.Count == (request?.Items?.Count ?? 0));

        Output.WriteLine($"Process {actionResult?.SuccessItems?.Count ?? 0} success items: ");
        actionResult?.SuccessItems?.ForEach(item => Output.WriteLine(item.ToString() ?? string.Empty));

        Output.WriteLine($"Not Process {actionResult?.NotSuccessItems?.Count ?? 0} items: ");
        actionResult?.NotSuccessItems?.ForEach(item => Output.WriteLine($"{item?.NotSuccessItem?.ToString() ?? string.Empty} - {item?.Reason?.ToString() ?? string.Empty}"));

        Assert.IsAssignableFrom<ResponseGroupActionDetailed<TResponse, TCheck>>(actionResult);

        List<TCheck> checkActionResult = new();

        foreach(var item in request.Items ?? new())
        {
            checkActionResult.Add(await checkAction?.Invoke(item));
        }

        var requestMapped = request?.Items?.Select(item => Mapper.Map<TResponse>(item));
        var checkResultMapped = checkActionResult?.Select(item => Mapper.Map<TResponse>(item));
        var expected = actionResult?.SuccessItems?.Union(actionResult?.NotSuccessItems?.Select(item => Mapper.Map<TResponse>(item.NotSuccessItem))?.ToList() ?? new());

        Assert.Equal(requestMapped, expected);

        Assert.Equal(checkResultMapped, expected);
    }

    protected async void AssertAction<TRequest, TResponse, TCheck>(
        RequestGroupAction<TRequest> request,
        Func<Task<ResponseGroupActionDetailed<TResponse, TCheck>>> action,
        Func<TResponse, Task<TCheck>> checkAction)
        where TRequest : class
        where TResponse : class
        where TCheck : class
    {
        var actionResult = await action.Invoke();

        Assert.NotNull(actionResult);
        Assert.NotNull(actionResult.SuccessItems);
        Assert.NotNull(actionResult.NotSuccessItems);
        Assert.True(actionResult.SuccessItems.Count + actionResult.NotSuccessItems.Count == (request?.Items?.Count ?? 0));

        Output.WriteLine($"Process {actionResult?.SuccessItems?.Count ?? 0} success items: ");
        actionResult?.SuccessItems?.ForEach(item => Output.WriteLine(item.ToString() ?? string.Empty));

        Output.WriteLine($"Not process {actionResult?.NotSuccessItems?.Count ?? 0} items: ");
        actionResult?.NotSuccessItems?.ForEach(item => Output.WriteLine($"{item?.NotSuccessItem?.ToString() ?? string.Empty} - {item?.Reason?.ToString() ?? string.Empty}"));

        Assert.IsAssignableFrom<ResponseGroupActionDetailed<TResponse, TCheck>>(actionResult);

        List<TCheck> checkActionResult = new();

        foreach (var item in actionResult?.SuccessItems ?? new())
        {
            checkActionResult.Add(await checkAction?.Invoke(item));
        }

        var requestMapped = request?.Items?.Select(item => Mapper.Map<TResponse>(item));
        var checkResultMapped = checkActionResult?.Select(item => Mapper.Map<TResponse>(item));

        Assert.True((requestMapped?.Count() ?? 0) == (actionResult?.SuccessItems?.Count() ?? 0) + (actionResult?.NotSuccessItems?.Count() ?? 0));

        Assert.Equal(checkResultMapped, actionResult?.SuccessItems ?? new());
    }
}