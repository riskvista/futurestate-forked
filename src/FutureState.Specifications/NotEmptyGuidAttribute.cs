﻿using System;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Specifications
{
    // an - made field name mandatory

    /// <summary>
    /// Used to validate string values. Use required to validate other object values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NotEmptyGuidAttribute : ValidationAttribute
    {
        private readonly string _fieldName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEmptyGuidAttribute" /> class.
        /// </summary>
        /// <param name="fieldDisplayName">Display name of the field.</param>
        public NotEmptyGuidAttribute(string fieldDisplayName)
        {
            _fieldName = fieldDisplayName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var val = Convert.ToString(value);

            Guid guid = Guid.Empty;

            if (!Guid.TryParse(val, out guid))
            {
                var typeName =
                    validationContext != null && validationContext.ObjectInstance != null
                        ? validationContext.ObjectInstance.GetType().ToString()
                        : "";

                ErrorMessage = "'{0}' cannot be an empty guid: {1}"
                    .Params(_fieldName, typeName);

                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}