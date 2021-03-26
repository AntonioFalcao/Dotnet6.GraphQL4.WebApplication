﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dotnet5.GraphQL4.CrossCutting.Notifications;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Server;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dotnet5.GraphQL3.Store.WebAPI.Graphs.Executers
{
    public class StoreGraphQLExecuter<TSchema> : DefaultGraphQLExecuter<TSchema>
        where TSchema : ISchema
    {
        private readonly IServiceProvider _serviceProvider;

        public StoreGraphQLExecuter(TSchema schema, IDocumentExecuter documentExecuter, IOptions<GraphQLOptions> options, IEnumerable<IDocumentExecutionListener> listeners, IEnumerable<IValidationRule> validationRules, IServiceProvider serviceProvider)
            : base(schema, documentExecuter, options, listeners, validationRules)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task<ExecutionResult> ExecuteAsync(string operationName, string query, Inputs variables, IDictionary<string, object> context, IServiceProvider requestServices, CancellationToken cancellationToken = new())
        {
            var result = await base.ExecuteAsync(operationName, query, variables, context, requestServices, cancellationToken);
            var notification = _serviceProvider.GetRequiredService<INotificationContext>();

            if (notification.HasNotifications is false) return result;

            result.Errors = notification.ExecutionErrors;
            result.Data = default;

            return result;
        }
    }
}