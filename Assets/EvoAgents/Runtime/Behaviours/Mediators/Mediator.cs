using UnityEngine;
using System.Collections.Generic;

namespace EvoAgents.Behaviours.Mediators {
    [DisallowMultipleComponent]
    [AddComponentMenu("EvoAgents/Behaviours/Mediator")]
    public class Mediator : MonoBehaviour, IMediator<Blackboard> {
        [SerializeField] private List<Blackboard> subscribers;
        public IReadOnlyCollection<Blackboard> Blackboards => subscribers;
        public Blackboard this[int index] => subscribers[index];

        public void AddSubscriber(Blackboard blackboard) {
            if (subscribers is null) subscribers = new List<Blackboard> { blackboard };
            else subscribers.Add(blackboard);
        }

        public void BroadcastMessage(IMessage<Blackboard> message) {
            if (subscribers is null) return;
            foreach (var subscriber in subscribers)
                subscriber.ReceiveMessage(message);
        }

        public void RemoveSubscriber(Blackboard blackboard) {
            if (subscribers is null) return;
            var index = subscribers.FindIndex((target) => target == blackboard);
            if(index > -1) subscribers.RemoveAt(index);
        }

        public void SendMessage(IMessage<Blackboard> message, Blackboard to) {
            if (subscribers is null || subscribers.Count == 0) return;
            Blackboard found = null;
            foreach (var subscriber in subscribers)
                if (subscriber.GUID == to.GUID) found = subscriber;
            if (found is null) return;
            found.ReceiveMessage(message);
        }
    }
}
