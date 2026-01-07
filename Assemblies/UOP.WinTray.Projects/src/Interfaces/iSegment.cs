using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Interfaces
{
    public interface iSegment
    {
        public uppSegmentTypes SegmentType { get; }
        public bool IsArc { get; }
        public uopArc Arc { get; }
        public uopLine Line { get; }

        public double Radius { get; }
        public double Length { get; }

        public uopVectors Points { get; }

        public iSegment Clone();
    }

}