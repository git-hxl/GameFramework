
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameFramework
{
    public class MultilanguagManager : Singleton<MultilanguagManager>
    {
        public MultilanguagType Type { get; private set; }

        private List<MultilanguageData> multilanguageDatas = new List<MultilanguageData>();

        protected override void OnDispose()
        {
            //throw new System.NotImplementedException();
            multilanguageDatas.Clear();
        }

        protected override void OnInit()
        {
            //throw new System.NotImplementedException();

            string json = File.ReadAllText(Application.streamingAssetsPath + "/MultiLanguage.json");

            multilanguageDatas = JsonConvert.DeserializeObject<List<MultilanguageData>>(json);
        }

        /// <summary>
        /// 获取多语言文本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetText(string id)
        {
            var languageData = multilanguageDatas.FirstOrDefault((a) => a.ID == id);
            if (languageData != null)
            {
                switch (Type)
                {
                    case MultilanguagType.Chinese: return languageData.Chinese;
                    case MultilanguagType.English: return languageData.English;
                }
            }
            Debug.LogError($"多语言ID：{id} 不存在");
            return "";
        }

        public static string TxtToID(string text)
        {
            return string.Format("{0:X}", text.GetHashCode());
        }
    }
}