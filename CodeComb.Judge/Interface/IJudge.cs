using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Interface
{
    public interface IJudge
    {
        public string GetToken();
        public Models.JudgeFeedback Judge(Models.JudgeTask task);
        public Models.HackFeedback Hack(Models.HackTask task);
        public bool Upload(Models.UploadTask task);
    }
}
