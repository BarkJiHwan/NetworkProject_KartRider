using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerConnect : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public void ConnectToServer()
    {       
        PhotonNetwork.ConnectUsingSettings();//���� ���ð����� ��������
        //���̾�̽��� �������Ӱ� ������ �г����� �����Ŵ
        //���� ���̾�̽��� �г��Ӱ� ������ �г����� ������
        PhotonNetwork.NickName = FirebaseDBManager.Instance.User.DisplayName;

    }
}
