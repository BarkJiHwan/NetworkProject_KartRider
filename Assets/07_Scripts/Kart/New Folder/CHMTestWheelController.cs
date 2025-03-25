//using UnityEngine;

//public class CHMTestWheelController : MonoBehaviour
//{

//    [Header("Steering Settings")]
//    [Tooltip("�ּ� ���� ����")]
//    public float steerAngleFrontMin = -45f;
//    [Tooltip("�ִ� ���� ����")]
//    public float steerAngleFrontMax = 45f;

//    [Tooltip("��Ű�� ��ũ ȿ��")]
//    public GameObject[] skidMarks;

//    public float maxTorque = 30f; // �ִ� ��ũ
//    public float maxSteerAngle = 30f; // �ִ� ���� ����
//    public Transform[] wheels; // ���� Ʈ������ �迭 (0: ���� �չ���, 1: ������ �չ���, 2: ���� �޹���, 3: ������ �޹���)

//    // īƮ ��Ʈ�ѷ� ����
//    private TestCHMKart kartController;

//    void Start()
//    {
//        // īƮ ��Ʈ�ѷ� ���� ��������
//        kartController = GetComponentInParent<TestCHMKart>();

//        // ��Ű�� ��ũ �ʱ� ��Ȱ��ȭ
//        SetSkidMarkActive(false);
//    }


//    public void SetSkidMarkActive(bool isActive)
//    {
//        foreach (GameObject skidMark in skidMarks)
//        {
//            skidMark.GetComponent<TrailRenderer>().emitting = isActive;
//        }
//    }

//    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
//    {
//        float steeringSensitivity = 1.0f; // �ʿ信 ���� ����

//        // ���� ����(���� ��, ������ ��)�� �����մϴ�.
//        if (wheels.Length >= 2)
//        {
//            // ���� �չ���
//            float leftSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
//            wheels[0].localRotation = Quaternion.Euler(0, leftSteerAngle-90, wheels[0].localRotation.eulerAngles.z);

//            // ������ �չ���
//            float rightSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
//            wheels[1].localRotation = Quaternion.Euler(0, rightSteerAngle-90, wheels[1].localRotation.eulerAngles.z);
//        }

//        // ��ü ������ ���� ȸ��(ȸ������ ���� X��)�� �����մϴ�.
//        // ȸ������ �ӵ��� motorInput(���� �Է�)�� �ݿ��ϵ��� �մϴ�.
//        float spinAngle = Mathf.Abs(speed) * Time.deltaTime * maxTorque; // (20f�� ������ ����� ��Ȳ�� �°� ����)
//        foreach (Transform wheel in wheels)
//        {
//            // �����̸� ������, �����̸� �ݴ�� ȸ���ϵ��� (�ʿ� �� �߰� ���� ó�� ����)
//            wheel.Rotate(Vector3.forward, spinAngle, Space.Self);
//        }

//        // �帮��Ʈ ���¶�� ��Ű�� ��ũ ȿ�� Ȱ��ȭ
//        SetSkidMarkActive(isDrifting);
//    }

//}
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

    [Header("Anti-Roll Settings")]
    [Tooltip("��ü �� ������ ����� �� (���� Ŭ���� �� ���� ������ �������ϴ�)")]
    public float antiRollForce = 5000f;

    // īƮ ��Ʈ�ѷ� �� Rigidbody ����
    private TestCHMKart kartController;
    private Rigidbody rb;

    void Start()
    {
        // �θ� ��ü���� īƮ ��Ʈ�ѷ��� Rigidbody ��������
        kartController = GetComponentInParent<TestCHMKart>();
        rb = GetComponentInParent<Rigidbody>();

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

    /// <summary>
    /// �Է°��� ������� ������ ȸ�� �� ����, ȸ��(spin) ó���ϰ� �⺻ ��Ƽ�� ����� �����մϴ�.
    /// </summary>
    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        float steeringSensitivity = 1.0f; // �ʿ信 ���� ����

        // ���� ����(���� ��, ������ ��) ���� ó��
        if (wheels.Length >= 2)
        {
            // ���� �չ���
            float leftSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[0].localRotation = Quaternion.Euler(0, leftSteerAngle - 90, wheels[0].localRotation.eulerAngles.z);

            // ������ �չ���
            float rightSteerAngle = Mathf.Lerp(steerAngleFrontMin, steerAngleFrontMax, (steerInput + 1f) / 2f) * steeringSensitivity;
            wheels[1].localRotation = Quaternion.Euler(0, rightSteerAngle - 90, wheels[1].localRotation.eulerAngles.z);
        }

        // ��� ������ ���� ȸ��(spin) ����
        float spinAngle = Mathf.Abs(speed) * Time.deltaTime * maxTorque;
        foreach (Transform wheel in wheels)
        {
            // �����̸� ������, �����̸� �ݴ�� ȸ���ϵ��� ���� �߰� ����
            wheel.Rotate(Vector3.forward, spinAngle, Space.Self);
        }

        // �帮��Ʈ ���¶�� ��Ű�� ��ũ Ȱ��ȭ
        SetSkidMarkActive(isDrifting);        
    }   
}