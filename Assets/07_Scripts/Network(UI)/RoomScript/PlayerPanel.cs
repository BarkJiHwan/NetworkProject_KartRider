using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// 플레이어 오브젝트 스크립트
/// 플레이어의 이미지, 닉네임, 액터넘버 (카트정보)등.. 표시 됨
/// </summary>
public class PlayerPanel : MonoBehaviourPun
{
    [Header("Player 정보")]
    public TMP_Text PlayerNameText;
    public Image playerIcon;

    [Header("준비 완료 이미지")]
    public Image readyImage;

    private bool _playerIsready;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private CharacterList characterList;
    private void Start()
    {
        roomManager = GameObject.FindObjectOfType<RoomManager>();
        characterList = GameObject.FindObjectOfType<CharacterList>();
        if (photonView.IsMine)
        {
            _playerIsready = false;
            //이후에 들어온 사람도 확인을 해야하기 때문에 RpcTarget.AllBuffered사용
            GetComponent<PhotonView>().RPC("SetOwnInfo", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, _playerIsready);
            OnClickGameObjSelectBtn();
        }
    }

    /// <summary>
    /// 플레이어 판넬의 생성은 RoomManager스크립트에서 PhotonNetwork.Instantiate로 생성
    /// 만들어 지면 PhotonView.IsMine으로 만든 사람이 PunRPC를 모두에게 뿌림
    /// 슬롯을 돌면서 빈 슬롯을 먼저 찾은 뒤 자신의 정보를 저장함,
    /// 버튼에 StartBtnClickTrigger()메서드를 저장한뒤 슬롯의 자식으로 위치를 옮김
    /// 저장이 완료 되면 위치 값과 크기를 조정함
    /// </summary>
    [PunRPC]
    public void SetOwnInfo(Player player, bool isReady)
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
                roomManager.roomUIManger.startBtn.onClick.AddListener(roomManager.playerSlots[i].playerPanel.ReadyBtnClickTrigger);
                roomManager.playerSlots[i].isReady = isReady;
                roomManager.roomUIManger.characterSelectBtn.onClick.AddListener(roomManager.playerSlots[i].playerPanel.OnClickGameObjSelectBtn);
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
    /// 준비 버튼
    /// 마스터 클라이언트가 아닐경우에만 활성화 
    /// 마스터는 따로 룸매니저에서 할당 됨
    /// </summary>
    public void ReadyBtnClickTrigger()
    {
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)
        {
            _playerIsready = !readyImage.gameObject.activeSelf;
            //이후에 들어온 사람도 확인을 해야하기 때문에 RpcTarget.AllBuffered사용
            GetComponent<PhotonView>().RPC
                ("SetReady", 
                RpcTarget.AllBuffered, 
                _playerIsready);
        }
    }
    /// <summary>
    /// StartBtnClickTrigger을 누르면 준비 완료 상태 이미지를 띄움
    /// 다시 누르면 이미지를 감춤
    /// 자신의 위치의 자신의 것만 확인하고 모두에게 정보를 알려줘야함.
    /// </summary>
    [PunRPC]
    public void SetReady(bool playerIsReady)
    {
        //부모객체에서 찾기
        Transform parentTransform = transform.parent;
        if (roomManager == null)
        {
            roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        }
        if (parentTransform != null)
        {
            //부모의 컴포넌트 가져오기
            PlayerSlot parentSlot = parentTransform.GetComponent<PlayerSlot>();

            if (parentSlot != null)
            {
                parentSlot.isReady = playerIsReady;
                readyImage.gameObject.SetActive(playerIsReady);
                roomManager.UpdateAllPlayersReady();
            }
        }
    }
    public void OnClickGameObjSelectBtn()
    {
        if (photonView.IsMine)
        {            
            GetComponent<PhotonView>().RPC("GameObjSelect", RpcTarget.AllBuffered, characterList.SelectedCharacter().name);
        }
    }
    [PunRPC]
    public void GameObjSelect(string characterName)
    {
        CharacterSo[] characters = Resources.LoadAll<CharacterSo>("Character");
        CharacterSo selectedChar = null;
        foreach (var charac in characters)
        {
            if (charac.name.Equals(characterName))
            {
                selectedChar = charac;
            }
        } 
            
        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            PlayerSlot parentSlot = parentTransform.GetComponent<PlayerSlot>();
            if (parentSlot != null)
            {
                Debug.Log(parentSlot + "의 슬롯");
                parentSlot.playerPanel.playerIcon.sprite = selectedChar.characterIcon;   
            }
        }
    }
}
