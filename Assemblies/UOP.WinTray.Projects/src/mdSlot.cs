using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;


namespace UOP.WinTray.Projects
{


    public class mdSlot
    {

        //flow slot object
        //flow slows are the slots in the deck panel that direct the flow of fluid across the panel surface
        private TFLOWSLOT _Struc;


        public mdSlot() => _Struc = new TFLOWSLOT(uppFlowSlotTypes.HalfC);
       


        public mdSlot(uppFlowSlotTypes aType = uppFlowSlotTypes.HalfC) => _Struc = new TFLOWSLOT(aType);

        internal mdSlot(TFLOWSLOT aStructure) => _Struc = aStructure;

        /// <summary>
        /// the center of the slot
        /// </summary>
        public uopVector Center
        {
            get =>  new uopVector(X,Y);

            set
            {
                if (value != null)
                {
                    _Struc.X = value.X;
                    _Struc.Y = value.Y;
                }
                else
                {
                    _Struc.X = 0;
                    _Struc.Y = 0;
                }
            }
        }


        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdSlot Clone() => new mdSlot(new TFLOWSLOT( _Struc));

        /// <summary>
        /// the die are for the slot footprint
        /// </summary>
        public double DieArea => DieWidth * DieHeight;

        /// <summary>
        /// the Height of the die used to punch the slot
        /// constant 1.25 inches
        /// </summary>
        public double DieHeight => mdGlobals.SlotDieHeight;

        public uopRectangle DieRectangle (double? aRotation = 0, uopVector aCenter = null) =>new uopRectangle(aCenter == null ? Center : aCenter,DieWidth,DieHeight,aRotation: aRotation.HasValue ? aRotation.Value : RotationAngle) ;

        /// <summary>
        /// the width of the die used to punch the slot
        /// constant 0.76 inches
        /// </summary>
        public double DieWidth => mdGlobals.SlotDieWidth;

        /// <summary>
        /// the polygon that represents the perimeter of the slot tool footprint
        /// </summary>
        /// <param name="bSuppressAngle"></param>
        /// <param name="bZeroCenter"></param>
        /// <param name="bArrowStyle"></param>
        /// <returns></returns>
        public dxePolyline FootPrint(bool bSuppressAngle = false, bool bZeroCenter = false, bool bArrowStyle = false,dxfDisplaySettings aDisplaySettings = null,bool bInverted = false )
        {
            dxePolyline _rVal = null;
            double ang = _Struc.Angle;
            double d1 = 0.1214;
            double d2 = 0.5036;
            double ht = DieHeight;
            double wd = DieWidth;
            double cX = 0;
            double cY = 0;
           
           
            if (!bZeroCenter)
            {
                cX = X;
                cY = Y;
            }
            //_rVal.Linetype = dxfLinetypes.Continuous;
            colDXFVectors verts = new colDXFVectors();

            if (bArrowStyle) verts.Add(cX, cY);



            verts.Add(cX - 0.5 * wd + d2, cY - 0.5 * ht + d1);
            verts.Add(cX - 0.5 * wd, cY);
            verts.Add(cX - 0.5 * wd, cY - 0.5 * ht);
            verts.Add(cX + 0.5 * wd, cY - 0.5 * ht);
            verts.Add(cX + 0.5 * wd, cY + 0.5 * ht);
            verts.Add(cX - 0.5 * wd, cY + 0.5 * ht);
            verts.Add(cX - 0.5 * wd, cY);
            verts.Add(cX - 0.5 * wd + d2, cY + 0.5 * ht - d1);
            if (bArrowStyle) verts.Add(cX, cY);
            if(bInverted) verts.MirrorPlanar(cX, null);
                if (!bSuppressAngle) verts.RotateAbout(Center, ang);
            _rVal = new dxePolyline(verts, false, aDisplaySettings) { Tag = "SLOT", Flag = "DIE RECTANGLE" } ;
            return _rVal;
        }

        public dxfBlock Block(uppPartViews aView, dxfImage aImage, string  aBlockName, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor, string aLayer = "ECMD SLOTS", dxxColors aColor = dxxColors.ByLayer, string aLinetype = "Continuous")
        {
            dxfBlock _rVal = new dxfBlock();
            dxfDisplaySettings aDisplay = null;
            dxfVector ctr =new dxfVector( Center);
            dxePolyline aPerim = FootPrint(bSuppressAngle: true,  bZeroCenter:true);
            _rVal.InsertionPt.SetCoordinates(0, 0);
            if (aImage != null)
                aDisplay = aImage.GetDisplaySettings(dxxEntityTypes.Polyline, aLayer, aColor,aLinetype, dxxLinetypeLayerFlag.Suppressed);
            else
                aDisplay = new dxfDisplaySettings( aLayer, aColor,aLinetype);
          
            _rVal.LayerName = aDisplay.LayerName;
            _rVal.Entities.Add(aPerim);
            aBlockName = string.IsNullOrWhiteSpace(aBlockName) ? $"FLOW_ SLOT" : aBlockName.Trim(); 
            _rVal.Name = aBlockName;
            aPerim.LayerName = aDisplay.LayerName;
            if (aLTLSetting == dxxLinetypeLayerFlag.ForceToColor || aLTLSetting == dxxLinetypeLayerFlag.ForceToLayer)
            {
                if (aImage != null)
                    aImage.LinetypeLayers.ApplyTo(aPerim, aLTLSetting, aImage);
            }

            _rVal.Entities.AddPoint(null, aDisplay.LayerName);
            return _rVal;
        }
        /// <summary>
        /// sets the X,Y  components of the part
        /// unpassed or non-numeric values are ignored
        /// </summary>
        /// <param name="aNewX">the value to set the X coordinate to</param>
        /// <param name="aNewY">the value to set the Y coordinate to</param>
        /// <param name="aRotation"></param>
        public void SetCoordinates(dynamic aNewX = null, dynamic aNewY = null, dynamic aRotation = null)
        {
            try
            {
                if (aNewX != null) _Struc.X = mzUtils.VarToDouble(aNewX, aDefault: _Struc.X);
                if (aNewY != null) _Struc.Y = mzUtils.VarToDouble(aNewY, aDefault: _Struc.Y);
                if (aRotation != null)
                {
                    if (mzUtils.IsNumeric(aRotation)) _Struc.Angle = mzUtils.VarToDouble(aRotation,aDefault: _Struc.Angle);
                    
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
        }
        /// <summary>
        /// the index number of the panel that the slot falls on
        /// </summary>
        public int PanelIndex { get=> _Struc.PanelIndex; set => _Struc.PanelIndex = value; }

        /// <summary>
        /// the rotation angle of the slot
        /// </summary>
        public double RotationAngle { get => _Struc.Angle; set => _Struc.Angle = value; }

        /// <summary>
        /// the index number of the section that the slot falls on
        /// </summary>
        public int SectionIndex { get => _Struc.SectionIndex; set => _Struc.SectionIndex = value; }

        /// <summary>
        /// the area of a single flow slot in sqr. inches
        /// depends on Deck.SlotType
        /// </summary>
        public double SlotArea =>SlotType.SlotArea();

        /// <summary>
        /// the height of the slot
        /// constant 0.7567 inches
        /// </summary>
        public double SlotHeight => _Struc.Height;

        /// <summary>
        /// the slot type
        /// </summary>
        public uppFlowSlotTypes SlotType
        {
            get => _Struc.SlotType;
            set
            {
                if (value == uppFlowSlotTypes.FullC || value == uppFlowSlotTypes.HalfC)
                {
                    _Struc.SlotType = value;
                }
            }
        }
        /// <summary>
        /// a string describing the type
        /// Full C or Half C
        /// </summary>
        public string SlotTypeName => _Struc.SlotTypeName;

        /// <summary>
        /// the width of the slot
        /// constant 0.3865 inches
        /// </summary>
        public double SlotWidth=> _Struc.Width;
            
        internal TFLOWSLOT Structure { get => _Struc; set =>_Struc = value; }

        /// <summary>
        /// the X coordinate of the center of the part
        /// </summary>
        public double X { get => _Struc.X; set => _Struc.X = value; }
        /// <summary>
        /// the Y coordinate of the center of the part
        /// </summary>
        public double Y { get => _Struc.Y; set => _Struc.Y = value; }
        /// <summary>
        /// the Z coordinate of the center of the part
        /// </summary>
        
        public double Z => 0;
            

    }
}
