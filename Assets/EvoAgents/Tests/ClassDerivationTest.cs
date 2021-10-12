using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using EvoAgents.Behaviours;
using EvoAgents.Behaviours.Registration;
using Cysharp.Threading.Tasks;
using System.Threading;
using EvoAgents.Behaviours.Actions;
using System;

public class ClassDerivationTest
{
    public class Sequencer : BehaviourTask
    {
        private bool test = false;
        private static async UniTask<BehaviourStatus> SequenceTask(BehaviourTask self, CancellationToken token = default) {
            var sequence = self as Sequencer;
            foreach (var task in self.Tasks) {
                if (token.IsCancellationRequested) return BehaviourStatus.Failure;

                task.SetToken(token);
                if (sequence.test) {
                    var status = await task;
                    sequence.test = false;

                    if (status == BehaviourStatus.Failure)
                        return BehaviourStatus.Failure;
                }
                return BehaviourStatus.Failure;
            }
            return BehaviourStatus.Success;
        }
        public Sequencer() : base(SequenceTask) { }
    }

    public class Conditional: BehaviourTask
    {
        private Func<bool> condition;
        private bool ExecuteTask(CancellationToken token) {
            Tasks[0].SetToken(token);
            return condition(); 
        }
        public static async UniTask<BehaviourStatus> ConditionalTask(BehaviourTask self, CancellationToken token)
        {
            var conditional = self as Conditional;
            if (conditional.ExecuteTask(token)) 
                return await self.Tasks[0];
            return BehaviourStatus.Success;
        }
        public Conditional(Func<bool> condition) : base(ConditionalTask) { this.condition = condition; }
    }

    [Test]
    public async void ClassDerivation()
    {
        var sequencer = new Sequencer();
        await sequencer.Register(BehaviourTask.Run((self, token) => {
            Debug.Log("Hello, world!");
            return UniTask.FromResult(BehaviourStatus.Success);
        }));
    }
}

public static class CustomSeqeunceExtension
{
    public static TaskGenerator CustomSequence(this TaskGenerator builder)
    {
        return builder.PushTask(new ClassDerivationTest.Sequencer());
    }

    public static TaskGenerator CustomActionWithFunc(this TaskGenerator builder, Func<float> func)
    {
        return builder.Action((self, token) =>
        {
            var valoare = func();
            if (valoare > 0.5f)
            {
                Debug.Log("Execute action");
                return UniTask.FromResult(BehaviourStatus.Success);
            }
            return UniTask.FromResult(BehaviourStatus.Failure);
        });
    }

    public static TaskGenerator CustomAction(this TaskGenerator builder, float attribute)
    {
        return builder.Action((self, token) =>
        {
            Debug.Log(attribute);
            return UniTask.FromResult(BehaviourStatus.Success);
        });
    }
}
