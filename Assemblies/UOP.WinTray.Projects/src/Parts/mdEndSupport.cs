using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using System.Linq;
using System.Data;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using System.Collections.Generic;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// represents the end support component of an mdDowncomer
    /// </summary>
    public class mdEndSupport : mdBoxSubPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.EndSupport;



        #region Constructors

        public mdEndSupport() : base(uppPartTypes.EndSupport) => Initialize();

        public mdEndSupport(mdDowncomerBox aBox, bool bGetBottomSupport = false) : base(uppPartTypes.EndSupport) => Initialize(aPartToCopy:null, aBox: aBox, bBottomSide: bGetBottomSupport);

        public mdEndSupport(mdEndSupport aPartToCopy) : base(uppPartTypes.EndSupport) => Initialize(aPartToCopy);

        internal override void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox)
        {
            mdEndSupport copy = null;
            if (aPartToCopy != null && aPartToCopy.GetType() == typeof(mdEndSupport)) copy = (mdEndSupport)aPartToCopy;
            Initialize(copy, aBox);
        }

        private bool _Init;
        private void Initialize(mdEndSupport aPartToCopy = null, mdDowncomerBox aBox = null, bool? bBottomSide = null)
        {
            if (!_Init)
            {
                Height = 0;
                Width = 0;
                Length = 0;
                LeftChamfer = 5 / 25.4;
                RightChamfer = 5 / 25.4;
                DeckThickness = 0;
                EndAngleHoleInsetL = 0;
                EndAngleHoleInsetR = 0;

                HasTriangularEndPlate = false;
                WeirHeight = 0;

                RingClipHole1 = null;
                RingClipHole2 = null;
                ParentPartType = uppPartTypes.Downcomer;
                X = 0;
                Y = 0;
                Z = 0;
                _DCRef = null;
                LimitLn = ULINE.Null;
                LapLn = ULINE.Null;
                RingClipHoleU = new UHOLE(mdGlobals.gsBigHole);
                ParentCenterV = UVECTOR.Zero;
                BoxLns = ULINEPAIR.Null;
                DefinitionLns = ULINEPAIR.Null;
                IntersectionType = uppIntersectionTypes.ToRing;
                Side = uppSides.Top;
                EndAngleHoleInsetL = 1;
                EndAngleHoleInsetR = 1;
                _Init = true;
            }

         
         if(aPartToCopy != null)   {

                Copy(aPartToCopy);
                Height = aPartToCopy.Height;
                Width = aPartToCopy.Width;
                Length = aPartToCopy.Length;
                LimitLn = new ULINE(aPartToCopy.LimitLn);
                LapLn = new ULINE(aPartToCopy.LapLn);
                LeftChamfer = aPartToCopy.LeftChamfer;
                RightChamfer = aPartToCopy.RightChamfer;

                SetCoordinates(aPartToCopy.X, aPartToCopy.Y, aPartToCopy.Z);
                RingClipHoleU = new UHOLE(aPartToCopy.RingClipHoleU);
                DeckThickness = aPartToCopy.DeckThickness;
                EndAngleHoleInsetL = aPartToCopy.EndAngleHoleInsetL;
                EndAngleHoleInsetR = aPartToCopy.EndAngleHoleInsetR;

                HasTriangularEndPlate = aPartToCopy.HasTriangularEndPlate;
                WeirHeight = aPartToCopy.WeirHeight;
                ParentPartType = aPartToCopy.ParentPartType;

                RingClipHole1 = new uopVector(aPartToCopy.RingClipHole1);
                RingClipHole2 = new uopVector(aPartToCopy.RingClipHole2);
                ParentCenterV = new UVECTOR(aPartToCopy.ParentCenterV);
                BoxLns = new ULINEPAIR(aPartToCopy.BoxLns);
                DefinitionLns = new ULINEPAIR(aPartToCopy.DefinitionLns);
                IntersectionType = aPartToCopy.IntersectionType;
                EndAngleHoleInsetL = aPartToCopy.EndAngleHoleInsetL;
                EndAngleHoleInsetR = aPartToCopy.EndAngleHoleInsetR;
                BoxIndex = aPartToCopy.BoxIndex;
                aBox ??= aPartToCopy.DowncomerBox;

            }
            if (aBox != null)
            {
             SubPart(aBox);
                Height = aBox.WeirHeight;
                ParentPartType = uppPartTypes.Downcomer;
                Width = aBox.InsideWidth;
                DowncomerIndex = aBox.DowncomerIndex;
                BoxIndex = aBox.Index;
                ParentCenterV = new UVECTOR(aBox.X, aBox.Y);
                bool top = true;
                if (bBottomSide.HasValue) top = !bBottomSide.Value;
                Side = top ? uppSides.Top : uppSides.Bottom;
                double thk = Thickness;

                BoxLns = new ULINEPAIR(aBox.BoxLns);
                IntersectionType = top ? BoxLns.IntersectionType1 : BoxLns.IntersectionType2;
                List<ULINEPAIR> espuplns = aBox.EndSupportLns;
                DefinitionLns = top ? aBox.EndSupportLns.Find((x) => x.Y > aBox.Y) : aBox.EndSupportLns.Find((x) => x.Y < aBox.Y);


                LimitLn = DefinitionLns.GetTaggedValue("LIMIT");
                LapLn = DefinitionLns.GetTaggedValue("LAP");

                SubPart(aBox);
                X = aBox.X;
                Y = LimitLn.MidPt.Y;
                Z = 0;
                ParentCenterV = aBox.CenterV;

                Length = Math.Sqrt(Math.Pow(aBox.DeckRadius, 2) - Math.Pow(X - ((Width + 2 * Thickness) / 2), 2));
                //Length -= aBox.ShortWeirLength / 2;

                Quantity = 2 * aBox.OccuranceFactor;

                RingClipHoleU = aBox.RingClipHoleU;
                DeckThickness = aBox.DeckThickness;
                EndAngleHoleInsetL = aBox.EndAngleHoleInsetL;
                EndAngleHoleInsetR = aBox.EndAngleHoleInsetR;

                HasTriangularEndPlate = aBox.HasTriangularEndPlate;
                WeirHeight = aBox.How + DeckThickness;
                OverridePartNumber = BottomSide ? "BOTTOM" : "TOP";
                OverridePartNumber = $"{aBox.PartNumber} - {OverridePartNumber}";


                RingClipHole1 = RingClipHoleCount >= 1 ? new uopVector(LimitLn.Points.Item(1)) : null;
                RingClipHole2 = RingClipHoleCount >= 2 ? new uopVector(LimitLn.Points.Item(2)) : null;
                //if (BottomSide)
                //{
                //    if(RingClipHoleCount >= 1) RingClipHole1.Y = BoxY - (RingClipHole1.Y - BoxY);
                //    if (RingClipHoleCount >= 2) RingClipHole2.Y = BoxY - (RingClipHole2.Y - BoxY);
                //}

            }
        }

        #endregion Constructors

        /// <summary>
        /// the direction of the support (up or down)
        /// </summary>
        /// <remarks>If the lap line is above the limit line the direction is up, otherwise it is down</remarks>
        public override dxxOrthoDirections Direction { get { base.Direction = LapLn.MidY < LimitLn.MidY ? dxxOrthoDirections.Down : dxxOrthoDirections.Up; return base.Direction; } set { base.Direction = LapLn.MidY < LimitLn.MidY ? dxxOrthoDirections.Down : dxxOrthoDirections.Up; } }

        #region Properties
 

        public uopLine LimitLine => new uopLine(LimitLn);

        internal ULINE LimitLn { get; set; }

        public uopLine LapLine => new uopLine(LapLn);

        internal ULINE LapLn { get; set; }
        internal ULINEPAIR BoxLns { get; set; }

        internal ULINEPAIR DefinitionLns { get; set; }

        public uppIntersectionTypes IntersectionType { get; set; }

        public override uppPartTypes ParentPartType { get { base.ParentPartType = uppPartTypes.Downcomer; return base.ParentPartType; } set { base.ParentPartType = uppPartTypes.Downcomer; } }

     
        public bool BottomSide => Y < BoxY;

        //^True if the parent downcomer is at X = 0
        public bool IsSquare => Math.Round(X, 1) == 0;
        public bool HasTriangularEndPlate { get; set; }

        public double LeftChamfer { get; set; }
        public double RightChamfer { get; set; }


        /// <summary>
        /// the height of the short side bends in the support
        /// </summary>
        public override double Height { get => base.Height; set => base.Height = Math.Abs(value); }

        /// <summary>
        /// the overall length of the part
        /// </summary>
        public override double Length { get => base.Length; set => base.Length = Math.Abs(value); }



        //^the hole used to attach the support to the ring
        public uopHole RingClipHole
        {
            get
            {
                mdDowncomer aDC = GetMDDowncomer();
                return aDC?.RingClipHole();
            }
        }



        public double DeckThickness { get; set; }

        /// <summary>
        ///the Weir Height of the support including the deck thickness
        /// </summary>
        public double WeirHeight { get; set; }

        /// <summary>
        ///the width of the downcomer box. equal to the Width property of the parent downcomer.
        /// </summary>
        public override double Width { get => base.Width; set => base.Width = Math.Abs(value); }

        internal UHOLE RingClipHoleU { get; set; }
        internal UHOLE EndAngleHoleU => new UHOLE(mdGlobals.gsBigHole, 0, 0, 0, "END ANGLE", Thickness, aInset: 0.625, aElevation: DeckThickness + 0.625);



        public double EndAngleHoleInsetL { get; set; }
        public double EndAngleHoleInsetR { get; set; }


        public int RingClipHoleCount => LimitLn.Points.Count;

        public uopVector RingClipHole1 { get; set; }
        public uopVector RingClipHole2 { get; set; }

        public override dxfPlane Plane => X >= 0 ? new dxfPlane(new dxfVector(X, Y), new dxfDirection(1, 0, 0), new dxfDirection(0, 1, 0)) : new dxfPlane(new dxfPlane(new dxfVector(X, Y), new dxfDirection(1, 0, 0), new dxfDirection(0, 1, 0)));


 
        #endregion Properties

        #region Methods

        public double BoxLength(bool bLongSide = false) => BoxLns.GetSideValue(bLongSide ? uppSides.Left : uppSides.Right).Length;

        public override string ToString() { return $"END SUPPORT DC:{DowncomerBox.Index} BOX:{BoxIndex} SIDE:{Side}"; }

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        /// <summary>
        /// returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdEndSupport Clone() => new mdEndSupport(this);


        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }

        private void UpdateRingClipHoles()
        {



            RingClipHole1 = RingClipHoleCount >= 1 ? new uopVector(LimitLn.Points.Item(1)) : null;
            RingClipHole2 = RingClipHoleCount >= 2 ? new uopVector(LimitLn.Points.Item(2)) : null;
        }

        public uopHoleArray GenHoles(string aTag = "", string aFlag = "") => new uopHoleArray(GenHolesV(aTag, aFlag));

        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bTimesTwo"></param>
        /// <returns></returns>
        internal UHOLEARRAY GenHolesV(string aTag = "", string aFlag = "")
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;

            aTag ??= string.Empty;
            aFlag ??= string.Empty;
            aTag = aTag.ToUpper().Trim();
            aFlag = aFlag.ToUpper().Trim();

            UHOLE aHole;

            double thk = Thickness;
            double xl = X - 0.5 * Width;
            double xr = X + 0.5 * Width;

            var leftWeirLine = WeirLn(bLeft: true);
            var rightWeirLine = WeirLn(bLeft: false);

            double yR, yL;
            if (Side == uppSides.Top)
            {
                yR = rightWeirLine.MaxY;
                yL = leftWeirLine.MaxY;
            }
            else
            {
                yR = rightWeirLine.MinY;
                yL = leftWeirLine.MinY;
            }

            UVECTOR u1;
            UHOLES aHls = UHOLES.Null;

            //first the ring clip holes
            if (aTag == "RING CLIP" || aTag ==  string.Empty)
            {
                if (RingClipHoleCount <= 0)
                    UpdateRingClipHoles();

                aHole = new UHOLE(RingClipHoleU)
                {
                    Elevation = 0.5 * thk,
                    Depth = thk
                };
                aHls.Member = aHole;


                if (RingClipHole1 != null)
                {

                    u1 = new UVECTOR(RingClipHole1)
                    {
                        Elevation = aHole.Elevation
                    };
                    aHole.Center = u1;
                    aHls.Centers.Add(u1);

                }

                if (RingClipHole2 != null)
                {

                    u1 = new UVECTOR(RingClipHole2)
                    {
                        Elevation = aHole.Elevation
                    };
                    aHole.Center = u1;

                    aHls.Centers.Add(u1);

                }
                _rVal.Add(aHls, "RING CLIP");
            }

            //end angle holes
            if (aTag == "END ANGLE" || aTag ==  string.Empty)
            {
                aHls = UHOLES.Null;

                UHOLE hole1 = new UHOLE(EndAngleHoleU);
                hole1.Center.DownSet = hole1.Inset;

                double isetL = EndAngleHoleInsetL;
                double isetR = EndAngleHoleInsetR;
                mzUtils.SwapTwoValues(ref isetL, ref isetR, X < 0);

                hole1.Flag = "LEFT";
                hole1.Center.X = xl - 0.5 * thk;
                hole1.Inset = isetL;
                hole1.Center.Y = Side == uppSides.Top ? yL - hole1.Inset : yL + hole1.Inset;


                aHls.Member = hole1;

                UHOLE hole2 = new UHOLE(EndAngleHoleU);
                hole2.Center.DownSet = hole1.Center.DownSet;
                hole2.Flag = "RIGHT";
                hole2.Center.X = xr + 0.5 * thk;
                hole2.Inset = isetR;
                hole2.Center.Y = Side == uppSides.Top ? yR - hole2.Inset : yR + hole2.Inset;

                if (aFlag == "LEFT" || aFlag ==  string.Empty)
                {
                    aHole = hole1.Flag == "LEFT" ? hole1 : hole2;
                    u1 = aHole.Center;
                    aHls.Centers.Add(u1, "LEFT");

                }


                if (aFlag == "RIGHT" || aFlag ==  string.Empty)
                {
                    aHole = hole1.Flag == "RIGHT" ? hole1 : hole2;
                    u1 = aHole.Center;
                    aHls.Centers.Add(u1, "RIGHT");

                }

                if (aHls.Centers.Count > 0)
                {
                    _rVal.Add(aHls, "END ANGLE");
                }
            }
            return _rVal;
        }

        public override void UpdatePartProperties()
        {
            DescriptiveName = $"End Support (DC {DowncomerIndex})";
        }

        public override void UpdatePartWeight()
        {
            base.Weight = Weight();
        }
        /// <summary>
        ///returns a dxePolygon that is used to draw the elevation view of the end support
        /// </summary>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aHoleClines"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(bool bSuppressFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", string aHoleClines = "")
       => mdPolygons.EndSupport_View_Elevation(this, bSuppressFillets, aCenter, aRotation, aLayerName, aHoleClines);

        /// <summary>
        ///returns a dxePolygon that is used to draw the plan view of the part
        /// </summary>
        /// <param name="aAssy">the parent tray assembly for this part</param>
        /// <param name="aDC">the parent downcomer for this part</param>
        /// <param name="bSuppressHoles">flag to suppress the holes</param>
        /// <param name="aCenterLineLength"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aScale"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(bool bSuppressHoles = false, double aCenterLineLength = 0, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", double aScale = 1, bool bIncludeFilletFoints = true)
        => mdPolygons.EndSupport_View_Plan(this, bSuppressHoles, aCenterLineLength, aCenter, aRotation, aLayerName, aScale, bIncludeFilletFoints);

        /// <summary>
        ///returns the max y ordinate of the end support
        /// </summary>
        /// <param name="aAssy">the parent tray assembly for this part</param>
        /// <param name="aDC">the parent downcomer for this part</param>
        /// <returns></returns>
        public double maxY(mdTrayAssembly aAssy, mdDowncomer aDC)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return 0;
            aDC = GetMDDowncomer(aAssy, aDC);

            if (aDC == null) return 0;
            return Math.Sqrt(Math.Pow(aAssy.DeckRadius, 2) - Math.Pow(aDC.X - (aDC.Width * 0.5), 2));
        }
        /// <summary>
        ///returns a dxePolygon that is used to draw the profile view of the end support
        /// </summary>
        /// <param name="aAssy">the parent tray assembly for this part</param>
        /// <param name="aDC">the parent downcomer for this part</param>
        /// <param name="bShowObscured">flag to add hidden lines where the profile is obscured</param>
        /// <param name="bLongSide"></param>
        /// <param name="bVisiblePartOnly"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(mdTrayAssembly aAssy, mdDowncomer aDC, bool bShowObscured = false, bool bLeftSide = true, bool bVisiblePartOnly = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        => mdPolygons.EndSupport_View_Profile(this, aAssy, aDC, bShowObscured, bLeftSide, bVisiblePartOnly, aCenter, aRotation, aLayerName);

        /// <summary>
        ///returns a dxePolygon that is used to draw the right view of the end support
        /// </summary>
        /// <param name="aAssy">the parent tray assembly for this part</param>
        /// <param name="aDC">the parent downcomer for this part</param>
        /// <param name="aPlanView"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public dxePolygon View_LayoutRight(dxePolygon aPlanView = null, bool bSuppressHoles = false, iVector aCenter = null, double aRotation = 0)
       => mdPolygons.EndSupport_View_LayoutRight(this, aPlanView, bSuppressHoles, aCenter, aRotation);

        /// <summary>
        ///returns a dxePolygon that is used to draw the left view of the end support
        /// </summary>
        /// <param name="aAssy">the parent tray assembly for this part</param>
        /// <param name="aDC">the parent downcomer for this part</param>
        /// <param name="aPlanView"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public dxePolygon View_LayoutLeft(dxePolygon aPlanView = null, bool bSuppressHoles = false, iVector aCenter = null, double aRotation = 0)
        => mdPolygons.EndSupport_View_LayoutLeft(this, aPlanView, bSuppressHoles, aCenter, aRotation);

        public new double Weight(mdDowncomer aDC = null, mdTrayAssembly aAssy = null)
        {

            //^returns the weight of the end support in english pounds

            aDC = GetMDDowncomer(aAssy, aDC);

            if (aDC == null) return 0;

            double thk = Thickness;
            double ht = Height;

            dxePolyline Perim = new dxePolyline(Vertices(), bClosed: true);
            try
            {



                double sArea = Perim.Area;
                sArea += WeirLn(bLeft: true).Length * WeirHeight;
                sArea += WeirLn(bLeft: false).Length * WeirHeight;

                sArea -= GenHolesV().TotalArea();

                return sArea * SheetMetalWeightMultiplier;
            }
            catch
            {
                return 0;
            }
        }


        private WeakReference<mdDowncomer> _DCRef;
        public override mdDowncomer GetMDDowncomer(mdTrayAssembly aAssy = null, mdDowncomer aDC = null, int aIndex = -1)
        {
            if (aDC != null) return aDC;
            if (_DCRef != null)
            {
                if (!_DCRef.TryGetTarget(out aDC)) _DCRef = null; else return aDC;
            }

            aAssy ??= GetMDTrayAssembly(aAssy);
            aIndex = (aIndex < 0) ? DowncomerIndex : aIndex;
            if (aAssy == null) return null;
            if (aIndex < 1 || aIndex > aAssy.Downcomers.Count) return null;
            aDC = aAssy.Downcomers.Item(aIndex);
            _DCRef = new WeakReference<mdDowncomer>(aDC);
            return aDC;
        }

        public override bool IsEqual(uopPart aPart)
        {

            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;

            try
            {
                return CompareTo((mdEndSupport)aPart, true);


            }
            catch { return false; }
        }

        public bool CompareTo(mdEndSupport aPart, bool bComareMaterial = true)
        {

            if (aPart == null) return false;


            try
            {



                if (aPart.IsSquare != IsSquare) return false;
                if (aPart.BottomSide != BottomSide) return false;

                if (bComareMaterial)
                    if (!aPart.Material.IsEqual(Material)) return false;

                if (!TVALUES.CompareNumeric(Math.Abs(aPart.X), Math.Abs(X), 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.Width, Width, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.Height, Height, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.Length, Length, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.DeckThickness, DeckThickness, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.EndAngleHoleInsetL, EndAngleHoleInsetL, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.EndAngleHoleInsetR, EndAngleHoleInsetR, 3)) return false;

                if (!TVALUES.CompareNumeric(aPart.BoxLength(bLongSide: false), BoxLength(bLongSide: false), 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.BoxLength(bLongSide: true), BoxLength(bLongSide: true), 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.RingClipHoleU.Diameter, RingClipHoleU.Diameter, 3)) return false;


                return true;

            }
            catch { return false; }
        }

        /// <summary>
        /// the line that is the weir on the indicated side of the support
        /// </summary>
        /// <param name="bLeft"></param>
        /// <returns>uopLine</returns>
        public uopLine WeirLine(bool bLeft) => new uopLine(WeirLn(bLeft));
        /// <summary>
        /// the line that is the weir on the indicated side of the support
        /// </summary>
        /// <param name="bLeft"></param>
        /// <returns>ULINE</returns>
        internal ULINE WeirLn(bool bLeft)
        {
            uppSides side = bLeft ? uppSides.Left : uppSides.Right;

            ULINE l1 = new ULINE(BoxLns.GetSideValue(aSide: side));

            double x1 = l1.sp.X;
            UVECTOR u1 = Direction == dxxOrthoDirections.Up ? new UVECTOR(x1, l1.MaxY + mdEndSupport.WeldGap) : new UVECTOR(x1, l1.MinY - mdEndSupport.WeldGap);
            UVECTOR u2 = LapLn.EndPoints.GetVector(side == uppSides.Left ? dxxPointFilters.AtMinX : dxxPointFilters.AtMaxX);
            return Direction == dxxOrthoDirections.Up ? new ULINE(x1, u1.Y, x1, u2.Y, aSide: side) : new ULINE(x1, u2.Y, x1, u1.Y, aSide: side);

        }

        /// <summary>
        /// returns the vertices which define the perimeter of the support
        /// </summary>
        /// <returns></returns>
        public uopVectors Vertices() => new uopVectors(GetVertices());

        internal UVECTORS GetVertices()
        {
            UVECTORS _rVal = UVECTORS.Zero;
            ULINE weirL = WeirLn(bLeft: true);
            ULINE lap = LapLn.Clone();
            ULINE weirR = WeirLn(bLeft: false);
            ULINE lim = LimitLn.Clone();
            if (lap.sp.X > lap.ep.X) lap.Invert();
            if (lim.sp.X < lim.ep.X) lim.Invert();

            UVECTORS lappts = LapLn.EndPoints;
            UVECTORS limpts = LimitLn.EndPoints;

            if (Direction == dxxOrthoDirections.Down) { weirL.Invert(); weirR.Invert(); }
            UVECTOR u1 = UVECTOR.Zero;
            double f1 = Direction == dxxOrthoDirections.Up ? 1 : -1;
            double thk = Thickness;
            //comput the chamfer lengths
            double defchmf = 5 / 25.4;
            LeftChamfer = defchmf;
            RightChamfer = defchmf;
            double slope = Math.Round(LimitLn.Slope, 3);

            dxeArc fArc;

            uopVector v1 = new uopVector(lim.ep).Moved(aY: 100 * f1);
            uopVector v2 = new uopVector(lim.ep);
            uopVector v3 = new uopVector(lim.sp);
            fArc = dxfPrimatives.CreateFilletArc(thk, v1, v2, v3);
            double d1 = v2.DistanceTo(fArc?.EndPt);
            d1 = uopUtils.RoundTo(d1, dxxRoundToLimits.Millimeter, bRoundUp: true);
            LeftChamfer = Math.Max(defchmf, d1);



            if (slope != 0)
            {

                v1 = new uopVector(lim.ep);
                v2 = new uopVector(lim.sp);
                v3 = v2.Moved(aY: 100 * f1);
                fArc = dxfPrimatives.CreateFilletArc(thk, v1, v2, v3);
                d1 = v2.DistanceTo(fArc?.StartPt);
                d1 = uopUtils.RoundTo(d1, dxxRoundToLimits.Millimeter, bRoundUp: true);
                RightChamfer = Math.Max(defchmf, d1);
            }
            else
            {
                RightChamfer = LeftChamfer;
            }

            u1 = _rVal.Add(weirL.ep);
            u1 = _rVal.Add(lap.sp);
            u1 = _rVal.Add(lap.ep);
            u1 = _rVal.Add(weirR.ep);
            u1 = _rVal.Add(weirR.sp);
            u1 = _rVal.Add(u1.Moved(-thk));
            u1 = _rVal.Add(u1.Moved(aYAdder: mdEndSupport.NotchDepth * f1));
            u1 = _rVal.Add(u1.Moved(-thk));

            u1 = _rVal.Add(lim.sp.Moved(aYAdder: RightChamfer * f1));
            u1 = _rVal.Add(lim.sp + lim.Direction() * RightChamfer);
            u1 = _rVal.Add(lim.ep + lim.Direction() * -LeftChamfer);
            u1 = _rVal.Add(lim.ep.Moved(aYAdder: LeftChamfer * f1));
            u1 = _rVal.Add(weirL.sp.Moved(2 * thk, mdEndSupport.NotchDepth * f1));
            u1 = _rVal.Add(u1.Moved(-thk));
            u1 = _rVal.Add(weirL.sp.Moved(thk));
            u1 = _rVal.Add(weirL.sp);

            return _rVal;

        }

        #endregion Methods

        #region Shared Properties
        /// <summary>
        ///the gap allowed between the downcomer box and end support to allow for a butt weld 
        /// </summary>
        public static double WeldGap => 0.125;
        /// <summary>
        /// the notch in the support that receives the end plate
        /// </summary>
        public static double NotchDepth => 0.905;

        #endregion Shared Properties
    }
}
