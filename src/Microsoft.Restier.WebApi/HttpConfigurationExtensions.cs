﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.Restier.Core;
using Microsoft.Restier.WebApi.Routing;

namespace Microsoft.Restier.WebApi
{
    /// <summary>
    /// Offers a collection of extension methods to <see cref="HttpConfiguration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// TODO GitHubIssue#51 : Support model lazy loading
        /// <summary>
        /// Maps the API routes to the RestierController.
        /// </summary>
        /// <typeparam name="TApi">The user API.</typeparam>
        /// <param name="config">The <see cref="HttpConfiguration"/> instance.</param>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routePrefix">The prefix of the route.</param>
        /// <param name="apiFactory">The callback to create API instances.</param>
        /// <returns>The task object containing the resulted <see cref="ODataRoute"/> instance.</returns>
//TODO (.NETCORE) - /// <param name="batchHandler">The handler for batch requests.</param>
        public static async Task<ODataRoute> MapRestierRoute<TApi>(
            this HttpConfiguration config,
            string routeName,
            string routePrefix,
            Func<IApi> apiFactory)
//TODO (.NETCORE) - RestierBatchHandler batchHandler = null)
            where TApi : ApiBase
        {
            Ensure.NotNull(apiFactory, "apiFactory");

            using (var api = apiFactory())
            {
                var model = await api.GetModelAsync();
                model.EnsurePayloadValueConverter();
                var conventions = CreateRestierRoutingConventions(config, model, apiFactory);

#if BATCH
//TODO (.NETCORE)
                if (batchHandler != null && batchHandler.ApiFactory == null)
                {
                    batchHandler.ApiFactory = apiFactory;
                }
#endif

                return config.MapODataServiceRoute(
                    routeName, routePrefix, model, new DefaultODataPathHandler(), conventions /* //TODO (.NETCORE) -, batchHandler*/);
            }
        }

        /// <summary>
        /// Maps the API routes to the RestierController.
        /// </summary>
        /// <typeparam name="TApi">The user API.</typeparam>
        /// <param name="config">The <see cref="HttpConfiguration"/> instance.</param>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routePrefix">The prefix of the route.</param>
        /// <returns>The task object containing the resulted <see cref="ODataRoute"/> instance.</returns>
//TODO (.NETCORE) - /// <param name="batchHandler">The handler for batch requests.</param>
        public static async Task<ODataRoute> MapRestierRoute<TApi>(
            this HttpConfiguration config,
            string routeName,
            string routePrefix)
//TODO (.NETCORE) -RestierBatchHandler batchHandler = null)
            where TApi : ApiBase, new()
        {
            return await MapRestierRoute<TApi>(
                config, routeName, routePrefix, () => new TApi()/* //TODO (.NETCORE) -, batchHandler*/);
        }

        /// <summary>
        /// Creates the default routing conventions.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration"/> instance.</param>
        /// <param name="model">The EDM model.</param>
        /// <param name="apiFactory">The API factory.</param>
        /// <returns>The routing conventions created.</returns>
        private static IList<IODataRoutingConvention> CreateRestierRoutingConventions(
            this HttpConfiguration config, IEdmModel model, Func<IApi> apiFactory)
        {
            // TODO (.NETCORE)
            //var conventions = DefaultODataRoutingConvention.CreateDefaultWithAttributeRouting(config, model);
            //var index = 0;
            //for (; index < conventions.Count; index++)
            //{
            //    var attributeRouting = conventions[index] as AttributeRoutingConvention;
            //    if (attributeRouting != null)
            //    {
            //        break;
            //    }
            //}

            //conventions.Insert(index + 1, new RestierRoutingConvention(apiFactory));
            //return conventions;
            return new IODataRoutingConvention[] { new DefaultODataRoutingConvention() };
        }

        private static void EnsurePayloadValueConverter(this IEdmModel model)
        {
            var payloadValueConverter = model.GetPayloadValueConverter();
            if (payloadValueConverter.GetType() == typeof(ODataPayloadValueConverter))
            {
                // User has not specified custom payload value converter
                // so use RESTier's default converter.
                model.SetPayloadValueConverter(RestierPayloadValueConverter.Default);
            }
        }
    }
}
