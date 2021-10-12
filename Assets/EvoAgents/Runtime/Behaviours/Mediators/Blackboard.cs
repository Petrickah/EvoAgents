using UnityEngine;
using System;
using System.Collections.Generic;

namespace EvoAgents.Behaviours.Mediators {
    [DisallowMultipleComponent]
    [AddComponentMenu("EvoAgents/Behaviours/Blackboard")]
    public class Blackboard: MonoBehaviour, IBlackboard<Blackboard> {
        [SerializeField] private AgentGenome _genome;
        [SerializeField] private Mediator _mediator;
        [SerializeField] private Guid guid;
        private Dictionary<string, object> _blackboard;
        public string GUID => guid.ToString();
        public Genome Genome => _genome.Genome;

        private readonly struct BlackboardMessage : IMessage<Blackboard> 
        {
            public string MessageName { get; }
            public object MessageValue { get; }
            public void UnpackMessage(Blackboard blackboard) {
                blackboard[MessageName] = MessageValue;
            }
            public BlackboardMessage(string name, object value) {
                MessageName = name;
                MessageValue = value;
            }
        }

        public static IMessage<Blackboard> CreateMessage<TObject>(string name, TObject value)
            => new BlackboardMessage(name, value);

        public object this[string name] { 
            get {
                if (_blackboard.ContainsKey(name))
                    return _blackboard[name];
                return null;
            } 
            set {
                if(name != "") {
                    if (_blackboard.ContainsKey(name)) {
                        if (value is null)
                            _blackboard.Remove(name);
                        else _blackboard[name] = value;
                    }
                    else _blackboard.Add(name, value);
                }
            }
        }

        public IMediator<Blackboard> Mediator => _mediator;

        public bool Equals(Blackboard other) {
            return guid.Equals(other.GUID);
        }

        public void ReceiveMessage(IMessage<Blackboard> message) {
            message.UnpackMessage(this);
        }

        public void SendMessage(IMessage<Blackboard> message, Blackboard to) {
            Mediator.SendMessage(message, to);
        }

        public void SubscribeTo(IMediator<Blackboard> mediator) {
            mediator.AddSubscriber(this);
            _mediator = (Mediator)mediator;
        }

        public void UnsubscribeFrom(IMediator<Blackboard> mediator) {
            mediator.RemoveSubscriber(this);
            _mediator = null;
        }

        private void Awake() {
            guid = Guid.NewGuid();
            _blackboard = new Dictionary<string, object> { { "", true } };
        }

        public void BroadcastMessage(IMessage<Blackboard> message) {
            Mediator.BroadcastMessage(message);
        }

        public void Clear() {
            _blackboard.Clear();
            _blackboard.Add("", true);
        }

        private void OnDestroy() {
            UnsubscribeFrom(_mediator);
        }
    }
}
