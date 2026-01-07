using System;
using System.Collections.Generic;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    public class uopDividerWall : uopPart
    {

        public delegate void UOPDividerWallPropertyChange(uopProperty aProperty);
        public event UOPDividerWallPropertyChange eventUOPDividerWallPropertyChange;

        public override uppPartTypes BasePartType => uppPartTypes.DividerWall;
        #region Constructors
        public uopDividerWall() : base(uppPartTypes.DividerWall, uppProjectFamilies.uopFamMD, "", "", false) { InitializeProperties(); }

        internal uopDividerWall(uopDividerWall aPartToCopy, uopPart aParent = null) : base(uppPartTypes.DividerWall, uppProjectFamilies.uopFamMD, "", "", false)
        {
            InitializeProperties();
            if (aPartToCopy != null) base.Copy(aPartToCopy);
            SubPart(aParent);

        }
        public void InitializeProperties()
        {

            //eventPartEvent += OPart_eventPartEvent;
            base.Rotation = 45;
            AddProperty("Offset", 0.0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("Thickness", 10.0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("Height", 0.0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("Elevation", 0.5, aUnitType: uppUnitTypes.SmallLength);
    

        }


        #endregion  Constructors


        #region Properties

        public double EndPlateClearance => 2.0;

        public double Offset { get => PropValD("Offset"); set => PropValSet("Offset", Math.Abs(value)); }
        public new double Thickness { get => PropValD("Thickness"); set => PropValSet("Thickness", Math.Abs(value)); }
        public override double Height { get => PropValD("Height"); set => PropValSet("Height", Math.Abs(value)); }
        public double Elevation { get => PropValD("Elevation"); set => PropValSet("Elevation", Math.Abs(value)); }


        public override double Length
        {
            get
            {
                base.Length = uopUtils.ComputeBeamLength(Offset + 0.5 * Thickness, ShellID/2, true);
                return base.Length;
            }
            set => base.Length = uopUtils.ComputeBeamLength(Offset + 0.5 * Thickness, ShellID / 2, true);
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

        public override string INIPath => $"COLUMN({ColumnIndex}).RANGE({RangeIndex}).TRAYASSEMBLY.WALL";

        public override dxfPlane Plane { get => new dxfPlane(new dxfVector(X, Y), aRotation: Rotation); }



        public uopVectors Vertices
        {
            get
            {
                dxfPlane myplane = Plane;
                uopVectors _rVal = new uopVectors();
                double lng = Length / 2;
                double wd = Thickness / 2;
                _rVal.Add(myplane.Vector(-lng, wd));
                _rVal.Add(myplane.Vector(-lng, -wd));
                _rVal.Add(myplane.Vector(lng, -wd));
                _rVal.Add(myplane.Vector(lng, wd));
                return _rVal;
            }
        }
        #endregion Properties

        #region Methods
    


        /// <summary>
        /// used by an object to respond to property changes of itself and of objects below it in the object model.
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        private void Notify(uopProperty aProperty) { if (aProperty == null || aProperty.Protected || SuppressEvents) return; eventUOPDividerWallPropertyChange?.Invoke(aProperty); }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public uopDividerWall Clone() => new uopDividerWall(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override uopPropertyArray SaveProperties(string aHeading = "")
        {
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
            uopProperties _rVal = new uopProperties(base.ActiveProps, aHeading);

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
        public override void ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref colUOPDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null)
        {

            try
            {
                ioWarnings ??= new colUOPDocuments();
                if (IsVirtual)
                {
                    return;
                }
                else
                {
                    if (this.Properties.Count == 0)
                    {
                        InitializeProperties();
                    }
                }

                colUOPDocuments warnings = new colUOPDocuments();
                base.ReadProperties(aProject, aFileProps, ref warnings, aFileVersion, aFileSection, bIgnoreNotFound, aAssy, aPartParameter, aSkipList, EqualNames);
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

        /// <summary>
        /// Returns the polygon for the elevation view of the wall
        /// </summary>
     
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation( iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
         => throw new NotImplementedException();

        /// <summary>
        /// Returns the polygon for the plan view of the wall
        /// </summary>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="bSuppressHoles"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(iVector aCenter = null, double aRotation = 0, bool bShowObscured = false, bool bSuppressHoles = false, string aLayerName = "GEOMETRY", double aCenterLineLength = 0)
        => throw new NotImplementedException();

        /// <summary>
        /// Returns the polygon for the profile view of the wall
        /// </summary>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        => throw new NotImplementedException();

        public override void SubPart(uopTrayAssembly aAssy, string aCategory = null, bool? bHidden = null)
        {
            if (aAssy == null)
            {
                return;
            }

            base.SubPart(aAssy, aCategory, bHidden);
            IsVirtual = !aAssy.DesignFamily.IsDividedWallDesignFamily();
         
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
        #endregion Methods
    }
}
