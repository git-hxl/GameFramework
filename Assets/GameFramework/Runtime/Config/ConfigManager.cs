
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameFramework
{
    public class ConfigManager : Singleton<ConfigManager>
    {

        private Dictionary<string, string> _config = new Dictionary<string, string>();

        private Dictionary<string, List<IConfig>> _configValues = new Dictionary<string, List<IConfig>>();

        protected override void OnDispose()
        {
            //throw new System.NotImplementedException();
        }

        protected override void OnInit()
        {
            //throw new System.NotImplementedException();

            ReadConfig();
        }

        private void ReadConfig()
        {
            string[] jsonFiles = new string[] { "TestConfig.json" };

            foreach (var file in jsonFiles)
            {
                string json = File.ReadAllText(Application.streamingAssetsPath + "/" + file);

                string fileName = Path.GetFileNameWithoutExtension(file);

                _config.Add(fileName, json);

                Debug.Log("加载配置文件：" + fileName);
            }
        }

        public List<T> GetAllConfigs<T>() where T : IConfig
        {
            string fileName = typeof(T).Name;

            List<T> values = new List<T>();

            if (_config.ContainsKey(fileName))
            {
                if (_configValues.ContainsKey(fileName))
                {
                    values = _configValues[fileName].Cast<T>().ToList();
                }
                else
                {
                    values = JsonConvert.DeserializeObject<List<T>>(_config[fileName]);

                    _configValues.Add(fileName, values.Cast<IConfig>().ToList());
                }
            }

            return values;
        }

        public T GetConfig<T>(int id) where T : IConfig
        {
            string fileName = typeof(T).Name;

            List<T> values = GetAllConfigs<T>();

            if (values != null)
            {
                return values.FirstOrDefault((a) => a.ID == id);
            }

            return default(T);
        }

    }
}