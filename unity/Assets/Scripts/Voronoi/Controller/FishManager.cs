namespace Voronoi
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Manages orientation of placed object ("fishes") by both players.
    /// Orients a fish to nearest fish of other team.
    /// </summary>
    public class FishManager
    {
        // collection of fishes of both teams
        private readonly List<Transform> m_FishesTeam1 = new List<Transform>();
        private readonly List<Transform> m_FishesTeam2 = new List<Transform>();

        public void AddFish(Transform a_FishTransform, bool a_IsTeam1, bool a_WithOrientationUpdate)
        {
            // add fish to corresponding team
            if (a_IsTeam1)
            {
                m_FishesTeam1.Add(a_FishTransform);
            }
            else
            {
                m_FishesTeam2.Add(a_FishTransform);
            }

            if (a_WithOrientationUpdate)
            {
                RecomputeOrientation();
            }
        }

        /// <summary>
        /// Computes a new orientation for both teams.
        /// </summary>
	    private void RecomputeOrientation()
        {
            OrientTeamToOpposition(m_FishesTeam1, m_FishesTeam2);
            OrientTeamToOpposition(m_FishesTeam2, m_FishesTeam1);
        }

        /// <summary>
        /// Computes an orientation for the given team with respect to its opponent.
        /// Orients a team member to closest opponent.
        /// </summary>
        /// <param name="a_Team"></param>
        /// <param name="a_OppositeTeam"></param>
	    private static void OrientTeamToOpposition(List<Transform> a_Team, List<Transform> a_OppositeTeam)
        {
            // 
            if (a_Team.Count < 1 || a_OppositeTeam.Count < 0) return;
            foreach (Transform tf1 in a_Team)
            {
                // get closest fish
                // unnecessary sort, but is concise (even better to use MinBy in MoreLinq)
                var closestFish = a_OppositeTeam.OrderBy(tf2 => (tf1.position - tf2.position).sqrMagnitude)
                        .FirstOrDefault();

                if (closestFish != null)
                {
                    tf1.LookAt(closestFish.position);
                }
            }
        }

    }
}
