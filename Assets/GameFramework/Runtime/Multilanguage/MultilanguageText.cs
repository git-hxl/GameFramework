using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public class MultilanguageText : MonoBehaviour
    {
        public string ID = "";
        private Text text;
        private TextMeshProUGUI textMeshPro;

        private void Awake()
        {
            text = GetComponent<Text>();
            textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            if (text != null)
                text.text = MultilanguagManager.Instance.GetText(ID);
            else if (textMeshPro != null)
                textMeshPro.text = MultilanguagManager.Instance.GetText(ID);
        }
    }
}