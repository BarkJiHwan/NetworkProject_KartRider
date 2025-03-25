using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;



public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public RoomUIManager roomUIManger;
    public PlayerInfo playerInfo;
    
    private Player[] players = PhotonNetwork.PlayerList;
    private RoomEntry roomEntry;
    private Dictionary<Player, PlayerInfo> playerDic = new Dictionary<Player, PlayerInfo>();

    private void Start()
    {
        RoomInfoUpdate();

        if (PhotonNetwork.IsMasterClient)
        {
            roomUIManger.startBtn.gameObject.SetActive(true);
            roomUIManger.readyBtn.gameObject.SetActive(false);
            //��� �÷��̾ �غ�Ϸ� �Ǹ� Ʈ��� �ٲٱ�(������Ʈ RPC���ߵ�)
            roomUIManger.startBtn.interactable = true;
            
        }
        else
        {
            roomUIManger.startBtn.gameObject.SetActive(false);
            roomUIManger.readyBtn.gameObject.SetActive(true);
        }
    }
    private void UpdateSlots()
    {
        
    }

    public void SetRoomInfoChange()
    {
        roomUIManger.roomInfoChangePanel.gameObject.SetActive(false);
        string roomName = roomUIManger.roomNameChangeField.text;
        string roomPassword = roomUIManger.roomPasswordChangeField.text;

        Hashtable newRoomInfo = new Hashtable
        {
            { "RoomName", roomName},
            { "Password", roomPassword}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newRoomInfo);
    }
    public void SetRoomMapChange()
    {
        string MapName = roomUIManger.roomMapNameText.text;
        Hashtable newMapInfo = new Hashtable
        {
            { "Map", MapName}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newMapInfo);
    }
    public void RoomInfoUpdate()
    {
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        roomUIManger.roomNameText.text = roomProperties.ContainsKey("RoomName") ? (string)roomProperties["RoomName"] : "�� �̸� ����";
        roomUIManger.roomNumberText.text = roomProperties.ContainsKey("RoomNumber") ? (string)roomProperties["RoomNumber"] : "�� ��ȣ ����";
        string mapName = roomProperties.ContainsKey("Map") ? (string)roomProperties["Map"]
        : "default";
        
        var mapSprite = Resources.Load<Sprite>($"Maps/{mapName}");
        roomUIManger.roomMapeImg.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>("Maps/default");

        bool hasPassword = roomProperties.ContainsKey("Password") && !string.IsNullOrEmpty((string)roomProperties["Password"]);
        roomUIManger.roomPasswordText.text = hasPassword ? (string)roomProperties["Password"] : null;
        roomUIManger.SetPasswordUI(hasPassword);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        RoomInfoUpdate(); //����� �� �Ӽ��� �뿡 �ݿ�        
    }
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        SetRoomInfoChange();

    }
    public override void OnLeftLobby()
    {
        Debug.Log("�κ� �����߽��ϴ�.");
    }
    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            //�� �������� ���̵�
            //PhotonNetwork.LoadLevel("InGameScene");
        }
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {        
        foreach (var player in players)
        {            
            if (PhotonNetwork.IsMasterClient == true)
            {
                roomUIManger.startBtn.gameObject.SetActive(true);
                roomUIManger.readyBtn.gameObject.SetActive(false);
                //��� �÷��̾ �غ�Ϸ� �Ǹ� Ʈ��� �ٲٱ�(������Ʈ RPC���ߵ�)
                roomUIManger.startBtn.interactable = true;
                return;
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //���� ����� ���� UI������Ʈ.
        UpdateSlots();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //���� ��Ż�� �ش� ������ �ִ� �κ��� UI������Ʈ
        UpdateSlots();
    }
    public void UpdatePlayerUIList()
    {
        for(int i = 0; i < players.Length; i++)
        {
            if(players[i] != null) 
            {
                UpdateSlots();
                //Instantiate(roomUIManger.PlayerInfo, )
            }
            else
            {
            }
        }
    }
    public void OutRoomBtn()
    {
        if (PhotonNetwork.InRoom)
        {            
            StartCoroutine(LoadJoinLobby("LobbyScene"));
        }
    }
    IEnumerator LoadJoinLobby(string sceneName)
    {
        PhotonNetwork.LeaveRoom();
        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync(sceneName);
        SceneCont.Instance.Oper.allowSceneActivation = false;
        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                //�� ���� ��� �ൿ ������Ű��.. �κ� �̵���... ����
            }
            else
            {
                break;
            }
        }        
        yield break; 
    }
    public override void OnLeftRoom()
    {        
        SceneCont.Instance.Oper.allowSceneActivation = true;
    }        

    public void PlayerBtnController()
    {//�غ� ���°� �ƴ϶�� �غ���·θ����
        if(roomUIManger.readyBtn.gameObject.activeSelf)
        {
            roomUIManger.readyBtn.gameObject.SetActive(false);
            roomUIManger.readyCanCelBtn.gameObject.SetActive(true);
        }
        else
        {//�غ���¶�� �غ� ���
            roomUIManger.readyBtn.gameObject.SetActive(true);
            roomUIManger.readyCanCelBtn.gameObject.SetActive(false);
        }
    }    
}
