using UnityEngine;

public class WheelManager : MonoBehaviour
{
    [Header("Wheel Settings")]
    public GameObject[] wheels; // 0, 1: �չ��� / 2, 3: �޹���
    public GameObject skidMarkPrefab; // ��Ű�帶ũ ������ (TrailRenderer)
    private TrailRenderer[] skidMarks; // TrailRenderer �迭

    private void Awake()
    {
       
        // �޹���(2, 3��)���� ��Ű�帶ũ ����
        skidMarks = new TrailRenderer[2];
        for (int i = 2; i < wheels.Length; i++) // �޹����� ó��
        {
            GameObject skidMarkObject = Instantiate(skidMarkPrefab, wheels[i].transform);
            skidMarks[i - 2] = skidMarkObject.GetComponent<TrailRenderer>();
            skidMarks[i - 2].emitting = false; // �ʱ⿡�� ��Ȱ��ȭ
        }
    }

    public void UpdateAndRotateWheels(float steerInput, float motorInput, float speed, bool isDrifting)
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            // ���� Z�� ȸ�� ó�� (���� ȿ��)
            wheels[i].transform.Rotate(Vector3.forward, speed * Time.deltaTime * motorInput);

            // �չ��� ���� ó�� (0, 1�� ����)
            if (i == 0 || i == 1)
            {
                float steerAngle = steerInput * 10f; // ���� ����
                wheels[i].transform.localEulerAngles = new Vector3(90 , steerAngle, 0);
            }

            // �޹���(2, 3��)�� ������ ó��, ���� ����
        }

        // ��Ű�帶ũ ���� ����
        UpdateSkidMarks(isDrifting);
    }

    private void UpdateSkidMarks(bool isDrifting)
    {
        // �帮��Ʈ ���� �� �޹������� ��Ű�帶ũ Ȱ��ȭ
        for (int i = 0; i < skidMarks.Length; i++)
        {
            skidMarks[i].emitting = isDrifting;
        }
    }
}