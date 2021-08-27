using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
	public float Damp_Time = 0.3f;
	public float m_ScreenEdgeBuffer = 4f;
	public float m_MinSize = 18f;
	
	public Transform[] m_Targets;

	private Camera m_Camera;
	private Vector3 m_MoveVelocity;
	private float m_ZoomTime;
	private Vector3 m_DesiredPosition;

	private void Awake()
	{
		m_Camera = GetComponentInChildren<Camera>();
	}

	private void FixedUpdate()
	{
		Move();
		Zoom();
	}

	void Move()
	{
		FindAvgPosition();
		transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, Damp_Time);
	}

	void FindAvgPosition()
	{
		Vector3 AvgPos = new Vector3();
		int NumTargets = 0;
		for(int i = 0; i < m_Targets.Length; i++)
		{
			if (!m_Targets[i].gameObject.activeSelf)
				continue;

			AvgPos += m_Targets[i].position;
			NumTargets++;
		}

		AvgPos /= NumTargets;
		AvgPos.y = transform.position.y;
		m_DesiredPosition = AvgPos;
	}

	void Zoom()
	{
		float RequiredSize = FindRequiredSize();
		m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, RequiredSize, ref m_ZoomTime, Damp_Time);
	}

	private float FindRequiredSize()
	{
		Vector3 DesiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);
		float Size = 0f;

		for(int i = 0; i < m_Targets.Length; i++)
		{
			if (!m_Targets[i].gameObject.activeSelf)
				continue;

			Vector3 TargetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);
			Vector3 DesiredPosToTarget = TargetLocalPos - DesiredLocalPos;
			Size = Mathf.Max(Size, Mathf.Abs(DesiredPosToTarget.y));
			Size = Mathf.Max(Size, Mathf.Abs(DesiredPosToTarget.x));
		}

		Size += m_ScreenEdgeBuffer;
		Size = Mathf.Max(Size, m_MinSize);

		return Size;
	}
	
	public void SetStartPositionandSize()
	{
		FindAvgPosition();
		transform.position = m_DesiredPosition;
		m_Camera.orthographicSize = FindRequiredSize();
	}

}