using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fancy.ResourceLinker.Gateway.Authentication
{
    /// <summary>
    /// A background service to regulary clean up expired tokens from the database.
    /// </summary>
    internal class TokenCleanupBackgroundService : BackgroundService
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<TokenCleanupBackgroundService> _logger;

        /// <summary>
        /// The service provider.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCleanupBackgroundService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public TokenCleanupBackgroundService(ILogger<TokenCleanupBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new(TimeSpan.FromMinutes(15));
            using IServiceScope scope = _serviceProvider.CreateScope();

            ITokenStore tokenStore = scope.ServiceProvider.GetRequiredService<ITokenStore>();

            // Clean up once at startup
            await tokenStore.CleanupExpiredTokenRecordsAsync();

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await tokenStore.CleanupExpiredTokenRecordsAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }
    }
}
