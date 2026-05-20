namespace Krepim.SharedKernel.Results.Interfaces
{
    public interface IResult
    {
        bool IsFailure { get; }
        Error Error { get; }
    }
}
