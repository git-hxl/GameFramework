
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<string, UIPanel> panels = new Dictionary<string, UIPanel>();

        private List<UIPanel> openList = new List<UIPanel>();

        protected override void OnDispose()
        {
            ClearAll();
        }

        protected override void OnInit()
        {

        }

        public T OpenUI<T>(string path, UILayer uILayer = UILayer.Low) where T : UIPanel
        {
            T t = default(T);

            if (panels.ContainsKey(path))
            {
                t = (T)panels[path];
            }

            if (t == null || t.Equals(null))
            {
                GameObject asset = ResourceManager.Instance.LoadAsset<GameObject>(path);
                t = GameObject.Instantiate(asset).GetComponent<T>();

                panels[path] = t;
            }

            if (t != null && !openList.Contains(t))
            {
                Canvas canvas = t.GetComponent<Canvas>();

                canvas.sortingOrder = (int)uILayer;

                t.OnOpen();

                openList.Add(t);
            }

            return t;
        }

        public T GetUI<T>() where T : UIPanel
        {
            for (int i = openList.Count - 1; i >= 0; i--)
            {
                if (openList[i] == null || openList[i].Equals(null))
                {
                    openList.RemoveAt(i);
                }
                else
                {
                    if(openList[i] is T)
                    {
                        return (T)openList[i];
                    }
                }
            }
            return null;
        }

        public void CloseUI(string path)
        {
            if (panels.ContainsKey(path))
            {
                UIPanel panel = panels[path];
                CloseUI(panel);
            }
        }

        public void CloseUI(UIPanel panel)
        {
            if (panel == null || panel.Equals(null))
                return;

            if (openList.Contains(panel))
            {
                panel.OnClose();
                openList.Remove(panel);
            }
        }

        public void ShowUI(UIPanel panel)
        {
            if (panel == null || panel.Equals(null))
                return;
            if (openList.Contains(panel))
            {
                panel.OnShow();
            }
        }

        public void HideUI(UIPanel panel)
        {
            if (panel == null || panel.Equals(null))
                return;
            if (openList.Contains(panel))
            {
                panel.OnHide();
            }
        }

        public void ClearAll()
        {
            foreach (var panel in panels)
            {
                if (panel.Value != null)
                {
                    GameObject.Destroy(panel.Value.gameObject);
                }
            }

            panels.Clear();

            openList.Clear();
        }
    }
}