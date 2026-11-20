using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TileAnimator : MonoBehaviour
{
    [Header("Настройки эквалайзера")]
    public float maxHeight = 5f;        // Максимальная высота прыжка
    public float attackSpeed = 20f;     // Как быстро плитки взлетают (чем больше, тем резче)
    public float releaseSpeed = 5f;     // Как плавно они падают обратно
    public int frequencyBands = 32;     // На сколько частотных групп делим музыку (ширина полос)

    [Header("Отладка")]
    public float currentMaxBass = 0f;

    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] vertices;
    private AudioSource audioSource;
    private float[] spectrum = new float[512];
    private float[] targetHeights;      // К какой высоте стремится колонка
    private float[] currentHeights;     // Текущая высота колонки (для плавности)

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
        vertices = new Vector3[baseVertices.Length];
        audioSource = GetComponent<AudioSource>();

        // Инициализируем массивы для плавной анимации
        targetHeights = new float[frequencyBands];
        currentHeights = new float[frequencyBands];
    }

    void Update()
    {
        if (baseVertices == null || baseVertices.Length == 0) return;

        // 1. Считываем весь спектр музыки (не только бас)
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
        }

        // 2. Находим максимальную частоту для отладки (чтобы видеть, что музыка играет)
        float maxVal = 0f;
        for (int i = 0; i < 20; i++) { if (spectrum[i] > maxVal) maxVal = spectrum[i]; }
        currentMaxBass = maxVal * 100f;

        // 3. Обновляем высоту для каждой колонки (полосы частот)
        for (int i = 0; i < frequencyBands; i++)
        {
            // Берем значение из спектра (первые 64 частоты обычно самые сочные)
            float targetHeight = spectrum[i] * maxHeight * 100f;

            // Математика: если высота растет - используем attackSpeed (резко), если падает - releaseSpeed (плавно)
            float speed = (targetHeight > currentHeights[i]) ? attackSpeed : releaseSpeed;
            currentHeights[i] = Mathf.Lerp(currentHeights[i], targetHeight, Time.deltaTime * speed);
        }

        // 4. Двигаем вершины каждой плитки
        // Мы вычисляем, в какую колонку (полосу частот) попадает плитка
        for (int v = 0; v < baseVertices.Length; v += 4)
        {
            Vector3 center = (baseVertices[v] + baseVertices[v + 1] + baseVertices[v + 2] + baseVertices[v + 3]) / 4f;

            // Определяем индекс полосы частот на основе координаты X
            // Предполагаем, что поле от -10 до 10 по X. 
            // (Если поле у тебя другого размера, эта формула все равно распределит колонки равномерно)
            float normalizedX = Mathf.InverseLerp(-10f, 10f, center.x);
            int bandIndex = Mathf.Clamp(Mathf.RoundToInt(normalizedX * (frequencyBands - 1)), 0, frequencyBands - 1);

            // Берем текущую высоту для этой полосы
            float yOffset = currentHeights[bandIndex];

            for (int j = 0; j < 4; j++)
            {
                vertices[v + j] = baseVertices[v + j];
                vertices[v + j].y += yOffset;
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}