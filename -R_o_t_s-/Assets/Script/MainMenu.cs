using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using SFB;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню")]
    public GameObject mainMenuPanel;
    public GameObject trackSelectionPanel;
    public GameObject confirmationPanel;
    public GameObject settingsPanel;
    public GameObject loadingPanel;

    [Header("Камеры")]
    public GameObject menuCamera;
    public GameObject gameCamera;

    [Header("Элементы интерфейса")]
    public GameObject gameTitle;
    public GameObject trackListPanel;

    [Header("Элементы настроек")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    public GameObject saveSettingsButton;

    [Header("Меню выбора трека")]
    public Transform contentParent;
    public GameObject trackButtonPrefab;
    public AudioSource audioSource;

    private string tracksFolderPath;

    void Start()
    {
        tracksFolderPath = Path.Combine(Application.persistentDataPath, "CustomTracks");
        if (!Directory.Exists(tracksFolderPath))
            Directory.CreateDirectory(tracksFolderPath);

        if (trackListPanel != null) trackListPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);

        LoadSettings();
        RefreshTrackList();
    }

    public void OnPlayButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        trackSelectionPanel.SetActive(true);
        if (trackListPanel != null) trackListPanel.SetActive(true);
        gameTitle.SetActive(false);
        RefreshTrackList();
    }

    public void PlayTrack(string path)
    {
        StartCoroutine(LoadAndPlayTrack(path));
    }

    IEnumerator LoadAndPlayTrack(string path)
    {
        // 1. Включаем черный экран
        loadingPanel.SetActive(true);

        // 2. ЖДЕМ ОДИН КАДР, чтобы экран успел отрисоваться
        yield return new WaitForEndOfFrame();

        // 3. Прячем меню и переключаем камеру (под черным экраном)
        trackSelectionPanel.SetActive(false);
        trackListPanel.SetActive(false);
        menuCamera.SetActive(false);
        gameCamera.SetActive(true);

        // 4. Ждем 2 секунды
        yield return new WaitForSeconds(2f);

        // 5. Убираем черный экран, показываем игру
        loadingPanel.SetActive(false);

        // 6. Загружаем и играем музыку
        string uri = "file://" + path;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Ошибка загрузки аудио: {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    public void ReturnToMenu()
    {
        audioSource.Stop();
        gameCamera.SetActive(false);
        menuCamera.SetActive(true);

        mainMenuPanel.SetActive(false);
        trackSelectionPanel.SetActive(true);
        trackListPanel.SetActive(true);
        loadingPanel.SetActive(false);
    }

    public void RefreshTrackList()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        string[] trackFiles = Directory.GetFiles(tracksFolderPath, "*.mp3");

        foreach (string filePath in trackFiles)
        {
            GameObject btnObj = Instantiate(trackButtonPrefab, contentParent);
            TrackItemUI item = btnObj.GetComponent<TrackItemUI>();
            if (item != null) item.Setup(filePath, this);
        }
    }

    public void OnAddTrackButtonClicked()
    {
        var extensions = new[] { new ExtensionFilter("MP3 Files", "mp3") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Выберите MP3 трек", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string destinationPath = Path.Combine(tracksFolderPath, Path.GetFileName(paths[0]));
            File.Copy(paths[0], destinationPath, true);
            RefreshTrackList();
        }
    }

    public void OnSettingsButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        gameTitle.SetActive(false);
        saveSettingsButton.SetActive(false);
    }

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        saveSettingsButton.SetActive(true);
    }

    public void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        saveSettingsButton.SetActive(true);
    }

    public void OnSaveSettingsClicked()
    {
        PlayerPrefs.SetFloat("GameVolume", volumeSlider.value);
        PlayerPrefs.SetInt("IsFullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
        saveSettingsButton.SetActive(false);
    }

    public void OnSettingsBackButtonClicked()
    {
        LoadSettings();
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        gameTitle.SetActive(true);
    }

    public void OnBackButtonClicked()
    {
        trackSelectionPanel.SetActive(false);
        if (trackListPanel != null) trackListPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        gameTitle.SetActive(true);
        audioSource.Stop();
    }

    public void OnExitButtonClicked()
    {
        confirmationPanel.SetActive(true);
    }

    public void OnConfirmExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnCancelExitClicked()
    {
        confirmationPanel.SetActive(false);
    }

    private void LoadSettings()
    {
        float v = PlayerPrefs.GetFloat("GameVolume", 1f);
        volumeSlider.SetValueWithoutNotify(v);
        AudioListener.volume = v;

        int f = PlayerPrefs.GetInt("IsFullscreen", 0);
        Screen.fullScreenMode = (f == 1) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        fullscreenToggle.SetIsOnWithoutNotify(f == 1);
    }
}