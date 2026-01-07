using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Structures;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace UOP.WinTray.Projects.Parts
{
    public class mdShelfAngle : mdBoxSubPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.ShelfAngle;

        //^represents a shelf angle of an md downcomer assembly

        #region Constructors

        public mdShelfAngle() : base(uppPartTypes.ShelfAngle) { Initialize(); }

        public mdShelfAngle(mdDowncomerBox aParent, bool bLeft = true) : base(uppPartTypes.ShelfAngle) { Initialize(aBox: aParent, bLeft: bLeft); }


        internal mdShelfAngle(mdShelfAngle aPartToCopy) : base(uppPartTypes.ShelfAngle) { Initialize(aPartToCopy: aPartToCopy); }

        internal override void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox)
        {
            mdShelfAngle copy = null;
            if (aPartToCopy != null && aPartToCopy.GetType() == typeof(mdShelfAngle)) copy = (mdShelfAngle)aPartToCopy;
            Initialize(copy, aBox);
        }

        bool _Init = false;
        private void Initialize(mdShelfAngle aPartToCopy = null, mdDowncomerBox aBox = null, bool? bLeft = null)
        {

            if (!_Init)
            {
                {
                    EdgeLn = ULINE.Null;
                    Width = 1;
                    PanelClearance = 0.0825;
                    Side = uppSides.Left;
                    BoxIndex = 1;
                    _Init = true;
                }
                if (aPartToCopy != null)
                {
                    Copy(aPartToCopy);
                    EdgeLn = new ULINE(aPartToCopy.EdgeLn);
                    Width = aPartToCopy.Width;
                    PanelClearance = aPartToCopy.PanelClearance;
                    Side = aPartToCopy.Side;
                    BoxIndex = aPartToCopy.BoxIndex;
                    aBox ??= aPartToCopy.DowncomerBox;
                }

                if (bLeft.HasValue)
                {
                    Side = bLeft.Value ? uppSides.Left : uppSides.Right;
                }


                if (aBox != null)
                {
                    SubPart(aBox);
                    Width = aBox.ShelfWidth;
                    EdgeLn = aBox.ShelfLn(Side == uppSides.Left);
                    Length = EdgeLn.Length;
                    X = Side == uppSides.Left ? aBox.X - 0.5 * aBox.Width - 0.5 * Width : aBox.X + 0.5 * aBox.Width + 0.5 * Width;
                    Y = EdgeLn.MidY;
                    PanelClearance = aBox.PanelClearance;
                    BoxIndex = aBox.Index;

                }

            }

        }

        #endregion Constructors


        #region Properties
        internal ULINE EdgeLn { get; set; }

        /// <summary>
        /// the direction of the shelf angle with respect to shell 
        /// </summary>
        public new dxxRadialDirections Direction { get => RadialDirection; set => RadialDirection = value; }



        /// <summary>
        /// the height of the shelf
        /// default =1
        /// </summary>
        public override double Height { get => Width; set => Width = Math.Abs(value); }

        /// <summary>
        /// the length the shelf angle
        /// calculated dynamically on each request
        /// </summary>
        public override double Length { get { base.Length = EdgeLn.Length; return base.Length; } set => base.Length = EdgeLn.Length; }

        


        /// <summary>
        ///returns the area of the top flange
        /// </summary>
        public double TopArea => Length * Width;
        /// <summary>
        ///returns the weight of the part in english pounds
        /// </summary>
        public new double Weight
        {
            get
            {

                try
                {
                    double sarea = (Height + (Width - Thickness)) * Length;
                    return sarea * SheetMetalWeightMultiplier;
                }
                catch (Exception exception)
                {
                    LoggerManager log = new LoggerManager();
                    log.LogError(exception.Message);
                }
                return 0;
            }
        }
        /// <summary>
        /// the top flange width of the angle
        /// default =1
        /// </summary>
        public override double Width { get => base.Width; set { base.Width = Math.Abs(value); base.Height = base.Width; } }

        public double PanelClearance { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString() { return $"SHELF ANLGE DC:{DowncomerBox.Index} BOX:{BoxIndex} SIDE:{Side}"; }


        /// <summary>
        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdShelfAngle Clone() => new mdShelfAngle(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);

        }

        public override void UpdatePartProperties() { }
        public override void UpdatePartWeight() { base.Weight = Weight; }

        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the shelf angle
        /// </summary>
        /// <param name="bObscured"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public colDXFVectors Vertices(bool bObscured = false, iVector aCenter = null, double aRotation = 0)
        => mdPolygons.ShelfAngle_Vertices(this, bObscured, aCenter, aRotation, out dxfPlane _);

        /// <summary>
        /// #1scale factor to apply to the returned polygon
        ///returns a dxePolygon that is used to draw the end view of the shelf angle
        /// </summary>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(bool bSuppressFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        => mdPolygons.md_ShelfAngle_View_Elevation(this, bSuppressFillets, aCenter, aRotation, aLayerName);

        /// <summary>
        /// ///returns a dxePolygon that is used to draw the layout view of the shelf angle
        /// </summary>
        /// <param name="aDC"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(iVector aCenter = null, double aRotation = 0, bool bShowObscured = false, string aLayerName = "GEOMETRY")
        => mdPolygons.ShelfAngle_View_Plan(this, aCenter, aRotation, bShowObscured, aLayerName);


        /// <summary>
        ///returns a dxePolygon that is used to draw the profile view of the end angle
        /// </summary>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(dxfVector aCenter, double aRotation, string aLayerName)
        => mdPolygons.md_ShelfAngle_View_Profile(this, aCenter, aRotation, aLayerName);

        #endregion Methods



    }
}
