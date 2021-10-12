using EvoAgents.Behaviours;
using EvoAgents.Behaviours.Mediators;

namespace EvoAgents.Agents {
    public interface IBehaviourAgent<TBlackboard> where TBlackboard: IBlackboard<TBlackboard> {
        public bool IsDead { get; }
        public TBlackboard Blackboard { get; }
        public BehaviourTask Behaviour { get; }
    }
}
