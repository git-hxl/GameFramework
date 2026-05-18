using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor
{
    public class BundleEditor : EditorWindow
    {
        private BuildTarget m_BuildTarget;
        private BuildAssetBundleOptions m_BuildAssetOptions;
        private string appVersion;
        private string assetVersion;
        private string assetBundlePath;

        private List<string> assetBundles = new List<string>();
        [MenuItem("Tools/Bundle")]
        public static void OpenWindow()
        {
            BundleEditor window = (BundleEditor)EditorWindow.GetWindow(typeof(BundleEditor), false, "BundleEditor");
            window.Show();
        }

        private void OnEnable()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();

            m_BuildTarget = BuildTarget.StandaloneWindows;
            m_BuildAssetOptions = BuildAssetBundleOptions.ChunkBasedCompression;
            appVersion = "1.0";
            assetVersion = "1.0.1";

            assetBundlePath = Application.streamingAssetsPath;
        }

        private void OnGUI()
        {
            DrawWindow();
        }

        /// <summary>
        /// 绘制窗口
        /// </summary>
        private void DrawWindow()
        {
            GUILayout.Label("打包平台");
            m_BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(m_BuildTarget);

            GUILayout.Label("打包方式");
            m_BuildAssetOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup(m_BuildAssetOptions);


            GUILayout.Label("App版本");
            appVersion = GUILayout.TextField(appVersion);
            GUILayout.Label("资源版本");
            assetVersion = GUILayout.TextField(assetVersion);

            GUILayout.Label("输出路径");
            assetBundlePath = GUILayout.TextField(assetBundlePath);

            if (GUILayout.Button("选择文件夹"))
            {
                string selectPath = EditorUtility.OpenFolderPanel("打包目录", Application.dataPath, "");
                assetBundlePath = selectPath;
            }

            foreach (var item in AssetDatabase.GetAllAssetBundleNames())
            {
                bool value = GUILayout.Toggle(assetBundles.Contains(item), item);

                if (value && !assetBundles.Contains(item))
                {
                    assetBundles.Add(item);
                }

                if (!value && assetBundles.Contains(item))
                {
                    assetBundles.Remove(item);
                }
            }

            if (GUILayout.Button("打包"))
            {
                Build();
            }

            if (GUILayout.Button("清除打包目录"))
            {
                ClearOutPath();
            }
        }

        /// <summary>
        /// 打包
        /// </summary>
        private void Build()
        {
            AssetBundleBuild[] builds = new AssetBundleBuild[assetBundles.Count];

            for (int i = 0; i < builds.Length; i++)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = assetBundles[i];
                build.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(build.assetBundleName);
                builds[i] = build;
            }

            string outputPath = assetBundlePath + "/" + m_BuildTarget;
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, builds, m_BuildAssetOptions, m_BuildTarget);

            //ResourceConfig abConfig = new ResourceConfig();
            //abConfig.Bundles = new Dictionary<string, string>();
            //foreach (var bundle in manifest.GetAllAssetBundles())
            //{
            //    abConfig.Bundles[bundle] = manifest.GetAssetBundleHash(bundle).ToString();
            //}
            //abConfig.AssetVersion = abToolConfig.AssetVersion;
            //abConfig.AppVersion = abToolConfig.AppVersion;
            //abConfig.DateTime = DateTime.Now.ToString();
            //string configJson = JsonConvert.SerializeObject(abConfig, Formatting.Indented);

            //File.WriteAllText(outputPath + "/ResourceConfig.json", configJson);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 清除目录
        /// </summary>
        private void ClearOutPath()
        {
            string outputPath = assetBundlePath + "/" + m_BuildTarget;
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);

                AssetDatabase.Refresh();

            }
        }
    }
}