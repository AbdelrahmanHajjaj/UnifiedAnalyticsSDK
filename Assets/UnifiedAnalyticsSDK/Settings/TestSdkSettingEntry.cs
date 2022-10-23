using System.Collections.Generic;
using UnifiedAnalyticsSDK.Common;

namespace UnifiedAnalyticsSDK.Projects
{
    /// <summary>
    /// A test class represents and handles the test sdk.
    /// This is a demonstration for how custom SDKs or new SDKs can be integrated in the <see cref="UnifiedAnalytics"/> sdk. 
    /// </summary>
    public sealed class TestSdkSettingEntry : SdkSettingEntry
    {
        private const string TestIdPropertyName = "Test Id";
        private const string IsEditorPropertyName = "Editor Only";

        /// <summary>
        /// Initializes a new instance of <see cref="TestSdkSettingEntry"/> class.
        /// </summary>
        /// <param name="name"></param>
        public TestSdkSettingEntry(string name) 
            : base(name, SdkType.Test)
        {
            this.InitializeDefaults(new List<SdkSettingProperty>
            {
                new SdkSettingProperty(TestIdPropertyName, string.Empty, typeof(string)),
                new SdkSettingProperty(IsEditorPropertyName, false, typeof(bool)),
            });
        }

        /// <summary>
        /// Gets the test id for the current <see cref="TestSdkSettingEntry"/> instance.
        /// </summary>
        public string TestId => this.EntryProperties[TestIdPropertyName].Value.ToString();

        /// <summary>
        /// Gets whether the setting is for editor only or not for the current <see cref="TestSdkSettingEntry"/> instance.
        /// </summary>
        public string IsEditor => this.EntryProperties[IsEditorPropertyName].Value.ToString();

        public override void Sync()
        {
            // Do the custom sync here.
            base.Sync();
        }

        public override void OnEntryDeleted()
        {
            // Do the custom deletion when editor ui updates.
            base.OnEntryDeleted();
        }
    }
}