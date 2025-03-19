using UnityEngine;
using Cinemachine;

public class DriftCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera
    public TestKartController kartController; // �帮��Ʈ ���θ� Ȯ���� ��Ʈ�ѷ�

    [Header("Drift Camera Settings")]
    public float driftOffsetX = 5f; // �帮��Ʈ �� X�� �߰� ������
    public float driftSmoothTime = 0.5f; // �帮��Ʈ �� ������ ��ȯ �ð�

    private CinemachineTransposer transposer;
    private float originalOffsetX; // ���� X ������
    private float targetOffsetX; // ��ǥ X ������
    private float currentOffsetX; // ���� X ������ (����)
    private const float fixedOffsetZ = -10f; // Z ���� �׻� -10���� ����

    private void Start()
    {
        // Transposer ������Ʈ ��������
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            originalOffsetX = transposer.m_FollowOffset.x;
            currentOffsetX = originalOffsetX;

            // Z �������� -10���� ����
            Vector3 followOffset = transposer.m_FollowOffset;
            followOffset.z = fixedOffsetZ;
            transposer.m_FollowOffset = followOffset;
        }
        else
        {
            Debug.LogWarning("Cinemachine Transposer�� �������� �ʾҽ��ϴ�!");
        }
    }

    private void LateUpdate()
    {
        if (kartController == null || transposer == null) return;

        // �帮��Ʈ ���� ��� X ������ �̵�
        if (kartController.isDrifting)
        {
            float driftDirection = kartController.currentDriftAngle > 0 ? 1f : -1f; // �帮��Ʈ ���� ����
            targetOffsetX = originalOffsetX + (driftDirection * driftOffsetX);
        }
        else
        {
            targetOffsetX = originalOffsetX; // �帮��Ʈ�� �ƴ� ��� ����
        }

        // �ε巴�� X ������ ��ȯ
        currentOffsetX = Mathf.Lerp(currentOffsetX, targetOffsetX, driftSmoothTime * Time.deltaTime);

        // Transposer�� Follow Offset ������Ʈ
        Vector3 followOffset = transposer.m_FollowOffset;
        followOffset.x = currentOffsetX;
        followOffset.z = fixedOffsetZ; // Z �������� �׻� -10���� ����
        transposer.m_FollowOffset = followOffset;
    }
}
