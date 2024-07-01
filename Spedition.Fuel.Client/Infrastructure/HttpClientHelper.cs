using System.Runtime;
using System.Xml.Linq;
using Newtonsoft.Json;
using NewtonSerializer = Newtonsoft.Json.JsonSerializer;
using SystemSerializer = System.Text.Json.JsonSerializer;

namespace Spedition.Fuel.Client.Infrastructure
{
    public static class HttpClientHelper
    {
        private const string ContentType = "application/json";

        private static JsonSerializerOptions SerializersOptions
            => new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = false,
                IncludeFields = false,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = false,
                MaxDepth = 5,
                DefaultBufferSize = 32 * 1024 * 1024, // 16Kb default - up to 32Mb
                UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
            };

        public static HttpRequestMessage GetRequestMessage(this string uri, HttpMethod httpMethod)
        {
            return new HttpRequestMessage(httpMethod, uri);
        }

        public static HttpRequestMessage GetRequestMessage<TRequest>(this string uri, HttpMethod httpMethod, TRequest content)
        {
            var request = new HttpRequestMessage(httpMethod, uri);

            request.Content = JsonContent.Create(
                inputValue: content,
                inputType: typeof(TRequest),
                mediaType: new MediaTypeHeaderValue(ContentType),
                options: SerializersOptions);

            return request;
        }  

        public static async Task<HttpResponseMessage> SendRequest(this HttpClient httpClient, HttpRequestMessage request, CancellationToken token = default)
        {
            var details = $"http-запроса по адресу «{request?.RequestUri?.ToString() ?? string.Empty}» ";

            Log.Debug($"Отправка {details}. ");
            try
            {
                var response = await httpClient.SendAsync(
                    request: request,
                    completionOption: HttpCompletionOption.ResponseContentRead,
                    cancellationToken: token).ConfigureAwait(false);

                if (response == null)
                {
                    Log.Error($"Ошибка на уровне сервера при отправке http-запроса {details}. ");
                }

                if (!token.IsCancellationRequested)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Log.Error($"Ошибка аутентификации при отправке {details}. ");
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Log.Error($"Ошибка авторизации при отправке {details}. ");
                    }
                    else if (!response.IsSuccessStatusCode)
                    {
                        Log.Error($"Ошибка на уровне сервера при отправке {details}. ");
                        var error = await response.Content.ReadAsStringAsync();
                        throw new Exception(error);
                    }
                }
                else
                {
                    Log.Warning($"Отмена {details}. ");
                }

                return response;
            }
            catch (OperationCanceledException exception)
            {
                Log.Warning($"Отмена {details}. {exception.GetExeceptionMessages()}");
                return default;
            }
            catch (Exception exception)
            {
                Log.Error($"Исключительная ситуация при отправке {details}. " +
                          $"Подробности: {exception.GetExeceptionMessages()}");
                throw;
            }
        }

        public static async Task<TResponse> DeserializeTResponse<TResponse>(this HttpResponseMessage response)
        {
            var details = $"после отправки http-запроса по адресу «{response?.RequestMessage?.RequestUri?.ToString() ?? string.Empty}». ";

            if ((response?.Content?.Headers?.ContentLength ?? 0) == 0)
            {
                Log.Error($"Ответ пустой после запроса по адресу «{response?.RequestMessage?.RequestUri?.ToString() ?? string.Empty}». ");
                return default;
            }

            TResponse result = default;
            try
            {
                Log.Debug($"Считывание контента (длина: {response?.Content?.Headers?.ContentLength ?? 0} байт) {details}");
                using var data = await response?.Content.ReadAsStreamAsync();
                if ((data?.Length ?? 0) == 0)
                {
                    Log.Error($"Ошибка при выполнении операции по чтению контента из ответа сервера (контент пустой) {details}");
                    return default;
                }

                Log.Information($"Десериализация контента (длина: {data?.Length ?? 0} символов) {details}");
                using var streamReader = new StreamReader(data);
                using var reader = new JsonTextReader(streamReader);
                reader.SupportMultipleContent = true;
                var serializer = new NewtonSerializer();
                result = serializer.Deserialize<TResponse>(reader);
                return result;
            }
            catch (OutOfMemoryException exception)
            {
                GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
                Log.Information($"Запуск принудительной сборки мусора по причине: {exception.GetExeceptionMessages()}");

                // Запуск считывания после сборки мусора на всех поколениях кучи
                var responseBody = await response?.Content?.ReadAsByteArrayAsync();
                if ((responseBody?.Length ?? 0) == 0)
                {
                    Log.Error($"Ошибка при выполнении операции по чтению контента из ответа сервера (контент пустой) {details} ");
                    throw;
                }

                return responseBody != default ? SystemSerializer.Deserialize<TResponse>(responseBody, SerializersOptions) : default;
            }
            catch (Exception exception)
            {
                Log.Error($"Исключительная ситуация при выполнении операции по чтению и десериализации контента " +
                          $"(длина: {response?.Content?.Headers?.ContentLength ?? 0} байт) {details} " +
                          $"Подробности: {exception.GetExeceptionMessages()} ");
                throw;
            }
        }
    }
}
