using System.Threading.Tasks;
using CommandQuery.AzureFunctions;
using CommandQuery.DependencyInjection;
using CommandQuery.Sample.Contracts.Commands;
using CommandQuery.Sample.Handlers;
using CommandQuery.Sample.Handlers.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandQuery.Sample.AzureFunctions.VsCode2
{
    public static class Command
    {
        private static readonly CommandFunction Func = new CommandFunction(
            new[] { typeof(FooCommandHandler).Assembly, typeof(FooCommand).Assembly }
                .GetCommandProcessor(GetServiceCollection()));
        
        [FunctionName("Command")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "command/{commandName}")] HttpRequest req, ILogger log, string commandName)
        {
            return await Func.Handle(commandName, req, log);
        }

        private static IServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();
            // Add handler dependencies
            services.AddTransient<ICultureService, CultureService>();

            return services;
        }
    }
}