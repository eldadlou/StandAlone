# 🔧 Dependency Injection Setup Guide

## 🎯 **Overview**

This guide explains how to use the new dependency injection system instead of `FindObjectOfType` calls. The system provides better performance, testability, and maintainability.

## 🚀 **Quick Setup**

### **1. Add SystemInitializer to Your Scene**

1. **Create a GameObject** in your scene named "SystemManager"
2. **Add the SystemInitializer component** to it
3. **Configure the settings**:
   - ✅ `Auto Setup On Awake`: Automatically initialize systems when the scene loads
   - ✅ `Create Missing Systems`: Automatically create systems if they don't exist

### **2. System Prefabs (Optional)**

If you have custom system prefabs, assign them in the SystemInitializer:
- `Fire System Prefab`: Custom LightweightFireSystem prefab
- `Movement System Prefab`: Custom MovementSystem prefab  
- `UI System Prefab`: Custom UISystem prefab

## 📋 **How to Use**

### **Getting Systems**

Instead of using `FindObjectOfType`, use the dependency injection system:

```csharp
// ❌ Old way (slow and unreliable)
LightweightFireSystem fireSystem = FindObjectOfType<LightweightFireSystem>();

// ✅ New way (fast and reliable)
LightweightFireSystem fireSystem = SystemInitializer.GetSystem<LightweightFireSystem>();
```

### **Checking if Systems Exist**

```csharp
// Check if a system is available
if (SystemInitializer.HasSystem<LightweightFireSystem>())
{
    var fireSystem = SystemInitializer.GetSystem<LightweightFireSystem>();
    // Use the system
}
```

### **Manual Registration**

```csharp
// Register a custom system
MyCustomSystem customSystem = new MyCustomSystem();
SystemInitializer.RegisterSystem(customSystem);

// Later, retrieve it
MyCustomSystem retrieved = SystemInitializer.GetSystem<MyCustomSystem>();
```

## 🔄 **System Lifecycle**

### **Automatic Initialization**

When `SystemInitializer` runs, it:

1. **Finds existing systems** in the scene
2. **Creates missing systems** (if enabled)
3. **Registers all systems** with the dependency container
4. **Initializes the unit registry**
5. **Sets up event systems**

### **Manual Initialization**

```csharp
// Get the SystemInitializer component
SystemInitializer initializer = FindObjectOfType<SystemInitializer>();

// Manually initialize systems
initializer.InitializeSystems();
```

## 🧪 **Testing Support**

### **Clear All Systems**

```csharp
// Clear all systems (useful for unit tests)
SystemInitializer.ClearAllSystems();
```

### **Test Setup**

```csharp
[Test]
public void TestCombatSystem()
{
    // Clear any existing systems
    SystemInitializer.ClearAllSystems();
    
    // Create and register test systems
    var testFireSystem = new MockFireSystem();
    SystemInitializer.RegisterSystem(testFireSystem);
    
    // Run your test
    // ...
}
```

## 📊 **Performance Benefits**

### **Before (FindObjectOfType)**
- ❌ Searches entire scene every call
- ❌ O(n) complexity for each lookup
- ❌ Can cause frame drops with many objects
- ❌ No caching

### **After (Dependency Injection)**
- ✅ O(1) lookup from registry
- ✅ Cached references
- ✅ No scene traversal
- ✅ Predictable performance

## 🔧 **Migration Guide**

### **Step 1: Replace FindObjectOfType Calls**

```csharp
// Find all instances of FindObjectOfType in your code
// Replace with SystemInitializer.GetSystem<T>()
```

### **Step 2: Add Null Checks**

```csharp
var system = SystemInitializer.GetSystem<MySystem>();
if (system != null)
{
    // Use the system
}
else
{
    Debug.LogWarning("MySystem not found in dependency container");
}
```

### **Step 3: Update Unit Tests**

```csharp
// Old test setup
var fireSystem = FindObjectOfType<LightweightFireSystem>();

// New test setup
SystemInitializer.ClearAllSystems();
var mockFireSystem = new MockFireSystem();
SystemInitializer.RegisterSystem(mockFireSystem);
```

## 🎮 **Example Usage**

### **ProjectileBehavior**

```csharp
private void OnHitTarget()
{
    // Get fire system from dependency container
    LightweightFireSystem fireSystem = SystemInitializer.GetSystem<LightweightFireSystem>();
    if (fireSystem != null)
    {
        fireSystem.OnProjectileHit(target, transform.position, attacker);
    }
}
```

### **Unit Movement**

```csharp
public virtual void MoveTo(Vector3 destination)
{
    if (unitData != null)
    {
        unitData.SetMoving(destination);
        var movementSystem = SystemInitializer.GetSystem<MovementSystem>();
        movementSystem?.RegisterUnit(this);
    }
}
```

### **Combat System**

```csharp
public void ProcessAttack(IUnit attacker, IUnit target)
{
    var fireSystem = SystemInitializer.GetSystem<LightweightFireSystem>();
    if (fireSystem != null)
    {
        fireSystem.ProcessAttack(attacker, target);
    }
}
```

## 🚨 **Common Issues**

### **System Not Found**

If you get "System not found" warnings:

1. **Check SystemInitializer**: Make sure it's in your scene
2. **Check Auto Setup**: Ensure `Auto Setup On Awake` is enabled
3. **Check Create Missing**: Ensure `Create Missing Systems` is enabled
4. **Manual Registration**: Register the system manually if needed

### **Performance Issues**

If you experience performance problems:

1. **Avoid frequent calls**: Cache system references
2. **Use HasSystem()**: Check if system exists before getting it
3. **Batch operations**: Group system operations together

## 📈 **Best Practices**

1. **Cache References**: Store system references in fields when possible
2. **Null Checks**: Always check if systems exist before using them
3. **Error Handling**: Provide fallback behavior when systems are missing
4. **Testing**: Use the testing utilities for unit tests
5. **Documentation**: Document which systems your components depend on

## 🔮 **Future Enhancements**

- **Async Initialization**: Load systems asynchronously
- **System Dependencies**: Define system dependencies
- **Hot Reloading**: Reload systems during development
- **Profiling**: Built-in performance monitoring
- **Configuration**: JSON-based system configuration

---

## ✅ **Summary**

The dependency injection system provides:

- **Better Performance**: O(1) lookups instead of O(n) searches
- **Better Testability**: Easy to mock and test systems
- **Better Maintainability**: Clear dependencies and initialization
- **Better Reliability**: No more "system not found" errors
- **Better Scalability**: Handles large scenes efficiently

Start using `SystemInitializer.GetSystem<T>()` instead of `FindObjectOfType<T>()` today!
