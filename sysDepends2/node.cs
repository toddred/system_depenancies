using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sysDepends2
{
    public class Node
    {
        public List<string> Parent { get; set; }
        public List<string> Children { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsImplicit { get; set; }

        public Node()
        {
            Parent = new List<string> { };
            Children = new List<string> { };
            IsInstalled = false;
            IsImplicit = false;
        }
    }
}
