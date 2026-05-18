using UnityEngine;

namespace MyGame.Core.Units.Combat
{
    /// <summary>
    /// Possible results of an accuracy check
    /// </summary>
    public enum HitResult
    {
        Miss,           // Complete miss - no damage
        PartialHit,     // Partial hit - reduced damage
        FullHit         // Perfect hit - full damage
    }
    
    /// <summary>
    /// Information about accuracy for debugging and UI display
    /// </summary>
    [System.Serializable]
    public struct AccuracyInfo
    {
        public float BaseAccuracy;      // Base accuracy of the weapon
        public float CurrentAccuracy;   // Current accuracy after modifiers
        public bool IsMoving;           // Whether the unit is currently moving
        public float MovementPenalty;   // Accuracy penalty from movement
        public string WeaponType;       // Type of weapon being used
        
        public override string ToString()
        {
            return $"Accuracy: {CurrentAccuracy:F1}% (Base: {BaseAccuracy:F1}%, Moving: {IsMoving}, Penalty: {MovementPenalty:F1}%, Weapon: {WeaponType})";
        }
    }
    
    /// <summary>
    /// Base interface for units that can perform accuracy checks
    /// </summary>
    public interface IAccurateUnit
    {
        float CalculateCurrentAccuracy();
        HitResult PerformAccuracyCheck();
        float CalculateDamage(HitResult hitResult);
        AccuracyInfo GetAccuracyInfo();
    }
}
