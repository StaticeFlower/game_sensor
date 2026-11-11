using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login Inputs")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;

    [Header("Register Inputs")]
    public TMP_InputField regUsernameInput;
    public TMP_InputField regPasswordInput;
    public TMP_InputField regConfirmPasswordInput;

    [Header("Feedback Text")]
    public TMP_Text statusText;

    private void Start()
    {
        // เริ่มต้นให้แสดงหน้า Login และซ่อนหน้า Register
        ShowLoginPanel();
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        statusText.text = "";
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        statusText.text = "";
    }

    // ฟังก์ชันเมื่อกดปุ่ม Login
    public void OnLoginButtonClicked()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "กรุณากรอกข้อมูลให้ครบถ้วน";
            return;
        }

        // TODO: เชื่อมต่อ Backend (เช่น Firebase หรือ REST API) เพื่อตรวจสอบรหัสผ่าน
        // ตัวอย่างการจำลองระบบ Login:
        if (PlayerPrefs.GetString("User_" + username) == password)
        {
            statusText.text = "เข้าสู่ระบบสำเร็จ!";
            // สลับไปหน้าตัวเกมหลัก เช่น SceneManager.LoadScene("MainMenu");
        }
        else
        {
            statusText.text = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
        }
    }

    // ฟังก์ชันเมื่อกดปุ่ม Register
    public void OnRegisterButtonClicked()
    {
        string username = regUsernameInput.text;
        string password = regPasswordInput.text;
        string confirmPassword = regConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "กรุณากรอกข้อมูลให้ครบถ้วน";
            return;
        }

        if (password != confirmPassword)
        {
            statusText.text = "รหัสผ่านไม่ตรงกัน";
            return;
        }

        // TODO: ส่งข้อมูลไปยัง Server / Database ในการใช้งานจริง
        // ตัวอย่างบันทึกลงในเครื่องท้องถิ่น (ใช้ทดสอบเท่านั้น):
        PlayerPrefs.SetString("User_" + username, password);
        PlayerPrefs.Save();

        statusText.text = "ลงทะเบียนสำเร็จ! กรุณาเข้าสู่ระบบ";
        ShowLoginPanel();
    }
}