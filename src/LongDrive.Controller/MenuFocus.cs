using System;
using System.Collections.Generic;

namespace LongDrive.Controller
{
    internal struct MenuFocusTarget
    {
        public float X, Y;
        public int Priority;
        public bool Eligible;
        public MenuFocusTarget(float x, float y, int priority, bool eligible)
        { X = x; Y = y; Priority = priority; Eligible = eligible; }
    }

    internal static class MenuFocus
    {
        public static int Initial(IList<MenuFocusTarget> targets)
        {
            int best = -1;
            for (int i = 0; i < targets.Count; i++)
            {
                var candidate = targets[i];
                if (!candidate.Eligible) continue;
                if (best < 0 || candidate.Priority < targets[best].Priority ||
                    candidate.Priority == targets[best].Priority && (candidate.Y > targets[best].Y || candidate.Y == targets[best].Y && candidate.X < targets[best].X)) best = i;
            }
            return best;
        }
        public static int Move(IList<MenuFocusTarget> targets, int current, float x, float y)
        {
            if (current < 0 || current >= targets.Count) return Initial(targets);
            int best = -1;
            float score = float.PositiveInfinity;
            var origin = targets[current];
            for (int i = 0; i < targets.Count; i++)
            {
                if (i == current || !targets[i].Eligible) continue;
                float dx = targets[i].X - origin.X, dy = targets[i].Y - origin.Y;
                float forward = dx * x + dy * y;
                if (forward <= 1) continue;
                float candidate = forward + Math.Abs(-dx * y + dy * x) * 3;
                if (candidate < score) { best = i; score = candidate; }
            }
            return best;
        }
    }
}
