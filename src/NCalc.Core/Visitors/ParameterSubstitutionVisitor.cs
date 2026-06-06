using NCalc.Handlers;

namespace NCalc.Visitors;

/// <summary>
/// Converts a <see cref="LogicalExpression"/> into a string representation while replacing known parameters
/// with their resolved values.
/// </summary>
public class ParameterSubstitutionVisitor(ExpressionContext context) : SerializationVisitor
{
    public override string Visit(Identifier identifier, CancellationToken cancellationToken = default)
    {
        if (TryEvaluateIdentifier(identifier, cancellationToken, out var value))
            return SerializeValue(value, cancellationToken);

        return base.Visit(identifier, cancellationToken);
    }

    protected override string EncapsulateNoValue(LogicalExpression expression, CancellationToken cancellationToken = default)
    {
        if (expression is Identifier identifier)
            return $"{Visit(identifier, cancellationToken).TrimEnd()} ";

        return base.EncapsulateNoValue(expression, cancellationToken);
    }

    private bool TryEvaluateIdentifier(Identifier identifier, CancellationToken cancellationToken, out object? value)
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

    private string SerializeValue(object? value, CancellationToken cancellationToken)
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

            return new ValueExpression(value).Accept(this, cancellationToken).TrimEnd(' ');
        }
    }
}
