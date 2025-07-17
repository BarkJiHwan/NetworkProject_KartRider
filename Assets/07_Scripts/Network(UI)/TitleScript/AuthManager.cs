using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Linq;

public class AuthManager : MonoBehaviour
{
    public TitleUI titleUI;
    public ServerConnect serverCon;
    private WaitForSeconds wait = new WaitForSeconds(1f);
    //���۰� ���ÿ� ���̾�̽��� ��� ������ �����ͺ��̽� ������ ���� ���Ӽ��� ���̾�̽� ������ ������
    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependStatus = task.Result;
            if (dependStatus == DependencyStatus.Available)
            {
                FirebaseDBManager.Instance.Auth = FirebaseAuth.DefaultInstance;
                FirebaseDBManager.Instance.DbRef = FirebaseDatabase.DefaultInstance.RootReference;

            }
            else
            {
                Debug.LogError("���̾�̽� ����" + dependStatus + "��� �Ұ�");
            }
        });
    }
    
    /// <summary>
    /// �α��� ������ �Է� �޾� �α��� �ڷ�ƾ ����
    /// �α��� ���� �Ǵ� ������ ���� ��ư�� ��Ȱ��ȭ
    /// </summary>
    public void Login()
    {
        string email = titleUI.loginEmailField.text;
        string password = titleUI.loginpasswordField.text;
        titleUI.SetLogInButtonsInteractable(false);
        StartCoroutine(LoginCoroutine(email, password));
    }
    IEnumerator LoginCoroutine(string email, string password)
    {
        // Firebase ������ ���� �̸��ϰ� ��й�ȣ �α��� ��û ó��
        var loginTask = FirebaseDBManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);
        float timeout = 5f; // Ÿ�Ӿƿ� �ð�
        float elapsedTime = 0f; 
        string message = ""; // ��� �޼���
        bool toggle = true; // ��� �޼��� ��ȯ�� ���� ���
        while (!loginTask.IsCompleted && elapsedTime <= timeout)
        {
            message = toggle ? "�α�����." : "�α�����..";
            titleUI.ShowMessage(titleUI.successMessage, message, true);
            toggle = !toggle;
            yield return wait;
            elapsedTime += 1f;
        }
        // 5�ʰ� ������ �۾��� �Ϸ���� ������ Ÿ�Ӿƿ� ó��
        if (!loginTask.IsCompleted)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "��Ʈ��ũ ���°� �Ҿ����մϴ�. �ٽ� �õ����ּ���.", true);
            yield return wait;
            titleUI.HideMessages();
            yield break;  // �ڷ�ƾ ���� ó��
        }
        if (loginTask.Exception != null)
        {
            //�α��� ���� ���� ó��
            titleUI.HideMessages();
            LoginError(loginTask.Exception);
            titleUI.SetLogInButtonsInteractable(true);
            yield return wait;
            yield break;
        }

        FirebaseDBManager.Instance.User = loginTask.Result.User;

        var userLoginTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("isLoggedIn").GetValueAsync();
        yield return new WaitUntil(() => userLoginTask.IsCompleted);
        if (userLoginTask.Exception != null)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "�α��� ���� Ȯ�� ���� �ٽ� �α����ϼ���.", true);
            yield return new WaitForSeconds(1f);
            titleUI.SetLogInButtonsInteractable(true);
            titleUI.InitializeLogin();//�ٽ� �α��� �ϴ� �� ó�� Ÿ��Ʋ â �ʱ�ȭ
            yield break;            
        }
        bool? isLoggedIn = userLoginTask.Result.Value as bool?;
        if (isLoggedIn.HasValue && isLoggedIn.Value)
        {
            // �̹� �α��� ���¶�� �α��� ����
            titleUI.SetLogInButtonsInteractable(true);
            titleUI.ShowMessage(titleUI.errorMessage, "�̹� �α��ε� �����Դϴ�. \n�ٸ� ��⿡�� �α׾ƿ� �� �ٽ� �õ��ϼ���.", true);
            yield break; // �α��� �õ��� �ߴ�
        }
        else
        {
            var userRef = FirebaseDBManager.Instance.DbRef.Child("users")
                .Child(FirebaseDBManager.Instance.User.UserId)
                .Child("isLoggedIn").SetValueAsync(false);

            yield return new WaitUntil(() => userRef.IsCompleted);
            if (userRef.Exception != null)
            {
                titleUI.ShowMessage(titleUI.errorMessage, "�α��� ���� Ȯ�� ���� �ٽ� �α����ϼ���.", true);
                yield return new WaitForSeconds(1f);
                titleUI.SetLogInButtonsInteractable(true);
                yield break;
            }

            var characterTask = FirebaseDBManager.Instance.DbRef.Child("users")
                    .Child(FirebaseDBManager.Instance.User.UserId)
                    .Child("CharacterList")
                    .GetValueAsync();
            yield return new WaitUntil(() => characterTask.IsCompleted);
            if (characterTask.Exception != null)
            {
                titleUI.ShowMessage(titleUI.errorMessage, "��Ʈ��ũ ���°� �Ҿ����մϴ�. �ٽ� �õ����ּ���.", true);
                yield return wait;
                titleUI.HideMessages();
                yield break;  // �ڷ�ƾ ���� ó��
            }
            if (string.IsNullOrEmpty("CharacterList"))
            {
                //���ҽ������� �ִ� ĳ���͵��� �̸��� ���̾�̽��� ������ ���̽��� ������
                List<CharacterSo> characters = Resources.LoadAll<CharacterSo>("Character").ToList();
                List<string> jsonList = characters.Select(p => p.characterName).ToList();

                var saveTask = FirebaseDBManager.Instance.DbRef.Child("users")
                    .Child(FirebaseDBManager.Instance.User.UserId)
                    .Child("CharacterList")
                    .SetValueAsync(jsonList);
                yield return new WaitUntil(() => saveTask.IsCompleted);
                if (!saveTask.IsCompleted)
                {
                    titleUI.ShowMessage(titleUI.errorMessage, "�ʱ� ĳ���� ����Ʈ ���� ����!�����ڿ��� �����ϼ���.", true);
                    yield return new WaitForSeconds(2);
                    yield break;
                }
                Debug.Log("�ʱ� ĳ���� ���� �Ϸ�");
            }
            // �α��� ���� ó��
            LoginSuccess(loginTask.Result.User);
        }
    }
    private void LoginError(AggregateException exception)
    {
        // Firebase �α��� ���� �� �߻��� ���� ó��, �޼��� ���
        FirebaseException firebasEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebasEx.ErrorCode;
        string message = "";
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "�̸����� �Է��� �ּ���.";
                break;
            case AuthError.MissingPassword:
                message = "�н����带 �Է��� �ּ���.";
                break;
            default:
                message = "�̸��� Ȥ�� ��й�ȣ�� �߸� �Է��ϼ̰ų�\n ��ϵ��� ���� �̸��� �Դϴ�.";
                break;
        }
        titleUI.SetLogInButtonsInteractable(true);
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }

    void LoginSuccess(FirebaseUser user)
    {
        //�α��� ���� �� ���� ���� ���� �� ó��        
        titleUI.HideMessages();
        titleUI.ToggleLoginPanel(false);  
        StartCoroutine(PostLogin(user));
    }
    IEnumerator PostLogin(FirebaseUser user)
    {
        titleUI.ResetField(titleUI.loginEmailField, titleUI.loginpasswordField);
        //�α��� ���� �� �г��� Ȯ��, �г����� ���ٸ� ������ �� ���� ���        
        if (string.IsNullOrEmpty(user.DisplayName))
        {
            titleUI.ToggleCreateNickNamePanel(true);
        }
        yield return new WaitUntil(predicate: () => !string.IsNullOrEmpty(user.DisplayName));

        titleUI.ToggleCreateNickNamePanel(false);//�г����� �ִٸ� ���
        titleUI.ShowMessage(titleUI.successMessage, "�α��� ����!", true);
        titleUI.SetLogInButtonsInteractable(true);
        serverCon.ConnectToServer(); //���� ���� �õ�

        SceneCont.Instance.Oper = SceneCont.Instance.SceneAsync("LobbyScene");
        SceneCont.Instance.Oper.allowSceneActivation = false;
        titleUI.lodingBar.gameObject.SetActive(true);

        while (SceneCont.Instance.Oper.isDone == false)
        {
            if (SceneCont.Instance.Oper.progress < 0.9f)
            {
                titleUI.lodingBar.value = SceneCont.Instance.Oper.progress;
            }
            else
            {
                //������ ���� ���� �����
                titleUI.lodingBar.value = 1f;
                break;
            }
            yield return new WaitUntil(predicate: () => serverCon.Connect());
            if (!serverCon.Connect())
            {
                //Ŀ��Ʈ ������
                titleUI.ShowMessage(titleUI.errorMessage, "���� ���� ���� �ٽ� �α������ּ���.", true);
                yield return new WaitForSeconds(2f);
                titleUI.InitializeLogin();//�ٽ� �α��� �ϴ� �� ó�� Ÿ��Ʋ â �ʱ�ȭ
                yield break;
            }
        }
        SceneCont.Instance.Oper.allowSceneActivation = true;
    }    
    public void CreateNickNameBottenCon()
    {
        //�г��� ���� ��ư Ŭ�� �� �г��� �ߺ� �˻� �ڷ�ƾ
        string nickName = titleUI.createNickNameField.text;
        StartCoroutine(CreateNickNameCor(nickName));
    }
    IEnumerator CreateNickNameCor(string nickName)
    {
        if (string.IsNullOrEmpty(nickName))
        {
            titleUI.ShowMessage(titleUI.errorMessage, "�г����� �����ϼ���!", true);
            yield break;
        }
        
        /// <summary>
        /// Firebase Realtime Database���� �г��� �ߺ� ���θ� Ȯ��
        /// </summary>
        /// <remarks>
        /// - �ֻ��� DbRef ��ο��� "users"�� �̵��մϴ�. �ش� ��ΰ� ������ �ڵ����� ����
        /// - "users" ��ο� �ִ� �����͸� "UserNickName" �ʵ尪�� �������� ����
        ///   Firebase �����ʹ� JSON ������ ����ǹǷ�, �ʵ尪�� �������� ����
        /// - EqualTo(nickName)�� ����Ͽ� "UserNickName" �ʵ尪�� nickName�� ������ �����͸� ���͸�
        /// - GetValueAsync()�� ȣ���Ͽ� �����͸� �񵿱�� �����ɴϴ�. �۾� ����� Task�� ��ȯ
        /// </remarks>
        /// <param name="nickName">�ߺ� ���θ� Ȯ���� �г���</param>
        /// <returns>Task�� ���� ���͸��� �����͸� ��ȯ�մϴ�.</returns>
        var nickNameCheckingTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .OrderByChild("userNickName")
            .EqualTo(nickName).GetValueAsync();

        float timeout = 5f;
        float elapsedTime = 0f;
        string message = "";
        bool toggle = true;
        while (!nickNameCheckingTask.IsCompleted && elapsedTime <= timeout)
        {
            message = toggle ? "�г��� ������." : "�г��� ������..";
            titleUI.ShowMessage(titleUI.successMessage, message, true);
            toggle = !toggle;
            yield return wait;
            elapsedTime += 1f;
        }
        if (!nickNameCheckingTask.IsCompleted)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "��Ʈ��ũ ���°� �Ҿ����մϴ�. �ٽ� �õ����ּ���.", true);
            yield break;
        }
        if (nickNameCheckingTask.Exception != null)
        {//�г��� ��ȿ�� �˻� ���� �޼���: ����� �� ���� �г����Դϴ�.
            titleUI.ShowMessage(titleUI.errorMessage, "����� �� ���� �г����Դϴ�.", true);
            yield break;
        }

        /// <summary>
        /// �־��� �г����� Firebase Realtime Database�� �̹� �����ϴ��� Ȯ��
        /// </summary>
        /// <remarks>
        /// - �г��� �˻��۾��� ����� DataSnapshot���� ������
        /// - Exists �Ӽ��� ����Ͽ� �г����� �̹� ��������� Ȯ��
        /// - �г����� ������ ���, ���� �޽����� ǥ���ϰ� ���� ó���� �ߴ�
        /// </remarks>
        /// <param name="nickName">�ߺ� ���θ� Ȯ���� �г���</param>
        DataSnapshot nickNameSnapshot = nickNameCheckingTask.Result;
        if (nickNameSnapshot.Exists)
        {//�޼���: �̹� ������� �г����Դϴ�.
            titleUI.ShowMessage(titleUI.errorMessage, "�̹� ������� �г��� �Դϴ�!", true);
            yield break;
        }

        /// <summary>
        /// ���� ������� �г����� Firebase Realtiem Database�� ����
        /// </summary>
        /// <remarks>
        /// - users/UserId/UserNickName ��η� �̵�
        /// - �ش� ����� "UserNickName" �ʵ忡 �г��� ���� ����
        
        /// - SetValueAsync() �޼��带 ȣ���Ͽ� �����͸� �񵿱�� �����ϰ� ������ �Ϸ� �Ǹ� Task�� ��ȯ
        /// </remarks>
        /// <param name="nickName"> ������ �г��� </param>            
        var nickNameTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("userNickName")
            .SetValueAsync(nickName);

        timeout = 5f;
        elapsedTime = 0f;
        while (!nickNameTask.IsCompleted && elapsedTime <= timeout)
        {
            message = toggle ? "�г��� ������." : "�г��� ������..";
            titleUI.ShowMessage(titleUI.successMessage, message, true);
            toggle = !toggle;
            yield return wait;
            elapsedTime += 1f;
        }
        if (!nickNameTask.IsCompleted)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "��Ʈ��ũ ���°� �Ҿ����մϴ�. �ٽ� �õ����ּ���.", true);
            yield break;
        }
        if (nickNameTask.Exception != null)
        {
            //���̾�̽� �г��� ���� ���� �޼���: ����� �� ���� �г����Դϴ�
            titleUI.errorMessage.text = "����� �� ���� �г����Դϴ�.";
            yield break;
        }
        Debug.Log("���� ���� ����");
        FirebaseDBManager.Instance.User.UpdateUserProfileAsync(new UserProfile { DisplayName = nickName });
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "�г��� ���� ����!", true);
        titleUI.ToggleCreateNickNamePanel(false);
    }


    /// <summary>
    /// ȸ�� ���� ��ư ���� ��
    /// ����ڰ� �Է��� �̸��� �� ��й�ȣ�� ������ �񵿱������� ȸ�� ���� ������ ����
    /// ������� �̸��� �� ��й�ȣ�� UI �ʵ忡�� ������
    /// �̸��� �� ��й�ȣ�� ���ڷ� �Ͽ� SignUpCoroutine �ڷ�ƾ�� ����
    /// </summary>

    public void Signup()
    {
        string email = titleUI.signUpEmailField.text;
        string password = titleUI.signUpPasswordField.text;
        StartCoroutine(SignUpCoroutine(email, password));
    }
    /// <summary>
    /// ȸ������ �ڷ�ƾ
    /// ���� �̸��ϰ� �н����带 ���̾�̽� ��� ���������� ���� ��.
    /// signUpTask�� Auth�� ȸ�� ������ ��û�ϰ�, ��û���� ���
    /// ���� �Ǵ� ����ġ ���� ��Ȳ�� ����ϱ� ���� while ���ǿ� Ÿ�Ӿƿ��ð��� �༭ ó����
    /// </summary>
    /// <param name="email">�̸��� �ʵ�� ���� �̸��� ����</param>
    /// <param name="password">�н����� �ʵ�� ���� �н����� ����</param>
    IEnumerator SignUpCoroutine(string email, string password)
    {
        titleUI.SetsignUpInteractable(false);// ��ư ��Ȱ��
        var signUpTask = FirebaseDBManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        float timeout = 5f;
        float elapsedTime = 0f;
        bool toggle = true;
        string message = "";
        while (!signUpTask.IsCompleted && elapsedTime <= timeout)
        {
            message = toggle ? "���� ������." : "���� ������..";
            titleUI.ShowMessage(titleUI.successMessage, message, true);
            toggle = !toggle;
            yield return wait; // 1�� ��⸦ ���� ������ �̸� ����
            elapsedTime += 1f;
        }
        if (!signUpTask.IsCompleted)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "��Ʈ��ũ ���°� �Ҿ����մϴ�. �ٽ� �õ����ּ���.", true);
            yield return wait;
            titleUI.SetsignUpInteractable(true);
            titleUI.HideMessages();
            yield break;
        }
        if (signUpTask.Exception != null)
        {
            titleUI.SetsignUpInteractable(true);
            SignUpError(signUpTask.Exception);
            yield break;
        }

        SignUpSuccess(signUpTask.Result.User);
    }
    /// <summary>
    /// ȸ������ ��ȿ���˻� �޼���
    /// ȸ�����Խõ��� �� ���� ���� ������ ó����
    /// </summary>
    private void SignUpError(AggregateException exception)
    {
        titleUI.HideMessages();
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = "";
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "�̸����� �Է��� �ּ���.";
                break;
            case AuthError.MissingPassword:
                message = "�н����带 �Է��� �ּ���.";
                break;
            case AuthError.WeakPassword:
                message = "�н������ �ּ� 6�� �̻� �Է��� �ּ���.";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "�̹� �����ϴ� �̸��� �Դϴ�.";
                break;
            default:
                message = "���� ���� ����! �����ڿ��� �����ϼ���.";
                break;
        }
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }
    /// <summary>
    /// ȸ������ ���� �޼���
    /// ȸ�����Կ� �����ϸ� ������ ������ ó���ϴ� �ڷ�ƾ ����
    /// </summary>
    /// <param name="user">���������� �޾Ƽ� ���̾�̽��� �����ϴ� ������ �� </param>
    private void SignUpSuccess(FirebaseUser user)
    {
        FirebaseDBManager.Instance.User = user;//���� ���� �����ϰ�               
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "���� ������.", true);
        //��ư ��Ȱ��ȭ
        titleUI.SetsignUpInteractable(false);
        StartCoroutine(FinishSignUp());
    }
    /// <summary>
    /// ȸ������ ������ ������ �Ǿ��ٸ� ���������� ���� ���̾�̽��� ������ �α��� ������ ������
    /// ���� ���� 5�� �̻��� �ð��� ������ �Ǹ� ������ ����
    /// ���� �߻� �� ������� ���� ������ �����ϰ� �ٽ� ������ ��
    /// ������ ���ٸ� ȸ������ �Ϸ�
    /// </summary>
    IEnumerator FinishSignUp()
    {
        var setPrfileTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId).Child("isLoggedIn")
            .SetValueAsync(false);
        float timeout = 2f;
        float elapsedTime = 0f;
        bool toggle = true;
        string message = "";
        while (elapsedTime <= timeout)
        {
            message = toggle ? "���� ������." : "���� ������..";
            titleUI.ShowMessage(titleUI.successMessage, message, true);
            toggle = !toggle;
            yield return wait; 
            elapsedTime += 1f;
        }
        if (!setPrfileTask.IsCompleted)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "��Ʈ��ũ ���°� �Ҿ����մϴ�. �ٽ� �õ����ּ���.", true);
            yield return wait;
            titleUI.InitializeLogin();
        }
        if (setPrfileTask.Exception != null)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "���� ������ ���� ���� �����ڿ��� �����ϼ���.", true);
            yield return wait;
            titleUI.InitializeLogin();
            yield break;
        }

        //���ҽ������� �ִ� ĳ���͵��� �̸��� ���̾�̽��� ������ ���̽��� ������
        List<CharacterSo> characters = Resources.LoadAll<CharacterSo>("Character").ToList();
        List<string> jsonList = characters.Select(p => p.characterName).ToList();

        var saveTask = FirebaseDBManager.Instance.DbRef.Child("users")
            .Child(FirebaseDBManager.Instance.User.UserId)
            .Child("CharacterList")
            .SetValueAsync(jsonList);
        timeout = 5f;
        elapsedTime = 0f;
        toggle = true;
        message = "";
        while (!setPrfileTask.IsCompleted && elapsedTime <= timeout)
        {
            message = toggle ? "ĳ���� ���� ������." : "ĳ���� ���� ������...";
            titleUI.ShowMessage(titleUI.successMessage, message, true);
            toggle = !toggle;
            yield return wait;
            elapsedTime += 1f;
        }
        if (!saveTask.IsCompleted)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "��Ʈ��ũ ���°� �Ҿ����մϴ�. �ٽ� �õ����ּ���.", true);
            yield return wait;
            titleUI.InitializeLogin();
            yield break;
        }
        if (saveTask.Exception != null)
        {
            titleUI.ShowMessage(titleUI.errorMessage, "ĳ���� ������ ���� ���� �����ڿ��� �����ϼ���.", true);
            yield return wait;
            titleUI.InitializeLogin();
            yield break;
        }
        Debug.Log("�ʱ� ĳ���� ���� �Ϸ�");

        titleUI.ToggleSignUpPanel(false);
        //ȸ������ �Ϸ� �޼���
        titleUI.ShowMessage(titleUI.successMessage, "ȸ������ �Ϸ�!", true);
        yield return new WaitForSeconds(1f); //1�� ���
        titleUI.SetsignUpInteractable(true); //��ư ��� Ǯ��
        titleUI.HideMessages(); //�޼��� ������
                                //�ؽ�Ʈ �ʱ�ȭ �ϰ�        
        titleUI.LoginToButtonCon(); //ȸ������ �Ϸ� �� �α��� ȭ������ �̵�
    }
}
