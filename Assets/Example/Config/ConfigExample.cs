using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ConfigExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            TestConfig textConfig = ConfigManager.Instance.GetConfig<TestConfig>(3);

            Debug.Log(JsonConvert.SerializeObject(textConfig));

            var configs = ConfigManager.Instance.GetAllConfigs<TestConfig>();

            Debug.Log(JsonConvert.SerializeObject(configs));
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }

    public class TestConfig : IConfig
    {
        public System.Int32 ID { get; set; }
        public System.String Des { get; set; }
        public System.Int32 Name { get; set; }
        public System.Single Length { get; set; }
        public System.Single Range { get; set; }
        public System.Single Scale { get; set; }
        public System.Single ScaledLength { get; set; }
    }
}
