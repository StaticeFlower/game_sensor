using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    public int currentLevel = 1;
    public int comboForLevel2 = 20;
    public int comboForLevel3 = 40;

    [Header("Speed Settings")]
    public float level1Speed = 5f;
    public float level2Speed = 7.5f;
    public float level3Speed = 10f;

    public float CurrentSpeed
    {
        get
        {
            switch (currentLevel)
            {
                case 2: return level2Speed;
                case 3: return level3Speed;
                default: return level1Speed;
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void UpdateLevel(int combo)
    {
        if (combo >= comboForLevel3)
            currentLevel = 3;
        else if (combo >= comboForLevel2)
            currentLevel = 2;
        else
            currentLevel = 1;

        Debug.Log($"Level {currentLevel} | Combo x{combo} | Speed {CurrentSpeed}");
    }

    public void ResetLevel()
    {
        currentLevel = 1;
        Debug.Log("พลาด! กลับไป Level 1");
    }
}