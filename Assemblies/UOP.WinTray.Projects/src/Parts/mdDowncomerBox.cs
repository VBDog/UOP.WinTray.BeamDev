using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.DXFGraphics.Utilities;
using System.Linq;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.Projects.src.Utilities.ExtensionMethods;
using static System.Net.WebRequestMethods;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// the box component of a mdDowncomer
    /// </summary>
    public class mdDowncomerBox : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.DowncomerBox;

        #region Constructors

        public mdDowncomerBox() : base(uppPartTypes.DowncomerBox, uppProjectFamilies.uopFamMD, "", "", true) { ParentPartType = uppPartTypes.Downcomer; Initialize(); }


        public mdDowncomerBox(mdDowncomer aDowncomer, int aRow = 1, int aIndex = 1) : base(uppPartTypes.DowncomerBox, uppProjectFamilies.uopFamMD, "", "", true) => Initialize(null, aDowncomer, aRow, aIndex);


        //public mdDowncomerBox(mdDowncomerBox aPartToCopy, int indexWithinDowncomer = 1) : base(uppPartTypes.DowncomerBox, uppProjectFamilies.uopFamMD, "", "", true) => Initialize(indexWithinDowncomer, aPartToCopy);
        public mdDowncomerBox(mdDowncomerBox aPartToCopy) : base(uppPartTypes.DowncomerBox, uppProjectFamilies.uopFamMD, "", "", true) => Initialize(aPartToCopy);

        private void Initialize(mdDowncomerBox aPartToCopy = null, mdDowncomer aDowncomer = null, int? aRow = null, int? aIndex = null)
        {
            
            FoldOverWeirs = false;
            EndPlateOverhang = 0.25;
            FoldOverHeight = 0;
            SetCoordinates(0, 0, 0);
            BaffleMountHeight = 0;
            BaffleHeight = 0;
            SupplementalDeflectorHeight = 0;
            BoltOnEndplates = false;
            DeckThickness = 0;
            ShelfWidth = 1;
            WeldedStiffeners = true;

            HasCrossBraces = false;
            HasAntiPenetrationPans = false;
            BottomInstall = false;
            StartUpSitesL = string.Empty;
            StartUpSitesR = string.Empty;
            StartupLength = 0;
            StartupDiameter = 0;
         
            FingerClipHole = mdGlobals.FingerClipHole(null);

           
            Bolting = uppUnitFamilies.Metric;
            HasTriangularEndPlate = false;

            OccuranceFactor = 1;
            VirtualChildPartNumber = string.Empty;

            BoxLns = new ULINEPAIR();
            LimitLns = new ULINEPAIR();
            WeirLns = new ULINEPAIR();
            ShelfLns = new ULINEPAIR();
            SupplementalDeflectorHeight = 0;
            DesignFamily = uppMDDesigns.MDDesign;
            ParentInfo = new DowncomerInfo(null, null, 0);
            Index = aIndex.HasValue ? aIndex.Value : 1;
            GussetedEndplates = false;
            IntersectionType_Top = uppIntersectionTypes.ToRing;
            IntersectionType_Bot = uppIntersectionTypes.ToRing;
            PanelClearance = 0.0825;
            _StiffenerYs = null;

            if (aPartToCopy != null)
            {
                base.Copy(aPartToCopy);

                DesignFamily = aPartToCopy.DesignFamily;
                FoldOverWeirs = aPartToCopy.FoldOverWeirs;
                FoldOverHeight = aPartToCopy.FoldOverHeight;

                BoltOnEndplates = aPartToCopy.BoltOnEndplates;
                StartupLength = aPartToCopy.StartupLength;
                StartupDiameter = aPartToCopy.StartupDiameter;
                WeldedStiffeners = aPartToCopy.WeldedStiffeners;
                HasCrossBraces = aPartToCopy.HasCrossBraces;
                HasAntiPenetrationPans = aPartToCopy.HasAntiPenetrationPans;
                StartUpSitesL = aPartToCopy.StartUpSitesL;
                StartUpSitesR = aPartToCopy.StartUpSitesR;
                BottomInstall = aPartToCopy.BottomInstall;
                Bolting = aPartToCopy.Bolting;
                HasTriangularEndPlate = aPartToCopy.HasTriangularEndPlate;
                OccuranceFactor = aPartToCopy.OccuranceFactor;
                ShelfWidth = aPartToCopy.ShelfWidth;
                FingerClipHole = new UHOLE(aPartToCopy.FingerClipHole);
               
               
                VirtualChildPartNumber = aPartToCopy.VirtualChildPartNumber;
                aDowncomer ??= aPartToCopy.Downcomer;
                DowncomerIndex = aPartToCopy.Index;
                ParentInfo = new DowncomerInfo(aPartToCopy.ParentInfo);
                LimitLns = new ULINEPAIR(aPartToCopy.LimitLns);
                BoxLns = new ULINEPAIR(aPartToCopy.BoxLns);
                WeirLns = new ULINEPAIR(aPartToCopy.WeirLns);
                ShelfLns = new ULINEPAIR(aPartToCopy.ShelfLns);
                EndSupportLns = uopLinePairs.Copy(aPartToCopy.EndSupportLns);
                SupplementalDeflectorHeight = aPartToCopy.SupplementalDeflectorHeight;
                EndPlateOverhang = aPartToCopy.EndPlateOverhang;
                Index = aIndex.HasValue ? aIndex.Value : aPartToCopy.Index;
                GussetedEndplates = aPartToCopy.GussetedEndplates;
                IntersectionType_Top = aPartToCopy.IntersectionType_Top;
                IntersectionType_Bot = aPartToCopy.IntersectionType_Bot;
                RingClipHoleU = new UHOLE(aPartToCopy.RingClipHoleU);
                PanelClearance = aPartToCopy.PanelClearance;
                BaffleMountHeight = aPartToCopy.BaffleMountHeight;
                BaffleHeight = aPartToCopy.BaffleHeight;
                if (aPartToCopy._StiffenerYs != null)_StiffenerYs = new List<double>(aPartToCopy._StiffenerYs);
            }
            Row = aRow.HasValue ? aRow.Value : 1;
            if (aDowncomer != null)
            {

                base.SubPart(aDowncomer);

                if (aRow.HasValue) Row = aRow.Value;
                int row = Row;
                DowncomerIndex = aDowncomer.Index;
         
                BaffleHeight = aDowncomer.BaffleHeight;
                mdTrayAssembly assy = aDowncomer.GetMDTrayAssembly();
                uppMDDesigns designfam = aDowncomer.DesignFamily;
                if(assy != null)
                {
                    designfam = assy.DesignFamily;
                     mdGlobals.BaffleMountSlot(assy,out double topz, out double mountht, out bool supbaf);
                    BaffleHeight =  assy.BaffleHeight;
                    BaffleMountHeight = mountht;
                }
                DesignFamily = designfam;

                ShelfWidth = aDowncomer.ShelfWidth;

                DowncomerInfo dcinfo = aDowncomer.CurrentInfo();
                //get the definition lines
                ParentInfo = new DowncomerInfo(dcinfo);
                BoxLns = new ULINEPAIR(dcinfo.BoxLns.Find((x) => x.Row == row));
                LimitLns = new ULINEPAIR(dcinfo.LimLines.Find((x) => x.Row == row));
                WeirLns = new ULINEPAIR(dcinfo.WeirLns.Find((x) => x.Row == row));
                ShelfLns = new ULINEPAIR(dcinfo.ShelfLns.Find((x) => x.Row == row));
                EndSupportLns = uopLinePairs.Copy(dcinfo.EndSupportLns.FindAll((x) => x.Row == row));
                IntersectionType_Top = LimitLns.IntersectionType1;
                IntersectionType_Bot = LimitLns.IntersectionType2;

                Row = row;
                _DCRef = new WeakReference<mdDowncomer>(aDowncomer);
                X = aDowncomer.X;
                Y = aDowncomer.Y;

                EndPlateOverhang = aDowncomer.EndPlateOverhang;
                FoldOverHeight = aDowncomer.FoldOverHeight;
                Width = aDowncomer.BoxWidth;
                How = aDowncomer.How;
                FoldOverWeirs = aDowncomer.FoldOverHeight > 0;
                Height = aDowncomer.OutsideHeight;
                SheetMetalStructure = aDowncomer.SheetMetalStructure;
                Quantity = aDowncomer.OccuranceFactor;
                ParentPartType = uppPartTypes.Downcomer;
                //LongShelfAngle = aDowncomer.ShelfAngle(bLongSide: true);
                //ShortShelfAngle = aDowncomer.ShelfAngle(bLongSide: false);

                if (IntersectionType_Top == uppIntersectionTypes.ToRing && IntersectionType_Bot == uppIntersectionTypes.ToRing)
                {
                    BoltOnEndplates = aDowncomer.BoltOnEndplates || Math.Round(X, 1) == 0;
                }

                DeckThickness = aDowncomer.DeckThickness;
                StartupLength = aDowncomer.StartupLength;
                StartupDiameter = aDowncomer.StartupDiameter;
                DowncomerIndex = aDowncomer.Index;
                GussetedEndplates = aDowncomer.GussetedEndplates;
                if (aIndex.HasValue) Index = aIndex.Value;
                WeldedStiffeners = true;

                WeldedStiffeners = assy == null || assy.DesignOptions.WeldedStiffeners;
                HasCrossBraces = assy != null && assy.DesignOptions.CrossBraces;

                FingerClipHole = assy != null ? mdGlobals.FingerClipHole( assy) : new UHOLE(mdGlobals.gsBigHole, 0, 0, 0, "FINGER CLIP", Thickness, aElevation: DeckThickness + 0.625, aZDirection: "1,0,0", aInset: 0.625, aDownSet: WeirHeight - DeckThickness + 0.625);
            
                HasAntiPenetrationPans = assy != null && assy.HasAntiPenetrationPans;
                BottomInstall = assy != null && assy.DesignOptions.BottomInstall && DesignFamily.IsStandardDesignFamily();
                Bolting = assy != null ? assy.Bolting : uppUnitFamilies.Metric;
                HasTriangularEndPlate = aDowncomer.HasTriangularEndPlate;
                PanelClearance = assy != null ? assy.PanelClearance(true) : 0.0825;

                StartUpSitesL = aDowncomer.StartUpSitesL;
                StartUpSitesR = aDowncomer.StartUpSitesR;

                SupplementalDeflectorHeight = aDowncomer.SupplementalDeflectorHeight;
                Y = BoxLns.Y;

                RingClipHoleU = new UHOLE(aDowncomer.RingClipHoleU);
                EndAngleHoleInsetL = aDowncomer.EndAngleHoleInset(true, null);
                EndAngleHoleInsetR = aDowncomer.EndAngleHoleInset(false, null);

                _StiffenerYs = mdDowncomerBox.StiffenerYValues(this, aDowncomer);

                if (IsVirtual)
                {
                    OccuranceFactor = 1;
                }
                else
                {
                    if (designfam.IsStandardDesignFamily())
                    {
                        OccuranceFactor = aDowncomer.OccuranceFactor;
                    }
                    else if (designfam.IsBeamDesignFamily())
                    {
                        OccuranceFactor = 2;
                    }
                    else if (designfam.IsDividedWallDesignFamily())
                    {
                        OccuranceFactor = 1;
                    }
                    else
                    {
                        OccuranceFactor = aDowncomer.OccuranceFactor;
                    }

                    _Instances.Clear();
                    if (OccuranceFactor == 2)
                    {
                        _Instances.Add(-2 * X, -2 * Y, 180);
                    }
                }


            }
            else
            {
                _DCRef = null;
            }

        }

        #endregion Constructors


        #region Properties

        public override uopInstances Instances
        {
            get
            {
                return base.Instances;
            }
            set
            {
                base.Instances = value;
            }
        }

        public double EndAngleHoleInsetL { get; set; }

        public double EndAngleHoleInsetR { get; set; }


        public override int DowncomerIndex { get => base.DowncomerIndex; set { base.DowncomerIndex = value; base.Col = value; } }

        public double SupplementalDeflectorHeight { get; set; }


        /// <summary>
        /// Flag indicating that supplemental deflector is required
        /// </summary>
        public bool HasSupplementalDeflector => SupplementalDeflectorHeight > 0;


        public mdSupplementalDeflector SupplementalDeflector => !HasSupplementalDeflector ? null : new mdSupplementalDeflector(this); 


        public override int Col { get => DowncomerIndex; set => DowncomerIndex = value; }

        internal DowncomerInfo ParentInfo { get; set; }

        /// <summary>
        ///the line pair that carries the top and bottom limit line
        /// </summary>
        internal ULINEPAIR LimitLns { get; set; }
        /// <summary>
        ///the line pair that carries the left and right weir lines
        /// </summary>
        internal ULINEPAIR WeirLns { get; set; }
        /// <summary>
        ///the line pair that carries the left and right box lines
        /// </summary>
        internal ULINEPAIR BoxLns { get; set; }
        /// <summary>
        ///the line pair that carries the left and right Shelf Lines
        /// </summary>
        internal ULINEPAIR ShelfLns { get; set; }

        /// <summary>
        ///the line pair that carries the top and bottom end support lines
        /// </summary>
        internal List<ULINEPAIR> EndSupportLns { get; set; }


        /// <summary>
        ///the line pair that carries the top and bottom limit line
        /// </summary>
        public uopLinePair LimitLines => new uopLinePair(LimitLns);
        /// <summary>
        ///the line pair that carries the left and right weir lines
        /// </summary>
        public uopLinePair WeirLines => new uopLinePair(WeirLns);
        /// <summary>
        ///the line pair that carries the left and right box lines
        /// </summary>
        public uopLinePair BoxLines => new uopLinePair(BoxLns);
        /// <summary>
        ///the line pair that carries the top and bottom end support lines
        /// </summary>
        public List<uopLinePair> EndSupportLines => uopLinePair.FromList(EndSupportLns);
        /// <summary>
        ///the line pair that carries the left and right shelf lines
        /// </summary>
        public uopLinePair ShelfLines => new uopLinePair(ShelfLns);

        /// <summary>
        /// the long box length if the downcomer used triangular end plates
        ///equal to the parent downcomers long length less 0.5 inches
        /// </summary>
        public double LongLength => BoxLns.MaxLength;


        public string StartUpSitesL { get; private set; }
        public string StartUpSitesR { get; private set; }


        public override uopMaterial Material => (uopMaterial)SheetMetal;

        public string VirtualChildPartNumber { get; set; }

        /// <summary>
        /// the parent downcomer of this box
        /// </summary>
        public mdDowncomer Downcomer
        {
            get => GetMDDowncomer();
            set { if (value != null) Initialize(this, value); }
        }

        public double InsideWidth { get => Width - 2 * Thickness; }

        public double FoldOverHeight { get; set; }

        public bool FoldOverWeirs { get; set; }

        public bool BoltOnEndplates { get; set; }


        public bool HasTriangularEndPlate { get; set; }

        public bool WeldedStiffeners { get; set; }

        public bool BottomInstall { get; set; }

        public bool HasCrossBraces { get; set; }

        public bool HasAntiPenetrationPans { get; set; }

        public double ShelfWidth { get; set; }

        public override uppPartTypes ParentPartType { get { base.ParentPartType = uppPartTypes.Downcomer; return base.ParentPartType; } set { base.ParentPartType = uppPartTypes.Downcomer; } }

        /// <summary>
        /// the height of the box
        /// same as parent downcomers outside height
        /// </summary>
        public override double Height { get => base.Height; set => base.Height = value; }
        public double InsideHeight  => Height - Thickness;
        /// <summary>
        /// the distance between the bottom of the downcomer and the deck of the tray below for downcomers within the range
        /// </summary>
        public double HeightAboveDeck => RingSpacing - (Height  - WeirHeight);
        /// <summary>
        /// the distance between the deck that the downcomer supports and the bottom of the downcomer
        /// </summary>
        public double HeightBelowDeck => Height - How;

        /// <summary>
        /// the weir height of the parent dowcomer
        /// </summary>
        public double How { get; set; }

        /// <summary>
        /// the weir height of the parent dowcomer plus a deck material thickness
        /// </summary>
        public double WeirHeight => How + DeckThickness;

        public double DeckThickness { get; set; }
        /// <summary>
        /// the box length
        /// equal to the parent downcomers length less 0.5 inches
        /// </summary>
        public override double Length { get { base.Length = ShortLength; return base.Length; } set => base.Length = ShortLength; }


        /// <summary>
        /// the box length
        /// equal to the parent downcomers length less 0.5 inches
        /// </summary>

        public double ShortLength => BoxLns.MinLength;
        public double LeftLength => BoxLns.SideLength(uppSides.Left);
        public double RightLength => BoxLns.SideLength(uppSides.Right);

        /// <summary>
        /// the sum of the long side and the short side
        /// </summary>
        public double TotalLength => LeftLength + Length;


        /// <summary>
        /// the center of the left upright wall of then downcomer
        /// </summary>
        public double WallCenterLeft => X - 0.5 * Width + 0.5 * Thickness;


        /// <summary>
        /// the center of the left upright wall of then downcomer
        /// </summary>
        public double WallCenterRight => X + 0.5 * Width - 0.5 * Thickness;


        /// <summary>
        /// the width of the downcomer box
        /// equal to the width property of the parent downcomer plus 2 times the material thickness (outside width)
        /// </summary>
        public override double Width { get => base.Width; set => base.Width = value; }

        public double StartupLength { get; set; }

        internal UHOLE FingerClipHole { get; set; }

     
        internal UHOLE BaffleMountSlot => mdGlobals.BaffleMountSlot(GetMDTrayAssembly(), Thickness);

        public double StartupDiameter { get; set; }

        public double X_Inside_Left => X - InsideWidth / 2;
        public double X_Inside_Right => X + InsideWidth / 2;

        public double X_Outside_Left => X - Width / 2;
        public double X_Outside_Right => X + Width / 2;
        public double EndPlateOverhang { get; set; } = 0.25;

        /// <summary>
        /// the true end-to-end length of the downcomer
        /// ~this is calculated on request and is read only
        /// </summary>
        public double LongAssemblyLength
        {
            get
            {
                double rad = DeckRadius;
                if (rad <= 0) return 0;
                double x1 = Math.Abs(X) - 0.5 * Width;
                return 2 * Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x1, 2));

            }
        }

        public override bool IsVirtual
        {
            get
            {
                bool _rVal = false;

                if (DesignFamily.IsStandardDesignFamily())
                {
                    _rVal = Math.Round(X, 3) < 0;
                }
                else
                {
                    _rVal = Row != ParentInfo.MaxRow;
                }

                if (DesignFamily.IsBeamDesignFamily())
                {
                    if (ParentInfo.DividerCount == 2 && Row == 2)
                        _rVal = Math.Round(X, 3) < 0;
                }
                base.IsVirtual = _rVal;
                return _rVal;
            }
            set => base.IsVirtual = value;
        }

        public bool GussetedEndplates { get; set; }

        public uppIntersectionTypes IntersectionType_Top { get; set; }
        public uppIntersectionTypes IntersectionType_Bot { get; set; }


        /// <summary>
        /// Indicates that the crossbrace and ap pan attachments nuts should be welded to the bottom
        /// of the downcomer because it is too narrow for installation.
        /// </summary>
        public bool WeldedBottomNuts => InsideWidth <= 4 || (HasSupplementalDeflector && InsideWidth <= 8);

        internal UHOLE RingClipHoleU { get; set; }

        public double PanelClearance { get; set; }

        public double BaffleMountHeight { get; set; }
     
     
        public double BaffleHeight { get; set; }

        /// <summary>
        /// the hole used to attach to attach the end angles to the end support
        /// </summary>
        public uopHole EndAngleHole
       => new uopHole(mdGlobals.gsBigHole, 0, 0, 0, "END ANGLE", Thickness, aInset: 0.625, aElevation: DeckThickness + 0.625);
        /// <summary>
        /// the slot used to attach to attach the end angles at the ends of the box
        /// </summary>
        public uopHole EndAngleSlot
        {
            get
            {
                uopHole _rVal = new uopHole(mdGlobals.gsBigHole, 0, 0, aLength: 22 / 25.4, aDepth: Thickness, aTag: "END ANGLE", aZDirection: "1,0,0", aInset: 1);
                if (Math.Round(X, 1) == 0) _rVal.Inset = 0.776;
                return _rVal;
            }
        }
        #endregion Properties

        #region Methods
        public List<uopRingClipSegment> RingClipSegments()
        {
            List<uopRingClipSegment> _rVal = new List<uopRingClipSegment>();


            double rad = this.BoundingRadius;
            if (rad <= 0) return _rVal;
            uopLinePair limits = LimitLines;
            uopLinePair sidelimits  = BoxLines;
            if (!limits.IsDefined() || !sidelimits.IsDefined() ) return _rVal;
            double buf = 1.5;
            
            uopLine top = limits.GetSide(uppSides.Top, bGetClone:true);
            uopLine bot = limits.GetSide(uppSides.Bottom, bGetClone: true);
            uopLine left = sidelimits.GetSide(uppSides.Left, bGetClone: true);
            uopLine right = sidelimits.GetSide(uppSides.Right, bGetClone: true);
            left.Move(buf);
            right.Move(-buf);
            left.Stretch(left.Length, uppSegmentPoints.MidPt);
            right.Stretch(right.Length, uppSegmentPoints.MidPt);

            sidelimits = new uopLinePair(left, right);

            if (top.Points.Count == 1)
            {
                top.Move(0, top.Points.Item(1).Y - top.MidPt.Y, false);
                top.ExtendTo(sidelimits, bTrimTo: true);
                _rVal.Add(new uopRingClipSegment(top));
            }
            else if (top.Points.Count == 2)
            {
                top.sp.SetCoordinates ( top.Points.Item(1).X,top.Points.Item(1).Y);
                top.ep.SetCoordinates(top.Points.Item(2).X, top.Points.Item(2).Y);
                top.ExtendTo(sidelimits, bTrimTo: true);
                _rVal.Add(new uopRingClipSegment(top));
            }

            if (bot.Points.Count == 1)
            {
                bot.Move(0, bot.Points.Item(1).Y - bot.MidPt.Y, false);
                bot.ExtendTo(sidelimits, bTrimTo: true);
                _rVal.Add(new uopRingClipSegment(bot));
            }
            else if (bot.Points.Count == 2)
            {
                bot.sp.SetCoordinates(bot.Points.Item(1).X, bot.Points.Item(1).Y);
                bot.ep.SetCoordinates(bot.Points.Item(2).X, bot.Points.Item(2).Y);
                bot.ExtendTo(sidelimits, bTrimTo: true);
                _rVal.Add(new uopRingClipSegment(bot));
            }
        

          
            return _rVal;
        }
        public bool HasAngledEndPlates(bool? bTop = null)
        {
            uopLinePair lims = LimitLines;
            if (lims.Count < 2) return false;
            if (bTop.HasValue)
                return Math.Round(lims.GetSide(bTop.Value ? uppSides.Top : uppSides.Bottom).DeltaY, 3) != 0;
            else
                return Math.Round(lims.GetSide(uppSides.Top).DeltaY, 3) != 0 || Math.Round(lims.GetSide(uppSides.Bottom).DeltaY, 3) != 0;


        }

        /// <summary>
        /// the perpendicular distance between the ringclip hole and the outside edge of the endplate
        /// </summary>
        /// <param name="rClrc1"></param>
        /// <param name="rClrc2"></param>
        /// <param name="rHoleCnt"></param>
        public void GetClipClearances(out double rClrc1, out double rClrc2, out int rHoleCnt)
        {
            rClrc1 = 0;
            rClrc2 = 0;
            rHoleCnt = 0;

            uopLinePair limits = LimitLines;
            uopLinePair sidelimits = BoxLines;
            if (!limits.IsDefined() || !sidelimits.IsDefined()) return;

            uopLine top = limits.GetSide(uppSides.Top);
            uopLine bot = limits.GetSide(uppSides.Bottom);
            uopLine left = sidelimits.GetSide(uppSides.Left, bGetClone:true);
            uopLine right = sidelimits.GetSide(uppSides.Right, bGetClone: true);
            left.Move(Thickness);
            right.Move(-Thickness);
            sidelimits = new uopLinePair(left, right);

            if (bot.Points.Count > 0)
            {
                uopVector u1 = top.Points.Item(1, bSuppressIndexErrors: true);
                uopVector u2 = top.Points.Item(2, bSuppressIndexErrors: true);
                if (u1.X > u2.X)
                {
                    uopVector u3 = u1;
                    u1 = u2;
                    u2 = u3;
                    if (u2 == null)
                    {
                        rHoleCnt = 1;
                        rClrc1 = u1.DistanceTo(left);
                    }
                    else
                    {
                        rHoleCnt = 2;
                        rClrc1 = u1.DistanceTo(left);
                        rClrc2 = u2.DistanceTo(right);
                      
                    }
                }

                return;
           
            }

            if (bot.Points.Count > 0)
            {
                uopVector u1 = bot.Points.Item(1, bSuppressIndexErrors: true);
                uopVector u2 = bot.Points.Item(2, bSuppressIndexErrors: true);
                if (u1.X > u2.X)
                {
                    uopVector u3 = u1;
                    u1 = u2;
                    u2 = u3;
                    if (u2 == null)
                    {
                        rHoleCnt = 1;
                        rClrc1 = u1.DistanceTo(left);
                    }
                    else
                    {
                        rHoleCnt = 2;
                        rClrc1 = u1.DistanceTo(left);
                        rClrc2 = u2.DistanceTo(right);

                    }
                }

                return;

            }
        }

        public double BoxLength(bool bLeftSide) => BoxLn(bLeftSide).Length;
        

        /// <summary>
        /// the distance the end angle holes are inset from the end of the angle
        /// varies from 1'' to 0.5'' to clear the endsupport weld
        /// </summary>
        /// <param name="bLeftSide"></param>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double EndAngleHoleInset(bool bLeftSide, bool bTop, mdTrayAssembly aAssy = null)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);


            double rad = (aAssy != null) ? aAssy.DeckRadius : DeckRadius;
            if (rad <= 0) return 0;


            ULINE bxline = BoxLn(bLeftSide);

            double x1 = bLeftSide ? X - 0.5 * Width : X + 0.5 * Width + Thickness;

            double yb =  0.5 * BoxLength(bLeftSide);
            double y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x1, 2));
            yb += mdEndSupport.WeldGap + mdEndSupport.NotchDepth;

            double clrc = Math.Abs(y1) - Math.Abs(yb);

            double hRad = EndAngleHole.Radius;

            //make sure the hole clears the weld below by at least 0.25''
            if (clrc - 1 - hRad >= 0.25)
            { return 1; }
            else if (clrc - 0.75 - hRad >= 0.25)
            { return 0.75; }
            else if (clrc - 0.625 - hRad >= 0.25)
            { return 0.625; }
            else
            { return 0.5; }


        }

       

        internal List<double> _StiffenerYs = null;
        public List<double> StiffenerYs(mdDowncomer aDowncomer = null) { _StiffenerYs ??= mdDowncomerBox.StiffenerYValues(this,aDowncomer); return _StiffenerYs; }


        internal UVECTORS StiffenerPoints(mdDowncomer aDowncomer = null)
        {

            UVECTORS _rVal = UVECTORS.Zero;
            aDowncomer ??= Downcomer;
            if (aDowncomer == null) return _rVal;

            uopVectors bxPts = BoxLines.EndPoints();
            if (bxPts.Count == 4)
            {

                double x = X;
                List<double> stiffYs = StiffenerYs(aDowncomer);
           
                foreach (var item in stiffYs)
                {
                      _rVal.Add(new UVECTOR(x, item));
                }

            }
            return _rVal;


        }

        public uopVectors FingerClipPoints(mdTrayAssembly aAssy, List<mdStiffener> aDCStiffeners, uppSides aSide = uppSides.Undefined, bool bUnsuppressedOnly = true, colDXFVectors aCollector = null, bool bGetEndAnglePts = false)
         => new uopVectors(FingerClipPts(aAssy,aDCStiffeners,aSide,bUnsuppressedOnly,aCollector,bGetEndAnglePts));
        

        internal UVECTORS FingerClipPts(mdTrayAssembly aAssy, List<mdStiffener> aDCStiffeners, uppSides aSide = uppSides.Undefined, bool bUnsuppressedOnly = true, colDXFVectors aCollector = null, bool bGetEndAnglePts = false)
        {
            UVECTORS _rVal = UVECTORS.Zero;

            aAssy ??= this.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            mdDowncomer downcomer = this.Downcomer;
           
            aDCStiffeners ??= mdPartGenerator.Stiffeners_ASSY(aAssy, false, DowncomerIndex);


            List<mdDeckSection> manSecs = aAssy.DeckSections.Manways;
            int dcindex = downcomer.Index;
            double thk = downcomer.Thickness;
            int occur = downcomer.OccuranceFactor;
            double dcX = X;
            UHOLES eaholes = UHOLES.Null;

            if (bGetEndAnglePts)
            {
                if (this.GenHolesV(aAssy, "END ANGLE").TryGet("END ANGLE", out eaholes))
                {
                    if (aSide == uppSides.Undefined || aSide == uppSides.Left)
                    {
                        if (eaholes.TryGetByFlag("UL", out UHOLE eahole))
                        {
                            UVECTOR u1 = new UVECTOR(eahole.Center)
                            {
                                PartIndex = dcindex,
                                Col = occur,
                                Row = dcindex,
                                Tag = "LEFT"
                            };


                            _rVal.Add(u1, aCollector: aCollector);
                        }
                    }
                    if (aSide == uppSides.Undefined || aSide == uppSides.Right)
                    {
                        if (eaholes.TryGetByFlag("UR", out UHOLE eahole))
                        {
                            UVECTOR u1 = new UVECTOR(eahole.Center)
                            {
                                PartIndex = dcindex,
                                Col = occur,
                                Row = dcindex,
                                Tag = "RIGHT"
                            };

                            _rVal.Add(u1, aCollector: aCollector);

                        }

                    }
                }
                else { bGetEndAnglePts = false; }

            }

            foreach (mdStiffener item in aDCStiffeners)
            {
                if (!IsValidStiffenerOrdinate(item.Y))
                {
                    continue;
                }


                if (Math.Round(item.X, 1) != Math.Round(dcX, 1))
                    continue;


                UVECTORS Stfs = bUnsuppressedOnly ? item.FingerClipPts : mdUtils.FingerClipPoints(aAssy, item, manSecs, downcomer, bReturnSuppressed: true);

                for (int k = 1; k <= Stfs.Count; k++)
                {
                    UVECTOR u1 = Stfs.Item(k);
                    UVECTOR u2 = new UVECTOR(u1)
                    {
                        PartIndex = dcindex,
                        Col = occur,
                        Row = dcindex,
                        Tag = u1.X < dcX ? "LEFT" : "RIGHT",
                        X = u1.X < dcX ? u1.X - thk : u1.X + thk,
                    };


                    if (this.IsVirtual)
                    {
                        UVECTOR u2Mirror = new UVECTOR(u2)
                        {
                            Tag = u2.Tag == "LEFT" ? "RIGHT" : "LEFT",
                            X = -u2.X,
                            Y = -u2.Y,
                        };

                        if (u2Mirror.Tag == "LEFT" && (aSide == uppSides.Undefined || aSide == uppSides.Left))
                            _rVal.Add(u2Mirror, aCollector: aCollector);
                        if (u2Mirror.Tag == "RIGHT" && (aSide == uppSides.Undefined || aSide == uppSides.Right))
                            _rVal.Add(u2Mirror, aCollector: aCollector);
                    }
                    else
                    {
                        if (u1.Tag == "LEFT" && (aSide == uppSides.Undefined || aSide == uppSides.Left))
                            _rVal.Add(u2, aCollector: aCollector);
                        if (u1.Tag == "RIGHT" && (aSide == uppSides.Undefined || aSide == uppSides.Right))
                            _rVal.Add(u2, aCollector: aCollector);
                    }
                }
            }


            if (bGetEndAnglePts)
            {


                if (aSide == uppSides.Undefined || aSide == uppSides.Left)
                {
                    if (eaholes.TryGetByFlag("LL", out UHOLE eahole))
                    {

                        UVECTOR u1 = new UVECTOR(eahole.Center)
                        {
                            PartIndex = dcindex,
                            Col = occur,
                            Row = dcindex,
                            Tag = "LEFT"
                        };


                        _rVal.Add(u1, aCollector: aCollector);

                    }

                }
                if (aSide == uppSides.Undefined || aSide == uppSides.Right)
                {
                    if (eaholes.TryGetByFlag("LR", out UHOLE eahole))
                    {
                        UVECTOR u1 = new UVECTOR(eahole.Center)
                        {
                            PartIndex = dcindex,
                            Col = occur,
                            Row = dcindex,
                            Tag = "RIGHT"
                        };

                        _rVal.Add(u1, aCollector: aCollector);
                    }

                }
            }


            return _rVal;
        }


        /// <summary>
        /// the functional active area for the downcomer box
        /// </summary>
        /// <param name="rNoTrapezoids"></param>
        /// <returns></returns>
        public double FunctionalActiveArea(out double rNoTrapezoids)
        {
            rNoTrapezoids = 0;
            uopShape active = FunctionalActiveAreaPolygon();
            double _rVal = active.Area;
            if(HasTriangularEndPlate)
                if (HasAngledEndPlates()) rNoTrapezoids = FunctionalActiveAreaPolygon_NoAngles().Area;
            return _rVal;
        }

        /// <summary>
        /// the active area polygon for the downcomer box
        /// </summary>
        public uopShape FunctionalActiveAreaPolygon_NoAngles()
        {
            uopVectors verts = FunctionalActiveAreaPolygon().Vertices;
            uopVector v1 = verts.GetVector(dxxPointFilters.GetLeftTop);
            uopVector v2 = verts.GetVector(dxxPointFilters.GetLeftBottom);
            uopVector v3 = verts.GetVector(dxxPointFilters.GetRightBottom);
            uopVector v4 = verts.GetVector(dxxPointFilters.GetRightTop);

            if(v1 == null || v2 == null || v3 == null || v4 == null) return new uopShape(verts, "ACTIVE AREA_NOTRAP") { Row = Row, Col = Col }; ;
            if (v1.Y > v4.Y)
            {
                v1.Y = v4.Y;
            }
            else if (v4.Y > v1.Y)
            {
                v4.Y = v1.Y;
            }
            if (v2.Y < v3.Y)
            {
                v2.Y = v3.Y;
            }
            else if (v2.Y > v3.Y)
            {
                v3.Y = v2.Y;
            }


            return new uopShape(new uopVectors(v1,v2,v3,v4), $"ACTIVE AREA_NO_ANGLES DC:{DowncomerIndex} BOX:{Index}") { Row = Row, Col = Col }; 
        }

        /// <summary>
        /// the functional active area polygon for the downcomer box
        /// </summary>
        public uopShape FunctionalActiveAreaPolygon()
        {
            uopLinePair boxlns = BoxLines;
            uopArc arc = new uopArc(BoundingRadius);
            if (arc.Radius <= 0) return null;
            uopLine l1 = boxlns.GetSide(uppSides.Left);
            uopLine l2 = boxlns.GetSide(uppSides.Right);
            uopVectors verts = new uopVectors();
            uopVectors ipts_L = l1.Intersections(arc, true, true).Sorted(dxxSortOrders.TopToBottom);
            uopVectors ipts_R = l2.Intersections(arc, true, true).Sorted(dxxSortOrders.BottomToTop);
            if (DesignFamily.IsStandardDesignFamily())
            {

                verts.AddRange(ipts_L);
               verts.AddRange(ipts_R);

            
            }
            else
            {

                uppIntersectionTypes top = IntersectionType(bTop: true);
                uppIntersectionTypes bot = IntersectionType(bTop: false);

                uopVectors p1 = new uopVectors();
                if(top == uppIntersectionTypes.ToRing)
                {
                    verts.Add(ipts_L.First());
                    if (bot == uppIntersectionTypes.ToRing)
                    {
                        verts.Add(ipts_L.Last());
                        verts.AddRange(ipts_R);
                    }
                    else 
                    { 
                        verts.Add(l1.EndPoints().GetVector(dxxPointFilters.AtMinY));
                        verts.Add(l2.EndPoints().GetVector(dxxPointFilters.AtMinY));
                        verts.Add(ipts_R.Last());
                    }
                }
                else
                {
                 
                    if (bot == uppIntersectionTypes.ToRing)
                    {
                        verts.Add(l1.EndPoints().GetVector(dxxPointFilters.AtMaxY));
                        verts.Add(ipts_L.Last());
                        verts.Add(ipts_R.First());
                        verts.Add(l2.EndPoints().GetVector(dxxPointFilters.AtMaxY));
                       }
                    else
                    {
                        verts.AddRange(l1.EndPoints().Sorted(dxxSortOrders.TopToBottom));
                        verts.AddRange(l2.EndPoints().Sorted(dxxSortOrders.BottomToTop));

                    }
                  
                }
            }
            return new uopShape(verts, $"ACTIVE AREA DC:{DowncomerIndex} BOX:{Index}") { Row=Row,Col = Col};
        }

        public uopLine LimitLine(bool bTop, double aOffset = 0) => new uopLine(LimitLn(bTop, aOffset));

        internal ULINE LimitLn(bool bTop, double aOffset = 0)
        {
            ULINE _rVal = bTop ? LimitLns.GetSideValue(uppSides.Top) : LimitLns.GetSideValue(uppSides.Bottom);
            if (aOffset != 0)
                _rVal.MoveOrtho(aOffset);
            return _rVal;
        }

        public uopLine EndLine(bool bTop, double aOffset = 0) => new uopLine(EndLn(bTop, aOffset));

        internal ULINE EndLn(bool bTop, double aOffset = 0)
        {
            ULINE _rVal = bTop ? new ULINE(X_Inside_Left, BoxLn(bLeft: true).MaxY, X_Inside_Right, BoxLn(bLeft: false).MaxY) : new ULINE(X_Inside_Left, BoxLn(bLeft: true).MinY, X_Inside_Right, BoxLn(bLeft: false).MinY);
            if (aOffset != 0)
                _rVal.MoveOrtho(aOffset);

            return _rVal;
        }

        public uopLine BoxLine(bool bLeft, double aOffset = 0) => new uopLine(BoxLn(bLeft, aOffset));

        internal ULINE BoxLn(bool bLeft, double aOffset = 0)
        {
            ULINE _rVal = bLeft ? BoxLns.GetSideValue(uppSides.Left) : BoxLns.GetSideValue(uppSides.Right);
            if (aOffset != 0)
                _rVal.MoveOrtho(aOffset);

            return _rVal;
        }

        public uopLine ShelfLine(bool bLeft, double aOffset = 0) => new uopLine(ShelfLn(bLeft, aOffset));
        internal ULINE ShelfLn(bool bLeft, double aOffset = 0)
        {
            ULINE _rVal = ShelfLns.GetSideValue(bLeft ? uppSides.Left : uppSides.Right);
            if (aOffset != 0)
                _rVal.MoveOrtho(aOffset);

            return _rVal;
        }
        public uopLine WeirLine(bool bLeft) => new uopLine(WeirLn(bLeft));

        internal ULINE WeirLn(bool bLeft) => bLeft ? WeirLns.GetSideValue(uppSides.Left) : WeirLns.GetSideValue(uppSides.Right);

        public uppIntersectionTypes IntersectionType(bool bTop) => bTop ? LimitLns.IntersectionType1 : LimitLns.IntersectionType2;

        /// <summary>
        /// the Y distance from the end of the box to the intersection of the limit Line with the box inside wall
        /// </summary>
        public double EndPlateInset(bool bTop)
        {
            ULINE limline = bTop ? new ULINE(LimitLns.GetSide(uppSides.Top)) : new ULINE(LimitLns.GetSide(uppSides.Bottom));
            ULINE leftbx = BoxLn(bLeft: true);
            return bTop ? leftbx.MaxY - limline.EndPoints.GetVector(dxxPointFilters.AtMinX).Y : limline.EndPoints.GetVector(dxxPointFilters.AtMinX).Y - leftbx.MinY;
        }



        public List<double> StartupOrdinates(bool bLeft)
        {
            List<double> ords = bLeft ? mzUtils.ListToNumericCollection(StartUpSitesL) : mzUtils.ListToNumericCollection(StartUpSitesR);
            uopLine weir = WeirLine(bLeft: bLeft);
            double max = weir != null ?  weir.MaxY : double.MinValue;
            double min = weir != null ?  weir.MinY : double.MaxValue;
            return ords.FindAll(x => x >= min && x <= max);
            
        }

        public new hdwHexBolt SmallBolt(string aDescription = "", string aCategory = "", int aQuantity = 0)
        {
            hdwHexBolt _rVal = new hdwHexBolt((Bolting == uppUnitFamilies.English) ? uppHardwareSizes.ThreeEights : uppHardwareSizes.M10, RangeHardwareMaterial)
            {
                IsVisible = IsVisible,
                Quantity = Quantity,
                DoubleNut = DoubleNuts,
                ObscuredLength = SheetMetalThickness
            };
            if (Bolting == uppUnitFamilies.English)
            {
                _rVal.Length = (!_rVal.DoubleNut) ? 1 : 1.25;
            }
            else
            {
                _rVal.Length = (!_rVal.DoubleNut) ? 25 / 25.4 : 30 / 25.4;
            }

            _rVal.DescriptiveName = aDescription;
            aCategory = string.IsNullOrWhiteSpace(aCategory) ? Category : aCategory.Trim();
            _rVal.Quantity = (aQuantity <= 0) ? Quantity : aQuantity;

            _rVal.SubPart(this, aCategory);
            return _rVal;

        }
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }


        public override string ToString() { return $"DOWNCOMER BOX({PartNumber})"; }

        /// <summary>
        /// returns the weir length of the box
        /// </summary>
        /// <remarks>if left or right is passed only the length on one side is returned otherwise the total is return</remarks>
        /// <param name="aSide">the side to retrieve</param>
        /// <param name="bTrayWide">if true, the return is multiplied but the boxes occurance factor</param>
        /// <returns></returns>
        public double WeirLength(uppSides aSide = uppSides.Undefined, bool bTrayWide = false)
        {
            double _rVal = aSide == uppSides.Left || aSide == uppSides.Right ? aSide == uppSides.Right ? WeirLn(bLeft: false).Length : WeirLn(bLeft: true).Length : WeirLn(bLeft: true).Length + WeirLn(bLeft: false).Length;

            return !bTrayWide ? _rVal : _rVal * OccuranceFactor;
        }

        /// <summary>
        /// the ideal spout area for the downcomer
        /// equal to the weir length of the downcomer divided by the total weir length of the tray times the spout area of the tray
        /// </summary>
        public double IdealSpoutArea(mdTrayAssembly aAssy, double aTotalWeirLength = 0, double aTrayIdealSpoutArea = 0, bool bTrayWide = false)
        {
            if (aTrayIdealSpoutArea <= 0)
            {
                aAssy ??= GetMDTrayAssembly();
                if (aAssy == null) return 0;
                aTrayIdealSpoutArea = aAssy.TheoreticalSpoutArea;

            }
            if (aTrayIdealSpoutArea <= 0) return 0;
            if (aTotalWeirLength <= 0)
                aTotalWeirLength = aAssy.TotalWeirLength;

            double fract = WeirFraction(aAssy, aTotalWeirLength, bTrayWide);
            return fract * aTrayIdealSpoutArea;


        }

        /// <summary>
        /// the fraction of the total tray weir that this downcomer represents
        /// </summary>
        /// <param name="aAssy">the parent tray assembly</param>
        /// <param name="aTotalWeirLength">if the know total is passed it it used otherwise it is retrievend from the tray</param>
        /// <param name="bTrayWide">if true the downcomers occurance factor is applies</param>
        /// <returns></returns>
        public double WeirFraction(mdTrayAssembly aAssy, double aTotalWeirLength = 0, bool bTrayWide = false)
        {
            if (aTotalWeirLength <= 0)
            {
                aAssy ??= GetMDTrayAssembly();
                if (aAssy == null) return 0;
                aTotalWeirLength = aAssy.TotalWeirLength;
            }
            double mytotal = WeirLength(uppSides.Undefined, bTrayWide);
            return (aTotalWeirLength != 0) ? mytotal / aTotalWeirLength : 0;
        }


        public double TotalSpoutArea(mdTrayAssembly aAssy, bool bTrayWide = false) => ComputeSpoutArea(aAssy, bTrayWide);

        /// <summary>
        /// recomputes the current total spout area value
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double ComputeSpoutArea(mdTrayAssembly aAssy, bool bTrayWide = false)
        {

            aAssy ??= GetMDTrayAssembly();
            colMDSpoutGroups Groups = SpoutGroups(aAssy);
            if (Groups == null) return 0;
            uppMDDesigns family = aAssy.DesignFamily;
            double _rVal = 0;
            for (int i = 1; i <= Groups.Count; i++)
            {
                mdSpoutGroup aGroup = Groups.Item(i);
                double tot = aGroup.ActualArea;
                
                int cnt = family .IsStandardDesignFamily() ? !bTrayWide ?aGroup.Instances.FindAll((x) => x.DX == 0).Count + 1 : aGroup.Instances.Count + 1 : 1;
                
                _rVal += tot * cnt;
            }
            return _rVal;
        }



        /// <summary>
        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdDowncomerBox Clone() => new mdDowncomerBox(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        ///returns a dxePolylines that represent the walls of the box from above
        /// </summary>
        /// <param name="aLayer"></param>
        /// <param name="aColor"></param>
        /// <param name="aLinetype"></param>
        /// <param name="aTopYLimit"></param>
        /// <param name="aBotYLimit"></param>
        /// <param name="bQuickLine"></param>
        /// <returns></returns>
        public virtual colDXFEntities Edges(string aLayer, dxxColors aColor, string aLinetype, double? aTopYLimit = null, double? aBotYLimit = null, bool bQuickLine = false,  colDXFEntities dXFEntities = null)
        {
            colDXFEntities _rVal = dXFEntities ?? new colDXFEntities();

            try
            {
                if (!BoxLns.IsDefined) return _rVal;

                ULINE left = BoxLns.GetSide(uppSides.Left).Value;
                ULINE right = BoxLns.GetSide(uppSides.Right).Value;
                double thk = Thickness;

                // make sure the start pts are at the bottom
                if (left.sp.Y > left.ep.Y) left = left.Inverse();
                if (right.sp.Y > right.ep.Y) right = right.Inverse();

                bool isBeamDesignFamily = DesignFamily.IsBeamDesignFamily();

                if (aTopYLimit.HasValue)
                {
                    if (left.ep.Y > aTopYLimit.Value)
                        left.ep.Y = aTopYLimit.Value;
                    if (right.ep.Y > aTopYLimit.Value)
                        right.ep.Y = aTopYLimit.Value;
                }

                if (aBotYLimit.HasValue)
                {
                    if (left.sp.Y < aBotYLimit.Value)
                        left.sp.Y = aBotYLimit.Value;
                    if (right.sp.Y < aBotYLimit.Value)
                        right.sp.Y = aBotYLimit.Value;


                }


                if (!bQuickLine)
                {
                    uopLinePair leftedge = new uopLinePair(left, left.Moved(thk));
                    uopLinePair rightedge = new uopLinePair(right, right.Moved(-thk));
                    _rVal.AddPolyline(leftedge.EndPoints(true), true, new dxfDisplaySettings(aLayer, aColor, aLinetype));
                    _rVal.AddPolyline(rightedge.EndPoints(true), true, new dxfDisplaySettings(aLayer, aColor, aLinetype));

                }
                else
                {
                    uopLinePair edges = new uopLinePair(left, right);
                    uopVectors verts = new uopVectors() 
                    {
                        left.ep.Moved(thk),
                        left.ep,
                        left.sp,
                        left.sp.Moved(thk),
                        right.sp.Moved(-thk),
                        right.sp,
                        right.ep,
                        right.ep.Moved(-thk)
                    };
                    _rVal.AddPolyline(verts, true, new dxfDisplaySettings(aLayer, aColor, aLinetype));
                }


            }
            catch (OverflowException ex)
            {
                throw ex;
            }
            return _rVal;
        }

        /// <summary>
        /// a line at the top of the downcomer aligned with the face of the end plate and offet by the passed clearance.
        /// </summary>
        /// <param name="aSpoutGroup"></param>
        /// <param name="aConstraints"></param>
        /// <param name="bIncludeSpoutRadius"></param>
        /// <param name="aSpoutRadius"></param>
        /// <param name="aEPClearance"></param>
        /// <returns></returns>
        internal ULINE SpoutGroupLimLine(mdSpoutGroup aSpoutGroup, mdConstraint aConstraints, bool bIncludeSpoutRadius, double aSpoutRadius = 0, double aEPClearance = 0)
        {
            ULINE _rVal;
            double offset = 0;
            bool top = true;
            if (aSpoutGroup != null)
            {
                top = aSpoutGroup.Direction == dxxOrthoDirections.Up;
                aConstraints ??= aSpoutGroup.Constraints(null);

                double arad = Math.Abs(aSpoutRadius);
                if (arad <= 0 && aConstraints != null)
                    arad = aConstraints.SpoutDiameter / 2;
                if (arad <= 0) arad = 0.375;

                double aclrc = !bIncludeSpoutRadius ? 0 :  arad;
                double cnclrc = aEPClearance;
                if (cnclrc <= 0)
                {
                    if (aSpoutGroup.LimitedBounds) cnclrc = aConstraints.EndPlateClearance;
                    if (cnclrc <= 0) cnclrc = 0.25;
                }
                offset = (aclrc + cnclrc);
            }

            _rVal = LimitLn(bTop: top);
            if(offset != 0)
            {
                
                if (top)
                {
                    if (_rVal.sp.X < _rVal.ep.X) offset *= -1;

                }
                else
                {
                    if (_rVal.sp.X>  _rVal.ep.X) offset *= -1;

                }
                _rVal.MoveOrtho(offset);
            }
            
            return _rVal;
        }

        /// <summary>
        /// the lines that limits the startup spouts from getting put too close to the endplate area
        /// </summary>
        public uopLinePair StartUpLimitLines { get => new uopLinePair(StartUpLimitLns); }

        /// <summary>
        /// the lines that limits the startup spouts from getting put too close to the endplate area
        /// </summary>
        internal ULINEPAIR StartUpLimitLns
        {
            get
            {
                if (!LimitLns.IsDefined) return ULINEPAIR.Null;

                ULINEPAIR _rVal = ULINEPAIR.Null;
                ULINE top = new ULINE(LimitLns.GetSide(uppSides.Top));
                ULINE bot = new ULINE(LimitLns.GetSide(uppSides.Bottom));
                top.MoveOrtho((top.sp.X < top.ep.X ? 1 : -1) * (0.25 + 0.375));
                top.Move(0, -2.625);
                _rVal.Line1 = top;
                bot.MoveOrtho((bot.sp.X < bot.ep.X ? -1 : 1) * (0.25 + 0.375));
                bot.Move(0, 2.625);
                _rVal.Line2 = bot;

                _rVal.ExtendTo(BoxLns, true);

                return _rVal;

                //=> GetLimLines(aClearance: 0.25 + 0.375).First().Line1.Value.Moved(0, -2.625);

            }
        } 



        public uopHoleArray GenHoles(mdTrayAssembly aAssy, string aTag = "", string aFlag = "", bool? bSuppressSpouts = null, string aSide = "", string aTags = "") => new uopHoleArray(GenHolesV(aAssy, aTag, aFlag, bSuppressSpouts, aSide, aTags));

        /// <summary>
        /// executed internally to create the holes collection for the box
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="aDC"></param>
        /// <param name="bSuppressSpouts"></param>
        /// <param name="aSide"></param>
        /// <param name="aTags"></param>
        /// <returns></returns>
        internal UHOLEARRAY GenHolesV(mdTrayAssembly aAssy, string aTag = "", string aFlag = "", bool? bSuppressSpouts = null, string aSide = "", string aTags = "")
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;
            aAssy ??= GetMDTrayAssembly(aAssy);

            aTag ??= string.Empty;
            aTags ??= string.Empty;
            aTag = aTag.ToUpper().Trim();
            aTags = aTags.ToUpper().Trim();
            aFlag = !string.IsNullOrEmpty(aFlag) ? aFlag.ToUpper().Trim() : string.Empty;
            aSide = !string.IsNullOrEmpty(aSide) ? aSide.ToUpper().Trim() : string.Empty;
            mzUtils.ListAdd(ref aTags, aTag);

            double cX = X;
            double thk = Thickness;

            double wd = Width;
            double dkthk = DeckThickness;

            double ctrl = cX - (0.5 * wd) + (0.5 * thk);
            double ctrr = cX + (0.5 * wd) - (0.5 * thk);
            double fcholelev = dkthk + 0.625;

            mdCrossBrace aCB = aAssy?.CrossBrace;
            double bc = WeirHeight - Height + (0.5 * thk);
            UHOLES aHls;
            UHOLES cHls;
            UHOLES dHls;
            UHOLE aHl;

            UVECTOR u1;
            UVECTORS fCPts;
            if (aTag !=  string.Empty) mzUtils.ListAdd(ref aTags, aTag);
            if (!bSuppressSpouts.HasValue && aTags !=  string.Empty & !mzUtils.ListContains("SPOUT", aTags)) bSuppressSpouts = true;
            if (!bSuppressSpouts.HasValue) bSuppressSpouts = false;

            //start with the startup spouts
            if (mzUtils.ListContains("STARTUP", aTags, bReturnTrueForNullList: true))
            {
                mdStartupSpouts sus = StartupSpouts(aSide);
                aHls = sus.SlotsV;
                aHls.Member.DownSet = WeirHeight - aHls.Member.Elevation;
                _rVal.Add(aHls, "STARTUP");

            }


            //add the spouts
            if (!bSuppressSpouts.Value)
                _rVal.Append(Spouts(aAssy, true));

            //holes for finger clips
            if (mzUtils.ListContains("FINGER CLIP", aTags, bReturnTrueForNullList: true))
            {
                fCPts = FingerClipPts(aAssy, null, bUnsuppressedOnly: WeldedStiffeners);

                aHl = FingerClipHole;
                aHl.DownSet = WeirHeight - aHl.Elevation;
                aHls = new UHOLES(false, "FINGER CLIP", aHl.Radius, 0, thk, 0, aHl.Elevation) { Member = aHl};

                for (int i = 1; i <= fCPts.Count; i++)
                {
                    aHl.Center = fCPts.Item(i);
                    if (aHl.X < cX)
                    {
                        if (aSide == "LEFT" || aSide ==  string.Empty)
                        {
                            aHl.X = ctrl;
                            aHl.Tag = "LEFT";
                            aHls.Centers.Add(aHl.Center);
                        }
                    }
                    else
                    {
                        if (aSide == "RIGHT" || aSide ==  string.Empty)
                        {
                            aHl.X = ctrr;
                            aHl.Tag = "RIGHT";
                            aHls.Centers.Add(aHl.Center);
                        }
                    }

                }


                if (aHls.Centers.Count > 0)
                {
                    _rVal.Add(aHls);
                }

            }

            //holes for end plates
            if (mzUtils.ListContains("ENDPLATE", aTags, bReturnTrueForNullList: true) && BoltOnEndplates)
            {
                aHls = new UHOLES("ENDPLATE");

                //double y1 = Length / 2 - 0.375;
                double y1 = Y - 0.375;
                ULINE aLn = new ULINE(new UVECTOR(y1, -1), new UVECTOR(y1, WeirHeight - Height + 0.25));

                aHl = new UHOLE(aDiameter: mdGlobals.gsBigHole, aX: 0, aY: 0, aTag: "ENDPLATE", aDepth: thk, aZDirection: "1,0,0");
                aHls.Member = aHl;
                cHls = uopUtils.LayoutHolesOnLine2(aLn, aHl, aTargetSpace: 8,aEndBuffer:  0.5, aTag:"ENDPLATE");

                double z1 = WeirHeight;
                z1 = (z1 - 1 >= mdGlobals.gsBigHole + 0.375) && !FoldOverWeirs ? z1 - ((z1 - 1) / 2) : dkthk + 0.625;
                if (aSide == "RIGHT" || aSide ==  string.Empty) aHls.Centers.Add(ctrr, y1, aTag: "RIGHT", aElevation: z1);
                if (aSide == "LEFT" || aSide ==  string.Empty) aHls.Centers.Add(ctrl, y1, aTag: "LEFT", aElevation: z1);


                for (int i = 1; i <= cHls.Centers.Count; i++)
                {
                    u1 = cHls.Centers.Item(i);
                    if (aSide == "RIGHT" || aSide ==  string.Empty) aHls.Centers.Add(ctrr, y1, aTag: "RIGHT", aElevation: u1.Y);
                    if (aSide == "LEFT" || aSide ==  string.Empty) aHls.Centers.Add(ctrl, y1, aTag: "LEFT", aElevation: u1.Y);
                }

                if (aHls.Count > 0)
                    _rVal.Add(aHls, "ENDPLATE");

            }

            //holes for end angles
            if (mzUtils.ListContains("END ANGLE", aTags, bReturnTrueForNullList: true))
            {
                aHls = new UHOLES("END ANGLE");
                aHl = new UHOLE(EndAngleSlot);

                var leftWeirLine = WeirLn(bLeft: true);
                var rightWeirLine = WeirLn(bLeft: false);

                aHl.Elevation = fcholelev;

                // Upper left hole
                aHl.X = ctrl;
                aHl.DownSet = WeirHeight - aHl.Elevation;
                aHl.Y = leftWeirLine.MaxY - 2;
                aHl.Inset = BoxLns.GetSideValue(uppSides.Left).MaxY - aHl.Y;
                aHls.Member = aHl;

                if (aSide == "LEFT" || aSide ==  string.Empty)
                {
                    if (aFlag == "UL" || aFlag ==  string.Empty) aHls.Centers.Add(new UVECTOR(aHl), "UL");
                }

                // lower left hole
                aHl.Y = leftWeirLine.MinY + 2;

                if (aSide == "LEFT" || aSide ==  string.Empty)
                {
                    if (aFlag == "LL" || aFlag ==  string.Empty) aHls.Centers.Add(new UVECTOR(aHl), "LL");
                }

                // Upper right hole
                aHl.X = ctrr;

                aHl.Y = rightWeirLine.MaxY - 2;
                if (aSide == "RIGHT" || aSide ==  string.Empty)
                {
                    if (aFlag == "UR" || aFlag ==  string.Empty) aHls.Centers.Add(new UVECTOR(aHl), "UR");
                }

                // Lower right hole
                aHl.Y = rightWeirLine.MinY + 2;
                if (aSide == "RIGHT" || aSide ==  string.Empty)
                {
                    if (aFlag == "LR" || aFlag ==  string.Empty) aHls.Centers.Add(new UVECTOR(aHl), "LR");
                }
                if (aHls.Centers.Count > 0) _rVal.Add(aHls, "END ANGLE");
            }

            //appan mount holes and slots
            if (mzUtils.ListContains("APPAN_HOLE", aTags, bReturnTrueForNullList: true) || mzUtils.ListContains("APPAN_SLOT", aTags, bReturnTrueForNullList: true))
            {

                if (HasAntiPenetrationPans)
                {
                    bool bFlg1 = mzUtils.ListContains("APPAN_HOLE", aTags, bReturnTrueForNullList: true);
                    bool bFlg2 = mzUtils.ListContains("APPAN_SLOT", aTags, bReturnTrueForNullList: true);

                    if (bFlg1 || bFlg2)
                    {


                        //if(uopUtils.RunningInIDE){
                        //     aAssy.Invalidate(uppPartTypes.APPan);
                        // }
                        List<mdAPPan> aPans = mdPartGenerator.APPans_DC(aAssy, DowncomerIndex);

                        //aPans = aPans.FindAll(x => x.IsAssociatedToParent(PartNumber));
                        cHls = new UHOLES("APPAN_HOLE");
                        dHls = new UHOLES("APPAN_SLOT");
                        bool isslot;
                        bool keep = false;
                        UHOLES apholes;

                        for (int i = 1; i <= aPans.Count; i++)
                        {
                            mdAPPan aPan = aPans[i - 1];


                            uopVectors appts = aPan.Instances.MemberPoints();
                            foreach (uopVector appt in appts)
                            {

                                double rot = appt.Y > Y ? 0 : 180;
                                UHOLEARRAY bHls = aPan.MountingHolesV(bc, thk, rot, appt);

                                apholes = bHls.Item(1);
                                isslot = apholes.Member.HoleType == uppHoleTypes.Slot;


                                if (bHls.Count >= 1)
                                {

                                    if ((!isslot && bFlg1) || (isslot && bFlg2))
                                    {
                                        for (int j = 1; j <= apholes.Count; j++)
                                        {
                                            aHl = apholes.Item(j);
                                            if (isslot)
                                            {
                                                if (dHls.Count <= 0) dHls.Member = aHl;
                                                dHls.Centers.Add(aHl.Center.Clone(), aTag: aHl.Tag);
                                            }
                                            else
                                            {
                                                keep = apholes.Count == 1;


                                                if (!keep)
                                                {
                                                    ULINE eline = EndLn(bTop: aHl.Y > Y);
                                                    if (aHl.Y > Y)
                                                        keep = eline.Slope == 0 || (eline.Slope < 0 ? aHl.X < X : aHl.X > X);
                                                    else
                                                        keep = eline.Slope == 0 || (eline.Slope > 0 ? aHl.X < X : aHl.X > X);

                                                }
                                                if (keep)
                                                {
                                                    if (cHls.Count <= 0) cHls.Member = aHl;
                                                    cHls.Centers.Add(aHl.Center.Clone(), aTag: aHl.Tag);
                                                }

                                            }

                                        }
                                    }


                                }
                                if (bHls.Count >= 2)
                                {
                                    apholes = bHls.Item(2);
                                    isslot = apholes.Member.HoleType == uppHoleTypes.Slot;
                                    if ((!isslot && bFlg1) || (isslot && bFlg2))
                                    {
                                        for (int j = 1; j <= apholes.Count; j++)
                                        {
                                            aHl = apholes.Item(j);
                                            if (isslot)
                                            {
                                                if (dHls.Count <= 0) dHls.Member = aHl;
                                                dHls.Centers.Add(aHl.Center.Clone(), aTag: aHl.Tag);
                                            }
                                            else
                                            {
                                                if (!keep)
                                                {
                                                    ULINE eline = EndLn(bTop: aHl.Y > Y);
                                                    if (aHl.Y > Y)
                                                        keep = eline.Slope == 0 || (eline.Slope < 0 ? aHl.X < X : aHl.X > X);
                                                    else
                                                        keep = eline.Slope == 0 || (eline.Slope > 0 ? aHl.X < X : aHl.X > X);

                                                }
                                                if (keep)
                                                {
                                                    if (cHls.Count <= 0) cHls.Member = aHl;
                                                    cHls.Centers.Add(aHl.Center.Clone(), aTag: aHl.Tag);
                                                }
                                            }

                                        }
                                    }


                                }
                            }



                        }

                        if (cHls.Count > 0) _rVal.Add(cHls, "APPAN_HOLE");
                        if (dHls.Count > 0) _rVal.Add(dHls, "APPAN_SLOT");


                    }
                }
            }


            if (mzUtils.ListContains("CROSSBRACE", aTags, bReturnTrueForNullList: true))
            {
                if (aCB != null)
                {
                    aHls = UHOLES.Null;
                    uopHoles sHoles = aCB.GenHoles(aAssy).Item(1);
                    uopHoles xHoles = sHoles.GetAtCoordinate(X, "", 2, bJustOne: true);

                    if (xHoles.Count > 0)
                    {
                        aHl = xHoles.ItemV(1);

                        aHl.Tag = "CROSSBRACE";
                        aHl.Radius = mdGlobals.gsBigHole / 2;
                        aHl.Elevation = bc;
                        aHls.Member = aHl;

                        aHls.Centers.Add(aHl.Center);

                    }


                    sHoles.Rotate(180, null);

                    xHoles = sHoles.GetAtCoordinate(X, "", 2, bJustOne: true);
                    if (xHoles.Count > 0)
                    {
                        aHl = xHoles.ItemV(1);
                        if (aHls.Centers.Count == 0)
                        {
                            aHl.Tag = "CROSSBRACE";
                            aHl.Radius = mdGlobals.gsBigHole / 2;
                            aHl.Elevation = bc;
                            aHls.Member = aHl;
                        }

                        aHls.Centers.Add(aHl.Center);
                    }

                    if (aHls.Centers.Count > 0)
                    {
                        _rVal.Add(aHls, "CROSSBRACE");
                    }
                }
            }
            return _rVal;
        }


        /// <summary>
        /// Method is used to draw the plan view of the box.
        /// </summary>
        /// <param name="aAssy">the parent tray assembly for this part</param>
        /// <param name="aDC">the parent downcomer for this part</param>
        /// <param name="rBottomNut"></param>
        /// <param name="bSuppressSpouts">flag to suppress then spout holes</param>
        /// <param name="aCenter"></param>
        /// <param name="aAngle"></param>
        /// <param name="aLayerName"></param>
        /// <returns>returns a dxePolygon that is used to draw the flat plate layout view of the box</returns>
        public dxePolygon View_Layout(mdTrayAssembly aAssy, out hdwHexNut rBottomNut, bool bSuppressSpouts = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        => mdPolygons.DCBox_View_Layout(this, aAssy, out rBottomNut, bSuppressSpouts, aCenter, aRotation, aLayerName);

        /// <summary>
        /// Method is used to draw the plan view of the box.
        /// </summary>
        /// <param name="aAssy">the parent tray assembly for this part</param>
        /// <param name="aDC">the parent downcomer for this part</param>
        /// <param name="bSuppressSpouts">flag to suppress then spout holes</param>
        /// <param name="aCenter"></param>
        /// <param name="aAngle"></param>
        /// <param name="aLayerName"></param>
        /// <returns>returns a dxePolygon that is used to draw the flat plate layout view of the box</returns>
        public dxePolygon View_Layout(mdTrayAssembly aAssy, bool bSuppressSpouts = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        => mdPolygons.DCBox_View_Layout(this, aAssy, out hdwHexNut _, bSuppressSpouts, aCenter, aRotation, aLayerName);

        /// <summary>
        /// the collection of spout groups defined for the downcomer
        /// </summary>
        /// <param name="aAssy">a downcomer retrieves its spout group from its parent tray assemblies collection of defined spout groups.</param>
        /// <returns></returns>
        public colMDSpoutGroups SpoutGroups(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly();

            return (aAssy != null) ? aAssy.SpoutGroups.GetByDowncomerIndex(DowncomerIndex, bNonZero: true, bParentsOnly: true, aBoxIndex:Index) : new colMDSpoutGroups();

        }



        public mdShelfAngle ShelfAngle(bool bLeft = true) => new mdShelfAngle(this, bLeft);


        public List<mdShelfAngle> ShelfAngles() => new List<mdShelfAngle>() { ShelfAngle(true), ShelfAngle(false) };

        public override void UpdatePartProperties()
        {
            DescriptiveName = $"Box (DC {DowncomerIndex})";
        }

        public override void UpdatePartWeight()
        {
            mdTrayAssembly aAssy = GetMDTrayAssembly();
            mdDowncomer aDC = GetMDDowncomer(aAssy, null, DowncomerIndex);
            base.Weight = Weight(aDC, aAssy);
        }

        /// <summary>
        /// Method to get vertices of a polyline that is the outside perimeter of the box
        /// </summary>
        /// <param name="bRotated">flag to return the shape roted 90 degrees</param>
        /// <param name="bSupressFoldovers"></param>
        /// <param name="bTrimOrder">to control showing the thickness detail</param>
        /// <returns>returns the vertices of a polyline that is the outside perimeter of the box</returns>
        public colDXFVectors Vertices(bool bRotated = false, bool bSupressFoldovers = false, bool bTrimOrder = false)
        {
            colDXFVectors _rVal = new colDXFVectors();

            ULINE leftBx = BoxLns.GetSideValue(uppSides.Left);
            ULINE rightBx = BoxLns.GetSideValue(uppSides.Right);
            double fldwd = (!bSupressFoldovers && FoldOverWeirs) ? mdDowncomer.FoldOverMaxWidth - Thickness : 0;
            if (fldwd != 0)
            {
                leftBx.Move(-fldwd);
                rightBx.Move(fldwd);
            }
            dxfPlane mypln = Plane;


            dxfVector ur = rightBx.sp.ToDXFVector().WithRespectToPlane(mypln);
            dxfVector lr = rightBx.ep.ToDXFVector().WithRespectToPlane(mypln);
            dxfVector ul = leftBx.sp.ToDXFVector().WithRespectToPlane(mypln);
            dxfVector ll = leftBx.ep.ToDXFVector().WithRespectToPlane(mypln);

            //DONT CHNAGE VERT ORDER !!
            if (bTrimOrder)
            {
                _rVal.Add(mypln, ll.X, ll.Y);
                _rVal.Add(mypln, ul.X, ul.Y);
                _rVal.Add(mypln, ur.X, ur.Y);
                _rVal.Add(mypln, lr.X, lr.Y);
            }
            else
            {
                dxfVector notch = new dxfVector(Thickness + fldwd, 0);
                dxfVector urn = ur - notch; // upper right notch
                dxfVector lrn = lr - notch; // lower right notch
                dxfVector uln = ul + notch; // upper left notch
                dxfVector lln = ll + notch; // lower left notch

                _rVal.Add(mypln, ur.X, ur.Y);
                _rVal.Add(mypln, lr.X, lr.Y);
                _rVal.Add(mypln, lrn.X, lrn.Y); // lower right notch
                _rVal.Add(mypln, lln.X, lln.Y); // lower left notch
                _rVal.Add(mypln, ll.X, ll.Y);
                _rVal.Add(mypln, ul.X, ul.Y);
                _rVal.Add(mypln, uln.X, uln.Y); // upper left notch
                _rVal.Add(mypln, urn.X, urn.Y); // upper right notch
            }

            if (bRotated)
            {
                if (DesignFamily.IsBeamDesignFamily())
                {
                    double length = RingID + 100;
                    double coordinate = 0.5 * Math.Sqrt(2) * length;
                    dxeLine fortyFiveDegDiameter = new dxeLine(new dxfVector(-coordinate, -coordinate), new dxfVector(coordinate, coordinate));

                    _rVal.Mirror(fortyFiveDegDiameter);
                }
                else
                {
                    _rVal.RotateAbout(dxfVector.Zero, -90);
                }
            }

            return _rVal;
        }

        /// <summary>
        /// Method to get vertices of a polyline that is the outside perimeter of the box
        /// </summary>
        /// <param name="aYTrimOrd">a Y ordinate to trim the vertices to</param>
        /// <param name="bRotated">flag to return the shape rotated 90 degrees</param>
        /// <param name="aDowncomer">the parent downcomer of the box</param>
        /// <param name="bSupressFoldovers"></param>
        /// <returns>returns the vertices of a polyline that is the outside perimeter of the box trimed to the passed Y ordinate</returns>
        public colDXFVectors TrimmedVertices(double aYTrimOrd, bool bRotated = false, mdDowncomer aDowncomer = null, bool bSupressFoldovers = false)
        {
            colDXFVectors _rVal = new colDXFVectors();

            mdDowncomer aDC = aDowncomer;
            if (!bSupressFoldovers && aDC == null) aDC = Downcomer;
            if (!bSupressFoldovers && aDC == null) bSupressFoldovers = true;
            if (!bSupressFoldovers && !aDC.FoldOverWeirs) bSupressFoldovers = true;
            double thk = Thickness;
            double wd = Width;
            double leng = Length;
            double lleng = LeftLength;
            double cX = X;

            bool fldov = !bSupressFoldovers && FoldOverWeirs;
            dxfPlane ocs = dxfPlane.World;
            if (bRotated) ocs.Rotate(-90);
            double fldwd = (!bSupressFoldovers) ? mdDowncomer.FoldOverMaxWidth - thk : 0;

            //DONT CHNAGE VERT ORDER !!

            aYTrimOrd = mzUtils.LimitedValue(aYTrimOrd, -lleng / 2, lleng / 2);

            if (aDowncomer.GetMDTrayAssembly()?.DesignFamily.IsBeamDesignFamily() ?? false)
            {
                double halfLongLength = lleng / 2;
                double halfShortLength = leng / 2;

                double halfLeftLength = halfLongLength;
                double halfRightLength = halfShortLength;
                if (!IsLongerEdgeOnLeftSide(new ULINE(LimitLns.GetSide(uppSides.Top))))
                {
                    halfLeftLength = halfShortLength;
                    halfRightLength = halfLongLength;
                }

                _rVal.Add(ocs.Vector(cX + (wd / 2 + fldwd), -halfRightLength));
                _rVal.Add(ocs.Vector(cX + (wd / 2 + fldwd), halfRightLength));
                _rVal.Add(ocs.Vector(cX - (wd / 2 + fldwd), halfLeftLength));
                _rVal.Add(ocs.Vector(cX - (wd / 2 + fldwd), -halfLeftLength));
            }
            else
            {
                _rVal.Add(ocs.Vector(cX + (wd / 2 + fldwd), aYTrimOrd));
                _rVal.Add(ocs.Vector(cX + (wd / 2 + fldwd), leng / 2));
                _rVal.Add(ocs.Vector(cX - (wd / 2 + fldwd), lleng / 2));
                _rVal.Add(ocs.Vector(cX - (wd / 2 + fldwd), aYTrimOrd));
            }

            return _rVal;
        }

        private bool IsLongerEdgeOnLeftSide(ULINE topLimitLine)
        {
            return topLimitLine.Slope <= 0;
        }


        /// <summary>
        /// Method to get dxePolygon that is used to draw the elevation view of the box
        /// </summary>
        /// <param name="aDC">the parent downcomer of the part</param>
        /// <param name="aAssy">the parent tray of the part</param>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aSuppressHoles"></param>
        /// <param name="aIncludeShelves"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="rBottomNut"></param>
        /// <returns>returns a dxePolygon that is used to draw the elevation view of the box</returns>
        public dxePolygon View_Elevation(mdTrayAssembly aAssy, bool bSuppressFillets, bool aSuppressHoles, bool aIncludeShelves, dxfVector aCenter, double aRotation, string aLayerName)
       => mdPolygons.DCBox_View_Elevation(this, aAssy, bSuppressFillets, aSuppressHoles, aIncludeShelves, aCenter, aRotation, aLayerName);


        /// <summary>
        /// Method to get dxePolygon that is used to draw the plan view of the box
        /// </summary>
        /// <param name="aAssy">the parent tray assembly</param>
        /// <param name="bOutLineOnly">flag to just return the outline of the box</param>
        /// <param name="bSuppressHoles">flag to return the holes with the polygon</param>
        /// <param name="bIncludeSpouts"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="aCenterLineLength"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aScale"></param>
        /// <param name="bIncludeEndPlates"> </param> 
        /// <param name="bIncludeEndSupports"></param>
        /// <param name="bIncludeShelfAngles"></param>
        /// <returns>returns a dxePolygon that is used to draw the layout view of the box</returns>
        public dxePolygon View_Plan(mdTrayAssembly aAssy, bool bOutLineOnly = false, bool bSuppressHoles = false, bool bIncludeSpouts = false, bool bShowObscured = false, double aCenterLineLength = 0, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", double aScale = 1, bool bIncludeEndPlates = false, bool bIncludeEndSupports = false, bool bIncludeShelfAngles = false)
    => mdPolygons.DCBox_View_Plan(this, aAssy, bOutLineOnly, bSuppressHoles, bIncludeSpouts, bShowObscured, aCenterLineLength, aCenter, aRotation, aLayerName, aScale, bIncludeEndPlates, bIncludeEndSupports, bIncludeShelfAngles);


        /// <summary>
        /// Method to get dxePolygon that is used to draw the profile view of the box
        /// </summary>
        /// <param name="aAssy">the parent tray of the part</param>
        /// <param name="aDC">the parent downcomer of the part</param>
        /// <param name="aShowObscured">flag to add hidden lines where the profile is obscured</param>
        /// <param name="aShowLongSide"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="rBottomNut"></param>
        /// <returns>returns a dxePolygon that is used to draw the profile view of the box</returns>
        public dxePolygon View_Profile(mdTrayAssembly aAssy, mdDowncomer aDC, bool aShowObscured, bool aShowLeftSide, dxfVector aCenter, double aRotation, string aLayerName, hdwHexNut rBottomNut)
        => mdPolygons.DCBox_View_Profile(this, aAssy, aDC, aShowObscured, aShowLeftSide, aCenter, aRotation, aLayerName, rBottomNut);
       


        /// <summary>
        /// Method to get weight of the downcomer box in english pounds
        /// </summary>
        /// <remarks> includes the the end plates & supports, shelf angles, stiffeners and the supplemental deflector </remarks>
        /// <param name="aDC"></param>
        /// <param name="aAssy"></param>
        /// <returns>returns the weight of the downcomer box in english pounds</returns>
        public new double Weight(mdDowncomer aDC, mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            aDC ??= MDDowncomer;

            if (aDC == null) return 0;
            double thk = Thickness;
            double ht = InsideHeight;
            double sArea = 0;
            double l1 = LeftLength;
            double l2 = Length;

            //bottom area


            sArea = l2 * Width;
            if (l1 > l2) sArea += (l1 - l2) * Width;


            //add the side areas
            sArea += (ht * l1) + (ht * l2);

            //remove the holes
            sArea -= GenHolesV(aAssy).TotalArea();

            //multiply by the thickness and density
            double _rVal = sArea * base.SheetMetalWeightMultiplier;

            _rVal += EndPlate(bTop: true).Weight() + EndPlate(bTop: false).Weight();
            _rVal += EndSupport(bTop: true).Weight() + EndSupport(bTop: false).Weight();
            _rVal += ShelfAngle(bLeft: true).Weight + ShelfAngle(bLeft: false).Weight;
            if (HasSupplementalDeflector)
            {
                _rVal += SupplementalDeflector.Weight;
            }
            List<mdStiffener> stiffeners = Stiffeners();
            if (stiffeners.Count > 0) 
            {
                _rVal += stiffeners.Count * stiffeners[0].Weight(this);
            }

            return _rVal;


        }

        /// <summary>
        /// the collection of stiffeners defined for the downcomer box
        /// </summary>
        /// <param name="bApplyInstances">if true, a single part is returned with it's instances populated</param>
        /// <returns></returns>
        public List<mdStiffener> Stiffeners(bool bApplyInstances = false)
        {

            List<mdStiffener> _rVal = new List<mdStiffener>();
            if ( IsGlobal || ProjectType == uppProjectTypes.MDSpout) return _rVal;

            List<double> yvals = StiffenerYs();
            if (yvals.Count <= 0) return _rVal;

            if (!bApplyInstances)
            {
                foreach (var y in yvals)
                    _rVal.Add(new mdStiffener(this, y));
            }
            else
            {

                mdStiffener stiff = null;
                TINSTANCES insts = new TINSTANCES("");
                for(int i = 1; i <= yvals.Count; i++)
                {
                    double y = yvals[i - 1];
                    if (i == 1)
                    {
                        stiff = new mdStiffener(this, y);
                        _rVal.Add(stiff);
                        insts = stiff.Instances_Get();
                    }
                    else { insts.Add(0, y - stiff.Y); }
                    
                }
                stiff.Instances_Set(insts);

            }
            
            return _rVal;
        }

        public mdStartupSpouts StartupSpouts(string aSide = "")
        {

            mdStartupSpouts _rVal = new mdStartupSpouts();
            if (StartupDiameter <= 0 || StartupLength <= 0) return _rVal;
            _rVal.Height = StartupDiameter;
            _rVal.Length = StartupLength;
            double thk = Thickness;
            aSide ??= string.Empty;
            aSide = aSide.ToUpper().Trim();
            mdStartupSpout aSU = new mdStartupSpout(StartupDiameter, StartupLength) { Z = DeckThickness + StartupDiameter / 2, Depth = thk };
            List<double> yVals;
            double xleft = X - Width / 2 + thk / 2;
            double xright = X + Width / 2 - thk / 2;

            if (aSide == string.Empty || aSide == "LEFT")
            {
                yVals = StartupOrdinates(bLeft: true);
                foreach (double item in yVals)
                    _rVal.Add(new mdStartupSpout(aSU) { Y = item, X = xleft });
            }
            if (aSide == string.Empty || aSide == "RIGHT")
            {
                yVals = StartupOrdinates(bLeft: false);
                foreach (double item in yVals)
                    _rVal.Add(new mdStartupSpout(aSU) { Y = item, X = xright });
            }
            return _rVal;
        }

        public uopHoleArray Spouts(mdTrayAssembly aAssy, bool bReturnMirroredGroups = true)
        {
            aAssy ??= GetMDTrayAssembly(aAssy); return (aAssy != null) ? new uopHoleArray(mdUtils.GetSpoutGroupsSpouts(  SpoutGroups(aAssy), false, false, bReturnMirroredGroups, Thickness, WeirHeight - Height + 0.5 * Thickness)) : new uopHoleArray();

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

        /// <summary>
        /// the end plate part of an md downcomer box
        /// </summary>
        /// <param name="bTop"></param>
        /// <returns></returns>
        public mdEndPlate EndPlate(bool bTop = true, double? aHeight = null) => new mdEndPlate(this, !bTop, aHeight) { Category = "Sub Parts (Welded)" };
        /// <summary>
        /// the end support part of an md downcomer box
        /// </summary>
        public mdEndSupport EndSupport(bool bTop = true) => new mdEndSupport(this, !bTop) { Category = "Sub Parts (Welded)" };
        public List<mdEndSupport> EndSupports(uppSides aSide = uppSides.Undefined)
        {
            List<mdEndSupport> _rVal = new List<mdEndSupport>();
            if (aSide != uppSides.Top && aSide != uppSides.Bottom)
            {
                _rVal.Add(EndSupport(bTop: true));
                _rVal.Add(EndSupport(bTop: false));
            }
            else
            {
                _rVal.Add(EndSupport(bTop: aSide == uppSides.Top));
            }

            return _rVal;
        }

        public List<mdEndPlate> EndPlates(dxxSides aSide = dxxSides.Undefined)
        {
            List<mdEndPlate> _rVal = new List<mdEndPlate>();
            if (aSide != dxxSides.Top && aSide != dxxSides.Bottom) aSide = dxxSides.Undefined;
            if (aSide == dxxSides.Top || aSide == dxxSides.Undefined)
            {
                _rVal.Add(EndPlate(bTop: true));
            }

            if (aSide == dxxSides.Bottom || aSide == dxxSides.Undefined)
            {
                _rVal.Add(EndPlate(bTop: false));
            }

            return _rVal;
        }

       
        public List<mdEndAngle> EndAngles(mdProject aProject = null)
        {
         aProject ??= GetMDProject();
            if(aProject != null)
            {
                return aProject.EndAngles(RangeGUID, this.PartNumber);
            }
            else
            {
                return mdPartGenerator.EndAngles_DCBox(this);
            }
            
        }

        public bool IsChamfered(uppSides aEnd, uppSides aSide)
        {
            if (aEnd == uppSides.Top) // The end angle is at the top of the box
            {
                if (IntersectionType_Top == uppIntersectionTypes.ToRing)
                {
                    if (X > 0)
                    {
                        return aSide == uppSides.Right;
                    }

                    if (X < 0)
                    {
                        return aSide == uppSides.Left;
                    }
                    else
                    {
                        return true; // The end angle at X = 0 is always chamfered when intersects the ring
                    }
                }
                else 
                { 
                    return aSide == uppSides.Left; // The end angle which intersects the beam is chamfered when is on top left
                }
            }

            if (aEnd == uppSides.Bottom) // The end angle is at the bottom of the box
            {
                if (IntersectionType_Bot == uppIntersectionTypes.ToRing)
                {
                    if (X > 0)
                    {
                        return aSide == uppSides.Right;
                    }

                    if (X < 0)
                    {
                        return aSide == uppSides.Left;
                    }
                    else
                    {
                        return true; // The end angle at X = 0 is always chamfered when intersects the ring
                    }
                }
                else
                {
                    return aSide == uppSides.Right; // The end angle which intersects the beam is chamfered when is on bottom right
                }
            }

            return false;
        }

        public colDXFVectors ExtractDowncomerBelowVertices(bool suppressFoldOvers = false, bool trimOrder = false)
        {
            colDXFVectors verts = new colDXFVectors();

            var mdTrayAssembly = GetMDTrayAssembly();
            if (mdTrayAssembly != null && mdTrayAssembly.DesignFamily.IsBeamDesignFamily())
            {
                verts = Vertices(false, suppressFoldOvers, trimOrder);

                // Mirror with respect to X = 0
                double length = mdTrayAssembly.RingID + 100;
                dxeLine verticalAtZero = new dxeLine(new dxfVector(0, length), new dxfVector(0, -length));
                verts.Mirror(verticalAtZero);

                // Rotate the shape based on the number of beams
                double rotationAngle = 90;
                if (mdTrayAssembly.Beam.Offset > 0 && Row == 2) // two beams and the middle row
                {
                    rotationAngle = 270;
                }

                verts.RotateAbout(dxfVector.Zero, rotationAngle);
            }
            else
            {
                verts = Vertices(true, suppressFoldOvers, trimOrder);
            }

            return verts;
        }

        /// <summary>
        /// This method returns the top and bottom ordinates of the zone at the top of the box in which a stiffener is required.
        /// </summary>
        /// <returns></returns>
        public (double MaxY, double MinY) TopMandatoryStiffenerRangeOrdinates()
        {
            double topOrdinate = TopMandatoryStiffenerRangeTopOrdinate();
            double bottomOrdinate = TopMandatoryStiffenerRangeBottomOrdinate(topOrdinate);
            return (topOrdinate, bottomOrdinate);
        }

        /// <summary>
        /// This method returns the top and bottom ordinates of the zone at the bottom of the box in which a stiffener is required.
        /// </summary>
        /// <returns></returns>
        public (double MaxY, double MinY) BottomMandatoryStiffenerRangeOrdinates()
        {
            double bottomOrdinate = BottomMandatoryStiffenerRangeBottomOrdinate();
            double topOrdinate = BottomMandatoryStiffenerRangeTopOrdinate(bottomOrdinate);
            return (topOrdinate, bottomOrdinate);
        }

        private double TopMandatoryStiffenerRangeTopOrdinate()
        {
            var boxHoles = GenHolesV(GetMDTrayAssembly(), aTag: "END ANGLE").Item(1);
            var topRightHole = boxHoles.GetFlagged("UR");
            var topLeftHole = boxHoles.GetFlagged("UL");
            double maxPossibleY = Math.Min(topRightHole.Y, topLeftHole.Y) - topLeftHole.Radius - 0.25 - 1.5;
            return maxPossibleY;
        }

        private double TopMandatoryStiffenerRangeBottomOrdinate(double topMandatoryStiffenerRangeTopOrdinate)
        {
            ULINE topLimitLine = LimitLns.Line1.Value;

            double bottomOrdinate = topLimitLine.MidPt.Y;

            if (DesignFamily.IsEcmdDesignFamily())
            {
                bottomOrdinate -= 9;
            }
            else
            {
                bottomOrdinate -= 18;
            }

            //if (bottomOrdinate < 0) bottomOrdinate = 0; // It looks like this logic was related to symmetry, but it is not needed anymore
            if (bottomOrdinate > topMandatoryStiffenerRangeTopOrdinate) bottomOrdinate = topMandatoryStiffenerRangeTopOrdinate;

            return bottomOrdinate;
        }

        private double BottomMandatoryStiffenerRangeBottomOrdinate()
        {
            var boxHoles = GenHolesV(GetMDTrayAssembly(), aTag: "END ANGLE").Item(1);
            var bottomRightHole = boxHoles.GetFlagged("LR");
            var bottomLeftHole = boxHoles.GetFlagged("LL");
            double minPossibleY = Math.Max(bottomRightHole.Y, bottomLeftHole.Y) + bottomLeftHole.Radius + 0.25 + 1.5;
            return minPossibleY;
        }

        private double BottomMandatoryStiffenerRangeTopOrdinate(double bottomMandatoryStiffenerRangeBottomOrdinate)
        {
            ULINE bottomLimitLine = LimitLns.Line2.Value;

            double topOrdinate = bottomLimitLine.MidPt.Y;

            if (DesignFamily.IsEcmdDesignFamily())
            {
                topOrdinate += 9;
            }
            else
            {
                topOrdinate += 18;
            }

            //if (topOrdinate > 0) topOrdinate = 0; // It looks like this logic was related to symmetry, but it is not needed anymore
            if (topOrdinate < bottomMandatoryStiffenerRangeBottomOrdinate) topOrdinate = bottomMandatoryStiffenerRangeBottomOrdinate;

            return topOrdinate;
        }

        public bool IsValidStiffenerOrdinate(double yOrdinate)
        {
            bool isValid = yOrdinate <= (TopMandatoryStiffenerRangeOrdinates().MaxY + 0.01) && yOrdinate >= (BottomMandatoryStiffenerRangeOrdinates().MinY - 0.01);

            return isValid;
        }

        public double MinimumStiffenerDistanceFromBox(double yOrdinate)
        {
            double topMax = TopMandatoryStiffenerRangeOrdinates().MaxY;
            double bottomMin = BottomMandatoryStiffenerRangeOrdinates().MinY;

            if (yOrdinate > (topMax + 0.01))
            {
                return yOrdinate - topMax;
            }

            if (yOrdinate < (bottomMin - 0.01))
            {
                return bottomMin - yOrdinate;
            }

            return 0;
        }

        public List<double> GetStiffenerOrdinates(bool bRelativeToCenter = false)
        {
            List<double> ords = StiffenerYs();
            if (!bRelativeToCenter) return ords;
            List<double> ordinates = new List<double>();

            foreach (var item in ords)
            { 
                ordinates.Add(item-Y);
            }
                return ordinates;


        }

        /// <summary>
        /// This method returns the non-virtual downcomer box. If the box is not virtual, it returns the box itself otherwise, it returns its non-virtual counterpart.
        /// </summary>
        /// <returns></returns>
        public mdDowncomerBox GetNonVirtualCounterpart(mdTrayAssembly aAssy = null)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return this;
            if (this.IsVirtual)
            {
                mdDowncomerBox nonVirtualCounterpart;
                foreach (var dc in aAssy.Downcomers)
                {
                    nonVirtualCounterpart = dc.Boxes.FirstOrDefault(b => !b.IsVirtual && b.PartNumber == PartNumber);
                    if (nonVirtualCounterpart != null)
                    {
                        return nonVirtualCounterpart;
                    }
                }

                return null;
            }
            else
            {
                return this;
            }
        }

        #endregion Methods

        #region Shared Methods

        public static List<double> StiffenerYValues( mdDowncomerBox aBox,  mdDowncomer aDowncomer = null)
        {

            List<double> _rVal = new List<double>();
            if (aBox == null) return _rVal;
            aDowncomer ??= aBox.Downcomer;
            if (aDowncomer == null) return _rVal;
            if(aDowncomer.ProjectType == uppProjectTypes.MDSpout) return _rVal;
            uopVectors bxPts = aBox.BoxLines.EndPoints();
            if (bxPts.Count == 4)
            {

                double x = aBox.X;
                List<double> stiffYs = aDowncomer.StiffenerYs;
                double maxY = bxPts.GetVectors(dxxPointFilters.GreaterThanY, aBox.Y).GetOrdinate(dxxOrdinateTypes.MinY) - 2;
                double minY = bxPts.GetVectors(dxxPointFilters.LessThanY, aBox.Y).GetOrdinate(dxxOrdinateTypes.MaxY) + 2;
                foreach (var item in stiffYs)
                {
                    if (item < maxY && item > minY)
                    {
                        _rVal.Add(item);
                    }
                }

            }
            return _rVal;


        }
        #endregion Shared Methods
    }
}