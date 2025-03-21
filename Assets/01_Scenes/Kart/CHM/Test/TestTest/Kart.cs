using UnityEngine;

public class Kart : MonoBehaviour
{
    [Header("Kart Components")]
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private WheelManager wheelManager;
    [SerializeField] private VelocityManager velocityManager;
    [SerializeField] private DriftManager driftManager;
    [SerializeField] private BoostManager boostManager;

    [Header("Movement Settings")]
    [SerializeField] private float movementForce = 200f;
    [SerializeField] private float steerAngle = 800f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        wheelManager = GetComponent<WheelManager>();
        velocityManager = GetComponent<VelocityManager>();
        driftManager = GetComponent<DriftManager>();
        boostManager = GetComponent<BoostManager>();
    }

    private void Update()
    {
        float motorInput = Input.GetAxis("Vertical"); // ����/���� �Է�
        float steerInput = Input.GetAxis("Horizontal"); // ���� �Է�

        // �帮��Ʈ ����
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            driftManager.StartDrift(steerInput);
        }

        // �ν�Ʈ Ȱ��ȭ
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostManager.CanBoost())
        {
            boostManager.StartBoost();
        }

        // �ν�Ʈ ������ ����
        boostManager.ChargeBoostGauge(driftManager.IsDrifting());

        // �̵� ó��
        HandleKartMovement(motorInput, steerInput);
    }

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        // ���� ���� ó��
        bool isReversing = motorInput < 0;

        // �ӵ� ������Ʈ (VelocityManager�� ���� ó��)
        velocityManager.UpdateSpeed(motorInput, Time.deltaTime, driftManager.IsDrifting());

        // ���� �ӵ� ��������
        float currentSpeed = velocityManager.GetCurrentSpeed();

        // �̵� ó��
        if (currentSpeed != 0) // �ӵ��� 0�� �ƴ� ���� �̵� ó��
        {
            Vector3 movementDirection = transform.forward * currentSpeed * Time.deltaTime;
            rigid.AddForce(movementDirection, ForceMode.Force);
        }

        // ���� ó�� (motorInput�� �����ϰ� ����)
        float turnAmount = steerInput * steerAngle * Time.deltaTime * (isReversing ? -1 : 1);
        transform.Rotate(Vector3.up, turnAmount);

        // �� �Ŵ��� ������Ʈ
        wheelManager.UpdateAndRotateWheels(steerInput, motorInput, currentSpeed, driftManager.IsDrifting());
    }
}