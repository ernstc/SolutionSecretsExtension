using System;
using System.Collections.Generic;

namespace SolutionSecrets.Core.Repository
{
    public class SolutionSettings : ISolution
    {
        public string Name { get; set; }
        public Guid Uid { get; set; }
        public ICollection<(string name, string content)> Settings { get; set; }
    }
}
