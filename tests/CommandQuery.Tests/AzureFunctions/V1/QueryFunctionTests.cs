﻿#if NET471
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CommandQuery.AzureFunctions;
using CommandQuery.Exceptions;
using LoFuUnit.AutoMoq;
using Moq;

namespace CommandQuery.Tests.AzureFunctions.V1
{
    public class QueryFunctionTests : LoFuTest<QueryFunction>
    {
        public async Task when_handling_the_query()
        {
            _req = new HttpRequestMessage { Method = HttpMethod.Post, Content = new StringContent("") };
            _req.SetConfiguration(new HttpConfiguration());
            _log = new FakeTraceWriter();

            Use<Mock<IQueryProcessor>>();

            async Task should_handle_QueryValidationException()
            {
                var queryName = "FakeQuery";
                The<Mock<IQueryProcessor>>().Setup(x => x.ProcessAsync<object>(queryName, It.IsAny<string>())).Throws(new QueryValidationException("invalid"));

                var result = await Subject.Handle(queryName, _req, _log);

                await result.ShouldBeErrorAsync("invalid");
            }

            async Task should_handle_Exception()
            {
                var queryName = "FakeQuery";
                The<Mock<IQueryProcessor>>().Setup(x => x.ProcessAsync<object>(queryName, It.IsAny<string>())).Throws(new Exception("fail"));

                var result = await Subject.Handle(queryName, _req, _log);

                await result.ShouldBeErrorAsync("fail");
            }
        }

        HttpRequestMessage _req;
        FakeTraceWriter _log;
    }
}
#endif