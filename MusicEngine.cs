using System;
using System.Collections.Generic;

namespace MusicHelper
{
    public enum ScaleType
    {
        Major,
        Minor,
        Chromatic
    }

    public class Chord
    {
        public string Roman { get; set; }
        public string Name { get; set; }
    }

    public static class MusicEngine
    {
        static readonly string[] Notes =
        {
            "C","C#","D","Eb","E","F",
            "F#","G","Ab","A","Bb","B"
        };

        static readonly (string Roman, string Quality)[] MajorScale =
        {
            ("I","Major"),
            ("ii","Minor"),
            ("iii","Minor"),
            ("IV","Major"),
            ("V","Major"),
            ("vi","Minor"),
            ("vii°","Diminished")
        };

        static readonly (string Roman, string Quality)[] MinorScale =
        {
            ("i","Minor"),
            ("ii°","Diminished"),
            ("III","Major"),
            ("iv","Minor"),
            ("v","Minor"),
            ("VI","Major"),
            ("VII","Major")
        };

        static readonly Dictionary<string, string> Enharmonics = new()
{
         { "Db", "C#" },
         { "Gb", "F#" },
};

        static readonly int[][] MajorProgressions =
{
    new[] { 0, 4, 5, 3 },   // I–V–vi–IV
    new[] { 1, 4, 0 },     // ii–V–I
    new[] { 5, 3, 0, 4 },  // vi–IV–I–V
    new[] { 0, 3, 4 },     // I–IV–V
};

        static readonly int[][] MinorProgressions =
        {
    new[] { 0, 5, 2, 6 },  // i–VI–III–VII
    new[] { 0, 3, 4 },     // i–iv–v
    new[] { 5, 6, 0 },     // VI–VII–i
};
        static readonly int[] HarmonicMinorIntervals = { 0, 2, 3, 5, 7, 8, 11 };
        static readonly int[] MajorScaleIntervals = { 0, 2, 4, 5, 7, 9, 11 };
        static readonly int[] MinorScaleIntervals = { 0, 2, 3, 5, 7, 8, 10 };

        static string[] BuildScale(string key, ScaleType scale)
        {
            // fix flats / sharps
            if (Enharmonics.ContainsKey(key))
                key = Enharmonics[key];

            // find root note index
            int rootIndex = Array.IndexOf(Notes, key);
            if (rootIndex < 0) return Array.Empty<string>();

            // choose intervals depending on scale type
            int[] intervals = scale switch
            {
                ScaleType.Minor => HarmonicMinorIntervals, // now uses harmonic minor
                ScaleType.Major => MajorScaleIntervals,
                _ => MajorScaleIntervals
            };

            // build the 7-note scale
            string[] result = new string[7];
            for (int i = 0; i < 7; i++)
            {
                result[i] = Notes[(rootIndex + intervals[i]) % Notes.Length];
            }

            return result;
        }



        public static List<Chord> GenerateProgression(
    string key,
    ScaleType scale,
    int chordCount)
        {
            var random = new Random();
            var result = new List<Chord>();

            // Build the scale once
            var scalenotes = BuildScale(key, scale);
            if (scalenotes.Length == 0) return result;

            var scaleDef = scale == ScaleType.Minor ? MinorScale : MajorScale;
            var progressions = scale == ScaleType.Minor ? MinorProgressions : MajorProgressions;

            // Pick a random progression pattern
            var pattern = progressions[random.Next(progressions.Length)];

            // Generate chords using the pattern
            for (int i = 0; i < chordCount; i++)
            {
                int degree = pattern[i % pattern.Length];
                var (roman, quality) = scaleDef[degree];

                string root = scalenotes[degree];
                string chordName = BuildChordName(root, quality);

                result.Add(new Chord
                {
                    Roman = roman,
                    Name = chordName
                });
            }

            return result;
        }


        static string BuildChordName(string root, string quality)
        {
            return quality switch
            {
                "Minor" => root + "m",
                "Diminished" => root + "°",
                _ => root
            };


        }
    }
}
