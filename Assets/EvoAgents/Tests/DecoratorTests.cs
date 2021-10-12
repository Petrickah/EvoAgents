using NUnit.Framework;
using UnityEngine;

using System;
using System.Threading;

using Cysharp.Threading.Tasks;
using EvoAgents.Behaviours;
using EvoAgents.Behaviours.Composites;
using EvoAgents.Behaviours.Actions;
using EvoAgents.Behaviours.Decorators;

public class DecoratorTests
{
    [Test]
    public async void RepeaterDecoratorTest() {
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromSeconds(0.25f));
        var task = TaskGenerator.Begin()
            .RepeatForever()
                .Selector()
                    .Action((self, token) => {
                        return UniTask.FromResult(BehaviourStatus.Failure);
                    })
                    .Action((self, token) => {
                        Debug.Log("This task is running");
                        return UniTask.FromResult(BehaviourStatus.Success);
                    })
                .End()
            .Generate();
        task.SetToken(tokenSource.Token);
        var status = await task;
        Assert.AreEqual(BehaviourStatus.Failure, status);
    }

    [Test]
    public async void ConditionalDecoratorTest() {
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromSeconds(0.25f));
        var task = TaskGenerator.Begin()
            .RepeatForever()
                .Selector()
                    .Conditional(() => false)
                        .Action((self, token) => {
                            Assert.Fail("This task dosen't run");
                            return UniTask.FromResult(BehaviourStatus.Success);
                        })
                    .End()
                    .Action((self, token) => {
                        Debug.Log("This task is running");
                        return UniTask.FromResult(BehaviourStatus.Success);
                    })
                .End()
            .Generate();
        task.SetToken(tokenSource.Token);
        var status = await task;
        Assert.AreEqual(BehaviourStatus.Failure, status);
    }

}
