# ANTLR

[Antlr](https://www.antlr.org/) was the original parser of NCalc. While Antlr is a powerful and flexible parsing library, it is approximately 67% slower than [Parlot](https://github.com/sebastienros/parlot), the default parser
used by NCalc. However, this example serves to illustrate the power of DI in allowing you to easily swap out components based 
on your specific needs. See [the source code](https://github.com/ncalc/ncalc/tree/master/src/Plugins/NCalc.Antlr) for information in how to implement your own parser.

# Installation

```shell
dotnet add package NCalc.Antlr
```

At your `Program.cs`

```csharp
builder.Services.AddNCalc().WithAntlr();
```

After this, simply create expressions from <xref:NCalc.Factories.IExpressionFactory>. For more information see [this article](../dependency_injection.md).