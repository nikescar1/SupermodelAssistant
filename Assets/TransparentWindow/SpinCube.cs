using UnityEngine;
using System.Collections;

public class SpinCube : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_SpinningVelocity;

    private Transform m_Transform;
    private Quaternion m_SpinningQuaternion;

    void Start()
    {
        m_Transform = GetComponent<Transform>();
    }

    void Update()
    {
        m_Transform.rotation *= Quaternion.Euler(Time.deltaTime * m_SpinningVelocity);
    }
}
