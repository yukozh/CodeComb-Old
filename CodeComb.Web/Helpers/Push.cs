using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Windows;
using PushSharp.WindowsPhone;
using PushSharp.Core;

namespace CodeComb.Web.Helpers
{
    public static class Push
    {
        public static PushBroker push = new PushBroker();
    }
}