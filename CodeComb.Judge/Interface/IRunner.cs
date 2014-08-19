using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Interface
{
    public interface IRunner
    {
        string Compiler { get; set; }
        string CompileArguments { get; set; }
        string Runner { get; set; }
        string RunArguments { get; set; }
        string WorkingDirectory { get; set; }
        Models.ProcessFeedback Compile();
        Models.ProcessFeedback Run();
    }
}
