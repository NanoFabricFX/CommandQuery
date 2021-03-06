﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using CommandQuery.Sample.Contracts.Queries;
using FluentAssertions;
using NUnit.Framework;

namespace CommandQuery.Sample.AWSLambda.Tests
{
    public class QueryTests
    {
        public class when_using_the_real_function_via_Post
        {
            [SetUp]
            public void SetUp()
            {
                Subject = new Query();
                Request = GetRequest("POST", content: "{ 'Id': 1 }");
                Context = new FakeLambdaContext();
            }

            [Test]
            public async Task should_work()
            {
                var result = await Subject.Handle(Request.QueryName("BarQuery"), Context);
                var value = result.As<Bar>();

                value.Id.Should().Be(1);
                value.Value.Should().NotBeEmpty();
            }

            [Test]
            public async Task should_handle_errors()
            {
                var result = await Subject.Handle(Request.QueryName("FailQuery"), Context);

                result.ShouldBeError("The query type 'FailQuery' could not be found");
            }

            Query Subject;
            APIGatewayProxyRequest Request;
            ILambdaContext Context;
        }

        public class when_using_the_real_function_via_Get
        {
            [SetUp]
            public void SetUp()
            {
                Subject = new Query();
                Request = GetRequest("GET", query: new Dictionary<string, string> { { "Id", "1" } });
                Context = new FakeLambdaContext();
            }

            [Test]
            public async Task should_work()
            {
                var result = await Subject.Handle(Request.QueryName("BarQuery"), Context);
                var value = result.As<Bar>();

                value.Id.Should().Be(1);
                value.Value.Should().NotBeEmpty();
            }

            [Test]
            public async Task should_handle_errors()
            {
                var result = await Subject.Handle(Request.QueryName("FailQuery"), Context);

                result.ShouldBeError("The query type 'FailQuery' could not be found");
            }

            Query Subject;
            APIGatewayProxyRequest Request;
            ILambdaContext Context;
        }

        static APIGatewayProxyRequest GetRequest(string method, string content = null, Dictionary<string, string> query = null)
        {
            var request = new APIGatewayProxyRequest
            {
                HttpMethod = method,
                Body = content,
                QueryStringParameters = query
            };

            return request;
        }
    }
}