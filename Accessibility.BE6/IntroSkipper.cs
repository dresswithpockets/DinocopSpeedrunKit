using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Accessibility.BE6;

public class IntroSkipper : MonoBehaviour
{
    private const string IntroSceneName = "STREET_level";

    public void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void Start()
    {
        Plugin.Logger.LogDebug($"IntroSkipper: Disabling on start");
        gameObject.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != IntroSceneName)
            return;

        Plugin.Logger.LogDebug($"IntroSkipper: Enabling to skip intro scene");
        gameObject.SetActive(true);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name != IntroSceneName)
            return;

        Plugin.Logger.LogDebug($"IntroSkipper: Disabling to stop spamming the O key :3");
        gameObject.SetActive(false);
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