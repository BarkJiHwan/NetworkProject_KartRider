using System.Collections;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class TestCHMKart : MonoBehaviour
{
    #region Serialized Fields

    [Header("īƮ ���� ���")]
    [SerializeField] private GameObject wheels;   // ���� ������Ʈ

    [Header("�̵� ����")]
    [SerializeField] private float maxSpeedKmh = 200f;           // �ִ� �ӵ�
    [SerializeField] private float movementForce = 20f;       // �̵��� ������ ��
    [SerializeField] private float steerAngle = 100f;         // ����(ȸ��) ����
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
    [SerializeField] public float boostDuration = 2f;         // �⺻ �ν�Ʈ ���ӽð�
    [SerializeField] public float momentboostDuration = 1.2f;         // �⺻ �ν�Ʈ ���ӽð�
    [SerializeField] public int maxBoostGauge = 100;           // �ִ� �ν�Ʈ ������
    [SerializeField] private float boostChargeRate = 5f;        // �⺻ �ν�Ʈ ���� �ӵ�
    [SerializeField] private float driftBoostChargeRate = 10f;   // �帮��Ʈ �� �ν�Ʈ ���� �ӵ�
    [SerializeField] private float boostMaxSpeedKmh = 280f;        // �ν�Ʈ ������ �ִ� �ӵ�
                     private float boostSpeed;         // �ν�Ʈ Ȱ��ȭ �� �ӵ�
    [Header("��� ���� ���� ����")]
    [SerializeField] private float uphillAngleThreshold = 10f;     // ��� �Ǻ� ���� (��: 10�� �̻��̸� ������� ����)
    [SerializeField] private float uphillForceMultiplier = 1.5f;     // ������� �߰� ������ ���� ���  
                                                                   

    #endregion

    #region Private Fields

    public float speedKM { get; private set; }     // ���� �ӷ� (km/h ����)
    public bool isBoostTriggered { get; private set; } // �ν�Ʈ Ȱ��ȭ ����
    public bool isBoostCreate { get; set; }    // �帮��Ʈ ������ ���� ���� ����
    public float boostGauge { get; private set; }                // ���� �ν�Ʈ ������

    private float driftDuration = 4f;  // �ν��� ���� �ð� (��: �� 4��, 2�� ����, 2�� ����)
    private CHMTestWheelController wheelCtrl;  // ���� ���� ��ũ��Ʈ
    private Rigidbody rigid;                   // ������ٵ� (���� ó��)
    private Coroutine postDriftBoostCoroutine; // �帮��Ʈ ���� �� �ν�Ʈ ó���� ���� �ڷ�ƾ ����
    private float initialDriftSpeed;           // �帮��Ʈ ���� �� ��ϵ� �ʱ� �ӵ�
    public bool isDrifting = false;            // �帮��Ʈ ���� �� ����
    public float currentDriftAngle = 0f;       // ���� ���� �帮��Ʈ ����
    private float currentDriftThreshold;       // �ӵ��� ���� �帮��Ʈ �Է� ���� ��
    private float driftForceMultiplier;        // �������� ���� �帮��Ʈ �� ���
    private float lockedYRotation = 0f;        // �帮��Ʈ �� �����Ǵ� Y ȸ����
    private float currentMotorInput;
    private float currentSteerInput;
    private int boostCount;
    // ���������� ����� m/s ���� ����
    private float maxSpeed;      // �ִ� �ӵ� (m/s)
    private float boostMaxSpeed; // �ν�Ʈ �ִ� �ӵ� (m/s)
    private Vector3 speed;       // ���� �ӵ� ����
    private float chargeAmount;
    private float currentMaxSpeed;//�ν�Ʈ,�⺻ �ִ�ӵ� �Ǵ��ؼ� ���
    public AnimationCurve boostCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);  // 0�ʿ� 0, 1�ʿ� 1 ���� ���� �ε巯�� Ŀ��


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
        currentMaxSpeed = isBoostTriggered ? boostMaxSpeed : maxSpeed;
        HandleKartMovement(currentMotorInput, currentSteerInput); // �Է��� Update()���� ���� �� ���
                                                                  // �׸��� �������� �׻� �ӵ� Ŭ������ �����մϴ�.
        ClampHorizontalSpeed(currentMaxSpeed);
        // ���� �̵� ó�� �� ���� ȸ�� ����
        //HandleAntiRoll();
        //HandleBoxCastContacts();
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
        Debug.Log(speed.magnitude);
        
        // �帮��Ʈ ���� �Ķ���� ������Ʈ
        UpdateDriftParameters();

        // �帮��Ʈ �Է� ó��
        HandleDriftInput(currentSteerInput);

        // �ν�Ʈ �Է� ó��
        HandleBoostInput();
      
        // �ν�Ʈ ������ ����
        if(currentMotorInput != 0 || isDrifting)
        {
            ChargeBoostGauge();
        }

        if (boostGauge >= maxBoostGauge)
        {
            isBoostCreate = true;
            boostGauge = 0;
            chargeAmount = 0;
            if (boostCount < 2)
            {
                boostCount++;
            }
        }
    }
    #endregion

    #region [Ű�Է� �Լ� ]

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
        // LeftControl Ű�� �ν�Ʈ ������ �ִ�ġ �� �ν��� �⺻ �ߵ�
        if (Input.GetKeyDown(KeyCode.LeftControl) &&boostCount > 0)
        {
            StartBoost(boostDuration);
            boostCount--;
        }
    }

    #endregion

    #region [�帮��Ʈ ���� �Լ�]

    // �帮��Ʈ ���� �Ķ���� ������Ʈ
    private void UpdateDriftParameters()
    {
        float currentSpeed = rigid.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed * 2f);

        // �ӵ��� ���� �帮��Ʈ �Է� �ΰ����� �� ��� ������Ʈ
        currentDriftThreshold = Mathf.Lerp(minDriftAngle, maxDriftAngle, speedFactor);
        driftForceMultiplier = Mathf.Lerp(minDriftForceMultiplier, maxDriftForceMultiplier, speedFactor);
    }

    // �帮��Ʈ ���� ó��
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

    // �帮��Ʈ �� �Է¿� ���� ���� ������Ʈ
    private void UpdateDriftAngle()
    {
        currentDriftAngle += Time.deltaTime * 10f;
        currentDriftAngle = Mathf.Clamp(currentDriftAngle, -maxDriftAngle, maxDriftAngle);
    }

    // �帮��Ʈ ���� ó�� �� ��� �ν�Ʈ �Է� ���
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
    #endregion

    #region [�ν��� ���� �Լ�]

    // �帮��Ʈ ���� �� �ٷ� �ν�Ʈ �Է��� �ޱ� ���� �ڷ�ƾ
    private IEnumerator PostDriftBoostCoroutine()
    {
        float timer = 0f;
        bool boosted = false;
        Debug.Log("���� �ν�Ʈ �Է� ��� ��...");

        while (timer < 0.5f)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                PerformInstantBoost(momentboostDuration);                
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

    // �帮��Ʈ �� ��� �ν�Ʈ (���� �ӷ��� 1.2�� ����)
    private void PerformInstantBoost(float momentboostDuration)
    {
        Debug.Log("��� �ν�Ʈ Ȱ��ȭ!");
        rigid.velocity *= 1.2f;
        postDriftBoostCoroutine = null;
    }
    // �ν�Ʈ ���� ó��
    private void EndBoost()
    {
        isBoostTriggered = false;
        Debug.Log("�ν�Ʈ ����");
    }
    // �ν�Ʈ ������ ���� ó�� (�帮��Ʈ �ÿ� �Ϲ� ���� �ӵ� ����)
    private void ChargeBoostGauge()
    {
        chargeAmount += isDrifting
            ? driftBoostChargeRate * Time.fixedDeltaTime  // �帮��Ʈ �� ���� �ӵ�
            : boostChargeRate * Time.fixedDeltaTime;       // �Ϲ� ���� �ӵ�

        boostGauge = Mathf.Clamp(chargeAmount, 0, maxBoostGauge);            
        if (boostGauge >= maxBoostGauge)
        {
            Debug.Log("�ν�Ʈ ������ �ִ�ġ ����!");
        }
    }    
    /// <summary>
    /// �ν�Ʈ�� �����ϴ� �Լ�. �ڷ�ƾ�� ȣ���� ���� �ӵ����� �ִ� �ν�Ʈ �ӵ����� ������ ������ �����մϴ�.
    /// </summary>
    /// <param name="duration">�ν�Ʈ ���� �ð�(��)</param>
    private void StartBoost(float duration)
    {
        if (isBoostTriggered) return;  // �̹� �ν�Ʈ ���̸� ����

        // �ڷ�ƾ ����: BoostCoroutine�� �� ������ �ӵ��� ������Ʈ�մϴ�.
        StartCoroutine(BoostCoroutine(duration));
    }

    /// <summary>
    /// BoostCoroutine �ڷ�ƾ�� boostDuration ���� linearly (���� ����) ���� �ӵ����� boostMaxSpeed�� �����ϵ��� ������ �����մϴ�.
    /// </summary>
    /// <param name="duration">�ν�Ʈ ���� �ð�(��)</param>
    /// <returns></returns>
    // Inspector���� ���� ������ AnimationCurve��
    public AnimationCurve boostAccelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve boostDecelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    
   

    private IEnumerator BoostCoroutine(float duration)
    {
        isBoostTriggered = true;

        // ��ü �ν��� �ð��� ����/���� �Ⱓ���� ����
        float accelDuration = duration * 0.5f;
        float decelDuration = duration * 0.5f;

        // �ν��� ���� ���� �ӵ� (���� m/s ����)
        float startSpeed = rigid.velocity.magnitude;

        float timer = 0f;

        // [Phase 1] Acceleration: startSpeed -> boostMaxSpeed
        while (timer < accelDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / accelDuration);
            // acceleration curve�� ����� ���� ���� ����
            float curveValue = boostAccelerationCurve.Evaluate(t);
            float newSpeed = Mathf.Lerp(startSpeed, boostMaxSpeed, curveValue);

            // forward �������� �� �ӵ� ����
            rigid.velocity = transform.forward * newSpeed;
            yield return null;
        }

        // ���� ���� �� Ȯ���� boostMaxSpeed�� ����
        rigid.velocity = transform.forward * boostMaxSpeed;

        // [Phase 2] Deceleration: boostMaxSpeed -> maxSpeed (Ȥ�� �ٸ� �⺻ �ӵ�)
        timer = 0f;
        while (timer < decelDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / decelDuration);
            // deceleration curve�� ����� ���� � ����
            float curveValue = boostDecelerationCurve.Evaluate(t);
            float newSpeed = Mathf.Lerp(boostMaxSpeed, maxSpeed, curveValue);

            rigid.velocity = transform.forward * newSpeed;
            yield return null;
        }

        // ������ �������� �ν��� ���� �÷��� ����
        isBoostTriggered = false;
        Debug.Log("�ν�Ʈ ����");
    }

    #endregion

    #region [����,����,���� �� ���]

    private void HandleKartMovement(float motorInput, float steerInput)
    {
        // ���� ���� �ӵ��� �����ϰ�, �ִ�ӵ� ��� ����(0~1)�� ����մϴ�.
        Vector3 currentHorizontalVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        float currentHorizontalSpeed = currentHorizontalVelocity.magnitude;       
        float speedFactor = Mathf.Pow(Mathf.Clamp01(currentHorizontalSpeed / currentMaxSpeed), 0.8f);
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
        // �⺻ ���� ó��: ����/������ ���� �� ���
        Vector3 acceleration = transform.forward * movementForce * motorInput * Time.fixedDeltaTime;

        //// �߰�: ��� ���� �� ���� (���鿡 �پ� ���� ����)
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            // ������ ���⸦ ��� (��ְ� ���� ���� ������ ����)
            float groundAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (groundAngle > uphillAngleThreshold && motorInput > 0)
            {
                // ������� ������ �� ���� �� ����
                acceleration *= uphillForceMultiplier;
            }
        }

        // ���� ���� �ӵ� ���� �� �����ջ�
        Vector3 currentHorizVelocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        Vector3 newHorizVelocity = currentHorizVelocity + acceleration;

        // ���� �ӵ��� �ִ� �ӵ��� Ŭ����
        newHorizVelocity = Vector3.ClampMagnitude(newHorizVelocity, currentMaxSpeed);

        // ���� ����: motorInput�� ������ �� �������� ����
        if (Mathf.Abs(motorInput) > 0.1f)
        {
            newHorizVelocity = (motorInput > 0 ? transform.forward : -transform.forward) * newHorizVelocity.magnitude;
        }

        // Y�� �ӵ��� ������� ����
        rigid.velocity = new Vector3(newHorizVelocity.x, rigid.velocity.y, newHorizVelocity.z);

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
            rigid.velocity = new Vector3(horizontalVelocity.x, rigid.velocity.y -10f, horizontalVelocity.z);
        }
    }
    #endregion

    //#region [ ����, �߷�, ��Ƽ��, �浹 ó��, ������Ʈ�� ]

    //// [�ν����� ���� ������]
    //[Header("���̾� ����")]
    //[SerializeField] private LayerMask groundLayer;    // ���� ���̾�
    //[SerializeField] private LayerMask boosterLayer;   // �ν���(������) ���̾�
    //[SerializeField] private LayerMask jumpLayer;      // ���� ���� ���̾�
    //[SerializeField] private LayerMask wallLayer;      // �� ���� ���̾�

    //[Header("����ĳ��Ʈ �� �ڽ�ĳ��Ʈ ����")]
    //[SerializeField] private float groundRayDistance = 0.8f;
    //[SerializeField] private Vector3 boxCastCenter = Vector3.zero;     // īƮ �߽ɿ����� ������
    //[SerializeField] private Vector3 boxCastSize = new Vector3(1, 1, 1);  // �ڽ� ũ��
    //[SerializeField] private float boxCastDistance = 1f;                // �ڽ� ĳ��Ʈ �˻� �Ÿ�

    //[Header("ȸ��/�߷�/��Ƽ�� ����")]
    //[SerializeField] private float rotationCorrectionSpeed = 2f;  // ȸ�� ���� �ӵ�
    //[SerializeField] private float downwardForce = 10f;           // �ϰ��� (�Ʒ��� ��)
    //[SerializeField] private float wallBounceFactor = 0.7f;         // �� �浹 �� �ӵ� ���� ���

    //[Header("���� ������Ʈ�� ����")]
    //[SerializeField] private float airGravityMultiplier = 0.7f;    // ���� �߷� ���
    //[SerializeField] private float airControlTorque = 5f;          // ���� ������Ʈ�ѿ� ȸ�� ��ũ

    //// ���� ����
    //private RaycastHit _boxCastHit;  // �ڽ� ĳ��Ʈ ��� hit ����
    //                                 // isBoostTriggered, isDrifting, rigid, EndDrift() ���� �̹� ���ǵǾ� �ִٰ� ����

    ///// <summary>
    ///// �ڽ�ĳ��Ʈ ����� ������� �ν���, ��, ���� �� �� ���̾��� ������ ó���մϴ�.
    ///// </summary>
    //private void HandleBoxCastContacts()
    //{
    //    // �ν��� ���̾� üũ
    //    if (CheckBoxContact(boosterLayer))
    //    {
    //        if (!isBoostTriggered)
    //        {
    //            ActivateBooster(1.5f);
    //        }
    //    }

    //    // ��(Wall) ���̾� üũ: _boxCastHit�� ����� ���� �̿��� �� ���� ó�� ����
    //    if (CheckBoxContact(wallLayer))
    //    {
    //        OnWallContact();
    //    }

    //    // ����(Jump) ���̾� üũ: ���� �� ���� ����(������Ʈ��, �߷� ����)
    //    if (CheckBoxContact(jumpLayer))
    //    {
    //        ApplyCustomGravity();
    //        AirControl();
    //        Debug.Log("���� ���̾� BoxCast ����: ������Ʈ�� �� �߷� ȣ��");
    //    }
    //}

    ///// <summary>
    ///// ������ �� īƮ ȸ�� ���� �� �Ʒ��� �� �߰� (��Ƽ�� ȿ��)
    ///// </summary>
    //private void CorrectAirborneRotation()
    //{
    //    // ���鿡 ������� ������ ȸ�� ���� �� �ϰ��� ����
    //    if (!IsGrounded())
    //    {
    //        Vector3 currentEuler = transform.eulerAngles;
    //        float correctedX = Mathf.LerpAngle(currentEuler.x, 0f, Time.deltaTime * rotationCorrectionSpeed);
    //        float correctedZ = Mathf.LerpAngle(currentEuler.z, 0f, Time.deltaTime * rotationCorrectionSpeed);
    //        transform.rotation = Quaternion.Euler(correctedX, currentEuler.y, correctedZ);

    //        rigid.AddForce(Vector3.down * downwardForce, ForceMode.VelocityChange);
    //    }
    //}

    ///// <summary>
    ///// ����ĳ��Ʈ�� ���� ��Ƽ�� ����� �����մϴ�.
    ///// - ����(Jump) ���̾� ���� �� ��Ƽ�� ����� �Ͻ� �����ϰ�,
    ///// - ����(Ground) ���� �� CorrectAirborneRotation()�� �����մϴ�.
    ///// </summary>
    //private void HandleAntiRoll()
    //{
    //    // �˻��� ���̾�: Ground�� Jump ���̾�
    //    int mask = groundLayer.value | jumpLayer.value;
    //    RaycastHit hit;
    //    if (Physics.Raycast(transform.position, Vector3.down, out hit, groundRayDistance, mask))
    //    {
    //        int hitLayer = hit.collider.gameObject.layer;
    //        if (hitLayer == LayerMask.NameToLayer("Jump"))
    //        {
    //            Debug.Log("���� ���̾� ������: ��Ƽ�� ��� �Ͻ� ����");
    //            return;
    //        }
    //        else if (hitLayer == LayerMask.NameToLayer("Ground"))
    //        {
    //            Debug.Log("���� ������: ��Ƽ�� ��� ����");
    //            CorrectAirborneRotation();
    //        }
    //    }
    //}

    ///// <summary>
    ///// �浹 �Ǻ�: Wall, Jump ���̾� ���� �� ������ ���� ����
    ///// </summary>
    //private void OnCollisionEnter(Collision collision)
    //{
    //    int colLayer = collision.gameObject.layer;

    //    // Wall ���̾� ���� ��: �� �ݻ� ���� ����
    //    if (((1 << colLayer) & wallLayer.value) != 0)
    //    {
    //        if (isDrifting)  // (�帮��Ʈ ���̸� �帮��Ʈ ���)
    //        {
    //            EndDrift();
    //        }
    //        Vector3 currentVelocity = rigid.velocity;
    //        Vector3 normal = collision.contacts[0].normal;
    //        Vector3 reflectedVelocity = Vector3.Reflect(currentVelocity, normal);
    //        rigid.velocity = reflectedVelocity * wallBounceFactor;
    //        Debug.Log("�� ���̾� ����: �ݻ�� �ӵ� �����");
    //    }

    //    // Jump ���̾� ���� ��: ���� ���� (������Ʈ�� �� ����� ���� �߷�)
    //    //if (((1 << colLayer) & jumpLayer.value) != 0)
    //    //{
    //    //    //ApplyCustomGravity();
    //    //    AirControl();
    //    //    Debug.Log("���� ���̾� ����: ������Ʈ�� �� �߷� �Լ� ȣ���");
    //    //}
    //}

    ///// <summary>
    ///// ������ �� ����� ���� �߷��� �����մϴ�.
    ///// </summary>
    //private void ApplyCustomGravity()
    //{
    //    if (!IsGrounded())
    //    {
    //        Vector3 customGravity = Physics.gravity * airGravityMultiplier;
    //        rigid.AddForce(customGravity, ForceMode.Acceleration);
    //        Debug.Log("����� ���� �߷� �����: " + customGravity);
    //    }
    //}

    ///// <summary>
    ///// ���߿��� ������Ʈ���� �����Ͽ� ȸ�� ��ũ�� �ο��մϴ�.
    ///// </summary>
    //private void AirControl()
    //{
    //    float airSteer = Input.GetAxis("Horizontal");
    //    Vector3 airTorque = new Vector3(0f, airSteer * airControlTorque, 0f);
    //    rigid.AddTorque(airTorque, ForceMode.Acceleration);
    //    Debug.Log("���� ������Ʈ�� �����: ��ũ " + airTorque);
    //}

    ///// <summary>
    ///// �ڽ� ĳ��Ʈ ���(_boxCastHit)�� �̿��Ͽ� ��(Wall) ���̾���� ���� �� ó���� ������ �����մϴ�.
    ///// </summary>
    //private void OnWallContact()
    //{
    //    // �帮��Ʈ ���̸� �帮��Ʈ ���� ó��
    //    if (isDrifting)
    //    {
    //        EndDrift();
    //    }

    //    // ���� �ӵ��� �ݻ��Ͽ� �� �浹 ȿ�� ����
    //    Vector3 currentVelocity = rigid.velocity;
    //    Vector3 normal = _boxCastHit.normal;  // CheckBoxContact()���� ���� hit ����
    //    Vector3 reflectedVelocity = Vector3.Reflect(currentVelocity, normal);

    //    // �ݻ�� �ӵ��� wallBounceFactor ����
    //    rigid.velocity = reflectedVelocity * wallBounceFactor;
    //    Debug.Log("�ڽ� ĳ��Ʈ: �� ���� - �ݻ�� �ӵ� �����");
    //}

    ///// <summary>
    ///// ����, �ν���, Jump ���̾� ���� �����Ͽ� �ٴ� ���� ���θ� �Ǻ��մϴ�.
    ///// Booster ���̾� ���� �� ActivateBooster�� ȣ���մϴ�.
    ///// </summary>
    //public bool IsGrounded()
    //{
    //    int layerMask = groundLayer.value | boosterLayer.value | jumpLayer.value;
    //    RaycastHit hit;
    //    if (Physics.Raycast(transform.position, Vector3.down, out hit, groundRayDistance, layerMask))
    //    {
    //        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Booster"))
    //        {
    //            ActivateBooster(1.5f);
    //        }
    //        return true;
    //    }
    //    return false;
    //}

    ///// <summary>
    ///// īƮ �߽ɿ��� �ڽ� ĳ��Ʈ�� ���� ���θ� �Ǻ��� ��, ����� _boxCastHit�� �����մϴ�.
    ///// </summary>
    //private bool CheckBoxContact(LayerMask layer)
    //{
    //    Vector3 worldCenter = transform.position + transform.TransformDirection(boxCastCenter);
    //    bool contact = Physics.BoxCast(worldCenter, boxCastSize * 0.5f, transform.forward, out _boxCastHit, transform.rotation, boxCastDistance, layer.value);

    //    // ���� ���̾��� ���, ���� ���̾� �ε����� �����Ͽ� �̸��� ����մϴ�.
    //    int layerIndex = Mathf.RoundToInt(Mathf.Log(layer.value, 2f));
    //    Debug.Log("BoxCast (" + LayerMask.LayerToName(layerIndex) + "): " +
    //              (contact ? "���˵� (" + _boxCastHit.collider.name + ")" : "���� ����"));
    //    return contact;
    //}

    ///// <summary>
    ///// ����� ���� �ڽ� ĳ��Ʈ ������ �ð�ȭ�մϴ�.
    ///// </summary>
    //private void OnDrawGizmosSelected()
    //{
    //    if (boxCastSize == Vector3.zero)
    //        return;

    //    Gizmos.color = Color.green;
    //    Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
    //    Gizmos.matrix = cubeTransform;
    //    Gizmos.DrawWireCube(boxCastCenter, boxCastSize);
    //}

    //#endregion


}