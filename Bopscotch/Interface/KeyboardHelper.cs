using System;
using System.Threading;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using Microsoft.Xna.Framework;

namespace Bopscotch.Interface
{
    public static class KeyboardHelper
    {
        private static bool _isVisible;

        delegate string ShowKeyboardInputDelegate(string title, string description, string defaultText);

        public static string ShowKeyboardInput(string title, string description, string defaultText)
        {
            string result = null;
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            _isVisible = true;

            Game.Activity.RunOnUiThread(() =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(Game.Activity);

                alert.SetTitle(title);
                alert.SetMessage(description);

                EditText input = new EditText(Game.Activity) 
                { 
                    Text = defaultText
                };

                alert.SetView(input);

                alert.SetPositiveButton("Ok", (dialog, whichButton) =>
                {
                    result = input.Text;
                    _isVisible = false;
                    waitHandle.Set();
                });

                alert.SetNegativeButton("Cancel", (dialog, whichButton) =>
                {
                    result = null;
                    _isVisible = false;
                    waitHandle.Set();
                });
                alert.SetCancelable(false);
                alert.Show();

            });
            waitHandle.WaitOne();
            _isVisible = false;

            return result;
        }

        public static IAsyncResult BeginShowKeyboardInput(string title, string description, string defaultText, AsyncCallback callback)
        {
            if (_isVisible)
            {
                throw new Exception("The function cannot be completed at this time: the Guide UI is already active. Wait until Guide.IsVisible is false before issuing this call.");
            }

            _isVisible = true;

            ShowKeyboardInputDelegate ski = ShowKeyboardInput;

            return ski.BeginInvoke(title, description, defaultText, callback, ski);
        }

        public static string EndShowKeyboardInput(IAsyncResult result)
        {
            try
            {
                return (result.AsyncState as ShowKeyboardInputDelegate).EndInvoke(result);
            }
            finally
            {
                _isVisible = false;
            }
        }
    }
}