using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LongDrive.Controller
{
    internal struct GlyphRun
    {
        public string Text, Sprite;
        public GlyphRun(string text, string sprite = null) { Text = text; Sprite = sprite; }
    }

    // A single vocabulary is shared by native TMP prompts and the custom menu renderer.
    // Only trusted controller instructions go through this parser, never typed text.
    internal static class GlyphTokens
    {
        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            ["A"] = "A", ["B"] = "B", ["X"] = "X", ["Y"] = "Y",
            ["LB"] = "LB", ["RB"] = "RB", ["LT"] = "LT", ["RT"] = "RT",
            ["LS Click"] = "L3", ["RS Click"] = "R3", ["LS click"] = "L3", ["RS click"] = "R3",
            ["L3"] = "L3", ["R3"] = "R3", ["View"] = "Select", ["Back"] = "Select", ["Select"] = "Select", ["Start"] = "Start",
            ["D-pad Up"] = "DPad_UP", ["D-pad Down"] = "DPad_Down", ["D-pad Left"] = "DPad_Left", ["D-pad Right"] = "DPad_Right"
        };
        private static readonly Regex Pattern = new Regex(@"(?<![A-Za-z0-9_])(?:D-pad Right|D-pad Down|D-pad Left|D-pad Up|LS Click|RS Click|LS click|RS click|Select|Start|View|Back|LB|RB|LT|RT|L3|R3|A|B|X|Y)(?![A-Za-z0-9_])");
        private static readonly Dictionary<string, GlyphRun[]> Cache = new Dictionary<string, GlyphRun[]>();
        public static GlyphRun[] Parse(string text)
        {
            if (Cache.TryGetValue(text, out var cached)) return cached;
            var result = new List<GlyphRun>();
            int start = 0;
            foreach (Match match in Pattern.Matches(text))
            {
                if (match.Index > start) result.Add(new GlyphRun(text.Substring(start, match.Index - start)));
                result.Add(new GlyphRun(match.Value, Names[match.Value]));
                start = match.Index + match.Length;
            }
            if (start < text.Length) result.Add(new GlyphRun(text.Substring(start)));
            return Cache[text] = result.ToArray();
        }
    }
}
