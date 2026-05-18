using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameFramework;
public class UITmp2 : MonoBehaviour
{
	#region 脚本工具生成的代码
	private Toggle @toggle;
	public Toggle Tg_toggle { get { if (@toggle == null) { @toggle = transform.Find("_Toggle").GetComponent<Toggle>(); } return @toggle; } } 
	private Button @button;
	public Button Bt_button { get { if (@button == null) { @button = transform.Find("_Button").GetComponent<Button>(); } return @button; } } 
	private Button @button2;
	public Button Bt_button2 { get { if (@button2 == null) { @button2 = transform.Find("_Button2").GetComponent<Button>(); } return @button2; } } 
	private Button @button21;
	public Button Bt_button21 { get { if (@button21 == null) { @button21 = transform.Find("_Button2 (1)").GetComponent<Button>(); } return @button21; } } 
	#endregion


    void Start()
    {
        Debug.Log(Bt_button.name);
    }
}
