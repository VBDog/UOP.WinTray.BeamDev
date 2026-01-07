using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using uppPartTypes = UOP.WinTray.Projects.Enums.uppPartTypes;
using uppUnitFamilies = UOP.WinTray.Projects.Enums.uppUnitFamilies;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    ///manway clip object
    //~the sliding clip used to hold down the manway panel
    /// </summary>
    public class uopManwayClip : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.ManwayClip;
        private TCLIP tStruc;

        public uopManwayClip() :base(uppPartTypes.ManwayClip, uppProjectFamilies.uopFamMD, "", "", true)
        {
            InitializeProperties();
        }

        public uopManwayClip(uopTrayAssembly aParent) : base(uppPartTypes.ManwayClip, uppProjectFamilies.uopFamMD, "", "", true)
        {
            InitializeProperties();
            if (aParent != null)
            {
                uopMaterials mtrls = uopGlobals.goSheetMetalOptions();
                uopPart aDeck = aParent.DeckObj;
                uppSheetGages aGage = aDeck.SheetMetal.IsMetric ? uppSheetGages.Gage3pt5mm : uppSheetGages.Gage10;
                SubPart(aParent);
                SheetMetal = mtrls.GetByFamilyAndGauge(aDeck.SheetMetalFamily, aGage);
                HardwareMaterial = aParent.RangeHardwareMaterial;

                SubPart(aParent);

            }
          

        }



        internal uopManwayClip(TCLIP aStructure, uopManwayClip aPartToCopy) : base(uppPartTypes.ManwayClip, uppProjectFamilies.uopFamMD, "", "", true)
        {
            InitializeProperties();
            tStruc = aStructure;
            Copy(aPartToCopy);
        }
        private void InitializeProperties()
        {

            try
            {
               tStruc = new TCLIP() { 
                Width = 1,
                TabWidth = 0.375,
                TabHeight = 0.625,
                Height = 0.9375,
                LipLength = 0.25,
                BendAngle = 135,
                Length = 2.05,
            };
                PartNumber = "64";
                SparePercentage = 5;
                Category = "Manway Attachment";
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public override void UpdatePartProperties() { throw new NotImplementedException(); }
 

        internal  TCLIP Structure { get => tStruc; set =>tStruc = value; }
        
        /// <summary>
        /// the rotation angle of the clip
        /// </summary>
        public override double Angle { get => tStruc.Rotation; set =>  tStruc.Rotation = value; }

        /// <summary>
        /// the angle of the bended section
        /// </summary>
        public double BendAngle { get => tStruc.BendAngle; set => tStruc.BendAngle = value; }


        public dxfBlock Block(Enums.uppPartViews aView, dxfImage aImage, bool bIncludeWasher = false, bool bObscured = false, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor, string aLayer = "MANWAY_CLIPS", dxxColors aColor = dxxColors.ByLayer, string aLinetype = "")
        {
         

            if (bObscured) aLinetype = dxfLinetypes.Hidden;



            dxePolygon aPerim = View_Plan(bIncludeWasher, true, bObscured, bObscured);

            dxfDisplaySettings aDisplay = (aImage != null) ?
            aImage.GetDisplaySettings(dxxEntityTypes.Polyline, aLayer, aColor, aLinetype, dxxLinetypeLayerFlag.Suppressed) :
             new dxfDisplaySettings(aLayer, aColor, aLinetype);

            aPerim.DisplaySettings = aDisplay;
            return aPerim.Block("MANWAY_CLIP", false, aImage, aLTLSetting, false, aDisplay.LayerName);
        }


     

        /// <summary>
        /// the center point of the angle with respect to center of the column
        /// </summary>
        public override dxfVector CenterDXF
        {
            get => new dxfVector(X, Y, Z);
            set
            {
                if (value != null)
                {
                    X = value.X;
                    Y = value.Y;
                    Z = value.Z;
                }
                else
                {
                    X = 0;
                    Y = 0;
                    Z = 0;
                }

            }
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public uopManwayClip Clone() => new uopManwayClip(tStruc,this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false) { UpdatePartProperties(); return base.CurrentProperty(aPropertyName, bSupressNotFoundError); }

       
        /// <summary>
        ///returns a polygon that is the elevation view of the clip
        /// </summary>
        /// <param name="ScaleFactor"></param>
        /// <returns></returns>
        public dxePolygon Elevation(double ScaleFactor = 1)
        {
   
            dxeHole aHole = Hole;
            double thk = Thickness;
            double cX = X;
            double cY = Y;
            dxfVector v1 = new dxfVector();
        

            //initialize
    



            // ================ SIDE VIEW ============================

            dxePolygon _rVal = uopPolygons.CreateAngle(v1, 1.875, tStruc.Height, thk, thk);
            _rVal.RotateAbout(v1, 90);
            _rVal.Move(-Hole.Inset, 0.5 * tStruc.Height);

            dxeLine l1 = new dxeLine(_rVal.Vertices.Item(5), _rVal.Vertices.Item(5).PolarVector(tStruc.BendAngle + 180, 0.25));
            dxeLine l3 = new dxeLine(_rVal.Vertices.Item(3), _rVal.Vertices.Item(4));
            dxeLine l2 = l1.Clone();
            l2.MovePolar(l2.StartPt, tStruc.BendAngle + 90, thk);
            _rVal.Vertices.Item(4).MoveTo(l2.IntersectPoint(l3));

            _rVal.Vertices.Add(l2.EndPt);
            _rVal.Vertices.Add(l1.EndPt);

            if (_rVal.FilletAtVertex(4, thk)) _rVal.FilletAtVertex(8, 2 * thk);


            v1 = _rVal.Vertex(1).Moved(0, -TabHeight);

            _rVal.AdditionalSegments.Add(new dxeLine(v1, v1.Moved(thk)));
            colDXFEntities Hls = new colDXFEntities(aHole.AsViewedFrom("Y"));
            uopUtils.AddHolesToPGON(_rVal, Hls,uppPartViews.Top);

            _rVal.InsertionPt.SetCoordinates(cX, cY);
            _rVal.Rescale(ScaleFactor);

            return _rVal;
        }

        
        /// <summary>
        /// the height of the clip
        /// </summary>
        public override double Height { get => tStruc.Height; set => tStruc.Height = value; }

        /// <summary>
        /// flat sided hole in the center of the clip
        /// </summary>
        public dxeHole Hole  => new dxeHole(CenterDXF,mdGlobals.gsBigHole,aDepth:Thickness,aInset: 1.062 ) ;

        public dxeInsert Insert(Enums.uppPartViews aView, dxfImage aImage, out dxfBlock rBlock, bool bIncludeWasher = false, bool bObscured = false, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor, string aLayer = "MANWAY_CLIPS", dxxColors aColor = dxxColors.ByLayer, string aLinetype = "", bool bDrawNegativeYs = false, bool bDrawNegativeXs = false)
        {
            dxfVector ctr = CenterDXF;
            if (Math.Round(ctr.X, 1) == 0) bDrawNegativeXs = false;
            rBlock = Block(aView, aImage, bIncludeWasher, bObscured, aLTLSetting, aLayer, aColor, aLinetype);
            dxfDisplaySettings aDisplay = (aImage != null) ?
                aImage.GetDisplaySettings(dxxEntityTypes.Polyline, aLayer, (dxxColors)aColor, aLinetype, dxxLinetypeLayerFlag.Suppressed)
            :
              new dxfDisplaySettings(aLayer, aColor, aLinetype);

            dxeInsert _rVal = new dxeInsert(rBlock)
            {
                InsertionPt = (aImage != null) ? aImage.UCS.Vector(ctr.X, ctr.Y) : new dxfVector(ctr.X, ctr.Y),

                DisplaySettings = aDisplay
            };


            return _rVal;
        }

        public bool IsEqual(uopManwayClip aPart)
        {
           
            if (!Material.IsEqual(aPart.Material)) return false;
            return true;
        }


        /// <summary>
        ///  the overall length of the clip
        ///~2.050
        /// </summary>
        public override double Length { get => tStruc.Length; set => tStruc.Length = value; }

        /// <summary>
        /// the length of the bent lip
        /// </summary>
        public double LipLength { get => tStruc.LipLength; set => tStruc.LipLength = value; }

        /// <summary>
        ///the lock washer used to attach the manway clip.
        ///~recalculated on each request.
        /// </summary>
        public hdwLockWasher LockWasher => SetScrew.GetLockWasher(2 * Quantity);




        /// <summary>
        /// the slot to use to attach the clip to a panel
        /// </summary>
        public uopHole MountingSlot => mdGlobals.ManwayClipMountingSlot(null, Rotation);

        /// <summary>
        ///the nut used to attach the manway clip.
        //~recalculated on each request.
        /// </summary>
        public hdwHexNut Nut => SetScrew.GetNut(2 * Quantity);


        /// <summary>
        ///returns a polygon that is the Plan view of the clip
        /// </summary>
        /// <param name="bIncludeWasher"></param>
        /// <param name="bSuppressAngle"></param>
        /// <param name="bObscured"></param>
        /// <param name="bSuppressHole"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(bool bIncludeWasher = false, bool bSuppressAngle = false, bool bObscured = false, bool bSuppressHole = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
         => uopPolygons.ManwayClip_Plan(this, bIncludeWasher, bSuppressAngle, bObscured, bSuppressHole, aCenter, aRotation, aLayerName);
       
        /// <summary>
        ///returns a polygon that is the end view of the clip
        /// </summary>
        /// <param name="ScaleFactor"></param>
        /// <returns></returns>
        public dxePolygon Profile(double ScaleFactor = 1)
        {
           
            dxeHole aHole = Hole;
            double thk = Thickness;
            double cX = X;
            double cY = Z;
            double d1 = Math.Sqrt(Math.Pow(0.25, 2) / 2);
            dxePolygon elev  = Elevation(ScaleFactor);
            dxfVector v1 = elev.Vertex(6);
            dxfVector v2 = elev.Vertex(7);


            colDXFVectors verts = new colDXFVectors
            {


                //================ END VIEW ============================

                { cX - 0.5 * tStruc.TabWidth, cY + tStruc.Height }
            };
            verts.AddRelative();
            verts.AddRelative(-(tStruc.Width - tStruc.TabWidth) / 2);
            double x1 = verts.LastVector().X;
            verts.Add(x1, cY + thk);
            verts.Add(x1, cY);
            verts.Add(x1, v2.Y);
            verts.Add(x1, v1.Y);
            verts.AddMirrors(cX);

            dxePolygon _rVal = new dxePolygon(verts,new dxfVector(cX,cY),true);

            _rVal.AddRelationsByTag("BEND1", true);
            _rVal.AddRelationsByTag("THK1", true);
            _rVal.AddRelationsByTag("THK2", true, dxfLinetypes.Hidden);

            colDXFEntities Hls = new colDXFEntities(aHole.AsViewedFrom("Y"));
            uopUtils.AddHolesToPGON(_rVal, Hls, uppPartViews.Top);

            _rVal.InsertionPt.SetCoordinates(cX, cY);

            _rVal.Rescale(ScaleFactor);
            return _rVal;
        }



        /// <summary>
        /// the shaved stud used to attach the manway clips.
        /// recalculated on each request.
        /// </summary>
        public hdwSetScrew SetScrew
        {
            get
            {   double lng = (Bolting == uppUnitFamilies.English) ? 2 : 50 / 25.4;
               return new hdwSetScrew(this) { Quantity = Quantity, DescriptiveName = "Manway Clamp Set Screw", SparePercentage = SparePercentage,Length =lng };
          
            }
        }

        /// <summary>
        /// the height alignment tab
        /// </summary>
        public double TabHeight { get => tStruc.TabHeight; set => tStruc.TabHeight = value; }
        /// <summary>
        /// the width alignment tab
        /// </summary>
        public double TabWidth { get => tStruc.TabWidth; set => tStruc.TabWidth = value; }


        public override void UpdatePartWeight()  => base.Weight = Weight;
     
        public uopManwayClipWasher Washer
        {
            get
            {
                uopManwayClipWasher _rVal = new uopManwayClipWasher
                {
                    Angle = Angle,
                    SheetMetal = SheetMetal
                };
                _rVal.CenterDXF.SetCoordinates(X, Y, Z - Thickness);
                _rVal.SparePercentage = SparePercentage;
                _rVal.SubPart(this);
                _rVal.PartNumber = "63";
                _rVal.Quantity = Quantity;
                return _rVal;
            }
        }

        /// <summary>
        /// the weight of the part in english pounds
        /// </summary>
        public override double Weight
        {
            get=> ((Height * Width) - Math.PI * Math.Pow(mdGlobals.gsBigHole / 2, 2)) * base.SheetMetalWeightMultiplier;
               
        }

        /// <summary>
        /// the overall width of the clip
        /// 1.00
        /// </summary>
        public override double Width { get => tStruc.Width; set => tStruc.Width = value; }

        /// <summary>
        /// the X coordinate of the center of the part
        /// </summary>
        public override double X { get => tStruc.X; set => tStruc.X = value; }

        /// <summary>
        /// the Y coordinate of the center of the part
        /// </summary>
        public override double Y { get => tStruc.Y; set => tStruc.Y = value; }
   

        /// <summary>
        /// the Z coordinate of the center of the part
        /// </summary>
        public override double Z { get => tStruc.Z; set => tStruc.Z = value; }
      

    }
}