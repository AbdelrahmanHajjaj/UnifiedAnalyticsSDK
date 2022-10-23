using System.Collections.Generic;
using Facebook.Unity;
using Facebook.Unity.Settings;
using UnifiedAnalyticsSDK.Common;
using UnityEngine;

namespace UnifiedAnalyticsSDK.Projects
{
    /// <summary>
    /// The class responsible for representing and handling the Facebook SDK
    /// </summary>
    public sealed class FacebookSdkSettingEntry : SdkSettingEntry
    {
        private const string AppNamePropertyName = "App Name (Optional)";
        private const string AppIdPropertyName = "App Id";
        private const string ClientTokenPropertyName = "Client Token (Optional)";

        /// <summary>
        /// Initializes a new instance of <see cref="FacebookSdkSettingEntry"/> class.
        /// </summary>
        /// <param name="name"></param>
        public FacebookSdkSettingEntry(string name) 
            : base(name, SdkType.Facebook)
        {
            this.InitializeDefaults(new List<SdkSettingProperty>
            {
                new SdkSettingProperty(AppNamePropertyName, string.Empty, typeof(string), true),
                new SdkSettingProperty(AppIdPropertyName, string.Empty, typeof(string)),
                new SdkSettingProperty(ClientTokenPropertyName, string.Empty, typeof(string), true),
            });
        }

        /// <summary>
        /// Gets the app name for the current <see cref="FacebookSdkSettingEntry"/> instance.
        /// </summary>
        public string AppName => this.EntryProperties[AppNamePropertyName].Value.ToString();
        
        /// <summary>
        /// Gets the app id for the current <see cref="FacebookSdkSettingEntry"/> instance.
        /// </summary>
        public string AppId => this.EntryProperties[AppIdPropertyName].Value.ToString();

        /// <summary>
        /// Gets the client token for the current <see cref="FacebookSdkSettingEntry"/> instance.
        /// </summary>
        public string ClientToken => this.EntryProperties[ClientTokenPropertyName].Value.ToString();

        /// <inheritdoc/>
        public override void Sync()
        {
            FacebookSettings.AppLabels = AddItem(FacebookSettings.AppLabels, this.AppName);
            FacebookSettings.AppIds = AddItem(FacebookSettings.AppIds, this.AppId);
            FacebookSettings.ClientTokens = AddItem(FacebookSettings.ClientTokens, this.ClientToken);

            this.SaveFacebookSettings();
        }

        /// <inheritdoc/>
        public override void OnEntryDeleted()
        {
            FacebookSettings.AppLabels = RemoveItem(FacebookSettings.AppLabels, this.AppName);
            FacebookSettings.AppIds = RemoveItem(FacebookSettings.AppIds, this.AppId);
            FacebookSettings.ClientTokens = RemoveItem(FacebookSettings.ClientTokens, this.ClientToken);

            this.SaveFacebookSettings();
        }

        /// <inheritdoc/>
        public override void InitializeSdk()
        {
            base.InitializeSdk();

            if (!FB.IsInitialized)
            {
                // Initializing Facebook.
                Debug.Log("Initializing Facebook.");
                FB.Init(this.OnFacebookInitialized);
            }
            else
            {
                this.OnFacebookInitialized();
            }
        }

        /// <inheritdoc/>
        public override void ClearSdkConfig()
        {
            base.ClearSdkConfig();

            // Clean up fb settings.
            FacebookSettings.AppIds.Clear();
            FacebookSettings.ClientTokens.Clear();
            FacebookSettings.AppLabels.Clear();
        }

        /// <summary>
        /// Triggers after the <see cref="Facebook.Unity.FB"/> is initialized.
        /// </summary>
        private void OnFacebookInitialized()
        {
            FB.ActivateApp();
            FB.Mobile.SetAutoLogAppEventsEnabled(true);
        }

        /// <summary>
        /// Saves the <see cref="FacebookSettings"/> instance after changes.
        /// </summary>
        private void SaveFacebookSettings()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(FacebookSettings.Instance);
#endif
        }

        /// <summary>
        /// Adds an item to a list.
        /// </summary>
        /// <param name="originalList"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static List<string> AddItem(List<string> originalList, string item)
        {
            if (originalList.Contains(item))
            {
                return originalList;
            }

            originalList.Add(item);

            return originalList;
        }

        /// <summary>
        /// Removes an item from a list.
        /// </summary>
        /// <param name="originalList"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static List<string> RemoveItem(List<string> originalList, string item)
        {
            if (!originalList.Contains(item))
            {
                return originalList;
            }

            originalList.Remove(item);

            return originalList;
        }
    }
}