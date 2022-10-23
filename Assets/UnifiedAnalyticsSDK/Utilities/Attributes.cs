using System;

namespace UnifiedAnalyticsSDK.Utilities
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class SdkTooltip : Attribute
    {
        public string Value { get; }
        
        public SdkTooltip(string value)
        {
            this.Value = value;
        }
    }
}