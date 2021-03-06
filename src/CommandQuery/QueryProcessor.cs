﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandQuery.Exceptions;
using CommandQuery.Internal;
using Newtonsoft.Json.Linq;

namespace CommandQuery
{
    /// <summary>
    /// Process queries by invoking the corresponding handler.
    /// </summary>
    public interface IQueryProcessor
    {
        /// <summary>
        /// Process a query.
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="queryName">The name of the query</param>
        /// <param name="json">The JSON representation of the query</param>
        /// <returns>The result of the query</returns>
        Task<TResult> ProcessAsync<TResult>(string queryName, string json);

        /// <summary>
        /// Process a query.
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="queryName">The name of the query</param>
        /// <param name="json">The JSON representation of the query</param>
        /// <returns>The result of the query</returns>
        Task<TResult> ProcessAsync<TResult>(string queryName, JObject json);

        /// <summary>
        /// Process a query.
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="queryName">The name of the query</param>
        /// <param name="dictionary">The key/value representation of the query</param>
        /// <returns>The result of the query</returns>
        Task<TResult> ProcessAsync<TResult>(string queryName, IDictionary<string, string> dictionary);

        /// <summary>
        /// Process a query.
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="query">The query</param>
        /// <returns>The result of the query</returns>
        Task<TResult> ProcessAsync<TResult>(IQuery<TResult> query);

        /// <summary>
        /// Returns the types of queries that can be processed.
        /// </summary>
        /// <returns>Supported queries</returns>
        IEnumerable<Type> GetQueries();
    }

    /// <summary>
    /// Process queries by invoking the corresponding handler.
    /// </summary>
    public class QueryProcessor : IQueryProcessor
    {
        private readonly ITypeCollection _typeCollection;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryProcessor" /> class.
        /// </summary>
        /// <param name="typeCollection">A collection of supported queries</param>
        /// <param name="serviceProvider">A service provider with supported query handlers</param>
        public QueryProcessor(IQueryTypeCollection typeCollection, IServiceProvider serviceProvider)
        {
            _typeCollection = typeCollection;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Process a query.
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="queryName">The name of the query</param>
        /// <param name="json">The JSON representation of the query</param>
        /// <returns>The result of the query</returns>
        public async Task<TResult> ProcessAsync<TResult>(string queryName, string json)
        {
            return await ProcessAsync<TResult>(queryName, JObject.Parse(json));
        }

        /// <summary>
        /// Process a query.
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="queryName">The name of the query</param>
        /// <param name="json">The JSON representation of the query</param>
        /// <returns>The result of the query</returns>
        public async Task<TResult> ProcessAsync<TResult>(string queryName, JObject json)
        {
            var queryType = _typeCollection.GetType(queryName);

            if (queryType == null) throw new QueryProcessorException($"The query type '{queryName}' could not be found");

            var query = json.SafeToObject(queryType);

            if (query == null) throw new QueryProcessorException("The json could not be converted to an object");

            return await ProcessAsync((dynamic)query);
        }

        /// <summary>
        /// Process a query.
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="queryName">The name of the query</param>
        /// <param name="dictionary">The key/value representation of the query</param>
        /// <returns>The result of the query</returns>
        public async Task<TResult> ProcessAsync<TResult>(string queryName, IDictionary<string, string> dictionary)
        {
            var queryType = _typeCollection.GetType(queryName);

            if (queryType == null) throw new QueryProcessorException($"The query type '{queryName}' could not be found");

            var query = dictionary.SafeToObject(queryType);

            if (query == null) throw new QueryProcessorException("The dictionary could not be converted to an object");

            return await ProcessAsync((dynamic)query);
        }

        /// <summary>
        /// Process a query.
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="query">The query</param>
        /// <returns>The result of the query</returns>
        public async Task<TResult> ProcessAsync<TResult>(IQuery<TResult> query)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));

            dynamic handler = _serviceProvider.GetService(handlerType);

            if (handler == null) throw new QueryProcessorException($"The query handler for '{query}' could not be found");

            return await handler.HandleAsync((dynamic)query);
        }

        /// <summary>
        /// Returns the types of queries that can be processed.
        /// </summary>
        /// <returns>Supported queries</returns>
        public IEnumerable<Type> GetQueries()
        {
            return _typeCollection.GetTypes();
        }
    }
}