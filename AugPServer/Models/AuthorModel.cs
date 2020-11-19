using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AugPServer.Models
{
    public class AuthorModel
    {
        [Required]
        [DisplayName("Full name")]
        public string FullName { get; set; }

        [DefaultValue("None")]
        public string Affiliation { get; set; }
    }
}
