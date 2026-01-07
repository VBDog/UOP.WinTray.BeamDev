using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    public class mdBeam : uopPart
    {

        public delegate void MDBeamPropertyChange(uopProperty aProperty);
        public event MDBeamPropertyChange eventMDBeamPropertyChange;

        public override uppPartTypes BasePartType => uppPartTypes.TraySupportBeam;
        #region Constructors
        public mdBeam() : base(uppPartTypes.TraySupportBeam, uppProjectFamilies.uopFamMD, "", "", false) { InitializeProperties(); }

        internal mdBeam(mdBeam aPartToCopy, uopPart aParent = null) : base(uppPartTypes.TraySupportBeam, uppProjectFamilies.uopFamMD, "", "", false)
        {
            InitializeProperties();
            if (aPartToCopy != null)
            {
                base.Copy(aPartToCopy);
                _Offset = aPartToCopy._Offset;
                DowncomerSpacing = aPartToCopy.DowncomerSpacing;
                ShellID = aPartToCopy.ShellID;
            }
            SubPart(aParent);

        }


        public void InitializeProperties()
        {

            //eventPartEvent += OPart_eventPartEvent;
            base.Rotation = 45;
            _Offset = null;
            ShellID = 0;
            AddProperty("OffsetFactor", 0.0d);
           AddProperty("Width", 10.0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("Height", 0.0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("WebThickness", 0.5, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("FlangeThickness", 0.5, aUnitType: uppUnitTypes.SmallLength);

            //AddProperty("WebOpeningSize", 0, aUnitType: uppUnitTypes.SmallLength);
            //AddProperty("WebOpeningCount", 0, aUnitType: uppUnitTypes.Undefined);
        }


        #endregion  Constructors


        #region Properties

        public double Clearance => 0.5;

        /// <summary>
        /// the multiplier that sets the beam offset from cetner as a multiple of the parent trays downcomer space
        /// </summary>
        
        public double OffsetFactor { 
            get => PropValD("OffsetFactor"); 
            set => PropValSet("OffsetFactor", Math.Abs(value)); 
        }

        internal double? _Offset;
        public double Offset 
        { 
            get
            {
                double _rVal = 0.0;
                if (!_Offset.HasValue)
                {
                    return mdBeam.OffsetValue(Rotation,DowncomerSpacing, OffsetFactor);
                    
                }else _rVal = _Offset.Value;
                return _rVal;
            }
                
        }
        
        public override double Width { get => PropValD("Width"); set => PropValSet("Width", Math.Abs(value)); }
        
        public override double Height { get => PropValD("Height"); set => PropValSet("Height", Math.Abs(value)); }
        
        public double WebThickness { get => PropValD("WebThickness"); set => PropValSet("WebThickness", Math.Abs(value)); }
        
        public double FlangeThickness { get => PropValD("FlangeThickness"); set => PropValSet("FlangeThickness", Math.Abs(value)); }
       public double TrayDowncomerSpace { get; set; }
        
        public int SupportQuantity { get => TotalQuantity * 2; }

        /// <summary>
        /// the square size of the web opening
        /// </summary>
        /// <remarks> half the height round down to the nearest millimeter (expressed in inches)</remarks>
        public double DefaultWebOpeningSize => Math.Truncate((Height / 2 * 25.4)) / 25.4;

        /// <summary>
        /// the square size of the web opening
        /// </summary>
        /// <remarks> half the height rounded down to the nearest millimeter (expressed in inches)</remarks>
        public double WebOpeningSize  => Math.Truncate((Height / 2 * 25.4)) / 25.4;

        /// <summary>
        /// the radius of the corners on the web opeings
        /// </summary>
        /// <remarks> 1.5 * the web thickness rounded up to the nearest 5 millimeters (expressed in inches)</remarks>
        public double WebOpeningCornerRadius
         { 
            get 
            {
                double _rVal = 1.5 * WebThickness * 25.4;
                int fives = (int)Math.Truncate(_rVal / 5.0);

                if (fives * 5 < _rVal) fives++;
                _rVal = (fives * 5) / 25.4;

                return _rVal;
             }
        }

        public int WebOpeningCount 
        { 
            get 
            {
                double sz = WebOpeningSize;
                return (int)uopUtils.RoundTo((Length - 2 * DistanceToFirstWebOpening - sz) / (2 * sz), dxxRoundToLimits.One, bRoundUp: true);
            }
        }

        public double MountSlotInset => 104 / 25.4;
        public double MountSlotDownInset => 60 / 25.4;

        public uopHole MountSlot => new uopHole(MountSlotV);

        internal UHOLE MountSlotV => new UHOLE(aDiameter: 29 / 25.4, aLength: 50 / 25.4, aDepth: FlangeThickness, aRotation: Rotation, aElevation: Z + 0.5 * FlangeThickness, aInset: MountSlotInset, aDownSet: MountSlotDownInset) { Name = "MOUNT SLOT" };

        public uopHole WebOpening => new uopHole(WebOpeningV);

        internal UHOLE WebOpeningV => new UHOLE(aDiameter: WebOpeningSize, aLength: WebOpeningSize, aDepth: WebThickness, aZDirection: "1,0,0", bIsSquare: true, aCornerRadius: WebOpeningCornerRadius);

        public double WebInset
        {
            get
            {
                var assembly = GetMDTrayAssembly();
                return assembly != null ? assembly.DeckRadius - (assembly.RingID / 2) + 0.5 : DeckRadius - RingID / 2 + 0.5;

            }
        }

        /// <summary>
        /// the distance from the end of the beam to the edge of the first web opening
        /// </summary>
        public double DistanceToFirstWebOpening  =>  Math.Max(2 * Height,0.1 * Length);
            
        public override double Length
        {
            get
            {
                base.Length = uopUtils.ComputeBeamLength(Offset + 0.5 * Width, BoundingRadius, true);
                return base.Length;
            }
            set => base.Length = uopUtils.ComputeBeamLength(Offset + 0.5 * Width, BoundingRadius, true);
        }
        public override double Y
        {
            get
            {
                base.Y = (Offset == 0) ? 0 : Math.Sin(Rotation * Math.PI / 180) * Offset;
                return base.Y;
            }
            set => base.Y = (Offset == 0) ? 0 : Math.Sin(Rotation * Math.PI / 180) * Offset;
        }

        public override double X
        {
            get
            {
                base.X = (Offset == 0) ? 0 : -Math.Cos(Rotation * Math.PI / 180) * Offset;
                return base.X;
            }
            set => base.X = (Offset == 0) ? 0 : -Math.Cos(Rotation * Math.PI / 180) * Offset;
        }

        public override int OccuranceFactor { get { base.OccuranceFactor = OffsetFactor == 0 ? 1 : 2; return base.OccuranceFactor; } set => base.OccuranceFactor =OffsetFactor == 0 ? 1 : 2; }

        public override string INIPath => $"COLUMN({ColumnIndex}).RANGE({RangeIndex}).TRAYASSEMBLY.BEAM";

        public override dxfPlane Plane { get => new dxfPlane(new dxfVector(X, Y), aRotation: Rotation); }
             
        public uopVectors BeamSupportInsertionPoints => new uopVectors(BeamSupportInsertionPoint(true), BeamSupportInsertionPoint(false));
            

        internal USHAPES BlockedAreas
        {
            get
            {
                double rotation = 45.0; //angle is CCW
                USHAPES _rVal = new USHAPES();
                double beamlength = TrayRange.ShellID + Width * 2.0;
                USHAPE blockedArea = USHAPE.Rectangle(0, 0, beamlength, Width);
                USHAPE blockedArea2 = blockedArea.Clone();
                blockedArea.Vertices.Move(Offset);
                blockedArea.Vertices.Rotate(blockedArea.Center, rotation);
                blockedArea.Update();
                _rVal.Add(blockedArea);

                if (Offset > 0.0)
                {
                    blockedArea2.Vertices.Move(-Offset);
                    blockedArea2.Vertices.Rotate(blockedArea.Center, rotation);
                    blockedArea2.Update();
                    _rVal.Add(blockedArea2);
                }

                return _rVal;
            }
        }

        //Get the long edges of the beam(s) in the tray assembly
        internal List<(ULINE topedge, ULINE bottomedge)> LongEdges(double aBeamOffset = 0)
        {
            var _rVal = new List<(ULINE topedge, ULINE bottomedge)>();
            if (IsVirtual)
                return _rVal;


            var myplane = Plane;
            double lng = Length / 2;
            double wd = (Width / 2) + aBeamOffset;
            ULINE l1 = new ULINE(myplane.Vector(-lng, wd), myplane.Vector(lng, wd)) { Tag = "TOP_EDGE", Row = 1 };
            ULINE l2 = new ULINE(myplane.Vector(-lng, -wd), myplane.Vector(lng, -wd)) { Tag = "BOTTOM_EDGE", Row = 1 };
            _rVal.Add((l1, l2));
            if (Offset > 0)
            {
                l1 = new ULINE(myplane.Vector(-lng, -2 * Offset + wd), myplane.Vector(lng, -2 * Offset + wd)) { Tag = "TOP_EDGE", Row = 2 };
                l2 = new ULINE(myplane.Vector(-lng, -2 * Offset - wd), myplane.Vector(lng, -2 * Offset - wd)) { Tag = "BOTTOM_EDGE", Row = 2 };
                _rVal.Add((l1, l2));
            }

            //dxeLine tline = new dxeLine( myplane.Vector(-lng, wd ), myplane.Vector( lng, wd ) );
            //dxeLine bline = new dxeLine( myplane.Vector( -lng, -wd ), myplane.Vector( lng, -wd ) );

            //dxeLine tline2 = tline.Projected( myplane.YDirection, -2 * Offset );
            //dxeLine bline2 = bline.Projected( myplane.YDirection, -2 * Offset );



            //if (Offset > 0)
            //{
            //    _rVal.Add( ((new ULINE( new UVECTOR( tline2.StartPt.X, tline2.StartPt.Y ), new UVECTOR( tline2.EndPt.X, tline2.EndPt.Y ) )),
            //        (new ULINE( new UVECTOR( bline2.StartPt.X, bline2.StartPt.Y ), new UVECTOR( bline2.EndPt.X, bline2.EndPt.Y ) ))) );
            //}

            //double lx = -(Width / 2.0);
            //double rx = Width / 2.0;
            //double ty = uopUtils.ComputeBeamLength( lx, TrayRange.ShellID / 2.0, true );
            //ULINE tedge = new ULINE( lx, ty, lx, -ty );
            //ULINE bedge = new ULINE( rx, ty, rx, -ty );
            //tedge.Move( Offset, 0 );
            //bedge.Move( Offset, 0 );
            //tedge.Points.Rotate( new UVECTOR( 0, 0 ), 45 );
            //bedge.Points.Rotate( new UVECTOR( 0, 0 ), 45 );
            //_rVal.Add( (tedge, bedge) );
            //if (Offset > 0)
            //{
            //    ULINE tedge2 = new ULINE( lx, ty, lx, -ty );
            //    ULINE bedge2 = new ULINE( rx, ty, rx, -ty );
            //    tedge2.Move( -Offset, 0 );
            //    bedge2.Move( -Offset, 0 );
            //    tedge2.Points.Rotate( new UVECTOR( 0, 0 ), 45 );
            //    bedge2.Points.Rotate( new UVECTOR( 0, 0 ), 45 );
            //    _rVal.Add( (tedge2, bedge2) );
            //}
            return _rVal;
        }

        public double DowncomerSpacing { get; internal set; }
        public override double ShellID { get; set; }

        #endregion Properties

        #region Methods

        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.GetType() != typeof(mdBeam)) return false;
            return IsEqual((mdBeam)aPart);
        }

        public bool IsEqual(mdBeam aPart)
        {
            if (aPart == null) return false;
            if (mzUtils.CompareVal(aPart.Width, Width, 4) != mzEqualities.Equals) return false;
            if (mzUtils.CompareVal(aPart.Height, Height, 4) != mzEqualities.Equals) return false;
            if (mzUtils.CompareVal(aPart.Length, Length, 4) != mzEqualities.Equals) return false;
            if (mzUtils.CompareVal(aPart.WebThickness, WebThickness, 4) != mzEqualities.Equals) return false;
            if (mzUtils.CompareVal(aPart.FlangeThickness, FlangeThickness, 4) != mzEqualities.Equals) return false;
            return true;
        }

        public mdBeamSupport BeamSupport(bool bLeft = true) => new mdBeamSupport(this, isLeft: bLeft);
        
        public uopVector BeamSupportInsertionPoint(bool bLeft = true)
        {
            dxfPlane plane = Plane;
          return bLeft ? new uopVector(Plane.Vector(-0.5 * Length + MountSlotInset, 0)) : new uopVector(Plane.Vector(0.5 * Length - MountSlotInset, 0));
        }
        
        public uopShapes FunctionalActiveAreaPolygons()
        {
            uopShapes _rVal = new uopShapes();
            uopArc arc = new uopArc(RingRadius);
            List<uopLinePair> edges = GetEdges(aOffset: 0);
            foreach (var item in edges)
            {
                uopVectors verts = new uopVectors();
                uopVectors ips=arc.Intersections( item.GetSide(aSide: uppSides.Top),true,true);
                verts.AddRange( ips.Sorted(dxxSortOrders.RightToLeft));
                ips = arc.Intersections(item.GetSide(aSide: uppSides.Bottom), true, true);
                verts.AddRange(ips.Sorted(dxxSortOrders.LeftToRight));
                _rVal.Add(new uopShape(verts, aName: "ACTIVE_AREA_BEAM") { Row= Row,Col =Col});

            }
            return _rVal;


        }
        public uopVectors Vertices(bool bGetMirror = false)
        {

            dxfPlane myplane = Plane;

            if (bGetMirror && Offset != 0)
            {
                myplane.Project(myplane.YDirection, -2 * Offset);
            }
            uopVectors _rVal = new uopVectors();
            double lng = Length / 2;
            double wd = Width / 2;
            //vectors defined in WCS
            _rVal.Add(myplane.Vector(-lng, wd, aTag: "TOP"));
            _rVal.Add(myplane.Vector(-lng, -wd, aTag: "BOTTOM"));
            _rVal.Add(myplane.Vector(lng, -wd, aTag: "BOTTOM"));
            _rVal.Add(myplane.Vector(lng, wd, aTag: "TOP"));
            return _rVal;

        }

        /// <summary>
        /// used by an object to respond to property changes of itself and of objects below it in the object model.
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        private void Notify(uopProperty aProperty) { if (aProperty == null || aProperty.Protected || SuppressEvents) return; eventMDBeamPropertyChange?.Invoke(aProperty); }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdBeam Clone() => new mdBeam(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override uopPropertyArray SaveProperties(string aHeading = "")
        {
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
            uopProperties _rVal = new uopProperties(base.ActiveProps, aHeading);
            _rVal.Add(new uopProperty("Offset", Offset, uppUnitTypes.SmallLength, PartType));
            return new uopPropertyArray(_rVal, aName: aHeading, aHeading: aHeading);
        }

        /// extracts the parts property values from the passed file array that was read from an INI style project file.
        /// </summary>
        /// <param name="aProject">The project requesting the read event</param>
        /// <param name="aFileProps">The property array containing the INI file properties or a subset. The Name of the array should contain the original file name.</param>
        /// <param name="ioWarnings">A collection to populate if errors or warnings are found during the property value extraction  </param>
        /// <param name="aFileVersion">The version of th efile being read. Supplied to account for backward compatibility</param>
        /// <param name="aFileSection">the INI file heading to search for the properties to extract </param>
        /// <param name="bIgnoreNotFound">A flag to ignore properties that exist on the part but were not found in the file properties</param>
        /// <param name="aAssy">An optional parent tray assembly for the part being read</param>
        /// <param name="aPartParameter">An optional parent part for the part being read</param>
        /// <param name="aSkipList">An optional list of property names to skip over during the read</param>
        public override void ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null)
        {

            try
            {
                ioWarnings ??= new uopDocuments();
                if (IsVirtual)
                    return;
                    if (this.Properties.Count == 0)
                        InitializeProperties();
                
                uopDocuments warnings = new uopDocuments();
                aFileSection ??= INIPath;
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("The Passed File Section is Undefined");
                if (!aFileProps.TryGet(aFileSection, out uopProperties props)) throw new Exception($"The Passed File Property Array Does Not Contain '{aFileSection}'");

                bool factorfound = props.TryGet("OffsetFactor", out uopProperty factorprop,EqualNames);

                aSkipList ??= new List<string>();
                if (factorfound) aSkipList.Add("OffsetFactor");
                double factor = factorfound ? factorprop.ValueD : 0;

                if (!factorfound)
                {
                    bool offsetfound = props.TryGet("Offset", out uopProperty offsetprop, EqualNames);
                    if (offsetfound)
                    {
                        double? offset = offsetprop.ValueD;
                        if(offset.Value > 0)
                        {
                            factor = mdBeam.DefaultOffsetFactor(ShellRadius, Rotation, DowncomerSpacing, offset);
                            PropValSet("OffsetFactor", Math.Abs(factor), bSuppressEvnts: true);
                        }
                    }
                }
                else
                {
                    PropValSet("OffsetFactor", Math.Abs(factor), bSuppressEvnts: true);
                }

               aSkipList.Add("Offset");
          
                base.ReadProperties(aProject, aFileProps, ref warnings, aFileVersion, aFileSection, bIgnoreNotFound, aAssy, aPartParameter, aSkipList, EqualNames, props);
                ioWarnings.Append(warnings);
            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);
            }
            finally
            {
                Reading = false;
                aProject?.ReadStatus("", 2);
            }
        }

        public uopHoleArray GenHoles(string aTag = "", string aFlag = "") => new uopHoleArray(GenHolesV(aTag, aFlag));

        internal UHOLEARRAY GenHolesV(string aTag = "", string aFlag = "")
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;

            aTag ??= string.Empty;
            aFlag ??= string.Empty;
            aTag = aTag.ToUpper().Trim();
            aFlag = aFlag.ToUpper().Trim();
            double lng = Length / 2;
            double wd = Width / 2;
            dxfPlane plane = Plane;

            if (aTag == "BEAM" || aTag ==  string.Empty)
            {
                UHOLE mount = MountSlotV;
                UHOLES mountslots = new UHOLES(aBaseHole: mount, UVECTORS.Zero) { Name = "BEAM" };
                if (aFlag.Contains("TOP") || aFlag ==  string.Empty)
                {
                    if (aFlag.Contains("LEFT") || aFlag ==  string.Empty)
                        mountslots.Centers.Add(new UVECTOR(plane, aX: -lng + mount.Inset, aY: wd - mount.DownSet, aTag: "TOP", aFlag: "LEFT"));

                    if (aFlag.Contains("RIGHT") || aFlag ==  string.Empty)
                        mountslots.Centers.Add(new UVECTOR(plane, aX: lng - mount.Inset, aY: wd - mount.DownSet, aTag: "TOP", aFlag: "RIGHT"));
                }
                if (aFlag.Contains("BOTTOM") || aFlag ==  string.Empty)
                {
                    if (aFlag.Contains("LEFT") || aFlag ==  string.Empty)
                        mountslots.Centers.Add(new UVECTOR(plane, aX: -lng + mount.Inset, aY: -wd + mount.DownSet, aTag: "BOTTOM", aFlag: "LEFT"));

                    if (aFlag.Contains("RIGHT") || aFlag ==  string.Empty)
                        mountslots.Centers.Add(new UVECTOR(plane, aX: lng - mount.Inset, aY: -wd + mount.DownSet, aTag: "BOTTOM", aFlag: "RIGHT"));
                }


                _rVal.Add(mountslots);

            }

            return _rVal;
        }



        /// <summary>
        /// Returns the polygon for the elevation view of the beam
        /// </summary>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(
            bool bSuppressFillets = false,
            iVector aCenter = null,
            string aLayerName = "GEOMETRY",
            bool suppressRotation = true,
            bool showHoles = true,
            bool angledView = false,
            dxfDisplaySettings beamDisplaySettings = null)
         => mdPolygons.Beam_View_Elevation(this, bSuppressFillets, aCenter, aLayerName, suppressRotation, showHoles, angledView, beamDisplaySettings);

        /// <summary>
        /// Returns the polygon for the plan view of the beam
        /// </summary>
        /// <param name="aCenter"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="bSuppressHoles"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(
            iVector aCenter = null,
            bool bShowObscured = false,
            bool bSuppressHoles = false,
            string aLayerName = "GEOMETRY",
            double aCenterLineLength = 0,
            bool suppressRotation = false)
        => mdPolygons.Beam_View_Plan(this, null, aCenter, bShowObscured, bSuppressHoles, aLayerName, aCenterLineLength, suppressRotation);

        /// <summary>
        /// Returns the polygon for the profile view of the beam
        /// </summary>
        /// <param name="aCenter"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(iVector aCenter = null, string aLayerName = "GEOMETRY", bool suppressRotation = true, double aScaleFactor = 1, double aCenterlineScalefactor = 1.0)
        => mdPolygons.Beam_View_Profile(this, aCenter, aLayerName, suppressRotation, aScaleFactor, aCenterlineScalefactor);

        public  void SubPart(mdTrayAssembly aAssy, string aCategory = null, bool? bHidden = null, double? aDCSpace = null)
        {
            if (aAssy == null)  return;
            base.SubPart(aAssy, aCategory, IsVirtual);
            DowncomerSpacing = aDCSpace.HasValue ? aDCSpace.Value : aAssy.DowncomerSpacing;
            ShellID = aAssy.ShellID; 
            Height = aAssy.RingSpacing + FlangeThickness;
            _Instances = new TINSTANCES() { BasePt = new UVECTOR(Center)};
            IsVirtual = !aAssy.DesignFamily.IsBeamDesignFamily();

            if (Offset != 0 && !IsVirtual) _Instances.Add(-2*X,-2*Y,0);
        }

        public override uopProperty PropValSet(string aName, object aPropVal, int aOccur = 0, bool? bSuppressEvnts = null, bool? bHiddenVal = null)
        {
            bool supEvents = bSuppressEvnts.HasValue ? bSuppressEvnts.Value : SuppressEvents || Reading;

            uopProperty _rVal = base.PropValSet(aName, aPropVal, aOccur, supEvents, bHiddenVal);
            if (_rVal == null || supEvents)
            {
                return _rVal;
            }

            Notify(_rVal);
            return _rVal;
        }

        public List<uopLinePair> GetEdges(double aOffset = 0)
        {
            dxfPlane myplane = Plane;
            double lg = Length / 2;
            double wd = Width / 2 + aOffset;
            List<uopLinePair> _rVal = new List<uopLinePair>();
            if (lg <= 0 || wd <= 0) return _rVal;

            _rVal.Add(new uopLinePair(new uopLine(myplane.Vector(-lg, wd), myplane.Vector(lg, wd), aSide: uppSides.Top), new uopLine(myplane.Vector(-lg, -wd), myplane.Vector(lg, -wd), aSide: uppSides.Bottom)));
            if (Offset != 0)
            {
                dxfPlane p2 = new dxfPlane(myplane);
                p2.Project(p2.YDirection, -2 * Offset);
                _rVal.Add(new uopLinePair(new uopLine(p2.Vector(-lg, wd), p2.Vector(lg, wd), aSide: uppSides.Top), new uopLine(p2.Vector(-lg, -wd), p2.Vector(lg, -wd), aSide: uppSides.Bottom)));

            }
            return _rVal;

        }



        #endregion Methods

        #region Shared Methods

        public static double OffsetValue(  double aBeamRotation, double aDCSpace, double? aSpaceFactor = null)
        {
            double _rVal = 0;
            if (aSpaceFactor <= 0 || aDCSpace <=0) return _rVal;
            double ang = mzUtils.NormAng(aBeamRotation, false, true, true);
            while (ang > 90) ang -= 90;
            //ang = (90 - aBeamRotation) * Math.PI/180;
            //ang = ang * Math.PI / 180;
            _rVal =  ang != 0 ? (0.5 * aDCSpace) / Math.Cos(ang * (Math.PI / 180)) : 0.5 * aDCSpace; 
         
            return  aSpaceFactor.HasValue ? _rVal * aSpaceFactor.Value : _rVal;
        }

        public static List<double> OffsetStepValues(double aShellRadius, double aBeamRotation, double aDCSpace)
        {
            List<double> _rVal = new List<double>() { 0};
            if (aShellRadius <= 0 || aDCSpace <= 0) return _rVal;
            double step = OffsetValue(aBeamRotation,aDCSpace);
            double d1 = step;
            double d2 =1;
            while (d1 < aShellRadius - aDCSpace)
            {
                _rVal.Add(d2);
                d2 += 1;
                d1 += step;
            }
            return _rVal;
        }

        public static double DefaultOffsetFactor( double aShellRadius,  double aBeamRotation, double aDCSpace, double? aOffsetToSnap = null, double? aCurrentOffset = null)
        {
            
            if (aShellRadius <= 0 || aDCSpace <=0) return 0;
            List<double> steps = OffsetStepValues(aShellRadius, aBeamRotation, aDCSpace);
            if(steps.Count <=1 ) return 0;
            int idx = -1;
            if (aOffsetToSnap.HasValue) // get the nearst factor to the target offset value
            {
                double dif = double.MaxValue;
                for(int i = 1; i <= steps.Count; i++)
                {
                    double dif2 =   Math.Abs(aOffsetToSnap.Value - steps[i - 1] * aDCSpace);
                    if(dif2 < dif)
                    {
                        dif = dif2;
                        idx = i - 1;
                    }
                }

            }
            else
            {

            }

            return idx >=0 ? steps[idx]: 0;
        }
        #endregion Shared Methods
    }
}
