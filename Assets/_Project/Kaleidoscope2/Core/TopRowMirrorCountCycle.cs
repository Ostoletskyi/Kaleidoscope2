namespace Kaleidoscope2.Core
{
    public static class TopRowMirrorCountCycle
    {
        public const int MinGroupIndex = 1;
        public const int MaxGroupIndex = 4;
        public const int GroupCount = MaxGroupIndex - MinGroupIndex + 1;
        public const int SequenceLength = 4;
        public const int FallbackCount = 6;

        private static readonly int[][] Sequences =
        {
            new[] { 3, 4, 6, 4 },
            new[] { 8, 10, 12, 10 },
            new[] { 14, 16, 24, 16 },
            new[] { 28, 36, 48, 36 }
        };

        public static bool IsCycleGroupIndex(int groupIndex)
        {
            return groupIndex >= MinGroupIndex && groupIndex <= MaxGroupIndex;
        }

        public static int ResolveWrappedStep(int stepIndex)
        {
            int resolved = stepIndex % SequenceLength;
            return resolved < 0 ? resolved + SequenceLength : resolved;
        }

        public static int ResolveCountForStep(int groupIndex, int stepIndex)
        {
            if (!IsCycleGroupIndex(groupIndex))
            {
                return FallbackCount;
            }

            return Sequences[groupIndex - MinGroupIndex][ResolveWrappedStep(stepIndex)];
        }
    }
}
