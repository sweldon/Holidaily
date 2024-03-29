﻿using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EventApp.Views;
using EventApp.Models;
using EventApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Plugin.PushNotification;
using Badge.Plugin;
using System.Diagnostics;
using System.Linq;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace EventApp
{
    public partial class App : Application
    {
        public static bool syncDeviceToken = false;
        public NavigationPage NavigationPage { get; private set; }
        public Holiday OpenHolidayPage { get; set; }
        public Comment OpenComment { get; set; }
        public static User GlobalUser = new User() { };
        public static bool NotificationsRefreshed = false;
        #if DEBUG
            #if __IOS__
                    public static string HolidailyHost = "http://localhost:8888";
            #else
                    public static string HolidailyHost = "http://10.0.2.2:8888";
            #endif
        #else
            public static string HolidailyHost = "https://holidailyapp.com";
        #endif
        public static HttpClient globalClient = new HttpClient();
        // App-wide reusable instance for choosing random ads
        public static Random randomGenerator = new Random();


        public static async void popModalIfActive(INavigation nav)
        {
            #if __IOS__
                var isModalShowing = Application.Current.MainPage?.Navigation?.ModalStack?.LastOrDefault();
                if (isModalShowing != null)
                {
                    Debug.WriteLine($"Modal closed using swipe, popping before re-opening");
                    await nav.PopModalAsync();
                }
            #endif
        }

        public static async void promptLogin(INavigation nav)
        {
            popModalIfActive(nav);
            await nav.PushModalAsync(new NavigationPage(new PortalPage()));
        }

        public int launchedCount
        {
            get { return Settings.LaunchedCount; }
            set
            {
                if (Settings.LaunchedCount == value)
                    return;
                Settings.LaunchedCount = value;
                OnPropertyChanged();
            }
        }

        public bool askedToReview
        {
            get { return Settings.AskedToReview; }
            set
            {
                if (Settings.AskedToReview == value)
                    return;
                Settings.AskedToReview = value;
                OnPropertyChanged();
            }
        }

        public bool refreshToken
        {
            get { return Settings.RefreshToken; }
            set
            {
                if (Settings.RefreshToken == value)
                    return;
                Settings.RefreshToken = value;
                OnPropertyChanged();
            }
        }

        public bool eulaAccepted
        {
            get { return Settings.EulaAccepted; }
            set
            {
                if (Settings.EulaAccepted == value)
                    return;
                Settings.EulaAccepted = value;
                OnPropertyChanged();
            }
        }

        public string devicePushId
        {
            get { return Settings.DevicePushId; }
            set
            {
                if (Settings.DevicePushId == value)
                    return;
                Settings.DevicePushId = value;
                OnPropertyChanged();
            }
        }

        public bool isActive
        {
            get { return Settings.IsActive; }
            set
            {
                if (Settings.IsActive == value)
                    return;
                Settings.IsActive = value;
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

        public string appInfo
        {
            get { return Settings.AppInfo; }
            set
            {
                if (Settings.AppInfo == value)
                    return;
                Settings.AppInfo = value;
                OnPropertyChanged();
            }
        }

        public bool isLoggedIn
        {
            get { return Settings.IsLoggedIn; }
            set
            {
                if (Settings.IsLoggedIn == value)
                    return;
                Settings.IsLoggedIn = value;
                OnPropertyChanged();
            }
        }
        public bool OpenNotifications
        {
            get { return Settings.OpenNotifications; }
            set
            {
                if (Settings.OpenNotifications == value)
                    return;
                Settings.OpenNotifications = value;
                OnPropertyChanged();
            }
        }

        public App()
        {
            InitializeComponent();
            var menuPage = new MenuPage(); // Build hamburger menu
            NavigationPage = new NavigationPage(new HolidaysPage()); // Push main logged-in page on top of stack
            var rootPage = new RootPage(); // Root handles master detail navigation
            rootPage.Master = menuPage; // Menu
            rootPage.Detail = NavigationPage; // Content
            MainPage = rootPage; // Set root to built master detail
        }
        private async void MakeUserActive()
        {
            await Task.Delay(5000);
            isActive = true;

        }
        private Dictionary<string, string>
            ConsumePush(IDictionary<string, object> pushData)
        {

            string pushType = "";
            string holidayId = "";
            string commentId = "";
            string postId = "";
            string commentUser = "";
            string unread = "";
            foreach (var data in pushData)
            {
                if (data.Key == "push_type")
                {
                    pushType = data.Value.ToString();
                }
                else if (data.Key == "comment_id")
                {
                    commentId = data.Value.ToString();
                }
                else if (data.Key == "post_id")
                {
                    postId = data.Value.ToString();
                }
                else if (data.Key == "holiday_id")
                {
                    holidayId = data.Value.ToString();
                }
                else if (data.Key == "comment_user")
                {
                    commentUser = data.Value.ToString();
                }
                else if (data.Key == "unread")
                {
                    unread = data.Value.ToString();
                }
            }
            var parsedData = new Dictionary<string, string> {
                {"push_type", pushType },
                {"holiday_id", holidayId },
                {"comment_id", commentId },
                {"post_id", postId },
                {"comment_user", commentUser },
                {"unread", unread },
            };
            return parsedData;
        }

        async void handleUpdate()
        {
            var userAlert = await Application.Current.MainPage.DisplayAlert("Holidaily Update!",
                "There is a new version of Holidaily available",
                "Update Now",
                "Later");
            if (userAlert)
            {
                #if __IOS__
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                        Xamarin.Forms.Device.OpenUri(new Uri("https://apps.apple.com/us/app/holidaily-find-holidays/id1449681401"));
                    });
                #else
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    Xamarin.Forms.Device.OpenUri(new Uri("https://play.google.com/store/apps/details?id=com.divinity.holidailyapp"));
                    });
                #endif
            }
        }
        private async void CheckForUpdates()
        {
            try
            {
                await Task.Delay(10000);
                var values = new Dictionary<string, string>{
                { "version", appInfo },
                { "check_update", "true" },
                };
                #if __IOS__
                    values["platform"] = "ios";
                #elif __ANDROID__
                    values["platform"] = "android";
                #endif
                dynamic response = await ApiHelpers.MakePostRequest(values, "user");
                bool needsUpdate = response.needs_update;
                bool forceUpdate = response.force_update;

                if (forceUpdate && needsUpdate)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.Current.MainPage = new NavigationPage(new ForceUpdatePage());
                    });
                    return;
                }

                if (needsUpdate && !forceUpdate)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        handleUpdate();
                    });
                }

                await Task.Delay(10000);
                Utils.AskForReview();
            }
            catch
            {
            }
        }

        // Note: On Android this is always invoked on deeplink b/c intent
        // comes through MainActivity
        protected override async void OnStart()
        {
            isActive = false;
            launchedCount += 1;

            CrossPushNotification.Current.OnTokenRefresh += (s, p) =>
            {
                syncDeviceToken = true;
                var token = p.Token.ToString();
                devicePushId = token;
            };

            await Task.Run(async () =>
            {
                MakeUserActive();
            });

            await Task.Run(async () =>
            {
                CheckForUpdates();
            });

            // Clear badge notifications
            CrossBadge.Current.ClearBadge();
            CrossPushNotification.Current.OnNotificationReceived += (s, p) =>
            {
                // User is active in the app
                Dictionary<string, string> pushData = ConsumePush(p.Data);
                string pushType = pushData["push_type"];
                string holidayId = pushData["holiday_id"];
                int unread = int.Parse(pushData["unread"]);
                if (pushType == "news")
                {
                    //AlertNews();
                }
                else
                {
                    MessagingCenter.Send(Application.Current, "UpdateBellCount", unread);
                }
            };
            CrossPushNotification.Current.OnNotificationOpened += (s, p) =>
            {
                // User is not in the app
                Dictionary<string, string> pushData = ConsumePush(p.Data);
                string pushType = pushData["push_type"];
                string holidayId = pushData["holiday_id"];
                string commentId = pushData["comment_id"];
                string postId = pushData["post_id"];
                string commentUser = pushData["comment_user"];
                if (pushType == "news")
                {
                    OpenNotifications = true;
                }
                else if (pushType == "holiday")
                {
                    NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                }
                else if (pushType == "comment")
                {
                    NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                    Utils.ReadNotification("comment", commentId);
                }
                else if (pushType == "post")
                {
                    NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                    Utils.ReadNotification("post", postId);
                }
                else if (pushType == "like")
                {
                    NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                    Utils.ReadNotification("like", postId);
                }
                else if (pushType == "rewards")
                {
                    NavigationPage.PushAsync(new RewardsPage());
                }
            };
            CrossPushNotification.Current.OnNotificationDeleted += (s, p) =>
            {
                // Clear badge notifications
            };

            // Logged-in global user updates, must occur immediately. Other
            // Async device token tasks that hit /users must be delayed, i.e.
            // syncUser(). Duplicate device deletion occurs in here.
            if (isLoggedIn)
            {
                try
                {
                    
                    var values = new Dictionary<string, string>{
                        { "username", currentUser },
                        { "version", appInfo }
                    };

                    #if __IOS__
                        values["platform"] = "ios";
                    #elif __ANDROID__
                        values["platform"] = "android";
                    #endif

                    var content = new FormUrlEncodedContent(values);
                    var response = await App.globalClient.PostAsync(App.HolidailyHost + "/users/", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic responseJSON = JsonConvert.DeserializeObject(responseString);
                    bool active = responseJSON.results.is_active;
                    isPremium = responseJSON.results.is_premium;
                    confettiCount = responseJSON.results.confetti;
                    App.GlobalUser.LastOnline = responseJSON.results.last_online;
                    App.GlobalUser.Approved = responseJSON.results.approved_holidays;
                    App.GlobalUser.Comments = responseJSON.results.num_comments;
                    App.GlobalUser.Confetti = responseJSON.results.confetti;
                    App.GlobalUser.Email = responseJSON.results.email;
                    App.GlobalUser.UserName = responseJSON.results.username;
                    string avatar = responseJSON.results.profile_image;
                    bool deviceActive = responseJSON.results.device_active;


                    if (!deviceActive)
                    {

                        #if __ANDROID__
                            refreshToken = true;
                        #endif

                    }

                    if (avatar != null)
                    {
                        App.GlobalUser.Avatar = avatar;
                        MessagingCenter.Send(this, "UpdateMenuProfilePicture", avatar);
                    }
                    if (!active)
                    {
                        App.Current.MainPage = new NavigationPage(new LimboPage());
                    }

                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"ERROR: {ex}");
                }
            }

            // EULA
            if (!eulaAccepted)
            {
                var acceptEULA = await Application.Current.MainPage.DisplayAlert("Welcome!",
                    "Welcome to Holidaily! You will only see this message once. " +
                    "Please view our End User License Agreement before proceeding.",
                    "Accept", "View");
                if (acceptEULA)
                {
                    eulaAccepted = true;
                }
                else
                {
                    App.Current.MainPage = new NavigationPage(new Eula());
                }
            }

            MessagingCenter.Subscribe<Application, bool>(this, "UpdateAskedToReview", (sender, val) =>
            {
                askedToReview = val;
            });
        }


        async void handleCommentResponse(string commentId, string holidayId, string commentUser)
        {
            var title = commentUser + " mentioned you!";
            var userAlert = await Application.Current.MainPage.DisplayAlert(title, "", "Go to Comment", "Close");
            if (userAlert)
            {
                await NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
                Utils.ReadNotification("comment", commentId);
            }
        }

        private void AlertUserComment(string commentId, string holidayId, string commentUser)
        {
            Device.BeginInvokeOnMainThread(() => {
                handleCommentResponse(commentId, holidayId, commentUser);
            });
        }

        async void handleNewsResponse()
        {
            var title = "Holidaily just posted an announcement";
            var newsAlert = await Application.Current.MainPage.DisplayAlert(title, "", "Read Now", "Close");
            if (newsAlert)
            {
                // Modal Async requires you to be in a content page, so use settings variable
                OpenNotifications = true;
            }
        }

        private void AlertNews()
        {
            Device.BeginInvokeOnMainThread(() => {
                handleNewsResponse();
            });
        }

        async void handleHolidayResponse(string holidayId)
        {
            var title = "Todays holidays are live!";
            var userAlert = await Application.Current.MainPage.DisplayAlert(title, "Want to see today's featured holiday?", "OK", "Close");
            if (userAlert == true)
            {
                await NavigationPage.PushAsync(new HolidayDetailPage(new HolidayDetailViewModel(holidayId)));
            }
        }
        private void AlertUserHolidays(string holidayId)
        {
            Device.BeginInvokeOnMainThread(() => {
                handleHolidayResponse(holidayId);
            });
        }

        protected override void OnSleep()
        {
            isActive = false;
        }

        protected override void OnResume()
        {
            isActive = true;
        }

    }
}
