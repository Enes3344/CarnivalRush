using System;

namespace CarnivalRush
{
    public static class RoundRules
    {
        public const int Columns = 7;
        public const int Rows = 4;
        public const int MaximumTargets = 25;
        public const float Spacing = 0.96f;
        public const float Jitter = 0.08f;
        public const float BalloonWidth = 0.60f;
        public const float BalloonHeight = 0.66f;

        public static int BlackCount(int level) => 1 + Math.Min(5, (level - 1) / 3);
        public static int SafeCount(int level) =>
            Math.Min(8 + Math.Min(17, (level - 1) / 2), MaximumTargets - BlackCount(level));

        public static float Seconds(int level) => Math.Max(8f, 20f - (level - 1) * 0.75f);

        // Approach a bounded speed so late rounds retain at least 6.4 real seconds.
        public static float CountdownSpeed(int level)
        {
            double extra = Math.Max(0, level - 17);
            return (float)(1.0 + 0.25 * extra / (extra + 10.0));
        }

        public static (float X, float Y) Position(int cell, Random random)
        {
            float x = (cell % Columns - (Columns - 1) * 0.5f) * Spacing + Offset(random);
            float y = (cell / Columns - (Rows - 1) * 0.5f) * Spacing + Offset(random);
            return (x, y);
        }

        private static float Offset(Random random) => ((float)random.NextDouble() * 2f - 1f) * Jitter;

        public static int[] ShuffledCells(Random random)
        {
            var cells = new int[Columns * Rows];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = i;

            for (int i = cells.Length - 1; i > 0; i--)
            {
                int other = random.Next(i + 1);
                int previous = cells[i];
                cells[i] = cells[other];
                cells[other] = previous;
            }

            return cells;
        }
    }
}
