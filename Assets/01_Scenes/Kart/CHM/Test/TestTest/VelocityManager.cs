using UnityEngine;

public class VelocityManager : MonoBehaviour
{
    [Header("Speed Settings")]
    public float currentSpeed = 0f;
    public float maxSpeed = 200f;
    public float acceleration = 10f;
    public float deceleration = 5f;
    public float friction = 0.1f;

    public void UpdateSpeed(float input, float deltaTime, bool isDrifting)
    {
        // ���� ó��
        if (input != 0)
        {
            currentSpeed += acceleration * input * deltaTime;
        }
        else
        {
            // ���� �� ���� ����
            if (currentSpeed > 0) // ���� ����
            {
                currentSpeed -= deceleration * deltaTime + friction * deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0f); // 0 ���Ϸ� �������� �ʰ� ����
            }
            else if (currentSpeed < 0) // ���� ����
            {
                currentSpeed += deceleration * deltaTime + friction * deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, 0f); // 0 �̻����� �ö��� �ʰ� ����
            }
        }

        // �帮��Ʈ �� �ӵ� ����
        if (isDrifting)
        {
            currentSpeed *= 0.8f;
        }

        // ����/������ �ӵ� ����
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed / 2, maxSpeed);
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}