namespace FlosonicsSession.Helpers;

public static class MagicStrings
{
    public static readonly string InternalServerError = "Internal server error, please try again";
    public static readonly string RouteIdAndSessionIdDontMatch = "The route id and the session id don't match";
    public static readonly string SessionAboutToBeUpdatedNotFound = "The session you are trying to update does not exist";
    public static readonly string NotCorrectETag = "Ensure that the session has the correct Session (ETag) Id and try again";
    public static readonly string NameExists = "A session with the same name already exists. Please choose a different name.";
    public static readonly string DeleteSessionNotFound = "The session you are trying to delete could not be found (or has been deleted)";
    public static readonly string SessionDeleted = "Session Deleted Successfully";
    public static readonly string SessionNotFound = "Session could not be found";
    public static readonly string SessionCreated = "Session Created Successfully";
}