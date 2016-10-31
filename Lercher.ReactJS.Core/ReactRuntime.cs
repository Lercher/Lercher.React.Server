using System;
using System.Linq;

namespace Lercher.ReactJS.Core
{
    public class ReactRuntime
    {
        public readonly JsEnginePool ReactPool;

        public ReactRuntime(JsEnginePool reactPool)
        {
            ReactPool = reactPool;
        }
    }
}
