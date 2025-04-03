using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class RoomUIManager : MonoBehaviour
{
    [Header("�� �������̼�")]
    public Button roomTitleChangeBtn;
    public TMP_Text roomNumberText;
    public TMP_Text roomNameText;
    public TMP_Text roomPasswordText;
    public TMP_Text roomMapNameText;
    public Image roomMapeImg;
       
    [Header("�� �̸� ����")]
    public GameObject roomInfoChangePanel;
    public TMP_InputField roomNameChangeField;
    public TMP_InputField roomPasswordChangeField;

    [Header("�غ� ��ư")]
    public Button startBtn;
    public TMP_Text startBtnText;

    [Header("�� ��й�ȣ ������")]
    public Image passwordGroup;

    [Header("��(Ʈ������) ��ư")]
    public Button MapChangeBtn;

    [Header("�� ������ ��ư")]
    public Button exitBtn;

    [Header("īƮ �ǳ�")]
    public GameObject kartPanel;
    public Button kartRightBtn;
    public Button kartLeftBtn;
    public Button kartSelectBtn;
    public Image kartImg;
    public Button kartPanelBtn;

    [Header("ĳ���� �ǳ�")]
    public GameObject characterPanel;
    public Button characterRightBtn;
    public Button characterLeftBtn;
    public Button characterSelectBtn;
    public Image characterImg;
    public Button characterPanelBtn;

    [Header("Ʈ�� ���� �ǳ�")]
    public GameObject trackSelectPanel;
    public MapList mapListPanel;
    public Image selectMapImage;
    public TMP_Text SelectMapName;
    public Button trackSelectBtn;

    [Header("�ȳ��޼��� �ǳ�")]
    public GameObject progressMessagePanel;
    public TMP_Text progressMessageText;

    public void SetPasswordUI(bool hasPassword)
    {
        passwordGroup.gameObject.SetActive(hasPassword);
    }

    public void RoomInfoChangePanelBtn()
    {
        roomInfoChangePanel.gameObject.SetActive(true);
    }
    public void RoominfoChangeCancelBtn()
    {
        roomInfoChangePanel.gameObject.SetActive(false);
    }
    
    public void TrackSelectPanelBtn()
    {
        trackSelectPanel.gameObject.SetActive(true);
    }   
    public void TrackSelectBtn()
    {
        trackSelectPanel.gameObject.SetActive(false);
    }

    public void progressMessage(string massage)
    {
        progressMessageText.text = massage;
    }
}
