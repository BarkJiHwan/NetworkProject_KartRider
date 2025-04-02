using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// �÷��̾� ������Ʈ ��ũ��Ʈ
/// �÷��̾��� �̹���, �г���, ���ͳѹ� (īƮ����)��.. ǥ�� ��
/// </summary>
public class PlayerPanel : MonoBehaviourPun
{
    [Header("Player ����")]
    public RawImage playerImg;
    public TMP_Text PlayerNameText;
    public Image playerIcon;

    [Header("�غ� �Ϸ� �̹���")]
    public Image readyImage;

    private string targetTag = "Player";
    private int currentIndex = 0;
    List<GameObject> myTaggedObjects = new List<GameObject>();

    [SerializeField] private RoomManager roomManager;
    [SerializeField] private CharacterList characterList;
    [SerializeField] private Button rightBtn;
    [SerializeField] private Button leftBtn;
    [SerializeField] private Button characterSelectBtn;
    private void Start()
    {
        roomManager = GameObject.FindObjectOfType<RoomManager>();
        characterList = GameObject.FindObjectOfType<CharacterList>();
        rightBtn = roomManager.roomUIManger.characterRightBtn.GetComponent<Button>();
        leftBtn = roomManager.roomUIManger.characterLeftBtn.GetComponent<Button>();
        characterSelectBtn = roomManager.roomUIManger.characterSelectBtn.GetComponent<Button>();

        rightBtn.onClick.AddListener(CharacterChangeNextBtn);
        leftBtn.onClick.AddListener(PreviousCharacterBtn);
        if (photonView.IsMine)
        {
            //���Ŀ� ���� ����� Ȯ���� �ؾ��ϱ� ������ RpcTarget.AllBuffered���
            GetComponent<PhotonView>().RPC("SetOwnInfo", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);
            GetComponent<PhotonView>().RPC("PlayerCharacterLoad", RpcTarget.AllBuffered);
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
    public void SetOwnInfo(Player player)
    {
        if (roomManager == null)
        {
            roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        }
        PlayerNameText.text = photonView.Controller.NickName;
        for (int i = 0; i < roomManager.playerSlots.Length; i++)
        {
            if (roomManager.playerSlots[i].playerPanel == null)
            {
                roomManager.playerSlots[i].playerPanel = GetComponent<PlayerPanel>();
                roomManager.playerSlots[i].actorNumber = player.ActorNumber;
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
        if (roomManager == null)
        {
            roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        }
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

    //public void CharacterSelectBtn()
    //{
    //    if(photonView.IsMine)
    //    {
    //        .RPC("PlayerCharacterLoad", RpcTarget.AllBuffered);
    //    }
    //}
    [PunRPC]
    public void PlayerCharacterLoad()
    {
        myTaggedObjects.Clear();
        PhotonView[] allPhotonViews = GameObject.FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in allPhotonViews)
        {
            if (pv.gameObject.CompareTag(targetTag)) // ���� �����߰� �±װ� ��ġ�ϴ� ���
            {
                pv.gameObject.gameObject.SetActive(false);
                if (pv.IsMine)
                {
                    myTaggedObjects.Add(pv.gameObject);
                    Debug.Log(pv.gameObject.name);
                }
            }
        }
        for (int i = 0; i < myTaggedObjects.Count; i++)
        {
            myTaggedObjects[0].gameObject.SetActive(true);            
            if(i == 1)
            {
                myTaggedObjects[1].transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    public void CharacterChangeNextBtn()
    {
        myTaggedObjects[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex + 1) % myTaggedObjects.Count;
        myTaggedObjects[currentIndex].gameObject.SetActive(true);
    }
    public void PreviousCharacterBtn()
    {
        myTaggedObjects[currentIndex].gameObject.SetActive(false);
        currentIndex = (currentIndex - 1 + myTaggedObjects.Count) % myTaggedObjects.Count; // ù ��°
        myTaggedObjects[currentIndex].gameObject.SetActive(true);
    }
}