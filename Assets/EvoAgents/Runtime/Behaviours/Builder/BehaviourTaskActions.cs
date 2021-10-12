using Cysharp.Threading.Tasks;
using EvoAgents.Behaviours.Mediators;
using EvoAgents.Behaviours.Registration;
using System;

namespace EvoAgents.Behaviours.Actions
{
    public static class BehaviourTaskActions
    {
        public static TaskGenerator Action(this TaskGenerator builder, InvokeAction invokeAction) {
            builder.Task.Register(BehaviourTask.Run(invokeAction));
            return builder;
        }
        public static TaskGenerator Action(this TaskGenerator builder, BehaviourTask taskAction)
        {
            builder.Task.Register(taskAction);
            return builder;
        }
        public static TaskGenerator WaitFor(this TaskGenerator builder, float waitTimeSeconds) =>
            builder.Action(async (self, token) => {
                if (token.IsCancellationRequested)
                    return BehaviourStatus.Failure;

                await UniTask.Delay(TimeSpan.FromSeconds(waitTimeSeconds), DelayType.DeltaTime, PlayerLoopTiming.Update);
                return BehaviourStatus.Success;
            });
        public static TaskGenerator SetBlackboard<TBlackboard, TObject>(this TaskGenerator builder, TBlackboard blackboard, string key, Func<TObject> factory)
            where TBlackboard : IBlackboard<TBlackboard> =>
            builder.Action((self, token) => {
                if (token.IsCancellationRequested) return UniTask.FromResult(BehaviourStatus.Failure);
                blackboard.ReceiveMessage((IMessage<TBlackboard>)Blackboard.CreateMessage(key, factory()));
                return UniTask.FromResult(BehaviourStatus.Success);
            });
        public static TaskGenerator Broadcast<TBlackboard, TMessage>(this TaskGenerator builder, TBlackboard blackboard, TMessage message)
            where TBlackboard : IBlackboard<TBlackboard>
            where TMessage : IMessage<TBlackboard> =>
            builder.Action((self, token) => {
                if (token.IsCancellationRequested) return UniTask.FromResult(BehaviourStatus.Failure);
                blackboard.BroadcastMessage(message);
                return UniTask.FromResult(BehaviourStatus.Success);
            });
        public static TaskGenerator SendMessage<TBlackboard, TMessage>(this TaskGenerator builder, TBlackboard from, TMessage message, Func<TBlackboard> to)
            where TBlackboard : IBlackboard<TBlackboard>
            where TMessage : IMessage<TBlackboard> =>
            builder.Action((self, token) => {
                if (token.IsCancellationRequested) return UniTask.FromResult(BehaviourStatus.Failure);
                from.SendMessage(message, to());
                return UniTask.FromResult(BehaviourStatus.Success);
            });
    }
}
