using System.Threading.Tasks;
using CommandQuery.AzureFunctions;
using CommandQuery.DependencyInjection;
using CommandQuery.Sample.Contracts.Queries;
using CommandQuery.Sample.Handlers;
using CommandQuery.Sample.Handlers.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandQuery.Sample.AzureFunctions.VsCode2
{
    public static class Query
    {
        private static readonly QueryFunction Func = new QueryFunction(
            new[] { typeof(BarQueryHandler).Assembly, typeof(BarQuery).Assembly }
                .GetQueryProcessor(GetServiceCollection()));

        [FunctionName("Query")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "query/{queryName}")] HttpRequest req, ILogger log, string queryName)
        {
            return await Func.Handle(queryName, req, log);
        }

        private static IServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();
            // Add handler dependencies
            services.AddTransient<IDateTimeProxy, DateTimeProxy>();

            return services;
        }
    }
}