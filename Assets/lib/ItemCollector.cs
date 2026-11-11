using UnityEngine;

using System.Collections.Generic;



public class ItemCollector : MonoBehaviour

{

    public static ItemCollector Instance { get; private set; }



    public string itemTag = "Item";

    public int count = 0;

    public int combo = 0;



    [Header("Defeat Settings (ระบบแพ้)")]

    public int maxMisses = 5;

    public int currentMisses = 0;

    public bool isGameOver = false;

    public GameObject defeatUI;



    [Header("Ball Tags")]

    public string whiteBallTag = "WhiteBall";

    public string pinkBallTag = "PinkBall";


    [Header("New Ball Tags")]

    public string Tram = "Tram";

    public string ohm = "ohm";

    public string dew = "dew";

    public string F = "F";

    public string Heart = "Heart";



    public float praiseDuration = 1.5f;



    private string praiseText = "";

    private float praiseUntil = 0f;



    private HashSet<GameObject> collectedItems = new HashSet<GameObject>();



    private void Awake()

    {

        if (Instance != null && Instance != this)

        {

            Destroy(this);

            return;

        }



        Instance = this;

    }



    public void AddWhiteBallItemCount()

    {

        count++;

    }



    private void OnTriggerEnter2D(Collider2D other)

    {

        if (isGameOver) return;



        if (collectedItems.Contains(other.gameObject)) return;

        collectedItems.Add(other.gameObject);



        int comboChange;



        string ballName = other.gameObject.name.Replace("(Clone)", "").Trim();

        string lowerBallName = ballName.ToLower();



        if (lowerBallName == "item 1")

        {

            comboChange = -2;

        }

        else if (lowerBallName == "item" || other.CompareTag(whiteBallTag) || other.CompareTag(itemTag) || lowerBallName.Contains("ball-removebg"))
        {
            comboChange = 1;
            AddWhiteBallItemCount(); //  เพิ่มบรรทัดนี้: นับเพิ่ม 1 เมื่อผู้เล่นชนเก็บลูกบอลสีขาวสำเร็จจริง
        }

        else if (lowerBallName == "item 2")

        {

            comboChange = -5;

        }

        else if (lowerBallName == "item 3" || (!string.IsNullOrEmpty(Tram) && other.CompareTag(Tram)))

        {

            comboChange = 3;
            AddWhiteBallItemCount();

        }

        else if (lowerBallName == "item 4" || (!string.IsNullOrEmpty(ohm) && other.CompareTag(ohm)))

        {

            comboChange = 5;
            AddWhiteBallItemCount();

        }

        else if (lowerBallName == "item 5" || (!string.IsNullOrEmpty(dew) && other.CompareTag(dew)))

        {

            comboChange = 10;
            AddWhiteBallItemCount();

        }

        else if (lowerBallName == "item 6" || (!string.IsNullOrEmpty(F) && other.CompareTag(F)))

        {

            comboChange = -10;

        }

        else if (lowerBallName == "item 7" || (!string.IsNullOrEmpty(Heart) && other.CompareTag(Heart)))

        {

            ReduceMissedCount();
            AddWhiteBallItemCount();
            comboChange = 1;

        }

        else if (lowerBallName == "item" || other.CompareTag(whiteBallTag) || other.CompareTag(itemTag) || lowerBallName.Contains("ball-removebg"))

        {

            comboChange = 1;

        }

        else if (other.CompareTag(pinkBallTag))

        {

            comboChange = -5;

        }

        else

        {

            collectedItems.Remove(other.gameObject);

            return;

        }



        combo = Mathf.Max(0, combo + comboChange);



        if (LevelManager.Instance != null)

        {

            LevelManager.Instance.UpdateLevel(combo);

        }



        SaveMaxCombo();



        string praise = GetPraise(combo);



        if (!string.IsNullOrEmpty(praise))

        {

            praiseText = praise;

            praiseUntil = Time.time + praiseDuration;

        }



        Destroy(other.gameObject);

    }



    private string GetPraise(int c)

    {

        if (c >= 5) return "Perfect!";

        if (c == 4) return "Excellent!";

        if (c == 3) return "Great!";

        if (c == 2) return "OK!";

        return "";

    }



    public void ResetCombo()

    {

        combo = 0;



        if (LevelManager.Instance != null)

        {

            LevelManager.Instance.ResetLevel();

        }



        if (!isGameOver)

        {

            currentMisses++;

            Debug.Log($"Missed White Ball: {currentMisses}/{maxMisses}");



            if (currentMisses >= maxMisses)

            {

                TriggerDefeat();

            }

        }

    }



    public void ReduceMissedCount()

    {

        if (currentMisses > 0)

        {

            currentMisses--;

            Debug.Log($"Heart Collected! Reduced Missed Count: {currentMisses}/{maxMisses}");

        }

    }



    private void TriggerDefeat()

    {

        isGameOver = true;



        if (defeatUI != null)

        {

            defeatUI.SetActive(true);

        }



        Debug.Log("Defeat! Game Over.");

    }



    public void SaveMaxCombo()

    {

        AccountSaver saver = FindAnyObjectByType<AccountSaver>(FindObjectsInactive.Exclude);

        if (saver != null)

        {

            saver.SaveScore(combo);

        }

    }

    [Header("Missed UI Icons")]
    public Texture2D redCrossIcon;   // รูปวงกลมกากบาทสีแดง
    public Texture2D grayCircleIcon;  // รูปวงกลมสีเทา

[Header("Level UI Textures")]
public Texture2D level1Texture; // รูป 1.png
public Texture2D level2Texture; // รูป 2.png
public Texture2D level3Texture; // รูป 3.png

private void OnGUI()
    {
        if (Instance != this) return;

        // --- 1. คะแนน (มุมซ้ายบน) ---
        GUIStyle scoreStyle = new GUIStyle();
        scoreStyle.fontSize = 26;
        scoreStyle.fontStyle = FontStyle.Bold;
        scoreStyle.normal.textColor = Color.black;
        scoreStyle.alignment = TextAnchor.MiddleLeft;

        GUI.Label(new Rect(130, 20, 200, 35), $"\n{count}", scoreStyle);


        // --- 2. พลาด + วาดไอคอนวงกลม (มุมซ้าย ด้านล่างคะแนน) ---
        GUIStyle missedStyle = new GUIStyle();
        missedStyle.fontSize = 20;
        missedStyle.fontStyle = FontStyle.Bold;
        missedStyle.normal.textColor = Color.red;
        missedStyle.alignment = TextAnchor.MiddleLeft;

        // สลับแสดงผล: หากมีรูปกากบาท ให้ลูปวาดรูป หากไม่มีรูป ให้แสดง Text สำรอง
        if (redCrossIcon != null && grayCircleIcon != null)
        {
            GUIStyle missedTitleStyle = new GUIStyle(missedStyle);
            missedTitleStyle.normal.textColor = new Color(0.3f, 0.3f, 0.3f);

            GUI.Label(new Rect(110, 105, 80, 25), $"{currentMisses} / {maxMisses}", missedStyle);

            float iconStartX = 63f;
            float iconStartY = 133f;
            float iconSize = 30f;
            float iconSpacing = 4f;

            for (int i = 0; i < maxMisses; i++)
            {
                Rect iconRect = new Rect(iconStartX + i * (iconSize + iconSpacing), iconStartY, iconSize, iconSize);
                Texture2D iconToDraw = (i < currentMisses) ? redCrossIcon : grayCircleIcon;
                GUI.DrawTexture(iconRect, iconToDraw);
            }
        }
        else
        {
            GUI.Label(new Rect(35, 60, 200, 30), $" {currentMisses}/{maxMisses}", missedStyle);
        }


        // --- 3. COMBO (ตรงกลางบน) ---
        GUIStyle comboStyle = new GUIStyle();
        comboStyle.fontSize = 30;
        comboStyle.fontStyle = FontStyle.Bold;
        comboStyle.normal.textColor = Color.white;
        comboStyle.alignment = TextAnchor.MiddleCenter;

        float centerTopX = (Screen.width / 2f) - 100f;
        GUI.Label(new Rect(centerTopX, 15, 200, 50), $"\nx{combo}", comboStyle);


        // --- 4. ระดับ / Level (มุมขวาบน - แสดงรูปภาพ) ---
        int level = LevelManager.Instance != null ? LevelManager.Instance.currentLevel : 1;
        
        Texture2D levelTextureToDraw = null;
        if (level == 1) levelTextureToDraw = level1Texture;
        else if (level == 2) levelTextureToDraw = level2Texture;
        else if (level >= 3) levelTextureToDraw = level3Texture;

        if (levelTextureToDraw != null)
        {
            // ขนาดรูปภาพ Level (กว้าง 140px สูง 55px ปรับเปลี่ยนขนาดยอดขยายได้ตรงนี้)
            float imgW = 270f;
            float imgH = 60f;
            Rect levelRect = new Rect(Screen.width - imgW - 35f, 20f, imgW, imgH);
            GUI.DrawTexture(levelRect, levelTextureToDraw);
        }
        else
        {
            // fallback หากยังไม่ได้ใส่รูปใน Inspector
            GUIStyle levelStyle = new GUIStyle();
            levelStyle.fontSize = 22;
            levelStyle.fontStyle = FontStyle.Bold;
            levelStyle.normal.textColor = Color.black;
            levelStyle.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(Screen.width - 235, 20, 200, 30), $"ระดับ: {level}", levelStyle);
        }


        // --- 5. หน้าจอ Defeat และ คำชมกลางจอ ---
        if (isGameOver)
        {
            GUIStyle defeatStyle = new GUIStyle();
            defeatStyle.fontSize = 72;
            defeatStyle.alignment = TextAnchor.MiddleCenter;
            defeatStyle.fontStyle = FontStyle.Bold;
            defeatStyle.normal.textColor = Color.red;

            GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 100), "Defeat", defeatStyle);
            return;
        }

        if (Time.time < praiseUntil)
        {
            GUIStyle bigStyle = new GUIStyle();
            bigStyle.fontSize = 52;
            bigStyle.alignment = TextAnchor.MiddleCenter;
            bigStyle.fontStyle = FontStyle.Bold;
            bigStyle.normal.textColor = new Color(1f, 0.5f, 0f);

            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 70), praiseText, bigStyle);
            
            GUIStyle subComboStyle = new GUIStyle(bigStyle);
            subComboStyle.fontSize = 26;
            subComboStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(0, (Screen.height * 0.35f) + 55, Screen.width, 40), $"Combo x{combo}", subComboStyle);
        }
    }
}