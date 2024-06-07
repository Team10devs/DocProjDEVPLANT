namespace DocProjDEVPLANT.Services.Utils.ResultPattern;

public record Error(ErrorType Type, string Description)
{
    public static readonly Error None = new(ErrorType.None,string.Empty);
    
    public static implicit operator Result(Error error) => Result.Failure(error);
}

//here comes all error types within your application
public enum ErrorType
{
    None,
    NotFound,
    BadRequest,
}

// here comes all the errors for the student entity
// use this for error documentation
public class EntityErrors
{
    public static Error NotFound(string id) => new(ErrorType.NotFound, $"Entity with id={id} does not exists");
    public static Error EmptyField(string field) => new(ErrorType.BadRequest, $"field {field} can't be empty");
    public static Error BadRequest(string description) => new(ErrorType.BadRequest, description);
}



