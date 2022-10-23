using System;
using System.Collections.Generic;
using UnifiedAnalyticsSDK.Common;
using UnifiedAnalyticsSDK.Utilities;
using UnityEditor;
using UnityEngine;

namespace UnifiedAnalyticsSDK.Editor
{
    /// <summary>
    /// The <see cref="CustomEditor"/> for <see cref="Config"/> instance.
    /// </summary>
    [CustomEditor(typeof(Config))]
    public class ConfigEditor : UnityEditor.Editor
    {
        private int selectedSdkSettingIndex = 0;
        
        private string tempSdkEntryName = string.Empty;

        private string tempCohortName = string.Empty;

        private readonly List<string> settingPropertiesPaths = new List<string>();

        private readonly List<object> settingPropertiesValues = new List<object>();

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            var config = (Config)this.target;

            // Header
            GUILayout.Label("Unified Analytics SDK Config", EditorStyles.largeLabel);

            LayoutBreak(new Color(0.35f, 0.35f, 0.35f));

            // Config visualization
            GUILayout.Label("SDKs Settings Entries", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUI.indentLevel++;

            this.VisualizeConfig(ref config);

            EditorGUI.indentLevel--;

            // Add new entries visualization.
            this.VisualizeAddSettingsEntry(ref config);

            LayoutBreak(new Color(0.35f, 0.35f, 0.35f));

            // AB Test cohorts visualization
            this.VisualizeCohortGroup(ref config);

            LayoutBreak(new Color(0.35f, 0.35f, 0.35f));

            // Build number visualization.
            this.VisualizeBuildNumberField(ref config);

            this.VisualizeSyncConfig();

            if (!GUI.changed)
            {
                return;
            }

            // Apply modification on gui change.
            EditorUtility.SetDirty(config);
            this.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Creates a visualization for the <see cref="Config"/> instance.
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void VisualizeConfig(ref Config config)
        {
            foreach (var settingEntry in UnifiedAnalytics.Config.SdkSettingEntries)
            {
                var propertyInfo = settingEntry.GetType().GetProperty(nameof(SdkSettingEntry.Name)) ??
                                   throw new ArgumentNullException(nameof(SdkSettingEntry.Name));
                var tooltip = (SdkTooltip)Attribute.GetCustomAttribute(propertyInfo, typeof(SdkTooltip));

                var isFolded = EditorGUILayout.Foldout(config.GetEntryFoldoutValue(settingEntry.Name),
                    new GUIContent($"[{settingEntry.Type}] => {settingEntry.Name}", tooltip.Value));

                config.ToggleEntryFoldout(settingEntry.Name, isFolded);

                if (!isFolded)
                {
                    continue;
                }

                this.VisualizeSettingsProperties(settingEntry);

                this.VisualizeRemoveSettingsEntry(ref config, settingEntry);

                LayoutBreak(new Color(0.35f, 0.35f, 0.35f));
                GUILayout.Space(5);
            }
        }

        /// <summary>
        /// Creates a visualization for the <see cref="Config.SdkSettingEntries"/>.
        /// </summary>
        /// <param name="settingEntry"></param>
        private void VisualizeSettingsProperties(SdkSettingEntry settingEntry)
        {
            foreach (var property in settingEntry.Properties)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent($"{property.Name}"), EditorStyles.boldLabel, GUILayout.MaxWidth(100));

                var propertyKey = $"{settingEntry.Name}.{property.Name}";

                if (!this.settingPropertiesPaths.Contains(propertyKey))
                {
                    this.settingPropertiesPaths.Add(propertyKey);
                    this.settingPropertiesValues.Add(Convert.ChangeType(property.Value, property.ValueType));
                }

                try
                {
                    var propertyValueIndex = this.settingPropertiesPaths.IndexOf(propertyKey);
                    this.UpdateSettingsProperty(property, propertyValueIndex);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error when updating config settings of type: {settingEntry.Type}.\n{e}");
                }

                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Updates the specified <see cref="SdkSettingProperty"/> based on it's type and specified index.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyValueIndex"></param>
        /// <exception cref="NotSupportedException"></exception>
        private void UpdateSettingsProperty(SdkSettingProperty property, int propertyValueIndex)
        {
            if (property.HasSelectionValues)
            {
                var selectedIndex = EditorGUILayout.Popup(
                    GetChoiceIndexFromArray(this.settingPropertiesValues[propertyValueIndex], property.ValueChoiceArray),
                    GetChoicesFromArray(property.ValueChoiceArray));

                this.settingPropertiesValues[propertyValueIndex] =
                    property.ValueChoiceArray.GetValue(selectedIndex);
            }
            else
            {
                if (property.ValueType == typeof(string))
                {
                    this.settingPropertiesValues[propertyValueIndex] =
                        EditorGUILayout.TextField($"{this.settingPropertiesValues[propertyValueIndex]}");
                }
                else if (property.ValueType == typeof(bool))
                {
                    this.settingPropertiesValues[propertyValueIndex] =
                        EditorGUILayout.Toggle((bool)this.settingPropertiesValues[propertyValueIndex]);
                }
                else if (property.ValueType.IsEnum)
                {
                    this.settingPropertiesValues[propertyValueIndex] = EditorGUILayout.Popup(
                        (int)this.settingPropertiesValues[propertyValueIndex], GetEnumNames(property.ValueType));
                }
                else if (property.HasSelectionValues)
                {
                }
                else
                {
                    throw new NotSupportedException($"Property type {property.ValueType} in settings is not supported");
                }
            }
        }

        /// <summary>
        /// Creates a visualization for removing an existing entry from <see cref="Config.SdkSettingEntries"/>.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="settingEntry"></param>
        private void VisualizeRemoveSettingsEntry(ref Config config, SdkSettingEntry settingEntry)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Remove Setting Entry"))
            {
                config.RemoveSettingEntry(settingEntry.Name);
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a visualization for adding a new entry to <see cref="Config.SdkSettingEntries"/>.
        /// </summary>
        /// <param name="config"></param>
        private void VisualizeAddSettingsEntry(ref Config config)
        {
            GUILayout.BeginVertical();

            this.selectedSdkSettingIndex = EditorGUILayout.Popup("SDK to Add", this.selectedSdkSettingIndex,
                GetEnumNames(typeof(SdkType)));

            this.tempSdkEntryName = EditorGUILayout.TextField("Setting entry name", this.tempSdkEntryName);

            if (GUILayout.Button("Add Setting Entry"))
            {
                Enum.TryParse<SdkType>($"{this.selectedSdkSettingIndex}", out var type);

                if (!string.IsNullOrEmpty(this.tempSdkEntryName))
                {
                    config.AddSettingsEntry(this.tempSdkEntryName, type);
                }

                this.selectedSdkSettingIndex = 0;
                this.tempSdkEntryName = string.Empty;
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Creates a visualization for the <see cref="Config.AbTestCohorts"/>.
        /// </summary>
        /// <param name="config"></param>
        private void VisualizeCohortGroup(ref Config config)
        {
            GUILayout.Space(5);

            config.IsCohortGroupFolded = EditorGUILayout.Foldout(config.IsCohortGroupFolded, new GUIContent("[A/B Test Cohorts]"));

            if (!config.IsCohortGroupFolded)
            {
                return;
            }

            foreach (var cohortGroup in config.AbTestCohorts)
            {
                GUILayout.BeginHorizontal();

                EditorGUILayout.SelectableLabel(cohortGroup, EditorStyles.linkLabel);

                if (GUILayout.Button("Remove"))
                {
                    config.RemoveCohort(cohortGroup);
                }

                GUILayout.EndHorizontal();
            }

            this.tempCohortName = EditorGUILayout.TextField("Cohort Name", this.tempCohortName);
            if (!GUILayout.Button("Add Cohort") || string.IsNullOrEmpty(this.tempCohortName))
            {
                return;
            }

            config.AddCohort(this.tempCohortName);
            this.tempCohortName = string.Empty;
        }

        /// <summary>
        /// Creates a visualization for the <see cref="Config.BuildNumber"/>.
        /// </summary>
        /// <param name="config"></param>
        private void VisualizeBuildNumberField(ref Config config)
        {
            GUILayout.Space(5);

            config.BuildNumber = EditorGUILayout.FloatField(
                new GUIContent("Build Number", "This the build number that will be synced with analytics SDKs"),
                config.BuildNumber, EditorStyles.boldLabel);

            LayoutBreak(new Color(0.35f, 0.35f, 0.35f));
            GUILayout.Space(20);
        }

        /// <summary>
        /// Creates a visualization for the <see cref="UnifiedAnalytics.SyncSettings"/>.
        /// </summary>
        private void VisualizeSyncConfig()
        {
            if (!GUILayout.Button("Save & Sync", GUILayout.Height(50)))
            {
                return;
            }

            foreach (var propertyPath in this.settingPropertiesPaths)
            {
                var valueIndex = this.settingPropertiesPaths.IndexOf(propertyPath);
                var settingEntryName = propertyPath.Split('.')[0];
                var propertyName = propertyPath.Split('.')[1];
                var propertyValue = this.settingPropertiesValues[valueIndex];

                UnifiedAnalytics.Config.UpdateSettingEntryProperty(settingEntryName, propertyName, propertyValue);
            }

            this.settingPropertiesPaths.Clear();
            this.settingPropertiesValues.Clear();

            UnifiedAnalytics.SyncSettings();
        }

        /// <summary>
        /// Gets the index of the specified object/choice from the specified array.
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        private static int GetChoiceIndexFromArray(object choice, Array array)
        {
            var index = 0;
            for (var i = 0; i < array.Length; i++)
            {
                var value = array.GetValue(i);

                if (!value.Equals(choice))
                {
                    continue;
                }

                index = i;
                break;
            }

            return index;
        }

        /// <summary>
        /// Gets an array of string representation or names from the specified array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private static string[] GetChoicesFromArray(Array array)
        {
            var result = new string[array.Length];

            for (var i = 0; i < array.Length; i++)
            {
                result[i] = $"{array.GetValue(i)}";
            }

            return result;
        }

        /// <summary>
        /// Gets the names of the specified enum.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string[] GetEnumNames(Type type)
        {
            if (type.IsEnum)
            {
                return Enum.GetNames(type);
            }

            Debug.LogError($"Type {type} is not an enum!");
            return Array.Empty<string>();
        }

        /// <summary>
        /// Creates a visual break (similar to line break) in inspector gui.
        /// </summary>
        /// <param name="rgb"></param>
        /// <param name="thickness"></param>
        /// <param name="margin"></param>
        private static void LayoutBreak(Color rgb, float thickness = 1, int margin = 0)
        {
            var splitter = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.whiteTexture
                },
                stretchWidth = true,
                margin = new RectOffset(margin, margin, 7, 7)
            };

            var position = GUILayoutUtility.GetRect(GUIContent.none, splitter, GUILayout.Height(thickness));

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var restoreColor = GUI.color;
            GUI.color = rgb;
            splitter.Draw(position, false, false, false, false);
            GUI.color = restoreColor;
        }
    }
}