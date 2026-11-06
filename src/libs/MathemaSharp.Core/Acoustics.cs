// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;

namespace MathemaSharp.Core;

/// <summary>Pitch arithmetic in equal temperament, for every IEEE Generic Math format.</summary>
public static class Acoustics
{
    /// <summary>The MIDI note number nearest to a frequency, with A4 = 440 Hz = note 69 and twelve notes per octave: round(69 + 12 log2(f / 440)). Ties round to even.
    /// The caller checks that <paramref name="hertz"/> is positive and finite.</summary>
    public static int MidiNoteNumber<T>(T hertz) where T : IFloatingPointIeee754<T> => MidiNoteNumber(hertz, T.CreateChecked(440));

    /// <summary>The MIDI note number nearest to a frequency, with the given frequency for A4 (note 69).</summary>
    public static int MidiNoteNumber<T>(T hertz, T a4) where T : IFloatingPointIeee754<T>
        => int.CreateChecked(T.Round(T.CreateChecked(69) + T.CreateChecked(12) * T.Log2(hertz / a4)));

    /// <summary>The frequency of a MIDI note number with A4 = 440 Hz: 440 · 2^((note − 69) / 12).</summary>
    public static T FrequencyOfMidiNote<T>(int note) where T : IFloatingPointIeee754<T> => FrequencyOfMidiNote(note, T.CreateChecked(440));

    /// <summary>The frequency of a MIDI note number with the given frequency for A4.</summary>
    public static T FrequencyOfMidiNote<T>(int note, T a4) where T : IFloatingPointIeee754<T>
        => a4 * T.Pow(T.CreateChecked(2), T.CreateChecked(note - 69) / T.CreateChecked(12));
}
