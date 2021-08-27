using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public LayerMask m_TankMask;
	public ParticleSystem m_ExplosionParticles;
	public AudioSource m_ExplosionAudio;
	public float m_ExplosionForce = 1000f;
	public float m_MaxDamage = 100f;
	public float m_MaxLifeTime = 2f;
	public float m_ExplosionRadius = 5f;

	void Start()
    {
        Destroy(gameObject, m_MaxLifeTime);
    }

	private void OnTriggerEnter(UnityEngine.Collider other)
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

		for (int i = 0; i < colliders.Length; i++)
		{
			Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();
			if (!targetRigidbody)
				continue;
			
			targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

			TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
			if (!targetHealth)
				continue;

			float damage = CalculateDamage(targetRigidbody.position);
			targetHealth.TakeDamage(damage);
		}
		m_ExplosionParticles.transform.parent = null;
		m_ExplosionParticles.Play();
		m_ExplosionAudio.Play();

		Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);
		Destroy(gameObject);
	}

	private float CalculateDamage(Vector3 targetPosition)
	{
		Vector3 ExplosionToTarget = targetPosition - transform.position;
		float ExplosionDistance = ExplosionToTarget.magnitude;
		float RelativeDistance = (m_ExplosionRadius - ExplosionDistance) / m_ExplosionRadius;
		float damage = RelativeDistance * m_MaxDamage;
		damage = Mathf.Max(0f, damage);

		return damage;
	}
}
