using NUnit.Framework;
using UnityEngine;

using EvoAgents.Behaviours;
using EvoAgents.Behaviours.Registration;
using Cysharp.Threading.Tasks;
using System.Threading;


public class BehaviourTaskTests
{
    [Test]
    public async void TaskCreationTest()
    {
        var task = BehaviourTask.Run((self, token) => {
            Debug.LogWarning("Main task running.");
            return UniTask.FromResult(BehaviourStatus.Success);
        });
        Assert.IsNotNull(task, "Task creation failed.");

        var status = await task;
        Assert.AreEqual(BehaviourStatus.Success, status, "Values are not equals.");
    }

    [Test]
    public async void TaskRegistrationTest()
    {
        var task = BehaviourTask.Run(async (self, token) => {
            Debug.LogWarning("Main task running.");
            Assert.GreaterOrEqual(1, self.Tasks.Count, "There is no task in the list");
            var status = await self.Tasks[0];
            Assert.AreEqual(BehaviourStatus.Success, status);
            return BehaviourStatus.Success;
        });
        Assert.IsNotNull(task, "Task creation failed.");

        var status = await task.Register(BehaviourTask.Run((self, token) => {
            Debug.LogWarning("Task has been runned.");
            return UniTask.FromResult(BehaviourStatus.Success);
        }));
        Assert.AreEqual(BehaviourStatus.Success, status, "Values are not equals.");
    }

    [Test]
    public async void TaskCancellationTest()
    {
        var tokenSource = new CancellationTokenSource();
        var task = BehaviourTask.Run(async (self, token) => {
            Debug.LogWarning("Main task running.");
            self.Tasks[0].SetToken(token);
            var status = await self.Tasks[0];
            Assert.AreEqual(BehaviourStatus.Failure, status);
            return BehaviourStatus.Success;
        }, tokenSource.Token);
        Assert.IsNotNull(task, "Task creation failed.");

        tokenSource.Cancel();
        var status = await task.Register(BehaviourTask.Run((self, token) => {
            if (token.IsCancellationRequested) {
                Debug.Log("Task has been cancelled.");
                return UniTask.FromResult(BehaviourStatus.Failure);
            }
            Debug.LogWarning("Task has been runned.");
            return UniTask.FromResult(BehaviourStatus.Success);
        }));
        Assert.AreEqual(BehaviourStatus.Success, status, "Values are not equals.");
    }
}

