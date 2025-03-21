using UnityEngine;

public class DriftManager : MonoBehaviour
{
    [Header("Drift Settings")]
    [SerializeField] public float driftForceMultiplier = 2f;
    [SerializeField] public float driftSpeedReduction = 0.7f;

    private Rigidbody rigid;
    private bool isDrifting = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public void StartDrift(float steerInput)
    {
        isDrifting = true;

        // ���� �� �߰�
        Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier;
        rigid.AddForce(lateralForce, ForceMode.Force);

        // �帮��Ʈ �� �ӵ� ����
        rigid.velocity *= driftSpeedReduction;

        Debug.Log("�帮��Ʈ ����");
    }

    public void EndDrift()
    {
        isDrifting = false;
        Debug.Log("�帮��Ʈ ����");
    }

    public bool IsDrifting()
    {
        return isDrifting;
    }
}