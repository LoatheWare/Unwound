using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MusicHelper;

namespace Unwound
{
    public partial class MainWindow : Window
    {
        private List<Chord> currentChords = new List<Chord>();
        private string currentInstrument = "Guitar";

        public MainWindow()
        {
            InitializeComponent();
            Generate();
        }

        void Generate()
        {
            if (KeyCombo.SelectedItem == null)
                KeyCombo.SelectedIndex = 0;
            if (ScaleCombo.SelectedItem == null)
                ScaleCombo.SelectedIndex = 0;
            if (CountCombo.SelectedItem == null)
                CountCombo.SelectedIndex = 2;

            string key = ((ComboBoxItem)KeyCombo.SelectedItem).Content.ToString();
            string scaleText = ((ComboBoxItem)ScaleCombo.SelectedItem).Content.ToString();

            ScaleType scale = scaleText switch
            {
                "Minor" => ScaleType.Minor,
                "Chromatic" => ScaleType.Chromatic,
                _ => ScaleType.Major
            };

            int count = int.Parse(((ComboBoxItem)CountCombo.SelectedItem).Content.ToString());
            currentChords = MusicEngine.GenerateProgression(key, scale, count);

            if (currentChords.Count == 0)
            {
                RomanText.Text = "—";
                ChordText.Text = "No chords generated";
                return;
            }

            RomanText.Text = string.Join("  –  ", currentChords.Select(c => c.Roman));
            ChordText.Text = string.Join("  –  ", currentChords.Select(c => c.Name));

            UpdateDiagrams();
        }

        void UpdateDiagrams()
        {
            if (DiagramToggle == null || DiagramsContainer == null)
                return;

            if (DiagramToggle.IsChecked == true)
            {
                DiagramsContainer.Children.Clear();

                foreach (var chord in currentChords)
                {
                    var diagram = CreateChordDiagram(chord.Name, currentInstrument);
                    DiagramsContainer.Children.Add(diagram);
                }
            }
        }

        UIElement CreateChordDiagram(string chordName, string instrument)
        {
            var container = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0a0a0a")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ff88")),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 15, 0),
                Width = 180,
                Height = 220
            };

            var stack = new StackPanel();

            var label = new TextBlock
            {
                Text = chordName,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ff88")),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(label);

            Canvas diagram = instrument switch
            {
                "Guitar" => CreateGuitarDiagram(chordName),
                "Piano" => CreatePianoDiagram(chordName),
                "Bass" => CreateBassDiagram(chordName),
                "Violin" => CreateViolinDiagram(chordName),
                _ => CreateGuitarDiagram(chordName)
            };

            stack.Children.Add(diagram);
            container.Child = stack;

            return container;
        }

        Canvas CreateGuitarDiagram(string chordName)
        {
            var canvas = new Canvas { Width = 140, Height = 160 };
            var greenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ff88"));
            var darkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

            // Flip horizontally
            canvas.RenderTransform = new ScaleTransform(-1, 1, 70, 0);

            int baseFret = GetGuitarBaseFret(chordName);

            if (baseFret > 0)
            {
                var fretLabel = new TextBlock
                {
                    Text = baseFret.ToString(),
                    Foreground = greenBrush,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    RenderTransform = new ScaleTransform(-1, 1)

                };

                Canvas.SetRight(fretLabel, -11);
                Canvas.SetTop(fretLabel, 55);
                canvas.Children.Add(fretLabel);
            }

            // Draw frets (horizontal lines)
            for (int i = 0; i < 5; i++)
            {
                var line = new Line
                {
                    X1 = 20,
                    X2 = 120,
                    Y1 = 20 + i * 30,
                    Y2 = 20 + i * 30,
                    Stroke = darkBrush,
                    StrokeThickness = 2
                };
                canvas.Children.Add(line);
            }

            // Draw strings (vertical lines)
            for (int i = 0; i < 6; i++)
            {
                var line = new Line
                {
                    X1 = 20 + i * 20,
                    X2 = 20 + i * 20,
                    Y1 = 20,
                    Y2 = 140,
                    Stroke = darkBrush,
                    StrokeThickness = 1.5
                };
                canvas.Children.Add(line);
            }

            // Add finger positions based on chord
            var positions = GetGuitarPositions(chordName);
            foreach (var pos in positions)
            {
                var dot = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = greenBrush
                };
                Canvas.SetLeft(dot, 20 + pos.String * 20 - 6);
                Canvas.SetTop(dot, 20 + pos.Fret * 30 - 15 - 6);
                canvas.Children.Add(dot);
            }

            return canvas;
        }

        Canvas CreatePianoDiagram(string chordName)
        {
            var canvas = new Canvas { Width = 140, Height = 160 };
            var greenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ff88"));
            var whiteBrush = new SolidColorBrush(Colors.White);
            var darkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a1a1a"));

            // Draw white keys
            for (int i = 0; i < 7; i++)
            {
                var key = new Rectangle
                {
                    Width = 18,
                    Height = 100,
                    Fill = whiteBrush,
                    Stroke = darkBrush,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(key, 5 + i * 19);
                Canvas.SetTop(key, 30);
                canvas.Children.Add(key);
            }

            // Draw black keys
            int[] blackKeyPositions = { 0, 1, 3, 4, 5 };
            foreach (var pos in blackKeyPositions)
            {
                var key = new Rectangle
                {
                    Width = 12,
                    Height = 60,
                    Fill = darkBrush,
                    Stroke = darkBrush,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(key, 17 + pos * 19);
                Canvas.SetTop(key, 30);
                canvas.Children.Add(key);
            }

            // Highlight keys for the chord
            var keys = GetPianoKeys(chordName);
            foreach (var keyPos in keys)
            {
                var highlight = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = greenBrush
                };

                double topPosition;
                if (keyPos % 1 == 0.5)
                {
                    topPosition = 75; // Black key
                }
                else
                {
                    topPosition = 140; // White key
                }

                Canvas.SetLeft(highlight, 9 + keyPos * 19);
                Canvas.SetTop(highlight, topPosition);
                canvas.Children.Add(highlight);
            }

            return canvas;
        }

        Canvas CreateBassDiagram(string chordName)
        {
            var canvas = new Canvas { Width = 140, Height = 160 };
            var greenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ff88"));
            var darkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

            // Flip vertically (not horizontally)
            canvas.RenderTransform = new ScaleTransform(-1, 1, 70, 0);

            int baseFret = GetGuitarBaseFret(chordName);

            if (baseFret > 0)
            {
                var fretLabel = new TextBlock
                {
                    Text = baseFret.ToString(),
                    Foreground = greenBrush,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    RenderTransform = new ScaleTransform(-1, 1)

                };

                Canvas.SetRight(fretLabel, -5);
                Canvas.SetTop(fretLabel, 55);
                canvas.Children.Add(fretLabel);
            }

            // Draw frets (4 strings for bass)
            for (int i = 0; i < 5; i++)
            {
                var line = new Line
                {
                    X1 = 30,
                    X2 = 110,
                    Y1 = 20 + i * 30,
                    Y2 = 20 + i * 30,
                    Stroke = darkBrush,
                    StrokeThickness = 2
                };
                canvas.Children.Add(line);
            }

            // Draw strings
            for (int i = 0; i < 4; i++)
            {
                var line = new Line
                {
                    X1 = 30 + i * 26,
                    X2 = 30 + i * 26,
                    Y1 = 20,
                    Y2 = 140,
                    Stroke = darkBrush,
                    StrokeThickness = 2
                };
                canvas.Children.Add(line);
            }

            // Add finger positions
            var positions = GetBassPositions(chordName);
            foreach (var pos in positions)
            {
                var dot = new Ellipse
                {
                    Width = 14,
                    Height = 14,
                    Fill = greenBrush
                };
                Canvas.SetLeft(dot, 30 + pos.String * 26 - 7);
                Canvas.SetTop(dot, 20 + pos.Fret * 30 - 15 - 7);
                canvas.Children.Add(dot);
            }

            return canvas;
        }

        Canvas CreateViolinDiagram(string chordName)
        {
            var canvas = new Canvas { Width = 140, Height = 160 };
            var greenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ff88"));
            var darkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));

            // Draw fingerboard
            var fingerboard = new Rectangle
            {
                Width = 80,
                Height = 140,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a2a2a")),
                Stroke = darkBrush,
                StrokeThickness = 2
            };
            Canvas.SetLeft(fingerboard, 30);
            Canvas.SetTop(fingerboard, 10);
            canvas.Children.Add(fingerboard);

            // Draw strings (4 strings for violin)
            for (int i = 0; i < 4; i++)
            {
                var line = new Line
                {
                    X1 = 40 + i * 20,
                    X2 = 40 + i * 20,
                    Y1 = 10,
                    Y2 = 150,
                    Stroke = darkBrush,
                    StrokeThickness = 1.5
                };
                canvas.Children.Add(line);
            }

            // Draw position markers (frets)
            for (int i = 1; i <= 4; i++)
            {
                var line = new Line
                {
                    X1 = 30,
                    X2 = 110,
                    Y1 = 10 + i * 28,
                    Y2 = 10 + i * 28,
                    Stroke = darkBrush,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                canvas.Children.Add(line);
            }

            // Add finger positions
            var positions = GetViolinPositions(chordName);
            foreach (var pos in positions)
            {
                var dot = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = greenBrush
                };
                Canvas.SetLeft(dot, 40 + pos.String * 20 - 6);
                Canvas.SetTop(dot, 25 + pos.Fret * 28 - 6);
                canvas.Children.Add(dot);
            }

            return canvas;
        }

        int GetGuitarBaseFret(string chordName)
        {
            var baseChord = chordName.Replace("m", "").Replace("°", "");

            return baseChord switch
            {
                "C#" or "Db" => 4,
                "G#" or "Ab" => 4,

                "D#" or "Eb" => 6,
                "A#" or "Bb" => 6,

                _ => 0
            };
        }


        // Helper methods to get chord positions
        List<(int String, int Fret)> GetGuitarPositions(string chord)
        {
            var baseChord = chord.Replace("m", "").Replace("°", "");
            var isMinor = chord.Contains("m") && !chord.Contains("maj");

            // Strings from left to right after flip: E(0), A(1), D(2), G(3), B(4), E(5)
            return baseChord switch
            {
                "C" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 3), (2, 4), (3, 4), (4, 2) } :  // Cm
                    new List<(int, int)> { (1, 1), (2, 0), (3, 2), (4, 3) },            // C

                "C#" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 3), (2, 4), (3, 4), (4, 2) } :  // C#m
                    new List<(int, int)> { (0, 2), (1, 4), (2, 4), (3, 4), (4, 2) },            // C#

                "Db" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 3), (2, 4), (3, 4), (4, 2) } :  // Dbm
                    new List<(int, int)> { (0, 2), (1, 4), (2, 4), (3, 4), (4, 2) },   // Db

                "D" => isMinor ?
                    new List<(int, int)> { (0, 1), (1, 3), (2, 2), (3, 0) } :          // Dm: XX0231
                    new List<(int, int)> { (0, 2), (1, 3), (2, 2), (3, 0) },           // D: XX0232

                "D#" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 3), (2, 4), (3, 4), (4, 2) } : // D#m: XX1342
                    new List<(int, int)> { (0, 2), (1, 4), (2, 4), (3, 4), (4, 2) },  // D#: XX1343

                "Eb" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 3), (2, 4), (3, 4), (4, 2) } : // Ebm: XX1342
                    new List<(int, int)> { (0, 2), (1, 4), (2, 4), (3, 4), (4, 2) },  // Eb: XX1343

                "E" => isMinor ?
                    new List<(int, int)> { (2, 0), (3, 2), (4, 2), (5, 0) } :          // Em: 022000
                    new List<(int, int)> { (2, 1), (3, 2), (4, 2), (5, 0) },           // E: 022100

                "F" => isMinor ?
                    new List<(int, int)> { (0, 1), (1, 1), (2, 1), (3, 3), (4, 3), (5, 1) } : // Fm: 133111}
                    new List<(int, int)> { (0, 1), (1, 1), (2, 2), (3, 3), (4, 3), (5, 1) }, // F: 133211

                "F#" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 2), (2, 2), (3, 4), (4, 4), (5, 2) } : // F#m: 244222
                    new List<(int, int)> { (0, 2), (1, 2), (2, 3), (3, 4), (4, 4), (5, 2) }, // F#: 244322

                "Gb" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 2), (2, 2), (3, 4), (4, 4), (5, 2) } : // Gbm: 244222
                    new List<(int, int)> { (0, 2), (1, 2), (2, 3), (3, 4), (4, 4), (5, 2) }, // Gb: 244322

                "G" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 2), (2, 2), (3, 4), (4, 4), (5, 2) } : // Gm: 320033
                    new List<(int, int)> { (0, 3), (1, 0), (2, 0), (3, 0), (4, 2), (5, 3) }, // G: 320003

                "G#" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 2), (2, 2), (3, 4), (4, 4), (5, 2) } : // G#m: 244222
                    new List<(int, int)> { (0, 2), (1, 2), (2, 3), (3, 4), (4, 4), (5, 2) }, // G#: 244322

                "Ab" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 2), (2, 2), (3, 4), (4, 4), (5, 2) } : // Abm: 244222
                    new List<(int, int)> { (0, 2), (1, 2), (2, 3), (3, 4), (4, 4), (5, 2) }, // Ab: 244322

                "A" => isMinor ?
                    new List<(int, int)> { (1, 1), (2, 2), (3, 2), (4, 0) } :          // Am: X02210
                    new List<(int, int)> { (1, 2), (2, 2), (3, 2), (4, 0) },           // A: X02220

                "A#" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 2), (2, 2), (3, 4), (4, 4), (5, 2) } : // A#m: 244222
                    new List<(int, int)> { (0, 2), (1, 2), (2, 3), (3, 4), (4, 4), (5, 2) }, // A#: 244322

                "Bb" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 2), (2, 2), (3, 4), (4, 4), (5, 2) } : // Bbm: 244222
                    new List<(int, int)> { (0, 2), (1, 2), (2, 3), (3, 4), (4, 4), (5, 2) }, // : 244322

                "B" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 3), (2, 4), (3, 4), (4, 2) } :  // Bm: X24432
                    new List<(int, int)> { (0, 2), (1, 4), (2, 4), (3, 4), (4, 2) },   // B: X24442

                _ => new List<(int, int)> { (2, 1), (3, 2), (4, 2) }
            };

        }

        List<double> GetPianoKeys(string chord)
        {
            var baseChord = chord.Replace("m", "").Replace("°", "");
            var isMinor = chord.Contains("m") && !chord.Contains("maj");

            return baseChord switch
            {
                "C" => isMinor
                    ? new List<double> { 0, 1.5, 4 }   // C–Eb–G
                    : new List<double> { 0, 2, 4 },    // C–E–G

                "C#" => isMinor
                    ? new List<double> { 0.5, 2, 4.5 }   // C#–E–G#
                    : new List<double> { 0.5, 3, 4.5 },    // C#–F–G#

                "Db" => isMinor
                    ? new List<double> { 0.5, 2, 4.5 }   // C#–E–G#
                    : new List<double> { 0.5, 3, 4.5 },    // C#–F–G#

                "D" => isMinor
                    ? new List<double> { 1, 3, 5 }   // D–F–A
                    : new List<double> { 1, 3.5, 5 },    // D–F#–A

                "D#" => isMinor
                    ? new List<double> { 1.5, 3.5, 5.5 }   // D#–F#–A#
                    : new List<double> { 1.5, 4, 5.5 },    // D#–G–A#

                "Eb" => isMinor
                    ? new List<double> { 1.5, 3.5, 5.5 }   // D#–F#–A#
                    : new List<double> { 1.5, 4, 5.5 },    // D#–G–A#

                "E" => isMinor
                    ? new List<double> { 2, 4, 6 }     // E–G–B
                    : new List<double> { 2, 4.5, 6 },  // E–G#–B

                "F" => isMinor
                    ? new List<double> { 3, 4.5, 0 }   // F–Ab–C
                    : new List<double> { 3, 5, 0 },    // F–A–C

                "F#" => isMinor
                    ? new List<double> { 3.5, 5, 0.5 }   // F#–A–C#
                    : new List<double> { 3.5, 5.5, 0.5 },    // F#–A#–C#

                "Gb" => isMinor
                    ? new List<double> { 3.5, 5, 0.5 }   // F#–A–C#
                    : new List<double> { 3.5, 5.5, 0.5 },    // F#–A#–C#

                "G" => isMinor
                    ? new List<double> { 4, 5.5, 1 }     // G–Bb–D
                    : new List<double> { 4, 6, 1 },  // G–B–D

                "G#" => isMinor
                    ? new List<double> { 4.5, 6, 1.5 }     // G#–B–D#
                    : new List<double> { 4.5, 0, 1.5 },  // G#–C–D#

                "Ab" => isMinor
                    ? new List<double> { 4.5, 6, 1.5 }     // G#–B–D#
                    : new List<double> { 4.5, 0, 1.5 },  // G#–C–D#

                "A" => isMinor
                    ? new List<double> { 0, 3, 5 }     // A–C–E
                    : new List<double> { 0.5, 3, 5 },  // A–C#–E

                "A#" => isMinor
                    ? new List<double> { 0.5, 3, 5.5 }     // A#–C#–F
                    : new List<double> { 1, 3.5, 5.5 },  // A#–D–F

                "Bb" => isMinor
                    ? new List<double> { 0.5, 3, 5.5 }     // A#–C#–F
                    : new List<double> { 1, 3.5, 5.5 },  // A#–D–F

                "B" => isMinor
                    ? new List<double> { 6, 1, 3 }     // B–D–F#
                    : new List<double> { 6, 1.5, 3 },  // B–D#–F#

                _ => new List<double> { 0, 2, 4 }
            };
        }


        List<(int String, int Fret)> GetBassPositions(string chord)
        {
            var baseChord = chord.Replace("m", "").Replace("°", "");
            var isMinor = chord.Contains("m") && !chord.Contains("maj");

            // Bass guitar root notes (strings: G=0, D=1, A=2, E=3)
            return baseChord switch
            {
                "C" => new List<(int, int)> { (2, 3) },           // A string, 3rd fret = C
                "C#" => new List<(int, int)> { (2, 4) },
                "Db" => new List<(int, int)> { (2, 4) },
                "D" => new List<(int, int)> { (1, 0) },   // D string open or A string 5th
                "D#" => new List<(int, int)> { (2, 2) },
                "Eb" => new List<(int, int)> { (2, 2) },
                "E" => new List<(int, int)> { (3, 0), (1, 2) },   // E string open
                "F" => new List<(int, int)> { (3, 1), (1, 3) },   // E string 1st fret = F
                "F#" => new List<(int, int)> { (3, 2), (1, 4) },
                "Gb" => new List<(int, int)> { (3, 2), (1, 4) },
                "G" => new List<(int, int)> { (3, 3) },   // E string 3rd or G string open
                "G#" => new List<(int, int)> { (3, 2) },
                "Ab" => new List<(int, int)> { (3, 2) },
                "A" => new List<(int, int)> { (2, 0) },   // A string open
                "A#" => new List<(int, int)> { (3, 2) },
                "Bb" => new List<(int, int)> { (3, 2) },
                "B" => new List<(int, int)> { (2, 2), (0, 4) },   // A string 2nd fret = B
                _ => new List<(int, int)> { (2, 3) }
            };
        }

        List<(int String, int Fret)> GetViolinPositions(string chord)
        {
            var baseChord = chord.Replace("m", "").Replace("°", "");
            var isMinor = chord.Contains("m") && !chord.Contains("maj");

            // Violin strings: G=0, D=1, A=2, E=3
            // Each fret is about a whole step
            return baseChord switch
            {
                "C" => isMinor ?
                    new List<(int, int)> { (1, 0), (2, 2), (3, 2) } :
                    new List<(int, int)> { (1, 1), (2, 2), (3, 2) },   // C on G string (3rd pos), C on D string

                "C#" or "Db" => isMinor ?
                    new List<(int, int)> { (1, 1), (2, 3), (3, 3) } :
                    new List<(int, int)> { (1, 2), (2, 3), (3, 3) },   // C# on G string, C# on D string

                "D" => isMinor ?
                    new List<(int, int)> { (1, -1), (2, -1), (3, 0) } :
                    new List<(int, int)> { (1, -1), (2, -1), (3, 1) },   // D string open, D on A string

                "D#" or "Eb" => isMinor ?
                    new List<(int, int)> { (1, 0), (2, 0), (3, 1) } :
                    new List<(int, int)> { (0, -1), (1, 0), (2, 0), (3, 2) },

                "E" => isMinor ?
                    new List<(int, int)> { (1, 1), (2, 1), (3, 2) } :
                    new List<(int, int)> { (1, 1), (2, 1), (3, 3) },   // E string open, E on D string
                "F" => isMinor ?
                    new List<(int, int)> { (0, 0), (1, 2), (2, 2), (3, 3) } :
                    new List<(int, int)> { (0, 1), (1, 2), (2, 2), (3, 4) },   // F on E string, F on D string

                "F#" or "Gb" => isMinor ?
                    new List<(int, int)> { (0, 1), (1, 3), (2, 3), (3, 4) } :
                    new List<(int, int)> { (0, 2), (1, 3), (2, 3), (3, 4) },   // F# on E string, F# on D string

                "G" => isMinor ?
                    new List<(int, int)> { (0, -1), (1, -1), (2, 0), (3, 2) } :   // G string open
                    new List<(int, int)> { (0, -1), (1, -1), (2, 1), (3, 2) },   // G string open, G on D string

                "G#" or "Ab" => isMinor ?
                    new List<(int, int)> { (0, 0), (1, 0), (2, 1), (3, 3) } :
                    new List<(int, int)> { (0, 0), (1, 0), (2, 2), (3, 3) },

                "A" => isMinor ?
                    new List<(int, int)> { (0, 1), (1, 1), (2, 2), (3, 4) } :   // A string open
                    new List<(int, int)> { (0, 1), (1, 1), (2, 3), (3, 4) },   // A string open, A on G string

                "A#" or "Bb" => isMinor ?
                    new List<(int, int)> { (0, 2), (1, 2), (2, 3) } :
                    new List<(int, int)> { (0, 2), (1, 2), (2, 4) },

                "B" => isMinor ?
                    new List<(int, int)> { (0, 3), (1, -1), (2, 1), (3, 1) } :   // B on A string
                    new List<(int, int)> { (0, 3), (1, 0), (2, 1) },   // B on E string, B on A string
                _ => new List<(int, int)> { (1, 1), (2, 2) }
            };
        }

        void DiagramToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (DiagramToggle == null || InstrumentLabel == null || InstrumentList == null || DiagramsPanel == null || ChordDisplayBorder == null)
                return;

            bool isEnabled = DiagramToggle.IsChecked == true;

            // Update visibility of diagrams panel
            DiagramsPanel.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;

            // Update chord display row span
            if (isEnabled)
            {
                Grid.SetRowSpan(ChordDisplayBorder, 1);
            }
            else
            {
                Grid.SetRowSpan(ChordDisplayBorder, 2);
            }

            // Update instrument list appearance
            var enabledColor = (Color)ColorConverter.ConvertFromString("#00ff88");
            var disabledColor = (Color)ColorConverter.ConvertFromString("#333333");

            InstrumentLabel.Foreground = new SolidColorBrush(isEnabled ? enabledColor : disabledColor);

            // Update all items in the list
            foreach (ListBoxItem item in InstrumentList.Items)
            {
                if (!item.IsSelected)
                {
                    item.Foreground = new SolidColorBrush(isEnabled ? enabledColor : disabledColor);
                }
            }

            if (isEnabled)
            {
                UpdateDiagrams();
            }
        }

        void InstrumentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InstrumentList.SelectedItem == null || DiagramToggle == null)
                return;

            var selectedItem = (ListBoxItem)InstrumentList.SelectedItem;
            currentInstrument = selectedItem.Tag.ToString();

            // Enable diagram toggle if not already enabled
            if (DiagramToggle.IsChecked != true)
            {
                DiagramToggle.IsChecked = true;
            }
            else
            {
                UpdateDiagrams();
            }
        }

        void SurpriseMe_Click(object sender, RoutedEventArgs e)
        {
            var rnd = new Random();
            KeyCombo.SelectedIndex = rnd.Next(KeyCombo.Items.Count);
            ScaleCombo.SelectedIndex = rnd.Next(0, 2);
            CountCombo.SelectedIndex = rnd.Next(CountCombo.Items.Count);
            Generate();
        }

        void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Generate();
        }

        // Custom Title Bar Controls
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
