using System.Net.Http.Headers;
using Contracts.Models.Cafe;
using Core.Services;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Sms.Test;
using MenuItem = Contracts.Models.Cafe.MenuItem;

namespace Library.gRpc.Services;

public class CafeService : ICafeService
{
    private SmsTestService.SmsTestServiceClient _client;
    private ILogger<CafeService> _logger;

    public CafeService(SmsTestService.SmsTestServiceClient client, ILogger<CafeService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Menu> GetMenu(MenuParameters parameters, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"[{nameof(CafeService)}] GetMenu");

        BoolValue value = new()
        {
            Value = parameters.WithPrice
        };

        try
        {
            var response = await _client.GetMenuAsync(value, cancellationToken: cancellationToken);
            if (response is null)
            {
                throw new Exception("GetMenu returned null");
            }

            if (!response.Success)
            {
                throw new Exception(response.ErrorMessage);
            }

            return new Menu
            {
                MenuItems = response.MenuItems.Select(menu =>
                        new MenuItem
                        {
                            Id = menu.Id,
                            Article = menu.Article,
                            Name = menu.Name,
                            Price = (decimal)menu.Price,
                            IsWeighted = menu.IsWeighted,
                            FullPath = menu.FullPath,
                            Barcodes = menu.Barcodes.ToArray()
                        })
                    .ToArray()
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, ex.Message);
            throw;
        }
    }

    public async Task<bool> SendOrder(MenuItem[] menuItems, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"[{nameof(CafeService)}] SendOrder");

        if (menuItems.Length == 0)
        {
            throw new ArgumentNullException(nameof(menuItems));
        }

        if (menuItems.Any(menuItem => string.IsNullOrEmpty(menuItem.Id)
                                      || menuItem.Quantity is null))
        {
            throw new Exception("Id and Quantity cannot be null");
        }

        Order order = new()
        {
            Id = Guid.NewGuid().ToString()
        };
        order.OrderItems.AddRange(menuItems.Select(menuItem =>
            new OrderItem
            {
                Id = menuItem.Id,
                Quantity = (double)menuItem.Quantity!
            }));

        try
        {
            var response = await _client.SendOrderAsync(order, cancellationToken: cancellationToken);
            if (response is null)
            {
                throw new Exception("SendOrder returned null");
            }

            return !response.Success 
                ? throw new Exception(response.ErrorMessage) 
                : true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, ex.Message);
            throw;
        }
    }
}