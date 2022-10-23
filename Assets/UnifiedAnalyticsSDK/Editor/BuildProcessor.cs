#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace UnifiedAnalyticsSDK.Editor
{
    public class BuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!UnifiedAnalytics.ValidateConfigSettings())
            {
                throw new BuildFailedException("Config Validation Failed");
            }
        }
    }
}
#endif