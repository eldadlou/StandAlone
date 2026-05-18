using UnityEngine;
using UnityEditor;
using MyGame.Core.Units.Combat;
using System.IO;

namespace MyGame.Editor
{
    /// <summary>
    /// Editor utility to create weapon preset ScriptableObjects with default values
    /// </summary>
    public class WeaponPresetCreator : EditorWindow
    {
        private const string PRESETS_FOLDER = "Assets/Data/WeaponPresets";
        
        [MenuItem("Tools/Combat/Create All Weapon Presets")]
        public static void CreateAllWeaponPresets()
        {
            // Ensure directory exists
            CreateDirectoryIfNotExists(PRESETS_FOLDER);
            
            int created = 0;
            int skipped = 0;
            
            // Create preset for each weapon type
            foreach (WeaponType weaponType in System.Enum.GetValues(typeof(WeaponType)))
            {
                string presetPath = $"{PRESETS_FOLDER}/{weaponType}Preset.asset";
                
                // Skip if already exists
                if (AssetDatabase.LoadAssetAtPath<WeaponPreset>(presetPath) != null)
                {
                    Debug.Log($"⏭️ Skipping {weaponType} - preset already exists");
                    skipped++;
                    continue;
                }
                
                // Create the preset
                WeaponPreset preset = CreateWeaponPreset(weaponType);
                
                // Save as asset
                AssetDatabase.CreateAsset(preset, presetPath);
                Debug.Log($"✅ Created {weaponType} preset at {presetPath}");
                created++;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"🎯 Weapon Preset Creation Complete: {created} created, {skipped} skipped");
            Debug.Log($"📁 Presets saved to: {PRESETS_FOLDER}");
            
            // Select the folder in Project window
            var folder = AssetDatabase.LoadAssetAtPath<Object>(PRESETS_FOLDER);
            if (folder != null)
            {
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }
        }
        
        [MenuItem("Tools/Combat/Create Single Weapon Preset")]
        public static void ShowCreatePresetWindow()
        {
            WeaponPresetCreatorWindow window = EditorWindow.GetWindow<WeaponPresetCreatorWindow>("Create Weapon Preset");
            window.minSize = new Vector2(350, 200);
            window.Show();
        }
        
        private static WeaponPreset CreateWeaponPreset(WeaponType weaponType)
        {
            WeaponPreset preset = ScriptableObject.CreateInstance<WeaponPreset>();
            
            // Get default values for this weapon type
            WeaponDefaultData defaults = WeaponDefaults.GetDefaults(weaponType);
            
            // Use reflection to set private serialized fields
            var presetType = typeof(WeaponPreset);
            
            SetPrivateField(preset, "weaponName", defaults.WeaponName);
            SetPrivateField(preset, "weaponType", weaponType);
            SetPrivateField(preset, "description", defaults.Description);
            SetPrivateField(preset, "damage", defaults.Damage);
            SetPrivateField(preset, "range", defaults.Range);
            SetPrivateField(preset, "cooldown", defaults.Cooldown);
            SetPrivateField(preset, "accuracy", defaults.Accuracy);
            SetPrivateField(preset, "rotationSpeed", defaults.RotationSpeed);
            SetPrivateField(preset, "rotationThreshold", defaults.RotationThreshold);
            SetPrivateField(preset, "projectileSpeed", defaults.ProjectileSpeed);
            
            return preset;
        }
        
        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"Could not find field '{fieldName}' on {obj.GetType().Name}");
            }
        }
        
        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                // Split path and create each folder
                string[] folders = path.Split('/');
                string currentPath = folders[0]; // "Assets"
                
                for (int i = 1; i < folders.Length; i++)
                {
                    string nextPath = $"{currentPath}/{folders[i]}";
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                        Debug.Log($"📁 Created folder: {nextPath}");
                    }
                    currentPath = nextPath;
                }
            }
        }
    }
    
    /// <summary>
    /// Window for creating a single weapon preset with custom settings
    /// </summary>
    public class WeaponPresetCreatorWindow : EditorWindow
    {
        private WeaponType selectedType = WeaponType.MachineGun;
        private string customName = "";
        private bool useCustomName = false;
        
        private void OnGUI()
        {
            GUILayout.Label("Create Weapon Preset", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Weapon type selection
            selectedType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", selectedType);
            
            // Show default values preview
            WeaponDefaultData defaults = WeaponDefaults.GetDefaults(selectedType);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default Values Preview", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Name: {defaults.WeaponName}");
            EditorGUILayout.LabelField($"Damage: {defaults.Damage}");
            EditorGUILayout.LabelField($"Range: {defaults.Range}m");
            EditorGUILayout.LabelField($"Cooldown: {defaults.Cooldown}s");
            EditorGUILayout.LabelField($"Accuracy: {defaults.Accuracy}%");
            EditorGUILayout.LabelField($"Projectile Speed: {defaults.ProjectileSpeed}");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Custom name option
            useCustomName = EditorGUILayout.Toggle("Use Custom Name", useCustomName);
            if (useCustomName)
            {
                customName = EditorGUILayout.TextField("Custom Name", customName);
            }
            
            EditorGUILayout.Space();
            
            // Create button
            if (GUILayout.Button("Create Preset", GUILayout.Height(30)))
            {
                CreatePreset();
            }
            
            EditorGUILayout.Space();
            
            // Create all button
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            if (GUILayout.Button("Create ALL Weapon Presets", GUILayout.Height(25)))
            {
                WeaponPresetCreator.CreateAllWeaponPresets();
            }
        }
        
        private void CreatePreset()
        {
            string folderPath = "Assets/Data/WeaponPresets";
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data"))
                {
                    AssetDatabase.CreateFolder("Assets", "Data");
                }
                AssetDatabase.CreateFolder("Assets/Data", "WeaponPresets");
            }
            
            // Determine file name
            string fileName = useCustomName && !string.IsNullOrEmpty(customName) 
                ? customName 
                : $"{selectedType}Preset";
            
            string path = $"{folderPath}/{fileName}.asset";
            
            // Check if exists
            if (AssetDatabase.LoadAssetAtPath<WeaponPreset>(path) != null)
            {
                if (!EditorUtility.DisplayDialog("Overwrite?", 
                    $"A preset already exists at {path}. Overwrite?", "Yes", "No"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(path);
            }
            
            // Create the preset
            WeaponPreset preset = ScriptableObject.CreateInstance<WeaponPreset>();
            WeaponDefaultData defaults = WeaponDefaults.GetDefaults(selectedType);
            
            // Set fields via reflection
            SetField(preset, "weaponName", useCustomName ? customName : defaults.WeaponName);
            SetField(preset, "weaponType", selectedType);
            SetField(preset, "description", defaults.Description);
            SetField(preset, "damage", defaults.Damage);
            SetField(preset, "range", defaults.Range);
            SetField(preset, "cooldown", defaults.Cooldown);
            SetField(preset, "accuracy", defaults.Accuracy);
            SetField(preset, "rotationSpeed", defaults.RotationSpeed);
            SetField(preset, "rotationThreshold", defaults.RotationThreshold);
            SetField(preset, "projectileSpeed", defaults.ProjectileSpeed);
            
            // Save
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            
            // Select the created asset
            Selection.activeObject = preset;
            EditorGUIUtility.PingObject(preset);
            
            Debug.Log($"✅ Created weapon preset: {path}");
        }
        
        private void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
