using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

using EvoAgents.Behaviours.Mediators;
using EvoAgents.World;
using Unity.Mathematics;
using Cysharp.Threading.Tasks;

namespace EvoAgents.Agents
{
    [RequireComponent(typeof(Mediator))]
    [AddComponentMenu("EvoAgents/Agents/Agent Manager")]
    public class AgentManager : MonoBehaviour {
        public Mediator mediator;
        public List<AgentType> agents;
        public Text score, totalScore;
        public Image gameOver;
        [Range(0f, 50f)] public float SpawnRadius = 10f;

        public List<GameObject> agentParentObjects = new List<GameObject>();
        private bool hasCleared = false;

        private static AgentManager agentManager;
        public static AgentManager Instance {
            get => agentManager;
        }

        public static void CatchRabbit(Rabbit.RabbitAgent rabbitAgent, Collider other) {
            var rabbits = agentManager.agents.Find((type) => type.name == "Rabbit");
            rabbits.nrSpawnedAgents--;
            Destroy(rabbitAgent.gameObject);

            if (other.CompareTag("Player") && !rabbitAgent.IsDead) {
                var score = System.Convert.ToInt32(agentManager.score.text) + 1;
                agentManager.score.text = score.ToString();
            }

            if (rabbits.nrSpawnedAgents == 0) {
                agentManager.gameOver.gameObject.SetActive(true);
            }
        }


        void Awake() {
            mediator ??= GetComponent<Mediator>();
            if (agentManager is null) agentManager = this;
        }

        public void SpawnAgent(string agentType, Vector3 point) {
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, SpawnRadius, -1)) {
                int agentIndex = agents.FindIndex((target) => target.name == agentType);
                var prefab = agents[agentIndex].agentVariants[Mathf.FloorToInt(UnityEngine.Random.Range(0f, agents[agentIndex].agentVariants.Count - 1f))];
                var agentObject = agentParentObjects.Find((target) => target.name == agentType);
                if (agentObject == null) {
                    agentObject = new GameObject(agents[agentIndex].name);
                    agentObject.transform.position = transform.position;
                    agentObject.transform.parent = transform;
                    agentParentObjects.Add(agentObject);
                }
                var spawnedAgent = GameObject.Instantiate(prefab, hit.position, Quaternion.identity, agentObject.transform);
                if (spawnedAgent.TryGetComponent(out Blackboard blackboard)) {
                    blackboard.SubscribeTo(mediator);
                    agents[agentIndex].nrSpawnedAgents++;
                }
                else Destroy(spawnedAgent);
            }
        }

        private void UnsubscribeAll() {
            foreach (Blackboard blackboard in mediator.Blackboards) {
                Destroy(blackboard.gameObject);
            }
        }

        public async void SpawnBatch(uint seed) {
            if (agents != null) {
                if (!hasCleared) {
                    UnsubscribeAll();
                    for (int agentIndex = 0; agentIndex < agents.Count; agentIndex++)
                        agents[agentIndex].nrSpawnedAgents = 0;
                    hasCleared = true;

                    score.text = "0";
                    gameOver.gameObject.SetActive(false);
                }

                foreach (var agentType in agents) {
                    var density = UnityEngine.Random.Range(3, agentType.initialAgentNumber);
                    var points = await PointSampling.GeneratePoints(seed, SpawnRadius, density, 15);

                    float2[] finalPoints = new float2[density];
                    points.CopyTo(finalPoints);
                    points.Dispose();
                    
                    foreach(var point in finalPoints) {
                        var p = new Vector3(point.x, 2f, point.y);
                        SpawnAgent(agentType.name, p);
                        await UniTask.WaitForEndOfFrame();
                    }
                    if (agentType.name == "Rabbit") totalScore.text = agentType.nrSpawnedAgents.ToString();
                }
                hasCleared = false;
            }
        }
    }

    [System.Serializable]
    public class AgentType {
        public string name;
        public List<GameObject> agentVariants;
        [Range(0f, 30f)] public int initialAgentNumber;
        public int nrSpawnedAgents;
    }
}
