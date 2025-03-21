using System.Collections;
using UnityEngine;

public class CHMTestKartController : MonoBehaviour
{
    [Header("Speed Settings")]
    [Tooltip("�ִ� �ӵ� (km/h)")]
    [SerializeField] private float maxSpeed = 200f; // �ִ� �ӵ�
    [Tooltip("���ӵ� (m/s^2)")]
    [SerializeField] private float acceleration = 10f; // ���ӵ�
    [Tooltip("���ӵ� (m/s^2)")]
    [SerializeField] private float deceleration = 15f; // ���ӵ�

    [Header("Steering Settings")]
    [Tooltip("���� �ΰ��� (Ŭ���� ���� �ΰ�)")]
    [SerializeField] private float steeringForce = 200f; // ���� �ΰ���

    [Header("Drift Settings")]
    [Tooltip("�帮��Ʈ �� �ӵ� ���� ����")]
    [SerializeField] private float driftSpeedReduction = 0.8f; // �帮��Ʈ �ӵ� ���� ����
    [Tooltip("�帮��Ʈ �� �������� �������� ��")]
    [SerializeField] private float driftForce = 50f; // �帮��Ʈ �� �������� �������� ��

    [Header("Motor Force")]
    [Tooltip("������ ���� ��")]
    [SerializeField] private float motorForce = 500f; // ���� �� (���� ��)

    public CHMTestWheelController CHM;
    private Rigidbody rigid;
    private float currentSpeed = 0f; // ���� �ӵ�
    private float inputVertical; // ����/���� �Է�
    private float inputHorizontal; // ���� �Է�
    private bool isDrifting = false; // �帮��Ʈ ����

    private float savedSpeed = 0f; // �帮��Ʈ ���� �� �ӵ� ����
    private float savedSteeringAngle = 0f; // �帮��Ʈ ���� �� ���Ⱒ ����

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ����� �Է�
        inputVertical = Input.GetAxis("Vertical"); // W/S �Ǵ� ��/�Ʒ� ����Ű
        inputHorizontal = Input.GetAxis("Horizontal"); // A/D �Ǵ� ��/�� ����Ű

        // �帮��Ʈ ����
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDrifting)
        {
            StartDrift();
        }

        // �帮��Ʈ ����
        if (Input.GetKeyUp(KeyCode.LeftShift) && isDrifting)
        {
            StopDrift();
        }
    }

    private void FixedUpdate()
    {
        if (!isDrifting)
        {

            HandleAcceleration(); // ����/���� �ӵ� ó��
            HandleSteering();     // ���� ó��
        }
    }

    private void HandleAcceleration()
    {
        if (inputVertical != 0)
        {
            // ����/���� ó��
            currentSpeed += inputVertical * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

            // Rigidbody �ӵ� ���� (m/s�� ��ȯ)
            rigid.velocity = transform.forward * (currentSpeed / 3.6f);
            rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y, rigid.velocity.z);
        }
        else
        {
            // �Է��� ���� ��� ���� ó��
            if (currentSpeed > 0)
                currentSpeed -= deceleration * Time.fixedDeltaTime;
            else if (currentSpeed < 0)
                currentSpeed += deceleration * Time.fixedDeltaTime;

            if (Mathf.Abs(currentSpeed) < 0.1f)
                currentSpeed = 0;

            // Rigidbody �ӵ� ������Ʈ
            rigid.velocity = transform.forward * (currentSpeed / 3.6f);
            rigid.velocity = new Vector3(rigid.velocity.x, rigid.velocity.y, rigid.velocity.z);
        }
    }

    private void HandleSteering()
    {
        // �ӵ� ��� ���� ���� ���
        float steerAngle = inputHorizontal * GetSpeedProportionalValue() * steeringForce; // ���� �ΰ��� �ݿ�
        Quaternion turnRotation = Quaternion.Euler(0, steerAngle * Time.fixedDeltaTime, 0);
        rigid.MoveRotation(rigid.rotation * turnRotation);
    }

    // �ӵ��� ����� �� ��ȯ (0 ~ 1)
    private float GetSpeedProportionalValue()
    {
        return Mathf.Clamp(1 - (Mathf.Abs(currentSpeed) / maxSpeed), 0.3f, 1f); // �ּ� 0.3 ~ �ִ� 1
    }

    private void StartDrift()
    {
        isDrifting = true;

        // �帮��Ʈ ���� �� ���� �ӵ��� ���Ⱒ ����
        savedSpeed = currentSpeed;
        savedSteeringAngle = inputHorizontal;

        // �帮��Ʈ �� �ӵ� ����
        currentSpeed *= driftSpeedReduction;

        CHM.SetSkidMarkActive(isDrifting);
        // �帮��Ʈ �ڷ�ƾ ����
        StartCoroutine(DriftRoutine());

    }

    private void StopDrift()
    {
        isDrifting = false;
        CHM.SetSkidMarkActive(isDrifting);
        StopCoroutine(DriftRoutine());
    }

    private IEnumerator DriftRoutine()
    {
        while (isDrifting)
        {
            // �帮��Ʈ �� ����� �ӵ��� �����ϸ� �̵�
            rigid.velocity = transform.forward * (savedSpeed / 3.6f);

            // ���� �������� �� ���ϱ�
            Vector3 driftForceVector = transform.right * savedSteeringAngle * driftForce;
            rigid.AddForce(driftForceVector, ForceMode.Acceleration);

            // ���Ⱚ �ݿ�
            float steerAngle = inputHorizontal * GetSpeedProportionalValue() * steeringForce;
            Quaternion turnRotation = Quaternion.Euler(0, steerAngle * Time.fixedDeltaTime, 0);
            rigid.MoveRotation(rigid.rotation * turnRotation);
            Debug.Log("�帮��Ʈ");

            yield return null;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            rigid.velocity = new Vector3(0,rigid.velocity.y,0);
            currentSpeed = -1;

        }
    }
}
