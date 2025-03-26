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

    private Player[] players = PhotonNetwork.PlayerList;
    private RoomEntry roomEntry;
    private PhotonView photonView;

    private IEnumerator Start()
    {
        photonView = GetComponent<PhotonView>();
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        RoomInfoUpdate();
        //AssignLocalPlayerToSlot();
        InitializeUI();
        if(PhotonNetwork.LocalPlayer != null)
        {
            AssignLocalPlayerToSlot();
        }
    }

    private void InitializeUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            roomUIManger.startBtn.gameObject.SetActive(true);
            roomUIManger.readyBtn.gameObject.SetActive(false);
            roomUIManger.startBtn.interactable = true;
            
        }
        else
        {
            roomUIManger.startBtn.gameObject.SetActive(false);
            roomUIManger.readyBtn.gameObject.SetActive(true);
        }
    }

    private void AssignLocalPlayerToSlot()
    {
        foreach(var slot in playerSlots)
        {
            if (slot.IsEmpty) // ������ ��� �ִ��� Ȯ��
            {
                int slotIndex = Array.IndexOf(playerSlots, slot);
                Debug.Log($"{slotIndex}�� ���Կ� �Ҵ��");

                // ���ÿ��� ���� �ʱ�ȭ
                slot.CreatePlayerPanel();
                slot.playerPanel.UpdatePanel(PhotonNetwork.LocalPlayer.NickName);

                // �ٸ� Ŭ���̾�Ʈ�� ���� ���� ����
                photonView.RPC("UpdateSlotForAllClients", RpcTarget.All, slotIndex,PhotonNetwork.LocalPlayer.NickName);
                break;
            }
        }
    }

    [PunRPC]
    public void UpdateSlotForAllClients(int slotIndex, string playerName)
    {
        if (slotIndex >= 0 && slotIndex < playerSlots.Length) // ���� �ε��� ��ȿ�� Ȯ��
        {
            Debug.Log($"{slotIndex}�� ���� ������Ʈ ��");

            // �г��� �̹� ������ ��� �ߺ� ���� ����
            if (playerSlots[slotIndex].playerPanel == null)
            {
                playerSlots[slotIndex].CreatePlayerPanel();
            }

            // ���� �����͸� ����� �ʵ��� ������ ���Ÿ� ����
            if (playerSlots[slotIndex].playerPanel.PlayerNameText.text != playerName)
            {
                playerSlots[slotIndex].playerPanel.UpdatePanel(playerName);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //ResetAndBroadcastUI(); // ���ο� ������ ������ �ʱ�ȭ
        }

    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // ���� ���� ������ ���ο� �÷��̾�� ����
            SyncSlotsForNewPlayer(newPlayer);
        }

    }
    private void SyncSlotsForNewPlayer(Player newPlayer)
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (!playerSlots[i].IsEmpty) // ��� ���� ���� ���Ը� ����
            {
                photonView.RPC("UpdateSlotForNewPlayer", newPlayer, i, playerSlots[i].playerPanel.PlayerNameText.text);
            }
        }
    }
    [PunRPC]
    public void UpdateSlotForNewPlayer(int slotIndex, string playerName)
    {
        if (slotIndex >= 0 && slotIndex < playerSlots.Length) // ���� ��ȿ�� Ȯ��
        {
            if (playerSlots[slotIndex].playerPanel == null) // �г��� ���� ��� ����
            {
                playerSlots[slotIndex].CreatePlayerPanel();
            }

            // ���Կ� ������ ������Ʈ
            playerSlots[slotIndex].playerPanel.UpdatePanel(playerName);
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //ResetAndBroadcastUI();
        }
    }
    private void ResetAndBroadcastUI()
    {
        foreach (var slot in playerSlots)
        {
            slot.ClearPlayerPanel(); // ��� ���� �ʱ�ȭ
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            foreach (var slot in playerSlots)
            {
                if (slot.IsEmpty)
                {
                    slot.AssignPlayer(player.ActorNumber, player.NickName);
                    break;
                }
            }
        }
        //photonView.RPC("UpdateUIForAllClients", RpcTarget.Others, PhotonNetwork.PlayerList);
    }
    [PunRPC]
    public void UpdateUIForAllClients(Player[] players)
    {
        //foreach (var slot in playerSlots)
        //{
        //    slot.ClearPlayerPanel(); // ��� ���� �ʱ�ȭ
        //}
        //foreach (var player in players)
        //{
        //    foreach (var slot in playerSlots)
        //    {
        //        if (slot.IsEmpty)
        //        {
        //            slot.AssignPlayer(player.ActorNumber, player.NickName);
        //            break;
        //        }
        //    }
        //}
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
