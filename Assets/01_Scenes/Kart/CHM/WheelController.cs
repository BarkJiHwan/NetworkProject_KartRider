using UnityEngine;

public class WheelController : MonoBehaviour
{
    public float maxTorque = 10f; // ������ �ִ� ��ũ
    public float maxSteerAngle = 30f; // �ִ� ���� ����
    public float maxSpeed = 50f; // �ִ� �ӵ�
    public float antiRollPow;
    public float AnglePow;
    public Transform[] wheels; // ������ Ʈ������ �迭 (0: ���� �չ���, 1: ���� �޹���, 2: ������ �չ���, 3: ������ �޹���)

    private Rigidbody rb;
    private float motorInput; // ���� �Է� ��
    private float steerInput; // ���� �Է� ��

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");
        SteerAndRotateWheels(steerInput, motorInput);
        Move(steerInput, motorInput);

    }

    public void Move(float steerInput, float motorInput)
    //public void Move()
    {
        if (rb.velocity.magnitude < maxSpeed)
        {
            Vector3 forwardDirection = rb.transform.forward;
            Vector3 forwardForce = forwardDirection * motorInput *maxTorque;
            rb.AddForce(forwardForce);
        }
        else
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        Vector3 turnTorque = transform.up * steerInput * maxTorque * AnglePow;
        rb.AddTorque(turnTorque);
    }

    public void SteerAndRotateWheels(float steerInput, float motorInput)
    {
        float steerAngle = maxSteerAngle * steerInput;
        float rotationAngle = motorInput * maxTorque * Time.deltaTime * 50f; // ȸ�� ���� ���

        // ���� �չ��� (wheels[0]) ���� �����̼� ������ Y = 0
        float clamp = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle) + 90;
        Vector3 wheelEulerAngles0 = wheels[0].localEulerAngles;
        wheels[0].localRotation = Quaternion.Euler(0, clamp, wheelEulerAngles0.z);

        // ������ �չ��� (wheels[2]) ���� �����̼� ������ Y = 180
        Vector3 wheelEulerAngles2 = wheels[2].localEulerAngles;
        wheels[2].localRotation = Quaternion.Euler(0, clamp, wheelEulerAngles2.z);

        // ������ �ð��� ��� ȸ��
        Vector3 leftRotation = Vector3.forward * rotationAngle;
        Vector3 rightRotation = Vector3.back * rotationAngle;
        //����
        wheels[0].Rotate(rightRotation);
        wheels[1].Rotate(rightRotation);

        //����
        wheels[2].Rotate(leftRotation);
        wheels[3].Rotate(leftRotation);
    }

    //void FixedUpdate()
    //{
    //    ApplyAntiRollBar();
    //}

    public void ApplyAntiRollBar()
    {
        for (int i = 0; i < wheels.Length / 2; i++)
        {
            Transform wheelL = wheels[i];
            Transform wheelR = wheels[i + wheels.Length / 2];

            RaycastHit hitL;
            RaycastHit hitR;
            bool groundedL = Physics.Raycast(wheelL.position, -transform.up, out hitL, 1f);
            bool groundedR = Physics.Raycast(wheelR.position, -transform.up, out hitR, 1f);

            float travelL = groundedL ? 1 - hitL.distance : 1;
            float travelR = groundedR ? 1 - hitR.distance : 1;

            float antiRollForce = (travelL - travelR) * antiRollPow;

            if (groundedL)
                rb.AddForceAtPosition(transform.up * -antiRollForce, wheelL.position);
            if (groundedR)
                rb.AddForceAtPosition(transform.up * antiRollForce, wheelR.position);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < wheels.Length; i++)
        {
            Gizmos.DrawRay(wheels[i].position, -transform.up * 1f);
        }
    }
}
