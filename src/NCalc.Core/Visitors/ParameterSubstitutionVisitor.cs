using NCalc.Handlers;

namespace NCalc.Visitors;

/// <summary>
/// Converts a <see cref="LogicalExpression"/> into a string representation while replacing known parameters
/// with their resolved values.
/// </summary>
public class ParameterSubstitutionVisitor(ExpressionContext context, CancellationToken cancellationToken = default) : SerializationVisitor
{
    public override string Visit(Identifier identifier)
    {
        if (TryEvaluateIdentifier(identifier, out var value))
            return SerializeValue(value);

        return base.Visit(identifier);
    }

    protected override string EncapsulateNoValue(LogicalExpression expression)
    {
        if (expression is Identifier identifier)
            return $"{Visit(identifier).TrimEnd()} ";

        return base.EncapsulateNoValue(expression);
    }

    private bool TryEvaluateIdentifier(Identifier identifier, out object? value)
    {
        var identifierName = identifier.Name;
        var parameterArgs = new ParameterEventArgs(identifier.Id, cancellationToken);

        context.EvaluateParameterHandler?.Invoke(identifierName, parameterArgs);

        if (parameterArgs.HasResult)
        {
            value = parameterArgs.Result;
            return true;
        }

        if (context.StaticParameters.TryGetValue(identifierName, out value))
            return true;

        if (context.DynamicParameters.TryGetValue(identifierName, out var dynamicParameter))
        {
            value = dynamicParameter(new ParameterData(identifier.Id, context, cancellationToken));
            return true;
        }

        if (identifierName.Equals("null", StringComparison.InvariantCultureIgnoreCase)
            && context.Options.HasFlag(ExpressionOptions.AllowNullParameter))
        {
            value = null;
            return true;
        }

        value = null;
        return false;
    }

    private string SerializeValue(object? value)
    {
        while (true)
        {
            if (value is null)
                return "null ";

            if (value is Expression expression)
            {
                value = expression.Evaluate(cancellationToken);
                continue;
            }

            return new ValueExpression(value).Accept(this).TrimEnd(' ');
        }
    }
}
