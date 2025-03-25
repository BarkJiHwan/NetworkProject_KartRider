using System.Collections;
using Photon.Pun;
using UnityEngine;

public class TestCHMKart : MonoBehaviour
{
    #region Serialized Fields

    [Header("īƮ ���� ���")]
    [SerializeField] private GameObject wheels;   // ���� ������Ʈ

    [Header("�̵� ����")]
    [SerializeField] private float maxSpeedKmh = 200f;           // �ִ� �ӵ�
    [SerializeField] private float movementForce = 20f;       // �̵��� ������ ��
    [SerializeField] private float steerAngle = 200f;         // ����(ȸ��) ����
    [SerializeField] private float decelerationRate = 2f;  // Ű ���� ���� ���� ���
    [SerializeField] private float minSteerMultiplier = 1f;  // �ּ� ���� ��� (�ӵ��� ���� ��)
    [SerializeField] private float maxSteerMultiplier = 2f;  // �ִ� ���� ��� (�ӵ��� �ִ��� ��)

    [Header("�帮��Ʈ ����")]
    [SerializeField] private float minDriftAngle = 30f;       // �ּ� �帮��Ʈ ����
    [SerializeField] public float maxDriftAngle = 180f;       // �ִ� �帮��Ʈ ����
    [SerializeField] private float minDriftDuration = 0.2f;     // �ּ� �帮��Ʈ ���ӽð�
    [SerializeField] private float maxDriftDuration = 2f;       // �ִ� �帮��Ʈ ���ӽð�
    [SerializeField] private float minDriftForceMultiplier = 1f;// �ּ� �帮��Ʈ �� ���
    [SerializeField] private float maxDriftForceMultiplier = 5f;// �ִ� �帮��Ʈ �� ���
    [SerializeField] private float driftSpeedReduction = 0.7f;  // �帮��Ʈ �� �ӵ� ���� ����

    [Header("�ν�Ʈ ����")]
    [SerializeField] public float boostDuration = 1.2f;         // �ν�Ʈ ���ӽð�
    [SerializeField] public int maxBoostGauge = 100;           // �ִ� �ν�Ʈ ������
    [SerializeField] private float boostChargeRate = 5f;        // �⺻ �ν�Ʈ ���� �ӵ�
    [SerializeField] private float driftBoostChargeRate = 10f;   // �帮��Ʈ �� �ν�Ʈ ���� �ӵ�
    [SerializeField] private float boostMaxSpeedKmh = 280f;        // �ν�Ʈ ������ �ִ� �ӵ�
                     private float boostSpeed;         // �ν�Ʈ Ȱ��ȭ �� �ӵ�

    [Header("��Ÿ ����")]
    [SerializeField] private float rotationCorrectionSpeed = 5f;// ȸ�� ���� �ӵ�
    [SerializeField] private float downwardForce = 50f;         // �ϰ�(�߷� ����) ��
    [SerializeField] private float wallBounceFactor = 2f;  // �� �浹 �� ������ ƨ�� ��(���� ����)

    #endregion

    #region Private Fields

    private CHMTestWheelController wheelCtrl;  // ���� ���� ��ũ��Ʈ
    private Rigidbody rigid;                     // ������ٵ� (���� ó��)
    public float speedKM { get; private set; }     // ���� �ӷ� (km/h ����)
    public bool isBoostTriggered { get; private set; } // �ν�Ʈ Ȱ��ȭ ����
    private float driftDuration;  // �帮��Ʈ ���� �����ð�
    public bool isBoostCreate { get; set; }            // �帮��Ʈ ������ ���� ���� ����

    private Coroutine postDriftBoostCoroutine; // �帮��Ʈ ���� �� �ν�Ʈ ó���� ���� �ڷ�ƾ ����
    private float initialDriftSpeed;           // �帮��Ʈ ���� �� ��ϵ� �ʱ� �ӵ�
    public bool isDrifting = false;            // �帮��Ʈ ���� �� ����
    public float currentDriftAngle = 0f;       // ���� ���� �帮��Ʈ ����
    private float currentDriftThreshold;       // �ӵ��� ���� �帮��Ʈ �Է� ���� ��
    private float driftForceMultiplier;        // �������� ���� �帮��Ʈ �� ���
    public float boostGauge { get; private set; }                // ���� �ν�Ʈ ������
    private float lockedYRotation = 0f;        // �帮��Ʈ �� �����Ǵ� Y ȸ����
    private float currentMotorInput;
    private float currentSteerInput;
    // ���������� ����� m/s ���� ����
    private float maxSpeed;      // �ִ� �ӵ� (m/s)
    private float boostMaxSpeed; // �ν�Ʈ �ִ� �ӵ� (m/s)



    private Vector3 speed;                     // ���� �ӵ� ����
    private float chargeAmount;

    #endregion

    #region
    /* Network Instantiate */
    private Transform _playerParent;
    private Transform _tr;
    private PhotonView _photonView;    
    #endregion
    
    #region Unity Methods

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<CHMTestWheelController>(); // ���� ��Ʈ�ѷ� ����
        rigid = GetComponent<Rigidbody>();                         // ������ٵ� ����
        
        /* TODO : ���� ���϶� �������ֱ� */
        _tr = gameObject.transform;
        _photonView = GetComponent<PhotonView>();
        _playerParent = GameObject.Find("Players").transform;
        transform.parent = _playerParent;                
    }
    private void FixedUpdate()
    {
        if (!_photonView.IsMine)
        {
            return;
        }        
        
        maxSpeed = maxSpeedKmh / 3.6f;
        boostMaxSpeed = boostMaxSpeedKmh / 3.6f;
        float currentMaxSpeed = isBoostTriggered ? boostMaxSpeed : maxSpeed;
        HandleKartMovement(currentMotorInput, currentSteerInput); // �Է��� Update()���� ���� �� ���
                                                                  // �׸��� �������� �׻� �ӵ� Ŭ������ �����մϴ�.
        ClampHorizontalSpeed(currentMaxSpeed);
        // ���� �̵� ó�� �� ���� ȸ�� ����
        CorrectAirborneRotation();
    }
    

    private void Update()
    {
        if (!_photonView.IsMine)
        {
            return;
        }        
        
        // �Է°� �о����
        currentSteerInput = Input.GetAxis("Horizontal");
        currentMotorInput = Input.GetAxis("Vertical");

        // ���� �ӵ� ���� (Y�� ����) �� km/h ��ȯ
        speed = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        speedKM = speed.magnitude * 3.6f;
        
        // �帮��Ʈ ���� �Ķ���� ������Ʈ
        UpdateDriftParameters();

        // �帮��Ʈ �Է� ó��
        HandleDriftInput(currentSteerInput);

        // �ν�Ʈ �Է� ó��
        HandleBoostInput();

        // �帮��Ʈ �� ���ӽð� ���� �� ���� üũ
        //if (isDrifting)
        //{
        //    driftDuration += Time.deltaTime;
        //    if (driftDuration >= 1f)
        //    {
        //        isBoostCreate = true;
        //        driftDuration = 0f;
        //    }
        //}

        // �ν�Ʈ ������ ����
        if(currentMotorInput != 0 || isDrifting)
        {
            ChargeBoostGauge();
        }

        if(boostGauge >= maxBoostGauge)
        {
            isBoostCreate = true;
            boostGauge = 0;
            chargeAmount = 0;
        }

        // īƮ �̵� ó�� (�̵�/ȸ��)
        //HandleKartMovement(currentMotorInput, currentSteerInput);
    }
    #endregion

    #region Input Handling

    private void HandleDriftInput(float steerInput)
    {
        // LeftShift Ű�� ���� �Է��� ���� �� �帮��Ʈ ����
        if (Input.GetKeyDown(KeyCode.LeftShift) && Mathf.Abs(steerInput) > 0)
        {
            StartDrift(steerInput * currentDriftThreshold);
        }

        // �帮��Ʈ �� �߰� �Է����� �帮��Ʈ ���� ������Ʈ
        if (isDrifting && Input.GetKey(KeyCode.LeftShift))
        {
            UpdateDriftAngle();
        }
    }

    private void HandleBoostInput()
    {
        // LeftControl Ű�� �ν�Ʈ ������ �ִ�ġ �� �⺻ �ν�Ʈ Ȱ��ȭ
        if (Input.GetKeyDown(KeyCode.LeftControl) && boostGauge >= maxBoostGauge)
        {
            StartBoost(boostDuration);
        }
    }

    #endregion

    #region Drift and Boost Methods

    private void UpdateDriftParameters()
    {
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed * 2f);

        // �ӵ��� ���� �帮��Ʈ �Է� �ΰ����� �� ��� ������Ʈ
        currentDriftThreshold = Mathf.Lerp(minDriftAngle, maxDriftAngle, speedFactor);
        driftForceMultiplier = Mathf.Lerp(minDriftForceMultiplier, maxDriftForceMultiplier, speedFactor);
    }

    private void StartDrift(float driftInputAngle)
    {
        isDrifting = true;
        currentDriftAngle = driftInputAngle;
        lockedYRotation = transform.eulerAngles.y;  // �帮��Ʈ ���� �� ���� ȸ���� ����

        // �ӵ��� ���� �Է¿� ���� �帮��Ʈ ���ӽð� ����
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float steerFactor = Mathf.Abs(driftInputAngle / currentDriftThreshold);
        float influenceFactor = (speedFactor + steerFactor) / 2f;
        driftDuration = Mathf.Lerp(minDriftDuration, maxDriftDuration, influenceFactor);

        Debug.Log($"�帮��Ʈ ����: �Է°�={driftInputAngle}, �ӵ�={currentSpeed}, �����={steerFactor}, ���ӽð�={driftDuration:F2}��");

        // ������ �ð� �� �帮��Ʈ ���� ����
        Invoke(nameof(EndDrift), driftDuration);
    }

    private void UpdateDriftAngle()
    {
        currentDriftAngle += Time.deltaTime * 10f;
        currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
    }

    private void EndDrift()
    {
        isDrifting = false;
        currentDriftAngle = 0f;
        Debug.Log("�帮��Ʈ ����");

        // �帮��Ʈ ���� �� 0.5�� ���� ��� �ν�Ʈ �Է� ���
        if (postDriftBoostCoroutine != null)
        {
            StopCoroutine(postDriftBoostCoroutine);
        }
        postDriftBoostCoroutine = StartCoroutine(PostDriftBoostCoroutine());

        initialDriftSpeed = 0f;
    }

    private IEnumerator PostDriftBoostCoroutine()
    {
        float timer = 0f;
        bool boosted = false;
        Debug.Log("��� �ν�Ʈ �Է� ��� ��...");

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
        {
            Debug.Log("��� �ν�Ʈ �Է� �ð� �ʰ�");
        }

        postDriftBoostCoroutine = null;
    }

    private void PerformInstantBoost()
    {
        Debug.Log("��� �ν�Ʈ Ȱ��ȭ!");
        rigid.velocity *= 1.2f;  // ���� �ӷ��� 1.2�� ����
        postDriftBoostCoroutine = null;
    }

    private void StartBoost(float duration)
    {
        boostSpeed = speedKM;
        isBoostTriggered = true;
        boostGauge = 0;
        rigid.velocity = transform.forward * boostSpeed;
        Debug.Log("�ν�Ʈ Ȱ��ȭ");
        Invoke(nameof(EndBoost), duration);
    }

    private void EndBoost()
    {
        isBoostTriggered = false;
        Debug.Log("�ν�Ʈ ����");
    }

    private void ChargeBoostGauge()
    {
        chargeAmount += isDrifting
            ? driftBoostChargeRate * Time.fixedDeltaTime  // �帮��Ʈ �� ���� �ӵ�
            : boostChargeRate * Time.fixedDeltaTime;       // �Ϲ� ���� �ӵ�

        boostGauge = Mathf.Clamp(chargeAmount, 0, maxBoostGauge);
        Debug.Log("boostGage : " + boostGauge);

        if (boostGauge >= maxBoostGauge)
        {
            Debug.Log("�ν�Ʈ ������ �ִ�ġ ����!");
        }
    }

    #endregion
    #region Movement Handling

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        // �ν��� ���� ���ο� ���� �ִ� �ӵ�(m/s)�� �����մϴ�.
        float currentMaxSpeed = isBoostTriggered ? boostMaxSpeed : maxSpeed;

        // ���� ���� �ӵ��� �����ϰ�, �ִ�ӵ� ��� ����(0~1)�� ����մϴ�.
        Vector3 currentHorizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        float currentHorizontalSpeed = currentHorizontalVelocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentHorizontalSpeed / currentMaxSpeed);

        // �ӵ��� ���� ���� �ΰ����� ���� �����մϴ�.
        float steeringMultiplier = Mathf.Lerp(minSteerMultiplier, maxSteerMultiplier, speedFactor);

        // �帮��Ʈ ������ �ƴ����� ���� ���� ó��
        if (isDrifting)
        {
            ProcessDrift(steerInput, steeringMultiplier);
        }
        else
        {
            initialDriftSpeed = 0f;
            ProcessAcceleration(motorInput, currentMaxSpeed);
        }

        // ���������� ���� �ӵ��� currentMaxSpeed ���Ϸ� �����մϴ�.
        ClampHorizontalSpeed(currentMaxSpeed);

        // ���� �ΰ����� ������ ȸ�� ó��
        RotateKart(steerInput, steeringMultiplier);

        // ���� ������Ʈ (�ʿ� ��)
        if (wheelCtrl != null)
        {
            wheelCtrl.UpdateAndRotateWheels(steerInput, motorInput, rigid.velocity.magnitude, isDrifting);
        }
    }

    /// <summary>
    /// �帮��Ʈ ������ �� �ӵ� ó�� �� ���� �� ����
    /// </summary>
    private void ProcessDrift(float steerInput, float steeringMultiplier)
    {
        // �帮��Ʈ ���� �� ���� �ӵ��� ����մϴ�.
        if (Mathf.Approximately(initialDriftSpeed, 0f))
        {
            initialDriftSpeed = rigid.velocity.magnitude;
        }
        float driftSpeed = initialDriftSpeed * driftSpeedReduction;

        // ���� �Է¿� �ΰ����� �����Ͽ� ���� ȸ���� �����մϴ�.
        lockedYRotation += steerInput * ((steerAngle / 2.5f) * steeringMultiplier) * Time.fixedDeltaTime;
        Quaternion driftRotation = Quaternion.Euler(0f, lockedYRotation, 0f);
        Vector3 driftDirection = driftRotation * Vector3.forward;

        // ���� �ӵ��� �ε巴�� �帮��Ʈ �������� ��ȯ�մϴ�.
        rigid.velocity = Vector3.Lerp(rigid.velocity, driftDirection * driftSpeed, Time.fixedDeltaTime * 5f);

        // ���� ���� �߰��Ͽ� �帮��Ʈ ������ ��ȭ�մϴ�.
        Vector3 lateralForce = transform.right * steerInput * driftForceMultiplier * movementForce;
        rigid.AddForce(lateralForce, ForceMode.Force);
    }

    /// <summary>
    /// ����/���� ���� ó�� (�帮��Ʈ�� �ƴ� ��)
    /// </summary>
    private void ProcessAcceleration(float motorInput, float currentMaxSpeed)
    {
        Vector3 acceleration = transform.forward * movementForce * motorInput * Time.fixedDeltaTime;
        Vector3 currentHorizVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        Vector3 newHorizVelocity = currentHorizVelocity + acceleration;

        // ������ �ӵ��� �ִ� �ӵ��� Ŭ�����մϴ�.
        newHorizVelocity = Vector3.ClampMagnitude(newHorizVelocity, currentMaxSpeed);

        // �Է��� �ִ� ���, �ӵ� ���͸� ��� transform.forward �������� �����մϴ�.
        if (Mathf.Abs(motorInput) > 0.1f)
        {
            newHorizVelocity = transform.forward * newHorizVelocity.magnitude;
        }

        // ���� �ӵ�(Y)�� �״�� �ΰ�, XZ ���и� �����մϴ�.
        rigid.velocity = new Vector3(newHorizVelocity.x, rigid.velocity.y, newHorizVelocity.z);

        // ������ ȸ���� ���� (�ʿ� ��)
        lockedYRotation = transform.eulerAngles.y;
    }

    /// <summary>
    /// ���� �Է°� �ӵ� ��� �ΰ����� ������ ȸ�� ó���� ����մϴ�.
    /// </summary>
    private void RotateKart(float steerInput, float steeringMultiplier)
    {
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steerAngle * steeringMultiplier * Time.fixedDeltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));
    }

    /// <summary>
    /// XZ ������ ���� �ӵ��� �ִ밪 ���Ϸ� �����մϴ�.
    /// </summary>
    private void ClampHorizontalSpeed(float maxHorizontalSpeed)
    {
        Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        if (horizontalVelocity.magnitude > maxHorizontalSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxHorizontalSpeed;
            rigid.velocity = new Vector3(horizontalVelocity.x, rigid.velocity.y, horizontalVelocity.z);
        }
    }

    #endregion


    #region ��Ƽ�� & �浹 ó��
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

    public bool IsGrounded()
    {
        // ������ Raycast�� ����Ͽ� kart�� �ϴ��� ���鿡 ��Ҵ��� Ȯ���մϴ�.
        float rayDistance = 0.8f; // �ʿ信 ���� ���� ����
        return Physics.Raycast(transform.position, Vector3.down, rayDistance);
    }
    private void OnCollisionEnter(Collision collision)
    {
        // �浹�� ������Ʈ�� "Wall" �±��� ���
        if (collision.gameObject.CompareTag("Wall"))
        {
            // �帮��Ʈ ���̸� �帮��Ʈ ��� (��: EndDrift() ȣ��)
            if (isDrifting)
            {
                EndDrift();
            }

            // ���� �ӵ��� ������
            Vector3 currentVelocity = rigid.velocity;

            // �浹 ������ ù ��° ���� ���͸� ������
            Vector3 normal = collision.contacts[0].normal;

            // Vector3.Reflect()�� ����Ͽ� ���� ���� ���⿡ ���� �ݻ�� �ӵ��� ���
            Vector3 reflectedVelocity = Vector3.Reflect(currentVelocity, normal);

            // ƨ�� �� ���� ȿ���� �����ϱ� ���� ����� ����
            rigid.velocity = reflectedVelocity * wallBounceFactor;

            Debug.Log("�� �浹: �ݻ�� ���ν�Ƽ �����");
        }
    }
    #endregion
}