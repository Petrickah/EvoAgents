using EvoAgents.Behaviours.Registration;
using System.Collections.Generic;

namespace EvoAgents.Behaviours {
    using BehaviourStack = Stack<BehaviourTask>;
    public struct TaskGenerator
    {
        private readonly BehaviourStack currentPointer;
        public BehaviourTask Task => currentPointer.Peek();
        public static TaskGenerator Begin() { return new TaskGenerator(false); }
        private TaskGenerator(bool _ = false) => currentPointer = new BehaviourStack();

        public TaskGenerator PushTask(BehaviourTask _task) {
            currentPointer.Push(_task);
            return this;
        }
        public TaskGenerator End() {
            if(currentPointer.Count >= 2) {
                var task = currentPointer.Pop();
                currentPointer.Peek().Register(task);
            }
            return this;
        }
        public BehaviourTask Generate() => currentPointer.Pop();
    }
}
