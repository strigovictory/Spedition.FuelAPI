using System.Net.Sockets;
using Spedition.Fuel.Shared.Settings.Configs;
using static Grpc.Core.Metadata;

namespace Spedition.Fuel.BFF.Infrastructure.gRPC;

public abstract class GRPCServiceBase<T>
    where T : ClientBase<T>
{
    private const string ApiKeyHeaderName = "x-api-key"; // аутентификационный ключ для отправки в метаданных gRPC-запроса

    private readonly ApiConfigs apiConfigs;

    private readonly T client;

    public GRPCServiceBase(T client, IOptions<ApiConfigs> apiConfigs)
    {
        this.client = client;
        this.apiConfigs = apiConfigs.Value;
    }

    private List<dynamic> MethodsParameters { get; set; } // параметры метода для файла proto

    private MethodInfo MethodToInvoke { get; set; } // метод из файла proto, подлежащий выполнению

    /// <summary>
    /// Метод вызова любого из методов, прописанных в proto-файле.
    /// </summary>
    /// <param Name="methodName">Наименование метода из файла .proto.</param>
    /// <param Name="methodsParameters">Параметры, передаваемые методу для его выполнения.</param>
    /// <returns>Динамический тип - прописан в proto-файле (например returns (ListFuelTransactionReply)).</returns>
    protected async Task<TResult> DoRequest<TResult>(string methodName, List<dynamic> methodsParameters)
    {
        object notMappedResponse = default;
        try
        {
            MethodsParameters = methodsParameters;
            InitializeMethod(methodName);
            AddMetadataToRequest();

            notMappedResponse = MethodToInvoke?.Invoke(client, MethodsParameters?.ToArray() ?? Array.Empty<dynamic>()) ?? default;
        }
        catch (Exception exc) when (exc.InnerException is RpcException rpcEsc && rpcEsc.StatusCode == StatusCode.PermissionDenied)
        {
            Log.Error($"User does not have permission to resource {MethodToInvoke.Name}. " +
                      $"Details: {exc.GetExeceptionMessages()}");
        }
        catch (Exception exc) when (exc.InnerException is RpcException rpcEsc)
        {
            Log.Error($"RpcException in class «{GetType().Name}» method «{MethodToInvoke.Name}». " +
                      $"Status: {rpcEsc.StatusCode}. " +
                      $"Details: {exc.GetExeceptionMessages()}");
        }
        catch (Exception exc) when (exc.InnerException is SocketException)
        {
            Log.Error($"SocketException in class «{GetType().Name}» method «{MethodToInvoke.Name}». " +
                      $"Details: {exc.GetExeceptionMessages()}");
        }
        catch (Exception exc)
        {
            Log.Error($"Error in {GetType().Name}.{nameof(DoRequest)}. " +
                      $"Details: {exc.GetExeceptionMessages()}");
            throw;
        }

        return MapResponse(notMappedResponse);
    }

    protected abstract dynamic MapResponse(object response);

    private void InitializeMethod(string methodName)
    {
        var grpcType = client.GetType();
        var methods = grpcType.GetTypeInfo().GetDeclaredMethods(methodName);
        MethodToInvoke = methods?.FirstOrDefault(mi =>
        (mi.GetParameters()
            .Where(pi => !pi.HasDefaultValue)?
            .Count() ?? 0) == (MethodsParameters?.Count() ?? 0) + 1);
    }

    private void AddMetadataToRequest()
    {
        CallOptions callOptions = new (headers: new ());
        callOptions.Headers.Add(new Entry(ApiKeyHeaderName, apiConfigs.Key));
        MethodsParameters = MethodsParameters?.Append(callOptions)?.ToList() ?? new ();
    }
}
