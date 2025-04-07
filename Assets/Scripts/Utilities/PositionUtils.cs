using UnityEngine;

namespace Utilities
{
    public static class PositionUtils
    {
        private const int GridWidth = 5;
        private const int GridHeight = 5;

        public static Vector3 CalculateGridPosition(int itemIndex, float verticalSpacing, float horizontalSpacing,
            float heightPerLayer)
        {
            var cellsPerLayer = GridWidth * GridHeight;

            var layer = itemIndex / cellsPerLayer;

            var indexInLayer = itemIndex % cellsPerLayer;

            var x = indexInLayer % GridWidth;
            var z = (indexInLayer / GridWidth) % GridHeight;

            var xCenter = (GridWidth - 1) * horizontalSpacing / 2;
            var zCenter = (GridHeight - 1) * verticalSpacing / 2;

            return new Vector3(
                x * horizontalSpacing - xCenter,
                layer * heightPerLayer,
                z * verticalSpacing - zCenter
            );
        }
    }
}