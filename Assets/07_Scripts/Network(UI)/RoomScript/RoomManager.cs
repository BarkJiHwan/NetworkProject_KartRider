using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;


public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public RoomUIManager roomUIManger;
    private Player[] players = PhotonNetwork.PlayerList;
    private RoomEntry roomEntry;
    private void Start()
    {
        RoomInfoUpdate();
        foreach (var player in players)
        {
            //�̹����� ����ָ� ��
            //�迭�� 0 ������ ���ʴ��
            Debug.Log("�� ���� ����� ���"+ player.NickName);
        }
        
        if (PhotonNetwork.IsMasterClient)
        {
            roomUIManger.startBtn.gameObject.SetActive(true);
            roomUIManger.readyBtn.gameObject.SetActive(false);
            //��� �÷��̾ �غ�Ϸ� �Ǹ� Ʈ��� �ٲٱ�(������Ʈ RPC���ߵ�)
            roomUIManger.startBtn.interactable = true;
            int max = PhotonNetwork.CurrentRoom.MaxPlayers - 1;

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
            {
                {"0", PhotonNetwork.LocalPlayer.ActorNumber }, { "1",0},
                { "2", 2<= max ? 0 : -1 }, { "3", 3 <= max ? 0 : -1 }, { "4",4 <= max ? 0: -1  },
                { "5", 5 <= max ? 0 : -1 }, {"6",  6 <= max ? 0 : -1}, {"7", 7<= max ? 0 : -1}
                
            });
        }        
        else
        {
            roomUIManger.startBtn.gameObject.SetActive(false);
            roomUIManger.readyBtn.gameObject.SetActive(true);            
        }
    }
    public void SetRoomInfoChange()
    {
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
        roomUIManger.roomPasswordText.text = (string)roomProperties["Password"];
        roomUIManger.SetPasswordUI(hasPassword);
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        RoomInfoUpdate(); // ����� �� �Ӽ��� UI�� �ݿ�
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
                roomUIManger.startBtn.interactable = true;
                roomUIManger.readyBtn.interactable = false;
                return;
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //���� ����� ���� UI������Ʈ.
        UpdatePlayerUIList();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //���� ��Ż�� �ش� ������ �ִ� �κ��� UI������Ʈ
        UpdatePlayerUIList();
    }
    public void UpdatePlayerUIList()
    {
        for(int i = 0; i < players.Length; i++)
        {

        }
    }
    public void OutRoomBtn()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("�ϴ� ���� �½��ϴ�.");
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
        Debug.Log("�ɴϱ�?");
        yield break; 
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Ȯ���� ��Ż��");
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
