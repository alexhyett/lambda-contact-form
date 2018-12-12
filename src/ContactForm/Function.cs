using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Linq;

using Serilog;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly : LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ContactForm
{
    public class Function
    {
        private AppSettings _appSettings;
        private ILogger _logger;

        /// <summary>
        /// Lamda Function
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns>string</returns>
        public string FunctionHandler(string input, ILambdaContext context)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional : false, reloadOnChange : true)
                .AddEnvironmentVariables(prefix: "LAMBDA_")
                .Build();

            _appSettings = new AppSettings();
            configuration.GetSection("App").Bind(_appSettings);

            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Destructure.AsScalar<JObject>()
                .Destructure.AsScalar<JArray>()
                .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var result = serviceProvider.GetService<App>().Run(input);
            Log.CloseAndFlush();
            return result;
        }

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<App>();
            serviceCollection.AddSingleton<AppSettings>(_appSettings);
            serviceCollection.AddSingleton<ILogger>(_logger);
            serviceCollection.AddLogging(logBuilder => logBuilder.AddSerilog(dispose: true));
        }
    }

}