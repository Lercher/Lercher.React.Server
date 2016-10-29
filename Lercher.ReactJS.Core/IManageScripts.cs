using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ClearScript.V8;

namespace Lercher.ReactJS.Core
{
    public interface IManageScripts
    {
        IEnumerable<string> Scripts { get; }
        void Close();
    }
}
