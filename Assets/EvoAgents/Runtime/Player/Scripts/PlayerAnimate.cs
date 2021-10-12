using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimate : MonoBehaviour
{
    public event Action<PlayerAnimate> PlayerAnimationChecked;
    public enum AnimationState {
        idle, isRunning, isWalking
    }
    public Animator animator;
    public AnimationState currentState;
    public List<AnimationState> posibleStates;
    private void Awake() {
        if(currentState == AnimationState.idle) {
            DisableAllAnimations();
        }
    }
    private void DisableAllAnimations() {
        foreach (var posibleState in posibleStates) {
            DisableAnimation(posibleState);
        }
    }
    private void DisableAnimation(AnimationState animation) {
        animator.SetBool(animation.ToString(), false);
    }
    private void EnableAnimation(AnimationState animation) {
        DisableAllAnimations();
        animator.SetBool(animation.ToString(), true);
    }
    public void SetCurrentAnimation(AnimationState animation) {
        currentState = animation;
    }
    private void Update() {
        switch(currentState) {
            case AnimationState.idle: DisableAllAnimations(); break;
            default: EnableAnimation(currentState); break;
        }
        PlayerAnimationChecked?.Invoke(this);
    }
}
