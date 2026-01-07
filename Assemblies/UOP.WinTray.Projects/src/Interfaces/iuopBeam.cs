using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;


namespace UOP.WinTray.Projects.Interfaces
{

    public interface iuopBeam
    {
        //[I]this is an interface - no code here !!
        //^the parent class for channel beams
        // Option Explicit
        public int AssemblyIndex { get; set; }
        public uppBeamTypes BeamType { get; set; }
        public dxfVector Center { get; set; }
        public dxxRadialDirections Direction { get; set; }
        public Single Height { get; set; }
        public colDXFEntities Holes { get; set; }
        public int Index(int RHS);
        
        public Single Length { get; set; }
        public uopSheetMetal Material { get; set; }
        public Single MountFace { get; set; }
        public dxxOrientations Orientation { get; set; }
        public uopPart Part { get; set; }

        public string ProjectHandle();
        public string ProjectHandle(string RHS);
        public string RangeGUID();
        public string RangeGUID(string RHS);

        public object RingSupport { get; set; }
        public colUOPParts RingSupports { get; set; }
        public dxeHole Slot { get; set; }
        public object SupportClip { get; set; }
        public object SupportClips { get; set; }
        public Single Thickness { get; set; }
        public uopTrayAssembly TrayAssembly { get; set; }
        public Single Width { get; set; }
        public Single X { get; set; }

        public void UpdatePartProperties();
        public void UpdatePartWeight();
    }
}
