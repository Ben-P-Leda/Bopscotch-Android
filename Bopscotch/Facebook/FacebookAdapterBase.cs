using System;
using System.Collections.Generic;

using Facebook;

using Microsoft.Xna.Framework;

namespace Leda.FacebookAdapter
{
	public abstract class FacebookAdapterBase
	{
		private FacebookClient _actionClient;
		protected bool _loggedIn;

		public string ApplicationId { protected get; set; }
		public FacebookActionCallback ActionCallback { protected get; set; }
		public string AccessToken { get; set; }
		public bool IsLoggedIn { get { return _loggedIn; } }
		public ActionResult LastActionResult { get; private set; }
        public bool ActionInProgress { get; protected set; }

		public string ApplicationName { private get; set; }
		public string Caption { private get; set; }
		public string Description { private get; set; }
		public string Link { private get; set; }
		public string ImageUrl { private get; set; }

		protected bool ConnectedToNetwork { get { return true; } }

		public FacebookAdapterBase()
		{
			ActionCallback = null;
			AccessToken = string.Empty;
			LastActionResult = ActionResult.None;
            ActionInProgress = false;
		}

		protected void CompleteAction(ActionResult actionResult)
		{
			LastActionResult = actionResult;
			if (ActionCallback != null) 
            { 
                ActionCallback(actionResult); 
            }
		}

		protected void FinishLogin(string accessToken, bool manualLoginRequired)
		{
			AttemptToCreateActionClient(accessToken, manualLoginRequired, string.Empty);
		}

		public void AttemptLogin(string accessToken)
		{
			FinishLogin(accessToken, false);
		}

		private void AttemptToCreateActionClient(string accessToken, bool manualLoginRequired, string toPost)
		{
            if (!ActionInProgress)
            {
                if (!ConnectedToNetwork)
                {
                    CompleteAction(ActionResult.PostError);
                    return;
                }

                ActionInProgress = true;

                _actionClient = new FacebookClient(accessToken);
                _actionClient.GetTaskAsync("me?fields=id")
                    .ContinueWith(t =>
                        {
                            ActionInProgress = false;
                            if (t.IsFaulted)
                            {
                                CompleteAction(string.IsNullOrEmpty(toPost) ? ActionResult.PostError : ActionResult.LoginError);
                            }
                            else
                            {
                                _loggedIn = true;
                                AccessToken = accessToken;

                                if (!string.IsNullOrEmpty(toPost))
                                {
                                    AttemptPost(toPost);
                                }
                                else
                                {
                                    CompleteAction(manualLoginRequired ? ActionResult.LoginSuccessful : ActionResult.LoginAlreadyLoggedIn);
                                }
                            }
                        });
            }
		}
			
		public virtual void AttemptPost(string toPost)
		{
            if (!ActionInProgress)
            {
                if (_loggedIn)
                {
                    ActionInProgress = true;
                    _actionClient.PostTaskAsync("me/feed",
                        new
                        {
                            message = toPost,
                            name = ApplicationName,
                            caption = Caption,
                            description = Description,
                            link = Link,
                            picture = ImageUrl
                        })
                        .ContinueWith(t =>
                        {
                            ActionInProgress = false;
                            CompleteAction(t.IsFaulted ? ActionResult.PostError : ActionResult.PostSuccessful);
                        });
                }
                else if (!string.IsNullOrEmpty(AccessToken))
                {
                    AttemptToCreateActionClient(AccessToken, false, toPost);
                }
                else
                {
                    CompleteAction(ActionResult.PostNotLoggedIn);
                }
            }
		}

		public virtual void AttemptLogout()
		{
			_loggedIn = false;
			_actionClient = null;
			AccessToken = "";

			CompleteAction(ActionResult.Logout);
		}

        public virtual void AttemptGetOwnDetails()
        {
            if (!ActionInProgress)
            {
                IDictionary<string, object> result;

                if (_loggedIn)
                {
                    ActionInProgress = true;

                    _actionClient.GetTaskAsync("me?fields=first_name,last_name,link").ContinueWith(t =>
                    {
                        if (!t.IsFaulted)
                        {
                            ActionInProgress = false;

                            result = (IDictionary<string, object>)t.Result;
                            string myDetails = string.Format("Your name is: {0} {1} and your Facebook profile Url is: {3}",
                                                              (string)result["first_name"], (string)result["last_name"],
                                                              (string)result["link"]);
                            Console.WriteLine(myDetails);
                        }
                    });
                }
                else if (!string.IsNullOrEmpty(AccessToken))
                {
                    AttemptToCreateActionClient(AccessToken, false, "");
                }
                else
                {
                    CompleteAction(ActionResult.PostNotLoggedIn);
                }
            }
        }
	}
}

