using System;

using Serilog;

namespace ContactForm
{
    public class App
    {
        private readonly AppSettings _settings;
        private readonly ILogger _logger;

        public App(
            AppSettings settings,
            ILogger logger
        )
        {
            _settings = settings ??
                throw new ArgumentNullException(nameof(settings));
            _logger = logger ??
                throw new ArgumentNullException(nameof(settings));
        }

        public string Run(string input)
        {
            _logger.Information("Lambda Function Starting");
            var result = $"{_settings.Prefix}{input?.ToUpper()}";
            _logger.Information("Lambda Function Finished");
            return result;
        }
    }
}