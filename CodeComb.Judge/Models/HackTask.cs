using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Models
{
    public class HackTask
    {
        public string Token { get; set; }
        public int StatusID { get; set; }
        public int HackID { get; set; }
        public string Code { get; set; }
        public Entity.Language CodeLanguage { get; set; }
        public string SpecialJudgeCode { get; set; }
        public Entity.Language SpecialJudgeCodeLanguage { get; set; }
        public string RangeValidatorCode { get; set; }
        public Entity.Language RangeValidatorCodeLanguage { get; set; }
        public string StandardCode { get; set; }
        public Entity.Language StandardCodeLanguage { get; set; }
        public string DataMakerCode { get; set; }
        public Entity.Language DataMakerCodeLanguage { get; set; }
        public string InputData { get; set; }
        public int TimeLimit { get; set; }
        public int MemoryLimit { get; set; }
    }
}
