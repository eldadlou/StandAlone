using UnityEngine;
using System;
using System.Collections.Generic;

namespace MyGame.Core.Units
{
    public class Truck : Unit
    {
        public override UnitType Type => UnitType.Truck;

        protected override float GetInitialHealth() => 80f;
        protected override float GetInitialSpeed() => 7f;

        // Override methods if Truck has special behavior
    }
}