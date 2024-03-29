﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace EventApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {

        public string devicePushId
        {
            get { return Settings.DevicePushId; }
        }

        public string appInfo
        {
            get { return Settings.AppInfo; }
        }

        public string activationToken
        {
            get { return Settings.ActivationToken; }
            set
            {
                if (Settings.ActivationToken == value)
                    return;
                Settings.ActivationToken = value;
                OnPropertyChanged();
            }
        }

        public string currentUser
        {
            get { return Settings.CurrentUser; }
            set
            {
                if (Settings.CurrentUser == value)
                    return;
                Settings.CurrentUser = value;
                OnPropertyChanged();
            }
        }

        public string confettiCount
        {
            get { return Settings.ConfettiCount; }
            set
            {
                if (Settings.ConfettiCount == value)
                    return;
                Settings.ConfettiCount = value;
                OnPropertyChanged();
            }
        }
        public bool isPremium
        {
            get { return Settings.IsPremium; }
            set
            {
                if (Settings.IsPremium == value)
                    return;
                Settings.IsPremium = value;
                OnPropertyChanged();
            }
        }

        public NavigationPage NavigationPage { get; private set; }
        public RegisterPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(100);
        }

        public async void RegisterUser(object sender, EventArgs e)
        {
            try
            {


                this.IsEnabled = false;
                RegisterButton.Text = "Registering, please wait...";
                if (!string.IsNullOrEmpty(NameEntry.Text))
                {
                    string userName = NameEntry.Text.Trim();
                    string pass = PassEntry.Text;
                    string passConfirm = PassEntryConfirm.Text;
                    string email = EmailEntry.Text;
                    string referrer = ReferringSource.SelectedIndex > -1 ?
                        ReferringSource.Items[ReferringSource.SelectedIndex] : "";

                    if (String.Equals(pass, passConfirm))
                    {

                        Regex r = new Regex("^[a-zA-Z0-9_]*$");

                        if (!r.IsMatch(userName))
                        {
                            await DisplayAlert("Error!", "Username or password invalid", "Dang");
                        }
                        else if (userName.Length > 32 || userName.Length < 3)
                        {
                            await DisplayAlert("Error!", "Your username must be between 3 and 32 characters long.", "Dang");
                        }
                        else if (!Utils.IsValidEmail(email) || email.Contains(" "))
                        {
                            await DisplayAlert("Error!", "Invalid email", "Try again");
                        }
                        else
                        {
                            var values = new Dictionary<string, string>{
                                { "username", userName },
                                { "password", pass },
                                { "email", email },
                                { "device_id", devicePushId },
                                { "version", appInfo },
                                {"referrer", referrer }
                            };

                            #if __IOS__
                                values["platform"] = "ios";
                            #elif __ANDROID__
                                values["platform"] = "android";
                            #endif

                            var content = new FormUrlEncodedContent(values);
                            var response = await App.globalClient.PostAsync(App.HolidailyHost + "/accounts/register/", content);
                            var responseString = await response.Content.ReadAsStringAsync();
                            dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                            int status = responseJSON.status;
                            string message = responseJSON.message;
                            activationToken = responseJSON.token;
                            currentUser = userName;
                            isPremium = false;
                            confettiCount = "0";
                            // Reset global user
                            Utils.ResetGlobalUser(userName);
                            if (status == 200)
                            {
                                await DisplayAlert("Activate", $"We sent an " +
                                    $"activation link to {email}", "OK");
                                await Navigation.PopModalAsync();
                            }
                            else
                            {
                                await DisplayAlert("Uh oh!", message, "Try Again");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Wait a minute...", "Your passwords didn't match.", "Try again");
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
                await DisplayAlert("Sorry!", "Something went wrong on our end. Please try again", "Try again");
            }
            RegisterButton.Text = "Register";
            this.IsEnabled = true;

        }
        async void CancelRegister(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            await Navigation.PopModalAsync();
            this.IsEnabled = true;
        }
    }
}