using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _dependentProperty;
    private readonly object _targetValue;

    public RequiredIfAttribute(string dependentProperty, object targetValue)
    {
        _dependentProperty = dependentProperty;
        _targetValue   = targetValue;
    }

    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var dependentProp = context.ObjectType.GetProperty(_dependentProperty,
            BindingFlags.Public | BindingFlags.Instance);

        if (dependentProp == null)
            throw new ArgumentException($"Property '{_dependentProperty}' not found.");

        var dependentValue = dependentProp.GetValue(context.ObjectInstance, null);

        if (Equals(dependentValue, _targetValue))
        {
            var str = value as string;
            if (value == null || (str != null && string.IsNullOrWhiteSpace(str)))
                return new ValidationResult(
                    ErrorMessage ?? $"{context.MemberName} is required when {_dependentProperty} is {_targetValue}.",
                    new[] { context.MemberName });
        }

        return ValidationResult.Success;
    }
}
