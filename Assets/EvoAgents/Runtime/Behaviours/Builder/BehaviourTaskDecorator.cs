using Cysharp.Threading.Tasks;
using EvoAgents.Behaviours.Mediators;
using System;

namespace EvoAgents.Behaviours.Decorators {
    public static class BehaviourTaskDecorator {
        public static TaskGenerator Conditional(this TaskGenerator builder, Func<bool> condition) =>
            builder.PushTask(BehaviourTask.Run(async (self, token) => {
                if (token.IsCancellationRequested) return BehaviourStatus.Failure;
                if (!condition()) return BehaviourStatus.Failure;
                return await self.Tasks[0];
            }));

        public static TaskGenerator Conditional<TBlackboard>(this TaskGenerator builder, TBlackboard blackboard, string key)
            where TBlackboard : IBlackboard<TBlackboard> =>
            builder.Conditional(() => {
                bool? result = (bool?)blackboard[key];
                return result.HasValue && result.Value;
            });

        public static TaskGenerator RepeatForever(this TaskGenerator builder) =>
            builder.PushTask(BehaviourTask.Run(async (self, token) => {
                self.Tasks[0].SetToken(token);
                while (true) {
                    await self.Tasks[0];
                    if (token.IsCancellationRequested) break;
                    await UniTask.WaitForEndOfFrame();
                }
                return BehaviourStatus.Failure;
            }));
        public static TaskGenerator RepeatUntil(this TaskGenerator builder, BehaviourStatus behaviourStatus) =>
            builder.PushTask(BehaviourTask.Run(async (self, token) => {
                self.Tasks[0].SetToken(token);
                var status = await self.Tasks[0];
                while (status != behaviourStatus && !token.IsCancellationRequested) {
                    status = await self.Tasks[0];
                    await UniTask.WaitForEndOfFrame();
                }
                return behaviourStatus;
            }));
        public static TaskGenerator RepeatWhile(this TaskGenerator builder, BehaviourStatus behaviourStatus) =>
            builder.PushTask(BehaviourTask.Run(async (self, token) => {
                self.Tasks[0].SetToken(token);
                var status = await self.Tasks[0];
                while (status == behaviourStatus && !token.IsCancellationRequested) {
                    status = await self.Tasks[0];
                    await UniTask.WaitForEndOfFrame();
                }
                return status;
            }));
    }
}
