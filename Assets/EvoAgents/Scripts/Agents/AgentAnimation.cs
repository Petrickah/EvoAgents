using System.Collections.Generic;
using UnityEngine;

namespace EvoAgents.Agents
{
    [DisallowMultipleComponent]
    [AddComponentMenu("EvoAgents/Agents/Agent Animation")]
    public class AgentAnimation : MonoBehaviour {
        [Header("Animation Settings")]
        public Animator animator;
        public bool IsPlayingAnimation;
        public bool IsDead;

        [Header("Animation Lists")]
        public List<AnimationType> WalkingAnimations;
        public List<AnimationType> IdleAnimations;
        public List<AnimationType> DeadAnimations;

        private void SetAnimationAttribute(in AnimationType animation, ref bool value) {
            if(animator != null)
                animator.SetBool(animation.ParameterName, !value);
            value = !value;
        }
        private void StopAnimations(List<AnimationType> animations) {
            animations.ForEach((targetAnimation) => {
                if (targetAnimation.IsPlaying)
                    SetAnimationAttribute(in targetAnimation, ref targetAnimation.IsPlaying);
            });
        }

        private AnimationType FindParticularAnimation(string animationName, in List<AnimationType> animations) {
            if (animations == null || animations.Count == 0) return new AnimationType { Name = "Unknown" };
            return animations.Find((targetAnimation) => {
                return targetAnimation.Name == animationName;
            });
        }

        public void StopAnimations() {
            if(IsPlayingAnimation && !IsDead) {
                StopAnimations(WalkingAnimations);
                StopAnimations(IdleAnimations);
                this.IsPlayingAnimation = false;
            }
        }

        public void PlayAnimation(string animationName, AnimationCategory category) {
            AnimationType animation = new AnimationType { Name = "Unknown" };
            switch(category) {
                case AnimationCategory.IdleAnimation:
                    animation = FindParticularAnimation(animationName, IdleAnimations);
                    break;
                case AnimationCategory.WalkingAnimation:
                    animation = FindParticularAnimation(animationName, WalkingAnimations);
                    break;
                case AnimationCategory.DeadAnimation:
                    animation = DeadAnimations[Random.Range(0, DeadAnimations.Count)];
                    break;
            }
            StopAnimations();
            if (animation.Name != "Unknown" && !animation.IsPlaying && !IsPlayingAnimation) {
                SetAnimationAttribute(in animation, ref animation.IsPlaying);
                IsPlayingAnimation = true;
            }
            if (category == AnimationCategory.DeadAnimation && !IsDead) IsDead = true;
        }
    }

    public enum AnimationCategory {
        IdleAnimation, WalkingAnimation, DeadAnimation
    }

    [System.Serializable]
    public class AnimationType {
        public string Name;
        public string ParameterName;
        public bool IsPlaying;
    }
}
