using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Accessibility.BE6;

public class IntroSkipper : MonoBehaviour
{
    private const string IntroSceneName = "STREET_level";
    private const string HotelSceneName = "HOTEL_level";

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public void Start()
    {
        Plugin.Logger.LogDebug($"IntroSkipper: Disabling on start");
        gameObject.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == IntroSceneName)
        {
            Plugin.Logger.LogDebug($"IntroSkipper: Enabling to skip intro scene");
            gameObject.SetActive(true);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == IntroSceneName)
        {
            Plugin.Logger.LogDebug($"IntroSkipper: Disabling to stop spamming the O key :3");
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        var endIntroInstance = FindObjectsOfType<EventInstance>()?.FirstOrDefault(e => e.name == "endIntro");
        if (endIntroInstance?.gameObject.activeInHierarchy ?? false)
        {
            Plugin.Logger.LogDebug($"IntroSkipper: Successfully calling HandleEvents on `endIntro`, disabling...");
            endIntroInstance.HandleEvents(1f);
            gameObject.SetActive(false);
        }
    }
}