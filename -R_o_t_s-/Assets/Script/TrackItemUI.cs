using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class TrackItemUI : MonoBehaviour
{
    public Button selectButton;
    public Button playButton;
    public TextMeshProUGUI trackNameText;
    public HoldToDelete deleteButton;

    public Color normalColor = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Темный фон
    public Color selectedColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Синий неоновый

    private Image backgroundImage;
    private string filePath;
    private MainMenu mainMenu;

    void Awake()
    {
        backgroundImage = GetComponent<Image>();
    }

    public void Setup(string path, MainMenu menu)
    {
        filePath = path;
        mainMenu = menu;
        trackNameText.text = Path.GetFileNameWithoutExtension(path);

        playButton.gameObject.SetActive(false);
        backgroundImage.color = normalColor;

        // Клик по карточке -> Выделяем её и показываем Play
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => {
            playButton.gameObject.SetActive(true);
            backgroundImage.color = selectedColor;
        });

        // Клик по Play -> запускаем музыку
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => {
            mainMenu.PlayTrack(filePath);
        });

        // Настройка удаления (Long Press)
        deleteButton.onDelete = () => {
            File.Delete(filePath);
            mainMenu.RefreshTrackList();
        };
    }
}