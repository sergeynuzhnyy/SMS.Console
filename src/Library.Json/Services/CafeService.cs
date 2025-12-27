using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts.Models.Api;
using Contracts.Models.Cafe;
using Core.Services;
using Microsoft.Extensions.Logging;

namespace Library.Services;

public class CafeService : ICafeService
{
    private HttpClient _httpClient;
    private ILogger<CafeService> _logger;


    // Maybe add JsonSerializerOptions
    public CafeService(HttpClient httpClient, ILogger<CafeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Menu> GetMenu(MenuParameters parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug($"[{nameof(CafeService)}] GetMenu");
        
        const string command = "GetMenu";

        ApiRequest<MenuParameters> apiRequest = new()
        {
            Command = command,
            CommandParameters = parameters
        };

        return await SendPost<Menu, MenuParameters>(apiRequest, cancellationToken);
    }
    
    
    public async Task<bool> SendOrder(MenuItem[] menuItems, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug($"[{nameof(CafeService)}] SendOrder");
        
        if (menuItems.Length == 0)
        {
            throw new ArgumentNullException(nameof(menuItems));
        }
        
        const string command = "SendOrder";
        Guid id = Guid.NewGuid();

        ApiRequest<OrderParameters> apiRequest = new()
        {
            Command = command,
            CommandParameters = new OrderParameters
            {
                OrderId = id,
                MenuItems = menuItems
            }
        };

        return await SendPost(apiRequest, cancellationToken);
    }

    
    private async Task<bool> SendPost<TRequest>(ApiRequest<TRequest> apiRequest, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage requestMessage = new();
        requestMessage.Content = JsonContent.Create(apiRequest);
        try
        {
            var response = await _httpClient.PostAsJsonAsync("", requestMessage, cancellationToken: cancellationToken);
            response.EnsureSuccessStatusCode();
            var apiResponse =
                await response.Content.ReadFromJsonAsync<ApiResponse>(cancellationToken: cancellationToken);
            if (apiResponse is null)
            {
                throw new Exception("[ApiResponse] The server returned an invalid response.");
            }

            if (!apiResponse.Success)
            {
                throw new Exception(apiResponse.ErrorMessage);
            }

            if (!apiResponse.Command.Equals(apiRequest.Command, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("The server returned an invalid command.");
            }

            return apiResponse.Success;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, ex.Message);
            throw;
        }
    }

    private async Task<TResponse> SendPost<TResponse, TRequest>(ApiRequest<TRequest> apiRequest, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage requestMessage = new();
        requestMessage.Content = JsonContent.Create(apiRequest);
        try
        {
            var response = await _httpClient.PostAsJsonAsync("", requestMessage, cancellationToken: cancellationToken);
            response.EnsureSuccessStatusCode();
            var apiResponse =
                await response.Content.ReadFromJsonAsync<ApiResponse>(cancellationToken: cancellationToken);
            if (apiResponse is null)
            {
                throw new Exception("[ApiResponse] The server returned an invalid response.");
            }

            if (!apiResponse.Success)
            {
                throw new Exception(apiResponse.ErrorMessage);
            }

            if (!apiResponse.Command.Equals(apiRequest.Command, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("The server returned an invalid command.");
            }

            var apiResponseMenu =
                await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(cancellationToken: cancellationToken);
            return apiResponseMenu is null
                ? throw new Exception($"[ApiResponse<{typeof(TResponse).Name}>] The server returned an invalid response.")
                : apiResponseMenu.Data;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, ex.Message);
            throw;
        }
    }
}