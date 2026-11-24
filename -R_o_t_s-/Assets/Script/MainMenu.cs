using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using SFB; // Наш плагин
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню")]
    public GameObject mainMenuPanel;
    public GameObject trackSelectionPanel;
    public GameObject confirmationPanel;
    public GameObject settingsPanel;

    [Header("Элементы интерфейса")]
    public GameObject gameTitle;

    [Header("Элементы настроек")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    [Header("Меню выбора трека")]
    public Transform contentParent;        // Перетащим Content
    public GameObject trackButtonPrefab;   // Перетащим префаб кнопки
    public Button playSelectedButton;      // Кнопка "Играть"
    public AudioSource audioSource;        // Источник звука

    private string tracksFolderPath;
    private string selectedTrackPath;

    void Start()
    {
        // Папка для хранения треков
        tracksFolderPath = Path.Combine(Application.persistentDataPath, "CustomTracks");
        if (!Directory.Exists(tracksFolderPath))
            Directory.CreateDirectory(tracksFolderPath);

        playSelectedButton.gameObject.SetActive(false);
        LoadSettings();
        RefreshTrackList();
    }

    public void OnPlayButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        trackSelectionPanel.SetActive(true);
        gameTitle.SetActive(false);
        playSelectedButton.gameObject.SetActive(false);
        RefreshTrackList();
    }

    public void OnSettingsButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        gameTitle.SetActive(false);
    }

    public void OnSettingsBackButtonClicked()
    {
        PlayerPrefs.SetFloat("GameVolume", volumeSlider.value);
        PlayerPrefs.SetInt("IsFullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        gameTitle.SetActive(true);
    }

    public void OnVolumeChanged(float value) { AudioListener.volume = value; }
    public void OnFullscreenToggled(bool isFullscreen) { Screen.fullScreen = isFullscreen; }

    public void OnBackButtonClicked()
    {
        trackSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        gameTitle.SetActive(true);
        audioSource.Stop();
        playSelectedButton.gameObject.SetActive(false);
    }

    // 🟢 ДОБАВЛЕНИЕ ТРЕКА
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
            RefreshTrackList();
        }
    }

    // Обновление списка
    private void RefreshTrackList()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        string[] trackFiles = Directory.GetFiles(tracksFolderPath, "*.mp3");

        foreach (string filePath in trackFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            GameObject btnObj = Instantiate(trackButtonPrefab, contentParent);

            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = fileName;

            Button btn = btnObj.GetComponent<Button>();
            string pathToPlay = filePath; // Локальная переменная для правильного замыкания
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

    public void OnExitButtonClicked() { confirmationPanel.SetActive(true); }

    public void OnConfirmExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void OnCancelExitClicked() { confirmationPanel.SetActive(false); }

    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        int savedFullscreen = PlayerPrefs.GetInt("IsFullscreen", 0);
        fullscreenToggle.isOn = (savedFullscreen == 1);
        Screen.fullScreen = (savedFullscreen == 1);
    }
}