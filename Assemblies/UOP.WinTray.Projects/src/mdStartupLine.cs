using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Enums;
namespace UOP.WinTray.Projects
{
    /// <summary>
    /// a startup line used to calculate startup spout locations
    /// </summary>
    public class mdStartupLine : uopLine ,  ICloneable
    {

        #region Constructors

        public mdStartupLine() => Init();

        internal mdStartupLine(mdStartupLine aPartToCopy)  => Init(aPartToCopy);

        internal mdStartupLine(ULINE aLine) 
        {
            Init();
            base.Copy(aLine);
          
        }
        public mdStartupLine(uopLine aLine)
        {
            Init();
          if(aLine != null) base.Copy(aLine);

        }
        internal mdStartupLine(TSTARTUPLINE aLine) 
        {
            base.Copy(aLine.Core);

            Tag = aLine.Tag;
            DowncomerIndex = aLine.DowncomerIndex;
            BoxIndex = aLine.BoxIndex;
            SpoutGroupArea = aLine.SpoutGroupArea;
            Occurs = aLine.Occurs;
            SpoutGroupHandle = aLine.SpoutGroupHandle;

            _ReferencePt = new uopVector(aLine.ReferencePt);
            Index = aLine.Index;
            MinLength = aLine.MinLength;

            LineType = aLine.LineType;
            Z = aLine.Z;
            Suppressed = aLine.Suppressed;
            MirrorY = aLine.MirrorX;
            MirrorX = aLine.MirrorY;
            Mark = aLine.Mark;

        }

        private void Init(mdStartupLine aLine = null)
        {
            if(aLine == null)
            {
                DowncomerIndex = 0;
                BoxIndex = 0;
                SpoutGroupArea = 0;
                Occurs = 1;
                SpoutGroupHandle = string.Empty;

                _ReferencePt = uopVector.Zero;
                Index = 0;
                MinLength = 0;
                LineType = string.Empty;
                Z = 0;
                Suppressed = false;
                MirrorY = null;
                MirrorX = null;
                Mark = false;
                Side = uppSides.Left;
                End = uppSides.Top;
            }
            else
            {
                base.Copy(aLine);
                DowncomerIndex = aLine.DowncomerIndex;
                BoxIndex = aLine.BoxIndex;
                SpoutGroupArea = aLine.SpoutGroupArea;
                Occurs = aLine.Occurs;
                SpoutGroupHandle = aLine.SpoutGroupHandle;

                _ReferencePt = new uopVector(aLine.ReferencePt);
                Index = aLine.Index;
                MinLength = aLine.MinLength;
               
                LineType = aLine.LineType;
                Z = aLine.Z;
                Suppressed = aLine.Suppressed;
                MirrorY = aLine.MirrorX;
                MirrorX = aLine.MirrorY;
                Mark = aLine.Mark;
                Side = aLine.Side;
                End = aLine.End;
            }
          
        }

        #endregion Constructors


        private WeakReference<mdStartupLine> _NeightborBelowRef;
         public mdStartupLine NeightborBelow
        {
            get
            {
                if (_NeightborBelowRef == null) return null;
                if (!_NeightborBelowRef.TryGetTarget(out mdStartupLine _rVal)) _NeightborBelowRef = null;
                return _rVal;
            }
            set
            {
                _NeightborBelowRef = value == null ? null : new WeakReference<mdStartupLine>(value);
                
            }
        }
        private WeakReference<mdStartupLine> _NeightborAboveRef;
        public mdStartupLine NeightborAbove
        {
            get
            {
                if (_NeightborAboveRef == null) return null;
                if (!_NeightborAboveRef.TryGetTarget(out mdStartupLine _rVal)) _NeightborAboveRef = null;
                return _rVal;
            }
            set
            {
                _NeightborAboveRef = value == null ? null : new WeakReference<mdStartupLine>(value);
            }
        }
        /// <summary>
        /// Defines the color
        /// </summary>
        public dxxColors Color 
        {   
            get
            {
               
                switch (Tag.ToUpper())
                {
                   case "UL":
                   return  dxxColors.Blue;
                   case "LL":
                   return  dxxColors.Red;
                   case "UR":
                   return  dxxColors.Orange;
                   case "LR":
                   return  dxxColors.Green;
                   default:
                   return dxxColors.BlackWhite;
                }
            }
        }

        public new string Tag 
        { 
            get
            {
                string _rVal = End == uppSides.Top ? "U" : "L";
                _rVal += Side == uppSides.Left ? "L" : "R"; 
                base.Tag = _rVal;
                return _rVal;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    End = value.ToUpper().StartsWith("U") ?  uppSides.Top  : uppSides.Bottom;
                    Side = value.ToUpper().EndsWith("L") ? uppSides.Left : uppSides.Right;
                }
                
            }
        
        }

        public new uppSides Side { get => base.Side; internal set { if (value == uppSides.Left || value == uppSides.Right) base.Side = value; } }

        private uppSides _End;
        public  uppSides End { get => _End; internal set { if (value == uppSides.Top || value == uppSides.Bottom) _End = value; } }

        /// <summary>
        /// returns the downcomer index
        /// </summary>
        public int DowncomerIndex { get; set; }


        /// <summary>
        /// returns the downcomer index
        /// </summary>
        public int BoxIndex { get; set; }

        /// <summary>
        /// retuns the Vector object
        /// </summary>
        internal UVECTOR EPT { get =>  new UVECTOR(ep); set => ep = new uopVector(value); }

        
       
        /// <summary>
        /// provided to carry additional info about the line
        /// </summary>
        public string Handle
        {
            get
            {
                string _rVal = SpoutGroupHandle;
                if ( !string.IsNullOrWhiteSpace(_rVal)) _rVal += ",";
                _rVal += Tag;
                return _rVal;
            }
        }


        public override double MaxYr(int? aPrecis = null)
        {
            if (!Suppressed)
            {
                mdStartupLine below = NeightborAbove;
                return below != null && below.Suppressed ? below.MaxYr(aPrecis) : base.MaxYr(aPrecis);
            } else { return base.MaxYr(aPrecis); }
        }
        public override double MinYr(int? aPrecis = null)
        {
            if (!Suppressed)
            {
                mdStartupLine below = NeightborBelow;
                return below != null && below.Suppressed ? below.MinYr(aPrecis) : base.MinYr(aPrecis);
            }
            else { return base.MinYr(aPrecis); }
        }
        public double MaxLength { get => MaxY - MinY; }
    

        /// <summary>
        /// flag indicating if the startup is on the left or right side of it's owning downcomer
        /// </summary>
        public bool LeftSide => string.IsNullOrWhiteSpace(Tag) ?true:  Tag.ToUpper().EndsWith("L") ;

        /// <summary>
        /// flag indicating if the startup is on the top or bottom side of it's owning spout group
        /// </summary>
        public bool TopSide => string.IsNullOrWhiteSpace(Tag) ? true : Tag.ToUpper().EndsWith("U");


        /// <summary>
        /// the linetype to draw the segment in matches the startpoint linetype
        /// </summary>
        public string LineType { get; set; }


        /// <summary>
        /// returns the Min Length
        /// </summary>
        public double MinLength { get; set; }

        /// <summary>
        /// the number of times the line occurs
        /// </summary>
        public int Occurs { get; set; }

        /// <summary>
        /// returns the Y co ordinate
        /// </summary>
        public double RefPtY { get => ReferencePt.Y; set => ReferencePt.Y = value; }


        private uopVector _ReferencePt;
        /// <summary>
        /// used to carry an object reference for a line
        /// </summary>
        /// 
        public uopVector ReferencePt
        {
            get { _ReferencePt ??= uopVector.Zero; return _ReferencePt; }

            set
            {
                if (value != null)
                {
                    _ReferencePt ??= uopVector.Zero;
                    _ReferencePt.Tag = value.Tag;
                    Z = value.Elevation.HasValue ? value.Elevation.Value : 0;
                    _ReferencePt.X = value.X;
                    _ReferencePt.Y = value.Y;
                }
                else
                {
                    _ReferencePt = uopVector.Zero;
                }
            }
        }
        
        /// <summary>
        /// returns vector
        /// </summary>
        internal UVECTOR SPT {  get => new UVECTOR(sp); set => sp = new uopVector( value); }

        /// <summary>
        /// provided to carry additional info about the line
        /// </summary>
        public double SpoutGroupArea { get; set; }
       
        /// <summary>
        /// the handle of the spout group associated to this line
        /// </summary>
        public string SpoutGroupHandle { get; set; }

     

        /// <summary>
        /// provided to carry additional info about the line
        /// </summary>
        public int SpoutGroupIndex { get; set; }
       
      
        /// <summary>
        /// returns whether line is suppresed
        /// </summary>
        public bool Mark { get; set; }


        public double? MirrorY { get; set; }

        public double? MirrorX { get; set; }

        /// <summary>
        /// returns the X ordinate
        /// </summary>
        public new double X { get => sp.X; set { sp.X = value; ep.X = value; } }

        /// <summary>
        /// returns the Y ordinate
        /// </summary>
        public new double Y 
        { 
            get =>  (sp.Y < ep.Y) ?  sp.Y + Math.Abs(sp.Y - ep.Y)/ 2 : ep.Y + Math.Abs(sp.Y - ep.Y) / 2; 
            set 
            {
                double y = (sp.Y < ep.Y) ? sp.Y + Math.Abs(sp.Y - ep.Y) / 2 : ep.Y + Math.Abs(sp.Y - ep.Y) / 2;
                double dy = value - y;
           
                sp.Y += dy; 
                ep.Y += dy;
            } 
        }


        /// <summary>
        /// returns the Z ordinate
        /// </summary>
        public double Z { get; set; }
        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        public new mdStartupLine Clone() => new mdStartupLine(this);


        public bool CrossesY(double yVal)
        {
            if (yVal >= Math.Round(EPT.Y, 6) && yVal <= Math.Round(SPT.Y, 6)) return true;
            if (yVal >= Math.Round(SPT.Y, 6) && yVal <= Math.Round(EPT.Y, 6)) return true;
            return false;
        }

       
     

        public override string ToString()=> $"mdStartupLine [{ Handle }] X:{X:0.000} LEN:{Length:0.000} SUP:{Suppressed}";
        
        /// <summary>
        /// returns the poly line for display
        /// </summary>
        public dxePolyline DisplayLine(double aHookLength = 0.1)
        {
            colDXFVectors verts = new colDXFVectors();
            aHookLength = Math.Abs(aHookLength);
    
            string lt= Suppressed ? dxfLinetypes.Hidden : dxfLinetypes.Continuous;

            double top = MaxY;
            double bot = MinY;

            switch (Tag.ToUpper())
            {
                case "UL":
                    verts.Add(sp.X - aHookLength, top - aHookLength);
                    verts.Add(sp.X, top);
                    verts.Add(ep.X, bot);
                    verts.Add(ep.X - aHookLength, bot + aHookLength);
                    break;
                case "LL":
                    verts.Add(sp.X - aHookLength, top - aHookLength);
                    verts.Add(sp.X, top);
                    verts.Add(ep.X, bot);
                    verts.Add(ep.X - aHookLength, bot + aHookLength);
                    break;
                default:
                    verts.Add(sp.X + aHookLength, top - aHookLength);
                    verts.Add(sp.X, top);
                    verts.Add(ep.X, bot);
                    verts.Add(ep.X + aHookLength, bot + aHookLength);
                    break;
            }
            return new dxePolyline(verts, bClosed: false, new dxfDisplaySettings(aColor: Color, aLinetype:lt)) { Tag = Tag } ;
        }

        object ICloneable.Clone() => new mdStartupLine(this);
       
    }
}