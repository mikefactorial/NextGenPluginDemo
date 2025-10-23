using System.ComponentModel.DataAnnotations;

namespace IOTBulbFunctions.Models
{
    public class BulbControlRequest
    {
        [Required]
        public string BulbIP { get; set; } = string.Empty;
        
        [Required]
        public List<ColorStep> Colors { get; set; } = new();
        
        [Required]
        public PatternSettings Pattern { get; set; } = new();
    }

    public class ColorStep
    {
        public string? Hex { get; set; }
        public int? Hue { get; set; }
        public int? Saturation { get; set; }
        public int? Brightness { get; set; }
        public int? Kelvin { get; set; }
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