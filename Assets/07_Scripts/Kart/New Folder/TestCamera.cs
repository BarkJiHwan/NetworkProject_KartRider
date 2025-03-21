using UnityEngine;
using Cinemachine;

public class TestCamera : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    public Transform kart; // īƮ Transform
    public float driftOffsetX = 0f; // �帮��Ʈ �� X�� �߰� �̵���
    public float driftOffsetZ = 0f; // �帮��Ʈ �� Z�� �߰� �̵���
    public float driftSmoothTime = 0.5f; // ��ȯ �ε巯�� �ӵ�

    private CinemachineTransposer transposer;
    private Vector3 initialOffset; // �ʱ� Follow Offset
    private CHMTestKartController kartController; // īƮ ��Ʈ�ѷ� ��ũ��Ʈ
    private float targetOffsetX; // ��ǥ X ������
    private float currentOffsetX; // ���� X ������ (����)

    void Start()
    {
        if (kart != null)
        {
            kartController = kart.GetComponent<CHMTestKartController>();
        }

        // Cinemachine Transposer ������Ʈ ��������
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            initialOffset = transposer.m_FollowOffset;
            currentOffsetX = initialOffset.x;
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer�� ã�� �� �����ϴ�!");
        }
    }

   // void LateUpdate()
   // {
   //     if (kartController == null || transposer == null) return;
//
   //     // �帮��Ʈ ���¶�� X�� Z�� �̵�
   //     if (kartController.isDrifting)
   //     {
   //         float driftDirection = kartController.steeringDirection; // �帮��Ʈ ����
   //         targetOffsetX = initialOffset.x + (driftDirection * driftOffsetX);
//
   //         // Follow Offset�� Z�൵ ������Ʈ
   //         Vector3 followOffset = transposer.m_FollowOffset;
   //         followOffset.z = initialOffset.z - driftOffsetZ;
//
   //         // �ε巴�� X, Z �ݿ�
   //         followOffset.x = Mathf.Lerp(currentOffsetX, targetOffsetX, Time.deltaTime / driftSmoothTime);
   //         transposer.m_FollowOffset = followOffset;
//
   //         // X�� ���� ������ ����
   //         currentOffsetX = followOffset.x;
   //     }
   //     else
   //     {
   //         // �帮��Ʈ�� �����Ǿ��� �� �ʱ� Offset���� ����
   //         Vector3 followOffset = transposer.m_FollowOffset;
   //         followOffset.x = Mathf.Lerp(currentOffsetX, initialOffset.x, Time.deltaTime / driftSmoothTime);
   //         followOffset.z = Mathf.Lerp(followOffset.z, initialOffset.z, Time.deltaTime / driftSmoothTime);
   //         transposer.m_FollowOffset = followOffset;
//
   //         currentOffsetX = followOffset.x;
   //     }
   // }
}
