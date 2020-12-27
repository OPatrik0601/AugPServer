using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AugPServer.Models
{
    public class SessionModelCollector
    {
        public MetaDataModel MetaData { get; set; }
        public List<ImageModel> UploadedImages { get; set; }
        public List<FigureModel> Figures { get; set; }
        public ProjectFileModel ProjectFile { get; set; }
        public List<AuthorModel> Authors { get; set; }
        public string SessionId { get; set; }
        public string SessionDirectoryPath { get; set; }
        public bool IsFinished { get; set; }
    }
}
