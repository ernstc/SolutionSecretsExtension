using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionSecrets.Core
{
    public interface ISolution
    {
        string Name { get; }
        Guid Uid { get; }
    }
}
