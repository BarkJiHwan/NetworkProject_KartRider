using UnityEngine;

public class TestWheelController : MonoBehaviour
{
    [Header("Wheels Settings")]
    [SerializeField] public Transform[] wheels; // ���� �迭
    [SerializeField] public TrailRenderer[] skidMarks; // ��Ű�� ��ũ�� ���� ������ �迭

    [Header("Steering Settings")]
    [SerializeField] public float steerAngleFrontMin = -45f; // �չ��� �ּ� ���� ����
    [SerializeField] public float steerAngleFrontMax = 45f;  // �չ��� �ִ� ���� ����
    [SerializeField] public float steeringSensitivity = 1.0f; // ���� �ΰ���
    

    [Header("Wheel Rotation Settings")]
    [SerializeField] public float wheelRotationSpeed = 100f; // ���� ȸ�� �ӵ�

    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        // �չ��� �¿� ���� ���� ���
        float steerAngleFrontLeft = Mathf.Clamp(steerInput * steeringSensitivity, steerAngleFrontMin, steerAngleFrontMax);
        float steerAngleFrontRight = Mathf.Clamp(steerInput * steeringSensitivity, steerAngleFrontMin, steerAngleFrontMax);

        // �չ��� ���� ó��
        wheels[0].localRotation = Quaternion.Euler(0, steerAngleFrontLeft, wheels[0].localRotation.eulerAngles.z);
        wheels[1].localRotation = Quaternion.Euler(0, steerAngleFrontRight, wheels[1].localRotation.eulerAngles.z);

        // ���� Z�� ȸ�� ó��
        foreach (Transform wheel in wheels)
        {
            float rotationZ = motorInput * speed * Time.deltaTime * wheelRotationSpeed;
            wheel.Rotate(Vector3.back * rotationZ, Space.Self);
        }

        // �帮��Ʈ �� ��Ű�� ��ũ Ȱ��ȭ
        for (int i = 0; i < skidMarks.Length; i++)
        {
            if (isDrifting)
            {
                skidMarks[i].emitting = true; // ��Ű�� ��ũ ǥ��
            }
            else
            {
                skidMarks[i].emitting = false; // ��Ű�� ��ũ ��Ȱ��ȭ
            }
        }
    }
}
