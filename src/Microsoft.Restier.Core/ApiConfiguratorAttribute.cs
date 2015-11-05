// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Restier.Core
{
    /// <summary>
    /// Specifies a set of methods that can participate in the
    /// configuration, initialization and disposal of an API.
    /// </summary>
// TODO (.NETCORE)    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ApiConfiguratorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiConfiguratorAttribute" /> class.
        /// </summary>
        protected ApiConfiguratorAttribute()
        {
        }

        /// <summary>
        /// Applies configuration from any API configurator attributes
        /// specified on an API type to an API configuration.
        /// </summary>
        /// <param name="type">
        /// An API type.
        /// </param>
        /// <param name="configuration">
        /// An API configuration.
        /// </param>
        public static void ApplyConfiguration(
            Type type, ApiConfiguration configuration)
        {
            Ensure.NotNull(type, "type");
            Ensure.NotNull(configuration, "configuration");
            if (type.GetTypeInfo().BaseType != null)
            {
                ApiConfiguratorAttribute.ApplyConfiguration(
                    type.GetTypeInfo().BaseType, configuration);
            }

            var attributes = type.GetTypeInfo().GetCustomAttributes(
                typeof(ApiConfiguratorAttribute), false);
            foreach (ApiConfiguratorAttribute attribute in attributes)
            {
                attribute.Configure(configuration, type);
            }
        }

        /// <summary>
        /// Applies initialization routines from any API configurator
        /// attributes specified on an API type to an API context.
        /// </summary>
        /// <param name="type">
        /// An API type.
        /// </param>
        /// <param name="instance">
        /// An API instance, if applicable.
        /// </param>
        /// <param name="context">
        /// An API context.
        /// </param>
        public static void ApplyInitialization(
            Type type, object instance, ApiContext context)
        {
            Ensure.NotNull(type, "type");
            Ensure.NotNull(context, "context");
            if (type.GetTypeInfo().BaseType != null)
            {
                ApiConfiguratorAttribute.ApplyInitialization(
                    type.GetTypeInfo().BaseType, instance, context);
            }

            var attributes = type.GetTypeInfo().GetCustomAttributes(
                typeof(ApiConfiguratorAttribute), false);
            foreach (ApiConfiguratorAttribute attribute in attributes)
            {
                attribute.Initialize(context, type, instance);
            }
        }

        /// <summary>
        /// Applies disposal routines from any API configurator
        /// attributes specified on an API type to an API context.
        /// </summary>
        /// <param name="type">
        /// An API type.
        /// </param>
        /// <param name="instance">
        /// An API instance, if applicable.
        /// </param>
        /// <param name="context">
        /// An API context.
        /// </param>
        public static void ApplyDisposal(
            Type type, object instance, ApiContext context)
        {
            Ensure.NotNull(type, "type");
            Ensure.NotNull(context, "context");
            var attributes = type.GetTypeInfo().GetCustomAttributes(
                typeof(ApiConfiguratorAttribute), false);
            foreach (ApiConfiguratorAttribute attribute in attributes.Reverse())
            {
                attribute.Dispose(context, type, instance);
            }

            if (type.GetTypeInfo().BaseType != null)
            {
                ApiConfiguratorAttribute.ApplyDisposal(
                    type.GetTypeInfo().BaseType, instance, context);
            }
        }

        /// <summary>
        /// Configures an API configuration.
        /// </summary>
        /// <param name="configuration">
        /// An API configuration.
        /// </param>
        /// <param name="type">
        /// The API type on which this attribute was placed.
        /// </param>
        public virtual void Configure(
            ApiConfiguration configuration,
            Type type)
        {
        }

        /// <summary>
        /// Initializes an API context.
        /// </summary>
        /// <param name="context">
        /// An API context.
        /// </param>
        /// <param name="type">
        /// The API type on which this attribute was placed.
        /// </param>
        /// <param name="instance">
        /// An API instance, if applicable.
        /// </param>
        public virtual void Initialize(
            ApiContext context,
            Type type,
            object instance)
        {
        }

        /// <summary>
        /// Disposes an API context.
        /// </summary>
        /// <param name="context">
        /// An API context.
        /// </param>
        /// <param name="type">
        /// The API type on which this attribute was placed.
        /// </param>
        /// <param name="instance">
        /// An API instance, if applicable.
        /// </param>
        public virtual void Dispose(
            ApiContext context,
            Type type,
            object instance)
        {
        }
    }
}
