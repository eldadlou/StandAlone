using UnityEngine;
using System;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Services;

namespace MyGame.RuntimeSystems.Audio
{
    /// <summary>
    /// Handles audio responses to unit events
    /// </summary>
    public class AudioSystem : MonoBehaviour, ISelectionAudioFeedback
    {
        [Header("Audio Sources")]
        public AudioSource unitAudioSource;
        public AudioSource ambientAudioSource;

        [Header("Unit Sound Effects")]
        public AudioClip unitDeathSound;
        public AudioClip unitAttackSound;
        public AudioClip unitMoveSound;
        public AudioClip unitSelectionSound;

        [Header("Animation Sound Effects")]
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip upgradeSound;

        private void Awake()
        {
            var container = DependencyContainer.Instance;
            container.Register(this);
            container.RegisterAs<ISelectionAudioFeedback>(this);
        }

        public void SubscribeToUnit(IUnit unit)
        {
            if (unit == null) return;

            unit.OnDeath += HandleUnitDeath;
            unit.OnAttack += HandleUnitAttack;
            unit.OnMove += HandleUnitMove;
            unit.OnAnimationEvent += HandleAnimationEvent;
        }

        public void UnsubscribeFromUnit(IUnit unit)
        {
            if (unit == null) return;

            unit.OnDeath -= HandleUnitDeath;
            unit.OnAttack -= HandleUnitAttack;
            unit.OnMove -= HandleUnitMove;
            unit.OnAnimationEvent -= HandleAnimationEvent;
        }

        private void HandleUnitDeath(IUnit unit)
        {
            if (unitDeathSound != null && unitAudioSource != null)
            {
                unitAudioSource.PlayOneShot(unitDeathSound);
                Debug.Log($"🔊 Played death sound for {unit.Type}");
            }
        }

        private void HandleUnitAttack(IUnit attacker, IUnit target)
        {
            if (unitAttackSound != null && unitAudioSource != null)
            {
                unitAudioSource.PlayOneShot(unitAttackSound);
                Debug.Log($"🔊 Played attack sound for {attacker.Type}");
            }
        }

        private void HandleUnitMove(IUnit unit, Vector3 destination)
        {
            if (unitMoveSound != null && unitAudioSource != null)
            {
                unitAudioSource.PlayOneShot(unitMoveSound);
                Debug.Log($"🔊 Played move sound for {unit.Type}");
            }
        }

        private void HandleAnimationEvent(string eventName)
        {
            AudioClip clipToPlay = null;

            switch (eventName.ToLower())
            {
                case "fire":
                    clipToPlay = fireSound;
                    break;
                case "reload":
                    clipToPlay = reloadSound;
                    break;
                case "upgrade":
                    clipToPlay = upgradeSound;
                    break;
            }

            if (clipToPlay != null && unitAudioSource != null)
            {
                unitAudioSource.PlayOneShot(clipToPlay);
                Debug.Log($"🔊 Played animation sound: {eventName}");
            }
        }

        public void PlaySelectionSound()
        {
            if (unitSelectionSound != null && unitAudioSource != null)
            {
                unitAudioSource.PlayOneShot(unitSelectionSound);
                Debug.Log("🔊 Played selection sound");
            }
        }
    }
} 