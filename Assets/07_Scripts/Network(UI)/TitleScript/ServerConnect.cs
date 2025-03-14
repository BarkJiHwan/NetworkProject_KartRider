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
        PhotonNetwork.ConnectUsingSettings();
        if(FirebaseDBManager.Instance.User.DisplayName == null)
        {

        }
        //���� ���� �õ�, ����� �г��� Ȯ��, ����� �г����� ���ٸ�
        //user.DisplayName�� ���ٸ� �������� 
        //��ǲ�ʵ� ����
        
        //PhotonNetwork.NickName = FirebaseDBManager.Instance.User.DisplayName;
    }
}
