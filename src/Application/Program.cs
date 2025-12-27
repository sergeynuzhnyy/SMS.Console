using System.Text.RegularExpressions;
using Contracts.Models;
using Contracts.Models.Cafe;
using Core.Services;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


// Конфигурируем хост
var host = new HostBuilder()
    .ConfigureHost(args)
    .Build();

// Запускаем все задачи, одна из которых применяет миграции
var hasErrorStartupTask = false;
var serviceProvider = host.Services;

using var serviceScope = serviceProvider.CreateScope();
var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();


using (var serviceScopeStartupTasks = serviceProvider.CreateScope())
{
    foreach (var task in serviceProvider.GetServices<IStartupTask>())
    {
        try
        {
            await task.RunAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            hasErrorStartupTask =  true;
            logger.LogError(ex, ex.Message);
            throw;
        }
    }
}

if (hasErrorStartupTask)
{
    logger.LogInformation("Exit");
    return;
}


var cafeService = serviceScope.ServiceProvider.GetRequiredService<ICafeService>();
var smsRepository = serviceScope.ServiceProvider.GetRequiredService<ISmsRepository>();

Menu menu;
try
{
    menu = await cafeService
        .GetMenu(new MenuParameters { WithPrice = true }, CancellationToken.None)
        .ConfigureAwait(false);
    
    await InsertSuccessRepo(smsRepository, "GetMenu").ConfigureAwait(false);
}
catch (Exception ex)
{
    logger.LogError(ex, ex.Message);
    try
    {
        await InsertErrorRepo(smsRepository, "GetMenu", ex.Message).ConfigureAwait(false);
    }
    catch (Exception ex2)
    {
        logger.LogError(ex2, ex2.Message);
    }

    return;
}

if (menu?.MenuItems is null || menu.MenuItems.Length == 0)
{
    logger.LogError("Menu is null or empty");
    logger.LogInformation("Exit");
    return;
}

Console.WriteLine(string.Join(
    Environment.NewLine,
    menu.MenuItems.Select(item => $"{item.Name} - {item.Id} ({item.Article}) - {item.Price}")));

var regex = new Regex(@"(?<code>[^:;]+):(?<count>[^;]+)(?:;|$)");

List<MenuItem> orderItems = [];
while (true)
{
    orderItems.Clear();
    var error = false;
    Console.WriteLine("Enter your order in the format: code1:count1;code2:count2;...");
    Console.WriteLine("Leave a blank line for the exit.");
    
    var orderRaw = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(orderRaw))
    {
        break;
    }

    logger.LogInformation("Your order is {orderRaw}", orderRaw);

    if (!regex.IsMatch(orderRaw))
    {
        logger.LogInformation("Invalid order");
        logger.LogInformation("Reply");
        continue;
    }
    
    foreach (Match match in regex.Matches(orderRaw))
    {
        var code = match.Groups["code"].Value;
        var count = match.Groups["count"].Value;

        if (string.IsNullOrWhiteSpace(code))
        {
            logger.LogInformation($"Invalid code '{code}'");
            error = true;
            break;
        }

        if (menu.MenuItems.FirstOrDefault(item => item.Id == code) is null)
        {
            logger.LogInformation($"Not found menu item '{code}'");
            error = true;
            break;
        }

        if (!decimal.TryParse(count, out var countDecimal))
        {
            logger.LogInformation("Invalid count '{count}'");
            error = true;
            break;
        }

        if (!(countDecimal > 0))
        {
            logger.LogInformation($"Count '{count}' should be greater than zero");
            error = true;
            break;
        }
        
        orderItems.Add(new MenuItem { Id = code, Quantity =  countDecimal });
    }

    if (!error && orderItems.Count != 0)
    {
        break;
    }
}

if (orderItems.Count == 0)
{
    logger.LogInformation("Empty order");
    logger.LogInformation("Exit");
    return;
}

logger.LogInformation("Order Items = {orderItems}", orderItems.Count);
try
{
    var result = await cafeService.SendOrder(orderItems.ToArray(), CancellationToken.None);
    if (result)
    {
        logger.LogInformation("SUCCESS");
    }
    else
    {
        throw new Exception("Unknown error");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, ex.Message);
}
finally
{
    logger.LogInformation("Exit");
}

return;


static Task InsertErrorRepo(ISmsRepository smsRepository, string command, string error)
{
    return smsRepository.InsertDbResultAsync(new DbResult
    {
        Id = Guid.NewGuid(),
        Command = command,
        IsSuccess = false,
        Message = error,
        CreatedOn = DateTimeOffset.UtcNow
    });
}

static Task InsertSuccessRepo(ISmsRepository smsRepository, string command)
{
    return smsRepository.InsertDbResultAsync(new DbResult
    {
        Id = Guid.NewGuid(),
        Command = command,
        IsSuccess = true,
        Message = "",
        CreatedOn = DateTimeOffset.UtcNow
    });
}