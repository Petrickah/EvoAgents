using Cysharp.Threading.Tasks;

namespace EvoAgents.Behaviours.Composites
{
    public static class BehaviourTaskComposite
    {
        public static TaskGenerator Sequencer(this TaskGenerator builder)
        {
            return builder.PushTask(BehaviourTask.Run(async (self, token) => {
                foreach (var task in self.Tasks) {
                    if (token.IsCancellationRequested)
                        return BehaviourStatus.Failure;

                    task.SetToken(token);
                    var status = await task;
                    if (status == BehaviourStatus.Failure) 
                        return BehaviourStatus.Failure;
                }
                return BehaviourStatus.Success;
            }));
        }

        public static TaskGenerator Selector(this TaskGenerator builder)
        {
            return builder.PushTask(BehaviourTask.Run(async (self, token) => {
                foreach (var task in self.Tasks) {
                    if (token.IsCancellationRequested)
                        return BehaviourStatus.Failure;

                    task.SetToken(token);
                    var status = await task;
                    if (status == BehaviourStatus.Success)
                        return BehaviourStatus.Success;
                }
                return BehaviourStatus.Failure;
            }));
        }
        public static TaskGenerator RandomSelector(this TaskGenerator builder)
        {
            return builder.PushTask(BehaviourTask.Run(async (self, token) => {
                if (token.IsCancellationRequested)
                    return BehaviourStatus.Failure;
                var task = self.Tasks[UnityEngine.Random.Range(0, self.Tasks.Count)];
                
                task.SetToken(token);
                var status = await task;
                if (status == BehaviourStatus.Success)
                    return BehaviourStatus.Success;
                return BehaviourStatus.Failure;
            }));
        }

        public static TaskGenerator Parallel(this TaskGenerator builder)
        {
            return builder.PushTask(BehaviourTask.Run(async (self, token) => {
                var tasks = new UniTask<BehaviourStatus>[self.Tasks.Count];
                for (int i = 0; i < self.Tasks.Count; i++)
                    tasks[i] = UniTask.Create(async () => await self.Tasks[i]);
                await UniTask.WhenAll(tasks);
                return BehaviourStatus.Success;
            }));
        }
    }
}
