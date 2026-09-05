namespace NCalc.Handlers;

public delegate void EvaluateBinaryHandler(BinaryEventArgs args);
public delegate Task EvaluateBinaryAsyncHandler(BinaryEventArgs args);