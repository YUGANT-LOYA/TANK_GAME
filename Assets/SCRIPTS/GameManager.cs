using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public int m_NumRoundsToWins = 5;
	public float m_StartDelay = 3f;
	public float m_EndDelay = 3f;
	public Camera_Controller m_CameraControl;
	public Text m_MessageText;
	public GameObject m_Tank_Prefab;
	public Tank_Manager[] m_Tanks;

	private int m_RoundNumber;
	private WaitForSeconds m_StartWait;
	private WaitForSeconds m_EndWait;
	private Tank_Manager m_RoundWinner;
	private Tank_Manager m_GameWinner;

	private void Start()
	{
		m_StartWait = new WaitForSeconds(m_StartDelay);
		m_EndWait = new WaitForSeconds(m_EndDelay);

		SpawnAllTanks();
		SetCameraTargets();

		StartCoroutine(GameLoop());
	}

	private void SpawnAllTanks()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			m_Tanks[i].m_Instance = Instantiate(m_Tank_Prefab, m_Tanks[i].m_SpawnPoints.position, m_Tanks[i].m_SpawnPoints.rotation) as GameObject;
			m_Tanks[i].m_PlayerNumber = i + 1;
			m_Tanks[i].Setup();
		}
	}

	private void SetCameraTargets()
	{
		Transform[] target = new Transform[m_Tanks.Length];

		for (int i = 0; i < target.Length; i++)
		{
			target[i] = m_Tanks[i].m_Instance.transform;
		}

		m_CameraControl.m_Targets = target;
	}

	private IEnumerator GameLoop()
	{
		yield return StartCoroutine(RoundStarting());
		yield return StartCoroutine(RoundPlaying());
		yield return StartCoroutine(RoundEnding());

		if (m_GameWinner != null)
		{
			Application.LoadLevel(Application.loadedLevel);
		}
		else
		{
			StartCoroutine(GameLoop());
		}
	}

	private IEnumerator RoundStarting()
	{
		ResetAllTanks();
		DisableTankControl();

		m_CameraControl.SetStartPositionandSize();
		
		m_RoundNumber++;
		m_MessageText.text = "ROUND " + m_RoundNumber;

		yield return m_StartWait;
	}

	private IEnumerator RoundPlaying()
	{
		EnableTankControl();

		m_MessageText.text = string.Empty;

		while (!OneTankLeft())
		{
			yield return null;
		}
	}

	private IEnumerator RoundEnding()
	{
		DisableTankControl();

		m_RoundWinner = null;
		m_RoundWinner = GetRoundWinner();

		if (m_RoundWinner != null)
			m_RoundWinner.m_Wins++;

		m_GameWinner = GetGameWinner();

		string msg = EndMessage();
		m_MessageText.text = msg;

		yield return m_EndWait;
	}

	private Tank_Manager GetRoundWinner()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			if (m_Tanks[i].m_Instance.activeSelf)
				return m_Tanks[i];
		}

		return null;
	}

	private Tank_Manager GetGameWinner()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			if (m_Tanks[i].m_Wins == m_NumRoundsToWins)
				return m_Tanks[i];
		}
		return null;
	}

	private string EndMessage()
	{
		string msg = "DRAW !";

		if (m_RoundWinner != null)
			msg = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND !";
		msg += "\n\n\n\n";

		for (int i = 0; i < m_Tanks.Length; i++)
		{
			msg += m_Tanks[i].m_ColoredPlayerText + " : " + m_Tanks[i].m_Wins + " WINS \n";
		}

		if (m_GameWinner != null)
			msg = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME !";

		return msg;
	}

	private void ResetAllTanks()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			m_Tanks[i].Reset();
		}
	}

	private void EnableTankControl()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			m_Tanks[i].EnableControl();
		}
	}

	private void DisableTankControl()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			m_Tanks[i].DisableControl();
		}
	}

	private bool OneTankLeft()
	{
		int NumTankLeft = 0;
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			if (m_Tanks[i].m_Instance.activeSelf)
				NumTankLeft++;
		}

		return NumTankLeft <= 1;
	}
}
