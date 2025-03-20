using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LobbyUIManager lobbyUiMgr;
    [SerializeField] private RoomEntry roomEntry;

    private void Start()
    {
        PhotonNetwork.JoinLobby();
    }

    public void CreateRoomBtnClick()
    {
        CreateRoom(lobbyUiMgr.roomNameInputField.text, lobbyUiMgr.roomPasswordInputField.text);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomEntry.roomNameText.text);
    }
    public override void OnJoinedRoom()
    {
        //�� ������ �ڷ�ƾ���� ������
        SceneCont.Instance.SceneAsync("RoomScene");
        SceneCont.Instance.Oper.allowSceneActivation = true;
    }

    public void CreateRoom(string roomName, string password)
    {
        Hashtable custom = new Hashtable
        {
            { "RoomName", roomName },
            { "Password", password }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 1,
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password" }
        };
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // ���� �� ����Ʈ ����
        foreach (Transform child in lobbyUiMgr.roomListPanel.transform)
        {
            Destroy(child.gameObject);
        }

        int roomIndex = 1;
        // ���ο� �� ����Ʈ ������Ʈ
        foreach (RoomInfo roomInfo in roomList)
        {
            string roomNumber = roomIndex.ToString("D6"); // 6�ڸ� �������� ��ȯ (��: 000001)
            roomInfo.CustomProperties["RoomNumber"] = roomNumber;
            lobbyUiMgr.AddRoomToList(roomInfo);
            roomIndex++;            
        }
    }
    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            return;
        }
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        switch (returnCode)
        {
            case 32765: // ���� ���� ��
                lobbyUiMgr.RoomJoinFaildeText("�ڸ��� ���� ������ �� �����ϴ�.");
                break;

            case 32764: // �� ���� �Ǵ� ���� ����
                lobbyUiMgr.RoomJoinFaildeText("������ �̹� �������̶� ������ �� �����ϴ�.");
                break;

            case 32762: // ���� ����
                lobbyUiMgr.RoomJoinFaildeText("�Է��Ͻ� ��й�ȣ�� ��ġ���� �ʽ��ϴ�. �ٽ� Ȯ���� �ּ���.");
                break;

            default: // ��Ÿ ���� ����                
                lobbyUiMgr.RoomJoinFaildeText($"�� ���忡 �����߽��ϴ�. ���� �ڵ�: {returnCode}");
                break;
        }
    }
}