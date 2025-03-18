using UnityEngine;

public class TestKartController : MonoBehaviour
{
    [Header("Kart Components")]
    [SerializeField] public GameObject wheels; // �� ��Ʈ�ѷ� ������Ʈ
    [SerializeField] public GameObject carBody; // ���� ��ü

    [Header("Movement Settings")]
    [SerializeField] public float maxSpeed = 200f; // �ִ� �ӵ�
    [SerializeField] public float movementForce = 1000f; // �̵� ��
    [SerializeField] public float steerAngle = 200f; // ���� ����

    [Header("Drift Settings")]
    [SerializeField] public float maxDriftAngle = 45f; // �ִ� �帮��Ʈ ����
    [SerializeField] public float minDriftDuration = 1.0f; // �ּ� �帮��Ʈ ���� �ð�
    [SerializeField] public float maxDriftDuration = 2.0f; // �ִ� �帮��Ʈ ���� �ð�
    [SerializeField] public float driftForceMultiplier = 0.4f; // �帮��Ʈ �� ����
    [SerializeField] public float driftSpeedReduction = 0.8f; // �帮��Ʈ �� �ӵ� ���� ����

    [Header("Boost Settings")]
    [SerializeField] public float boostSpeed = 280f; // �ν��� �ӵ�
    [SerializeField] public float boostDuration = 1.2f; // �ν��� ���� �ð�
    [SerializeField] public int maxBoostGauge = 100; // �ִ� �ν��� ������
    [SerializeField] public float boostChargeRate = 1f; // ���� �� �ν��� ���� �ӵ�
    [SerializeField] public float driftBoostChargeRate = 5f; // �帮��Ʈ �� �ν��� ���� �ӵ�

    private TestWheelController wheelCtrl;
    private Rigidbody rigid;

    private bool isDrifting = false;
    private bool isBoosting = false;
    private float currentDriftAngle = 0f;
    private float driftDuration;
    private int boostGauge = 0;

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<TestWheelController>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float steerInput = Input.GetAxis("Horizontal");
        float motorInput = Input.GetAxis("Vertical");

        // �帮��Ʈ ����
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            StartDrift(steerInput * maxDriftAngle);
        }

        // �帮��Ʈ �� ���� ����
        if (isDrifting && Input.GetKey(KeyCode.LeftShift))
        {
            currentDriftAngle += Time.deltaTime * 10f; // ���� ����
            currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
        }

        // �ν��� ����
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostGauge >= maxBoostGauge)
        {
            StartBoost(boostDuration);
        }

        // �ν��� ������ ����
        ChargeBoostGauge();

        // �̵� ó��
        HandleKartMovement(motorInput, steerInput);
    }

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        // �⺻ �̵� ó��
        Vector3 forwardForce = transform.forward * motorInput * movementForce;

       // if (isDrifting)
       // {
       //     // �帮��Ʈ �� �ӵ� ����
       //     forwardForce *= driftSpeedReduction;
       //
       //     // �帮��Ʈ �������� �� �߰�
       //     Vector3 driftForce = transform.right * currentDriftAngle * motorInput * movementForce * driftForceMultiplier;
       //     rigid.AddForce(driftForce, ForceMode.Force);
       // }

        // �⺻ �� �߰�
        rigid.AddForce(forwardForce, ForceMode.Force);

        // ���� ó��
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steerAngle * Time.deltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));

        // �� ������Ʈ
        if (wheelCtrl != null)
        {
            wheelCtrl.UpdateAndRotateWheels(steerInput, motorInput, rigid.velocity.magnitude, isDrifting);
        }
    }

    private void StartDrift(float driftAngle)
    {
        isDrifting = true;
        currentDriftAngle = driftAngle;

        // �帮��Ʈ ���� �ð� ���
        float steerInputAbs = Mathf.Abs(driftAngle / maxDriftAngle);
        if (steerInputAbs <= 0.3f)
        {
            driftDuration = minDriftDuration; // ª�� �帮��Ʈ
        }
        else if (steerInputAbs > 0.3f && steerInputAbs <= 0.7f)
        {
            driftDuration = (minDriftDuration + maxDriftDuration) / 2; // �߰� �帮��Ʈ
        }
        else
        {
            driftDuration = maxDriftDuration; // �ִ� �帮��Ʈ
        }

        Debug.Log($"Drift started with angle: {driftAngle}, duration: {driftDuration}s");

        // �帮��Ʈ ���� �ð� �� ����
        Invoke(nameof(EndDrift), driftDuration);
    }

    private void EndDrift()
    {
        isDrifting = false;
        currentDriftAngle = 0f;
        Debug.Log("Drift ended.");
    }

    private void StartBoost(float duration)
    {
        isBoosting = true;
        boostGauge = 0;

        // �ν��� �ӵ� ����
        float boostForce = rigid.velocity.magnitude * 1.2f;
        rigid.AddForce(transform.forward * boostForce, ForceMode.VelocityChange);

        // �ν��� ����
        Invoke(nameof(EndBoost), duration);
        Debug.Log("Boost started.");
    }

    private void EndBoost()
    {
        isBoosting = false;
        Debug.Log("Boost ended.");
    }

    private void ChargeBoostGauge()
    {
        if (isDrifting)
        {
            boostGauge += Mathf.RoundToInt(driftBoostChargeRate * Time.deltaTime);
        }
        else
        {
            boostGauge += Mathf.RoundToInt(boostChargeRate * Time.deltaTime);
        }

        boostGauge = Mathf.Clamp(boostGauge, 0, maxBoostGauge);

        if (boostGauge >= maxBoostGauge)
        {
            Debug.Log("Boost ready!");
        }
    }
}
