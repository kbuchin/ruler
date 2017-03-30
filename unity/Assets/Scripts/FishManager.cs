using UnityEngine;
using System.Collections.Generic;

public sealed class FishManager {

	private List<Transform> m_FishesTeam1;
	private List<Transform> m_FishesTeam2;

	public FishManager()
	{
		m_FishesTeam1 = new List<Transform>();
		m_FishesTeam2 = new List<Transform>();
	}

	public void AddFish(Transform a_FishTransform, int a_TeamID, bool a_WithOrientationUpdate)
	{
		if (a_TeamID == 1)
		{
			m_FishesTeam1.Add(a_FishTransform);
		}
		else if (a_TeamID == 2)
		{
			m_FishesTeam2.Add(a_FishTransform);
		}
		if (a_WithOrientationUpdate)
		{
			RecomputeOrientation();
		}
	}

	private void RecomputeOrientation()
	{
		OrientTeamToOpposition(m_FishesTeam1, m_FishesTeam2);
		OrientTeamToOpposition(m_FishesTeam2, m_FishesTeam1);
	}

	private static void OrientTeamToOpposition(List<Transform> a_Team, List<Transform> a_OppositeTeam)
	{
		foreach (Transform tf1 in a_Team)
		{
			float smallestDistance = int.MaxValue;
			Transform closestFish = null;
			foreach (Transform tf2 in a_OppositeTeam)
			{
				float distance = (tf1.position - tf2.position).sqrMagnitude;
				if (distance < smallestDistance)
				{
					smallestDistance = distance;
					closestFish = tf2;
				}
			}
			if (closestFish != null)
			{
				tf1.LookAt(closestFish.position);
			}
		}
	}

}
