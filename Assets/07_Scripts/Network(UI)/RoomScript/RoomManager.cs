using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class RoomManager : MonoBehaviourPunCallbacks
{
    

    //    public void CreateRoomSettings()
    //    {
    //        if (PhotonNetwork.IsMasterClient)
    //        {
    //            Hashtable hashtable = new Hashtable
    //            {
    //                { "mapName", "SpeedTrack" },  // �� �̸�
    //                { "mapImg", "speedtrack.png" },  // �� �̹��� ���
    //                { "gameMode", "team" },  // ���� ���: ����
    //                { "roomNumber", 101 }  // �� ��ȣ
    //            };
    //            PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
    //        }
    //    }
}
