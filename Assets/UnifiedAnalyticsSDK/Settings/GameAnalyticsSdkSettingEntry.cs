using System;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using UnifiedAnalyticsSDK.Common;

namespace UnifiedAnalyticsSDK.Projects
{
    /// <summary>
    /// The class responsible for representing and handling game analytics sdk.
    /// </summary>
    public sealed class GameAnalyticsSdkSettingEntry : SdkSettingEntry
    {
        private const string PlatformPropertyName = "Platform";
        private const string GameKeyPropertyName = "Game Key";
        private const string SecretKeyPropertyName = "Secret Key";

        private static GameAnalyticsSDK.Setup.Settings GameAnalyticsSettings => GameAnalytics.SettingsGA;

        /// <summary>
        /// Initializes a new instance of <see cref="GameAnalyticsSdkSettingEntry"/> class.
        /// </summary>
        /// <param name="name"></param>
        public GameAnalyticsSdkSettingEntry(string name) 
            : base(name, SdkType.GameAnalytics)
        {
            this.InitializeDefaults(new List<SdkSettingProperty>
            {
                new SdkSettingProperty(PlatformPropertyName, GameAnalyticsSDK.Setup.Settings.AvailablePlatforms[0], typeof(string),
                    false, GameAnalyticsSDK.Setup.Settings.AvailablePlatforms),
                new SdkSettingProperty(GameKeyPropertyName, string.Empty, typeof(string)),
                new SdkSettingProperty(SecretKeyPropertyName, string.Empty, typeof(string)),
            });
        }

        /// <summary>
        /// Gets the <see cref="RuntimePlatform"/> for the current <see cref="GameAnalyticsSdkSettingEntry"/> instance.
        /// </summary>
        public RuntimePlatform Platform => Enum.TryParse<RuntimePlatform>(this.PlatformValue, out var platform) ? platform : default;

        /// <summary>
        /// Gets the game key for the current <see cref="GameAnalyticsSdkSettingEntry"/> instance.
        /// </summary>
        public string GameKey => this.EntryProperties[GameKeyPropertyName].Value.ToString();

        /// <summary>
        /// Gets the secret key for the current <see cref="GameAnalyticsSdkSettingEntry"/> instance.
        /// </summary>
        public string SecretKey => this.EntryProperties[SecretKeyPropertyName].Value.ToString();
        
        /// <summary>
        /// Gets the selected platform value for the current <see cref="GameAnalyticsSdkSettingEntry"/> instance.
        /// </summary>
        private string PlatformValue => this.EntryProperties[PlatformPropertyName].Value.ToString();

        /// <inheritdoc/>
        public override void Sync()
        {
            if (!this.IsPlatformRegistered())
            {
                GameAnalyticsSettings.AddPlatform(this.Platform);
            }

            var index = GameAnalyticsSettings.Platforms.IndexOf(this.Platform);

            GameAnalyticsSettings.UpdateGameKey(index, this.GameKey);
            GameAnalyticsSettings.UpdateSecretKey(index, this.SecretKey);

            this.SaveGameAnalyticsSettings();
        }

        /// <inheritdoc/>
        public override void OnEntryDeleted()
        {
            if (!this.IsPlatformRegistered())
            {
                return;
            }

            var index = GameAnalyticsSettings.Platforms.IndexOf(this.Platform);

            GameAnalyticsSettings.RemovePlatformAtIndex(index);

            this.SaveGameAnalyticsSettings();
        }

        /// <inheritdoc/>
        public override void InitializeSdk()
        {
            base.InitializeSdk();

            GameAnalytics.Initialize();

            // Configure Game Analytics Remote config
            GameAnalytics.OnRemoteConfigsUpdatedEvent += this.OnRemoteConfigsUpdatedEvent;
        }

        /// <inheritdoc/>
        public override void ClearSdkConfig()
        {
            base.ClearSdkConfig();

            // Clean up game analytics settings.
            for (var i = 0; i < GameAnalytics.SettingsGA.Platforms.Count; i++)
            {
                GameAnalytics.SettingsGA.RemovePlatformAtIndex(i);
            }
        }

        /// <summary>
        /// Sets the build for <see cref="GameAnalytics"/> all platforms.
        /// </summary>
        /// <param name="build"></param>
        public void SetBuild(string build)
        {
            GameAnalytics.SetBuildAllPlatforms(build);
        }

        /// <summary>
        /// Triggers when <see cref="GameAnalytics"/> remote config gets updated.
        /// </summary>
        private void OnRemoteConfigsUpdatedEvent()
        {
            if (!GameAnalytics.IsRemoteConfigsReady())
            {
                Debug.Log("Game analytics remote configs aren't ready.");
                return;
            }

            // TODO: Trigger custom analytics event.
        }

        /// <summary>
        /// Saves the <see cref="GameAnalytics.SettingsGA"/> instance after changes.
        /// </summary>
        private void SaveGameAnalyticsSettings()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(GameAnalyticsSettings);
#endif
        }

        /// <summary>
        /// Checks if the <see cref="Platform"/> is registered under <see cref="GameAnalytics.SettingsGA"/> or not.
        /// </summary>
        /// <returns></returns>
        private bool IsPlatformRegistered()
        {
            return GameAnalyticsSettings.Platforms.Contains(this.Platform);
        }
    }
}