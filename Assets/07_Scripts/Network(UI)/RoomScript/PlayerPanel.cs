using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

/// <summary>
/// �÷��̾� ������Ʈ ��ũ��Ʈ
/// �÷��̾��� �̹���, �г���, ���ͳѹ� (īƮ����)��.. ǥ�� ��
/// </summary>
public class PlayerPanel : MonoBehaviourPun
{
    [Header("Player ����")]
    public Image playerImg;
    public TMP_Text PlayerNameText;
    public Image playerIcon;
    public CharacterSo character;

    [Header("�غ� �Ϸ� �̹���")]
    public Image readyImage;

    [SerializeField] private RoomManager roomManager;
    private void Start()
    {
        roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        if (photonView.IsMine)
        {
            //���Ŀ� ���� ����� Ȯ���� �ؾ��ϱ� ������ RpcTarget.AllBuffered���
            GetComponent<PhotonView>().RPC("SetOwnInfo", RpcTarget.AllBuffered);
        }
    }

    /// <summary>
    /// �÷��̾� �ǳ��� ������ RoomManager��ũ��Ʈ���� PhotonNetwork.Instantiate�� ����
    /// ����� ���� PhotonView.IsMine���� ���� ����� PunRPC�� ��ο��� �Ѹ�
    /// ������ ���鼭 �� ������ ���� ã�� �� �ڽ��� ������ ������,
    /// ��ư�� StartBtnClickTrigger()�޼��带 �����ѵ� ������ �ڽ����� ��ġ�� �ű�
    /// ������ �Ϸ� �Ǹ� ��ġ ���� ũ�⸦ ������
    /// </summary>
    [PunRPC]
    public void SetOwnInfo()
    {
        if(roomManager == null)
        {            
            roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        }
        PlayerNameText.text = photonView.Controller.NickName;
        //playerImg.sprite = character.characterIcon;
        for (int i = 0; i < roomManager.playerSlots.Length; i++)
        {
            if (roomManager.playerSlots[i].playerPanel == null)
            {
                roomManager.playerSlots[i].playerPanel = GetComponent<PlayerPanel>();
                roomManager.playerSlots[i].actorNumber = photonView.Owner.ActorNumber;
                roomManager.roomUIManger.startBtn.onClick.AddListener(roomManager.playerSlots[i].playerPanel.StartBtnClickTrigger);
                roomManager.playerSlots[i].isReady = false;
                transform.SetParent(roomManager.playerSlots[i].transform);
                roomManager.UpdateAllPlayersReady();
                break;
            }
        }
        GetComponent<RectTransform>().anchorMin = Vector3.zero;
        GetComponent<RectTransform>().anchorMax = Vector3.one;
        GetComponent<RectTransform>().localPosition = Vector3.zero;
    }
    /// <summary>
    /// ��ŸƮ ��ư
    /// ������ Ŭ���̾�Ʈ�� �ƴҰ�쿡�� Ȱ��ȭ 
    /// �����ʹ� ���� ��Ŵ������� �Ҵ� ��
    /// </summary>
    public void StartBtnClickTrigger()
    {        
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)
        {
            //���Ŀ� ���� ����� Ȯ���� �ؾ��ϱ� ������ RpcTarget.AllBuffered���
            GetComponent<PhotonView>().RPC("SetReady", RpcTarget.AllBuffered);
        }
    }
    /// <summary>
    /// StartBtnClickTrigger�� ������ �غ� �Ϸ� ���� �̹����� ���
    /// �ٽ� ������ �̹����� ����
    /// �ڽ��� ��ġ�� �ڽ��� �͸� Ȯ���ϰ� ��ο��� ������ �˷������.
    /// </summary>
    [PunRPC]
    public void SetReady()
    {
        //�θ�ü���� ã��
        Transform parentTransform = transform.parent;
        RoomManager roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        if (parentTransform != null)
        {
            //�θ��� ������Ʈ ��������
            PlayerSlot parentSlot = parentTransform.GetComponent<PlayerSlot>();
            if (parentSlot != null)
            {
                if (!readyImage.gameObject.activeSelf)
                {
                    parentSlot.isReady = true;
                    readyImage.gameObject.SetActive(true);
                    roomManager.UpdateAllPlayersReady();
                }
                else
                {
                    parentSlot.isReady = false;
                    roomManager.UpdateAllPlayersReady();
                    readyImage.gameObject.SetActive(false);
                }
            }
        }
    }
}