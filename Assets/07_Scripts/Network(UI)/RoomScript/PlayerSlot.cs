using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField] public PlayerPanel playerPanel;
    public bool IsEmpty => playerPanel == null; // playerPanel�� null�̸� true ��ȯ

    public void CreatePlayerPanel()
    {
        if (playerPanel == null) // ���� ������ ����ִٸ�
        {
            var instancePanel = PhotonNetwork.Instantiate("PlayerPanelPrefab", transform.position, Quaternion.identity);
            instancePanel.transform.SetParent(gameObject.transform);
            playerPanel = instancePanel.GetComponent<PlayerPanel>();
        }
        //return playerPanel; // ������ �г� ��ȯ
    }

    public void ClearPlayerPanel()
    {
        if (playerPanel != null)
        {
            Destroy(playerPanel.gameObject); // �г� ����
            playerPanel = null; // ���� ����
        }
    }
    public void AssignPlayer(int actorNumber, string playerName)
    {
        if (playerPanel == null) CreatePlayerPanel(); // �г��� ������ ����
        playerPanel.UpdatePanel(playerName); // �гο� �г��� ������Ʈ
    }

}
