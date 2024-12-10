using UnityEngine;
using System.Collections.Generic;

public class GridPlacer : MonoBehaviour
{
    // Function to arrange GameObjects in a grid pattern
    public static void ArrangeInGrid(List<GameObject> gameObjects, Vector3 origin, float spacing)
    {
        int columns = 3;  // Number of GameObjects per row
        int rowCount = Mathf.CeilToInt((float)gameObjects.Count / columns); // Total rows required

        for (int i = 0; i < gameObjects.Count; i++)
        {
            // Determine the row and column for the current GameObject
            int row = i / columns;
            int column = i % columns;

            // Calculate the position of the GameObject
            float xOffset = (column - (columns - 1) / 2.0f) * spacing; // Center the row horizontally
            float yOffset = row * spacing; // Move up per row

            // Set the position based on the origin point
            Vector3 newPosition = origin + new Vector3(xOffset, yOffset, 0);
            gameObjects[i].transform.position = newPosition;
        }
    }

    public static void ArrangeInSemicircle(List<GameObject> gameObjects, Vector3 center, float radius)
    {
        // Limit the maximum number of objects to 5
        int maxObjects = Mathf.Min(gameObjects.Count, 5);

        // Define angles based on the number of objects to place
        float[] angles = GetAngles(maxObjects);

        // Place each GameObject at the calculated angle around the semicircle
        for (int i = 0; i < maxObjects; i++)
        {
            float angle = angles[i] + 90; // Get the predefined angle
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius; // Calculate position offset
            gameObjects[i].transform.position = center + offset; // Set the new position
        }
    }

    // Function to get predefined angles based on the number of GameObjects
    private static float[] GetAngles(int count)
    {
        // Define predefined angles in radians, centered first then symmetrically placed
        switch (count)
        {
            case 1:
                return new float[] { 0 }; // Centered
            case 2:
                return new float[] { -Mathf.PI / 4, Mathf.PI / 4 }; // Left and Right
            case 3:
                return new float[] { 0, -Mathf.PI / 4, Mathf.PI / 4 }; // Center, Left, Right
            case 4:
                return new float[] { -Mathf.PI / 6, Mathf.PI / 6, -Mathf.PI / 2, Mathf.PI / 2 }; // Spread evenly
            case 5:
                return new float[] { 0, -Mathf.PI / 6, Mathf.PI / 6, -Mathf.PI / 2, Mathf.PI / 2 }; // Centered, then spread evenly
            default:
                return new float[0]; // Empty array if no GameObjects
        }
    }
}
