using NUnit.Framework;
using UnityEngine;

using Cysharp.Threading.Tasks;
using EvoAgents.Behaviours;
using EvoAgents.Behaviours.Composites;
using EvoAgents.Behaviours.Actions;

public class CompositesTests
{
    [Test]
    public async void SequencerTest() {
        var status = await TaskGenerator.Begin()
            .Sequencer()
                .Action(BehaviourTask.Run((self, token) => {
                    Debug.LogWarning("This task runs first and fails");
                    return UniTask.FromResult(BehaviourStatus.Failure);
                }))
                .Action(BehaviourTask.Run((self, token) => {
                    Debug.LogAssertion("This task fails the test");
                    return UniTask.FromResult(BehaviourStatus.Success);
                }))
            .Generate();
        Assert.AreEqual(BehaviourStatus.Failure, status);
    }

    [Test]
    public async void SelectorTest()
    {
        var status = await TaskGenerator.Begin()
            .Selector()
                .Action(BehaviourTask.Run((self, token) => {
                    Debug.LogWarning("This task runs first and fails");
                    return UniTask.FromResult(BehaviourStatus.Failure);
                }))
                .Action(BehaviourTask.Run((self, token) => {
                    Debug.LogWarning("This task runs second and succeeds");
                    return UniTask.FromResult(BehaviourStatus.Success);
                }))
            .Generate();
        Assert.AreEqual(BehaviourStatus.Success, status);
    }

    [Test]
    public async void ParallelTest()
    {
        var status = await TaskGenerator.Begin()
            .Parallel()
                .Action(BehaviourTask.Run((self, token) => {
                    Debug.LogWarning("This task runs first in parallel");
                    return UniTask.FromResult(BehaviourStatus.Failure);
                }))
                .Action(BehaviourTask.Run((self, token) => {
                    Debug.LogWarning("This task runs second in parallel");
                    return UniTask.FromResult(BehaviourStatus.Success);
                }))
            .Generate();
        Assert.AreEqual(BehaviourStatus.Success, status);
    }
}
