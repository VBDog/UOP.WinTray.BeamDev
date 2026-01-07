
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    /// <summary>
    /// The uopHoles class
    /// </summary>
    public class uopHoles : ICloneable
    {
        //    public double Rotation;
        //    public bool Invalid;
        //    public string Name;
        //    public int Index;
        //    public UHOLE Member;
        //    public UVECTORS Centers;


        #region Constructors
      
        public uopHoles()  => Init();
        public uopHoles(string aName) => Init(aName: aName);

        public uopHoles(string aName, uopHole aBaseMember) => Init(aName: aName, aBaseMember: aBaseMember);

        internal uopHoles(UHOLES aStructure) => Init(aStructure);
        public uopHoles(uopHoles aHoles,bool bDontCopyMembers = false) => Init(null, aHoles,bDontCopyMembers );

        private void Init(UHOLES? aStructure = null, uopHoles aHoles = null, bool bDontCopyMembers = false, string aName = null, uopHole aBaseMember = null)
        {
            Rotation = 0;
            Invalid = false;
            _Member =  uopHole.Null;
            _Centers = uopVectors.Zero;
            Name = string.Empty;
            Index = 0;

            if (aStructure.HasValue)
            {
                _Member = new uopHole(aStructure.Value.Member);
                if(!bDontCopyMembers)_Centers = new uopVectors(aStructure.Value.Centers);
                Name = aStructure.Value.Name;
                Invalid = aStructure.Value.Invalid;
                Index = aStructure.Value.Index;
                Rotation = aStructure.Value.Rotation;
            }

            if(aHoles != null)
            {
                _Member = new uopHole(aHoles.Member);
                if (!bDontCopyMembers) _Centers = new uopVectors(aHoles.Centers);
                Name = aHoles.Name;
                Invalid = aHoles.Invalid;
                Index = aHoles.Index;
                Rotation = aHoles.Rotation;

            }

            if (aBaseMember != null) _Member = aBaseMember;
            if (aName != null) Name = aName;

        }
        #endregion Constructors

        #region Properties
        public List<uopHole> ToList
        {
            get
            {
                List<uopHole> _rVal = new List<uopHole>();

                for (int i = 1; i <= Count; i++)
                {
                    _rVal.Add(Item(i));
                }

                return _rVal;
            }
        }

        private uopVectors _Centers;
        public uopVectors Centers { get { _Centers ??= uopVectors.Zero; return _Centers; }  set { _Centers ??= uopVectors.Zero; if (value == null) _Centers.Clear(); else _Centers = value; }  }

        internal URECTANGLE BoundaryRectangle
        {
            get
            {
                    URECTANGLE _rVal = URECTANGLE.Null;
                    
                    int j = 0;
                    for (int i = 1; i <= Count; i++)
                    {
                    uopHole aHl = Item(i);
                    if (!aHl.Suppressed)
                        {
                            j += 1;
                            if (j == 1)
                              _rVal = aHl.BoundaryRectangle; 
                            else
                              _rVal.ExpandTo(aHl.BoundaryRectangle); 
                        }
                    }
                    return _rVal;
                }
            } 
        
        public double Elevation {  get => Member == null ? 0 : Member.Elevation.HasValue ? Member.Elevation.Value : 0; } 

        public int Index { get; set; }

        public string ExtrusionDirection { get => Member.ExtrusionDirection; set => Member.ExtrusionDirection = value; }

        public double Diameter { get => Member.Diameter; set => Member.Diameter = value; }

        public string Name { get; set; }


        public double Radius { get => Member.Radius; set => Member.Radius = value; }

        public double Length { get => Member.Length; set => Member.Length = Math.Abs(value); }


        public double MinorRadius { get => Member.MinorRadius; set => Member.MinorRadius = value; }

        public double Depth { get => Member.Depth; set => Member.Depth = value; }

        public bool WeldedBolt { get => Member.WeldedBolt; set => Member.WeldedBolt = value; }

        public bool IsSquare { get => Member.IsSquare; set => Member.IsSquare = value; }
 
        public dxfDirection ZDirection { get => new dxfDirection(ExtrusionDirection); set => ExtrusionDirection = (value == null) ? "0,0,1" : $"{value.X},{value.Y}{value.Z}"; }

        public double Rotation { get; set; }
        public bool Invalid { get; set; }

        private uopHole _Member;
        public uopHole Member { get { _Member ??= uopHole.Null; return _Member; } set { _Member = value == null ? uopHole.Null : value; } }
        /// <summary>
        /// returns counts
        /// </summary>
        public int Count => Centers.Count;
        #endregion Properties

        #region Methods


        public colDXFVectors CentersDXF(bool bReturnWithElevation = true) {  double? zVal = null; if (!bReturnWithElevation ) zVal = 0; return Centers.ToDXFVectors(aZ: zVal);  }

        public uopHoles Clone(bool bReturnEmpty = false) => new uopHoles(this, bReturnEmpty);
            

        object ICloneable.Clone() => new uopHoles( this);

        public void Clear() => Centers.Clear();

        public double Area(string aNamesList = "")
        {
            double _rVal = 0;
            bool testit = !string.IsNullOrEmpty(aNamesList);

            for (int i = 1; i <= Count; i++)
            {
                uopHole mem = Item(i);
                if (testit)
                {
                    if (mzUtils.ListContains(mem.Name, aNamesList))
                        _rVal += mem.Area;

                }
                else { _rVal += mem.Area; }
            }

            return _rVal;
        }


        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"uopHole[{Count}]" : $"uopHoles[{Name}] [{Count}]";
        /// <summary>
        /// method for adding x,y points
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aLength"></param>
        /// <param name="aFlag"></param>
        /// <param name="bSuppressed"></param>
        public void Add(double aX, double aY, double aLength=0, string aFlag="", bool bSuppressed=false) => Centers.Add( aX, aY,aValue:aLength,aTag:aFlag,bSuppressed:bSuppressed);

        public void Add(uopHole aHole) { if (aHole == null) return; Centers.Add(aHole.X, aHole.Y, aValue: aHole.Length, aTag: aHole.Flag, bSuppressed: aHole.Suppressed); }

        public void AppendMirrors(double? aX, double? aY)
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return;
            Centers.AppendMirrors(aX, aY);
        }

        public colDXFEntities ToDXFEntities(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0, dxfImage aImage = null, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined, bool bSuppressInstances = false,  bool bSuppressCenterPoints = false)
        {
          
            colDXFEntities _rVal = new colDXFEntities();
            if (Count <= 0) return _rVal;


            uopHole.GetData(Member, out double rad, out double mrad, out double lng);
            uopHole aHl = Item(1);
            uopVector u1 = new uopVector(aHl.Center);

            colDXFEntities aEnts = aHl.ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressCenterPoints);
            dxfRectangle aRec;
        
            aRec = aHl.BoundaryRectangle.ToDXFRectangle();
            dxfPlane aPln = aRec.Plane;
            uopVectors oSets = new uopVectors();
            List<uopVector> ctrs = Centers.FindAll(x => !x.Suppressed);
            // get the center offsets ffrom the first 
            for (int i = 1; i <= ctrs.Count; i++)
            {
                uopVector u2 = ctrs[i -1];
                double f1 = 0;
                double slng = u2.Value;
                if (slng > 2 * rad)
                {
                    if (Math.Round(slng, 6) != lng)
                    { f1 = (slng - lng) / 2; bSuppressInstances = true; }
                }
                oSets.Add(u2 - u1);
             
            }


            if (bSuppressInstances)
            {
                for (int i = 1; i <= Centers.Count; i++)
                {
                    aHl = Item(i);
                    aEnts = aHl.ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressCenterPoints);
                    _rVal.Append(aEnts);
                }
            }
            else
            {
                for (int i = 2; i <= oSets.Count; i++)
                {
                    u1 = oSets.Item(i);
                    for (int j = 1; j <= aEnts.Count; j++)
                    {
                        dxfEntity aEnt = aEnts.Item(j);
                        aEnt.Instances.Add(u1.X, u1.Y);
                    }
                }
                _rVal.Append(aEnts);


            }
            _rVal.UpdatePaths();
            return _rVal;
        }

        public int GetCount(bool aSuppressedVal) =>  Centers.FindAll(x => x.Suppressed == aSuppressedVal).Count;
         

        /// <summary>
        /// returns dxfRectangle
        /// </summary>
        /// <returns></returns>
        public uopRectangle BoundingRectangle(dxfDisplaySettings aDisplaySettings = null) => new uopRectangle(BoundaryRectangle);


        /// <summary>
        /// method for draw to image
        /// </summary>
        /// <param name="aImage"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aColor"></param>
        /// <param name="aLinetype"></param>
        /// <param name="aLTLSetting"></param>
        /// <param name="bSuppressInstances"></param>
        /// <param name="aHClineScale"></param>
        /// <param name="aVClineScale"></param>
        /// <param name="aDClineScale"></param>
        /// <returns></returns>
        public colDXFEntities DrawToImage(dxfImage aImage, string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined, bool bSuppressInstances = false, double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0)
        {
            colDXFEntities _rVal = new colDXFEntities();

            colDXFEntities aEnts = ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressInstances);
            if (aImage == null || aEnts.Count <= 0) return aEnts;


            dxfEntity aEnt = null;
            double dX = aImage.X;
            double dY = aImage.Y;
            bool bTrans = false;
            bTrans = dX != 0 || dY != 0;
            for (int i = 1; i <= aEnts.Count; i++)
            {
                aEnt = aEnts.Item(i);
                if (bTrans) aEnt.Move(dX, dY);

                _rVal.Add(aImage.Entities.Add(aEnt));
            }
            return _rVal;
        }



        /// <summary>
        /// return collection of dxf entities
        /// </summary>
        /// <param name="aLayerName"></param>
        /// <param name="aColor"></param>
        /// <param name="aLinetype"></param>
        /// <param name="aHClineScale"></param>
        /// <param name="aVClineScale"></param>
        /// <param name="aDClineScale"></param>
        /// <param name="aImage"></param>
        /// <param name="aLTLSetting"></param>
        /// <param name="rMultiLength"></param>
        /// <param name="bSuppressInstances"></param>
        /// <returns></returns>
        public colDXFEntities Entities(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0, dxfImage aImage = null, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined,  bool bSuppressInstances = true, bool bSuppressCenterPoints = false)
        => ToDXFEntities( aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressInstances, bSuppressCenterPoints);

        /// <summary>
        /// get x and y coordinate
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aPrecis"></param>
        /// <param name="bRemove"></param>
        /// <param name="bJustOne"></param>
        /// <returns></returns>
        public uopHoles GetAtCoordinate(dynamic aX = null, dynamic aY = null, int aPrecis = 3, bool bRemove = false, bool bJustOne = false)
        {
            uopHoles _rVal = Clone(true);
            _rVal.Centers = Centers.GetAtCoordinate(aX, aY, aPrecis, bRemove, bJustOne);
            return _rVal;
        }


        /// <summary>
        /// get flagged member
        /// </summary>
        /// <param name="aFlag"></param>
        /// <param name="rIndex"></param>
        /// <param name="bRemove"></param>
        /// <returns></returns>
        public uopHole GetFlagged(string aFlag, bool bRemove = false) => GetFlagged(aFlag,out int IXD, bRemove);
        

        /// <summary>
        /// get flagged member
        /// </summary>
        /// <param name="aFlag"></param>
        /// <param name="rIndex"></param>
        /// <param name="bRemove"></param>
        /// <returns></returns>
        public uopHole GetFlagged(string aFlag, out int rIndex, bool bRemove = false, string aTag = null)
        {

            uopHole _rVal = uopHole.Null;
            rIndex = 0;
            
            for (int i = 1; i <= Count; i++)
            {
                uopHole mem = Item(i);
                if (string.Compare(mem.Flag, aFlag, true) == 0)
                {
                    if (aTag != null)
                    {
                        if (string.Compare(mem.Tag, aTag, true) != 0) continue;
                    }

                    rIndex = i;
                    _rVal = mem;
                    break;
                }
            }

            if (bRemove && rIndex > 0) { Remove(rIndex); }
            return _rVal;

        }

        public uopHole Remove(int aIndex)
        {
            if(aIndex < 0 || aIndex >= Count)  return null;
            uopHole _rVal = Item(aIndex);
            Centers.Remove(aIndex);
            return _rVal;
        }


        /// <summary>
        /// returns item from provided index
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopHole Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) return null;

            uopHole _rVal = new uopHole(Member) { Index = 0 };
               _rVal.Index = aIndex;
            uopVector ctr = Centers.Item(aIndex);

            if (ctr.Value > Member.Diameter)
            {
                if (Math.Round(ctr.Value - Member.Diameter, 4) > 0) { _rVal.Length = ctr.Value; }
            }
            _rVal.Flag = ctr.Tag;
            _rVal.Rotation = Rotation;
            _rVal.Elevation =  mzUtils.VarToDouble(ctr.Elevation, aDefault: Elevation);
            _rVal.Center = ctr;

            return _rVal;
        }

        internal UHOLE ItemV(int aIndex) =>  new UHOLE(Item(aIndex));

        /// <summary>
        ///  //#1the subject holes
        ///#2flag indicating what type of vector to search for
        ///#3the ordinate to search for if the search is ordinate specific
        ///#4a precision for numerical comparison (1 to 8)
        ///^returns a hole from the collection whose properties or position in the collection match the passed control flag
        ///search for vectors at extremes
        ///returns the first one that satisfies
        /// </summary>
        /// <param name="aFilter"></param>
        /// <param name="aOrdinate"></param>
        /// <param name="aPrecis"></param>
        /// <param name="rIndex"></param>
        /// <param name="bRemove_UNUSED"></param>
        /// <returns></returns>
        public uopHole GetByPoint(dxxPointFilters aFilter, double aOrdinate = 0, int aPrecis = 3, bool bRemove = false)
        {
            uopVector u1 = Centers.GetVector(aFilter, aOrdinate, aPrecis);
            if (u1 == null) return null;
            int idx = Centers.IndexOf(u1);
            uopHole _rVal = Item(idx);
            if (bRemove) Centers.Remove(u1);
            return _rVal;
        }


        /// <summary>
        /// get last item
        /// </summary>
        /// <returns></returns>
        public uopHole Last() => (Count <= 0) ? null : Item(Count);


        /// <summary>
        /// returns nearest members
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        public uopHole Nearest(double aX, double aY) => Nearest(aX, aY, out _);

        public uopHole Nearest(double aX, double aY, out int rIndex)

        {
            rIndex = 0;
            uopHole _rVal = null;
            Centers.Nearest(new uopVector(aX, aY), out rIndex);
            if (rIndex >= 0) { _rVal = Item(rIndex); }
            return _rVal;
        }


        /// <summary>
        /// method to rotate
        /// </summary>
        /// <param name="aAngle"></param>
        /// <param name="aCenter"></param>
        public void Rotate(double aAngle, uopVector aCenter) => Centers.Rotate(aCenter, aAngle, false);

        /// <summary>
        /// method for setting properties
        /// </summary>
        /// <param name="aRadius"></param>
        /// <param name="aLength"></param>
        /// <param name="aDepth"></param>
        /// <param name="aRotation"></param>
        /// <param name="aTag"></param>
        /// <param name="aExtrusionDirection"></param>
        /// <param name="aMinorRadius"></param>
        /// <param name="bIsSquare"></param>
        /// <param name="bWeldedBolt"></param>
        public void SetProperties(double? aRadius = null, double? aLength = null, double? aDepth = null, double? aRotation = null, string aTag = null, string aExtrusionDirection = null, double? aMinorRadius = null, bool? bIsSquare = null, bool? bWeldedBolt = null)
        {
            Member.SetProps( aRadius, aLength, aDepth, aRotation, aTag, aExtrusionDirection, aMinorRadius, bIsSquare, bWeldedBolt);

        }

        #endregion Methods

        #region Shared Methods
        public static uopHoles Zero => new uopHoles();
        #endregion Shared Methods
    }
}