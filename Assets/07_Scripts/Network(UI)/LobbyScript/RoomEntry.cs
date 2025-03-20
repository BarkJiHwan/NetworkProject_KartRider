using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntry : MonoBehaviour
{
    [SerializeField] public TMP_Text roomNumberText;
    [SerializeField] public TMP_Text roomNameText;
    [SerializeField] public TMP_Text playerCountText;
    [SerializeField] public TMP_Text MaxPlayersText;
    [SerializeField] public TMP_Text mapNameText;
    [SerializeField] public Image mapImage;
    [SerializeField] public Image lockIcon;
    [SerializeField] public GameObject roomNamePanel;    
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        // �� ���� ������Ʈ
        roomNumberText.text = roomInfo.CustomProperties.ContainsKey("RoomNumber")
            ? (string)roomInfo.CustomProperties["RoomNumber"]
            : "�ʳѹ�?";
        roomNameText.text = roomInfo.CustomProperties.ContainsKey("RoomName")
            ? (string)roomInfo.CustomProperties["RoomName"]
            : "���̸�?";
        playerCountText.text = $"{roomInfo.PlayerCount}";
        MaxPlayersText.text = $"/ {roomInfo.MaxPlayers}";
        mapNameText.text = roomInfo.CustomProperties.ContainsKey("MapName")
            ? (string)roomInfo.CustomProperties["MapName"]
            : "�⺻ ��";

        // �� �̹��� ����
        var mapSprite = Resources.Load<Sprite>($"Maps/{mapNameText.text}");
        mapImage.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>("Maps/default");

        // ��� ���� ����
        lockIcon.enabled = !roomInfo.IsOpen;
    }
}

