using System;
using System.Collections.Generic;
using UnifiedAnalyticsSDK.Utilities;
using UnityEngine;

namespace UnifiedAnalyticsSDK.Common
{
    [Serializable]
    public abstract class SdkSettingEntry
    {
        /// <summary>
        /// Gets the name of of the <see cref="SdkSettingEntry"/> instance.
        /// </summary>
        [SdkTooltip("The name of an sdk setting entry.")]
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="SdkType"/> of the <see cref="SdkSettingEntry"/> instance.
        /// </summary>
        [SdkTooltip("The type of the sdk setting entry.")]
        public SdkType Type { get; }

        /// <summary>
        /// Gets a list of <see cref="SdkSettingProperty"/> for the <see cref="SdkSettingEntry"/> instance.
        /// </summary>
        [SdkTooltip("The properties of the sdk setting entry.")]
        public IReadOnlyCollection<SdkSettingProperty> Properties => this.EntryProperties.Values;

        /// <summary>
        /// Gets this entry properties.
        /// </summary>
        protected Dictionary<string, SdkSettingProperty> EntryProperties { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SdkSettingEntry"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        protected SdkSettingEntry(string name, SdkType type)
        {
            this.Name = name;
            this.Type = type;
            this.EntryProperties = new Dictionary<string, SdkSettingProperty>();
        }

        /// <summary>
        /// Initializes the default properties.
        /// </summary>
        /// <param name="defaultProperties"></param>
        protected virtual void InitializeDefaults(IEnumerable<SdkSettingProperty> defaultProperties)
        {
            this.EntryProperties.Clear();
            foreach (var defaultProperty in defaultProperties)
            {
                this.EntryProperties.Add(defaultProperty.Name, defaultProperty);
            }
        }

        /// <summary>
        /// Updates the specified property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool UpdateProperty(string name, object value)
        {
            if (string.IsNullOrEmpty(name) || !this.EntryProperties.ContainsKey(name))
            { 
                Debug.LogError($"Property {name} doesn't exist in project {this.Name}");
                return false;
            }

            this.EntryProperties[name].UpdateValue(value);
            return true;
        }

        /// <summary>
        /// Gets the type of an underlying property if exists.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public Type GetPropertyType(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && this.EntryProperties.ContainsKey(propertyName))
            {
                return this.EntryProperties[propertyName].ValueType;
            }

            Debug.LogError($"Property {propertyName} doesn't exist in project {this.Name}");
            return null;
        }

        /// <summary>
        /// Syncs the <see cref="SdkSettingEntry"/> data with the internal sdk representation.
        /// </summary>
        public virtual void Sync()
        {
        }

        /// <summary>
        /// Trigger when an <see cref="SdkSettingEntry"/> instance is deleted.
        /// </summary>
        public virtual void OnEntryDeleted()
        {
        }

        /// <summary>
        /// Initializes the original sdk.
        /// </summary>
        public virtual void InitializeSdk()
        {
        }

        /// <summary>
        /// Clears the original sdk config.
        /// </summary>
        public virtual void ClearSdkConfig()
        {
        }

        /// <summary>
        /// Validates properties and check if they are supplied valid values or not.
        /// </summary>
        /// <returns></returns>
        public bool ValidateProperties()
        {
            foreach (var property in this.EntryProperties.Values)
            {
                if (property.IsOptional)
                {
                    continue;
                }

                if (!IsNullOrDefault(property.Value, property.ValueType))
                {
                    continue;
                }

                Debug.LogError(
                    $"Failed to validate mandatory property {property.Name}! Please supply a valid value.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if an object is of the specified type and not null or default value (has a valid value).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsNullOrDefault(object value, Type type)
        {
            if (value == null)
            {
                return true;
            }

            if (type == typeof(string))
            {
                return string.IsNullOrEmpty($"{value}");
            }

            if (type == typeof(bool))
            {
                return false;
            }

            if (object.Equals(value, Activator.CreateInstance(type)))
            {
                return true;
            }

            if (Nullable.GetUnderlyingType(type) != null)
            {
                return false;
            }

            var valueType = value.GetType();
            if (!valueType.IsValueType || valueType == type)
            {
                return false;
            }

            var obj = Activator.CreateInstance(valueType);

            return obj.Equals(value);
        }
    }
}