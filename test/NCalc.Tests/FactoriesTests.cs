using Microsoft.Extensions.DependencyInjection;
using NCalc.DependencyInjection.Configuration;
using NCalc.Factories;

namespace NCalc.Tests;

public class FactoriesFixture
{
    public IExpressionFactory ExpressionFactory { get;  }
    public ILogicalExpressionFactory LogicalExpressionFactory { get; }
    public FactoriesFixture()
    {
        var serviceProvider = new ServiceCollection()
            .AddNCalc()
            .Services.BuildServiceProvider();

        ExpressionFactory = serviceProvider.GetRequiredService<IExpressionFactory>();
        LogicalExpressionFactory = serviceProvider.GetRequiredService<ILogicalExpressionFactory>();
    }


}

[Trait("Category", "DependencyInjection")]
public class FactoriesTests(FactoriesFixture fixture) : IClassFixture<FactoriesFixture>
{
    private readonly IExpressionFactory _expressionFactory = fixture.ExpressionFactory;
    private readonly ILogicalExpressionFactory _logicalExpressionFactory = fixture.LogicalExpressionFactory;
    
    [Fact]
    public void Expression_From_Factory_Should_Evaluate()
    {
        Assert.Equal(4, _expressionFactory.Create("2+2").Evaluate());
    }
    
    [Fact]
    public void Logical_Expression_From_Factory_Should_Evaluate()
    {
        Assert.Equal(4, _expressionFactory.Create(_logicalExpressionFactory.Create("2+2")).Evaluate());
    }
}