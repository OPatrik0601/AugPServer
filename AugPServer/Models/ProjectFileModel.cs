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
        [Required]
        [DisplayName("Direct link to the file")]
        public string URLToFile { get; set; }
    }
}
