using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameFramework
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        private Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();

        protected override void OnDispose()
        {
            foreach (var item in bundles)
            {
                UnloadAssetBundle(item.Key, true);
            }

            bundles.Clear();
        }

        protected override void OnInit()
        {
            //throw new System.NotImplementedException();
        }

        public T LoadAsset<T>(string path) where T : Object
        {
            T asset = default(T);

#if UNITY_EDITOR
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            return asset;
#endif
            var assetName = Path.GetFileName(path);

            foreach (var item in bundles)
            {
                if (item.Value.Contains(assetName))
                {
                    asset = item.Value.LoadAsset<T>(assetName);
                    break;
                }
            }

            if (asset == null)
                Debug.LogError("asset load failed: " + path);
            return asset;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(string path) where T : UnityEngine.Object
        {
            T asset = default(T);

#if UNITY_EDITOR
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            await UniTask.DelayFrame(1);

            return asset;
#endif

            var assetName = Path.GetFileName(path);

            foreach (var item in bundles)
            {
                if (item.Value.Contains(assetName))
                {
                    asset = await item.Value.LoadAssetAsync<T>(assetName) as T;
                    break;
                }
            }
            if (asset == null)
                Debug.LogError("asset load failed: " + path);
            return asset;
        }


        /// <summary>
        /// 加载AssetBundle
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string path)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
            AddAssetBundle(Path.GetFileName(path), assetBundle);
            return assetBundle;
        }

        /// <summary>
        /// 添加AssetBundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetBundle"></param>
        private void AddAssetBundle(string bundleName, AssetBundle assetBundle)
        {
            if (bundles.ContainsKey(bundleName))
            {
                Debug.Log("bundle is existed! check it!");
            }
            bundles[bundleName] = assetBundle;
        }

        /// <summary>
        /// 卸载AssetBundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="unloadAll"></param>
        public void UnloadAssetBundle(string bundleName, bool unloadAll)
        {
            if (bundles.ContainsKey(bundleName))
            {
                AssetBundle bundle = bundles[bundleName];
                if (bundle != null)
                {
                    bundle.Unload(unloadAll);
                }
                bundles.Remove(bundleName);
            }
        }
    }
}
