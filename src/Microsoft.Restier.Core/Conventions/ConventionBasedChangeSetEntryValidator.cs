﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Restier.Core.Submit;
using DataAnnotations = System.ComponentModel.DataAnnotations;
using ValidationResult = Microsoft.Restier.Core.Submit.ValidationResult;

namespace Microsoft.Restier.Core.Conventions
{
    /// <summary>
    /// A convention-based change set entry validator.
    /// </summary>
    internal class ConventionBasedChangeSetEntryValidator :
        IChangeSetEntryValidator
    {
        static ConventionBasedChangeSetEntryValidator()
        {
            Instance = new ConventionBasedChangeSetEntryValidator();
        }

        private ConventionBasedChangeSetEntryValidator()
        {
        }

        /// <summary>
        /// Gets a static instance of convention-based change set entry validator.
        /// </summary>
        public static ConventionBasedChangeSetEntryValidator Instance { get; private set; }

        /// <inheritdoc/>
        public Task ValidateEntityAsync(
            SubmitContext context,
            ChangeSetEntry entry,
            ValidationResults validationResults,
            CancellationToken cancellationToken)
        {
            Ensure.NotNull(validationResults, "validationResults");
            DataModificationEntry dataModificationEntry = entry as DataModificationEntry;
            if (dataModificationEntry != null)
            {
                object entity = dataModificationEntry.Entity;

                // TODO (.NETCORE): Use PropertyDescriptorCollection in .NET Core (when available?)
                // TODO GitHubIssue#50 : should this PropertyDescriptorCollection be cached?
                IEnumerable<PropertyInfo> properties = entity.GetType().GetTypeInfo().DeclaredProperties;

                DataAnnotations.ValidationContext validationContext = new DataAnnotations.ValidationContext(entity);

                foreach (var property in properties)
                {
                    validationContext.MemberName = property.Name;

                    IEnumerable<DataAnnotations.ValidationAttribute> validationAttributes =
                        property.GetCustomAttributes().OfType<DataAnnotations.ValidationAttribute>();

                    foreach (var validationAttribute in validationAttributes)
                    {
                        object value = property.GetValue(entity);
                        DataAnnotations.ValidationResult validationResult =
                            validationAttribute.GetValidationResult(value, validationContext);
                        if (validationResult != DataAnnotations.ValidationResult.Success)
                        {
                            validationResults.Add(new ValidationResult()
                            {
                                Id = validationAttribute.GetType().FullName,
                                Message = validationResult.ErrorMessage,
                                Severity = ValidationSeverity.Error,
                                Target = entity,
                                PropertyName = property.Name
                            });
                        }
                    }
                }
            }

            return Task.WhenAll();
        }
    }
}
