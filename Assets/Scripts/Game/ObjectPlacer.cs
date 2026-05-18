using System;
using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacer : MonoBehaviour
{
    [System.Serializable]
    public class Group
    {
        public string groupName = "DefaultGroup";
        public GameObject prefab; // Prefab to instantiate
        public int objectCount = 10; // Total number of objects to instantiate
        public int maxPerRow = 3; // Max objects per row
        public Vector3 startPosition = Vector3.zero; // Starting point for placement
        public Vector3 rowOffset = new Vector3(2f, 0, 0); // Offset between objects in a row
        public Vector3 rowVerticalOffset = new Vector3(0, 0, 2f); // Offset between rows
        public float yOffset = 0f; // Y position adjustment
    }

    public List<Group> groups = new List<Group>(); // Initialize with default groups if needed

    // Call this to instantiate and place objects
    public void PlaceObjects()
    {
        foreach (var group in groups)
        {
            PlaceGroupObjects(group);
        }
    }

    private void Start()
    {
        PlaceGroupObjects(groups[0]);
        PlaceGroupObjects(groups[1]);
    }

    private void PlaceGroupObjects(Group group)
    {
        int totalObjects = group.objectCount;
        int maxPerRow = group.maxPerRow;
        Vector3 startPos = group.startPosition;
        Vector3 rowOffset = group.rowOffset;
        Vector3 rowVOffset = group.rowVerticalOffset;
        float yOffset = group.yOffset;

        int totalRows = Mathf.CeilToInt((float)totalObjects / maxPerRow);

        int objectIndex = 0;
        for (int row = 0; row < totalRows; row++)
        {
            for (int col = 0; col < maxPerRow; col++)
            {
                if (objectIndex >= totalObjects)
                    break;

                Vector3 position = startPos
                    + rowVOffset * row
                    + rowOffset * col
                    + new Vector3(0, yOffset, 0);

                if (group.prefab != null)
                {
                    Instantiate(group.prefab, position, Quaternion.identity);
                }
                objectIndex++;
            }
        }
    }
}