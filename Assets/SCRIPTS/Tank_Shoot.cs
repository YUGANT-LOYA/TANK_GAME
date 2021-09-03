using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tank_Shoot : MonoBehaviour
{
	public int m_PlayerNumber = 1;
	public Rigidbody m_Bullet;
	public Transform m_FireTransform;
    public Slider m_AimSlider;
	public AudioSource m_ShootingAudio;
    public AudioClip m_FiringClip;
    public AudioClip m_ChargingClip;
    public float BulletSpeed = 1.7f;
    public float m_MinLaunchForce = 15f;
    public float m_MaxLaunchForce = 30f;
    public float m_ChargingTime = 0.75f;

    private float m_CurrentLaunchForce;
    private string m_FireButton;
    private float m_ChargingSpeed;
    private bool m_Fired;

	private void OnEnable()
	{
		m_CurrentLaunchForce = m_MinLaunchForce;
		m_AimSlider.value = m_CurrentLaunchForce;
	}

	void Start()
	{
		m_FireButton = "Fire" + m_PlayerNumber;
		m_ChargingSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_ChargingTime;
	}

    void Update()
    {
	    m_AimSlider.value = m_MinLaunchForce;
	    if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
	    {
			//at max charged, but not fired yet
			m_CurrentLaunchForce = m_MaxLaunchForce;
			Fire();
	    }
		else if (Input.GetButtonDown(m_FireButton))
	    {
			//Have player pressed the Fire Button ?
			m_Fired = false;
			m_CurrentLaunchForce = m_MinLaunchForce;

			m_ShootingAudio.clip = m_ChargingClip;
			m_ShootingAudio.Play();
	    }
		else if (Input.GetButton(m_FireButton) && !m_Fired)
	    {
			//Holding the Fire Button, but not yet fired.
			m_CurrentLaunchForce += m_ChargingSpeed * Time.deltaTime;
			m_AimSlider.value = m_CurrentLaunchForce;
	    }
	    else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
	    {
		    //We released the Fire Button, having not fired yet.
		    Fire();
	    }
    }

    private void Fire()
    {
		m_Fired = true;
		Rigidbody BulletInstance =
			Instantiate(m_Bullet, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
		BulletInstance.velocity = m_CurrentLaunchForce * transform.forward * BulletSpeed;
		m_ShootingAudio.clip = m_FiringClip;
		m_ShootingAudio.Play();

		m_CurrentLaunchForce = m_MinLaunchForce;
    }
}
