using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;

public class TitleUI : MonoBehaviour
{
    [Header("�α��� �ĳ� ����")]
    public GameObject loginPanel;
    public TMP_InputField loginEmailField;
    public TMP_InputField loginpasswordField;
    public Button loginBtn;
    public Button signUpBtn;

    [Header("ȸ������ �ǳ� ����")]
    public GameObject signUpPansl;
    public TMP_InputField signUpEmailField;
    public TMP_InputField signUpPasswordField;
    public Button signUpLoingToBtn;
    public Button signUpCompleteBtn;

    [Header("�޼��� ����")]
    public TMP_Text errorMessage;
    public TMP_Text successMessage;
    private void Start()
    {
        loginPanel.SetActive(true);
        signUpPansl.SetActive(false);
        errorMessage.gameObject.SetActive(false);
        successMessage.gameObject.SetActive(false);
    }

    public void SignUpButtonCon()
    {

        loginPanel.SetActive(false);
        signUpPansl.SetActive(true);
    }

    public void LoginToButtonCon()
    {
        loginPanel.SetActive(true);
        signUpPansl.SetActive(false);
    }
}
