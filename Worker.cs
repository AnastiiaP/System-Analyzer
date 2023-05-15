using Microsoft.Extensions.Options;
using SystemAnalyzer.Configurations;
using SystemAnalyzer.Helpers;

namespace SystemAnalyzer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly AppSetting appSetting;
    private bool lastInternetState;
    private long lastAvailableDiskSpace;

    public Worker(ILogger<Worker> logger, IOptions<AppSetting> appSettings)
    {
        _logger = logger;
        appSetting = appSettings.Value; 
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int delay = Convert.ToInt32(appSetting.TaskMilliSecondsDellay);
        InternetHelper internetHelper = new InternetHelper(_logger, appSetting);
        DiskHelper diskHelper = new DiskHelper(_logger, appSetting);    
        while (!stoppingToken.IsCancellationRequested)
        {
            bool isConnected = await internetHelper.AnalyzeInternetConnection(lastInternetState);
            lastInternetState = isConnected;
            lastAvailableDiskSpace = diskHelper.AnalyzeChangingDiskMemory(lastAvailableDiskSpace);
            await Task.Delay(delay, stoppingToken);
        }
    }
    public override async Task<Task> StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{time} SystemAnalyzer почав роботу.", DateTimeOffset.Now);
        InternetHelper internetHelper = new InternetHelper(_logger, appSetting);
        DiskHelper diskHelper = new DiskHelper(_logger, appSetting);
        lastAvailableDiskSpace = diskHelper.AnalyzeDiskSpace(); 
        bool isConnected =  await internetHelper.GetInternetConnectionState();
        lastInternetState = isConnected;
        return base.StartAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{time} SystemAnalyzer закінчив роботу.", DateTimeOffset.Now);
        return base.StopAsync(stoppingToken);
    }
}

