using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AugPServer.Models;

namespace AugPServer.ViewModels
{
    public class EditAllImagesViewModel
    {
        [DisplayName("Glyph outside")]
        public bool? GlyphOutside { get; set; }

        [DisplayName("Glyph position")]
        public GlyphPositionChoises? GlyphPosition { get; set; }

        [DisplayName("Glyph size")]
        public GlyphSizeChoises? GlyphSize { get; set; }
    }
}
