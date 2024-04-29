namespace DocProjDEVPLANT.Services.Utils.ResultPattern;

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSucces, Error error)
        : base(isSucces, error)
    {
        _value = value;
    }

    public TValue Value => IsSucces
        ? _value!
        : throw new InvalidOperationException("the value of failure can not be accessed");

    public static implicit operator Result<TValue>(TValue value) => new (value, true, Error.None);
}