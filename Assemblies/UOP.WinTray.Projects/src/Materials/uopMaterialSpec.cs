using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Materials
{
    public class uopMaterialSpec : ICloneable
    {

        private TMATERIALSPEC tStruc;

        public uopMaterialSpec() { tStruc = new TMATERIALSPEC(); }
        internal uopMaterialSpec(TMATERIALSPEC aStructure) { tStruc = aStructure; }

        public uopMaterialSpec Clone() => new uopMaterialSpec(tStruc.Clone());

        object ICloneable.Clone() => (object)this.Clone();

        internal TMATERIALSPEC Structure { get => tStruc; set => tStruc = value; }
        
        public uppSpecTypes SpecType { get =>tStruc.Type; internal set => tStruc.Type = value ; }
        
        public string Name { get => tStruc.Name ; internal set =>tStruc.Name = value; }
    }
}