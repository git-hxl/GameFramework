using UnityEngine;

namespace GameFramework
{
    public class UITmp : UIPanel
    {
        public override void OnClose()
        {
            gameObject.SetActive(false);
            Debug.Log("OnClose");
        }

        public override void OnOpen()
        {
            gameObject.SetActive(true);
            Debug.Log("OnOpen");
        }
    }
}