using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AugPServer.Models
{
    public class ImageModel
    {
        [Required]
        [DisplayName("Image")]
        public string Path { get; set; }
        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }
        [DisplayName("Glyph outside")]
        public bool GlyphOutside { get; set; }
        [Required]
        [DisplayName("Glyph size")]
        public GlyphSizeChoises GlyphSize { get; set; }
        [Required]
        [DisplayName("Glyph position")]
        public GlyphPositionChoises GlyphPosition { get; set; }
    }

    public enum GlyphSizeChoises
    {
        Small,
        Medium,
        Big
    }

    public enum GlyphPositionChoises
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
