using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeComb.Judge.Interface
{
    public interface IJudge
    {
        string GetToken();
        Models.JudgeFeedback Judge(Models.JudgeTask task);
        Models.HackFeedback Hack(Models.HackTask task);
        bool Upload(Models.UploadTask task);
    }
}
