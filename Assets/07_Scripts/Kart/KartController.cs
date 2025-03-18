using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour
{
    [SerializeField] GameObject wheels;
    [SerializeField] GameObject carBody;

    public float maxSpeed = 50f; // �ִ� �ӵ�
    public float antiRollPow;
    public float AnglePow;

    WheelController wheelCtrl;
    Rigidbody rigid;
    Transform[] wheelTrans;

    private float motorInput; // ���� �Է� ��
    private float steerInput; // ���� �Է� ��
    private float driftTime = 0f;

    private bool isDrifting = false;
    private bool isBoostTriggered = false;
    private bool isUpArrowKeyPressed = false; // ���� Ű�� �����ִ� ����
    private bool wasUpArrowKeyReleased = true; // ������ Ű�� ������ ����

    [Header("Steering Settings")]
    [Tooltip("���� �ΰ���")]
    public float steeringForce = 200f;

    [Header("Motor Settings")]
    [Tooltip("���ӵ� ��")]
    public float motorForce = 1000f;
    [Tooltip("�ִ� �ӵ� (km/h)")]
    public float maxSpeedKPH = 280f;

    [Header("Physics Settings")]
    [Tooltip("��Ƽ �� ����")]
    public float antiRollForce = 5000f;
    [Tooltip("���� ���̾�")]
    public LayerMask groundLayer;

    [Header("Drift Settings")]
    [Tooltip("�帮��Ʈ Ű ����")]
    public KeyCode driftKey = KeyCode.LeftShift;
    [Tooltip("�帮��Ʈ ���� ����")]
    public float driftFactor = 0.5f;
    [Tooltip("�帮��Ʈ ���� ��")]
    public float driftForceSide = 200f;
    [Tooltip("�ִ� �帮��Ʈ ���� �ð�")]
    public float maxDriftDuration = 2f;
    [Tooltip("�帮��Ʈ �� ���� ���� ��")]
    public float currentDriftForce = 20f;

    [Header("Drag Settings")]
    [Tooltip("�⺻ �巡�� ��")]
    public float normalDrag = 0.5f;
    [Tooltip("�帮��Ʈ �� �巡�� ��")]
    public float driftDrag = 0.01f;
    [Tooltip("�⺻ Angular Drag ��")]
    public float normalAngularDrag = 0.05f;
    [Tooltip("�帮��Ʈ �� Angular Drag ��")]
    public float driftAngularDrag = 0.01f;

    [Header("Boost Settings")]
    [Tooltip("�ν�Ʈ ��")]
    public float boostForce = 1.1f;
    [Tooltip("�ν�Ʈ ���� �ð�")]
    public float boostDuration = 1.5f;

    private void Awake()
    {
        wheelCtrl = wheels.GetComponent<WheelController>();
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        wheelTrans = new Transform[wheels.transform.childCount];
        for (int i = 0; i < wheelTrans.Length; i++)
        {
            wheelTrans[i] = wheels.transform.GetChild(i).transform;
        }

        // ������ٵ� �巡�� �ʱ�ȭ
        rigid.drag = normalDrag;
        // �⺻ Angular Drag �� �ʱ�ȭ
        rigid.angularDrag = normalAngularDrag;
    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");

        // Ű �Է� ���� ����
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (wasUpArrowKeyReleased)
            {
                isUpArrowKeyPressed = true; // ���Ӱ� Ű �Է�
                wasUpArrowKeyReleased = false; // �Է� ���·� ��ȯ
            }
            else
            {
                isUpArrowKeyPressed = false; // ���� �Է��� ����
            }
        }
        else
        {
            wasUpArrowKeyReleased = true; // Ű�� ������ ���·� ����
            isUpArrowKeyPressed = false; // �Է� ����
        }

        if (steerInput != 0 || motorInput != 0)
        {
            //wheelCtrl.SteerAndRotateWheels(steerInput, motorInput);
            //Move(steerInput, motorInput);

            float speed = rigid.velocity.magnitude;
            wheelCtrl.UpdateWheelRotation(motorInput, speed);
            //DisplaySpeed();
        }
    }

    private void FixedUpdate()
    {
        if (steerInput != 0 || motorInput != 0)
        {
            HandleSteering(steerInput);  // ���� ó��
            HandleMovement(motorInput); // ����/���� ó��
            //ApplyAntiRoll();            // ��Ƽ�� ó��
            LimitSpeed();               // �ִ� �ӵ� ����

            // �帮��Ʈ ���� �� ���� ����
            if (Input.GetKey(driftKey))
            {
                StartDrift(); // �帮��Ʈ ����
            }
            else
            {
                EndDrift(); // �帮��Ʈ ����
            }
        }
    }

    private void LimitSpeed()
    {
        float currentSpeed = rigid.velocity.magnitude * 3.6f;
        if (currentSpeed > maxSpeedKPH)
        {
            float speedLimit = maxSpeedKPH / 3.6f;
            rigid.velocity = rigid.velocity.normalized * speedLimit;
        }
    }

    public void StartDrift()
    {
        if (!isDrifting)
        {
            isDrifting = true;
            driftTime = 0f;

            // �ﰢ���� ���� ȿ��
            rigid.velocity *= 0.8f;
            // �帮��Ʈ �� �巡�� �� ����
            rigid.drag = driftDrag;
            // Angular Drag ����
            rigid.angularDrag = driftAngularDrag;

            // �帮��Ʈ �� ����
            float speedFactor = Mathf.Clamp01(rigid.velocity.magnitude / (maxSpeedKPH / 3.6f));
            float adjustedDriftForceSide = driftForceSide * (1 - speedFactor) * 5f;

            if (Input.GetAxis("Horizontal") < 0)
            {
                currentDriftForce = Mathf.Lerp(currentDriftForce, adjustedDriftForceSide, Time.deltaTime * 10f);
                Vector3 sideForce = transform.right * -currentDriftForce;
                rigid.AddForce(sideForce, ForceMode.Acceleration);
            }
            else if (Input.GetAxis("Horizontal") > 0)
            {
                currentDriftForce = Mathf.Lerp(currentDriftForce, adjustedDriftForceSide, Time.deltaTime * 10f);
                Vector3 sideForce = transform.right * currentDriftForce;
                rigid.AddForce(sideForce, ForceMode.Acceleration);
            }

            // ��Ű�� ��ũ Ȱ��ȭ
            //wheelCtrl.SetSkidMarkActive(true);
            Debug.Log("�帮��Ʈ ����!");
        }
    }

    public void EndDrift()
    {
        if (isDrifting)
        {
            isDrifting = false;
            driftTime = 0f;
            currentDriftForce = 0f;
            // �⺻ �巡�� �� ����
            rigid.drag = normalDrag;
            // Angular Drag ����
            rigid.angularDrag = normalAngularDrag;

            // ��Ű�帶ũ ��Ȱ��ȭ
            //wheelCtrl.SetSkidMarkActive(false);

            Debug.Log("�帮��Ʈ ����!");
            StartCoroutine(BoostCheckCoroutine());
        }
    }
    private IEnumerator BoostCheckCoroutine()
    {
        float timer = 0f;
        while (timer < 0.5f)
        {
            if (isUpArrowKeyPressed) // ���Է� ����
            {
                TriggerBoost();
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void TriggerBoost()
    {
        if (!isBoostTriggered)
        {
            isBoostTriggered = true;
            Debug.Log("�ν��� �ߵ�!");
            StartCoroutine(ApplyBoostCoroutine());
        }
    }

    private IEnumerator ApplyBoostCoroutine()
    {
        float timer = 0f;
        while (timer < boostDuration)
        {
            float currentSpeed = rigid.velocity.magnitude * 3.6f; // m/s�� km/h�� ��ȯ
            if (currentSpeed < maxSpeedKPH) // �ִ� �ӵ� �ʰ� ���� Ȯ��
            {
                rigid.AddForce(transform.forward * boostForce, ForceMode.Acceleration);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        isBoostTriggered = false;
    }

    private void HandleSteering(float steerInput)
    {
        float currentSpeed = rigid.velocity.magnitude;
        float steeringSensitivity = Mathf.Clamp(1 - (currentSpeed / (maxSpeedKPH / 3.6f)), 0.5f, 2.0f); // �ӵ��� ���� ���� �ΰ��� ����

        // �̵� ����� ��ȭ
        if (isDrifting) // �帮��Ʈ �߿��� ��ȭ�� ���� ȿ�� �߰�
        {
            rigid.velocity = Quaternion.Euler(0, steerInput * 90f * Time.deltaTime, 0) * rigid.velocity;
        }

        wheelCtrl.RotateWheel(steerInput, steeringSensitivity);

        // ���� ��ü ȸ�� ó��
        Vector3 turnDirection = Quaternion.Euler(0, steerInput * steeringForce * steeringSensitivity * Time.deltaTime, 0) * transform.forward;
        rigid.MoveRotation(Quaternion.LookRotation(turnDirection));
    }

    private void HandleMovement(float motorInput)
    {
        float adjustedMotorForce = isDrifting ? motorForce * 0.3f : motorForce;
        Vector3 forwardForce = transform.forward * motorInput * adjustedMotorForce;

        rigid.AddForce(forwardForce, ForceMode.Force);
    }

    private void DisplaySpeed()
    {
        float speed = rigid.velocity.magnitude * 3.6f; // m/s�� km/h�� ��ȯ
        //if (speedText != null)
        //{
        //    speedText.text = "Speed: " + speed.ToString("F2") + " km/h";
        //}
    }

    void ApplyAntiRoll()
    {
        for (int i = 0; i < wheelTrans.Length / 2; i++)
        {
            float leftSuspension = GetWheelSuspensionForce(wheelTrans[i]);
            float rightSuspension = GetWheelSuspensionForce(wheelTrans[i + 1]);

            float antiRoll = (leftSuspension - rightSuspension) * antiRollForce;

            if (leftSuspension > 0)
                rigid.AddForceAtPosition(wheelTrans[i].up * antiRoll, wheelTrans[i].position, ForceMode.Force);

            if (rightSuspension > 0)
                rigid.AddForceAtPosition(wheelTrans[i + 1].up * -antiRoll, wheelTrans[i + 1].position, ForceMode.Force);
        }
    }

    private float GetWheelSuspensionForce(Transform wheel)
    {
        Ray ray = new Ray(wheel.position, -wheel.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 1f, groundLayer))
        {
            return 1 - hit.distance;
        }
        return 0;
    }
}
