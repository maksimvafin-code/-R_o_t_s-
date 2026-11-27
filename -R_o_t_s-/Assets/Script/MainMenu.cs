using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using SFB; // Плагин StandaloneFileBrowser
using TMPro; // TextMeshPro

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню")]
    public GameObject mainMenuPanel;
    public GameObject trackSelectionPanel;
    public GameObject confirmationPanel;
    public GameObject settingsPanel;

    [Header("Элементы интерфейса")]
    public GameObject gameTitle;
    public GameObject trackListPanel; // <--- НАШ ОТДЕЛЬНЫЙ СПИСОК ТРЕКОВ

    [Header("Элементы настроек")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    public GameObject saveSettingsButton;

    [Header("Меню выбора трека")]
    public Transform contentParent;
    public GameObject trackButtonPrefab;
    public Button playSelectedButton;
    public AudioSource audioSource;

    private string tracksFolderPath;
    private string selectedTrackPath;

    void Start()
    {
        // Создаем папку для треков
        tracksFolderPath = Path.Combine(Application.persistentDataPath, "CustomTracks");
        if (!Directory.Exists(tracksFolderPath))
            Directory.CreateDirectory(tracksFolderPath);

        // Скрываем то, что не должно быть видно при старте
        playSelectedButton.gameObject.SetActive(false);
        if (trackListPanel != null) trackListPanel.SetActive(false);

        LoadSettings();
        RefreshTrackList();
    }

    // ==========================================
    // --- ГЛАВНОЕ МЕНЮ ---
    // ==========================================
    public void OnPlayButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        trackSelectionPanel.SetActive(true);

        // ВКЛЮЧАЕМ СПИСОК ТРЕКОВ
        if (trackListPanel != null) trackListPanel.SetActive(true);

        gameTitle.SetActive(false);
        playSelectedButton.gameObject.SetActive(false);
        RefreshTrackList();
    }

    public void OnSettingsButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        gameTitle.SetActive(false);
        saveSettingsButton.SetActive(false);
    }

    // ==========================================
    // --- НАСТРОЙКИ ---
    // ==========================================
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

    // ==========================================
    // --- МЕНЮ ВЫБОРА ТРЕКА ---
    // ==========================================
    public void OnBackButtonClicked()
    {
        trackSelectionPanel.SetActive(false);

        // ВЫКЛЮЧАЕМ СПИСОК ТРЕКОВ
        if (trackListPanel != null) trackListPanel.SetActive(false);

        mainMenuPanel.SetActive(true);
        gameTitle.SetActive(true);
        audioSource.Stop();
        playSelectedButton.gameObject.SetActive(false);
    }

    public void OnAddTrackButtonClicked()
    {
        var extensions = new[] { new ExtensionFilter("MP3 Files", "mp3") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Выберите MP3 трек", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string sourcePath = paths[0];
            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = Path.Combine(tracksFolderPath, fileName);

            File.Copy(sourcePath, destinationPath, true);
            Debug.Log($"Трек добавлен: {fileName}");
            RefreshTrackList();
        }
    }

    private void RefreshTrackList()
    {
        // Очищаем старые кнопки
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Читаем файлы из папки
        string[] trackFiles = Directory.GetFiles(tracksFolderPath, "*.mp3");

        foreach (string filePath in trackFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            GameObject btnObj = Instantiate(trackButtonPrefab, contentParent);

            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = fileName;

            Button btn = btnObj.GetComponent<Button>();
            string pathToPlay = filePath;
            btn.onClick.AddListener(() => OnTrackSelected(pathToPlay));
        }
    }

    private void OnTrackSelected(string filePath)
    {
        selectedTrackPath = filePath;
        playSelectedButton.gameObject.SetActive(true);
    }

    public void OnPlaySelectedTrackClicked()
    {
        if (string.IsNullOrEmpty(selectedTrackPath)) return;
        StartCoroutine(LoadAndPlayTrack(selectedTrackPath));
    }

    IEnumerator LoadAndPlayTrack(string path)
    {
        string uri = "file://" + path;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Ошибка загрузки: {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    // ==========================================
    // --- ВЫХОД ---
    // ==========================================
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

    // ==========================================
    // --- ЗАГРУЗКА НАСТРОЕК ---
    // ==========================================
    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        volumeSlider.SetValueWithoutNotify(savedVolume);
        AudioListener.volume = savedVolume;

        int savedFullscreen = PlayerPrefs.GetInt("IsFullscreen", 0);
        Screen.fullScreenMode = (savedFullscreen == 1) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen == 1);
    }
}