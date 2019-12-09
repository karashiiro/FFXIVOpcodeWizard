using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVOpcodeWizard
{
    class PacketWizard
    {
        public string OpName { get; set; }
        public string Tutorial { get; set; }
        public Func<MetaPacket, string[], bool> PacketCheckerFunc { get; set; }
        public int ParamCount { get; set; }
        public PacketDirection ScanDirection { get; set; }
    }
}
