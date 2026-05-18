using UnityEngine;

namespace MyGame.Core.Units.Combat
{
    /// <summary>
    /// ScriptableObject that defines preset weapon configurations.
    /// Use these to quickly configure weapons on vehicles without manual setup.
    /// Create presets via Assets > Create > Combat > Weapon Preset
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon Preset", menuName = "Combat/Weapon Preset", order = 1)]
    public class WeaponPreset : ScriptableObject
    {
        [Header("Weapon Identity")]
        [SerializeField] private string weaponName = "Weapon";
        [SerializeField] private WeaponType weaponType = WeaponType.MachineGun;
        [TextArea(2, 4)]
        [SerializeField] private string description = "";
        
        [Header("Combat Stats")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float range = 15f;
        [SerializeField] private float cooldown = 0.5f;
        [SerializeField] private float accuracy = 80f;
        
        [Header("Turret Settings")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float rotationThreshold = 5f;
        
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 30f;
        
        [Header("Audio (Optional)")]
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private float fireSoundVolume = 1f;
        
        [Header("Visual Effects (Optional)")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private float muzzleFlashDuration = 0.1f;
        
        // Properties
        public string WeaponName => weaponName;
        public WeaponType Type => weaponType;
        public string Description => description;
        public float Damage => damage;
        public float Range => range;
        public float Cooldown => cooldown;
        public float Accuracy => accuracy;
        public float RotationSpeed => rotationSpeed;
        public float RotationThreshold => rotationThreshold;
        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;
        public AudioClip FireSound => fireSound;
        public float FireSoundVolume => fireSoundVolume;
        public GameObject MuzzleFlashPrefab => muzzleFlashPrefab;
        public float MuzzleFlashDuration => muzzleFlashDuration;
        
        /// <summary>
        /// Get a formatted description of this weapon preset
        /// </summary>
        public string GetStatsDescription()
        {
            return $"{weaponName} ({weaponType})\n" +
                   $"Damage: {damage} | Range: {range}m | Cooldown: {cooldown}s\n" +
                   $"Accuracy: {accuracy}% | Projectile Speed: {projectileSpeed}";
        }
    }
    
    /// <summary>
    /// Static class containing default values for each weapon type.
    /// Used by the editor to create preset ScriptableObjects.
    /// </summary>
    public static class WeaponDefaults
    {
        public static WeaponDefaultData GetDefaults(WeaponType type)
        {
            return type switch
            {
                WeaponType.MainCannon => new WeaponDefaultData
                {
                    WeaponName = "Main Cannon",
                    Description = "Heavy tank cannon. High damage, long range, slow fire rate.",
                    Damage = 50f,
                    Range = 25f,
                    Cooldown = 3f,
                    Accuracy = 85f,
                    RotationSpeed = 45f,
                    RotationThreshold = 3f,
                    ProjectileSpeed = 15f
                },
                
                WeaponType.MachineGun => new WeaponDefaultData
                {
                    WeaponName = "Machine Gun",
                    Description = "Fast-firing machine gun. Low damage, short range, rapid fire.",
                    Damage = 8f,
                    Range = 12f,
                    Cooldown = 0.15f,
                    Accuracy = 70f,
                    RotationSpeed = 120f,
                    RotationThreshold = 8f,
                    ProjectileSpeed = 40f
                },
                
                WeaponType.AutoCannon => new WeaponDefaultData
                {
                    WeaponName = "Auto Cannon",
                    Description = "Medium caliber automatic cannon. Balanced damage and fire rate.",
                    Damage = 20f,
                    Range = 18f,
                    Cooldown = 0.5f,
                    Accuracy = 75f,
                    RotationSpeed = 90f,
                    RotationThreshold = 5f,
                    ProjectileSpeed = 25f
                },
                
                WeaponType.Missile => new WeaponDefaultData
                {
                    WeaponName = "Guided Missile",
                    Description = "Lock-on guided missile. Very high damage, long range, slow reload.",
                    Damage = 80f,
                    Range = 40f,
                    Cooldown = 8f,
                    Accuracy = 95f,
                    RotationSpeed = 60f,
                    RotationThreshold = 10f,
                    ProjectileSpeed = 20f
                },
                
                WeaponType.Mortar => new WeaponDefaultData
                {
                    WeaponName = "Mortar",
                    Description = "Indirect fire weapon. Area damage, arc trajectory, slow fire rate.",
                    Damage = 40f,
                    Range = 30f,
                    Cooldown = 5f,
                    Accuracy = 60f,
                    RotationSpeed = 30f,
                    RotationThreshold = 15f,
                    ProjectileSpeed = 12f
                },
                
                WeaponType.Flamethrower => new WeaponDefaultData
                {
                    WeaponName = "Flamethrower",
                    Description = "Short range flame weapon. Continuous damage, very short range.",
                    Damage = 15f,
                    Range = 6f,
                    Cooldown = 0.1f,
                    Accuracy = 100f, // Always hits at short range
                    RotationSpeed = 150f,
                    RotationThreshold = 20f,
                    ProjectileSpeed = 8f
                },
                
                WeaponType.RocketPod => new WeaponDefaultData
                {
                    WeaponName = "Rocket Pod",
                    Description = "Multiple unguided rockets. Burst fire, medium accuracy.",
                    Damage = 25f,
                    Range = 20f,
                    Cooldown = 0.3f, // Fast burst, then long reload
                    Accuracy = 65f,
                    RotationSpeed = 75f,
                    RotationThreshold = 8f,
                    ProjectileSpeed = 35f
                },
                
                _ => new WeaponDefaultData
                {
                    WeaponName = "Unknown Weapon",
                    Description = "Default weapon configuration.",
                    Damage = 10f,
                    Range = 15f,
                    Cooldown = 1f,
                    Accuracy = 75f,
                    RotationSpeed = 90f,
                    RotationThreshold = 5f,
                    ProjectileSpeed = 20f
                }
            };
        }
    }
    
    /// <summary>
    /// Data structure holding default values for a weapon type
    /// </summary>
    public struct WeaponDefaultData
    {
        public string WeaponName;
        public string Description;
        public float Damage;
        public float Range;
        public float Cooldown;
        public float Accuracy;
        public float RotationSpeed;
        public float RotationThreshold;
        public float ProjectileSpeed;
    }
}
