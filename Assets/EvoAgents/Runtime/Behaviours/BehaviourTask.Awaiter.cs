using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;

namespace EvoAgents.Behaviours
{
    public partial class BehaviourTask
    {
        public readonly struct Awaiter : ICriticalNotifyCompletion {
            readonly UniTask<BehaviourStatus> task;
            readonly UniTask<BehaviourStatus>.Awaiter awaiter;

            public Awaiter(BehaviourTask _task) {
                task = _task.OnInvoke(_task, _task.Token);
                awaiter = task.GetAwaiter();
            }

            public bool IsCompleted { get => task.Status != UniTaskStatus.Pending; }
            public BehaviourStatus GetResult() {
                return awaiter.GetResult();
            }
            public void OnCompleted(Action continuation) {
                ((INotifyCompletion)awaiter).OnCompleted(continuation);
            }
            public void UnsafeOnCompleted(Action continuation) {
                ((ICriticalNotifyCompletion)awaiter).UnsafeOnCompleted(continuation);
            }
        }
    }
}