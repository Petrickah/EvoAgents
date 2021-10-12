using System.Collections.Generic;
using System;

namespace EvoAgents.Behaviours.Mediators {
    public interface IMessage<TBlackboard> where TBlackboard : IBlackboard<TBlackboard> {
        public void UnpackMessage(TBlackboard blackboard);
    }
    public interface IMediator<TBlackboard> where TBlackboard : IBlackboard<TBlackboard> {
        public IReadOnlyCollection<TBlackboard> Blackboards { get; }
        public void AddSubscriber(TBlackboard blackboard);
        public void RemoveSubscriber(TBlackboard blackboard);
        public void SendMessage(IMessage<TBlackboard> message, TBlackboard to);
        public void BroadcastMessage(IMessage<TBlackboard> message);
    }
    public interface IBlackboard<TBlackboard>: IEquatable<TBlackboard> where TBlackboard: IBlackboard<TBlackboard> {
        public string GUID { get; }
        public object this[string key] { get; set; }
        public IMediator<TBlackboard> Mediator { get; }
        public bool CompareTag(string tag);
        public void SubscribeTo(IMediator<TBlackboard> mediator);
        public void UnsubscribeFrom(IMediator<TBlackboard> mediator);
        public void SendMessage(IMessage<TBlackboard> message, TBlackboard to);
        public void BroadcastMessage(IMessage<TBlackboard> message);
        public void ReceiveMessage(IMessage<TBlackboard> message);
    }
}