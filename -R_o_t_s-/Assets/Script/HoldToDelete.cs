using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class HoldToDelete : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float holdTime = 3f; // Время удержания в секундах
    public Image fillImage; // Кружок загрузки (необязательно)
    public Action onDelete; // Событие, которое вызовется при удалении

    private bool isHolding = false;
    private float timer = 0f;

    // Когда игрок нажимает на кнопку
    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        timer = 0f;
    }

    // Когда игрок отпускает кнопку
    public void OnPointerUp(PointerEventData eventData)
    {
        // Если отпустили раньше 3 секунд — отменяем удаление
        isHolding = false;
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    void Update()
    {
        if (isHolding)
        {
            timer += Time.deltaTime;

            // Если есть картинка для заполнения (кружок), показываем прогресс
            if (fillImage != null)
                fillImage.fillAmount = timer / holdTime;

            // Если продержали достаточно долго — удаляем
            if (timer >= holdTime)
            {
                isHolding = false;
                if (fillImage != null) fillImage.fillAmount = 1f;
                onDelete?.Invoke(); // Вызываем удаление
            }
        }
    }
}