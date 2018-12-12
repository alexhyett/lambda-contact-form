using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using ContactForm.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Serilog;
using Serilog.Context;

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
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public object FunctionHandler(ContactRequest input, ILambdaContext context)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional : false, reloadOnChange : true)
                .AddJsonFile("appsettings.local.json", optional : true, reloadOnChange : true)
                .AddEnvironmentVariables(prefix: "LAMBDA_")
                .Build();

            _appSettings = new AppSettings();
            configuration.GetSection("App").Bind(_appSettings);

            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Destructure.AsScalar<JObject>()
                .Destructure.AsScalar<JArray>()
                .CreateLogger();

            try
            {
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var appService = serviceProvider.GetService<App>();

                appService.Run(input).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                using(LogContext.PushProperty("Name", input.Email))
                using(LogContext.PushProperty("Email", input.Email))
                using(LogContext.PushProperty("Phone", input.Phone))
                using(LogContext.PushProperty("Website", input.Website))
                using(LogContext.PushProperty("Body", input.Body))
                {
                    _logger.Error(ex, "Error sending email from contact form");
                }

                throw;
            }

            Log.CloseAndFlush();
            return new { location = _appSettings.ReturnUrl };
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