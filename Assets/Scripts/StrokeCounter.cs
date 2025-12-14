using UnityEngine;
using TMPro;

public class StrokeCounter : MonoBehaviour
{
    public static StrokeCounter Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI strokeText;

    private int _strokeCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    public void IncrementStroke()
    {
        _strokeCount++;
        UpdateUI();
    }

    public void ResetStrokes()
    {
        _strokeCount = 0;
        UpdateUI();
    }

    public int GetStrokeCount()
    {
        return _strokeCount;
    }

    private void UpdateUI()
    {
        if (strokeText != null)
        {
            strokeText.text = $"Strokes: {_strokeCount}";
        }
    }
}