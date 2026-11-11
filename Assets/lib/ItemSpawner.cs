using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Prefabs")]
    public GameObject whiteBallPrefab;
    public GameObject pinkBallPrefab;
    public GameObject item3Prefab;

    [Header("New Item Prefabs")]
    public GameObject item4Prefab; // +5 Combo
    public GameObject item5Prefab; // +10 Combo
    public GameObject item6Prefab; // -10 Combo
    public GameObject item7Prefab; // -1 Missed (ลดค่าการพลาดลง 1)

    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;
    public float spawnY = 6f;
    public float xMin = -8f;
    public float xMax = 8f;
    public float destroyYBoundary = -6f;

    [Header("Defeat Settings (ระบบแพ้)")]
    public int maxMissedWhiteBalls = 5;
    private int missedWhiteBallCount = 0;
    private bool isGameOver = false;
    public GameObject defeatUI;

    [Header("Spawn Probabilities (%)")]
    [Range(0, 100)] public int whiteChance = 50;
    [Range(0, 100)] public int greenChance = 25;
    [Range(0, 100)] public int redChance = 15;
    [Range(0, 100)] public int pinkChance = 10;
    [Range(0, 100)] public int item3Chance = 10;
    [Range(0, 100)] public int item4Chance = 5;
    [Range(0, 100)] public int item5Chance = 5;
    [Range(0, 100)] public int item6Chance = 5;
    [Range(0, 100)] public int item7Chance = 5;

    private Dictionary<GameObject, float> horizontalSpeeds = new Dictionary<GameObject, float>();
    private List<GameObject> activeItems = new List<GameObject>();
    private float timer = 0f;

    private void Update()
    {
        if (isGameOver) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnItem();
            timer = 0f;
        }

        float currentSpeed = LevelManager.Instance != null ? LevelManager.Instance.CurrentSpeed : 5f;

        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            GameObject item = activeItems[i];

            if (item == null)
            {
                horizontalSpeeds.Remove(item);
                activeItems.RemoveAt(i);
                continue;
            }

            float horizontalSpeed = 0f;

            if (horizontalSpeeds.TryGetValue(item, out float savedSpeed))
            {
                horizontalSpeed = savedSpeed;
            }

            Vector3 movement = new Vector3(horizontalSpeed, -currentSpeed, 0f);
            item.transform.Translate(movement * Time.deltaTime);

            if (item.transform.position.y <= destroyYBoundary)
            {
                string ballName = item.name.Replace("(Clone)", "").Trim().ToLower();

        bool isGameItem = !item.CompareTag("Untagged") || 
                        item.name.ToLower().Contains("ball") || 
                        item.name.ToLower().Contains("item");

        // ถ้าไม่อยากให้นับเฉพาะหัวใจ (Heart) ให้เพิ่มเงื่อนไขตรงนี้
        bool isIgnored = item.CompareTag("f") || item.name.ToLower().Contains("f");

                if (isGameItem && !isIgnored)
                {
                    if (ItemCollector.Instance != null)
                    {
                        ItemCollector.Instance.ResetCombo();
                    }

                    RegisterMissedWhiteBall();
                }

                horizontalSpeeds.Remove(item);
                Destroy(item);
                activeItems.RemoveAt(i);
            }
        }
    }

    private void RegisterMissedWhiteBall()
    {
        missedWhiteBallCount++;
        Debug.Log($"Missed White Ball: {missedWhiteBallCount}/{maxMissedWhiteBalls}");

        if (missedWhiteBallCount >= maxMissedWhiteBalls)
        {
            TriggerDefeat();
        }
    }

    public void ReduceMissedCount()
    {
        if (missedWhiteBallCount > 0)
        {
            missedWhiteBallCount--;
            Debug.Log($"Reduced Missed Count! Current Missed: {missedWhiteBallCount}/{maxMissedWhiteBalls}");
        }
    }

    private void TriggerDefeat()
    {
        isGameOver = true;

        if (defeatUI != null)
        {
            defeatUI.SetActive(true);
        }

        Debug.Log("Game Over: Defeat!");
    }

    private void SpawnItem()
    {
        GameObject prefabToSpawn = GetRandomItemPrefab();

        if (prefabToSpawn == null) return;

        float randomX = Random.Range(xMin, xMax);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        GameObject newItem = Instantiate(
            prefabToSpawn,
            spawnPosition,
            Quaternion.identity
        );

        activeItems.Add(newItem);

        float horizontalSpeed = 0f;

        if (IsSpecialItem(newItem))
        {
            int direction = Random.Range(0, 2);
            horizontalSpeed = (direction == 0) ? -Random.Range(1.5f, 3.5f) : Random.Range(1.5f, 3.5f);
        }

        horizontalSpeeds[newItem] = horizontalSpeed;

    }

    private bool IsSpecialItem(GameObject item)
    {
        string ballName = item.name.Replace("(Clone)", "").Trim().ToLower();

        return ballName == "item 3"
            || ballName == "item 4"
            || ballName == "item 5"
            || ballName == "item 6"
            || ballName == "item 7";
    }

    private GameObject GetRandomItemPrefab()
    {
        int totalWeight = whiteChance + greenChance + redChance + pinkChance +
                          item3Chance + item4Chance + item5Chance + item6Chance + item7Chance;

        if (totalWeight <= 0) return whiteBallPrefab;

        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < whiteChance) return whiteBallPrefab;
        randomValue -= whiteChance;

        if (randomValue < pinkChance)
            return pinkBallPrefab != null ? pinkBallPrefab : whiteBallPrefab;
        randomValue -= pinkChance;

        if (randomValue < item3Chance)
            return item3Prefab != null ? item3Prefab : whiteBallPrefab;
        randomValue -= item3Chance;

        if (randomValue < item4Chance)
            return item4Prefab != null ? item4Prefab : whiteBallPrefab;
        randomValue -= item4Chance;

        if (randomValue < item5Chance)
            return item5Prefab != null ? item5Prefab : whiteBallPrefab;
        randomValue -= item5Chance;

        if (randomValue < item6Chance)
            return item6Prefab != null ? item6Prefab : whiteBallPrefab;
        randomValue -= item6Chance;

        return item7Prefab != null ? item7Prefab : whiteBallPrefab;
    }
}