using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NextGenDemo.Shared.Models
{
    public class BulbControlRequest
    {
        public string BulbIP { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;

        public List<ColorStep> Colors { get; set; } = new List<ColorStep>();
        
        public PatternSettings Pattern { get; set; } = new PatternSettings();
    }

    public class ColorStep
    {
        public string Hex { get; set; }
        public Int32? Hue { get; set; }
        public Int32? Saturation { get; set; }
        public Int32? Brightness { get; set; }
        public Int32? Kelvin { get; set; }
        public int DurationMs { get; set; } = 1000; // Default 1 second
    }

    public class PatternSettings
    {
        public PatternType Type { get; set; } = PatternType.Sequential;
        public int RepeatCount { get; set; } = 1; // 0 for infinite
        public TransitionType Transition { get; set; } = TransitionType.Instant;
        public int TransitionDurationMs { get; set; } = 500;
    }

    public enum PatternType
    {
        Sequential,     // Go through colors in order
        Random,         // Random color selection
        PingPong,       // Forward then backward
        Pulse           // Fade in/out of each color
    }

    public enum TransitionType
    {
        Instant,        // Immediate color change
        Fade,           // Gradual transition (simulated)
        Flash           // Brief flash between colors
    }
}