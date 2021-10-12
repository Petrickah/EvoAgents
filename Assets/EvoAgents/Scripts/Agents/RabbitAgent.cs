using EvoAgents.Behaviours;
using EvoAgents.Behaviours.Mediators;
using EvoAgents.Behaviours.Actions;
using EvoAgents.Behaviours.Composites;
using EvoAgents.Behaviours.Decorators;
using UnityEngine;
using UnityEngine.AI;

using Cysharp.Threading.Tasks;

namespace EvoAgents.Agents.Rabbit {
    using EvoAgents.Agents.Animal;
    using System.Collections;
    using System.Threading;

    [AddComponentMenu("EvoAgents/Agents/Rabbit Behaviour Agent")]
    public class RabbitAgent : BehaviourAgent {
        [SerializeField] private LayerMask _awareTarget;
        private CancellationTokenSource tokenSource;

        public LayerMask AwareTarget { get => _awareTarget; }

        public BehaviourTask ConstructMemory =>
            TaskGenerator.Begin()
                .Sequencer()
                    .SetBlackboard(Blackboard, "Enemy", () => false)
                    .SetBlackboard(Blackboard, "Resting", () => false)
                    .SetBlackboard(Blackboard, "Energy", () => Blackboard.Genome.MaxStamina)
                .Generate();

        public override BehaviourTask Behaviour =>
            TaskGenerator.Begin()
                .RepeatForever()
                    .Selector()
                        .Action(Rest())
                        .Conditional(() => !(bool)Blackboard["Enemy"])
                            .Parallel()
                                .Action(Wander(2f, 18f))
                                .Action((self, token) => {
                                    if (Physics.CheckSphere(transform.position, Blackboard.Genome.Awareness, _awareTarget))
                                    {
                                        Blackboard.BroadcastMessage(Blackboard.CreateMessage("Enemy", true)); // Raise the event
                                        return UniTask.FromResult(BehaviourStatus.Success);
                                    }
                                    return UniTask.FromResult(BehaviourStatus.Failure);
                                })
                            .End()
                        .End()
                        .Selector()
                            .Conditional(() => (bool)Blackboard["Enemy"] && (float)Blackboard["Energy"] > 0 && !(bool)Blackboard["Resting"])
                                .Parallel()
                                    .Action((self, token) =>
                                    {
                                        Collider[] enemies = Physics.OverlapSphere(transform.position, Blackboard.Genome.Awareness, _awareTarget);
                                        var steering = Vector3.zero;
                                        if (enemies.Length > 0)
                                        {
                                            foreach (Collider enemy in enemies)
                                            {
                                                if (token.IsCancellationRequested)
                                                {
                                                    Blackboard["Enemy"] = false;
                                                    return UniTask.FromResult(BehaviourStatus.Failure);
                                                }
                                                Vector3 velocity = Vector3.zero;
                                                if (enemy.TryGetComponent(out NavMeshAgent enemyAgent))
                                                {
                                                    velocity = enemyAgent.velocity * 3f;
                                                }
                                                else if (enemy.TryGetComponent(out MoveController controller))
                                                {
                                                    velocity = controller.Velocity * 1.5f;
                                                }
                                                var futurePosition = enemy.transform.position + velocity;
                                                var desiredVelocity = -1f * Agent.speed * (futurePosition - transform.position).normalized;
                                                steering += (desiredVelocity - Agent.velocity).normalized;
                                            }
                                            if (steering.sqrMagnitude < 0.1f) Blackboard["Enemy"] = false;
                                            else Blackboard["EvadeEnemy"] = steering;
                                            return UniTask.FromResult(BehaviourStatus.Success);
                                        }
                                        if (steering.sqrMagnitude < 0.1f) Blackboard["Enemy"] = false;
                                        return UniTask.FromResult(BehaviourStatus.Failure);
                                    })
                                    .Sequencer()
                                        .PlayAnimation(Animation, "Running", AnimationCategory.WalkingAnimation)
                                        .UpdateBlackboard(Blackboard, "Energy", () => {
                                            float depletionRate = -0.05f * Random.value;
                                            return depletionRate * Blackboard.Genome.MaxStamina * Time.deltaTime;
                                        })
                                        .Wander(Agent, 6f, 18f)
                                        .ApplyForce(Agent, 1f, () => {
                                            if (Blackboard["EvadeEnemy"] is null) return Vector3.zero;
                                            return (Vector3)Blackboard["EvadeEnemy"];
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
            Gizmos.DrawWireSphere(transform.position, Blackboard.Genome.Awareness);
        }

        private void Start() {
            StartCoroutine(AICoroutine());
        }
        private void OnDestroy() {
            tokenSource.Cancel();
        }

        private IEnumerator AICoroutine() {
            tokenSource = new CancellationTokenSource();
            Blackboard.Clear();
            yield return UniTask.Create(async () => await ConstructMemory).ToCoroutine();
            var behaviourTask = Behaviour;
            yield return UniTask.Create(async () => {
                behaviourTask.SetToken(tokenSource.Token);
                return await behaviourTask;
            }).ToCoroutine();
        }

        IEnumerator DieCoroutine(Collider other) {
            if (tokenSource is null) yield break;

            tokenSource.Cancel();
            while (!Animation.IsDead) {
                Agent.isStopped = true;
                Animation.StopAnimations();
                Animation.PlayAnimation("Dead", AnimationCategory.DeadAnimation);
                yield return new WaitForSeconds(2f);
            }
            AgentManager.CatchRabbit(this, other);
        }

        private void OnTriggerEnter(Collider other) {
            if (other is null) return;
            if (other.CompareTag("Wolf")) {
                Debug.Log($"Attacked by wolf: {other}");
                StartCoroutine(DieCoroutine(other));
            }
            else if(other.CompareTag("Player")) AgentManager.CatchRabbit(this, other);
        }
    }
}