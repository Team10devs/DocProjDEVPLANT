namespace DocProjDEVPLANT.Services.Utils.ResultPattern;

public class Result
{
    public bool IsSucces { get; }
    public bool IsFailure => !IsSucces;
    public Error Error { get; }

    protected Result(bool isSucces, Error error)
    {
        if (isSucces && error != Error.None ||
            !isSucces && error == Error.None)
        {
            throw new ArgumentException("invalid error", nameof(error));
        }

        IsSucces = isSucces;
        Error = error;
    }
    
    public static Result Succes() => new(true, Error.None);
    public static Result<TValue> Succes<TValue>(TValue value) => new(value,true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}