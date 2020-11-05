using AugPServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AugPServer.Helpers
{
    public class SessionModelCollector
    {
        public MetaDataModel MetaData { get; set; }
        public List<string> UploadedImagePaths { get; set; }
        public List<FigureModel> Figures { get; set; }

        public string SessionDirectoryPath { get; set; }
    }
}
