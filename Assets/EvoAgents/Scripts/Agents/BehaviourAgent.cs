using EvoAgents.Agents.Animal;
using EvoAgents.Behaviours;
using EvoAgents.Behaviours.Actions;
using EvoAgents.Behaviours.Composites;
using EvoAgents.Behaviours.Decorators;
using EvoAgents.Behaviours.Mediators;
using UnityEngine;
using UnityEngine.AI;

namespace EvoAgents.Agents {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Blackboard), typeof(NavMeshAgent), typeof(AudioSource))]
    public abstract class BehaviourAgent : MonoBehaviour, IBehaviourAgent<Blackboard> {
        [SerializeField] private Blackboard _blackboard;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AgentAnimation _animation;

        public BehaviourTask Wander(float speed, float distance) =>
            TaskGenerator.Begin()
                .Selector()
                    .Conditional(() => (float)Blackboard["Energy"] > 0 && !(bool) Blackboard["Resting"])
                        .Sequencer()
                            .PlayAnimation(Animation, "Walking", AnimationCategory.WalkingAnimation)
                            .UpdateBlackboard(Blackboard, "Energy", () => {
                                float depletionRate = -0.05f * Random.value;
                                return depletionRate* Blackboard.Genome.MaxStamina * Time.deltaTime;
                            })
                            .Wander(Agent, speed, distance)
                        .End()
                    .End()
                    .Conditional(() => (float)Blackboard["Energy"] < 0)
                        .SetBlackboard(Blackboard, "Resting", () => true)
                    .End()
                .Generate();


        public BehaviourTask Rest() =>
            TaskGenerator.Begin()
                .Conditional(() => (float)Blackboard["Energy"] < 0 && (bool)Blackboard["Resting"])
                    .Sequencer()
                        .StopAnimation(Animation)
                        .StopMovement(Agent)
                        .WaitUntil(() => (float) Blackboard["Energy"] > 0.99 * Blackboard.Genome.MaxStamina)
                            .UpdateBlackboard(Blackboard, "Energy", () => {
                                float restoreRate = 0.25f * Random.value;
                                return restoreRate * Blackboard.Genome.MaxStamina * Time.deltaTime;
                            })
                        .End()
                        .Conditional(() => (float) Blackboard["Energy"] > 0.99 * Blackboard.Genome.MaxStamina)
                            .SetBlackboard(Blackboard, "Resting", () => false)
                        .End()
                    .End()
                .Generate();

        public NavMeshAgent Agent => _agent;
        public AgentAnimation Animation => _animation;
        public AudioSource AudioSource => _audioSource;
        public Blackboard Blackboard => _blackboard;
        public abstract BehaviourTask Behaviour { get; }

        public bool IsDead => Animation.IsDead;
    }
}
