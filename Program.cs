using Serilog;
using SystemAnalyzer;
using SystemAnalyzer.Configurations;

var progData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(progData, "SystemAnalyzer", "servicetracelog.txt"))
    .CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseSerilog()
    .ConfigureAppConfiguration((hostingContext, configuration) =>
    {
        configuration.Sources.Clear();
        IHostEnvironment env = hostingContext.HostingEnvironment;
        configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((hostingContext, services) =>
    {
        services.AddHostedService<Worker>();
        services.Configure<AppSetting>(hostingContext.Configuration.GetSection("AppSettings"));
    })
    .Build();

await host.RunAsync();
