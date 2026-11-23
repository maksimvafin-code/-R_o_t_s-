using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("Панели меню")]
    public GameObject mainMenuPanel;
    public GameObject trackSelectionPanel;
    public GameObject confirmationPanel;

    [Header("Элементы интерфейса")]
    public GameObject gameTitle;

    //Кнопка "Играть"
    public void OnPlayButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        trackSelectionPanel.SetActive(true);
        gameTitle.SetActive(false);
    }

    //Кнопка "Назад" (в меню выбора трека)
    public void OnBackButtonClicked()
    {
        trackSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        gameTitle.SetActive(true);
    }

    //Кнопка "Добавить трек" (Заглушка)
    public void OnAddTrackButtonClicked()
    {
        Debug.Log("Кнопка 'Добавить трек' нажата!");
    }

    //Кнопка "Настройки" (Заглушка)
    public void OnSettingsButtonClicked()
    {
        Debug.Log("Кнопка 'Настройки' нажата!");
    }

    //Кнопка "Выход" (теперь не выходит, а показывает панель)
    public void OnExitButtonClicked()
    {
        confirmationPanel.SetActive(true);
    }

    //Кнопка "Да" (в окне подтверждения)
    public void OnConfirmExitClicked()
    {
        Debug.Log("Выход из игры");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    //Кнопка "Нет" (в окне подтверждения)
    public void OnCancelExitClicked()
    {
        confirmationPanel.SetActive(false);
    }
}