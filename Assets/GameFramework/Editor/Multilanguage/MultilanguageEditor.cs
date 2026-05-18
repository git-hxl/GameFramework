using Excel2Json;
using Newtonsoft.Json;
using System.Data;
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Editor
{
    public class MultilanguageEditor : EditorWindow
    {
        private string excelPath = "";

        private string outJsonPath = "";

        private string addTxt;

        [MenuItem("Tools/多语言工具")]
        public static void OpenWindow()
        {
            MultilanguageEditor window = (MultilanguageEditor)EditorWindow.GetWindow(typeof(MultilanguageEditor), false, "MultilanguageEditor");
            window.Show();
        }

        private void OnEnable()
        {
            excelPath = Application.streamingAssetsPath + "/MultiLanguage.xlsx";
            outJsonPath = Application.streamingAssetsPath + "/MultiLanguage.json";
        }

        private void InitExcel()
        {
            if (File.Exists(excelPath))
            {
                Debug.LogError("文件已存在");
                return;

                //File.Delete(excelPath);
            }

            DataTable initTable = new DataTable();
            DataRow dataRow = initTable.NewRow();

            initTable.Rows.Add(dataRow);

            var languageTypes = Enum.GetValues(typeof(MultilanguagType));
            initTable.Columns.Add("ID", typeof(string));
            dataRow[0] = "ID";
            for (int i = 0; i < languageTypes.Length; i++)
            {
                string language = languageTypes.GetValue(i).ToString();
                initTable.Columns.Add(language, typeof(string));
                dataRow[i + 1] = language;
            }

            //增加一行类型
            dataRow = initTable.Rows.Add();
            for (int i = 0; i < initTable.Columns.Count; i++)
            {
                dataRow[i] = "string";
            }

            //增加一行空
            initTable.Rows.Add();

            ExcelHelper.CreateExcel(excelPath, "MultiLanguage", initTable);

            Debug.Log("创建成功:" + excelPath);

            AssetDatabase.Refresh();
        }


        private void ExportToJson()
        {
            if (File.Exists(excelPath))
            {
                var dataTable = ExcelHelper.ReadExcelAllSheets(excelPath)[0];

                string json = JsonConvert.SerializeObject(ExcelHelper.SelectContent(dataTable), Formatting.Indented);

                using (FileStream stream = new FileStream(outJsonPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    stream.Write(data, 0, data.Length);

                    Debug.Log("导出成功:" + outJsonPath);
                }
                AssetDatabase.Refresh();
            }
        }

        private void AddTxt(string text)
        {
            string id = MultilanguagManager.TxtToID(text);

            var dataTable = ExcelHelper.ReadExcelAllSheets(excelPath)[0];

            foreach (DataRow row in dataTable.Rows)
            {
                if (row[0].ToString() == id)
                {
                    Debug.Log("ID 已存在");
                    return;
                }
            }


            dataTable = new DataTable();
            dataTable.Columns.Add();
            dataTable.Columns.Add();

            var newRow = dataTable.NewRow();
            newRow[0] = id;
            newRow[1] = text;
            dataTable.Rows.Add(newRow);

            ExcelHelper.WriteToExcel(excelPath, 1, dataTable);

            Debug.Log("添加成功:" + id);

            AssetDatabase.Refresh();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("初始化表格"))
            {
                InitExcel();
            }

            addTxt = GUILayout.TextArea(addTxt);

            if (GUILayout.Button("添加文本"))
            {
                AddTxt(addTxt);
            }

            if (GUILayout.Button("导出Json"))
            {
                ExportToJson();
            }
        }
    }
}