using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// represents a startup spout on a spout group
    /// </summary>
    public class mdStartupSpout : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.StartupSpout;

        #region Constructors

        public mdStartupSpout() : base(uppPartTypes.StartupSpout, uppProjectFamilies.uopFamMD, "", "", false) => Init();

        public mdStartupSpout(double aHeight, double aLength) : base(uppPartTypes.StartupSpout, uppProjectFamilies.uopFamMD, "", "", false)
        {
            Init(aHeight, aLength);
        }
        internal mdStartupSpout(mdStartupSpout aPartToCopy, uopPart aParent = null) : base(uppPartTypes.StartupSpout, uppProjectFamilies.uopFamMD, "", "", false)
        {
            Init(0.375, 1, aPartToCopy);

            SubPart(aParent);
        }

        private void Init(double aHeight = 0.375, double aLength = 1, mdStartupSpout aPartToCopy = null)
        {
            Depth = 0;
            Suppressed = false;
            Obscured = false;
            SpoutGroupHandle = string.Empty;
            _ControlLine = new mdStartupLine();
            Center = UVECTOR.Zero;
            MaxY = 0;
            MinY = 0;
            Z = 0;
            Length = aLength;
            Height = aHeight;
            DowncomerIndex = 0;
            BoxIndex = 0;
            OccuranceFactor = 1;
            Tag = string.Empty;
            Index = 0;
            if (aPartToCopy == null) return;
            Copy(aPartToCopy);
            Depth = aPartToCopy.Depth;
            Suppressed = aPartToCopy.Suppressed;
            Obscured = aPartToCopy.Obscured;
            SpoutGroupHandle = aPartToCopy.SpoutGroupHandle;
            _ControlLine = new mdStartupLine(aPartToCopy.ControlLine);
            Center = aPartToCopy.Center;
            _MaxY = aPartToCopy._MaxY;
            _MinY = aPartToCopy._MinY;
            Z = aPartToCopy.Z;
            Length = aPartToCopy.Length;
            Height = aPartToCopy.Height;
            DowncomerIndex = aPartToCopy.DowncomerIndex;
            BoxIndex = aPartToCopy.BoxIndex;
            OccuranceFactor = aPartToCopy.OccuranceFactor;
            Tag = aPartToCopy.Tag;
            Index = aPartToCopy.Index;
        }

        #endregion Constructors

        #region Properties



        /// <summary>
        /// the hole area of the slot that is the spout
        /// </summary>
        public double Area
        {
            get
            {
                if (Height <= 0 || Length <= 0) return 0;

                double rad = 0.5 * Height;
                double _rVal = Math.PI * Math.Pow(rad, 2);
                if (Length > Height) _rVal += (Length - Height) * Height;
                return _rVal;
            }
        }

        public int BoxIndex { get => ControlLine.BoxIndex; set => ControlLine.BoxIndex = value; }

        /// <summary>
        /// the center point of the spout
        /// </summary>
        public override dxfVector CenterDXF { get { dxfVector _rVal = Center.ToDXFVector(false, false, aZ: Z); _rVal.TFVSet(Handle, LeftSide ? "LEFT" : "RIGHT"); return _rVal; } set => Center = UVECTOR.FromDXFVector(value); }

        /// <summary>
        /// the center point of the spout
        /// </summary>
        internal new UVECTOR Center { get => base.CenterV; set  =>  base.CenterV = value;  }

        public override uppSides Side { get => LeftSide ? uppSides.Left : Side = uppSides.Right; }


        private mdStartupLine _ControlLine;
        /// <summary>
        /// the line that was used to create the startup spout
        /// </summary>
        public mdStartupLine ControlLine
        {
            get
            {
                _ControlLine ??= new mdStartupLine();
                _ControlLine.Suppressed = Suppressed;
                
                return _ControlLine;
            }
            set => _ControlLine = new mdStartupLine(value);


        }
        /// <summary>
        /// the line that was used to create the startup spout
        /// </summary>
        public mdStartupLine LimitLine
        {
            get
            {
                mdStartupLine _rVal = new mdStartupLine(ControlLine);
                if (!Suppressed)
                {
                    if (_MaxY != _MinY)
                    {
                        _rVal.sp.Y = _MaxY;
                        _rVal.ep.Y = _MinY;
                    }
                }
                return _rVal;
            }
        }
        /// <summary>
        /// the Depth of the slot which defines the startup spout
        /// </summary>
        public double Depth { get; set; }


        /// <summary>
        /// the index of the downcomer that this startup spout is associated to
        /// </summary>
        public override int DowncomerIndex
        {
            get => ControlLine.DowncomerIndex;
            set { base.DowncomerIndex = value; ControlLine.DowncomerIndex = value; }
        }


        /// <summary>
        ///  the handle of the startup spout
        /// like 2,4,UL
        /// </summary>
        public string Handle
        { 
            get
            {
                string _rVal = SpoutGroupHandle;
                if (_rVal !=  string.Empty) _rVal += ",";

                _rVal += Tag;
                return _rVal;
            }
}

        /// <summary>
        /// flag indicating if the startup is on the left or right side of it's owning downcomer
        /// </summary>
        public bool LeftSide => ControlLine.Side == uppSides.Left;


        /// <summary>
        /// the length of the slot which defines the startup spout
        /// </summary>
        public override double Length
        {
            get=> base.Length;
            set { base.Length = Math.Abs(value);  }
        }


        /// <summary>
        /// the Height of the slot which defines the startup spout
        /// </summary>
        public override double Height
        {
            get => base.Height;
            set { base.Height = Math.Abs(value);  }
        }

        private double _MaxY;
        private double _MinY;

        public double MaxY { get => (_MaxY != _MinY) ? _MaxY : ControlLine.sp.Y - 0.5 * Length; set => _MaxY = value; }

        public double MinY { get => (_MaxY != _MinY)? _MinY :  ControlLine.ep.Y + 0.5 * Length; set=> _MinY = value; }

        public bool Mirrored => ControlLine.MirrorY.HasValue;

        public double? MIrrorY => ControlLine.MirrorY;
        public double? MIrrorX => ControlLine.MirrorX;
        /// <summary>
        /// returns True if the startup is obscured by a finger clip
        /// </summary>
        public bool Obscured { get ; set  ; }
        /// <summary>
        /// the number of times the spout occurs in the entire tray
        /// </summary>
        public override int OccuranceFactor
        {
            get => ControlLine.Occurs;
            
            set
            {
                ControlLine.Occurs = value;
                base.OccuranceFactor = value;
              }
        }

        public override string Name { get => Handle; set => base.Name = Handle; }
        
        /// <summary>
        /// the slot that is the opening of the startup spout
        /// </summary>
        public uopHole Slot => new uopHole(SlotV);

        /// <summary>
        /// the slot that is the opening of the startup spout
        /// </summary>
        internal UHOLE SlotV
        {
            get
            {
                UHOLE SlotV = new UHOLE(Height, X, Y, Length, "STARTUP", aElevation: Z, aFlag: Handle, aZDirection: "1,0,0")
                {
                    ZDirection = "1,0,0",
                    Center = Center,
                    Elevation = Z,
                    Radius = Height / 2,
                    Length = Length,
                    Depth = Depth,
                    Tag = "STARTUP",
                    Flag = Handle
                };
                return SlotV;
            }
        }
        
        /// <summary>
        /// the slots (positive and negative Y) that are the openings of the startup spout
        /// </summary>
        internal UHOLES SlotsV
        {
            get
            {
                UHOLES _rVal =  new UHOLES("");
                UHOLE aSlot = SlotV;
                _rVal.Member = aSlot;
                _rVal.Centers.Add( aSlot.Center);
                if (Mirrored)
                {
                    aSlot.Center.Y -= 2 * (aSlot.Center.Y - ControlLine.MirrorY.Value);
                    _rVal.Centers.Add(aSlot.Center);
                }
                return _rVal;
            }
        }

        private string _SpoutGroupHandle;
        /// <summary>
        /// the handle of the spout group associated to this spout
        /// </summary>
        public string SpoutGroupHandle { get => _SpoutGroupHandle; set => _SpoutGroupHandle = value != null ? value.Trim() : string.Empty; }


        /// <summary>
        /// the tag string that identifies the location of the startup spout with respect to
        /// the center of the parent spout groiups center
        /// like "UL" or "LL"
        /// </summary>
        public override string Tag { get => ControlLine.Tag; set { } } //base.Tag = value != null ? value.Trim() : string.Empty; } }
      

        #endregion Properties

        #region Methods


        public override void UpdatePartWeight() { base.Weight = 0; }


        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdStartupSpout Clone() => new mdStartupSpout(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();
        public override string ToString()
        {
            return $"{base.ToString()} [  {Handle} ]";
        }

        /// <summary>
        /// returns a solid or polyline that is an triangle that points to the center of the spout
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="pWidth"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bSuppressColors"></param>
        /// <param name="bRedIfObscured"></param>
        /// <returns></returns>
        public dxfEntity Pointer(mdTrayAssembly aAssy, double pWidth = 1.25, string aLayerName = "", bool bSuppressColors = false, bool bRedIfObscured = false)
        {
            mdSpoutGroup aSG = null;
            UVECTOR u1 = UVECTOR.Zero;
            dxeSolid aSld = null;
            dxxColors aClr;
            double oset = 0;
            oset = Depth / 2;
            double wd = (pWidth > 0) ? pWidth : 1.25;


            aSG = SpoutGroup(aAssy);
            if (aSG != null) SpoutGroupHandle = aSG.Handle;

            u1 = Center;
            if (!bSuppressColors)
            { aClr = ControlLine.Color; }
            else
            {
                aClr = dxxColors.BlackWhite;
                if (bRedIfObscured)
                {
                    if (!Obscured) aClr = dxxColors.Red;

                }
            }
            aSld = new dxeSolid()
            {
                Filled = !Suppressed,
                Tag = Handle,
                Triangular = true,
                Color = aClr,
                LayerName = aLayerName
            };

            if (LeftSide)
            {
                aSld.Vertex1.SetCoordinates(u1.X - oset, u1.Y);
                aSld.Vertex2.SetCoordinates(u1.X - (wd + oset), u1.Y - 0.5 * wd);
                aSld.Vertex3.SetCoordinates(u1.X - (wd + oset), u1.Y + 0.5 * wd);
                aSld.Flag = "LEFT";
            }
            else
            {
                aSld.Vertex1.SetCoordinates(u1.X + oset, u1.Y);
                aSld.Vertex2.SetCoordinates(u1.X + (wd + oset), u1.Y - 0.5 * wd);
                aSld.Vertex3.SetCoordinates(u1.X + (wd + oset), u1.Y + 0.5 * wd);
                aSld.Flag = "RIGHT";
            }
            return aSld;
        }

        public void SetProps(mdTrayAssembly aAssy, mdStartupLine aControl, mdSpoutGroup aSpoutGroup, bool aDontSetYCenter, double aMirrorLimit)
        {
            if (aSpoutGroup == null || aControl == null) return;

            double lg = Length;
            string aTag = aControl.Tag;
            double patLg = aSpoutGroup.PatternLength;

            double d1 = 0;
            double mxY = aControl.MaxY - 0.5 * lg;
            double mnY = aControl.MinY + 0.5 * lg;
            dxfVector alnPt = new dxfVector(aControl.X, aSpoutGroup.Spouts.BoundaryRectangle.Y, aAssy.Deck.Thickness + 0.625);
            OccuranceFactor = aSpoutGroup.OccuranceFactor;
            //align the spout
            if (aDontSetYCenter)
            {

                alnPt.Y = Y;
            }
            else
            {
                if (lg > aControl.Length)
                {
                    alnPt.Y = aControl.MidPt.Y;
                }
                else
                {
                    if (aTag == "UL" || aTag == "UR")
                    {
                        if (patLg > lg) alnPt.Move(aYChange: 0.5 * patLg - 0.5 * lg);

                        d1 = alnPt.Y - mxY;
                        if (d1 > 0) alnPt.Move(aYChange: -d1);

                        d1 = mnY - alnPt.Y;
                        if (d1 > 0) alnPt.Move(aYChange: d1);


                    }
                    else if (aTag == "LL" || aTag == "LR")
                    {
                        if (patLg > lg) alnPt.Move(aYChange: -0.5 * patLg + 0.5 * lg);

                        d1 = mnY - alnPt.Y;
                        if (d1 > 0) alnPt.Move(aYChange: d1);

                        d1 = alnPt.Y - mxY;
                        if (d1 > 0) alnPt.Move(aYChange: -d1);

                    }
                }
            }
            aSpoutGroup.GetMirrorValues(out double? mirrX, out double? mirrY);
            //set the startup properties
            Suppressed = aControl.Suppressed;
            Center = UVECTOR.FromDXFVector(alnPt);
            Z = alnPt.Z;

            aControl.ReferencePt =  new uopVector(alnPt);
            _ControlLine = new mdStartupLine( aControl);
            Depth = aSpoutGroup.Thickness;
            BoxIndex = aSpoutGroup.BoxIndex;
            DowncomerIndex = aSpoutGroup.DowncomerIndex;
            OccuranceFactor = aSpoutGroup.OccuranceFactor;
            SpoutGroupHandle = aSpoutGroup.Handle;
            OccuranceFactor = aSpoutGroup.OccuranceFactor;
            ControlLine.MirrorY = mirrY;
            ControlLine.MirrorX = mirrX;

           

        }
        /// <summary>
        /// the parent spout group of the startup spout
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public mdSpoutGroup SpoutGroup(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            return aAssy?.SpoutGroups.GetByHandle(SpoutGroupHandle);
        }


        #endregion Methods
    }
}