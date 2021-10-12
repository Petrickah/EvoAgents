using UnityEngine;

namespace EvoAgents.Behaviours {
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Genome", menuName = "EvoAgents/Agent Genome")]
    public class AgentGenome : ScriptableObject {
        [SerializeField] private Genome m_Genome;
        public Genome Genome => m_Genome;
    }

    [System.Serializable]
    public struct Genome
    {
        [Tooltip("Which distance this agent sense food/pray?")]
        public float Scent;

        [Tooltip("Which distance this agent sense predators")]
        public float Awareness;

        [Tooltip("How many seconds this agent wander before tiredness?")]
        public float MaxStamina;

        [Tooltip("Chance to attack another agent (in %)")]
        [Range(0f, 1f)]
        public float Aggression;

        [Tooltip("Dominance of this agent. Higher value means more dominant than others.")]
        public float Dominance;
    }
}
