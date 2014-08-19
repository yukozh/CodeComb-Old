using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Interface
{
    public interface IRunner
    {
        public string Compiler { get; set; }
        public string CompileArguments { get; set; }
        public string Runner { get; set; }
        public string RunArguments { get; set; }
        public string WorkingDirectory { get; set; }
        public Models.ProcessFeedback Compile();
        public Models.ProcessFeedback Run();
    }
}
