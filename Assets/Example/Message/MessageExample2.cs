using GameFramework;
using System.Text;
using UnityEngine;

public class MessageExample2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        if (Time.frameCount % 60 == 0)
        {
            MessageManager.Instance.Dispatch(0);

            MessageManager.Instance.Dispatch<string>(1, "Test");
            MessageManager.Instance.Dispatch<string, float>(2, "Test", 0.99f);
            MessageManager.Instance.Dispatch<string, string, string>(3, "3333", "444", "5555");
        }
        
    }
}