using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Web : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // !! YOU NEED TO HAVE SET UP A VRITUALHOST NAMED 'bimexpo', pointing to the 'PHP' folder
        //StartCoroutine(ExecutePHPScript("http://bimexpo/CreateTableFromCSV.php"));
        StartCoroutine(CreateTableFromCSV("C:/Users/aca/Documents/Projects/BIMEXPO/DB_Carrelages_Demo.csv", "tptiles"));
    }

    IEnumerator CreateTableFromCSV(string csvPath, string tableName)
    {
        WWWForm form = new WWWForm();
        form.AddField("tableName", tableName);
        form.AddField("csvPath", csvPath);

        using (UnityWebRequest www = UnityWebRequest.Post("http://bimexpo/CreateTableFromCSV.php", form))
        {
            // Request and wait for the desired page.
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    IEnumerator ExecutePHPScript(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    Debug.Log(webRequest.downloadHandler.text);
                    break;
            }
        }
    }
}
