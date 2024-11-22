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
        /// ���ƴ���
        /// </summary>
        private void DrawWindow()
        {
            GUILayout.Label("���ƽ̨");
            m_BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(m_BuildTarget);

            GUILayout.Label("�����ʽ");
            m_BuildAssetOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup(m_BuildAssetOptions);


            GUILayout.Label("App�汾");
            appVersion = GUILayout.TextField(appVersion);
            GUILayout.Label("��Դ�汾");
            assetVersion = GUILayout.TextField(assetVersion);

            GUILayout.Label("���·��");
            assetBundlePath = GUILayout.TextField(assetBundlePath);

            if (GUILayout.Button("ѡ���ļ���"))
            {
                string selectPath = EditorUtility.OpenFolderPanel("���Ŀ¼", Application.dataPath, "");
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

            if (GUILayout.Button("���"))
            {
                Build();
            }

            if (GUILayout.Button("������Ŀ¼"))
            {
                ClearOutPath();
            }
        }

        /// <summary>
        /// ���
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
        /// ���Ŀ¼
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