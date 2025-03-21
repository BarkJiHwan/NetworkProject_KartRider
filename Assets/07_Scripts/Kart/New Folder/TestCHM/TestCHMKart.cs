using System.Collections;
using UnityEngine;

public class TestCHMKart : MonoBehaviour
{
    #region Serialized Fields

    [Header("Kart Components")]
    [SerializeField] private GameObject wheels;
    [SerializeField] private GameObject carBody;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 200f;
    [SerializeField] private float movementForce = 20f;
    [SerializeField] private float steerAngle = 200f;

    [Header("Drift Settings")]
    [SerializeField] private float minDriftAngle = 30f;
    [SerializeField] public float maxDriftAngle = 180f;
    [SerializeField] private float minDriftDuration = 0.2f;
    [SerializeField] private float maxDriftDuration = 2f;
    [SerializeField] private float minDriftForceMultiplier = 1f;
    [SerializeField] private float maxDriftForceMultiplier = 5f;
    [SerializeField] private float driftSpeedReduction = 0.7f;

    [Header("Boost Settings")]
    [SerializeField] private float boostSpeed = 280f;
    [SerializeField] private float boostDuration = 1.2f;
    [SerializeField] private int maxBoostGauge = 100;
    [SerializeField] private float boostChargeRate = 1f;
    [SerializeField] private float driftBoostChargeRate = 5f;
    [SerializeField] private float boostMaxSpeed = 280f;

    // ���� �ӵ��� �Ʒ��� ������ ���� ������ �� �ֵ��� ����ȭ�մϴ�.
    [SerializeField] private float rotationCorrectionSpeed = 5f;
    [SerializeField] private float downwardForce = 10f;

    #endregion

    #region Private Fields

    private CHMTestWheelController wheelCtrl;
    private Rigidbody rigid;

    // Drift and Boost related state
    private Coroutine postDriftBoostCoroutine;
    private float initialDriftSpeed;
    private bool isBoosting = false;
    public bool isDrifting = false;
    public float currentDriftAngle = 0f;
    private float driftDuration;
    private float currentDriftThreshold;   // �ӵ��� ���� �帮��Ʈ �Է� ���� ��
    private float driftForceMultiplier;     // �������� ���� �帮��Ʈ �� ���
    private int boostGauge = 0;
    private float lockedYRotation = 0f;     // �帮��Ʈ �� ������ Y ȸ�� ��

    #endregion

    #region Unity Methods

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<CHMTestWheelController>();
        rigid = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        // ���� �̵� ó�� �Ŀ� ���� ȸ�� ���� ó��
        CorrectAirborneRotation();
    }
    private void Update()
    {
        float steerInput = Input.GetAxis("Horizontal");
        float motorInput = Input.GetAxis("Vertical");

        // �ӵ��� ���� �帮��Ʈ ���� �Ķ���� ����
        AdjustDriftParameters();

        // �帮��Ʈ ����: LeftShift Ű�� ���� �Է��� ���� ��
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            StartDrift(steerInput * currentDriftThreshold);
        }

        // �帮��Ʈ �� �߰� �Է� ó��
        if (isDrifting && Input.GetKey(KeyCode.LeftShift))
        {
            currentDriftAngle += Time.deltaTime * 10f;
            currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
        }

        // �⺻ �ν�Ʈ (LeftControl) ó��
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostGauge >= maxBoostGauge)
        {
            StartBoost(boostDuration);
        }

        ChargeBoostGauge();
        HandleKartMovement(motorInput, steerInput);
    }

    #endregion

    #region Drift and Boost Methods

    private void AdjustDriftParameters()
    {
        // ���� �ӵ��� �������� 0~1 ���� (�ΰ��� 2��)
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed * 2f);
        // �ּ� ~ �ִ� �帮��Ʈ ���� ���� ���� ���� (�帮��Ʈ ���� �� ���� �Է�)
        currentDriftThreshold = Mathf.Lerp(minDriftAngle, maxDriftAngle, speedFactor);
        // �帮��Ʈ �� ����� �ӵ��� ���� ����
        driftForceMultiplier = Mathf.Lerp(minDriftForceMultiplier, maxDriftForceMultiplier, speedFactor);
    }

    private void StartDrift(float driftInputAngle)
    {
        isDrifting = true;
        currentDriftAngle = driftInputAngle;
        lockedYRotation = transform.eulerAngles.y;  // �帮��Ʈ ���� �� ȸ���� ����

        // �ӵ��� ���� �Է� ������� �帮��Ʈ ���� �ð� ���
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float steerFactor = Mathf.Abs(driftInputAngle / currentDriftThreshold);
        float influenceFactor = (speedFactor + steerFactor) / 2f;
        driftDuration = Mathf.Lerp(minDriftDuration, maxDriftDuration, influenceFactor);

        Debug.Log($"Drift Started: InputAngle={driftInputAngle}, Speed={currentSpeed}, SteerFactor={steerFactor}, Duration={driftDuration:F2}s");

        // �帮��Ʈ ���� ����
        Invoke(nameof(EndDrift), driftDuration);
    }

    private void EndDrift()
    {
        isDrifting = false;
        currentDriftAngle = 0f;
        Debug.Log("Drift Ended.");

        // �帮��Ʈ ���� �� 0.5�� ���� ���� �ν�Ʈ �Է� ��� (�ڷ�ƾ)
        if (postDriftBoostCoroutine != null)
            StopCoroutine(postDriftBoostCoroutine);
        postDriftBoostCoroutine = StartCoroutine(PostDriftBoostCoroutine());

        initialDriftSpeed = 0f;
    }

    private IEnumerator PostDriftBoostCoroutine()
    {
        float timer = 0f;
        bool boosted = false;
        Debug.Log("Waiting for instant boost input...");

        while (timer < 0.5f)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                PerformInstantBoost();
                boosted = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (!boosted)
            Debug.Log("Instant boost input window expired.");
        postDriftBoostCoroutine = null;
    }

    private void PerformInstantBoost()
    {
        Debug.Log("Instant Boost Activated!");
        rigid.velocity *= 1.2f;  // ���� �ӷ��� 1.2�� ����
        postDriftBoostCoroutine = null;
    }

    private void StartBoost(float duration)
    {
        isBoosting = true;
        boostGauge = 0;
        rigid.velocity = transform.forward * boostSpeed;
        Debug.Log("Boost Activated.");
        Invoke(nameof(EndBoost), duration);
    }

    private void EndBoost()
    {
        isBoosting = false;
        Debug.Log("Boost Ended.");
    }

    private void ChargeBoostGauge()
    {
        if (isDrifting)
            boostGauge += Mathf.RoundToInt(driftBoostChargeRate * Time.deltaTime);
        else
            boostGauge += Mathf.RoundToInt(boostChargeRate * Time.deltaTime);

        boostGauge = Mathf.Clamp(boostGauge, 0, maxBoostGauge);

        if (boostGauge >= maxBoostGauge)
            Debug.Log("Boost Gauge Full!");
    }

    #endregion

    #region Movement Handling

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        float currentMaxSpeed = isBoosting ? boostMaxSpeed : maxSpeed;

        if (isDrifting)
        {
            // �帮��Ʈ ���� �� �ʱ� �ӵ� ��� (���� �� ��)
            if (initialDriftSpeed == 0f)
            {
                initialDriftSpeed = rigid.velocity.magnitude;
            }

            // �帮��Ʈ ���� �ӵ� ���� ����
            float driftSpeed = initialDriftSpeed * driftSpeedReduction;

            // steering �Է��� �����Ͽ� �帮��Ʈ ������ ������Ʈ (���� ȿ�� ����)
            lockedYRotation += steerInput * (steerAngle / 2.5f) * Time.deltaTime;
            // ���ο� ���� ������ ��� (Y�� ȸ��)
            Quaternion driftRotation = Quaternion.Euler(0f, lockedYRotation, 0f);
            Vector3 driftDirection = driftRotation * Vector3.forward;

            // velocity�� �ε巴�� ���ο� driftDirection���� ��ȯ (Lerp�� �ڿ�������)
            rigid.velocity = Vector3.Lerp(rigid.velocity, driftDirection * driftSpeed, Time.deltaTime * 5f);

            // �ʿ��ϸ� �߰� ���� ���� ���� (�帮��Ʈ ���� ��ȭ)
            Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier * movementForce;
            rigid.AddForce(lateralForce, ForceMode.Force);
        }
        else
        {
            // ����(�帮��Ʈ �ƴ� ��): �̵��� velocity �Ҵ����� ó��
            initialDriftSpeed = 0f;
            Vector3 targetVelocity = transform.forward * motorInput * movementForce;
            rigid.velocity = Vector3.Lerp(rigid.velocity, targetVelocity, Time.deltaTime * 5f);

            // �帮��Ʈ�� �ƴ� ��쿣 ȸ���� �Ϲ������� ó��
            lockedYRotation = transform.eulerAngles.y; // ����
        }

        // �ִ� �ӵ� ����
        if (rigid.velocity.magnitude > currentMaxSpeed)
        {
            rigid.velocity = rigid.velocity.normalized * currentMaxSpeed;
        }

        // �ε巯�� ���� ��ȯ ���� (��� �̹� �帮��Ʈ ���� ���� ���������� ȸ�� ó����)
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steerAngle * Time.deltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));


        // �ʿ� �� ���� ȸ�� ������Ʈ (WheelController)
        if (wheelCtrl != null)
            wheelCtrl.UpdateAndRotateWheels(steerInput, motorInput, rigid.velocity.magnitude, isDrifting);
    }

    #endregion    
    private void CorrectAirborneRotation()
    {
        // ���鿡 ������� ������ ����
        if (!IsGrounded())
        {
            // ���� ȸ������ �����ͼ�, x�� z�� õõ�� 0���� �����մϴ�. (y�� �״�� ����)
            Vector3 currentEuler = transform.eulerAngles;
            float correctedX = Mathf.LerpAngle(currentEuler.x, 0f, Time.deltaTime * rotationCorrectionSpeed);
            float correctedZ = Mathf.LerpAngle(currentEuler.z, 0f, Time.deltaTime * rotationCorrectionSpeed);

            transform.rotation = Quaternion.Euler(correctedX, currentEuler.y, correctedZ);

            // �ʿ� �� �Ʒ������� ���� �߰��Ͽ� ���������� �� �� ����.
            rigid.AddForce(Vector3.down * downwardForce, ForceMode.Acceleration);
        }
    }

    private bool IsGrounded()
    {
        // ������ Raycast�� ����Ͽ� kart�� �ϴ��� ���鿡 ��Ҵ��� Ȯ���մϴ�.
        float rayDistance = 0.8f; // �ʿ信 ���� ���� ����
        return Physics.Raycast(transform.position, Vector3.down, rayDistance);
    }
}