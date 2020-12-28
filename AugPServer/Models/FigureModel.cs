using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AugPServer.Models
{
    public class FigureModel
    {
        [Required]
        [DisplayName("Model name")]
        public string Name { get; set; }
        [Required]
        [DisplayName(".obj file direct URL")]
        public string ObjPath { get; set; }
        [DisplayName(".mtl file direct URL (optional)")]
        public string MtlPath { get; set; }

        [DisplayName("Attached image")]
        public string ImagePath { get; set; }
        public ImageModel Image { get; set; }
        public IEnumerable<SelectListItem> ImagePaths { get; set; }
    }
}
