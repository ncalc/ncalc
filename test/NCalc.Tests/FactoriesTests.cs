using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using NCalc.Cache;
using NCalc.DependencyInjection.Configuration;
using NCalc.Domain;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Visitors;

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