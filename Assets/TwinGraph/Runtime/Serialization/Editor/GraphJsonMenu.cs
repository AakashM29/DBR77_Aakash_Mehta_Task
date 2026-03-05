#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using TwinGraph.Runtime.Graph;
using UnityEditor;
using UnityEngine;

namespace TwinGraph.Runtime.Serialization.Editor
{
    internal static class GraphJsonMenu
    {
        private const string ExportMenuPath = "Assets/TwinGraph/Export Selected GraphAsset To JSON";
        private const string ImportMenuPath = "Assets/TwinGraph/Import JSON Into Selected GraphAsset";
        private const string ValidateMenuPath = "Assets/TwinGraph/Validate Selected GraphAsset";

        [MenuItem(ExportMenuPath)]
        private static void ExportSelectedGraphAsset()
        {
            if (!TryGetSelectedGraphAsset(out var asset))
            {
                return;
            }

            if (!GraphValidator.Validate(asset.graph, out var errors))
            {
                EditorUtility.DisplayDialog("Graph Validation Failed", BuildErrorMessage(errors), "OK");
                return;
            }

            var defaultName = string.IsNullOrWhiteSpace(asset.name) ? "GraphAsset" : asset.name;
            var path = EditorUtility.SaveFilePanel(
                "Export Graph JSON",
                Application.dataPath,
                defaultName + ".json",
                "json"
            );

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (!GraphSerializer.Save(asset.graph, path, out var error))
            {
                EditorUtility.DisplayDialog("Export Failed", error, "OK");
                return;
            }

            Debug.Log($"[TwinGraph] Exported graph to JSON: {path}");
        }

        [MenuItem(ExportMenuPath, true)]
        private static bool CanExportSelectedGraphAsset()
        {
            return Selection.activeObject is GraphAsset;
        }

        [MenuItem(ImportMenuPath)]
        private static void ImportJsonIntoSelectedGraphAsset()
        {
            if (!TryGetSelectedGraphAsset(out var asset))
            {
                return;
            }

            var path = EditorUtility.OpenFilePanel(
                "Import Graph JSON",
                Path.GetDirectoryName(Application.dataPath) ?? Application.dataPath,
                "json"
            );

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (!GraphSerializer.Load(path, out var graph, out var loadError))
            {
                EditorUtility.DisplayDialog("Import Failed", loadError, "OK");
                return;
            }

            if (!GraphValidator.Validate(graph, out var validationErrors))
            {
                EditorUtility.DisplayDialog(
                    "Graph Validation Failed",
                    BuildErrorMessage(validationErrors),
                    "OK"
                );
                return;
            }

            asset.graph = graph;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[TwinGraph] Imported graph JSON into asset: {AssetDatabase.GetAssetPath(asset)}");
        }

        [MenuItem(ImportMenuPath, true)]
        private static bool CanImportJsonIntoSelectedGraphAsset()
        {
            return Selection.activeObject is GraphAsset;
        }

        [MenuItem(ValidateMenuPath)]
        private static void ValidateSelectedGraphAsset()
        {
            if (!TryGetSelectedGraphAsset(out var asset))
            {
                return;
            }

            if (GraphValidator.Validate(asset.graph, out var errors))
            {
                EditorUtility.DisplayDialog("Graph Validation", "Graph is valid.", "OK");
                return;
            }

            EditorUtility.DisplayDialog("Graph Validation Failed", BuildErrorMessage(errors), "OK");
        }

        [MenuItem(ValidateMenuPath, true)]
        private static bool CanValidateSelectedGraphAsset()
        {
            return Selection.activeObject is GraphAsset;
        }

        private static bool TryGetSelectedGraphAsset(out GraphAsset asset)
        {
            asset = Selection.activeObject as GraphAsset;
            if (asset != null)
            {
                return true;
            }

            EditorUtility.DisplayDialog(
                "No GraphAsset Selected",
                "Select a GraphAsset in the Project window and try again.",
                "OK"
            );
            return false;
        }

        private static string BuildErrorMessage(List<string> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return "Unknown validation error.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Graph is invalid:");
            for (var i = 0; i < errors.Count; i++)
            {
                sb.Append("- ").AppendLine(errors[i]);
            }

            return sb.ToString();
        }
    }
}
#endif
