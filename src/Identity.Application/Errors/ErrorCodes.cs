namespace Identity.Application.Errors;

public static class ErrorCodes
{
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string CannotLockSelf = "CANNOT_LOCK_SELF";
    public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    public const string Forbidden = "FORBIDDEN";
    public const string InternalError = "INTERNAL_ERROR";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string ValidationError = "VALIDATION_ERROR";
}
