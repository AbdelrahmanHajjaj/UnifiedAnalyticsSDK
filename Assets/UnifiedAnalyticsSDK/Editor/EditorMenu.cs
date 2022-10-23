using UnityEngine;
using UnityEditor;

namespace UnifiedAnalyticsSDK.Editor
{
    public static class EditorMenu
    {
        [MenuItem ("Unified Analytics/View Config")]
        private static void ViewConfig()
        {
            Selection.activeObject = UnifiedAnalytics.Config;
        }

        [MenuItem ("Unified Analytics/Documentation")]
        private static void ViewDocumentation()
        {
            var documentationPath = FileUtility.GetPath(Application.dataPath, UnifiedAnalytics.ConfigAssetPath, "Documentation.pdf");

            System.Diagnostics.Process.Start(documentationPath);
        }
    }
}