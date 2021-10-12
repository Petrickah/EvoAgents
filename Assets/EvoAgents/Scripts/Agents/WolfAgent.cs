using EvoAgents.Behaviours;
using EvoAgents.Behaviours.Mediators;
using EvoAgents.Behaviours.Actions;
using EvoAgents.Behaviours.Composites;
using EvoAgents.Behaviours.Decorators;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace EvoAgents.Agents.Wolf {
    using EvoAgents.Agents.Animal;
    using System.Collections;
    using UnityEngine.AI;

    [AddComponentMenu("EvoAgents/Agents/Wolf Behaviour Agent")]
    public class WolfAgent : BehaviourAgent
    {
        [SerializeField] private LayerMask _awareTarget;
        private CancellationTokenSource tokenSource;
        public BehaviourTask ConstructMemory =>
            TaskGenerator.Begin()
                .Sequencer()
                    .SetBlackboard(Blackboard, "Pray", () => false)
                    .SetBlackboard(Blackboard, "Resting", () => false)
                    .SetBlackboard(Blackboard, "Energy", () => Blackboard.Genome.MaxStamina)
                .Generate();
        public override BehaviourTask Behaviour =>
            TaskGenerator.Begin()
                .RepeatForever()
                    .Selector()
                        .Action(Rest())
                        .Conditional(() => !(bool)Blackboard["Pray"])
                            .Parallel()
                                .Action(Wander(2.3f, 25f))
                                .Probability(Blackboard.Genome.Aggression)
                                    .Action((self, token) =>
                                    {
                                        if (Physics.CheckSphere(transform.position, Blackboard.Genome.Scent, _awareTarget))
                                        {
                                            Blackboard.BroadcastMessage(Blackboard.CreateMessage("Pray", true)); // Raise the event
                                            return UniTask.FromResult(BehaviourStatus.Success);
                                        }
                                        return UniTask.FromResult(BehaviourStatus.Failure);
                                    })
                                .End()
                            .End()
                        .End()
                        .Selector()
                            .Conditional(() => (bool)Blackboard["Pray"] && (float)Blackboard["Energy"] > 0 && !(bool)Blackboard["Resting"])
                                .Parallel()
                                    .Action((self, token) =>
                                    {
                                        Collider[] enemies = Physics.OverlapSphere(transform.position, Blackboard.Genome.Scent, _awareTarget);
                                        var steering = Vector3.zero;
                                        if (enemies.Length > 0)
                                        {
                                            foreach (Collider enemy in enemies)
                                            {
                                                if (token.IsCancellationRequested)
                                                {
                                                    Blackboard["Pray"] = false;
                                                    return UniTask.FromResult(BehaviourStatus.Failure);
                                                }
                                                var futurePosition = enemy.transform.position;
                                                if (enemy.TryGetComponent(out NavMeshAgent enemyAgent))
                                                    futurePosition += enemyAgent.velocity * 3f;
                                                var desiredVelocity = +1f * Agent.speed * (futurePosition - transform.position).normalized;
                                                steering += (desiredVelocity - Agent.velocity).normalized;
                                            }
                                            if (steering.sqrMagnitude < 0.1f) Blackboard["Pray"] = false;
                                            else Blackboard["PursuitPray"] = steering;
                                            return UniTask.FromResult(BehaviourStatus.Success);
                                        }
                                        if (steering.sqrMagnitude < 0.1f) Blackboard["Pray"] = false;
                                        return UniTask.FromResult(BehaviourStatus.Failure);
                                    })
                                    .Sequencer()
                                        .PlayAnimation(Animation, "Running", AnimationCategory.WalkingAnimation)
                                        .UpdateBlackboard(Blackboard, "Energy", () =>
                                        {
                                            float depletionRate = -0.05f * Random.value;
                                            return depletionRate * Blackboard.Genome.MaxStamina * Time.deltaTime;
                                        })
                                        .Wander(Agent, 4.8f, 25f)
                                        .ApplyForce(Agent, 1f, () =>
                                        {
                                            if (Blackboard["PursuitPray"] is null) return Vector3.zero;
                                            return (Vector3)Blackboard["PursuitPray"];
                                        })
                                    .End()
                                .End()
                            .End()
                            .Conditional(() => (float)Blackboard["Energy"] < 0)
                                .SetBlackboard(Blackboard, "Resting", () => true)
                            .End()
                        .End()
                    .End()
                .Generate();

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Blackboard.Genome.Scent);
        }

        private void Start() {
            StartCoroutine(AICoroutine());
        }

        private IEnumerator AICoroutine() {
            tokenSource = new CancellationTokenSource();
            Blackboard.Clear();
            yield return UniTask.Create(async () => {
                if (await ConstructMemory == BehaviourStatus.Success) {
                    var behaviourTask = Behaviour;
                    behaviourTask.SetToken(tokenSource.Token);
                    return await behaviourTask;
                }
                return BehaviourStatus.Failure;
            }).ToCoroutine();
        }

        private void OnDestroy() {
            tokenSource.Cancel();
        }

        private IEnumerator AttackCoroutine() {
            if (tokenSource is null) yield break;

            tokenSource.Cancel();
            Agent.isStopped = true;
            Animation.StopAnimations();
            Animation.PlayAnimation("Attacking", AnimationCategory.WalkingAnimation);
            yield return new WaitForSeconds(4f);
            StartCoroutine(AICoroutine());
        }

        private void OnTriggerEnter(Collider other) {
            if (other is null) return;
            if (other.CompareTag("Player") || other.CompareTag("Rabbit")) {
                StartCoroutine(AttackCoroutine());
            }
        }
    }
}
