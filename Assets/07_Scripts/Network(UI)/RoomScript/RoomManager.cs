using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public RoomUIManager roomUIManger;
    [SerializeField] public PlayerSlot[] playerSlots;

    bool allReady = false;
    private bool isReadyTextChange = false;
    private WaitForSeconds WFownS = new WaitForSeconds(1f); //1�� ��� �ڷ�ƾ���� ���
    private WaitForSeconds WFthreeS = new WaitForSeconds(3f); //1�� ��� �ڷ�ƾ���� ���
    private bool isCorRunning = false;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.AutomaticallySyncScene = true;
        RoomInfoUpdate();
        JoinRoom();
        InitializeUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var slot in playerSlots)
            {
                if(slot.actorNumber == newMasterClient.ActorNumber)
                {
                    slot.playerPanel.readyImage.gameObject.SetActive(false);
                }
            }
            roomUIManger.startBtnText.text = "���� ����";
            roomUIManger.startBtn.image.color = new Color(1, 1, 1, 0.5f);
        }
    }
    private void InitializeUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            roomUIManger.startBtnText.text = "���� ����";
            roomUIManger.startBtn.image.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            isReadyTextChange = true;
            roomUIManger.startBtnText.text = "�غ� �Ϸ�";
        }
    }
    public void JoinRoom()
    {
        var playerPanel = PhotonNetwork.Instantiate("PlayerPanelPrefab", transform.position, Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {        
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach(var slot in playerSlots)
        {
            if (slot.actorNumber == otherPlayer.ActorNumber)
            {
                slot.isReady = false;
                slot.actorNumber = 0;
                slot.playerPanel = null;
            }            
        }
        UpdateAllPlayersReady();
    }

    public void OutRoomBtn()
    {
        if (PhotonNetwork.InRoom)
        {
            StartCoroutine(LoadJoinLobby("LobbyScene"));
        }
    }
    public override void OnLeftRoom()
    {
        SceneCont.Instance.Oper.allowSceneActivation = true;
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
    public void SetRoomInfoChange()
    {
        roomUIManger.roomInfoChangePanel.gameObject.SetActive(false);
        string roomName = roomUIManger.roomNameChangeField.text;
        string roomPassword = roomUIManger.roomPasswordChangeField.text;

        Hashtable newRoomInfo = new Hashtable
        {
            { "RoomName", roomName},
            { "Password", roomPassword},
            { "IsGameStart", false }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newRoomInfo);
    }
    public void SetRoomMapChange()
    {
        roomUIManger.trackSelectPanel.gameObject.SetActive(false);
        string roomMap = roomUIManger.roomMapNameText.text;
        Hashtable newMapInfo = new Hashtable
        {
            { "Map", roomMap }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(newMapInfo);
    }
    public void RoomInfoUpdate()
    {
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        roomUIManger.roomNameText.text = roomProperties.ContainsKey("RoomName") ? (string)roomProperties["RoomName"] : "�� �̸� ����";
        roomUIManger.roomNumberText.text = roomProperties.ContainsKey("RoomNumber") ? (string)roomProperties["RoomNumber"] : "�� ��ȣ ����";
        roomUIManger.roomMapNameText.text = roomProperties.ContainsKey("Map") ? (string)roomProperties["Map"]
        : "default"; ;
        var mapSprite = Resources.Load<Sprite>($"Maps/{roomUIManger.roomMapNameText.text}");
        roomUIManger.roomMapeImg.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>($"Maps/{roomUIManger.roomMapNameText.text}");

        bool hasPassword = roomProperties.ContainsKey("Password") && !string.IsNullOrEmpty((string)roomProperties["Password"]);
        roomUIManger.roomPasswordText.text = hasPassword ? (string)roomProperties["Password"] : null;
        roomUIManger.SetPasswordUI(hasPassword);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (propertiesThatChanged.ContainsKey("AllPlayersReady"))
            {                
                roomUIManger.startBtn.image.color = new Color(1, 1, 1, 1f);
            }
            else
            {
                roomUIManger.startBtn.image.color = new Color(1, 1, 1, 0.5f);
            }
        }
        RoomInfoUpdate(); //����� �� �Ӽ��� �뿡 �ݿ�
    }
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        RoomInfoUpdate();
    }

    //�� �������� ���̵�
    public void StartGameBtn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            {
                if (!isCorRunning)
                {
                    string massage = "ȥ�ڼ��� ������ ������ �� �����ϴ�. �ٸ� ���̼��� ��ٷ��ּ���.";
                    StartCoroutine(progressMessageCor(massage));
                }
                return;
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AllPlayersReady") &&
    (bool)PhotonNetwork.CurrentRoom.CustomProperties["AllPlayersReady"])
            {
                Hashtable gameStart = new Hashtable
                {
                    {"IsGameStart", true }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(gameStart);
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.LoadLevel(roomUIManger.roomMapNameText.text);
            }
            else
            {
                if (!isCorRunning)
                {
                    string massage = "��� ���̼��� �غ� ���� �ʾ� ������ ������ �� �����ϴ�.";
                    StartCoroutine(progressMessageCor(massage));
                }
            }
        }
        else
        {
            StartCoroutine(ReadyBtnEnable());
            if (isReadyTextChange)
            {
                isReadyTextChange = false;
                roomUIManger.startBtnText.text = "�غ� ���";                
            }
            else
            {
                isReadyTextChange = true;
                roomUIManger.startBtnText.text = "�غ� �Ϸ�";
            }
        }
    }
    IEnumerator ReadyBtnEnable()
    {
        roomUIManger.startBtn.interactable = false;
        yield return WFownS;
        roomUIManger.startBtn.interactable = true;
    }
    IEnumerator progressMessageCor(string massage)
    {
        isCorRunning = true;
        roomUIManger.progressMessagePanel.gameObject.SetActive(true);
        roomUIManger.progressMessage(massage);
        yield return WFthreeS;
        roomUIManger.progressMessage("");
        roomUIManger.progressMessagePanel.gameObject.SetActive(false);
        isCorRunning = false;
    }

    public bool AllPlayersReady()
    {
        foreach (var slot in playerSlots)
        {
            if(slot.playerPanel != null && !PhotonNetwork.IsMasterClient)
            {
                if (slot.actorNumber > 0 && slot.isReady == false)
                {
                    return false; //�غ���� ���� �÷��̾ ������ false ��ȯ                
                }
            }
        }
        return true; //��� ������ �غ� �Ϸ�
    }

    public void UpdateAllPlayersReady()
    {        
        if(PhotonNetwork.IsMasterClient)
        {
            allReady = true;
            foreach (var slot in playerSlots)
            {
                if (slot.actorNumber > 0 && slot.isReady == false && !slot.playerPanel.photonView.Owner.IsMasterClient)
                {
                    allReady = false;
                    break;
                }                
            }
        }
        Hashtable start = new Hashtable
        {
            {"AllPlayersReady", allReady}
        };
        Debug.Log(allReady+ "���� �ܰ� ���ӽ��� ����?");
        PhotonNetwork.CurrentRoom.SetCustomProperties(start);
    }
}