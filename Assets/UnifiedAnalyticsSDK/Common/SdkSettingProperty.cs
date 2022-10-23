using System;
using UnityEngine;

namespace UnifiedAnalyticsSDK.Common
{
    /// <summary>
    /// The class representing an sdk project property <see cref="SdkSettingEntry.Properties"/>.
    /// </summary>
    public class SdkSettingProperty
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the property.
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// Gets whether the <see cref="SdkSettingProperty"/> is optional or not.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// Gets an array of choices to supply <see cref="Value"/> from it.
        /// </summary>
        public Array ValueChoiceArray { get; }

        /// <summary>
        /// Gets whether the <see cref="ValueChoiceArray"/> has values or not (used for drop downs and value selection).
        /// </summary>
        public bool HasSelectionValues => this.ValueChoiceArray != null;

        /// <summary>
        /// Initializes a new instance of <see cref="SdkSettingProperty"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="isOptional"></param>
        /// <param name="choiceArray"></param>
        public SdkSettingProperty(string name, object value, Type type, bool isOptional = false, Array choiceArray = null)
        {
            this.Name = name;
            this.Value = value;
            this.ValueType = type;
            this.IsOptional = isOptional;
            this.ValueChoiceArray = choiceArray;
        }

        /// <summary>
        /// Updates the value of the current property.
        /// </summary>
        /// <param name="rawValue"></param>
        public void UpdateValue(object rawValue)
        {
            if (rawValue == null || string.IsNullOrEmpty($"{rawValue}"))
            {
                return;
            }

            try
            {
                var newValue = Convert.ChangeType(rawValue, this.ValueType);

                this.Value = newValue;
            }
            catch (Exception e)
            {
                Debug.LogError($"Attempted to update property {this.Name} with wrong type value!\n{e}");
            }
        }
    }
}