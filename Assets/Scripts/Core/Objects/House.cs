using UnityEngine;
using MyGame.Core.Objects;

namespace MyGame.Core.Objects
{
    /// <summary>
    /// Example implementation of a destructible house using the DestructibleObject base class
    /// </summary>
    public class House : DestructibleObject
    {
        [Header("House Specific Settings")]
        [SerializeField] private HouseType houseType = HouseType.Residential;
        [SerializeField] private bool hasBasement = false;
        [SerializeField] private int floors = 1;
        
        [Header("House Resources")]
        [SerializeField] private GameObject[] windows;
        [SerializeField] private GameObject roof;
        [SerializeField] private GameObject door;
        
        [Header("Damage Stages")]
        [SerializeField] private GameObject[] damageStages; // Different visual states based on health
        [SerializeField] private float[] damageThresholds = { 0.75f, 0.5f, 0.25f }; // Health percentages for each stage
        
        private int currentDamageStage = 0;
        
        protected override void Awake()
        {
            // Set house-specific properties based on type
            ConfigureHouseByType();
            
            base.Awake();
        }
        
        protected override void Start()
        {
            base.Start();
            
            // Initialize visual state
            UpdateVisualDamage();
        }
        
        private void ConfigureHouseByType()
        {
            switch (houseType)
            {
                case HouseType.Residential:
                    // Small house - less health, smaller explosion
                    break;
                    
                case HouseType.Commercial:
                    // Larger building - more health, bigger explosion
                    break;
                    
                case HouseType.Industrial:
                    // Industrial building - high health, massive explosion
                    break;
                    
                case HouseType.Military:
                    // Military bunker - very high health, massive explosion with debris
                    break;
            }
        }
        
        public override void TakeDamage(float amount, MyGame.Core.Interfaces.IDestructible source = null)
        {
            float previousHealthPercent = Health / MaxHealth;
            
            base.TakeDamage(amount, source);
            
            float currentHealthPercent = Health / MaxHealth;
            
            // Check if we should update visual damage
            if (previousHealthPercent != currentHealthPercent)
            {
                UpdateVisualDamage();
                CheckForWindowBreaking(amount);
            }
        }
        
        private void UpdateVisualDamage()
        {
            float healthPercent = Health / MaxHealth;
            int newDamageStage = 0;
            
            // Determine which damage stage we should be in
            for (int i = 0; i < damageThresholds.Length; i++)
            {
                if (healthPercent <= damageThresholds[i])
                {
                    newDamageStage = i + 1;
                }
            }
            
            // Update visual state if it changed
            if (newDamageStage != currentDamageStage && newDamageStage < damageStages.Length)
            {
                // Disable previous stage
                if (currentDamageStage > 0 && currentDamageStage - 1 < damageStages.Length)
                {
                    damageStages[currentDamageStage - 1].SetActive(false);
                }
                
                // Enable new stage
                if (newDamageStage > 0)
                {
                    damageStages[newDamageStage - 1].SetActive(true);
                }
                
                currentDamageStage = newDamageStage;
                
                Debug.Log($"House {gameObject.name} entered damage stage {currentDamageStage}");
            }
        }
        
        private void CheckForWindowBreaking(float damage)
        {
            // Break windows randomly based on damage amount
            if (windows != null && damage > 10f)
            {
                float breakChance = Mathf.Clamp01(damage / 50f);
                
                foreach (var window in windows)
                {
                    if (window.activeInHierarchy && Random.value < breakChance)
                    {
                        // Disable window (simulate breaking)
                        window.SetActive(false);
                        
                        // Could spawn glass particle effects here
                        Debug.Log($"Window broken in {gameObject.name}!");
                    }
                }
            }
        }
        
        protected override void HandleDestruction()
        {
            Debug.Log($"House {gameObject.name} of type {houseType} destroyed!");
            
            // House-specific destruction logic
            BreakAllWindows();
            DestroyDoor();
            CollapseRoof();
            
            base.HandleDestruction();
        }
        
        private void BreakAllWindows()
        {
            if (windows != null)
            {
                foreach (var window in windows)
                {
                    if (window.activeInHierarchy)
                    {
                        window.SetActive(false);
                    }
                }
            }
        }
        
        private void DestroyDoor()
        {
            if (door != null)
            {
                door.SetActive(false);
            }
        }
        
        private void CollapseRoof()
        {
            if (roof != null)
            {
                // Could add physics simulation for roof collapse
                roof.SetActive(false);
            }
        }
        
        /// <summary>
        /// Check if house has any intact windows
        /// </summary>
        public bool HasIntactWindows()
        {
            if (windows == null) return false;
            
            foreach (var window in windows)
            {
                if (window.activeInHierarchy)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get the current damage stage (0 = no damage, higher = more damaged)
        /// </summary>
        public int GetDamageStage()
        {
            return currentDamageStage;
        }
        
        /// <summary>
        /// Force break a specific number of windows
        /// </summary>
        public void BreakWindows(int count)
        {
            if (windows == null) return;
            
            int broken = 0;
            foreach (var window in windows)
            {
                if (window.activeInHierarchy && broken < count)
                {
                    window.SetActive(false);
                    broken++;
                }
            }
        }
    }
    
    /// <summary>
    /// Different types of houses with different properties
    /// </summary>
    public enum HouseType
    {
        Residential,    // Small house
        Commercial,     // Office building, shop
        Industrial,     // Factory, warehouse
        Military        // Bunker, barracks
    }
}