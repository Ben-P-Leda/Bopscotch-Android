namespace Leda.FacebookAdapter
{
    public enum ActionResult
    {
        None,
        LoginLoadFailed,
        LoginCancelled,
        LoginSuccessful,
        LoginError,
        LoginAlreadyLoggedIn,
        PostNotLoggedIn,
        PostError,
        PostSuccessful,
        Logout
    }

    public delegate void FacebookActionCallback(ActionResult actionResult);
}

