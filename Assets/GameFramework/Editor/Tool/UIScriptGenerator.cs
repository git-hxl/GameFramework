using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor
{
    public class UIScriptGenerator
    {
        private enum UIElementType
        {
            TextMeshProUGUI,
            Text,
            RawImage,
            Button,
            Toggle,
            Slider,
            Scrollbar,
            ScrollRect,
            Dropdown,
            InputField,
            Image,
            RectTransform
        }

        private const string EditorToolName = "Assets/UI 生成UIPanel.cs";
        private const char tag = '_';

        private static string GetVarName(string typeName)
        {
            UIElementType uIElementType = Enum.Parse<UIElementType>(typeName);

            switch (uIElementType)
            {
                case UIElementType.TextMeshProUGUI:
                    return "Txt";
                case UIElementType.Text:
                    return "Txt";
                case UIElementType.RawImage:
                    return "RImg";
                case UIElementType.Button:
                    return "Bt";
                case UIElementType.Toggle:
                    return "Tg";
                case UIElementType.Scrollbar:
                    return "Scr";
                case UIElementType.ScrollRect:
                    return "ScrR";
                case UIElementType.Dropdown:
                    return "Drop";
                case UIElementType.InputField:
                    return "Input";
                case UIElementType.Image:
                    return "Img";
                case UIElementType.RectTransform:
                    return "Rect";
                case UIElementType.Slider:
                    return "Slider";
                default:
                    return "";
            }
        }

        [MenuItem(EditorToolName, true)]
        private static bool ValidateFunc()
        {
            GameObject obj = Selection.activeGameObject;
            return (obj != null && obj.GetComponentInChildren<Canvas>() != null);
        }

        [MenuItem(EditorToolName)]
        public static void Create()
        {
            GameObject root = Selection.activeGameObject;
            string path = AssetDatabase.GetAssetPath(root);
            string scriptName = root.name.Replace(" ", "");
            string directory = Path.GetDirectoryName(path);

            string filePath = $"./{directory}/{scriptName}.cs";

            Debug.Log("生成脚本路径：" + filePath);

            StringBuilder scriptStr = new StringBuilder();

            string[] filesGUIDs = AssetDatabase.FindAssets($"{scriptName} t:Script");

            if (filesGUIDs != null && filesGUIDs.Length > 0)
            {
                filePath = AssetDatabase.GUIDToAssetPath(filesGUIDs[0]);
            }

            bool overrideFile = false;

            if (File.Exists(filePath))
            {
                if (EditorUtility.DisplayDialog("生成UI脚本", "已存在同名类,是否覆盖自动生成部分？", "确定", "取消"))
                {
                    overrideFile = true;
                }
                else
                {
                    return;
                }
            }

            if (overrideFile)
            {
                string curScriptStr = File.ReadAllText(filePath);

                string startTag = "\t#region 脚本工具生成的代码\r\n";
                string endTag = "    #endregion\r\n";
                int startIndex = curScriptStr.IndexOf(startTag);
                int endIndex = curScriptStr.IndexOf(endTag);

                if (startIndex == -1 || endIndex == -1)
                {
                    Debug.LogError("生成脚本失败");
                    return;
                }

                string replaceStr = curScriptStr.Substring(startIndex + startTag.Length, endIndex - startIndex - startTag.Length);

                curScriptStr = curScriptStr.Replace(replaceStr, CreatVariables(root));

                scriptStr.Append(curScriptStr);
            }
            else
            {

                scriptStr.Append("using UnityEngine;\r\n");
                scriptStr.Append("using UnityEngine.UI;\r\n");
                scriptStr.Append("using TMPro;\r\n");
                scriptStr.Append("using GameFramework;\r\n");

                scriptStr.Append("public class " + scriptName + " : MonoBehaviour\r\n");
                scriptStr.Append("{\r\n");

                scriptStr.Append("\t#region 脚本工具生成的代码\r\n");

                scriptStr.Append(CreatVariables(root));

                scriptStr.Append("\t#endregion\r\n");

                scriptStr.Append("}\r\n");
            }


            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            {
                byte[] data = Encoding.UTF8.GetBytes(scriptStr.ToString());
                stream.Write(data, 0, data.Length);
            }
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 创建变量
        /// </summary>
        /// <param name="obj"></param>
        public static string CreatVariables(GameObject obj)
        {
            StringBuilder scriptStr = new StringBuilder();

            Transform[] transforms = obj.GetComponentsInChildren<Transform>(true);
            foreach (var item in transforms)
            {
                if (item.name[0] != tag)
                    continue;
                foreach (var type in Enum.GetNames(typeof(UIElementType)))
                {
                    Component component = item.GetComponent(type);
                    if (component != null)
                    {
                        string typeName = component.GetType().Name;

                        string varName = StringUtil.ToAlphaNumber(component.gameObject.name).ToLower();

                        string attrName = $"{GetVarName(type)}_{varName}";

                        string path = PathUtil.GetRouteNoRoot(component.transform);

                        scriptStr.Append($"\tprivate {typeName} {varName};\r\n");

                        scriptStr.Append($"\tpublic {typeName} {attrName} {{ get {{ if ({varName} == null) {{ {varName} = transform.Find(\"{path}\").GetComponent<{typeName}>(); }} return {varName}; }} }} \r\n");
                        break;
                    }
                }
            }
            return scriptStr.ToString();
        }
    }
}
