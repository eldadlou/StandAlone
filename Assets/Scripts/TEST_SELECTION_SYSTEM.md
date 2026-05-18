# 🧪 Testing the New Selection System

## ✅ **What to Test**

### **1. Single Unit Selection**
- [ ] Quick left-click on a unit selects it
- [ ] Other units are deselected
- [ ] Blue highlight appears on selected unit

### **2. Multiple Unit Selection**
- [ ] Hold left-click for 0.2 seconds
- [ ] Blue rectangle appears
- [ ] Drag to create selection area
- [ ] Release to select all units in rectangle
- [ ] Multiple units get highlighted

### **3. Movement Commands**
- [ ] Right-click to move single unit
- [ ] Right-click to move multiple units in formation
- [ ] Units move to target position

### **4. Conflict Resolution**
- [ ] Rectangle selection takes priority over single selection
- [ ] Commands are ignored during rectangle selection
- [ ] Smooth transition between modes

---

## 🛠️ **Setup Checklist**

### **Required Components:**
- [ ] **SelectionRectangle** component on UI Canvas
- [ ] **InputHandler** component in scene
- [ ] **CommandSystem** component in scene
- [ ] **SelectionManager** component in scene
- [ ] **Units** with Unit component and Colliders

### **UI Setup:**
- [ ] Canvas with CanvasScaler
- [ ] SelectionRectangle as child of Canvas
- [ ] SelectionRectangle settings configured

### **Input System:**
- [ ] Unity Input System enabled
- [ ] Input Actions configured
- [ ] InputHandler connected to Input Actions

---

## 🐛 **Debug Steps**

### **Enable Debug Logs:**
1. Select **InputHandler** in scene
2. Check **Enable Debug Logs**
3. Select **SelectionRectangle** in scene
4. Check **Show Debug Info**

### **Check Console for:**
- "Input: Left Click Pressed at X,Y"
- "Selection started at X,Y"
- "Long press detected - showing selection rectangle"
- "Selected X units in rectangle"
- "Input: Ending rectangle selection"

---

## 🔧 **Troubleshooting**

### **Rectangle not appearing:**
1. Check if SelectionRectangle is on Canvas
2. Verify longPressThreshold setting (0.2s)
3. Enable debug logs to see what's happening
4. Check if mouse movement is detected

### **Units not selecting:**
1. Ensure units have Unit component
2. Check if units have Colliders
3. Verify camera is set up correctly
4. Check if units are in front of camera

### **Input not working:**
1. Verify Unity Input System is enabled
2. Check Input Actions configuration
3. Ensure InputHandler is connected
4. Check if InputEvents are being triggered

---

## 🎯 **Expected Behavior**

### **Single Click:**
1. Left-click unit → Unit selected
2. Left-click empty space → All deselected
3. Right-click → Units move to target

### **Rectangle Selection:**
1. Hold left-click for 0.2s → Rectangle appears
2. Drag mouse → Rectangle follows
3. Release → Units in rectangle selected
4. Right-click → Units move in formation

### **Visual Feedback:**
- Blue rectangle shows selection area
- Selected units have blue highlights
- Smooth transitions between states

---

## 🚀 **Ready to Test!**

Your selection system should now work with:
- ✅ **Unity Input System** integration
- ✅ **Event-driven architecture**
- ✅ **Smart conflict resolution**
- ✅ **Professional RTS feel**

**Test it out and let me know how it works!** 🎮 