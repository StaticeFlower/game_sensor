using UnityEngine;
using TMPro; // สำคัญ: ใช้สำหรับ TMP_InputField, TMP_Dropdown และ TMP_Text

public class AccountSaver : MonoBehaviour
{
    [Header("Login UI References (ถ้ามี)")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("Settings UI References (ถ้ามี)")]
    public TMP_Dropdown controlTypeDropdown;

    [Header("Display UI References (ถ้ามี)")]
    public TMP_Text maxComboText;
    public TMP_Text playTimeText;

    private float sessionTimer = 0f;
    private bool isTimerRunning = false;

    private void Start()
    {
        // เริ่มนับเวลาเล่นในรอบนี้ทันทีที่เปิด Scene
        sessionTimer = 0f;
        isTimerRunning = true;

        // โหลดข้อมูลเก่าที่เคยเซฟไว้มาแสดงผล
        LoadSavedData();
    }

    private void Update()
    {
        // นับเวลาสะสมตามเวลาจริงที่ผ่านไป
        if (isTimerRunning)
        {
            sessionTimer += Time.deltaTime;
        }
    }

    // เมื่อปิดเกม หรือพับหน้าจอ ให้บันทึกเวลาเล่นสะสมทับลงเครื่องทันที
    private void OnApplicationQuit()
    {
        SavePlayTime();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SavePlayTime();
        }
    }

    // ==========================================
    // 🔹 1. ระบบบันทึกข้อมูล Login (ใช้ใน Scene 1)
    // ==========================================
    public void SaveAccountData()
    {
        // Authentication is handled by SupabaseAuth. Credentials are never saved locally.
    }

    // ==========================================
    // 🔹 2. ระบบบันทึกโหมดการเล่น finger / body (ใช้ใน Scene 2)
    // ==========================================
    public void SaveControlSelection()
    {
        if (controlTypeDropdown != null)
        {
            int selectedIndex = controlTypeDropdown.value;
            string selectedText = controlTypeDropdown.options[selectedIndex].text;

            PlayerPrefs.SetInt("SelectedControl", selectedIndex);
            PlayerPrefs.SetString("SelectedControlMode", selectedText);
            PlayerPrefs.Save();
            Debug.Log("บันทึกโหมดการเล่นเรียบร้อยแล้ว: " + selectedText);
        }
    }

    // ==========================================
    // 🔹 3. ระบบบันทึก Combo ที่มากที่สุด (ใช้ใน Scene 3)
    // ==========================================
    public void SaveScore(int currentCombo)
    {
        // ดึงค่า Max Combo เดิมที่เคยบันทึกไว้ (ถ้าไม่มีค่าเดิมจะให้เป็น 0)
        int highestCombo = PlayerPrefs.GetInt("MaxCombo", 0);

        // เช็คว่า Combo รอบนี้สูงกว่าสถิติเดิมหรือไม่
        if (currentCombo > highestCombo)
        {
            PlayerPrefs.SetInt("MaxCombo", currentCombo);
            PlayerPrefs.Save();
            Debug.Log("ทำลายสถิติใหม่! Max Combo สูงสุด: " + currentCombo);

            // อัปเดตข้อความบน UI ทันที (ถ้ามีใส่ไว้)
            if (maxComboText != null)
            {
                maxComboText.text = "Max Combo: " + currentCombo;
            }
        }
    }

    // ==========================================
    // 🔹 4. ระบบบันทึกเวลาเล่นรวมทั้งหมด
    // ==========================================
    public void SavePlayTime()
    {
        if (isTimerRunning)
        {
            float totalSavedTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f) + sessionTimer;
            PlayerPrefs.SetFloat("TotalPlayTime", totalSavedTime);
            PlayerPrefs.Save();

            // รีเซ็ตเวลารอบปัจจุบันเพื่อป้องกันการนับซ้ำ
            sessionTimer = 0f;
            Debug.Log("บันทึกเวลาเล่นรวมเรียบร้อยแล้ว: " + FormatTime(totalSavedTime));
        }
    }

    // ==========================================
    // 🔹 5. ฟังก์ชันโหลดข้อมูลเก่ามาแสดงผล
    // ==========================================
    private void LoadSavedData()
    {
        if (controlTypeDropdown != null && PlayerPrefs.HasKey("SelectedControl"))
        {
            controlTypeDropdown.value = PlayerPrefs.GetInt("SelectedControl");
        }

        if (maxComboText != null)
        {
            maxComboText.text = "Max Combo: " + PlayerPrefs.GetInt("MaxCombo", 0);
        }

        if (playTimeText != null)
        {
            float totalSavedTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f);
            playTimeText.text = "Play Time: " + FormatTime(totalSavedTime);
        }
    }

    // ฟังก์ชันแปลงเวลาจากวินาทีให้เป็นรูปแบบ นาที:วินาที (00:00)
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
