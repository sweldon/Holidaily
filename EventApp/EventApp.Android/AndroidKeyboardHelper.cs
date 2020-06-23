﻿using System.Diagnostics;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Xamarin.Forms;

namespace EventApp
{

    public class AndroidKeyboardHelper : IKeyboardHelper
    {
        public void ShowKeyboard()
        {
            var context = Forms.Context;
            var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;

            if (inputMethodManager != null && context is Activity)
            {
                var activity = context as Activity;
                var token = activity.CurrentFocus?.WindowToken;
                inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
            }
        }

        public void HideKeyboard()
        {
            var context = Forms.Context;
            var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;

            if (inputMethodManager != null && context is Activity)
            {
                var activity = context as Activity;
                var token = activity.CurrentFocus?.WindowToken;
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);

                activity.Window.DecorView.ClearFocus();
            }
        }
    }
}
