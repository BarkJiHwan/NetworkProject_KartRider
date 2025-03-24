using UnityEngine;
using Cinemachine;

public class TestCHMCamer : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Cinemachine Virtual Camera ��ü")]
    public CinemachineVirtualCamera virtualCamera;
    [Tooltip("īƮ�� ���� Ÿ�� (TestKartController�� ���� GameObject)")]
    public Transform target;
    [Tooltip("TestKartController ��ũ��Ʈ ����")]
    public TestCHMKart kartController;

    [Header("Drift Camera Settings")]
    [Tooltip("�帮��Ʈ �� �ִ� �� ���� (�� ����): �帮��Ʈ ������ �ݴ������� ȸ��")]
    public float maxPanAngleDelta = 15f;
    [Tooltip("������ ��ȯ �ε巯�� (�������� ��ȯ�� ����)")]
    public float smoothTime = 0.3f;

    private CinemachineTransposer transposer;
    private Vector3 initialOffset;
    private float baseDistance;   // x,z ������ ��ü �Ÿ� (����)
    private float baseAngle;      // �ʱ� �������� ���� (����)

    private void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera ������ �����ϴ�!");
            return;
        }
    }

    private void LateUpdate()
    {
        if (kartController == null || transposer == null) return;

        Vector3 currentOffset = transposer.m_FollowOffset;
        Vector3 targetOffset = initialOffset;

        if (kartController.isDrifting)
        {
            // �帮��Ʈ ����(0~1): ���� �帮��Ʈ ������ ���밪 / �ִ� �帮��Ʈ ����
            float driftIntensity = Mathf.Clamp01(Mathf.Abs(kartController.currentDriftAngle) / kartController.maxDriftAngle);
            // �帮��Ʈ ����: �����̸� +1, �����̸� -1
            float driftSign = Mathf.Sign(kartController.currentDriftAngle);
            // ī�޶� �帮��Ʈ �ݴ������� �ҵǾ�� �ϹǷ�, 
            // �� ������ �⺻ �������� (�帮��Ʈ �������ִ� �Ұ�, �� ����)�� �帮��Ʈ ������ ��ȣ�� ���� ���ݴϴ�.
            float newAngle = baseAngle - driftSign * driftIntensity * maxPanAngleDelta * Mathf.Deg2Rad;

            // ��ü ���� �Ÿ��� baseDistance�� �����ϰ�, �� ������ ���� x, z �� ����մϴ�.
            targetOffset.x = baseDistance * Mathf.Sin(newAngle);
            targetOffset.z = baseDistance * Mathf.Cos(newAngle);
            targetOffset.y = initialOffset.y; // y���� �״�� ����
        }
        else
        {
            targetOffset = initialOffset;
        }

        // �ε巯�� ��ȯ
        transposer.m_FollowOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime / smoothTime);
    }

    public void SetKart(GameObject kart)
    {
        Debug.Log("SetKart ȣ��!");
        
        if(kart == null)
        {
            Debug.LogWarning("īƮ ������ �����ϴ�!");
            return;
        }
        
        target = kart.transform;
        kartController = kart.GetComponent<TestCHMKart>();
        
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            initialOffset = transposer.m_FollowOffset;
            Vector2 horiz = new Vector2(initialOffset.x, initialOffset.z);
            baseDistance = horiz.magnitude;
            // Convention: x = r*sin(theta), z = r*cos(theta)
            baseAngle = Mathf.Atan2(initialOffset.x, initialOffset.z);
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer ������Ʈ�� ã�� �� �����ϴ�!");
        }        
    }
}