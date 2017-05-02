// LoadingScreenManager
// --------------------------------
// built by Martin Nerurkar (http://www.martin.nerurkar.de)
// for Nowhere Prophet (http://www.noprophet.com)
//
// Licensed under GNU General Public License v3.0
// http://www.gnu.org/licenses/gpl-3.0.txt

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Loading Visuals")]
    public List<Text> loadingText;
    public Image progressBar;
    public Image fadeOverlay;

    [Header("Timing Settings")]
    public float waitOnLoadEnd = 0.25f;
    public float fadeDuration = 3f;

    [Header("Loading Settings")]
    public LoadSceneMode loadSceneMode = LoadSceneMode.Single;
    public ThreadPriority loadThreadPriority;

    [Header("Other")]
    // If loading additive, link to the cameras audio listener, to avoid multiple active audio listeners
    public AudioListener audioListener;

    AsyncOperation operation;
    Scene currentScene;

    public static int sceneToLoad = -1;
    // IMPORTANT! This is the build index of your loading scene. You need to change this to match your actual scene index
    static int loadingSceneIndex = 2;

    private static bool doneLoading = false;
    private static float progress = 0f;
    private static LoadingScreenManager instance;

    public static void LoadScene(int levelNum)
    {
        Application.backgroundLoadingPriority = ThreadPriority.High;
        sceneToLoad = levelNum;
        SceneManager.LoadScene(loadingSceneIndex);
    }

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Attemping to create multiple loading screen instances.");
            Destroy(instance);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (sceneToLoad < 0)
            return;

        fadeOverlay.gameObject.SetActive(true); // Making sure it's on so that we can crossfade Alpha
        currentScene = SceneManager.GetActiveScene();
        StartCoroutine(LoadAsync(sceneToLoad));
    }

    private IEnumerator LoadAsync(int levelNum)
    {
        FadeIn();
        StartOperation(levelNum);

        float lastProgress = 0f;

        // operation does not auto-activate scene, so it's stuck at 0.9
        while (DoneLoading() == false)
        {
            yield return null;

            if (Mathf.Approximately(progress, lastProgress) == false)
            {
                progressBar.fillAmount = progress;
                lastProgress = progress;
            }
        }

        if (loadSceneMode == LoadSceneMode.Additive)
            audioListener.enabled = false;
        
        yield return new WaitForSeconds(waitOnLoadEnd);

        FadeOut();

        yield return new WaitForSeconds(fadeDuration);

        if (loadSceneMode == LoadSceneMode.Additive)
        {
            SceneManager.UnloadScene(currentScene.name);
        }
        Destroy(gameObject);
    }

    private void StartOperation(int levelNum)
    {
        Application.backgroundLoadingPriority = loadThreadPriority;
        operation = SceneManager.LoadSceneAsync(levelNum, loadSceneMode);
    }

    private bool DoneLoading()
    {
        return (loadSceneMode == LoadSceneMode.Additive && doneLoading) || (loadSceneMode == LoadSceneMode.Single && progress >= 0.9f);
    }

    public static void CompleteLoadingScreen()
    {
        doneLoading = true;
    }

    public static void SetLoadingProgress(float progressPercentage)
    {
        progress = progressPercentage;
    }

    void FadeIn()
    {
        fadeOverlay.CrossFadeAlpha(0, fadeDuration, true);
    }

    void FadeOut()
    {
        fadeOverlay.CrossFadeAlpha(1, fadeDuration, true);
    }

    public void ReplaceTextTokens(List<KeyValuePair<string,string>> textTokens)
    {
        foreach (Text text in loadingText)
        {
            foreach (KeyValuePair<string, string> token in textTokens)
            {
                text.text = text.text.Replace(token.Key, token.Value);
            }
        }
    }

    public static LoadingScreenManager GetInstance()
    {
        return instance;
    }

    public static List<KeyValuePair<string, string>> GetWorldGenerationTextTokens(WorldSettings worldSettings)
    {
        List<KeyValuePair<string, string>> textTokens = new List<KeyValuePair<string, string>>();
        textTokens.Add(new KeyValuePair<string, string>("$RandomSeed", worldSettings.randomSeed.ToString()));
        textTokens.Add(new KeyValuePair<string, string>("$WorldSize", worldSettings.sizeRating.ToString()));
        textTokens.Add(new KeyValuePair<string, string>("$StartLocations", worldSettings.numStartLocations.ToString()));
        textTokens.Add(new KeyValuePair<string, string>("$StartLocationSize", worldSettings.startLocationSizeRating.ToString()));
        textTokens.Add(new KeyValuePair<string, string>("$ResourceAbundance", worldSettings.resourceAbundanceRating.ToString()));
        textTokens.Add(new KeyValuePair<string, string>("$ResourceQuality", worldSettings.resourceQualityRating.ToString()));
        textTokens.Add(new KeyValuePair<string, string>("$AIPresenceFactor", worldSettings.aiPresenceRating.ToString()));
        textTokens.Add(new KeyValuePair<string, string>("$AIStrengthFactor", worldSettings.aiStrengthRating.ToString()));
        return textTokens;
    }
}