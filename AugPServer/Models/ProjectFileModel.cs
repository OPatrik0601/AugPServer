using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AugPServer.Models
{
    public class ProjectFileModel
    {
        public string PathToFile { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        [DisplayName("Paste the copied link here:")]
        public string URLToFile { get; set; }
    }
}
