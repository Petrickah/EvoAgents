using Cysharp.Threading.Tasks;
using EvoAgents.Behaviours;
using EvoAgents.Behaviours.Actions;
using EvoAgents.Behaviours.Mediators;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace EvoAgents.Agents.Animal
{
    public static class AnimalExtensions
    {
        public static TaskGenerator UpdateBlackboard(this TaskGenerator builder, Blackboard memory, string key, Func<float> updater) =>
            builder.SetBlackboard(memory, key, () => {
                float currentValue = (float)memory[key];
                return currentValue + updater();
            });

        public static TaskGenerator SetDestination(this TaskGenerator builder, NavMeshAgent agent, float speed, Func<Vector3> action) =>
            builder.Action((InvokeAction)((BehaviourTask self, System.Threading.CancellationToken token) => {
                if (token.IsCancellationRequested)
                    return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Failure);

                agent.speed = speed;
                if (agent.isStopped) agent.isStopped = false;
                if (agent.remainingDistance > agent.stoppingDistance)
                    return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Running);

                agent.SetDestination(action());
                return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Success);
            }));

        public static TaskGenerator ApplyForce(this TaskGenerator builder, NavMeshAgent agent, float value, Func<Vector3> force) =>
            builder.Action((InvokeAction)((BehaviourTask self, System.Threading.CancellationToken token) => {
                if (token.IsCancellationRequested)
                    return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Failure);
                if (agent.isStopped) agent.isStopped = false;
                agent.velocity = (agent.velocity + force().normalized * value).normalized * agent.speed;
                return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Success);
            }));

        public static TaskGenerator PlayAnimation(this TaskGenerator builder, AgentAnimation animation, string animationName, AnimationCategory animationCategory) =>
            builder.Action((InvokeAction)((BehaviourTask self, System.Threading.CancellationToken token) => {
                if (token.IsCancellationRequested || animation.IsDead)
                    return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Failure);

                if (animation.IsPlayingAnimation) animation.StopAnimations();
                animation.PlayAnimation(animationName, animationCategory);
                return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Success);
            }));

        public static TaskGenerator StopAnimation(this TaskGenerator builder, AgentAnimation animation) =>
            builder.Action((InvokeAction)((BehaviourTask self, System.Threading.CancellationToken token) => {
                if (token.IsCancellationRequested || animation.IsDead)
                    return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Failure);

                if (animation.IsPlayingAnimation) animation.StopAnimations();
                return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Success);
            }));
        public static TaskGenerator StopMovement(this TaskGenerator builder, NavMeshAgent agent) =>
            builder.Action((InvokeAction)((BehaviourTask self, System.Threading.CancellationToken token) => {
                if (token.IsCancellationRequested)
                    return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Failure);

                if (!agent.isStopped) agent.isStopped = true;
                return UniTask.FromResult<Behaviours.BehaviourStatus>((BehaviourStatus)Behaviours.BehaviourStatus.Success);
            }));
        public static TaskGenerator Wander(this TaskGenerator builder, NavMeshAgent agent, float speed, float distance) =>
            builder.SetDestination(agent, speed, () => {
                Transform rabbit = agent.transform;
                var randomPosition = rabbit.position + distance * UnityEngine.Random.insideUnitSphere;
                if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, distance, -1))
                    return hit.position;
                return rabbit.position;
            });

        public static TaskGenerator WaitUntil(this TaskGenerator builder, Func<bool> condition) =>
            builder.PushTask(BehaviourTask.Run((InvokeAction)(async (BehaviourTask self, System.Threading.CancellationToken token) => {
                while (!condition())
                {
                    if (token.IsCancellationRequested) return Behaviours.BehaviourStatus.Failure;
                    self.Tasks[0].SetToken(token);

                    await self.Tasks[0];
                    await UniTask.WaitForEndOfFrame();
                }
                return Behaviours.BehaviourStatus.Success;
            })));
        public static TaskGenerator Probability(this TaskGenerator builder, float chance) =>
            builder.PushTask(BehaviourTask.Run((InvokeAction)(async (BehaviourTask self, System.Threading.CancellationToken token) => {
                if (UnityEngine.Random.value < chance)
                {
                    self.Tasks[0].SetToken(token);
                    await self.Tasks[0];
                    return Behaviours.BehaviourStatus.Success;
                }
                return Behaviours.BehaviourStatus.Failure;
            })));
    }
}
