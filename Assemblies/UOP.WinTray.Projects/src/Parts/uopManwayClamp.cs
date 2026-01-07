using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;

using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// ^manway clamp object
    /// ~the oval clamp used to hold down the manway panel
    /// </summary>
    /// 

    public class uopManwayClamp : uopPart
    {
        /// <summary>
        /// //startinhg here

        public override uppPartTypes BasePartType => uppPartTypes.ManwayClamp;



        public uopManwayClamp() :base(uppPartTypes.ManwayClamp, uppProjectFamilies.uopFamMD, "","",true)
        {
            Class_Initialize();
        }

        public uopManwayClamp(uopTrayAssembly aParent) : base(uppPartTypes.ManwayClamp, uppProjectFamilies.uopFamMD, "", "", true)
        {
            Class_Initialize();

             if ( aParent != null){
                uopMaterials mtrls = uopGlobals.goSheetMetalOptions();
                uopPart aDeck = aParent.DeckObj;

                uppSheetGages aGage = aDeck.SheetMetal.IsMetric ? uppSheetGages.Gage3pt5mm : uppSheetGages.Gage10;
                SubPart(aParent);

                 SheetMetal = mtrls.GetByFamilyAndGauge(aDeck.SheetMetalFamily,aGage);
                HardwareMaterial = aParent.RangeHardwareMaterial;
             
            }
            BoltWidth = Stud.MinorDiameter;

        }

        internal uopManwayClamp(uopManwayClamp aPartToCopy) : base(uppPartTypes.ManwayClamp, uppProjectFamilies.uopFamMD, "", "", true)
        {
            Copy(aPartToCopy);
             CenterDXF = aPartToCopy.CenterDXF;
         
        }

        /// <summary>
        /// Class initialize
        /// </summary>
        private void Class_Initialize()
        {
            // TODO (not supported):     On Error Resume Next
               base.CenterDXF = new dxfVector();
            SparePercentage = 5;
            Category = "Manway Attachment";
            AddProperty("BigRadius", 1, uppUnitTypes.SmallLength);
            AddProperty("SmallRadius", 0.375, uppUnitTypes.SmallLength);
            AddProperty("GripHeight", 0.105, uppUnitTypes.SmallLength);
            AddProperty("GripLength", 0.5, uppUnitTypes.SmallLength);
            AddProperty("GripWidth", 0.25, uppUnitTypes.SmallLength);
            AddProperty("GripInset", 0.125, uppUnitTypes.SmallLength);
            AddProperty("Width", 1, uppUnitTypes.SmallLength);
            AddProperty("Length", 1, uppUnitTypes.SmallLength);
            AddProperty("BottomSide", false);
            AddProperty("BeamIndex", 0);
            AddProperty("AngleIndex", 0);
            AddProperty("Center","0,0,0");



            PartNumber = "62";
        }

        public override double Width { get => PropValD("Width"); set => PropValSet("Width", value, bSuppressEvnts: true); }

        public override double Length { get => PropValD("Length"); set => PropValSet("Length", value, bSuppressEvnts: true); }

        public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null)
        {
            uopHoleArray iHoleArrayobj = null;
            iHoleArrayobj = new uopHoleArray();
            //TODO:
            //iHoleArrayobj.AddHole(hl_Object(hl_FromHole(Hole)));
            return iHoleArrayobj;
        }


        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;
            return IsEqual((uopManwayClamp)aPart);
        }

        

        /// <summary>
        /// ~the index of the angle that this plate attaches to
        /// </summary>
        public int AngleIndex { get => PropValI("AngleIndex"); set => PropValSet("AngleIndex", value, bSuppressEvnts: true); }

        /// <summary>
        /// ~the index of the beam that this plate attaches to
        /// </summary>
        public int BeamIndex { get => PropValI("BeamIndex"); set => PropValSet("BeamIndex", value, bSuppressEvnts: true); }

        /// <summary>
        /// ~the larger of the two radius dimensions
        /// </summary>
        public double BigRadius { get => PropValD("BigRadius"); set => PropValSet("BigRadius", value, bSuppressEvnts: true); }


        /// <summary>
        /// ~the minor diameter of the flat hole
        /// </summary>
        public double BoltWidth { get => PropValD("BoltWidth"); set => PropValSet("BoltWidth", value, bSuppressEvnts: true); }


        /// <summary>
        /// ^True if the clamp is the bottom clamp of a set
        /// </summary>
        public bool BottomSide { get => PropValB("BottomSide"); set => PropValSet("BottomSide", value, bSuppressEvnts: true); }

      
        /// <summary>
        /// ^the center point of the angle with respect to center of the column
        /// </summary>
        public override dxfVector CenterDXF { get { base.Center.Value = Angle; return base.Center.ToDXFVector(bValueAsRotation: true, aTag: Tag, aFlag: Flag); ; ; } set => base.CenterDXF = value; }

        /// <summary>
        /// ^returns the height of the grip nib
        /// </summary>
        public double GripHeight { get => PropValD("GripHeight"); set => PropValSet("GripHeight", value, bSuppressEvnts: true); }

        /// <summary>
        /// ^the distance the grip nib is inset from the edege
        /// </summary>
        public double GripInset { get => PropValD("GripInset"); set => PropValSet("GripInset", value, bSuppressEvnts: true); }


        /// <summary>
        /// the length of the grip nib
        /// </summary>
        public double GripLength { get => PropValD("GripLength"); set => PropValSet("GripLength", value, bSuppressEvnts: true); }



        /// <summary>
        /// the radius of the grip nib
        /// </summary>
        public double GripRadius { get => PropValD("GripRadius"); set => PropValSet("GripRadius", value, bSuppressEvnts: true); }

        /// <summary>
        /// the width of the grip nib
        /// </summary>
        public double GripWidth { get => PropValD("GripWidth"); set => PropValSet("GripWidth", value, bSuppressEvnts: true); }


        /// <summary>
        /// flat sided hole in the center of the clamp
        /// </summary>
        public uopHole Hole
        {
            get
            {
                uopHole ret = new uopHole(aDiameter: mdGlobals.gsBigHole, aX: X, aY: Y, aMinorRadius: 0.14, aRotation:Angle-90, aDepth:Thickness, aInset: mdGlobals.gsBigHole/2);

               
                //dxeHole _rVal = new dxeHole( aCenter: new dxfVector(X, Y), aDiameter: mdGlobals.gsBigHole, aLength: 0, aDepth: Thickness) { MinorRadius = 0.14, Inset = 0.192 };
                //BoltWidth = Stud.MinorDiameter;
                //ret.MinorRadius = 1.0352 * Stud.MinorDiameter / 2;
                //_rVal.Inset = _rVal.Radius;

                //_rVal.Rotate(Angle - 90);
                return ret;
            }
            set
            {
                CenterDXF = (value != null) ? value.CenterDXF : dxfVector.Zero;
              

            }
        }

    
        /// <summary>
        /// ^the lock washer used to attach the manway clamp.
        /// ~recalculated on each request.
        /// </summary>
        public hdwLockWasher LockWasher =>  Stud.GetLockWasher(2 * Quantity);

       

        /// <summary>
        /// thenut used to attach the manway clamp.
        /// recalculated on each request.
        /// </summary>
        public hdwHexNut Nut => Stud.GetNut(2 * Quantity);
        
        
        /// <summary>
        /// the smaller of the two radius dimensions
        /// </summary>
        public double SmallRadius { get => PropValD("SmallRadius"); set => PropValSet("SmallRadius", value, bSuppressEvnts: true); }


        /// <summary>
        /// the shaved stud used to attach the manway clamps.
        /// recalculated on each request.
        /// </summary>
        public hdwStud Stud
        {
            get
            {
                hdwStud _rVal = new hdwStud
                {
                    Quantity = Quantity,
                    HardwareMaterial = HardwareMaterial
                };

                if (Bolting == uppUnitFamilies.English)
                {
                    _rVal.Size =uppHardwareSizes.ThreeEights;
                    _rVal.Length = 2;
                    _rVal.MinorDiameter = 0.339;

                }
                else
                {
                    _rVal.Size = uppHardwareSizes.M10;
                    _rVal.Length = 50 / 25.4;
                    _rVal.MinorDiameter = 0.339;
                }
                _rVal.DescriptiveName = "Manway Clamp Stud";
                _rVal.SubPart(this, Category);
                _rVal.SparePercentage = SparePercentage;

                return _rVal;
            }
        }

       
        /// <summary>
        /// the washer used to attach the manway clamp.
        /// recalculated on each request.
        /// </summary>
        public hdwFlatWasher Washer => Stud.GetWasher();
       
        /// <summary>
        /// the weight of the part in english pounds
        /// </summary>
        public override double Weight => (Perimeter().Area - Hole.Area) * base.SheetMetalWeightMultiplier;

       
        /// <summary>
        /// the X coordinate of the center of the part
        /// </summary>
        public override double X { get=> CenterDXF.X; set => CenterDXF.X = value; }


        /// <summary>
        /// the Y coordinate of the center of the part
        /// </summary>
        public override double Y { get => CenterDXF.Y; set => CenterDXF.Y = value; }

        /// <summary>
        /// the Z coordinate of the center of the part
        /// </summary>
        public override double Z { get => CenterDXF.Z; set => CenterDXF.Z = value; }


        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        public uopManwayClamp Clone() => new uopManwayClamp(this); 

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();
        
        /// <summary>
        ///returns a polygon that is the elevation view of the clamp
        /// </summary>
        public dxePolygon Elevation(double ScaleFactor = 1)
        {
           
            dxfVector v1 = new dxfVector();
            dxfVector v2 = new dxfVector();
            uopHole aHole = Hole;
            double leng = Length;
            double thk = Thickness;
            double cX = X;
            double cY = Z;

            //================ TOP VIEW ============================

            colDXFVectors verts = new colDXFVectors
            {
                { 0, 0 },
                { leng, 0 },
                { leng, thk },
                { 0.375, thk },
                { 0.375, thk + 0.005 },
                { 0.275, thk + 0.105 },
                { 0.225, thk + 0.105 },
                { 0.125, thk + 0.005 },
                { 0.125, thk },
                { 0, thk }
            };

            dxePolygon _rVal = new dxePolygon(verts);

            v1.SetCoordinates(0.375);
            v2.SetCoordinates(0.375, 0.005);
            _rVal.AdditionalSegments.Add(new dxeLine(v1, v2));
            v1.SetCoordinates(0.275, 0.105);
            _rVal.AdditionalSegments.AddArcPointToPoint(v2, v1, 0.1);

            v2.SetCoordinates(0.225, 0.105);
            _rVal.AdditionalSegments.Add(new dxeLine(v1, v2));
            v1.SetCoordinates(0.125, 0.005);
            _rVal.AdditionalSegments.AddArcPointToPoint(v2, v1, 0.1);
            v2.SetCoordinates(0.125, 0);
            _rVal.AdditionalSegments.Add(new dxeLine(v1, v2));

            v1.SetCoordinates(0.5 * leng - aHole.Radius, 0);
            v2.SetCoordinates(v1.X, thk);
            _rVal.AdditionalSegments.Add(new dxeLine(v1, v2));
            v1.SetCoordinates(0.5 * leng + aHole.Radius, 0);
            v2.SetCoordinates(v1.X, thk);
            _rVal.AdditionalSegments.Add(new dxeLine(v1, v2));

            _rVal.Closed = true;
            _rVal.Move(cX - 0.5 * leng, cY);

            _rVal.InsertionPt.SetCoordinates(cX, cY);
            _rVal.Rescale(ScaleFactor);

          
            return _rVal;
        }


        public dxeInsert Insert(uppPartViews aView, dxfImage aImage, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor,
            string aLayer = "MANWAY_CLAMPS", dxxColors aColor = dxxColors.ByLayer, string aLinetype = "Continuous",
            bool bDrawNegativeYs = false, bool bDrawNegativeXs = false, dxfBlock aBlock = null)
        {
          
            dxfVector ctr = CenterDXF;

            

            if (Math.Round(ctr.X, 1) == 0) bDrawNegativeXs = false;
      
            aBlock = Block(aView, aImage, aLTLSetting, aLayer, aColor, aLinetype);
            dxfDisplaySettings aDisplay = aImage != null ? aDisplay = aImage.GetDisplaySettings(dxxEntityTypes.Polyline, aLayer, (dxxColors)aColor, aLinetype, bSuppressed: Suppressed): new dxfDisplaySettings(aLayer,aColor,aLinetype);
            dxeInsert insert = new dxeInsert(aBlock)
            {
                InsertionPt = (aImage != null) ? aImage.UCS.Vector(ctr.X, ctr.Y) : new dxfVector(ctr.X, ctr.Y),
                DisplaySettings = aDisplay
            };

            return insert;
        }

        public dxfBlock Block(uppPartViews aView_UNUSED, dxfImage aImage, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor,
            string aLayer = "MANWAY_CLAMPS", dxxColors aColor = dxxColors.ByLayer,
            string aLinetype = "Continuous")
        {
       
            dxfDisplaySettings aDisplay;
            
            
            dxePolygon aPerim = Perimeter(true);

            if (aImage != null)
            {
                aDisplay = aImage.GetDisplaySettings(dxxEntityTypes.Polyline, aLayer, (dxxColors)aColor, aLinetype,bSuppressed: Suppressed);
            }
            else
            {
                aDisplay = new dxfDisplaySettings
                {
                    LayerName = aLayer,
                    Color = (dxxColors)aColor,
                    Linetype = aLinetype
                };
            }

            aPerim.DisplaySettings = aDisplay;
            return aPerim.Block("MANWAY_CLAMP", false, aImage, aLTLSetting, false, aDisplay.LayerName);


        }

        /// <summary>
        /// returns true if materials are equal
        /// </summary>
        public bool IsEqual(uopManwayClamp aPart)
        {
            bool isEqual = false;
            if (!Material.IsEqual(aPart.Material))
            {
                return isEqual;

            }
            isEqual = true;
            return isEqual;
        }

        /// <summary>
        ///returns a polygon that is the Plan view of the clamp
        /// </summary>
        public dxePolygon Perimeter(bool aSuppressRotation = false)
        {
            
            dxeHole aHole = Hole.ToDXFHole;
            double leng = Length;
            double radSmall = SmallRadius;
            double radBig = BigRadius;
         
            double cX = X;
            double cY = Y;
            //initialize

            dxeArc circ1  = new dxeArc(new dxfVector(0.5,0),radSmall);
            dxeArc circ2 = new dxeArc(dxfVector.Zero, radBig);

        


            //get the intercepts

            colDXFVectors iPts = circ1.Intersections(circ2);
            dxfVector v1 = iPts.GetVector(dxxPointFilters.AtMinY);
            //dxfVector  v2 = iPts.GetVector(dxxPointFilters.AtMaxY);

            //================ TOP VIEW ============================


            colDXFVectors verts = new colDXFVectors
            {
                { v1.X, v1.Y },

                { circ1.X + radSmall, circ1.Y },
                { v1.X, -v1.Y }
            };
            verts.AddMirrors(0);
            verts.Item(4).Radius = radSmall;
            verts.Item(5).Radius = radSmall;
            verts.Item(6).Radius = radBig;
            verts.Move(cX, cY + aHole.Inset);
            double val = -0.5 * leng + GripInset + 0.5 * GripWidth;
            v1.MoveTo(CenterDXF, val, aHole.Inset);
            dxePolygon _rVal = new dxePolygon(verts);

            
            _rVal.InsertionPt.SetCoordinates(X, Y);

            _rVal.AdditionalSegments.Append(uopGlobals.UopGlobals.Primatives.Rectangle(v1, false, GripLength, GripWidth, 0.1).Segments);
            _rVal.AdditionalSegments.Append(aHole.Perimeter().Segments);
            colDXFEntities Hls = new colDXFEntities
            {
                aHole
            };
            uopUtils.AddHolesToPGON(_rVal, Hls, uppPartViews.Top);
           if (!aSuppressRotation)
           {
                if (Angle != 0) _rVal.RotateAbout(_rVal.InsertionPt, Angle);
         
            }


            return _rVal;
        }

        /// <summary>
        /// //&Empty_Function&
        /// </summary>
        public override void UpdatePartProperties()
        {
            PropValSet("Center", CenterDXF.Coordinates, bSuppressEvnts: true);
        }

        public override void UpdatePartWeight()
        {
            base.Weight = Weight;
        }


    }
}