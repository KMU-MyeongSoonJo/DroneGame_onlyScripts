using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    static string nextScene;
    static string beforeScene;

    [SerializeField] Image progressBar;
    [SerializeField] Text logs;

    public static void LoadScene(string sceneName, string beforeSceneName = null)
    {
        nextScene = sceneName;
        beforeScene = beforeSceneName;
        
        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
    }
    
    private void Start()
    {
        try
        {
            logs.text += ">> Scene Load Start() called [LoadingManager.cs]\n";
            if (beforeScene != null)
            {
                if (beforeScene == "CityScene")
                {
                    logs.text += ">> Unload CityScene Scene [LoadingManager.cs]\n";
                    logs.text += ">> Unload PlayableScene Scene [LoadingManager.cs]\n";
                    SceneManager.UnloadSceneAsync("CityScene");
                    SceneManager.UnloadSceneAsync("PlayableScene");
                }
                else
                {
                    logs.text += ">> Unload " + beforeScene + " Scene [LoadingManager.cs]\n";
                    SceneManager.UnloadSceneAsync(beforeScene);
                    //SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(beforeScene));
                }
            
            }
            beforeScene = null;
            StartCoroutine(LoadSceneProcess());
        }
        catch (System.Exception e)
        {
            logs.text += e;
        }

    }

    IEnumerator LoadSceneProcess()
    {
        logs.text += ">> LoadSceneProcess() coroutine called [LoadingManager.cs]\n";

        AsyncOperation op;
        if(nextScene == "IngameScene")
        {
            logs.text += ">> Load CityScene Scene [LoadingManager.cs]\n";
            logs.text += ">> Load PlayableScene Scene [LoadingManager.cs]\n";
            op = SceneManager.LoadSceneAsync("CityScene", LoadSceneMode.Additive);
            SceneManager.LoadSceneAsync("PlayableScene", LoadSceneMode.Additive);
        }
        else if(nextScene == "TitleScene")
        {
            logs.text += ">> Load TitleScene Scene [LoadingManager.cs]\n";
            logs.text += ">> Load CityScene Scene [LoadingManager.cs]\n";
            op = SceneManager.LoadSceneAsync("TitleScene", LoadSceneMode.Additive);
            SceneManager.LoadSceneAsync("CityScene", LoadSceneMode.Additive);
        }
        else
        {
            logs.text += ">> Load " + nextScene + " Scene [LoadingManager.cs]\n";
            op = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        }
        
        op.allowSceneActivation = false;

        float timer = 0f;
        float lastFillAmount = 0f;

        while (!op.isDone)
        {

            timer += Time.deltaTime;
            if(op.progress < 0.9f)
            {
                //progressBar.fillAmount = op.progress;
                progressBar.fillAmount = Mathf.Lerp(lastFillAmount, op.progress, timer * 0.2f);
                if (progressBar.fillAmount >= op.progress)
                {
                    lastFillAmount = progressBar.fillAmount;
                    timer = 0f;
                }
            }
            else
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1.0f, timer * 0.2f);
                if(progressBar.fillAmount >= 1f)
                {
                    logs.text += ">> Loading Finish!! wait for start ...\n";
                    yield return new WaitForSeconds(1f);
                    op.allowSceneActivation = true;
                }
            }

            yield return null;
        }


        SceneManager.UnloadSceneAsync("LoadingScene");
    }
}
