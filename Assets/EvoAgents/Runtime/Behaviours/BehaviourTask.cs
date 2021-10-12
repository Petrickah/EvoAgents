using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace EvoAgents.Behaviours {
    public enum BehaviourStatus { Running, Success, Failure };
    public delegate UniTask<BehaviourStatus> InvokeAction(BehaviourTask self, CancellationToken token = default);

    public partial class BehaviourTask {
        private readonly List<BehaviourTask> _tasks;
        private InvokeAction OnInvoke { get; }
        private CancellationToken Token { get; set; }
        public List<BehaviourTask> Tasks { get => _tasks; }
        public BehaviourStatus Status { get; private set; }
        protected BehaviourTask(InvokeAction _invoke, CancellationToken _token = default) {
            OnInvoke = _invoke;
            Token = _token;
            Status = BehaviourStatus.Running;
            _tasks = new List<BehaviourTask>();
        }
        public Awaiter GetAwaiter() {
            return new Awaiter(this);
        }
        public void SetToken(CancellationToken token) => Token = token;
        public static BehaviourTask Run(InvokeAction _invoke, CancellationToken _token = default) {
            return new BehaviourTask(_invoke, _token);
        }
    }
}
