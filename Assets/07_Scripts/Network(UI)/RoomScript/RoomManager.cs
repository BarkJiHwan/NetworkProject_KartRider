using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;


public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] public RoomUIManager roomUIManger;
    private Player[] players = PhotonNetwork.PlayerList;
    private void Start()
    {        
        foreach (var player in players)
        {
            //�̹����� ����ָ� ��
            //�迭�� 0 ������ ���ʴ��
            Debug.Log("�� ���� ����� ���"+ player.NickName);
        }
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            roomUIManger.startBtn.interactable =false;
            roomUIManger.readyBtn.interactable = true;
            //���� �����Ͱ� �ƴ϶�� ��ŸƮ ��ư�� �ƴ϶� �غ�Ϸ� ��ư���� ����
        }
    }
    public override void OnLeftLobby()
    {
        Debug.Log("�κ� �����߽��ϴ�.");
    }
    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient == true)
        {
            //�� �������� ���̵�
            PhotonNetwork.LoadLevel("InGameScene");
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
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //���� ��Ż�� �ش� ������ �ִ� �κ��� UI������Ʈ
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
                Debug.Log(SceneCont.Instance.Oper.progress);
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
    public void JoinTestBtn()
    {
        Debug.Log(PhotonNetwork.IsConnected + "������");
        Debug.Log(PhotonNetwork.InLobby + "�κ�");
        Debug.Log(PhotonNetwork.InRoom + "��");
    }
}
