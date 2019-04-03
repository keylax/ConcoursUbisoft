using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempChangeScene : MonoBehaviour
{

    public void changeScene()
    {
        SceneManager.LoadScene(1);
    }

    public void changeMethod()
    {
    }

    public void newNotifyMethod(int result)
    {
        Debug.Log(result);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(20, 70, 80, 20), "poll"))
        {
            List<string> opts = new List<string>();
            opts.Add("blabla");
            opts.Add("blab");
            opts.Add("blablabla");
            opts.Add("bl");
            StaticObjects.CrowdClient.PollCrowd(opts, 15, newNotifyMethod);
        }

    }
}
