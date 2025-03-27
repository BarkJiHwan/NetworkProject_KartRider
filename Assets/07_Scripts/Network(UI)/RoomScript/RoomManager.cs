using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public RoomUIManager roomUIManger;
    [SerializeField] public PlayerSlot[] playerSlots;
    
    private RoomEntry roomEntry;
    private PhotonView photonView;

    private IEnumerator Start()
    {
        photonView = GetComponent<PhotonView>();
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        RoomInfoUpdate();
        //AssignLocalPlayerToSlot();
        JoinRoom();
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            roomUIManger.startBtn.gameObject.SetActive(true);
            roomUIManger.readyBtn.gameObject.SetActive(false);
            roomUIManger.startBtn.interactable = true;
            Debug.Log(PhotonNetwork.LocalPlayer + " ��������?");
            SyncSlotsForNewPlayer(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber - 1);
        }
        else
        {
            roomUIManger.startBtn.gameObject.SetActive(false);
            roomUIManger.readyBtn.gameObject.SetActive(true);
        }
    }
    
    private void AssignLocalPlayerToSlot()
    {
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        newMasterClient = PhotonNetwork.MasterClient;
        if (PhotonNetwork.IsMasterClient)
        {
            //ResetAndBroadcastUI(); // ���ο� ������ ������ �ʱ�ȭ
        }
    }
    public void JoinRoom()
    {
        Player[] playerList = PhotonNetwork.PlayerList;
        for (int i = 0; i < playerList.Length; i++)
        {
            if (i < playerSlots.Length) //���� ���� Ȯ��
            {
                playerSlots[i].playerPanel = playerSlots[i].CreatePlayerPanel();
                playerSlots[i].actorNumber = playerList[i].ActorNumber; //���� �ѹ� �Ҵ�
                playerSlots[i].playerName = playerList[i].NickName;
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {        
            SyncSlotsForNewPlayer(newPlayer, newPlayer.ActorNumber - 1);        
    }
   
    private void SyncSlotsForNewPlayer(Player newPlayer, int slotIndex)
    {
        photonView.RPC("UpdateSlotForNewPlayer", RpcTarget.AllBuffered , newPlayer.ActorNumber, newPlayer.NickName, slotIndex);
    }
    [PunRPC]
    public void UpdateSlotForNewPlayer(int actorNumber, string playerName, int slotIndex)
    {
        Debug.Log(playerName +"���ϴ�." + slotIndex +"���� �ε���" + actorNumber +"��Ʈ�ѹ�");
        Debug.Log(playerSlots[slotIndex] + "���� �ѹ�");
        Debug.Log(playerSlots[slotIndex].actorNumber + "���� ���� �ѹ�");
        Debug.Log(playerSlots[slotIndex].playerName + "���� �÷��̾� �̸�");        
        Debug.Log(playerSlots[slotIndex].playerPanel + "������Ʈ");
        playerSlots[slotIndex].actorNumber = actorNumber; //���� �ѹ� �Ҵ�
        playerSlots[slotIndex].playerName = playerName;
        if (PhotonNetwork.IsMasterClient)
        {
            if (playerSlots[slotIndex].actorNumber == actorNumber)
            {
                Debug.Log("������");
                playerSlots[slotIndex].actorNumber = actorNumber; //���� �ѹ� �Ҵ�
                playerSlots[slotIndex].playerPanel.transform.SetParent(playerSlots[slotIndex].transform);
                playerSlots[slotIndex].playerPanel.PlayerNameText.text = playerName;
            }
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //ResetAndBroadcastUI();
        }
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
    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            //�� �������� ���̵�
            //PhotonNetwork.LoadLevel("InGameScene");
        }
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
