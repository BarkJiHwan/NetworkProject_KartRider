using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Firebase.Database;
using System.Linq;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField]private LobbyUIManager lobbyUiMgr;
    private List<RoomInfo> roomInfos = new List<RoomInfo>();
    private Dictionary<string, RoomEntry> roomEntryMap = new Dictionary<string, RoomEntry>();
    private Queue<int> availableRoomNumbers = new Queue<int>(); // �� ��ȣ ���� Queue
    private HashSet<int> usedRoomNumbers = new HashSet<int>(); // ��� ���� �� ��ȣ ����
    private Coroutine roomListUpdateCor;
    [HideInInspector] public TestCHMKart kartCtrl;
    public GameObject kartPrefab;
    private CharacterSo[] _characterSoArray;
    public CharacterSo characterSo;
    private void Awake()
    {
        _characterSoArray = Resources.LoadAll<CharacterSo>("Character");

        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        if (pool != null)
        {
            if (!pool.ResourceCache.ContainsKey(kartPrefab.name))
            {
                pool.ResourceCache.Add(kartPrefab.name, kartPrefab);
            }

            foreach (var soCharacter in _characterSoArray)
            {
                if (pool.ResourceCache.ContainsKey(soCharacter.characterName))
                {
                    continue;
                }
                pool.ResourceCache.Add(soCharacter.characterName, soCharacter.characterPrefab);
            }
        }
        StartCoroutine(KartandCharacterCor());
    }

    /// <summary>
    /// ��ŸƮ�� �ڷ�ƾ���� �Ͽ� ��������� ���
    /// Ÿ��Ʋ ������ �Ѿ���鼭 �� ���濡 ���� �������� ���� �ޱ� ���� �۾�
    /// ��Ʈ��ũ���� ������ �Ϸ� �Ǹ� �κ� ������ �õ���
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
        PhotonNetwork.JoinLobby();
        lobbyUiMgr.ClickOffPanelActive(false);//�κ� ������ �ϴ� ��
        InitializeRoomNumber(); //�� ��ȣ �ʱ�ȭ
    }

    /// <summary>
    /// ���̸��� ���������� �����ϰ� �� ���̱� ������ ����� ���� ��ȣ�� �ʿ�
    /// �ִ��� ��ġ���ʴ� �� ��ȣ�� ���� �������� 100���� ���ڸ� ����
    /// </summary>
    private void InitializeRoomNumber()
    {
        HashSet<int> uniqueNumbers = new HashSet<int>();

        while (uniqueNumbers.Count < 100) // 100���� ������ �� ��ȣ ����
        {
            int roomNumber = Random.Range(100000, 999999);
            uniqueNumbers.Add(roomNumber);
            Debug.Log(roomNumber);
        }

        foreach (var number in uniqueNumbers)
        {
            availableRoomNumbers.Enqueue(number);
        }
    }
    /// <summary>
    /// ���� ��ȣ�� ������̶�� �� ���� ���� ������ �����
    /// </summary>
    /// <returns></returns>
    private int GetRoomNumber()
    {
        if (availableRoomNumbers.Count > 0)
        {
            int roomNumber = availableRoomNumbers.Dequeue();
            usedRoomNumbers.Add(roomNumber); //��� ���� ��ȣ�� �߰�
            return roomNumber;
        }
        //ť�� �����ִ� �� ��ȣ�� ���� ���, UI�� ���� �޽��� ǥ��
        lobbyUiMgr.RoomJoinFaildeText("���� ���� �� �����ϴ�.");
        return -1; //���� ��Ȳ�� ó���� �� �ֵ��� -1 ��ȯ
    }

    /// <summary>
    /// �������ϴ� �濡 ���ν�Ű�� ���� �޼ҵ�
    /// </summary>
    /// <param name="roomName">�� �̸� ��Ī</param>
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// �� ���� ��ư�� ���� ��ư Ŭ�� �� 
    /// ��ǲ �ʵ忡 �ۼ��� �̸����� ���� ������
    /// �н����� �� �Ǵ� ��ĭ�� Ȯ��, ������ ���� ������ �������� ����
    /// �� �ѹ��� �޾ƿͼ� ���� ���� ��ȣ�� ���ٸ� ������� ����
    /// </summary>
    /// 
    public void CreateRoomBtnClick()
    {
        lobbyUiMgr.ClickOffPanelActive(true);
        string roomName = lobbyUiMgr.roomNameInputField.text;
        string password = string.IsNullOrEmpty(lobbyUiMgr.roomPasswordInputField.text) ? null : lobbyUiMgr.roomPasswordInputField.text;
        int roomNumber = GetRoomNumber();

        if (roomNumber <= 0)
            return;

        CreateRoom(roomName, password, roomNumber); //���濡�� �����ϴ� �޼��尡 �ƴ� ���� �ż���
    }

    /// <summary>
    /// ������ �� ���� ��ư�� ����, ���� ������ ������ ���� ������
    /// </summary>
    public void JoinRandomRoomBtn()
    {
        lobbyUiMgr.ClickOffPanelActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// RoomNumberJoinBtn�� ����Ǿ� ����
    /// ��ѹ��� �´� �濡 ��
    /// �̸����� ���� ���� ������ �̸��� �ߺ������� �����ϱ� ������ 
    /// ���ϴ� �濡 �����ϱ� ���ؼ� ������ �±װ� �ʿ� �߰� �� �±׸� ������ 6�ڸ� ���ڷ� ������
    /// </summary>
    public void RoomNumberJoinBtn()
    {
        lobbyUiMgr.ClickOffPanelActive(true);
        PhotonNetwork.JoinRoom(lobbyUiMgr.roomNumberInputField.text);
    }

    /// <summary>
    /// �� ���� �޼���
    /// �����Ʈ��ũ���� �������ִ� CreateRoom�� ����ϱ� ���� Hashtable�� Ȱ���Ͽ� RoomOptions�� Ŀ���� �� �� ����
    /// �����Ʈ��ũ���� �����ϴ� CustomRoomProperties, CustomRoomPropertiesForLobby�� Ȱ���Ͽ�
    /// �ɼ��� �߰� �� �����ϴ� �͵� ������
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="password"></param>
    /// <param name="roomNumber"></param>
    public void CreateRoom(string roomName, string password, int roomNumber)
    {
        string roomNum = roomNumber.ToString();
        Hashtable custom = new Hashtable
        {
            { "RoomName", roomName },
            { "Password", password },
            { "RoomNumber", roomNum },
            { "Map", "default"},
            { "IsGameStart", false }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, //�ִ� ���� �÷��̾� ����
            EmptyRoomTtl = 0, //�濡 ����� ���ٸ� �ٷ� ���� �����ϵ��� ����
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber","Map","IsGameStart" }
        };

        PhotonNetwork.CreateRoom(roomNum, roomOptions, TypedLobby.Default); //�κ�� �� ���ۿ� ������ �κ�Ÿ���� �⺻
    }

    /// <summary>
    /// ���� ���� Ŭ�� �� ����
    /// �濡 ���� ���ߴٸ� ������ ���� ���鵵�� ������
    /// CreateRoom�� �ٸ��� �������� ������ ���� ��й�ȣ ������ ���� ����     
    /// ���� ���н� �Ǵ� ���� ���ٸ� ���� ���� ����
    /// </summary>
    /// <param name="returnCode"> �� ���� ���п� ���� �����ڵ� : ���� �������� ���� </param>
    /// <param name="message"> �� ���� ���� ���� �޼��� : ���� �������� ���� </param>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //���� ���� �õ� �� ���� ���ٸ� ���� �����ϱ� ���� �޼���
        //������ ������ �ϳ��� ���� �߰����� ����
        string[] roomNames = { "���Բ� īƮ���̴�", "1:1 �ų���", "�� ������ �Ұ� ���׿�" };
        int randomName = Random.Range(0, roomNames.Length);
        string randomRoomName = roomNames[randomName];
        int roomNumber = GetRoomNumber();
        string roomNum = roomNumber.ToString();
        Hashtable custom = new Hashtable
        {
            { "RoomName", randomRoomName },
            { "Password", "" },
            { "RoomNumber", roomNum },
            { "Map", "default"},
            { "IsGameStart", false }
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 8, //�ִ� ���� �÷��̾� ����
            EmptyRoomTtl = 0, //�濡 ����� ���ٸ� �ٷ� ���� �����ϵ��� ����
            CustomRoomProperties = custom,
            CustomRoomPropertiesForLobby = new string[] { "RoomName", "Password", "RoomNumber", "Map", "IsGameStart" }
        };

        PhotonNetwork.JoinRandomOrCreateRoom(custom, 8, MatchmakingMode.FillRoom, null, null, roomNum, roomOptions, null);
    }

    /// <summary>
    /// OnJoinRoom�� ���� �Ǹ� �뿡 ���� ���� (����ȯ) �ڷ�ƾ ����
    /// </summary>
    public override void OnJoinedRoom()
    {
        StartCoroutine(LoadJoinRoom("RoomScene"));
    }

    /// <summary>
    /// ��� �� ��ȯ�� ��Ʈ��ũ �������� ������ �ּ�ȭ �ϱ� ���� �ε� �ð��� ���� �߰��ؼ� ����
    /// </summary>
    IEnumerator LoadJoinRoom(string sceneName)
    {
        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync(sceneName);
        SceneCont.Instance.Oper.allowSceneActivation = false;
        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                //�κ񿡼� ������ �̵��� ���α׷����ٸ� ���� �ʿ䰡 ������..?
            }
            else
            {
                SceneCont.Instance.Oper.allowSceneActivation = true;
                break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// �κ������� �Ϸ� �Ǿ��ٸ� �븮��Ʈ�� ������Ʈ �ϰ�
    /// ������Ʈ �ڷ�ƾ ����
    /// </summary>
    public override void OnJoinedLobby()
    {
        roomListUpdateCor = StartCoroutine(RoomListUpdateCor());        
    }
    IEnumerator KartandCharacterCor()
    {
        GameObject kart = Instantiate(kartPrefab, Vector3.zero, Quaternion.Euler(0, 140, 0));
        string characterName = "";
        var characterTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("SelectedCharacter").GetValueAsync();
        yield return new WaitUntil(() => characterTask.IsCompleted);        
        if (string.IsNullOrEmpty("SelectedCharacter"))
        {            
            characterName = "Bazzi";
            var characTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("SelectedCharacter")
            .SetValueAsync(characterName);
            yield return new WaitUntil(() => characterTask.IsCompleted);
            if (characterTask.Exception != null)
            {
                characterSo = _characterSoArray[1];
            }
            else
            {
                foreach (var character in _characterSoArray)
                {
                    if (characterName == character.characterName)
                    {
                        characterSo = character;
                        break;
                    }
                }
            }
        }
        else
        {
            var charactTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("SelectedCharacter").GetValueAsync();

            yield return new WaitUntil(() => characterTask.IsCompleted);
            DataSnapshot snapshot = characterTask.Result;
            if (snapshot.Exists && snapshot.Value != null)
            {
                characterName = snapshot.Value.ToString();
            }
            foreach (var character in _characterSoArray)
            {
                if (characterName == character.characterName)
                {
                    characterSo = character;
                    break;
                }
            }
        }
        if (characterSo.characterName == "Airi" || characterSo.characterName == "Lena")
        {
            GameObject playerChar = Instantiate(characterSo.characterPrefab, Vector3.zero, Quaternion.Euler(-90, -30, -90));
        }
        else if (characterSo.characterName == "Bazzi" || characterSo.characterName == "Dao" || characterSo.characterName == "Kephi")
        {
            GameObject playerChar = Instantiate(characterSo.characterPrefab, Vector3.zero, Quaternion.Euler(0, -30, 0));
        }
    }

    /// <summary>
    /// �κ񿡼� ���̴� ���� ������Ʈ ��
    /// ���濡�� ������Ʈ �Ǵ� ����� ���� ���� ����Ʈ�� ��Ƽ� ���
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //RoomListUpdate(roomList); //��ȭ�� �����Ǹ� �ڵ����� ������Ʈ
        roomInfos.Clear();
        roomInfos = roomList.ToList(); // ����Ʈ�� ���纻�� �� ����Ʈ�� ��Ƶ�
    }
    public void OnJoinRoomByNumberButtonClick()
    {
        if (int.TryParse(lobbyUiMgr.roomNumberInputField.text, out int inputRoomNumber))
        {
            JoinRoomByNumber(inputRoomNumber, roomInfos);
        }
        else
        {
            lobbyUiMgr.RoomJoinFaildeText("��ȿ�� ���ڸ� �Է��ϼ���.");
        }
    }
    public void JoinRoomByNumber(int inputRoomNumber, List<RoomInfo> roomInfos)
    {
        foreach (var room in roomInfos)
        {
            string roomNumber = room.CustomProperties.ContainsKey("RoomNumber")
            ? (string)room.CustomProperties["RoomNumber"].ToString() : "�ش� ���� �������� �ʽ��ϴ�.";
            if (roomNumber == inputRoomNumber.ToString())
            {
                PhotonNetwork.JoinRoom(room.Name); // ���� �� �̸�(ID)
                return;
            }
        }
        // ���� ��
        lobbyUiMgr.RoomJoinFaildeText("�ش� ���� �������� �ʽ��ϴ�.");
    }
    /// <summary>
    /// ���� ��ư�� ������ ���� ���Ƿ� �κ�Room ������Ʈ
    /// </summary>
    public void RoomListUpdateBtn()
    {
        RoomListUpdate(roomInfos);
    }

    /// <summary>
    /// �κ񿡼� ���� �� �븮��Ʈ ������Ʈ �ڷ�ƾ ���߱�
    /// </summary>
    IEnumerator RoomListUpdateCor()
    {
        WaitForSeconds waitForSec = new WaitForSeconds(10f);

        while (PhotonNetwork.InLobby)
        {
            RoomListUpdate(roomInfos);
            yield return waitForSec;
        }
    }

    public override void OnLeftLobby()
    {
        StopAllCoroutines();
    }
    /// <summary>
    ///15�� ���� �� ����Ʈ �ڵ����� ������Ʈ �ǵ��� ������
    ///�ð��� �ٲ㵵 �˴ϴ�.
    /// </summary>
    /// <example waitForSec = new WaitForSeconds(s);> s�� ���ϴ� �ð��� ������ �� </example>
    /// <summary>
    /// ���濡�� OnRoomListUpdate�� ȣ�� �Ǹ� ��ü���� �� ����Ʈ ������Ʈ�� ������
    /// ���� ���� ���� �ǰų� �������ų� ���°� ��ȭ�Ǹ� �ڵ����� ���� �ǵ��� ����
    /// </summary>
    /// <param name="roomList">OnRoomListUpdate���� �� ����Ʈ�� �޾ƿͼ� ó����</param>
    public void RoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList) //�� ���� ó��
            {
                RemoveRoomEntry(roomInfo.Name);
            }
            else
            {
                Debug.Log(roomInfo.Name);
                if (!roomEntryMap.ContainsKey(roomInfo.Name)) //���ο� �� �߰�
                {
                    CreateRoomButton(roomInfo);
                }
                else
                {
                    UpdateRoomEntry(roomInfo); //���� �� ������Ʈ
                }
            }
        }
    }
    /// <summary>
    /// �� ������Ʈ�� ������ ����ϴ� �޼���
    /// ������ �Ϸ� �Ǹ� ��ųʸ����� ������
    /// </summary>
    private void RemoveRoomEntry(string roomName)
    {
        if (roomEntryMap.ContainsKey(roomName))
        {
            RoomEntry entryToRemove = roomEntryMap[roomName];

            // UI ������Ʈ ����
            Destroy(entryToRemove.gameObject);

            // ����
            roomEntryMap.Remove(roomName);
        }
    }
    /// <summary>
    /// �� ������ ��ȭ �Ǿ��ٸ� �ش� ���� ������ �ٲ� ������ �ٽ� ������
    /// </summary>
    private void UpdateRoomEntry(RoomInfo roomInfo)
    {
        if (roomEntryMap.ContainsKey(roomInfo.Name))
        {
            RoomEntry existingEntry = roomEntryMap[roomInfo.Name];
            existingEntry.SetRoomInfo(roomInfo); //�ֽ� RoomInfo�� ������Ʈ
        }
    }
    /// <summary>
    /// �κ� �� ������Ʈ�� �����ϴ� �޼���
    /// �κ� �� ������Ʈ�� �����ϰ� �ش� ������ �뿣Ʈ��.SetRoomInfo�� �޼ҵ带 ���� �ش� ������ ������
    /// ������Ʈ�� ������ �Ǿ��ٸ� ������Ʈ�� ��ư�� ������
    /// ��ư ������ ���� �ش� ���� Ŭ���ϸ� �� ������ �̵��ϵ��� �ϴ� ������ ��
    /// �濡 ���� �Ӽ����� �ִٸ�?(������ ���� �Ǿ��ų�, ��й�ȣ�� �ְų� ��... �ɼ� �߰����� RoomEntry ��ũ��Ʈ���� �� ��)
    /// </summary>
    /// <param name="roomInfo">������ �������� �޾Ƽ� ó���� </param>
    public void CreateRoomButton(RoomInfo roomInfo)
    {
        var roomEntry = Instantiate(lobbyUiMgr.roomPrefab, lobbyUiMgr.roomListPanel.transform); //�� ������Ʈ ����
        var roomEntryScript = roomEntry.GetComponent<RoomEntry>(); //������ �뿡 RoomEntry ��ũ��Ʈ ������Ʈ ���
        roomEntryMap.Add(roomInfo.Name, roomEntryScript);//�������� ��¥������Ʈ�� ����
        
        if (roomEntryScript != null)
        {
            roomEntryScript.SetRoomInfo(roomInfo); //RoomInfo���� ���� ����
            
            roomEntry.onClick.AddListener(() =>
            {
                //FirstOrDefault �� ���� roomInfos���� roomInfo���� ���� �̸��� �´� �͵� ��
                //�ִٸ� �ش� ��ü�� ��ȯ���ְ� ���ٸ� null�� ��ȯ�� ��
                //roomInfo = roomInfos.FirstOrDefault(r => r.CustomProperties["RoomNumber"].ToString() == roomEntryScript.roomNumberText.text);
                if (roomEntryScript.IsPasswrod(roomInfo))
                {
                    ShowPasswordPrompt(roomInfo.Name, roomEntryScript.roomPasswordText.text);
                }
                else if (roomEntryScript.IsGameStarted(roomInfo))
                {
                    lobbyUiMgr.RoomJoinFaildeText("������ �̹� ���� ���Դϴ�.");
                    RoomListUpdate(roomInfos);
                }
                else if (roomEntryScript.IsRoomFull(roomInfo))
                {
                    lobbyUiMgr.RoomJoinFaildeText("���� ���� á���ϴ�.");
                    RoomListUpdate(roomInfos);
                }
                else if (!roomEntryMap.ContainsKey(roomInfo.Name))
                {
                    lobbyUiMgr.RoomJoinFaildeText("���� �����ϴ�.");
                    RoomListUpdate(roomInfos);
                }
                else
                {
                    JoinRoom(roomInfo.Name);
                }
            });
        }
    }
    /// <summary>
    /// �濡 ��й�ȣ�� ���� ��� ����� �޼���
    /// ��� �н����尡 �´ٸ� ����, ���� ���н� �����޼���
    /// </summary>
    /// <param name="roomName">���̸�</param>
    /// <param name="correctPassword">�н�����</param>
    public void ShowPasswordPrompt(string roomName, string correctPassword)
    {
        lobbyUiMgr.LockRoomPasswrodPanelActive(true);
        lobbyUiMgr.lockRoomConnectBtn.onClick.AddListener(() =>
        {
            string enteredPassword = lobbyUiMgr.lockRoomPasswordInputField.text;
            lobbyUiMgr.LockRoomPasswrodPanelActive(false);
            lobbyUiMgr.lockRoomPasswordInputField.text = "";
            if (enteredPassword == correctPassword)
            {
                JoinRoom(roomName);
            }
            else
            {
                RoomListUpdate(roomInfos);
                lobbyUiMgr.RoomJoinFaildeText("�Է��� ��й�ȣ�� ��ġ���� �ʽ��ϴ�.");
            }
        });
    }    
}