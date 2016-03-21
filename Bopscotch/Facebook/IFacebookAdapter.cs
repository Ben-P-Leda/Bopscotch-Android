using System;

namespace Leda.FacebookAdapter
{
    public interface IFacebookAdapter
    {
        string ApplicationId { set;}
        FacebookActionCallback ActionCallback { set; }
        string AccessToken { get; set; }
        bool IsLoggedIn { get; }
        ActionResult LastActionResult { get; }

        string ApplicationName { set; }
        string Caption { set; }
        string Description { set; }
        string Link { set; }
        string ImageUrl { set; }

        void AttemptLogin();
        void AttemptLogin(string accessToken);
        void AttemptGetOwnDetails();
        void AttemptPost(string toPost);
        void AttemptLogout();
    }
}