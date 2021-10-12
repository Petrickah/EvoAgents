namespace EvoAgents.Behaviours.Registration
{
    public static partial class BehaviourTaskExtensions {
        public static BehaviourTask Register(this BehaviourTask task, BehaviourTask child) {
            task.Tasks.Add(child);
            return task;
        }
        public static BehaviourTask Unregister(this BehaviourTask task, BehaviourTask child) {
            task.Tasks.Remove(child);
            return task;
        }
        public static BehaviourTask Unregister(this BehaviourTask task) {
            task.Tasks.RemoveAt(task.Tasks.Count - 1);
            return task;
        }
    }
}
