using UnityEngine;

public class CHMTestWheelController : MonoBehaviour
{

    [Header("Steering Settings")]
    [Tooltip("�ּ� ���� ����")]
    public float steerAngleFrontMin = -45f;
    [Tooltip("�ִ� ���� ����")]
    public float steerAngleFrontMax = 45f;

    [Tooltip("��Ű�� ��ũ ȿ��")]
    public GameObject[] skidMarks;

    public float maxTorque = 30f; // �ִ� ��ũ
    public float maxSteerAngle = 30f; // �ִ� ���� ����
    public Transform[] wheels; // ���� Ʈ������ �迭 (0: ���� �չ���, 1: ������ �չ���, 2: ���� �޹���, 3: ������ �޹���)

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
  
    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        float steeringSensitivity = 1.0f; // �ʿ信 ���� ����

        // ���� ����(���� ��, ������ ��)�� �����մϴ�.
        if (wheels.Length >= 2)
        {
            // ���� �չ���
            float leftSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[0].localRotation = Quaternion.Euler(90f, leftSteerAngle, wheels[0].localRotation.eulerAngles.z);

            // ������ �չ���
            float rightSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[1].localRotation = Quaternion.Euler(90f, rightSteerAngle, wheels[1].localRotation.eulerAngles.z);
        }

        // ��ü ������ ���� ȸ��(ȸ������ ���� X��)�� �����մϴ�.
        // ȸ������ �ӵ��� motorInput(���� �Է�)�� �ݿ��ϵ��� �մϴ�.
        float spinAngle = Mathf.Abs(speed) * Time.deltaTime * maxTorque; // (20f�� ������ ����� ��Ȳ�� �°� ����)
        foreach (Transform wheel in wheels)
        {
            // �����̸� ������, �����̸� �ݴ�� ȸ���ϵ��� (�ʿ� �� �߰� ���� ó�� ����)
            wheel.Rotate(Vector3.forward, spinAngle, Space.Self);
        }

        // �帮��Ʈ ���¶�� ��Ű�� ��ũ ȿ�� Ȱ��ȭ
        SetSkidMarkActive(isDrifting);
    }

}
