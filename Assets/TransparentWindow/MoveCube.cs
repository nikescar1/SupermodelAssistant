using UnityEngine;
using System.Collections;
using System;

public class MoveCube : MonoBehaviour
{
    [SerializeField]
    private float m_Range;

    [SerializeField]
    private float m_Velocity;

    private Transform m_Transform;
    private float m_StartingX;
    private bool m_GoingRight;

    void Start()
    {
        if (m_Range < 0.0f || m_Velocity < 0.0f)
            throw new Exception();

        m_Transform = GetComponent<Transform>();
        m_StartingX = m_Transform.position.x;
    }

    void Update()
    {
        if (m_GoingRight)
        {
            var newPosition = m_Transform.position + new Vector3(Time.deltaTime * m_Velocity, 0.0f);
            m_Transform.position = newPosition;

            if (newPosition.x - m_StartingX > m_Range)
                m_GoingRight = false;
        }
        else
        {
            var newPosition = m_Transform.position - new Vector3(Time.deltaTime * m_Velocity, 0.0f);
            m_Transform.position = newPosition;

            if (m_StartingX - newPosition.x > m_Range)
                m_GoingRight = true;
        }
    }
}
