using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[AddComponentMenu("")]
[ExecuteInEditMode]
[RequireComponent(typeof(CinemachineStateDrivenCamera), typeof(Animator))]
public class CinemachineProvider : CinemachineExtension {
    public enum Tag {
        Player, Drone
    }
    [Serializable]
    public class CameraTarget {
        public enum CameraTag {
            PlayerCamera, DroneCamera
        }
        public string name;
        public Tag tag;
        public CameraTag cameraTag;
    }

    public List<CameraTarget> cameraTargets;
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
        CameraTarget target = cameraTargets.Find((target) => { return vcam.tag == target.cameraTag.ToString(); });
        if (target != null && vcam != null) {
            var obj = GameObject.FindGameObjectWithTag(target.tag.ToString());
            if (obj != null) {
                vcam.Follow = obj.transform;
                vcam.LookAt = obj.transform;
            }
        }
    }
}
