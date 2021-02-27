using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllanMilne.Ardkit;

namespace PALCompiler
{
    class PALParser : RdParser
    {
        PALParser(IScanner scanner) : base(scanner) { }

        protected override void recStarter() => throw new NotImplementedException();
    }
}
