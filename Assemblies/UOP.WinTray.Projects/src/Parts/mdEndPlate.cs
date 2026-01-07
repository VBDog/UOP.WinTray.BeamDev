using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using System.Linq;

namespace UOP.WinTray.Projects.Parts
{
    public class mdEndPlate : mdBoxSubPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.EndPlate;


        //^the end plate component of a mdDowncomer

        #region Constructors

        public mdEndPlate() : base(uppPartTypes.EndPlate) => Initialize();


        public mdEndPlate(mdDowncomerBox aDowncomerBox, bool bBottomSide = false, double? aHeight = null) : base(uppPartTypes.EndPlate) => Initialize( aBox: aDowncomerBox, bBottomSide:bBottomSide, aHeight:aHeight);


        internal mdEndPlate(mdEndPlate aPartToCopy) : base(uppPartTypes.EndPlate) => Initialize(aPartToCopy: aPartToCopy);


        internal override void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox)
        {
            mdEndPlate copy = null;
            if (aPartToCopy != null && aPartToCopy.GetType() == typeof(mdEndPlate)) copy = (mdEndPlate)aPartToCopy;
            Initialize(copy, aBox);
        }


        private bool _Init = false;

        private void Initialize(mdEndPlate aPartToCopy = null, mdDowncomerBox aBox = null, bool? bBottomSide = null, double? aHeight = null)
        {
            if (!_Init)
            {
                Height = aHeight ?? 0;
                Length = 0;
                Width = 0;
                TabHeight = 0;
                TabLength = 0.75;
                HasGussets = false;
                HasFoldovers = false;
                BoltOn = false;
                IsTriangular = false;
                DeckThickness = 0;
                GussetLength = 0;
                NotchDim = 0.25;
                WeirHeight = 0;
                LeftShelfLength = 0;
                RightShelfLength = 0;
                ParentPartType = uppPartTypes.Downcomer;
                X = 0;
                Y = 0;
                Z = 0;
                Side = uppSides.Top;
                BoltOnHoles = UHOLES.Null;
                LimitLn = ULINE.Null;
                BoxLns = ULINEPAIR.Null;
                IntersectionType = uppIntersectionTypes.ToRing;
                BoxIndex = 1;
        
            _Init = true;
        }
            if (aPartToCopy != null)
            {
                base.Copy(aPartToCopy);
                Height = aHeight ?? aPartToCopy.Height;
                Length = aPartToCopy.Length;
                Width = aPartToCopy.Width;
                NotchDim = aPartToCopy.NotchDim;
                X = aPartToCopy.X;
                Y = aPartToCopy.Y;
                Z = aPartToCopy.Z;
                BoltOnHoles = new UHOLES(aPartToCopy.BoltOnHoles);
                Quantity = aPartToCopy.Quantity;

                TabHeight = aPartToCopy.TabHeight;
                TabLength = aPartToCopy.TabLength;
                HasGussets = aPartToCopy.HasGussets;
                HasFoldovers = aPartToCopy.HasFoldovers;
                BoltOn = aPartToCopy.BoltOn;
                IsTriangular = aPartToCopy.IsTriangular;
                DeckThickness = aPartToCopy.DeckThickness;
                GussetLength = aPartToCopy.GussetLength;
                BoltOnHoles = new UHOLES(aPartToCopy.BoltOnHoles);
                LeftLegLength = aPartToCopy.LeftLegLength;
                RightLegLength = aPartToCopy.RightLegLength;
                WeirHeight = aPartToCopy.WeirHeight;
                LeftShelfLength = aPartToCopy.LeftShelfLength;
                RightShelfLength = aPartToCopy.RightShelfLength;
                ParentPartType = aPartToCopy.ParentPartType;
                Side = aPartToCopy.Side;
                LimitLn = new ULINE(aPartToCopy.LimitLn);
                Overhang = aPartToCopy.Overhang;
                BoxLns = new ULINEPAIR(aPartToCopy.BoxLns);
                IntersectionType = aPartToCopy.IntersectionType;
                BoxIndex = aPartToCopy.BoxIndex;
                aBox ??= aPartToCopy.DowncomerBox;
            }

            if (aBox != null)
            {
                SubPart(aBox);

                bool top = true;
                if (bBottomSide.HasValue) top = !bBottomSide.Value;
                Side = top ? uppSides.Top : uppSides.Bottom;

                IntersectionType = top ? aBox.IntersectionType_Top : aBox.IntersectionType_Bot;
             
                LimitLn = aBox.LimitLn(bTop: top);

                BoxLns = new ULINEPAIR(aBox.BoxLns);
                OverridePartNumber = BottomSide ? "BOTTOM" : "TOP";
                OverridePartNumber = $"{aBox.PartNumber}_EP_{OverridePartNumber}";
                Overhang = aBox.EndPlateOverhang;



                X = aBox.X;
                Y = LimitLn.MidPt.Y;

                //LeftShelfLength = aBox.ShelfAngle(bLeft: true).Length;
                LeftShelfLength = aBox.LeftLength;
                //RightShelfLength = aBox.ShelfAngle(bLeft: false).Length;
                RightShelfLength = aBox.RightLength;
                
                DeckThickness = aBox.DeckThickness;
                TabHeight = DeckThickness + aBox.How;
                IsTriangular = !LimitLn.IsHorizontal(2);

                Z = 0; //TabHeight
                if (top)
                {
                    HasGussets = !IsTriangular && aBox.GussetedEndplates && IntersectionType == uppIntersectionTypes.ToRing;
                }

                HasFoldovers = aBox.FoldOverWeirs;


                BoltOn = aBox.BoltOnEndplates && IntersectionType == uppIntersectionTypes.ToRing;

                SheetMetalStructure = aBox.SheetMetalStructure;
                Width = aBox.Width;
                Height = aHeight ?? aBox.Height - Thickness;

                WeirHeight = aBox.How;

                GussetLength = 0;
                if (HasGussets)
                {
                    GussetLength = Math.Sqrt(Math.Pow(aBox.BoundingRadius, 2) - Math.Pow(X - Width / 2 + Thickness, 2));
                    GussetLength -= Math.Abs(LimitLn.ep.Y) + Length;
                    GussetLength -= 2 * Thickness;
                }

                // The leg length is defined as the sum of the distance from the limit line to the end of the box + overhang + tab length
                var leftEndOfBoxY = BottomSide ? BoxLn(bLeft: true).MinY : BoxLn(bLeft: true).MaxY;
                var rightEndOfBoxY = BottomSide ? BoxLn(bLeft: false).MinY : BoxLn(bLeft: false).MaxY;
                LeftLegLength = Math.Abs(leftEndOfBoxY - LimitLn.EndPoints.GetVector(dxxPointFilters.AtMinX).Y) + Overhang;
                RightLegLength = Math.Abs(rightEndOfBoxY - LimitLn.EndPoints.GetVector(dxxPointFilters.AtMaxX).Y) + Overhang;

                Length = LeftLegLength + TabLength;

                OverridePartNumber = BottomSide ? "BOTTOM" : "TOP";
                OverridePartNumber = $"{aBox.PartNumber} - {OverridePartNumber}";

              
            }

        }

        #endregion Constructors

        #region Properties

        public override uppPartTypes ParentPartType { get { base.ParentPartType = uppPartTypes.DowncomerBox; return base.ParentPartType; } set { base.ParentPartType = uppPartTypes.DowncomerBox; } }

        public double GussetLength { get; set; }


        public double DeckThickness { get; set; }

        public double WeirHeight { get; set; }

     
        //^True if the parent downcomer is marked for gussets on its end plates
        public bool HasGussets { get; set; }

        //^True if the parent downcomer is marked fordover weirs
        public bool HasFoldovers { get; set; }

        //^True if the parent downcomer is marked for bolting on its end plates and this plate is the top plate
        public bool BoltOn { get; set; }

        //^True if the parent downcomer is marked for triangular endplates
        public bool IsTriangular { get; set; }


        //^the size of the notch in the inside corners to accomodate the bend radius of the downcomer box
        //~constant value of 0.25''
        public double NotchDim { get; set; } = 0.25;


        //^the length of the weld tab that attaches the end plate to the support angle
        //~default is 0.75
        public double TabLength { get; set; } = 0.75;
        public double LeftLegLength { get; set; } = 1.25;
        public double RightLegLength { get; set; } = 1.25;

        public bool BottomSide => Y < BoxY;

        public double LeftBoxLength => BoxLn(bLeft: true).Length;

        public double RightBoxLength => BoxLn(bLeft: false).Length;

        public double LeftShelfLength { get; set; }
        public double RightShelfLength { get; set; }

        //^the length of the weld tab that attaches the end plate to the support angle
        //~WeirHeight + Deck Thickness
        public double TabHeight { get; set; }

        internal UHOLES BoltOnHoles { get; set; }

        public uopLine LimitLine => new uopLine(LimitLn);

        internal ULINE LimitLn { get; set; }

        internal ULINEPAIR BoxLns { get; set; }
        public double Overhang { get; set; } = 0.25;

        public uppIntersectionTypes IntersectionType { get; set; }
       

        #endregion Properties

        #region Methods

        internal ULINE BoxLn(bool bLeft, double aOffset = 0)
        {
            ULINE _rVal = bLeft ? BoxLns.GetSideValue(uppSides.Left) : BoxLns.GetSideValue(uppSides.Right);
            if (aOffset != 0)
                _rVal.MoveOrtho(aOffset);

            return _rVal;
        }

        public override string ToString() { return $"END PLATE DC:{DowncomerIndex} BOX:{BoxIndex} SIDE:{Side}"; }
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public uopHoleArray GenHoles(string aSide = "") //todo
         => new uopHoleArray(GenHolesV(aSide));


        //^executed internally to create the holes collection for the end plate
        UHOLEARRAY GenHolesV(string aSide = "")
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;
            if (!BoltOn) return _rVal;
            UHOLES holes = BoltOnHoles;
            if (holes.Count <= 0) return _rVal;
            UHOLES rholes = new UHOLES(holes, true);

            for (int i = 1; i <= holes.Count; i++)
            {
                UHOLE hole = holes.Item(i);
                UVECTOR u1 = hole.Center;
                if (string.IsNullOrWhiteSpace(aSide))
                {
                    rholes.Centers.Add(u1);
                }
                else
                {
                    if (string.Compare(aSide, "LEFT", true) == 0 && u1.X < X) rholes.Centers.Add(u1);
                    if (string.Compare(aSide, "RIGHT", true) == 0 && u1.X > X) rholes.Centers.Add(u1);
                }
            }
            _rVal.Add(rholes, rholes.Name);
            return _rVal;
        }

        /// <summary>
        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdEndPlate Clone() => new mdEndPlate(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();


        public double Inset_L
        {
            get
            {
                ULINE boxline = BoxLns.GetSideValue(aSide: uppSides.Left).Moved(Thickness);
                return Math.Abs(!BottomSide ? boxline.MaxY :    boxline.MinY - LimitLn.IntersectionPt(boxline).Y);

            }
        }

        public double Inset_R
        {
            get
            {
                ULINE boxline = BoxLns.GetSideValue(aSide: uppSides.Right).Moved(-Thickness);
                return Math.Abs(!BottomSide ? boxline.MaxY : boxline.MinY - LimitLn.IntersectionPt(boxline).Y);

            }
        }

        /// <summary>
        ///returns a the vertices of a polyline that is used to draw the plan view of the end plate without bend radiuses
        /// </summary>
        /// <returns></returns>
        public virtual uopVectors EdgeVertices()
        {
            uopVectors _rVal = new uopVectors();
            double sign = BottomSide ? -1 : 1;
            double thk = Thickness;
            ULINE leftbox = BoxLns.GetSideValue(aSide: uppSides.Left).Moved(thk);
            ULINE rightbox = BoxLns.GetSideValue(aSide: uppSides.Right).Moved(-thk);
            ULINE limline = new ULINE(LimitLn);

            double y1 = sign == 1 ? leftbox.MaxY : leftbox.MinY;
            double y2 = sign == 1 ? rightbox.MaxY : rightbox.MinY;
            y1 += sign * (this.Overhang + TabLength);
            y2 += sign * (Overhang + TabLength);

            if (limline.sp.X > limline.ep.X) limline.Invert();

            UVECTOR u2 = limline.IntersectionPt(leftbox);
            UVECTOR u1 = new UVECTOR(u2.X, y1);
            UVECTOR u3 = limline.IntersectionPt(rightbox);
            UVECTOR u4 = new UVECTOR(u3.X, y2);

            _rVal.Add(u1); _rVal.Add(u2); _rVal.Add(u3); _rVal.Add(u4);

            leftbox.Move(thk);
            rightbox.Move(-thk);

            limline.MoveOrtho(sign * thk);
            u1.X += thk;

            u2 = limline.IntersectionPt(leftbox);
            u3 = limline.IntersectionPt(rightbox);
            u4.X -= thk;

            _rVal.Add(u4); _rVal.Add(u3); _rVal.Add(u2); _rVal.Add(u1);

            return _rVal;

        }


        public override void UpdatePartProperties()
        { DescriptiveName = $"End Plate (Box {DowncomerIndex})"; }

        public override void UpdatePartWeight() => base.Weight = Weight();

        public override uopInstances Instances { get => base.Instances; set { base.Instances = value; Quantity = value == null ? 1 : value.Count + 1; } }
        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the end plate
        /// </summary>
        /// <param name="aDC"></param>
        /// <param name="bApplyFillets"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(bool bApplyFillets, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", double aCenterLineLength = 0, double aScale = 1, bool bIncludeBendPoints = false)
         => mdPolygons.EndPlate_View_Plan(this, bApplyFillets, aCenter, aRotation, aLayerName, aCenterLineLength, aScale, bIncludeBendPoints);


        //^returns a dxePolygon that is used to draw the elevation view of the end plate
        public dxePolygon View_Elevation(dxfVector aCenter, double aRotation, string aLayerName, double aCenterLineLength) //todo
         => mdPolygons.EndPlate_View_Elevation(this, aCenter, aRotation, aLayerName, aCenterLineLength);

        //#1the parent downcomer of the end plate
        //#2flag to add hidden lines where the profile is obscured
        //^returns a dxePolygon that is used to draw the profile view of the end plate
        public dxePolygon View_Profile( bool bShowObscured, bool bLeftSide, bool bVisiblePartOnly, dxfVector aCenter, double aRotation, string aLayerName, bool bSuppressHoles)
        => mdPolygons.EndPlate_View_Profile(this,  bShowObscured, bLeftSide, bVisiblePartOnly, aCenter, aRotation, aLayerName, bSuppressHoles);

        /// <summary>
        /// #1the parent downcomer of the end plate
        ///#2the plan view of this part
        ///#3flag to add hidden lines where the profile is obscured
        ///^returns a dxePolygon that is used to draw the profile view of the end plate
        /// </summary>
        /// <param name="rPlanView"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public dxePolygon View_LayoutRight(out dxePolygon rPlanView, dxfVector aCenter, double aRotation, string aLayerName = "GEOMETRY", bool bSuppressHoles = false)
         => mdPolygons.Endplate_View_LayoutRight(this, out rPlanView, aCenter, aRotation, aLayerName, bSuppressHoles);


        /// <summary>
        /// #1the parent downcomer of the end plate
        ///#2the plan view of this part
        ///#3flag to add hidden lines where the profile is obscured
        ///^returns a dxePolygon that is used to draw the profile view of the end plate
        /// </summary>
        /// <param name="rPlanView"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public dxePolygon View_LayoutLeft(out dxePolygon rPlanView, dxfVector aCenter, double aRotation, string aLayerName = "GEOMETRY", bool bSuppressHoles = false)
         => mdPolygons.Endplate_View_LayoutLeft(this, out rPlanView, aCenter, aRotation, aLayerName, bSuppressHoles);
        public new double Weight()
        {
            //^returns the weight of the endplate in english pounds

            double thk = Thickness;
            double ht = TabHeight;
            double l1 = LeftLegLength + TabLength;
            double l2 = RightLegLength + TabLength;
            double sArea = l1 - TabLength - thk + (l2 - TabLength - thk);

            sArea += 2 * TabLength * ht;
            sArea -= GenHolesV().TotalArea();
            return sArea * SheetMetalWeightMultiplier;
        }

        public override uopHoleArray HoleArray(uopTrayAssembly aAssy, string aFlag = "") // Todo
         => new uopHoleArray(GenHolesV(aFlag));

        public override bool IsEqual(uopPart aPart)
        {

            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;

            try
            {



                return CompareTo((mdEndPlate)aPart, true);

            }
            catch { return false; }
        }

        public bool CompareTo(mdEndPlate aPart, bool bCompareMaterial = true)
        {

            if (aPart == null) return false;

            try
            {



                if (!TVALUES.CompareNumeric(aPart.Width, Width, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.Height, Height, 3)) return false;

                if (aPart.IsTriangular != IsTriangular) return false;
                if (aPart.IsTriangular && aPart.BottomSide != BottomSide) return false;
                if (aPart.BoltOn != BoltOn) return false;
                if (BoltOn && aPart.HasFoldovers != HasFoldovers) return false;

                if (aPart.GussetLength > 0 && GussetLength <= 0)
                {
                    return false;
                }
                else if (aPart.GussetLength <= 0 && GussetLength > 0)
                {
                    return false;
                }
                else if (GussetLength > 0)
                {
                    if (!TVALUES.CompareNumeric(aPart.GussetLength, GussetLength, 3) || this.BottomSide != aPart.BottomSide) return false;
                }


                if (!TVALUES.CompareNumeric(aPart.DeckThickness, DeckThickness, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.TabHeight, TabHeight, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.TabLength, TabLength, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.LeftLegLength, LeftLegLength, 3)) return false;
                if (!TVALUES.CompareNumeric(aPart.RightLegLength, RightLegLength, 3)) return false;
                if (bCompareMaterial)
                    if (!aPart.Material.IsEqual(Material)) return false;


                return true;

            }
            catch { return false; }
        }

        #endregion Methods
    }
}
