using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sysDepends2
{
    public class Node
    {
        public int Location { get; set; }
        public string Name { get; set; }
        public List<int> Children { get; set; }
        public bool IsInstalled { get; set; }

        public Node()
        {
            Location = 0;
            Name = "";
            Children = new List<int> { };
            IsInstalled = false;
        }

        public Node(int location, string name)
        {
            Location = location;
            Name = name;
            Children = new List<int> { };
            IsInstalled = false;
        }
    }
}
