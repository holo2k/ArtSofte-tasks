using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Web;
using Core.HttpLogic.Services.Interfaces;
using Core.TraceLogic.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.HttpLogic.Services;

public enum ContentType
{
    ///
    Unknown = 0,

    ///
    ApplicationJson = 1,

    ///
    XWwwFormUrlEncoded = 2,

    ///
    Binary = 3,

    ///
    ApplicationXml = 4,

    ///
    MultipartFormData = 5,

    /// 
    TextXml = 6,

    /// 
    TextPlain = 7,

    ///
    ApplicationJwt = 8
}

public record HttpRequestData
{
    /// <summary>
    ///     Тип метода
    /// </summary>
    public HttpMethod Method { get; init; } = HttpMethod.Get;

    /// <summary>
    ///     Адрес запроса
    /// </summary>
    /// \
    public Uri Uri { get; init; } = default!;

    /// <summary>
    ///     Тело метода
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    ///     content-type, указываемый при запросе
    /// </summary>
    public ContentType ContentType { get; set; } = ContentType.ApplicationJson;

    /// <summary>
    ///     Заголовки, передаваемые в запросе
    /// </summary>
    public IDictionary<string, string> HeaderDictionary { get; set; } = new Dictionary<string, string>();

    /// <summary>
    ///     Коллекция параметров запроса
    /// </summary>
    public ICollection<KeyValuePair<string, string>> QueryParameterList { get; set; } =
        new List<KeyValuePair<string, string>>();
}

public record BaseHttpResponse
{
    /// <summary>
    ///     Статус ответа
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    ///     Заголовки, передаваемые в ответе
    /// </summary>
    public HttpResponseHeaders? Headers { get; set; }

    /// <summary>
    ///     Заголовки контента
    /// </summary>
    public HttpContentHeaders? ContentHeaders { get; init; }

    /// <summary>
    ///     Является ли статус код успешным
    /// </summary>
    public bool IsSuccessStatusCode
    {
        get
        {
            var statusCode = (int)StatusCode;

            return statusCode >= 200 && statusCode <= 299;
        }
    }
}

public record HttpResponse<TResponse> : BaseHttpResponse
{
    /// <summary>
    ///     Тело ответа
    /// </summary>
    public TResponse? Body { get; set; }
}

/// <inheritdoc />
public class HttpRequestService : IHttpRequestService
{
    private readonly IHttpConnectionService _httpConnectionService;
    private readonly IEnumerable<ITraceWriter> _traceWriterList;

    ///
    public HttpRequestService(
        IHttpConnectionService httpConnectionService,
        IEnumerable<ITraceWriter> traceWriterList)
    {
        _httpConnectionService = httpConnectionService;
        _traceWriterList = traceWriterList;
    }

    /// <inheritdoc />
    public async Task<HttpResponse<TResponse>> SendRequestAsync<TResponse>(HttpRequestData requestData,
        HttpConnectionData connectionData = default)
    {
        var client = _httpConnectionService.CreateHttpClient(connectionData);

        var uriBuilder = new UriBuilder(requestData.Uri);
        if (requestData.QueryParameterList?.Any() == true)
        {
            var qp = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var kv in requestData.QueryParameterList)
                qp[kv.Key] = kv.Value;
            uriBuilder.Query = qp.ToString();
        }

        var httpRequestMessage = new HttpRequestMessage(requestData.Method, uriBuilder.Uri);

        foreach (var traceWriter in _traceWriterList)
            httpRequestMessage.Headers.TryAddWithoutValidation(traceWriter.Name, traceWriter.GetValue());

        if (requestData.HeaderDictionary != null)
            foreach (var kv in requestData.HeaderDictionary)
                httpRequestMessage.Headers.TryAddWithoutValidation(kv.Key, kv.Value);

        if (requestData.Body != null && requestData.Method != HttpMethod.Get && requestData.Method != HttpMethod.Head)
            httpRequestMessage.Content = PrepairContent(requestData.Body, requestData.ContentType);

        var httpResponse = await _httpConnectionService.SendRequestAsync(
            httpRequestMessage,
            client,
            connectionData.CancellationToken);

        var result = new HttpResponse<TResponse>
        {
            StatusCode = httpResponse.StatusCode,
            Headers = httpResponse.Headers,
            ContentHeaders = httpResponse.Content?.Headers
        };

        var raw = httpResponse.Content != null ? await httpResponse.Content.ReadAsStringAsync() : null;

        if (!string.IsNullOrEmpty(raw))
        {
            if (typeof(TResponse) == typeof(string))
                result.Body = (TResponse)(object)raw!;
            else
                try
                {
                    var body = JsonConvert.DeserializeObject<TResponse>(raw);
                    result.Body = body!;
                }
                catch (JsonException)
                {
                    result.Body = default!;
                }
        }
        else
        {
            result.Body = default!;
        }

        return result;
    }


    private static HttpContent PrepairContent(object body, ContentType contentType)
    {
        switch (contentType)
        {
            case ContentType.ApplicationJson:
                if (body is string stringBody) body = JToken.Parse(stringBody);
                var serializeSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
                var serializedBody = JsonConvert.SerializeObject(body, serializeSettings);
                return new StringContent(serializedBody, Encoding.UTF8, MediaTypeNames.Application.Json);

            case ContentType.XWwwFormUrlEncoded:
                if (body is not IEnumerable<KeyValuePair<string, string>> list)
                    throw new Exception(
                        $"Body for content type {contentType} must be IEnumerable<KeyValuePair<string,string>>");
                return new FormUrlEncodedContent(list);

            case ContentType.ApplicationXml:
            case ContentType.TextXml:
                if (body is not string s)
                    throw new Exception($"Body for content type {contentType} must be XML string");
                return new StringContent(s, Encoding.UTF8,
                    contentType == ContentType.ApplicationXml
                        ? MediaTypeNames.Application.Xml
                        : MediaTypeNames.Text.Xml);

            case ContentType.Binary:
                if (body.GetType() != typeof(byte[]))
                    throw new Exception($"Body for content type {contentType} must be byte[]");
                return new ByteArrayContent((byte[])body);

            default:
                throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null);
        }
    }
}