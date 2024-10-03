using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using NaughtyAttributes;
using DG.Tweening;

public class SceneLoadingManager : MonoBehaviour
{
    #region Singleton
    private static SceneLoadingManager _Instance;
    public static SceneLoadingManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindFirstObjectByType<SceneLoadingManager>();
            return _Instance;
        }
    }
    #endregion

    public CanvasGroup loadingCanvasGroup; 
    public float fadeDuration = 1f;

    [Scene]
    public string mainMenuSceneName;
    [Scene]
    public string gameplaySceneName;

    public Image loadingImage;

    private void Start()
    {
        loadingCanvasGroup.alpha = 0f;
        loadingCanvasGroup.interactable = false;
        loadingCanvasGroup.blocksRaycasts = false;
    }

    public void LoadMainMenu()
    {
        StartCoroutine(LoadSceneWithFade(mainMenuSceneName));
    }

    public void LoadGameplayScene()
    {
        StartCoroutine(LoadSceneWithFade(gameplaySceneName));
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        DOTween.Clear();
        loadingImage.transform.DOLocalRotate(new Vector3(0f, 0f, 90f), .5f, RotateMode.WorldAxisAdd).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
        FadeCanvas(true);
        yield return new WaitForSecondsRealtime(.5f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
        yield return new WaitForSecondsRealtime(.5f);
        FadeCanvas(false);
    }

    private void FadeCanvas(bool fadeIn)
    {
        if(fadeIn)
        {
            loadingCanvasGroup.DOFade(1f, .3f).SetUpdate(true);
        }
        else
        {
            // fade out
            loadingCanvasGroup.DOFade(0f, .3f).SetUpdate(true);
        }
    }
    
}
