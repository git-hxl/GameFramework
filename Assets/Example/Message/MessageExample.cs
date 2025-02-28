using GameFramework;
using System.Text;
using System.Threading;
using UnityEngine;

public class MessageExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MessageManager.Instance.Register(0, Test);
        MessageManager.Instance.Register<string>(1, Test2);
        MessageManager.Instance.Register<string, float>(2, Test3);

        MessageManager.Instance.Register<string, string, float>(3, Test4);

        MessageManager.Instance.Register<string, string, string>(3, Test5);


        MessageSyncManager.Instance.Register(0, Test);

        Thread thread = new Thread(() =>
        {
            while (true)
            {
                MessageSyncManager.Instance.Enqueue(0, Encoding.UTF8.GetBytes("MessageSyncManager"));

                Thread.Sleep(1000);
            }
        });

        thread.Start();
    }

    private void Test(byte[] data)
    {
        Debug.Log(Encoding.UTF8.GetString(data));
    }

    private void Test()
    {
        Debug.Log("msg");
    }

    private void Test2(string msg)
    {
        Debug.Log(msg);
    }

    private void Test3(string msg, float value)
    {
        Debug.Log(msg + value);
    }

    private void Test4(string msg, string value, float value3)
    {
        Debug.Log("Test4:" + msg + value + value3);
    }

    private void Test5(string msg, string value, string value3)
    {
        Debug.Log("Test5:" + msg + value + value3);
    }

    private void OnDestroy()
    {
        MessageManager.Instance.UnRegister(0, Test);
        MessageManager.Instance.UnRegister<string>(1, Test2);
        MessageManager.Instance.UnRegister<string, float>(2, Test3);

        MessageSyncManager.Instance.UnRegister(0, Test);
    }
}
