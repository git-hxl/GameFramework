using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public class UIExample : MonoBehaviour
    {
        public Button btOpen;
        public Button btClose;


        public Button btShow;
        public Button btHide;
        // Start is called before the first frame update
        void Start()
        {
            btOpen.onClick.AddListener(() =>
            {
                UIManager.Instance.OpenUI<UITmp>("Assets/GameFramework/Example/UI/UITmp.prefab");
            });

            btClose.onClick.AddListener(() =>
            {
                UIManager.Instance.CloseUI("Assets/GameFramework/Example/UI/UITmp.prefab");
            });


            btShow.onClick.AddListener(() =>
            {
                UITmp uITmp = UIManager.Instance.GetUI<UITmp>();

                UIManager.Instance.ShowUI(uITmp);
            });

            btHide.onClick.AddListener(() =>
            {
                UITmp uITmp = UIManager.Instance.GetUI<UITmp>();

                UIManager.Instance.HideUI(uITmp);
            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
