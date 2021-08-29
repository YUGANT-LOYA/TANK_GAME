using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber = 1;
    public float m_Speed = 15f;
    public float m_TurnSpeed = 180f;
    public AudioSource m_MovementAudio;
    public AudioClip m_EngineIdle;
    public AudioClip m_EngineDriving;
    public float m_PitchRange = 0.2f;

    private string m_MovementAxisName;
    private string m_TurnAxisName;
    private Rigidbody m_RigidBody;
    private float m_MovementInputValue;
    private float m_TurnInputValue;
    private float m_OriginalPitch;

	private void Awake()
	{
        m_RigidBody = GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
        // When the tank is turned on, make sure it's not kinematic.
        m_RigidBody.isKinematic = false;

        // Also reset the input values.
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
	}

	private void OnDisable()
	{
        // When the tank is turned off, set it to kinematic so it stops moving.
        m_RigidBody.isKinematic = true;
	}

	void Start()
    {
        // The axes names are based on player number.
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        // Store the original pitch of the audio source.
        m_OriginalPitch = m_MovementAudio.pitch;
    }

    void Update()
    {
        //Stores the Player's input Axis and make sure the audio for the Engine is playing.
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputValue = Input.GetAxis(m_TurnAxisName);
        EngineAudio();
    }

    void EngineAudio()
	{
        //Play the correct audio based on whether the Tank is moving or not and what audio is playing.
        if(Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
		{
            if (m_MovementAudio.clip == m_EngineDriving)
			{
                m_MovementAudio.clip = m_EngineIdle;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
			}
		}
        else
		{
            if (m_MovementAudio.clip == m_EngineIdle)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
	}

	private void FixedUpdate()
	{
        Move();
        Turn();
	}

    void Move()
    {
	    Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
	    //m_RigidBody.velocity = new Vector3(m_TurnInputValue, 0, m_MovementInputValue) * m_Speed;

	    m_RigidBody.MovePosition(m_RigidBody.position + movement);
    }

    void Turn()
	{
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        //Sequence of variable/parameter matters a lot in some Functions,if you try to write m_RigidBody.rotation after turnRotation, then the rotation of tank will be weird, which is not correct rotation.
        m_RigidBody.MoveRotation(m_RigidBody.rotation * turnRotation);
	}
}
