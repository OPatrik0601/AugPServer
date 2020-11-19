using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AugPServer.Models
{
    public class MetaDataModel
    {
        [Required]
        [DisplayName("Article name")]
        public string ProjectName { get; set; }
        [Required]
        public string DOI { get; set; }
    }
}
