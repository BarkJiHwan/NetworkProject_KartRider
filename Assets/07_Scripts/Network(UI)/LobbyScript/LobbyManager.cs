using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LobbyUIManager lobbyUiMgr;
    [SerializeField] private RoomEntry roomEntry;
    private List<RoomInfo> currentRoomList = new List<RoomInfo>();
    private void Start()
    {
        PhotonNetwork.JoinLobby();
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }
    public void CreateRoomBtnClick()
    {
        string roomName = lobbyUiMgr.roomNameInputField.text;
        string passwrod = lobbyUiMgr.roomPasswordInputField.text;
        int roomIndex = GetNextRoomIndex();
        //if(string.IsNullOrEmpty(roomName) )
        //{
        //�� �̸��� ����ִٸ� ����Ʈ�� ����� �͵� �� �ϳ��� ����ֱ� ���߿�
        //}
        if (string.IsNullOrEmpty(passwrod))
        {
            passwrod = null;
        }
        CreateRoom(roomName, passwrod, roomIndex);
    }
    private int GetNextRoomIndex()
    {
        return currentRoomList.Count + 1;
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomEntry.roomNameText.text);
    }
    public override void OnJoinedRoom()
    {
        StartCoroutine(LoadJoinRoom("RoomScene"));
    }    
    IEnumerator LoadJoinRoom(string sceneName)
    {
        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync(sceneName);
        SceneCont.Instance.Oper.allowSceneActivation = false;

        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                SceneCont.Instance.Oper.allowSceneActivation = true;
            }
            yield return null;
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
    public void CreateRoom(string roomName, string password, int roomNumber)
    {//ũ������Ʈ �� Ŀ���� �ϴ� ��
        Hashtable custom = new Hashtable
        {
            { "RoomName", roomName },//�� �̸���
            { "Password", password },//�н����带 Ȯ���Ͽ� �ʱ�ȭ
            { "RoomNumber", roomNumber }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8,
            //�ʱ� ���� ���� �� Ŀ���� ������Ƽ�� custom ���� ����
            CustomRoomProperties = custom,
            //Ŀ���� �� ������Ƽ�� �����ͼ� ����
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password","RoomNumber" },            
        };
        //Ŀ���� �� ������ ���� ��¥ ���� ������.
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public void RandomCreateRoom(string roomName, string password, int roomNumber)
    {//ũ������Ʈ �� Ŀ���� �ϴ� ��
        Hashtable custom = new Hashtable
        {
            { "RoomName", roomName },//�� �̸���            
            { "RoomNumber", roomNumber }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8,
            //�ʱ� ���� ���� �� Ŀ���� ������Ƽ�� custom ���� ����
            CustomRoomProperties = custom,
            //Ŀ���� �� ������Ƽ�� �����ͼ� ����
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "RoomNumber" },
        };
        //���� ã�ų� ���� ��� ���ǿ� ���� ���� ����
        PhotonNetwork.JoinRandomOrCreateRoom(custom, 8, MatchmakingMode.RandomMatching,TypedLobby.Default, null, roomName, roomOptions, null);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCurrentRoomList(roomList);
        Dictionary<string, RoomEntry> existingRoomEntries = new Dictionary<string, RoomEntry>();
        // ���� UI���� RoomEntry���� ����
        foreach (Transform child in lobbyUiMgr.roomListPanel.transform)
        {
            var roomEntry = child.GetComponent<RoomEntry>();
            if (roomEntry != null)
            {
                existingRoomEntries[roomEntry.roomNameText.text] = roomEntry;
            }
        }

        // ������Ʈ�� �� ����Ʈ ó��
        foreach (RoomInfo roomInfo in roomList)
        {
            if (existingRoomEntries.ContainsKey(roomInfo.Name))
            {
                // ���� �̹� �����ϸ� UI ������Ʈ
                existingRoomEntries[roomInfo.Name].SetRoomInfo(roomInfo);
            }
            else
            {
                // ���� �߰��� �游 UI ����
                AddRoomToList(roomInfo);
            }
            // ���� ��Ͽ��� ó���� �� ���� (�߰��� �� ����� ����)
            existingRoomEntries.Remove(roomInfo.Name);
        }
        // ������ �� UI ����
        foreach (var remainingEntry in existingRoomEntries.Values)
        {
            Destroy(remainingEntry.gameObject);
        }
    }
    public void UpdateCurrentRoomList(List<RoomInfo> roomList)
    {
        currentRoomList.Clear();
        currentRoomList.AddRange(roomList);
    }
    public void RoomResetBtnClick()
    {
        OnRoomListUpdate(currentRoomList);
    }
    public void AddRoomToList(RoomInfo roomInfo)
    {
        // �� ����Ʈ ������ ����
        var roomEntry = Instantiate(lobbyUiMgr.roomPrefab, lobbyUiMgr.roomListPanel.transform);
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>();
        if (roomEntryScript != null)
        {
            //���� ���� ������ �ѱ�
            roomEntryScript.SetRoomInfo(roomInfo);

        }
        roomEntry.onClick.AddListener(() =>
        {
            // �� ���� üũ
            bool hasPassword = roomEntryScript.IsPasswrod(roomInfo);
            bool isGameStart = roomEntryScript.IsGameStarted(roomInfo);
            bool isRoomFull = roomEntryScript.IsRoomFull(roomInfo);

            if (hasPassword)
            {
                ShowPasswordPrompt(roomInfo.Name, roomInfo.CustomProperties["Password"] as string);
                return;
            }
            else if (isGameStart)
            {
                lobbyUiMgr.RoomJoinFaildeText("������ �̹� �������̶� ������ �� �����ϴ�.");
                return;
            }
            else if (isRoomFull)
            {
                lobbyUiMgr.RoomJoinFaildeText("�ڸ��� ���� ������ �� �����ϴ�.");
                return;
            }
            else if (roomInfo == null)
            {
                lobbyUiMgr.RoomJoinFaildeText("�����Ϸ��� ���� �����ϴ�.");
                return;
            }
            JoinRoom(roomInfo.Name);
        });
    }
    public void ShowPasswordPrompt(string roomName, string correctPassword)
    {
        // ��й�ȣ �Է�â�� Ȱ��ȭ
        lobbyUiMgr.LockRoomPasswrodPanelActive(true);

        // Ȯ�� ��ư ���� ����
        lobbyUiMgr.lockRoomConnectBtn.onClick.AddListener(() =>
        {
            string enteredPassword = lobbyUiMgr.lockRoomPasswordInputField.text;
            //��й�ȣ �Է� �� ������ �ߴٸ�
            lobbyUiMgr.LockRoomPasswrodPanelActive(false);
            if (enteredPassword == correctPassword)
            {                
                JoinRoom(roomName); // ��й�ȣ ��ġ �� �� ����
            }
            else
            {
                lobbyUiMgr.RoomJoinFaildeText("�Է��Ͻ� ��й�ȣ�� ��ġ���� �ʽ��ϴ�. �ٽ� Ȯ���� �ּ���.");
            }
        });
    }
}