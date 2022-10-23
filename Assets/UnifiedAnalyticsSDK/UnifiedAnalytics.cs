#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Linq;
using UnifiedAnalyticsSDK.Utilities;
using UnifiedAnalyticsSDK.Projects;

namespace UnifiedAnalyticsSDK
{
    /// <summary>
    /// The unified analytics sdk controller and api.
    /// This SDK is responsible for tracking App installs (using facebook sdk) and level progression (using game analytics sdk),
    /// The sdk is able to sync settings and configurations with supported SDKs (Currently supported Facebook and GameAnalytics).
    /// NOTE: Make sure to have  the Game analytics and facebook sdk configured in the <see cref="UnifiedAnalyticsSDK.Utilities.Config"/>.
    /// </summary>
    [ExecuteInEditMode]
    public class UnifiedAnalytics : Singleton<UnifiedAnalytics>
    {
        #region Core

        public const string ConfigAssetPath = "UnifiedAnalyticsSDK";

        private const string AbTestCohortPrefKey = "UnifiedAnalytics.AB_TestCohortName";

        /// <summary>
        /// The <see cref="UnifiedAnalyticsSDK.Utilities.Config"/> instance.
        /// </summary>
        private Config config;

        /// <summary>
        /// The <see cref="UnifiedAnalyticsTracker"/> instance.
        /// </summary>
        private UnifiedAnalyticsTracker tracker;

        private bool isInitialized = false;

        /// <inheritdoc/>
        protected override void InitSingleton()
        {
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(this.gameObject);
            }

            this.InitializeConfig();

            if (this.config == null)
            {
                Debug.LogError("Failed to Initialize Config");
            }

            this.tracker = new UnifiedAnalyticsTracker();
        }

        /// <summary>
        /// Initializes the <see cref="UnifiedAnalyticsSDK.Utilities.Config"/> instance.
        /// </summary>
        private void InitializeConfig()
        {
            try
            {
                this.config = Resources.Load<Config>($"{ConfigAssetPath}/{nameof(Config)}");

#if UNITY_EDITOR
                if (this.config != null)
                {
                    return;
                }

                const string resourcePath = "Resources";

                var path = FileUtility.GetPath("Assets", resourcePath, ConfigAssetPath, $"{nameof(Config)}.asset");

                FileUtility.CreateDirectory(Application.dataPath, resourcePath, ConfigAssetPath);

                if (FileUtility.FileExists(path))
                {
                    Debug.LogWarning("Found an existing config when attempting to create new one.\n Deleting old config!");

                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.Refresh();
                }

                var asset = ScriptableObject.CreateInstance<Config>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.Refresh();

                AssetDatabase.SaveAssets();
                Selection.activeObject = asset;

                this.config = asset;
#endif
            }
            catch (Exception e)
            {
                Debug.Log($"Error getting config {e.Message}");
            }
        }

        /// <summary>
        /// Initializes the sdk internally.
        /// </summary>
        /// <param name="initializeConfigEntries">Indicates whether the sdk should initialize the other SDKs internally based on the config entries or not.</param>
        private void InitializeSdk(bool initializeConfigEntries = true)
        {
            if (this.isInitialized)
            {
                Debug.Log("Unified analytics SDK is already initialized!");
                return;
            }

            Debug.Log("Initializing Unified analytics SDK...");

            if (initializeConfigEntries)
            {
                foreach (var entry in this.config.SdkSettingEntries)
                {
                    if (entry.Type == Common.SdkType.GameAnalytics)
                    {
                        var gameAnalyticsEntry = (GameAnalyticsSdkSettingEntry)entry;
                        gameAnalyticsEntry.SetBuild($"{this.config.BuildNumber}{this.GetClientTestCohort()}");
                    }

                    entry.InitializeSdk();
                }
            }

            this.isInitialized = true;
        }

        /// <summary>
        /// Gets The test cohort name assigned for the client instance (Persists between sessions).
        /// </summary>
        /// <returns></returns>
        private string GetClientTestCohort()
        {
            if (PlayerPrefs.HasKey(AbTestCohortPrefKey))
            {
                return PlayerPrefs.GetString(AbTestCohortPrefKey, string.Empty);
            }

            var randomCohort = this.GetRandomTestCohort();

            PlayerPrefs.SetString(AbTestCohortPrefKey, randomCohort);

            return randomCohort;
        }

        /// <summary>
        /// Gets a random test cohort from the defined <see cref="UnifiedAnalyticsSDK.Utilities.Config.AbTestCohorts"/>
        /// if the <see cref="AbTestCohortPrefKey"/> didn't exist, that means this is the first run for the game for this user.
        /// </summary>
        /// <returns></returns>
        private string GetRandomTestCohort()
        {
            if (this.config.AbTestCohorts.Count <= 0)
            {
                return string.Empty;
            }

            var index = new System.Random().Next(0, this.config.AbTestCohorts.Count);

            return this.config.AbTestCohorts.ToList()[index];
        }

        private void OnApplicationPause(bool pause)
        {
            if (Application.isEditor)
            {
                return;
            }

            if (pause)
            {
                Debug.Log("Application was paused.");
                this.isInitialized = false;
                return;
            }

            Debug.Log("Re-Initializing after app resume.");
            this.InitializeSdk();
        }

        #endregion

        #region EditorOnly
#if UNITY_EDITOR
        /// <summary>
        /// Syncs the <see cref="UnifiedAnalyticsSDK.Utilities.Config"/> data with all of the supported SDKs.
        /// </summary>
        public static void SyncSettings()
        {
            if (!UnifiedAnalytics.ValidateConfigSettings())
            {
                return;
            }

            // Sync the settings internally for each setting entry in the config.
            foreach (var sdkSettingEntry in Config.SdkSettingEntries)
            {
                sdkSettingEntry.ClearSdkConfig();
                sdkSettingEntry.Sync();
            }
        }

        /// <summary>
        /// Validates the <see cref="UnifiedAnalytics.Config"/> properties.
        /// </summary>
        /// <returns></returns>
        public static bool ValidateConfigSettings()
        {
            foreach (var settingEntry in UnifiedAnalytics.Config.SdkSettingEntries)
            {
                if (!settingEntry.ValidateProperties())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates the configuration settings upon entering play mode.
        /// TODO: This might not really be required since, GameAnalytics and fb won't be initialized on editor anyway.
        /// </summary>
        [InitializeOnEnterPlayMode]
        public static void ValidateOnEnterPlayMode()
        {
            if (!Application.isEditor || UnifiedAnalytics.ApplicationIsQuitting)
            {
                return;
            }

            if (!UnifiedAnalytics.ValidateConfigSettings())
            {
                EditorApplication.isPlaying = false;
            }
        }
#endif
        #endregion

        #region PublicAPi

        /// <summary>
        /// Gets the <see cref="UnifiedAnalyticsSDK.Utilities.Config"/> instance.
        /// </summary>
        public static Config Config => Instance.config;

        /// <summary>
        /// Gets the <see cref="UnifiedAnalyticsTracker"/> instance.
        /// </summary>
        public static UnifiedAnalyticsTracker Tracker => Instance.tracker;

        /// <summary>
        /// Gets the assigned test cohort for this client (Persisted between session).
        /// </summary>
        public static string ClientTestCohort => Instance.GetClientTestCohort();

        /// <summary>
        /// Initializes the <see cref="UnifiedAnalytics"/>.
        /// [Pre-Condition] Make sure to have the <see cref="GameAnalyticsSDK.Events.GA_SpecialEvents"/> GameObject exists in the scene.
        /// </summary>
        /// <param name="initializeConfigEntries">Indicates whether the sdk should initialize the other SDKs internally based on the config entries or not.</param>
        public static void Initialize(bool initializeConfigEntries = true)
        {
            Instance.InitializeSdk(initializeConfigEntries);
        }

        #endregion
    }
}