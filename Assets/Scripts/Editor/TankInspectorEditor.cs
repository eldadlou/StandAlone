using UnityEngine;
using UnityEditor;
using MyGame.Core;
using MyGame.Core.Units;
using MyGame.Core.Units.Combat;
using MyGame.Game;

namespace MyGame.Editor
{
    /// <summary>
    /// Custom editor for Tank components to display current stats and UnitData
    /// </summary>
    [CustomEditor(typeof(Tank))]
    public class TankInspectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();
            
            Tank tank = (Tank)target;
            
            // Add some space
            EditorGUILayout.Space();
            
            // Display current UnitData stats
            if (tank.GetUnitData() != null)
            {
                EditorGUILayout.LabelField("Current Unit Data", EditorStyles.boldLabel);
                
                var unitData = tank.GetUnitData();
                
                EditorGUILayout.BeginVertical("box");
                
                // Health with color coding
                Color originalColor = GUI.color;
                float healthPercent = unitData.Health / 150f; // Assuming max health is 150
                GUI.color = Color.Lerp(Color.red, Color.green, healthPercent);
                EditorGUILayout.LabelField($"Health: {unitData.Health:F1}/150");
                GUI.color = originalColor;
                
                EditorGUILayout.LabelField($"Speed: {unitData.Speed:F1}");
                EditorGUILayout.LabelField($"Team: {unitData.Owner?.Team ?? Team.None}");
                EditorGUILayout.LabelField($"Owner: {unitData.Owner?.Name ?? "None"}");
                
                EditorGUILayout.EndVertical();
                
                // Display combat info
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Combat Information", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
                
                var combatUnit = tank.GetComponent<VehicleCombatUnit>();
                if (combatUnit != null)
                {
                    // Display all weapons
                    for (int i = 0; i < combatUnit.WeaponCount; i++)
                    {
                        var weapon = combatUnit.GetWeapon(i);
                        if (weapon != null)
                        {
                            EditorGUILayout.LabelField($"{weapon.WeaponName} ({weapon.Type}):");
                            EditorGUILayout.LabelField($"  Damage: {weapon.Damage}");
                            EditorGUILayout.LabelField($"  Range: {weapon.Range}m");
                            EditorGUILayout.LabelField($"  Cooldown: {weapon.Cooldown}s");
                            EditorGUILayout.LabelField($"  Accuracy: {weapon.Accuracy}%");
                            EditorGUILayout.LabelField($"  Available: {weapon.IsAvailable}");
                            EditorGUILayout.LabelField($"  Has Projectile: {weapon.HasProjectile}");
                            if (i < combatUnit.WeaponCount - 1) EditorGUILayout.Space();
                        }
                    }
                    
                    if (combatUnit.WeaponCount == 0)
                    {
                        EditorGUILayout.LabelField("No weapons configured!", EditorStyles.boldLabel);
                    }
                    
                    // Show current weapon selection
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Current Active Weapon:");
                    EditorGUILayout.LabelField($"  Damage: {combatUnit.AttackDamage}");
                    EditorGUILayout.LabelField($"  Range: {combatUnit.AttackRange}m");
                    EditorGUILayout.LabelField($"  Cooldown: {combatUnit.AttackCooldown}s");
                }
                else
                {
                    EditorGUILayout.LabelField("No VehicleCombatUnit component found!", EditorStyles.boldLabel);
                }
                
                EditorGUILayout.EndVertical();
                
                // Display combat state
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Combat State", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
                
                if (combatUnit != null)
                {
                    EditorGUILayout.LabelField($"In Combat: {combatUnit.IsInCombat}");
                    EditorGUILayout.LabelField($"Current Target: {combatUnit.CurrentTarget?.Name ?? "None"}");
                    EditorGUILayout.LabelField($"Target In Range: {combatUnit.IsTargetInRange}");
                    EditorGUILayout.LabelField($"Gun Facing Target: {combatUnit.IsGunFacingTarget}");
                    EditorGUILayout.LabelField($"Detection Radius: {combatUnit.DetectionRadius}m");
                }
                
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("UnitData not found!", EditorStyles.boldLabel);
            }
            
            // Add refresh button
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Stats"))
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
