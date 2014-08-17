using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Entity
{
    public class CommonEnums
    {
        public static string[] LanguageDisplay = { "C", "C++", "C++11", "Java", "Pascal", "Python 2.7", "Python 3.3", "Ruby", "C#", "VB.Net" };
        public static string[] JudgeResultDisplay = { "Accepted", "Presentation Error", "Wrong Answer", "Output Limit Exceeded", "Time Limit Exceeded", "Memory Limit Exceeded", "Runtime Error", "Compile Error", "System Error", "Hacked", "Running", "Pending", "Hidden" };
        public static string[] HackResultDisplay = { "Success", "Failure", "Bad Data", "Data-maker Error", "Running", "Pending" };
    }
    public enum Language { C, Cxx, Cxx11, Java, Pascal, Python27, Python33, Ruby, CSharp, VB };
    public enum JudgeResult { Accepted, PresentationError, WrongAnswer, OutputLimitExceeded, TimeLimitExceeded, MemoryLimitExceeded, RuntimeError, CompileError, SystemError, Hacked, Running, Pending, Hidden };
    public enum HackResult { Success, Failure, BadData, DatamakerError, Running, Pending };
}
