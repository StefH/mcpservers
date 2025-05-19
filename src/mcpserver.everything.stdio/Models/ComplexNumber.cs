namespace ModelContextProtocolServer.Everything.Stdio.Models;

public record ComplexNumber(double Real, double Imaginary)
{
    public override string ToString() => $"{Real} {(Imaginary < 0 ? "-" : "+")} {Math.Abs(Imaginary)}i";
}