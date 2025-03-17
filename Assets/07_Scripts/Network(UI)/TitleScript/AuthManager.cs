using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Database;
using OpenCover.Framework.Model;
using System;
using UnityEngine.Rendering;

public class AuthManager : MonoBehaviour
{
    public TitleUI titleUI;
    public ServerConnect serverCon;

    private bool nickNameCheck = false;

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
    /// </summary>
    public void Login()
    {
        string email = titleUI.loginEmailField.text;
        string password = titleUI.loginpasswordField.text;
        StartCoroutine(LoginCoroutine(email, password));        
    }

    IEnumerator LoginCoroutine(string email, string password)
    {
        // Firebase ������ ���� �̸��ϰ� ��й�ȣ �α��� ��û ó��
        var loginTask = FirebaseDBManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);
        // �α��� ��û�� �Ϸ�� �� ���� ���
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            //�α��� ���� ���� ó��
            LoginError(loginTask.Exception);
            yield break;
        }
        // �α��� ���� ó��
        LoginSuccess(loginTask.Result.User);
        
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
                message = "MissingEmail";
                break;
            case AuthError.MissingPassword:
                message = "MissingPassword";
                break;
            default:
                message = "Email or Password or UserNotFound";
                break;
        }
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }

    void LoginSuccess(FirebaseUser user)
    {
        //�α��� ���� �� ���� ���� ���� �� ó��
        FirebaseDBManager.Instance.User = user;
        titleUI.HideMessages();
        titleUI.ToggleLoginPanel(false);  
        StartCoroutine(PostLogin(user));        
    }
    IEnumerator PostLogin(FirebaseUser user)
    {
        //�α��� ���� �� �г��� Ȯ��, �г����� ���ٸ� ������ �� ���� ���
        //�г����� �ִٸ� ���
        titleUI.ResetField(titleUI.loginEmailField, titleUI.loginpasswordField);
        titleUI.ToggleCreateNickNamePanel(true);        
        yield return new WaitUntil(predicate: () => !string.IsNullOrEmpty(user.DisplayName));
        titleUI.ToggleCreateNickNamePanel(false);
        titleUI.ShowMessage(titleUI.successMessage, "Login Successful!", true);
        serverCon.ConnectToServer();
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
        {//�г��� ����â�� ����ִٸ� �޼���: "�г����� �����ϼ���"
            titleUI.ShowMessage(titleUI.errorMessage, "CreateNickName!", true);
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
            .OrderByChild("UserNickName")
            .EqualTo(nickName).GetValueAsync();

        yield return new WaitUntil(predicate: () => nickNameCheckingTask.IsCompleted);
        if (nickNameCheckingTask.Exception != null)
        {//�г��� ��ȿ�� �˻� ���� �޼���: ����� �� ���� �г����Դϴ�.
            Debug.Log("�ߺ��г��Ӱ˻� ����!");
            titleUI.ShowMessage(titleUI.errorMessage, "This username is not available", true);
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
            titleUI.ShowMessage(titleUI.errorMessage, "Nickname is already in use!", true);
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
            .Child("UserNickName")
            .SetValueAsync(nickName);

        yield return new WaitUntil(predicate: () => nickNameTask.IsCompleted);
        if (nickNameTask.Exception != null)
        {
            //���̾�̽� �г��� ���� ���� �޼���: ����� �� ���� �г����Դϴ�
            Debug.Log("���̾�̽� �������!");
            titleUI.errorMessage.text = "This username is not available";
            yield break;
        }

        Debug.Log("���� ���� ����");
        FirebaseDBManager.Instance.User.UpdateUserProfileAsync(new UserProfile { DisplayName = nickName });
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "Creation complete!", true);
        titleUI.ToggleCreateNickNamePanel(false);
    }


    //������������ �����ϴ� ��ư �׽�Ʈ ���� ����ϱ� ���� Ŀ��Ʈ ������ �� �����ϱ�
    public void DeletuserProfile()
    {//�������Ͽ� ������ ���� �г��� �ʱ�ȭ�ϱ�        
        var nickNameTask = FirebaseDBManager.Instance.DbRef.Child("users").Child(FirebaseDBManager.Instance.User.UserId).Child("UserNickName").SetValueAsync(null);
        UserProfile userProfile = new UserProfile { DisplayName = null };
        var user = FirebaseDBManager.Instance.User.UpdateUserProfileAsync(userProfile);
    }
    public void Signup()
    {
        string email = titleUI.signUpEmailField.text;
        string password = titleUI.signUpPasswordField.text;
        StartCoroutine(SignUpCoroutine(email, password));
    }
    IEnumerator SignUpCoroutine(string email, string password)
    {        
        var signUpTask = FirebaseDBManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => signUpTask.IsCompleted);
        
        if (signUpTask.Exception != null)
        {
            SignUpError(signUpTask.Exception);
            yield break;
        }

        SignUpSuccess(signUpTask.Result.User);        
    }
    private void SignUpError(AggregateException exception)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

        string message = "";
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                message = "MissingEmail";
                break;
            case AuthError.MissingPassword:
                message = "MissingPassword";
                break;
            case AuthError.WeakPassword:
                message = "Kindly use at least 6 characters.";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "This email is already in use.";
                break;
            default:
                message = "Please contact the administrator";
                break;
        }
        titleUI.ShowMessage(titleUI.errorMessage, message, true);
    }
    private void SignUpSuccess(FirebaseUser user)
    {
        FirebaseDBManager.Instance.User = user;//���� ���� �����ϰ�
        titleUI.HideMessages();
        titleUI.ShowMessage(titleUI.successMessage, "Please Wait", true);
        //��ư ��Ȱ��ȭ
        titleUI.SetButtonsInteractable(false);

        StartCoroutine(FinishSignUp());
    }
    IEnumerator FinishSignUp()
    {
        yield return new WaitForSeconds(1); //1�� ���
        titleUI.ToggleSignUpPanel(false);
        //ȸ������ �Ϸ� �޼���
        titleUI.ShowMessage(titleUI.successMessage, "SignUp Successful!", true);
        yield return new WaitForSeconds(1f); //1�� ���
        titleUI.SetButtonsInteractable(true);
        titleUI.HideMessages();
        titleUI.ResetField(titleUI.signUpEmailField, titleUI.signUpPasswordField);
        titleUI.ToggleLoginPanel(true);
    }
}
