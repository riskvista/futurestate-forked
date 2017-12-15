﻿#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace FutureState.Specifications
{
    // an - made field name mandatory

    /// <summary>
    /// Used to validate string values. Use required to validate other object values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NotEmptyAttribute : ValidationAttribute
    {
        private readonly string _fieldName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEmptyAttribute" /> class.
        /// </summary>
        /// <param name="fieldDisplayName">Display name of the field.</param>
        public NotEmptyAttribute(string fieldDisplayName)
        {
            _fieldName = fieldDisplayName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var val = Convert.ToString(value);

            var isValid = !string.IsNullOrWhiteSpace(val);
            if (!isValid)
            {
                var typeName =
                    validationContext != null && validationContext.ObjectInstance != null
                        ? validationContext.ObjectInstance.GetType().ToString()
                        : "";

                ErrorMessage = "'{0}' cannot be null or empty at: {1}"
                    .Params(_fieldName, typeName);

                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}