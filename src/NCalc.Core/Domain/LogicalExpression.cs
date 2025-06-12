#if NET
using System.Text.Json.Serialization;
#endif
using System.Diagnostics.Contracts;

using NCalc.Visitors;

namespace NCalc.Domain;

/// <summary>
/// Represents an abstract syntax tree (AST) node for logical expressions.
/// </summary>
#if NET
[JsonPolymorphic]
[JsonDerivedType(typeof(BinaryExpression), typeDiscriminator: "binary")]
[JsonDerivedType(typeof(Function), typeDiscriminator: "function")]
[JsonDerivedType(typeof(Identifier), typeDiscriminator: "identifier")]
[JsonDerivedType(typeof(LogicalExpressionList), typeDiscriminator: "list")]
[JsonDerivedType(typeof(TernaryExpression), typeDiscriminator: "ternary")]
[JsonDerivedType(typeof(UnaryExpression), typeDiscriminator: "unary")]
[JsonDerivedType(typeof(PercentExpression), typeDiscriminator: "percent")]
[JsonDerivedType(typeof(ValueExpression), typeDiscriminator: "value")]
#endif
public abstract class LogicalExpression
{
    protected ExpressionOptions _options;
    protected CultureInfo? _cultureInfo;
    protected AdvancedExpressionOptions? _advancedOptions;

    public LogicalExpression()
    {
        _options = ExpressionOptions.None;
    }

    public LogicalExpression(ExpressionOptions options, CultureInfo? cultureInfo, AdvancedExpressionOptions? advancedOptions)
    {
        SetOptions(options, cultureInfo, advancedOptions);
    }

    public LogicalExpression SetOptions(ExpressionOptions options, CultureInfo? cultureInfo, AdvancedExpressionOptions? advancedOptions)
    {
        _options = options;
        _cultureInfo = cultureInfo;
        _advancedOptions = advancedOptions;
        return this;
    }

    public override string ToString()
    {
        var serializer = new SerializationVisitor(new SerializationContext(_options, _cultureInfo, _advancedOptions));
        return Accept(serializer).TrimEnd(' ');
    }

    [Pure]
    public abstract T Accept<T>(ILogicalExpressionVisitor<T> visitor);
}