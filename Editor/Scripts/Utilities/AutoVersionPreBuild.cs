using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VoyageForge.Depot.Editor.Utilities
{
    public class AutoVersionPreBuild : IPreprocessBuildWithReport
    {
        private const string ToggleMenuPath = "VoyageForge/Depot/Auto Version";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = DepotProjectSettings.instance;
            settings.EnsureSaved();

            if (!settings.AutoVersionEnabled)
            {
                Debug.Log("[AutoVersion] Auto increment disabled. Skipping version update.");
                return;
            }

            string version = PlayerSettings.bundleVersion;
            Debug.Log("Current Version: " + version);

            string[] parts = version.Split('.');
            int major = ParseVersionPart(parts, 0);
            int minor = ParseVersionPart(parts, 1);
            int patch = ParseVersionPart(parts, 2);
            int incrementStep = settings.AutoVersionIncrementStep;

            patch += incrementStep;
            string newVersion = $"{major}.{minor}.{patch}";

            PlayerSettings.bundleVersion = newVersion;

            if (report.summary.platform == BuildTarget.Android)
            {
                PlayerSettings.Android.bundleVersionCode += incrementStep;
                Debug.Log("Android VersionCode: " + PlayerSettings.Android.bundleVersionCode);
            }

            Debug.Log($"[AutoVersion] Updated Version: {newVersion} (step: {incrementStep})");
        }

        private static int ParseVersionPart(string[] parts, int index)
        {
            if (parts == null || index >= parts.Length)
            {
                return 0;
            }

            return int.TryParse(parts[index], out int value) ? value : 0;
        }

        [MenuItem(ToggleMenuPath)]
        private static void ToggleAutoIncrement()
        {
            var settings = DepotProjectSettings.instance;
            settings.EnsureSaved();
            settings.AutoVersionEnabled = !settings.AutoVersionEnabled;
            Debug.Log($"[AutoVersion] Auto increment {(settings.AutoVersionEnabled ? "enabled" : "disabled")}.");
        }

        [MenuItem(ToggleMenuPath, true)]
        private static bool ToggleAutoIncrementValidate()
        {
            var settings = DepotProjectSettings.instance;
            settings.EnsureSaved();
            Menu.SetChecked(ToggleMenuPath, settings.AutoVersionEnabled);
            return true;
        }
    }
}
