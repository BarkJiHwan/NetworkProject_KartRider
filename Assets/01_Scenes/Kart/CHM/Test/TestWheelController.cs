using UnityEngine;

public class TestWheelController : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] public Transform[] wheels; // ���� ��ü �迭
    [SerializeField] public TrailRenderer[] skidMarks; // ��Ű�� ��ũ TrailRenderer �迭

    [Header("���� ����")]
    [SerializeField] public float steerAngleFrontMin = -45f; // �չ��� �ּ� ���� ����
    [SerializeField] public float steerAngleFrontMax = 45f;  // �չ��� �ִ� ���� ����
    [SerializeField] public float steeringSensitivity = 0.5f; // ���� �ΰ���

    [Header("���� ȸ�� ����")]
    [SerializeField] public float wheelRotationSpeed = 100f; // ���� ȸ�� �ӵ�

    private float driftTimer = 0f; // �帮��Ʈ Ÿ�̸�
    private bool isDrifting = false; // �帮��Ʈ ����

    // �帮��Ʈ ���� �޼���
    public void StartDrift(float driftDuration)
    {
        isDrifting = true;
        driftTimer = driftDuration; // �帮��Ʈ ���� �ð� ����
        Debug.Log($"�帮��Ʈ ����: ���� �ð� = {driftDuration}��");
    }

    private void Update()
    {
        // �帮��Ʈ ���� �ð� üũ
        if (isDrifting)
        {
            driftTimer -= Time.deltaTime;
            Debug.Log($"�帮��Ʈ Ÿ�̸�: ���� �ð� = {driftTimer}��");

            if (driftTimer <= 0f)
            {
                isDrifting = false; // �帮��Ʈ ����
                Debug.Log("�帮��Ʈ ����");
            }
        }
    }

    // ���� ������Ʈ �� ȸ��
    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        // �չ��� ���� ���� ���
        float steerAngleFrontLeft = Mathf.Clamp(steerInput * steeringSensitivity, steerAngleFrontMin, steerAngleFrontMax);
        float steerAngleFrontRight = Mathf.Clamp(steerInput * steeringSensitivity, steerAngleFrontMin, steerAngleFrontMax);

        // �չ��� ���� ����
        wheels[0].localRotation = Quaternion.Euler(0, steerAngleFrontLeft , wheels[0].localRotation.eulerAngles.z);
        wheels[1].localRotation = Quaternion.Euler(0, steerAngleFrontRight , wheels[1].localRotation.eulerAngles.z);

        // ��� ���� ȸ��
        foreach (Transform wheel in wheels)
        {
            float rotationZ = motorInput * speed * Time.deltaTime * wheelRotationSpeed;
            wheel.Rotate(Vector3.back * rotationZ, Space.Self);
        }

        // ��Ű�� ��ũ ���� ����
        for (int i = 0; i < skidMarks.Length; i++)
        {
            skidMarks[i].emitting = isDrifting; // �帮��Ʈ ���¿� ���� ��Ű�� ��ũ Ȱ��ȭ
        }
    }

}
