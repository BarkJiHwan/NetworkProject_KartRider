using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField]public LobbyManager lobbyManager;

    [Header("�� ����� �ǳ�")]
    [SerializeField]public GameObject createRoomPanel;
    [SerializeField]public TMP_InputField roomNameInputField;
    [SerializeField]public TMP_InputField roomPasswordInputField;

    [Header("���� ���� �ǳ�")]
    [SerializeField]public GameObject roomNumberJoinPanel;
    [SerializeField] public TMP_InputField roomNumberInputField;

    [Header("�� �ɼ�")]
    [SerializeField]public GameObject roomListPanel;
    [SerializeField]public Button roomPrefab;

    [Header("�� �ɼ�")]
    [SerializeField] public GameObject roomJoinFaildePanel;
    [SerializeField] public TMP_Text roomJoinFaildeText;
    private void Start()
    {
        createRoomPanel.SetActive(false);
        roomNumberJoinPanel.SetActive(false);
    }

    public void CreateRoomPanleCon()
    {
        createRoomPanel.SetActive(true);
    }
    public void RoomNumberJoinPanelCon()
    {
        roomNumberJoinPanel.SetActive(true);
    }
    public void CreateRoomPanleCancelCon()
    {
        createRoomPanel.SetActive(false);
    }
    public void RoomNumberJoinPanelCancelCon()
    {
        roomNumberJoinPanel.SetActive(false);
    }
    public void RoomJoinFaildeText(string message)
    {
        // UI �г� �Ǵ� �˾� Ȱ��ȭ
        roomJoinFaildePanel.SetActive(true);
        roomJoinFaildeText.text = message; // Feedback �ؽ�Ʈ ����
    }
    public void RoomJoinFaildeBtn()
    {
        roomJoinFaildePanel.SetActive(false);
    }
    public void AddRoomToList(RoomInfo roomInfo)
    {
        // �� ����Ʈ ������ ����
        var roomEntry = Instantiate(roomPrefab, roomListPanel.transform);
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>();
        var roomButton = roomEntry.GetComponent<Button>();

        if (roomEntryScript != null)
        {
            roomEntryScript.SetRoomInfo(roomInfo);
        }
        roomEntry.onClick.AddListener(() => lobbyManager.JoinRoom(roomInfo.Name));
    }
}