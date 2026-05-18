# Presentation System Setup Guide

This guide will help you set up the presentation scripts (UnitVisual and UnitVisualCoordinator) to properly display selection circles and destination markers.

## Required Components

### 1. Unit Prefab Setup

Each unit prefab should have these components:
- `Unit` (Tank, Truck, etc.)
- `UnitVisualCoordinator`
- `UnitVisual`

### 2. UnitVisual Component Configuration

The `UnitVisual` component requires two prefab references:

#### Selection Circle Prefab
- Create a simple GameObject with a visual representation (e.g., a ring, circle, or highlight)
- Add a SpriteRenderer or MeshRenderer with appropriate material
- Make it a prefab
- Assign it to the `selectionCirclePrefab` field in UnitVisual

#### Destination Marker Prefab
- Create a simple GameObject with a visual representation (e.g., an arrow, flag, or target)
- Add a SpriteRenderer or MeshRenderer with appropriate material
- Make it a prefab
- Assign it to the `destinationMarkerPrefab` field in UnitVisual

## Setup Steps

### Step 1: Create Visual Prefabs

1. **Selection Circle Prefab:**
   ```
   - Create Empty GameObject
   - Add SpriteRenderer or MeshRenderer
   - Set appropriate visual (circle, ring, etc.)
   - Position at (0, 0.1, 0) to appear above ground
   - Make it a prefab
   ```

2. **Destination Marker Prefab:**
   ```
   - Create Empty GameObject
   - Add SpriteRenderer or MeshRenderer
   - Set appropriate visual (arrow, flag, etc.)
   - Position at (0, 0.1, 0) to appear above ground
   - Make it a prefab
   ```

### Step 2: Configure Unit Prefabs

For each unit prefab (Tank, Truck, etc.):

1. **Add UnitVisualCoordinator component**
   - This component will be automatically configured

2. **Add UnitVisual component**
   - Assign the Selection Circle Prefab to `selectionCirclePrefab`
   - Assign the Destination Marker Prefab to `destinationMarkerPrefab`

### Step 3: Test the Setup

1. **Use the UnitSetupTest script:**
   - Press `S` to test selection
   - Press `D` to test destination marker
   - Check console for debug messages

2. **Manual Testing:**
   - Select units in the scene
   - Move units to see destination markers
   - Verify selection circles appear/disappear

## How It Works

### Initialization Process
1. **Awake()**: Components are found and cached
2. **Start()**: Attempts to initialize if unit data is available
3. **AssignToTeam()**: Triggers initialization when unit is assigned to a team
4. **OnSelectionChanged()**: Triggers initialization if not already initialized

### Visual Object Management
- **Selection Circle**: Created at origin, positioned at unit location when selected
- **Destination Marker**: Created at origin, positioned at destination when unit moves
- Both objects are properly cleaned up when the unit is destroyed

## Troubleshooting

### Issue: Selection circles not appearing
**Possible causes:**
- Selection circle prefab not assigned
- UnitVisual component missing
- UnitVisualCoordinator not initialized
- Unit not assigned to a team

**Solutions:**
- Check console for debug messages
- Verify prefab assignments in UnitVisual
- Ensure all components are present on unit prefab
- Make sure unit is assigned to a team via `AssignToTeam()`

### Issue: Destination markers not appearing
**Possible causes:**
- Destination marker prefab not assigned
- Unit not calling MoveTo() method
- UnitVisualCoordinator not initialized

**Solutions:**
- Check console for debug messages
- Verify prefab assignments in UnitVisual
- Test with UnitSetupTest script

### Issue: Visual components not initializing
**Possible causes:**
- Missing Unit component
- Unit data not available yet
- Missing dependencies

**Solutions:**
- Check console for error messages
- Verify component order on prefab
- Ensure unit is assigned to a team
- Check that UnitData is properly created

## Debug Information

The system includes extensive debug logging. Check the console for:
- Component initialization messages
- Prefab instantiation confirmations
- Selection and movement events
- Error messages for missing components
- Initialization retry attempts

## Key Points

1. **Prefab References:** Always assign prefabs, not scene objects
2. **Component Order:** UnitVisualCoordinator and UnitVisual should be on the same GameObject as Unit
3. **Initialization:** Components initialize when unit data becomes available
4. **Team Assignment:** Units must be assigned to a team for proper initialization
5. **Testing:** Use the provided test script to verify functionality

## Example Prefab Hierarchy

```
UnitPrefab (Tank/Truck)
├── Unit (Tank/Truck component)
├── UnitVisualCoordinator
├── UnitVisual
│   ├── selectionCirclePrefab (assigned)
│   └── destinationMarkerPrefab (assigned)
└── [Other components...]
```

## Recent Fixes

### Fixed Issues:
1. **Selection Circle Position**: Now created at origin and positioned when needed
2. **Initialization Timing**: Components wait for unit data to be available
3. **Team Assignment**: Visual coordinator initializes when unit is assigned to team
4. **Retry Logic**: Multiple attempts to initialize if data isn't ready
