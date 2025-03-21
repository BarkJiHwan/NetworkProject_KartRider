using UnityEngine;

public class CHMTestWheelController : MonoBehaviour
{
    public float maxTorque = 10f; // �ִ� ��ũ
    public float maxSteerAngle = 30f; // �ִ� ���� ����
    public Transform[] wheels; // ���� Ʈ������ �迭 (0: ���� �չ���, 1: ������ �չ���, 2: ���� �޹���, 3: ������ �޹���)

    [Header("Steering Settings")]
    [Tooltip("�ּ� ���� ����")]
    public float steerAngleFrontMin = -45f;
    [Tooltip("�ִ� ���� ����")]
    public float steerAngleFrontMax = 45f;

    [Tooltip("��Ű�� ��ũ ȿ��")]
    public GameObject[] skidMarks;

    // īƮ ��Ʈ�ѷ� ����
    private CHMTestKartController kartController;

    void Start()
    {
        // īƮ ��Ʈ�ѷ� ���� ��������
        kartController = GetComponentInParent<CHMTestKartController>();

        // ��Ű�� ��ũ �ʱ� ��Ȱ��ȭ
        SetSkidMarkActive(false);
    }


    public void SetSkidMarkActive(bool isActive)
    {
        foreach (GameObject skidMark in skidMarks)
        {
            skidMark.GetComponent<TrailRenderer>().emitting = isActive;
        }
    }

    public void RotateWheel(float steerInput, float steeringSensitivity)
    {
        float steerAngleFrontLeft = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1) / 2) * steeringSensitivity;
        float steerAngleFrontRight = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1) / 2) * steeringSensitivity;

        wheels[0].localRotation = Quaternion.Euler(0, steerAngleFrontLeft - 90, wheels[0].localRotation.eulerAngles.z);
        wheels[2].localRotation = Quaternion.Euler(0, steerAngleFrontRight - 90, wheels[1].localRotation.eulerAngles.z);
    }


}
