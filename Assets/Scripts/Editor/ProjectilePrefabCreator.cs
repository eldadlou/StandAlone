using UnityEngine;
using UnityEditor;
using MyGame.RuntimeSystems.Combat;

namespace MyGame.Editor
{
    /// <summary>
    /// Editor utility to create projectile prefabs with proper configuration
    /// </summary>
    public class ProjectilePrefabCreator : EditorWindow
    {
        [MenuItem("Tools/Combat/Create Projectile Prefab")]
        public static void CreateProjectilePrefab()
        {
            // Create the projectile GameObject
            GameObject projectile = CreateProjectileGameObject();
            
            // Create and apply material
            Material material = CreateProjectileMaterial();
            ApplyMaterialToProjectile(projectile, material);
            
            // Save as prefab
            SaveAsPrefab(projectile, "Projectile");
            
            // Clean up
            DestroyImmediate(projectile);
            
            Debug.Log("✅ Projectile prefab created successfully! Assign it to LightweightFireSystem.projectilePrefab");
        }
        
        [MenuItem("Tools/Combat/Create Tank Shell Prefab")]
        public static void CreateTankShellPrefab()
        {
            // Create the projectile GameObject
            GameObject projectile = CreateProjectileGameObject();
            
            // Configure for tank shell
            projectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            projectile.name = "TankShell";
            
            // Configure ProjectileBehavior for tank shell
            ProjectileBehavior behavior = projectile.GetComponent<ProjectileBehavior>();
            if (behavior != null)
            {
                // Use reflection to set private fields
                var speedField = typeof(ProjectileBehavior).GetField("speed", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var lifetimeField = typeof(ProjectileBehavior).GetField("lifetime", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (speedField != null) speedField.SetValue(behavior, 15f);
                if (lifetimeField != null) lifetimeField.SetValue(behavior, 15f);
            }
            
            // Create and apply material
            Material material = CreateTankShellMaterial();
            ApplyMaterialToProjectile(projectile, material);
            
            // Save as prefab
            SaveAsPrefab(projectile, "TankShell");
            
            // Clean up
            DestroyImmediate(projectile);
            
            Debug.Log("✅ Tank Shell prefab created successfully!");
        }
        
        [MenuItem("Tools/Combat/Create Machine Gun Bullet Prefab")]
        public static void CreateMachineGunBulletPrefab()
        {
            // Create the projectile GameObject
            GameObject projectile = CreateProjectileGameObject();
            
            // Configure for machine gun bullet
            projectile.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            projectile.name = "MachineGunBullet";
            
            // Configure ProjectileBehavior for machine gun bullet
            ProjectileBehavior behavior = projectile.GetComponent<ProjectileBehavior>();
            if (behavior != null)
            {
                // Use reflection to set private fields
                var speedField = typeof(ProjectileBehavior).GetField("speed", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var lifetimeField = typeof(ProjectileBehavior).GetField("lifetime", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (speedField != null) speedField.SetValue(behavior, 30f);
                if (lifetimeField != null) lifetimeField.SetValue(behavior, 8f);
            }
            
            // Create and apply material
            Material material = CreateMachineGunMaterial();
            ApplyMaterialToProjectile(projectile, material);
            
            // Save as prefab
            SaveAsPrefab(projectile, "MachineGunBullet");
            
            // Clean up
            DestroyImmediate(projectile);
            
            Debug.Log("✅ Machine Gun Bullet prefab created successfully!");
        }
        
        private static GameObject CreateProjectileGameObject()
        {
            // Create sphere
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Projectile";
            
            // Remove the default collider and add our own
            DestroyImmediate(projectile.GetComponent<Collider>());
            
            // Add Sphere Collider with trigger
            SphereCollider collider = projectile.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.1f;
            
            // Add ProjectileBehavior component
            projectile.AddComponent<ProjectileBehavior>();
            
            // Set default scale
            projectile.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            
            return projectile;
        }
        
        private static Material CreateProjectileMaterial()
        {
            // Create material
            Material material = new Material(Shader.Find("Standard"));
            material.name = "ProjectileMaterial";
            material.color = new Color(1f, 0.8f, 0f, 1f); // Bright yellow/orange
            
            // Save material
            string materialPath = "Assets/Materials/ProjectileMaterial.mat";
            CreateDirectoryIfNotExists("Assets/Materials");
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
            
            return material;
        }
        
        private static Material CreateTankShellMaterial()
        {
            // Create material
            Material material = new Material(Shader.Find("Standard"));
            material.name = "TankShellMaterial";
            material.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Dark gray
            material.SetFloat("_Metallic", 0.8f);
            material.SetFloat("_Smoothness", 0.2f);
            
            // Save material
            string materialPath = "Assets/Materials/TankShellMaterial.mat";
            CreateDirectoryIfNotExists("Assets/Materials");
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
            
            return material;
        }
        
        private static Material CreateMachineGunMaterial()
        {
            // Create material
            Material material = new Material(Shader.Find("Standard"));
            material.name = "MachineGunMaterial";
            material.color = new Color(1f, 1f, 0f, 1f); // Bright yellow
            material.SetFloat("_EmissionIntensity", 0.5f);
            
            // Save material
            string materialPath = "Assets/Materials/MachineGunMaterial.mat";
            CreateDirectoryIfNotExists("Assets/Materials");
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
            
            return material;
        }
        
        private static void ApplyMaterialToProjectile(GameObject projectile, Material material)
        {
            MeshRenderer renderer = projectile.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
        
        private static void SaveAsPrefab(GameObject projectile, string prefabName)
        {
            // Create Prefabs directory if it doesn't exist
            CreateDirectoryIfNotExists("Assets/Prefabs");
            
            // Save as prefab
            string prefabPath = $"Assets/Prefabs/{prefabName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(projectile, prefabPath);
            
            if (prefab != null)
            {
                Debug.Log($"✅ {prefabName} prefab saved to {prefabPath}");
                
                // Select the prefab in the Project window
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            else
            {
                Debug.LogError($"❌ Failed to save {prefabName} prefab");
            }
        }
        
        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentPath = System.IO.Path.GetDirectoryName(path);
                string folderName = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }
        
        [MenuItem("Tools/Combat/Setup Unit Projectiles")]
        public static void SetupUnitProjectiles()
        {
            // Find all combat units in the scene
            var combatUnits = FindObjectsOfType<MyGame.Core.Units.Combat.CombatUnit>();
            var vehicleCombatUnits = FindObjectsOfType<MyGame.Core.Units.Combat.VehicleCombatUnit>();
            
            if (combatUnits.Length == 0 && vehicleCombatUnits.Length == 0)
            {
                Debug.LogWarning("⚠️ No combat units found in scene! Add some units first.");
                return;
            }
            
            Debug.Log($"✅ Found {combatUnits.Length} CombatUnits and {vehicleCombatUnits.Length} VehicleCombatUnits");
            Debug.Log("📋 Next steps:");
            Debug.Log("1. Select each unit in the hierarchy");
            Debug.Log("2. In the inspector, expand the [Vehicle Weapons] section");
            Debug.Log("3. For each weapon: Assign turret transform and projectile prefab");
            Debug.Log("4. Configure damage, range, cooldown, and accuracy for each weapon");
            
            // Select the first unit if any exist
            if (vehicleCombatUnits.Length > 0)
            {
                Selection.activeGameObject = vehicleCombatUnits[0].gameObject;
                EditorGUIUtility.PingObject(vehicleCombatUnits[0].gameObject);
                Debug.Log($"✅ Selected {vehicleCombatUnits[0].name} - configure weapons in inspector");
            }
            else if (combatUnits.Length > 0)
            {
                Selection.activeGameObject = combatUnits[0].gameObject;
                EditorGUIUtility.PingObject(combatUnits[0].gameObject);
                Debug.Log($"✅ Selected {combatUnits[0].name} - assign projectile prefab in inspector");
            }
        }
        
        [MenuItem("Tools/Combat/Test Projectile System")]
        public static void TestProjectileSystem()
        {
            // Check for units in scene
            var units = FindObjectsOfType<MyGame.Core.Units.Unit>();
            if (units.Length == 0)
            {
                Debug.LogWarning("⚠️ No units found in scene! Add some units to test the projectile system.");
                return;
            }
            
            Debug.Log($"✅ Found {units.Length} units in scene");
            
            // Check combat units for projectile assignments
            var combatUnits = FindObjectsOfType<MyGame.Core.Units.Combat.CombatUnit>();
            var vehicleCombatUnits = FindObjectsOfType<MyGame.Core.Units.Combat.VehicleCombatUnit>();
            
            int unitsWithProjectiles = 0;
            int unitsWithoutProjectiles = 0;
            int weaponsWithoutProjectiles = 0;
            
            // Check regular combat units (non-vehicle)
            foreach (var combatUnit in combatUnits)
            {
                // Skip if it's a VehicleCombatUnit (we'll check those separately)
                if (combatUnit is MyGame.Core.Units.Combat.VehicleCombatUnit) continue;
                
                var projectilePrefabField = typeof(MyGame.Core.Units.Combat.CombatUnit).GetField("projectilePrefab", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (projectilePrefabField != null)
                {
                    GameObject projectilePrefab = (GameObject)projectilePrefabField.GetValue(combatUnit);
                    if (projectilePrefab != null)
                    {
                        unitsWithProjectiles++;
                    }
                    else
                    {
                        unitsWithoutProjectiles++;
                        Debug.LogWarning($"⚠️ {combatUnit.name} has no projectile prefab assigned!");
                    }
                }
            }
            
            // Check vehicle combat units - check each weapon mount
            foreach (var vehicleUnit in vehicleCombatUnits)
            {
                bool allWeaponsHaveProjectiles = true;
                
                for (int i = 0; i < vehicleUnit.WeaponCount; i++)
                {
                    var weapon = vehicleUnit.GetWeapon(i);
                    if (weapon != null && weapon.IsAvailable && !weapon.HasProjectile)
                    {
                        allWeaponsHaveProjectiles = false;
                        weaponsWithoutProjectiles++;
                        Debug.LogWarning($"⚠️ {vehicleUnit.name} weapon '{weapon.WeaponName}' has no projectile prefab assigned!");
                    }
                }
                
                if (allWeaponsHaveProjectiles && vehicleUnit.WeaponCount > 0)
                {
                    unitsWithProjectiles++;
                }
                else
                {
                    unitsWithoutProjectiles++;
                }
            }
            
            Debug.Log($"📊 Projectile Assignment Status:");
            Debug.Log($"   ✅ Units with projectiles: {unitsWithProjectiles}");
            Debug.Log($"   ❌ Units without projectiles: {unitsWithoutProjectiles}");
            
            if (unitsWithoutProjectiles > 0)
            {
                Debug.LogWarning("⚠️ Some units are missing projectile assignments! Use Tools/Combat/Setup Unit Projectiles to configure them.");
            }
            else
            {
                Debug.Log("✅ All units have projectile prefabs assigned!");
            }
            
            Debug.Log("🎮 To test: Press Play and units should start fighting with visible projectiles!");
        }
    }
}
