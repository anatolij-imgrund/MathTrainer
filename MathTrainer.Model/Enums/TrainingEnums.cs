namespace MathTrainer.Model.Enums
{
    public enum OperationType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

    public enum EquationMode
    {
        ResultUnknown,          // 1 + 2 = ?
        FirstOperandUnknown,    // ? + 2 = 3
        SecondOperandUnknown    // 1 + ? = 3
    }
}