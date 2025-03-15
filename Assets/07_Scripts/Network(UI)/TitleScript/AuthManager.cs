using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Database;
using OpenCover.Framework.Model;

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
    
    public void Login()
    {
        StartCoroutine(LoginCoroutine(titleUI.loginEmailField.text, titleUI.loginpasswordField.text));        
    }

    IEnumerator LoginCoroutine(string email, string password)
    {
        var loginTask = FirebaseDBManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebasEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebasEx.ErrorCode;
            string message = "";
            titleUI.errorMessage.gameObject.SetActive(true);
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
            titleUI.errorMessage.text = message;
            yield break;
        }
        else
        {
            FirebaseDBManager.Instance.User = loginTask.Result.User;
            titleUI.errorMessage.gameObject.SetActive(false);
            titleUI.successMessage.gameObject.SetActive(true);
            titleUI.successMessage.text = "Login Successful!";
            titleUI.loginPanel.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f); //��� ���
            titleUI.successMessage.gameObject.SetActive(false);
            titleUI.loginEmailField.text = "";
            titleUI.loginpasswordField.text = "";

            //�г����� ���ٸ� �г��� ���� �����
            titleUI.createNickNamePanel.gameObject.SetActive(true);
            yield return new WaitUntil(predicate: () => !string.IsNullOrEmpty(FirebaseDBManager.Instance.User.DisplayName));
            titleUI.createNickNamePanel.gameObject.SetActive(false);

            //�г����� �ְų� ������ٸ� ���� �õ���û
            serverCon.ConnectToServer();
        }
    }
    public void CreateNickNameBottenCon()
    {
        StartCoroutine(CreateNickNameCor(titleUI.createNickNameField.text));
    }
    IEnumerator CreateNickNameCor(string nickName)
    {
        titleUI.errorMessage.gameObject.SetActive(true);
        if (nickName == "")
        {
            titleUI.errorMessage.text = "CreateNickName!";
        }
        else
        {
            /// <summary>
            /// Firebase Realtime Database���� �г��� �ߺ� ���θ� Ȯ���մϴ�.
            /// </summary>
            /// <remarks>
            /// - �ֻ��� DbRef ��ο��� "users"�� �̵��մϴ�. �ش� ��ΰ� ������ �ڵ����� �����˴ϴ�.
            /// - "users" ��ο� �ִ� �����͸� "UserNickName" �ʵ尪�� �������� �����մϴ�.
            ///   Firebase �����ʹ� JSON ������ ����ǹǷ�, �ʵ尪�� �������� ������ �����մϴ�.
            /// - EqualTo(nickName)�� ����Ͽ� "UserNickName" �ʵ尪�� nickName�� ������ �����͸� ���͸��մϴ�.
            /// - GetValueAsync()�� ȣ���Ͽ� �����͸� �񵿱�� �����ɴϴ�. �۾� ����� Task�� ��ȯ�˴ϴ�.
            /// </remarks>
            /// <param name="nickName">�ߺ� ���θ� Ȯ���� �г���</param>
            /// <returns>Task�� ���� ���͸��� �����͸� ��ȯ�մϴ�.</returns>
            var nickNameCheckingTast = FirebaseDBManager.Instance.DbRef.Child("users")
                .OrderByChild("UserNickName")
                .EqualTo(nickName).GetValueAsync();

            yield return new WaitUntil(predicate: () => nickNameCheckingTast.IsCompleted);
            if (nickNameCheckingTast.Exception != null)
            {//�г��� ��ȿ�� �˻� ���� �޼���: ����� �� ���� �г����Դϴ�.
                Debug.Log("�ߺ��г��Ӱ˻� ����!");                
                titleUI.errorMessage.text = "This username is not available";
                yield break;
            }

            /// <summary>
            /// �־��� �г����� Firebase Realtime Database�� �̹� �����ϴ��� Ȯ���մϴ�.
            /// </summary>
            /// <remarks>
            /// - �г��� �˻��۾��� ����� DataSnapshot���� �����ɴϴ�.
            /// - Exists �Ӽ��� ����Ͽ� �г����� �̹� ��������� Ȯ���մϴ�.
            /// - �г����� ������ ���, ���� �޽����� ǥ���ϰ� ���� ó���� �ߴ��մϴ�.
            /// </remarks>
            /// <param name="nickName">�ߺ� ���θ� Ȯ���� �г���</param>
            DataSnapshot nickNameSnapshot = nickNameCheckingTast.Result;
            if (nickNameSnapshot.Exists)
            {//�޼���: �̹� ������� �г����Դϴ�.
                titleUI.errorMessage.text = "Nickname is already in use!";
                yield break;
            }

            /// <summary>
            /// ���� ������� �г����� Firebase Realtiem Database�� �����մϴ�.
            /// </summary>
            /// <remarks>
            /// - users/UserId/UserNickName ��η� �̵��մϴ�.
            /// - �ش� ����� "UserNickName" �ʵ忡 �г��� ���� �����մϴ�.
            /// - SetValueAsync() �޼��带 ȣ���Ͽ� �����͸� �񵿱�� �����ϰ� ������ �Ϸ� �Ǹ� Task�� ��ȯ�մϴ�.
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
            {//�г��� ���� �Ϸ�!
                UserProfile userProfile = new UserProfile { DisplayName = nickName };
                var profileTask = FirebaseDBManager.Instance.User.UpdateUserProfileAsync(userProfile);
                titleUI.errorMessage.gameObject.SetActive(false);
                titleUI.successMessage.gameObject.SetActive(true);
                titleUI.successMessage.text = "Creation complete!";
                yield break;
            }
        }
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
        StartCoroutine(SignUpCoroutine(titleUI.signUpEmailField.text, titleUI.signUpPasswordField.text));
    }
    IEnumerator SignUpCoroutine(string email, string password)
    {        
        var signUpTask = FirebaseDBManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => signUpTask.IsCompleted);
        titleUI.loginPanel.gameObject.SetActive(false);
        titleUI.signUpPanel.gameObject.SetActive(true);
        if (signUpTask.Exception != null)
        {
            FirebaseException firebaseEx = signUpTask.Exception.GetBaseException() as FirebaseException;

            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "";
            titleUI.errorMessage.gameObject.SetActive(true);
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
            titleUI.errorMessage.text = message;
            yield break;
        }
        else
        {
            FirebaseDBManager.Instance.User = signUpTask.Result.User;//���� ���� �����ϰ�
            titleUI.signUpCompleteBtn.interactable = false; //��� ��ư ��Ȱ��ȭ
            titleUI.signUpLoingToBtn.interactable = false; //��� ��ư ��Ȱ��ȭ
            titleUI.successMessage.gameObject.SetActive(true);//�޼��� Ű��
            titleUI.successMessage.text = "Please Wait"; //��� ��ٸ�����...(���� ���� ���� ���վ�ƿ �����°� ����)
            yield return new WaitForSeconds(1); //1�� ���
            titleUI.signUpPanel.gameObject.SetActive(false); //ȸ������â ����
            titleUI.successMessage.text = "SignUp Successful!";//���� ���� �Ϸ�
            yield return new WaitForSeconds(1f); //��� ���
            titleUI.signUpCompleteBtn.interactable = true; //��ư Ȱ��ȭ
            titleUI.signUpLoingToBtn.interactable = true; //��ư Ȱ��ȭ
            titleUI.successMessage.gameObject.SetActive(false); //�޼��� ����
            titleUI.signUpEmailField.text = "";
            titleUI.signUpPasswordField.text = "";
            titleUI.loginPanel.gameObject.SetActive(true); // �α��� �ǳ� Ű��
            yield break;
        }
    }
}
