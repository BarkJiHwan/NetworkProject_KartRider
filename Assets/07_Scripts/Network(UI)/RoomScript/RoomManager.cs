using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class RoomManager : MonoBehaviourPunCallbacks
{

    private void Start()
    {
        Player[] players = PhotonNetwork.PlayerList;

        foreach (var player in players)
        {
            //�̹����� ����ָ� ��
            //�迭�� 0 ������ ���ʴ��
            Debug.Log("�� ���� ����� ���"+ player.NickName);
        }
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            //���� �����Ͱ� �ƴ϶�� ��ŸƮ ��ư�� �ƴ϶� �غ�Ϸ� ��ư���� ����
        }
    }

    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient == true)
        {
            //�� �������� ���̵�
            PhotonNetwork.LoadLevel("InGameScene");
        }
    }
}

