using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{
    public Transform    m_target;
    public bool         m_reverseFace = false;

    void Update()
    {
        if (m_target != null)
        {
            Vector3 targetPos = new Vector3(m_target.position.x, this.transform.position.y, m_target.position.z);
            if ( m_reverseFace ) {
                targetPos = transform.position * 2.0f - targetPos;
            }
            transform.LookAt(targetPos);
        }
    }
}