using UnityEngine;
using System.Collections.Generic;

namespace MyGame.Core.Interfaces
{
    public interface IMovable
    {
        void MoveTo(Vector3 destination);
        float Speed { get; }
        bool IsMoving { get; }
        Vector3 Destination { get; }
        void UpdatePosition(Vector3 newPosition);
    }
}
