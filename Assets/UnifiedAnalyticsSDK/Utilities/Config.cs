using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnifiedAnalyticsSDK.Common;
using UnifiedAnalyticsSDK.Projects;

namespace UnifiedAnalyticsSDK.Utilities
{
    /// <summary>
    /// The main configuration for the <see cref="UnifiedAnalytics"/> sdk.
    /// </summary>
    public class Config : ScriptableObject
    {
        #region serializables

        /// <summary>
        /// A simple serializable representation for <see cref="SdkSettingEntry"/> class.
        /// </summary>
        [Serializable]
        internal class SerializedSdkSettingEntry
        {
            [SerializeField] internal string Name;

            [SerializeField] internal SdkType Type;

            [SerializeField] internal List<SerializedSdkSettingProperty> Properties = new List<SerializedSdkSettingProperty>();

            [SerializeField] private List<string> propertiesNames = new List<string>();

            public void AddProperty(SdkSettingProperty property)
            {
                if (this.propertiesNames.Contains(property.Name))
                {
                    return;
                }

                var serializedProperty = new SerializedSdkSettingProperty()
                {
                    Name = property.Name,
                    Value = JsonConvert.SerializeObject(property.Value),
                };

                this.propertiesNames.Add(serializedProperty.Name);
                this.Properties.Add(serializedProperty);
            }

            public bool UpdateProperty(string propertyName, object value)
            {
                if (!this.propertiesNames.Contains(propertyName))
                {
                    return false;
                }

                var propertyIndex = this.propertiesNames.IndexOf(propertyName);
                this.Properties[propertyIndex].Value = JsonConvert.SerializeObject(value);

                return true;
            }
        }

        /// <summary>
        /// A simple serializable representation for <see cref="SdkSettingProperty"/> class.
        /// </summary>
        [Serializable]
        internal class SerializedSdkSettingProperty
        {
            [SerializeField] internal string Name;
            [SerializeField] internal string Value;
        }
         
        [SerializeField] private List<string> sdkSettingEntryNames = new List<string>();

        [SerializeField] private List<SerializedSdkSettingEntry> sdkSettingEntries = new List<SerializedSdkSettingEntry>();

        [SerializeField] private List<bool> foldoutsMap = new List<bool>();

        [SerializeField] private List<string> abTestCohorts = new List<string>();

        public bool IsCohortGroupFolded = false;

        public float BuildNumber = 0.1f;

        #endregion

        /// <summary>
        /// Gets all the registered <see cref="SdkSettingEntry"/> instances.
        /// </summary>
        public IReadOnlyCollection<SdkSettingEntry> SdkSettingEntries => this.sdkSettingEntries.Select(GetSdkSettingEntry).ToList();

        /// <summary>
        /// Gets all the defined A/B Test cohort groups.
        /// </summary>
        public IReadOnlyCollection<string> AbTestCohorts => this.abTestCohorts.ToList();

        /// <summary>
        /// Adds a new A/B test cohort group.
        /// </summary>
        /// <param name="cohortName"></param>
        /// <returns></returns>
        public bool AddCohort(string cohortName)
        {
            if (this.abTestCohorts.Contains(cohortName))
            {
                Debug.LogWarning($"Cohort group with the name {cohortName} is already added!");
                return false;
            }

            this.abTestCohorts.Add(cohortName);
            return true;
        }

        /// <summary>
        /// Adds an existing A/B test cohort group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public void RemoveCohort(string groupName)
        {
            if (!this.abTestCohorts.Contains(groupName))
            {
                return;
            }

            this.abTestCohorts.Remove(groupName);
        }

        /// <summary>
        /// Adds a new <see cref="SdkSettingEntry"/> entry if conditions are met.
        /// </summary>
        /// <param name="entryName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool AddSettingsEntry(string entryName, SdkType type)
        {
            if (string.IsNullOrEmpty(entryName))
            {
                Debug.LogError("Entry name needs to be supplied!");
                return false;
            }

            if (this.sdkSettingEntryNames.Contains(entryName))
            {
                Debug.LogError("An entry with the same name already exists!");
                return false;
            }

            if (this.sdkSettingEntryNames.Count - 1 >= 0)
            {
                var lastEntry = this.sdkSettingEntryNames.Last();
                this.ToggleEntryFoldout(lastEntry, false);
            }

            var serializedSdkSetting = GetSerializedSdkEntry(GetDefaultSdkSettingEntry(entryName, type));

            this.sdkSettingEntryNames.Add(entryName);
            this.sdkSettingEntries.Add(serializedSdkSetting);
            this.foldoutsMap.Add(true);

            return true;
        } 

        /// <summary>
        /// Updates a <see cref="SdkSettingProperty"/> from a <see cref="SdkSettingEntry"/> instance.
        /// </summary>
        /// <param name="entryName"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void UpdateSettingEntryProperty(string entryName, string propertyName, object value)
        {
            if (!this.sdkSettingEntryNames.Contains(entryName) || string.IsNullOrEmpty($"{value}"))
            {
                return;
            }

            var entryIndex = this.sdkSettingEntryNames.IndexOf(entryName);

            var settingEntry = this.sdkSettingEntries[entryIndex];
            if (!settingEntry.UpdateProperty(propertyName, value))
            {
                Debug.LogError($"Failed to update property {entryName}.{propertyName} with value {value}");
            }
             
            this.sdkSettingEntries[entryIndex] = settingEntry;
        }

        /// <summary>
        /// Removes a <see cref="SdkSettingEntry"/> instance.
        /// </summary>
        /// <param name="entryName"></param>
        public void RemoveSettingEntry(string entryName)
        {
            if (!this.sdkSettingEntryNames.Contains(entryName))
            {
                return;
            }

            var entryIndex = this.sdkSettingEntryNames.IndexOf(entryName);

            var serializedEntry = this.sdkSettingEntries[entryIndex];
            var settingEntry = GetSdkSettingEntry(serializedEntry);
            settingEntry.OnEntryDeleted();

            this.sdkSettingEntries.RemoveAt(entryIndex);
            this.sdkSettingEntryNames.Remove(entryName);
            this.foldoutsMap.RemoveAt(entryIndex);
        }

        /// <summary>
        /// Gets the foldout value for a setting entry.
        /// </summary>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public bool GetEntryFoldoutValue(string entryName)
        {
            if (string.IsNullOrEmpty(entryName))
            {
                Debug.LogError("Entry name needs to be supplied!");
                return false;
            }

            if (!this.sdkSettingEntryNames.Contains(entryName))
            {
                Debug.LogError($"Entry with name {entryName} doesn't exist!");
                return false;
            }

            var entryIndex = this.sdkSettingEntryNames.IndexOf(entryName);

            return this.foldoutsMap[entryIndex];
        }

        /// <summary>
        /// Toggle the foldout value for a setting entry.
        /// </summary>
        /// <param name="entryName"></param>
        /// <param name="isFolded"></param>
        public void ToggleEntryFoldout(string entryName, bool isFolded)
        {
            if (string.IsNullOrEmpty(entryName))
            {
                Debug.LogError("Entry name needs to be supplied!");
                return;
            }

            if (!this.sdkSettingEntryNames.Contains(entryName))
            {
                Debug.LogError($"Entry with name {entryName} doesn't exist!");
                return;
            }

            var entryIndex = this.sdkSettingEntryNames.IndexOf(entryName);
            this.foldoutsMap[entryIndex] = isFolded;
        }

        /// <summary>
        /// Gets a default instance for a <see cref="SdkSettingEntry"/> based on supplied <see cref="SdkType"/>.
        /// </summary>
        /// <param name="entryName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static SdkSettingEntry GetDefaultSdkSettingEntry(string entryName, SdkType type)
        {
            SdkSettingEntry sdkSetting;
            switch (type)
            {
                case SdkType.Test:
                    sdkSetting = new TestSdkSettingEntry(entryName);
                    break;
                case SdkType.GameAnalytics:
                    sdkSetting = new GameAnalyticsSdkSettingEntry(entryName);
                    break;
                case SdkType.Facebook: 
                    sdkSetting = new FacebookSdkSettingEntry(entryName);
                    break;
                default:
                    throw new NotSupportedException($"{type} sdk is not supported yet!");
            }

            return sdkSetting;
        }

        /// <summary>
        /// Gets a <see cref="SerializedSdkSettingEntry"/> from the supplied <see cref="SdkSettingEntry"/> instance.
        /// </summary>
        /// <param name="sdkSetting"></param>
        /// <returns></returns>
        private static SerializedSdkSettingEntry GetSerializedSdkEntry(SdkSettingEntry sdkSetting)
        {
            var serializedSettingEntry = new SerializedSdkSettingEntry
            {
                Name = sdkSetting.Name,
                Type = sdkSetting.Type,
                Properties = new List<SerializedSdkSettingProperty>()
            };

            foreach (var property in sdkSetting.Properties)
            {
                serializedSettingEntry.AddProperty(property);
            }

            return serializedSettingEntry;
        }

        /// <summary>
        /// Gets an updated <see cref="SdkSettingEntry"/> from a <see cref="SerializedSdkSettingEntry"/> instance.
        /// </summary>
        /// <param name="serializedSdkSetting"></param>
        /// <returns></returns>
        private static SdkSettingEntry GetSdkSettingEntry(SerializedSdkSettingEntry serializedSdkSetting)
        {
            var project = GetDefaultSdkSettingEntry(serializedSdkSetting.Name, serializedSdkSetting.Type);

            foreach (var serializedProperty in serializedSdkSetting.Properties)
            {
                var propertyType = project.GetPropertyType(serializedProperty.Name);

                if (propertyType == null)
                {
                    Debug.LogError($"Failed to get type for property {project.Name}.{serializedProperty.Name}");
                    continue;
                }

                var deserializedValue = JsonConvert.DeserializeObject(serializedProperty.Value, propertyType);
                project.UpdateProperty(serializedProperty.Name, deserializedValue);
            }
             
            return project;
        }

        /// <summary>
        /// Resets the config data.
        /// </summary>
        public void Reset()
        {
            foreach (var entry in this.SdkSettingEntries)
            {
                entry.ClearSdkConfig();
            }

            this.sdkSettingEntries.Clear();
            this.sdkSettingEntryNames.Clear();
            this.foldoutsMap.Clear();
        }
    } 
}