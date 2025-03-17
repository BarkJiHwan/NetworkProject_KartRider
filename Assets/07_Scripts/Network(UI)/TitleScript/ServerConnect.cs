using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerConnect : MonoBehaviourPunCallbacks
{    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();//���� ���ð����� ��������
    }
    
    public bool Connect()
    {
        return PhotonNetwork.IsConnected;
    }


    public override void OnConnectedToMaster()
    {
        //���̾�̽��� �������Ӱ� ������ �г����� �����Ŵ        
        PhotonNetwork.NickName = FirebaseDBManager.Instance.User.DisplayName; 
    }
}
