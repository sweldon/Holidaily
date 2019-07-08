﻿// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace EventApp
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string AppInfoKey = "app_info_key";
        private static readonly string AppInfoKeyDefault = "Alpha-0.5.15";

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = "no";

        private const string DevicePushIdKey = "device_push_id_key";
        private static readonly string DevicePushIdDefault = "none";

        private const string IsLoggedInKey = "is_logged_in_key";
        private static readonly string IsLoggedInDefault= "no";

        private const string CurrentUserKey = "current_user_key";
        private static readonly string CurrentUserDefault = "none";

        private const string IsActiveKey = "is_active_key";
        private static readonly bool IsActiveDefault = false;

        #endregion

        public static bool IsActive
        {
            get
            {
                return AppSettings.GetValueOrDefault(IsActiveKey, IsActiveDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IsActiveKey, value);
            }
        }

        public static string AppInfo
        {
            get
            {
                return AppSettings.GetValueOrDefault(AppInfoKey, AppInfoKeyDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AppInfoKey, value);
            }
        }

        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SettingsKey, value);
            }
        }

        public static string DevicePushId
        {
            get
            {
                return AppSettings.GetValueOrDefault(DevicePushIdKey, DevicePushIdDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(DevicePushIdKey, value);
            }
        }

        public static string IsLoggedIn
        {
            get
            {
                return AppSettings.GetValueOrDefault(IsLoggedInKey, IsLoggedInDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IsLoggedInKey, value);
            }
        }

        public static string CurrentUser
        {
            get
            {
                return AppSettings.GetValueOrDefault(CurrentUserKey, CurrentUserDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CurrentUserKey, value);
            }
        }

    }
}
