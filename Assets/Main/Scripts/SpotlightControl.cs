using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SpotlightControl : MonoBehaviour
{
    public Slider logicMeterSlider;
    public Image fillImage;
    public float logicValueLerpSpeed = 10f;
    public float pulseDuration = 0.2f;
    public float maxLogicPoints = 100f;

    public GameObject gameOverPanel;
    public Button mainMenuButton;

    public RectTransform meterRectTransform;
    public float screenHeightOffsetRatio = 0.05f;
    public float meterMovementSpeed = 5f;
    public float fluctuationSpeed = 3f;
    public float fluctuationMagnitude = 1.5f;

    private float currentLogicPoints = 50f;
    private float visualLogicPoints;
    private Vector3 initialPosition;
    private bool isDialogueActive = false;

    private Color originalFillColor;
    private float pulseTimer = 0f;
    private Color pulseColor = Color.clear;

    void Start()
    {
        if (logicMeterSlider == null)
        {
            Debug.LogError("[SpotlightControl] Missing Logic Meter Slider.");
            return;
        }

        if (meterRectTransform != null)
            initialPosition = meterRectTransform.localPosition;

        if (fillImage != null)
            originalFillColor = fillImage.color;

        logicMeterSlider.minValue = 0f;
        logicMeterSlider.maxValue = maxLogicPoints;
        currentLogicPoints = Mathf.Clamp(currentLogicPoints, 0f, maxLogicPoints);
        visualLogicPoints = currentLogicPoints;
        logicMeterSlider.value = visualLogicPoints;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    void Update()
    {
        visualLogicPoints = Mathf.Lerp(visualLogicPoints, currentLogicPoints, Time.deltaTime * logicValueLerpSpeed);
        ApplyVisualFluctuation(visualLogicPoints);
        HandlePulseVisual();

        if (currentLogicPoints <= 2)
            ShowGameOverPanel();
    }

    public void AdjustMeter(float points)
    {
        currentLogicPoints += points;
        currentLogicPoints = Mathf.Clamp(currentLogicPoints, 0f, maxLogicPoints);

        if (fillImage != null)
        {
            if (points > 0)
                pulseColor = Color.green;
            else if (points < 0)
                pulseColor = Color.red;
            else
                return;

            pulseTimer = pulseDuration;
        }
    }

    public void MoveUp() => isDialogueActive = true;
    public void MoveDown() => isDialogueActive = false;

    private void ApplyVisualFluctuation(float baseValue)
    {
        if (logicMeterSlider == null) return;

        float offset = Mathf.Sin(Time.time * fluctuationSpeed) * fluctuationMagnitude;
        float visualValue = baseValue + offset;
        logicMeterSlider.value = Mathf.Clamp(visualValue, logicMeterSlider.minValue, logicMeterSlider.maxValue);
    }

    private void HandlePulseVisual()
    {
        if (fillImage == null) return;

        if (pulseTimer > 0)
        {
            pulseTimer -= Time.deltaTime;
            fillImage.color = Color.Lerp(originalFillColor, pulseColor, pulseTimer / pulseDuration);
        }
        else
        {
            fillImage.color = originalFillColor;
        }
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("[SpotlightControl] Logic points depleted. Showing panel.");
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void AddPointsTest()
    {
        AdjustMeter(10f);
        Debug.Log($"Logic Meter: Increased. Current base score: {currentLogicPoints}");
    }

    public void ReducePointsTest()
    {
        AdjustMeter(-5f);
        Debug.Log($"Logic Meter: Reduced. Current base score: {currentLogicPoints}");
    }
}
