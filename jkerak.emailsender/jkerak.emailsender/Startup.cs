using System;
using System.Net.Http.Headers;
using System.Text;
using jkerak.emailsender;
using jkerak.emailsender.Interfaces.Services;
using jkerak.emailsender.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]
namespace jkerak.emailsender
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string mailgunApiKey = Environment.GetEnvironmentVariable("MAILGUN_API_KEY");
            string mailgunUri = Environment.GetEnvironmentVariable("MAILGUN_URI");


            builder.Services.AddHttpClient<MailgunService>(c =>
            {
                c.BaseAddress = new Uri(mailgunUri);
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{mailgunApiKey}")));
            });
            
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient =
                new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            
            builder.Services.AddSingleton(keyVaultClient);
            
            builder.Services.AddSingleton<IActivityService, ActivityService>();
            builder.Services.AddSingleton<ILoggingService, LoggingService>();

        }
    }
}