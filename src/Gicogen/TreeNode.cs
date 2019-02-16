using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicogen
{
    internal class _TreeNode
    {
        public int NodeId { get; set; }
        public int ParentNodeId { get; set; }

        public bool IsFile { get; set; }

        public string Name { get; set; }
        public string NameWithoutExtension { get; set; }
        public string Path { get; set; }
        public DateTime CreationDate { get; set; }

        public int VersionId { get; set; }
        public int FlatPropertyId { get; set; }

        public int BinaryPropertyId { get; set; }
        public int FileId { get; set; }
        public byte[] Stream { get; set; }


        public string Version => "v1.0.a";
        public int UserId => 2;
        public string FileContent { get; set; }
    }
}
