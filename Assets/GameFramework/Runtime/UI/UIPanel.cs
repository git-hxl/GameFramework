using UnityEngine;

namespace GameFramework
{
    public abstract class UIPanel : MonoBehaviour
    {
        public abstract void OnOpen();

        public abstract void OnClose();

        public virtual void OnHide()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }

        public virtual void OnShow()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
            }
        }

        public void Close()
        {
            UIManager.Instance.CloseUI(this);
        }
    }
}