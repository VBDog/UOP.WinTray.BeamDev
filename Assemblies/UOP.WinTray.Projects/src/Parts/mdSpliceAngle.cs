using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// splice angle object
    /// the angles used to splice deck panels together 
    /// if they are too long to be manufactured as a single piece
    /// </summary>
    public class mdSpliceAngle : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.SpliceAngle;

        #region Constructor

        public mdSpliceAngle() : base(uppPartTypes.SpliceAngle, uppProjectFamilies.uopFamMD, "", "", true) => Initialize();
        public mdSpliceAngle(mdTrayAssembly aAssy, uopDeckSplice aSplice, mdDeckSection aSection) : base(uppPartTypes.SpliceAngle, uppProjectFamilies.uopFamMD, "", "", true) => Initialize(null, aAssy, aSplice, aSection);

        public mdSpliceAngle(uppSpliceAngleTypes aType) : base(aType.PartType(), uppProjectFamilies.uopFamMD, "", "", true) => Initialize();


        internal mdSpliceAngle(mdSpliceAngle aPartToCopy) : base(aPartToCopy == null ? uppPartTypes.SpliceAngle : aPartToCopy.PartType, uppProjectFamilies.uopFamMD, "", "", true) => Initialize(aPartToCopy);



        private void Initialize(mdSpliceAngle aPartToCopy = null, mdTrayAssembly aAssy = null, uopDeckSplice aSplice = null, mdDeckSection aSection = null)
        {

            _Instances = null;
            AngleType = uppSpliceAngleTypes.SpliceAngle;
            Height = 2;
            Width = 2.5;
            Length = 10;
            Direction = dxxRadialDirections.AwayFromCenter;

            Z = 0;
            BoltCount = 0;
            Tag = string.Empty;
            JoggleDepth = 0;
            SetCoordinates(0, 0, 0);
            if (aPartToCopy != null)
            {

                Copy(aPartToCopy);
                AngleType = aPartToCopy.AngleType;


                Direction = aPartToCopy.Direction;
                BoltCount = aPartToCopy.BoltCount;

                JoggleDepth = aPartToCopy.JoggleDepth;

                SpliceHandle = aPartToCopy.SpliceHandle;


            }

            Splice = aSplice;

            if (aSplice != null)
            {
                SpliceHandle = aSplice.Handle;
                AngleType = aSplice.SpliceType == uppSpliceTypes.SpliceWithAngle ? aSplice.SpliceAngleType : uppSpliceAngleTypes.SpliceAngle;
                if (aSplice.IsManwayCenter)
                    AngleType = uppSpliceAngleTypes.ManwaySplicePlate;
                uopFlangeLine flngl = aSplice.FlangeLine;
                Length = flngl.Length;
                BoltCount = flngl.PointCount();
                if (AngleType == uppSpliceAngleTypes.SpliceAngle)
                {
                    Direction = dxxRadialDirections.TowardsCenter;
                }
                SetCoordinates(flngl.MidPt.X, flngl.MidPt.Y);
  
            }
            if (aAssy != null)
            {
                SheetMetal = aAssy.Project?.FirstDeckMaterial;
                SubPart(aAssy);
            }
            if (aSection != null)
            {
                DeckSection = aSection;
                SubPart(aSection);
                if (aAssy == null) SheetMetal = aSection.SheetMetal;
               
                uopInstances insts = aSection.Instances;
                if (insts.Count > 0&& AngleType != uppSpliceAngleTypes.ManwaySplicePlate  )
                {
                    uopInstances myinsts = Instances;
                    uopVector basept = insts.BasePt;
                    uopVector offset = Center - basept;
                    foreach(var inst in insts)
                    {
                        uopVector newcenter = basept.Moved(inst.DX, inst.DY);
                        if (inst.Rotation == 0) newcenter.Move(offset.X, offset.Y); else newcenter.Move(offset.X, -offset.Y);
                        myinsts.Add(newcenter.X - X, newcenter.Y - Y, aRotation: inst.Rotation);
                    }
                    Instances = myinsts;
                }
            }
           
           
            Z = 0.5 * Thickness;
        }

        #endregion Constructors

        #region Properties

        private WeakReference<mdDeckSection> _SectionRef;
        public mdDeckSection DeckSection
        {
            get
            {
                if (_SectionRef == null) return null;
                if (!_SectionRef.TryGetTarget(out mdDeckSection _rVal)) _SectionRef = null;
                return _rVal;

            }

            set
            {
                if (value == null) { _SectionRef = null; return; }
                _SectionRef = new WeakReference<mdDeckSection>(value);
                
            }
        }
        private new uopInstances _Instances;
        public override uopInstances Instances { get{_Instances??= new uopInstances(); _Instances.BasePt = Center; return _Instances;} set => _Instances = value; }
        /// <summary>
        /// the angle type
        /// one of three types splice angle, manway angle or manway splice plate
        /// </summary>
        public uppSpliceAngleTypes AngleType {  get;  set; }

        /// <summary>
        /// the part type of the angle
        /// one of three types splice angle, manway angle or manway splice plate
        /// </summary>
        public override uppPartTypes PartType { get => AngleType.PartType(); internal set { if (value == uppPartTypes.SpliceAngle) { AngleType = uppSpliceAngleTypes.SpliceAngle; } if (value == uppPartTypes.ManwayAngle) { AngleType = uppSpliceAngleTypes.ManwayAngle; } if (value == uppPartTypes.ManwaySplicePlate) { AngleType = uppSpliceAngleTypes.ManwaySplicePlate; } } }
        /// <summary>
        /// the bolt welded into the angle
        /// </summary>
        public hdwHexBolt Bolt => base.SmallBolt(true, GenHoles().HoleCount(), $"{PartName } Bolt");
       
        /// <summary>
        /// the number of bolts and bolt holes in the angle
        /// </summary>
        public int BoltCount { get; set; }



        /// <summary>
        /// the hole in the angle that receives the bolts
        /// </summary>
        public uopHole BoltHole => new uopHole(mdGlobals.SpliceBoltHole(this));
        
      
        /// <summary>
        /// the distance between bolts
        /// </summary>
        public double BoltSpacing => mdUtils.SpliceBoltSpacing(BoltCount, Length);



        public override int OccuranceFactor { get => Instances.Count + 1; set {} }

        /// <summary>
        /// the depth of a joggle for manway splices
        /// </summary>
        public double JoggleDepth { get; set; }

        public double HoleSpan => !SupportsManway ? 1.125 : AngleType == uppSpliceAngleTypes.ManwaySplicePlate ? 1.125 : 0.75; 


        /// <summary>
        /// flag indicating that this angle is used to splice manways together in slot and tab design
        /// </summary>
        public bool IsManwaySplice => AngleType == uppSpliceAngleTypes.ManwaySplicePlate;



        /// <summary>
        /// the lock washer used to attach the angle between deck panels 
        /// material and size follow the bolt
        /// </summary>
        public hdwLockWasher LockWasher => Bolt.GetLockWasher();

        /// <summary>
        /// the sheet metal object property of the part
        /// </summary>
        public new uopMaterial Material
        {
            get => SheetMetal;

            set
            {
                if (value == null) return;
                if (value.MaterialType == uppMaterialTypes.SheetMetal) SheetMetal = (uopSheetMetal)value;

            }
        }

        /// <summary>
        /// the nut used to attach the angle between deck panels 
        /// material and size follow the bolt
        /// </summary>
        public hdwHexNut Nut => Bolt.GetNut();


        public dxxRadialDirections Orientation => Direction;



        private WeakReference<uopDeckSplice> _SpliceRef = null;
        /// <summary>
        /// the splice that gave rise to the angle
        /// </summary>
        public uopDeckSplice Splice
        {
            get
            {
                uopDeckSplice _rVal = null;
                if (_SpliceRef != null)
                {
                    _SpliceRef.TryGetTarget( out _rVal);

                }
                if(_rVal == null)
                {
                    if (SpliceHandle == string.Empty) return null;
                    mdTrayAssembly aAssy = GetMDTrayAssembly();
                    _rVal = aAssy?.DeckSplices.Find(x => x.Handle == SpliceHandle);
                    if(_rVal != null) _SpliceRef = new WeakReference<uopDeckSplice>(_rVal);
                }
                return _rVal;
            }

            set
            {
                _SpliceRef = value == null ? null : new WeakReference<uopDeckSplice>(value);
            }
           
        }

        /// <summary>
        /// the handle of the splice that is the parent of the angle
        /// </summary>
        public string SpliceHandle { get ; set; }

        /// <summary>
        /// the direction that the angles top leg points
        /// </summary>
        public dxxOrthoDirections FlangeDirection
        {
            get
            {
                if (Direction == dxxRadialDirections.AwayFromCenter)
                {
                    return (Math.Round(Y, 1) >= 0) ? dxxOrthoDirections.Up : dxxOrthoDirections.Down;
                }
                else
                {
                    return (Math.Round(Y, 1) < 0) ? dxxOrthoDirections.Up : dxxOrthoDirections.Down;
                }
            }
        }


        /// <summary>
        /// true if the angle supports a manway
        /// </summary>
        public bool SupportsManway  => AngleType == uppSpliceAngleTypes.ManwayAngle || AngleType == uppSpliceAngleTypes.ManwaySplicePlate;

        public override double Width { get => mdGlobals.SpliceAngleWidth; set { } }
        public hdwFlatWasher Washer => Bolt.GetWasher();

        /// <summary>
        /// the weight of the part in english pounds
        /// </summary>
        public new dynamic Weight
        {
            get
            {

                double sArea = (!IsManwaySplice) ? (Height + (Width - Thickness)) * Length : Width * 2 * Length;
                sArea -= GenHoles().TotalArea();
                return sArea * base.SheetMetalWeightMultiplier;
            }
        }

      

     

        /// <summary>
        /// the direction that the angles top leg points with respect to the shell center
        /// </summary>
        public new dxxRadialDirections Direction { get => base.RadialDirection; set => base.RadialDirection = value; }

        public double HoleInset => (Width - HoleSpan) / 2;

        #endregion Properties

        #region Methods
  

        /// <summary>
        /// ^returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdSpliceAngle Clone() => new mdSpliceAngle(this);

        public override uopPart Clone(bool aFlag = false) => this.Clone();

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(String aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }

        public override uopDocuments Drawings() => GenerateDrawings();
        
        /// <summary>
        /// the drawings that are available for this part
        /// </summary>
        public uopDocuments GenerateDrawings(string aCategory = null,uopDocuments aCollector = null, uppUnitFamilies aUnits = uppUnitFamilies.Metric)
        {
            aCategory = string.IsNullOrWhiteSpace(aCategory) ? Category : aCategory.Trim();    
            uopDocuments _rVal = aCollector ?? new uopDocuments();
               
                if (PartType == uppPartTypes.SpliceAngle)
                {
                _rVal.AddDrawing(uppDrawingFamily.Manufacturing, this, "Splice Angle", "Splice Angle", uppDrawingTypes.SpliceAngle, uppBorderSizes.BSize_Landscape,aCategory: aCategory);
                }
                else if (PartType == uppPartTypes.ManwayAngle)
                {
                _rVal.AddDrawing(uppDrawingFamily.Manufacturing, this, "Manway Angle", "Manway Angle", uppDrawingTypes.ManwayAngle, uppBorderSizes.BSize_Landscape, aCategory: aCategory);
                }
                else if (PartType == uppPartTypes.ManwaySplicePlate)
                {
                //_rVal.AddDrawing(uppDrawingFamily.Manufacturing, this, "Manway Splice Plate", "Manway Splice Plate", uppDrawingTypes.ManwaySplicePlate, uppBorderSizes.BSize_Landscape, aCategory: aCategory);
                }

                return _rVal;
         
        }

      
        /// <summary>
        /// executed internally to create the holes collection
        /// </summary>
        /// <returns></returns>
        public uopHoleArray GenHoles() => new uopHoleArray(GenHolesV());
       


        /// <summary>
        /// executed internally to create the holes collection
        /// </summary>
        /// <returns></returns>
        internal UHOLEARRAY GenHolesV(string aTag = null)
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;
            UHOLE Hole = mdGlobals.SpliceBoltHole(this);
            bool bIsPlate  = PartType == uppPartTypes.ManwaySplicePlate;
            double cY = Y;
            double cZ = Z;
            double thk = Hole.Depth;
            double swap = 1;
            
            uopDeckSplice mysplice = this.Splice;
            if (mysplice != null)
                {
                if (mysplice.IsManwayCenter)
                    AngleType = uppSpliceAngleTypes.ManwaySplicePlate;
                
                }

            double span = HoleSpan;

            if (AngleType== uppSpliceAngleTypes.ManwayAngle)
            {
                span = Width - 2* 0.875;
                if(mysplice != null)
                {
                    if (mysplice.IsTop) swap = -1;
                }
                swap = -1;
            }
                //returns Holes laid out with respect to the top center of the angle
                ULINE aLn = new ULINE(new UVECTOR(X - 0.5 * Length, cY +(swap * span / 2)), new UVECTOR(X + 0.5 * Length, cY + (swap * span / 2)));

            UHOLES aHls = mdUtils.SpliceBoltHoles(aLn, BoltCount, Hole, Hole.Elevation, UHOLES.Null, out double space);

            if (AngleType != uppSpliceAngleTypes.ManwayAngle)
            {
                aLn.ep.Y = cY - ((swap * span) / 2);
                aLn.sp.Y = aLn.ep.Y;
                Hole.Inset += HoleSpan / 2;
                double elev = bIsPlate ? Hole.Elevation - Hole.Depth : Hole.Elevation;
                aHls = mdUtils.SpliceBoltHoles(aLn, BoltCount, Hole, elev, aHls,out double space2);

            }
            
            aHls.Member = Hole;

            if (Math.Round(Y, 1) < 0)
            {
                if (RadialDirection == dxxRadialDirections.TowardsCenter) aHls.Centers.Rotate( CenterV, 180);
                
            }
            else
            {
                if (RadialDirection == dxxRadialDirections.AwayFromCenter) aHls.Centers.Rotate(CenterV, 180);
            }

            if (bIsPlate)
            {
                UHOLES bHls = new UHOLES(aHls,true);
                UHOLES cHls = new UHOLES(bHls);

                double jd = JoggleDepth;
                for (int i = 1; i <= aHls.Centers.Count; i++)
                {
                    Hole = aHls.Item(i);
                    if (Hole.Y > cY)
                    {
                        if (bHls.Centers.Count <= 0) bHls.Member.Elevation = cZ + (thk / 2 + jd / 2);
                       
                        bHls.Centers.Add(Hole.Center);
                    }
                    else
                    {
                        if (cHls.Centers.Count <= 0) cHls.Member.Elevation = cZ - (thk / 2 + jd / 2);
                        
                       cHls.Centers.Add(Hole.Center);
                    }

                   
                 
                }

                if(bHls.Count > 0) _rVal.Add(bHls, $"{Hole.Tag}_TOP");
                if (cHls.Count > 0) _rVal.Add(cHls, $"{Hole.Tag}_BOT");
            }
            else
            {
                _rVal.Add(aHls, Hole.Tag);
            }
            return _rVal;
        }

        /// <summary>
        /// returns True if the passed angle is equal to this angle
        /// </summary>
        /// <param name="aAngle"></param>
        /// <returns></returns>
        public bool IsEqual(ref mdSpliceAngle aAngle)
        {
         
            if (aAngle == null)  return false;
          
            if (aAngle.PartType != PartType) return false;
         
            if (aAngle.SupportsManway != SupportsManway) return false; 
            if (aAngle.IsManwaySplice != IsManwaySplice)  return false;
         
            if (!Material.IsEqual(aAngle.Material)) return false;
          
            if (Math.Abs(Math.Round(aAngle.Length - Length, 3)) != 0)  return false;
        
            if (aAngle.BoltCount != BoltCount)  return false; 
        
            return true;
        }

             

        public override void UpdatePartProperties()
        {
            IsCommon = true;
            DescriptiveName = PartName + " (" + Length.ToString("0.000") + ")";
        }

        public override void UpdatePartWeight() => base.Weight = Weight;
        

        /// <summary>
        ///returns a dxePolygon that is used to draw the elevation view of the angle
        /// </summary>
        /// <param name="aCenter">scale factor to apply to the returned polygon</param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        => mdPolygons.SpliceAngle_View_Elevation(this, aCenter, aRotation, aLayerName);
           
        /// <summary>
        /// ^returns a dxePolygon that is used to draw the layout view of the angle
        /// </summary>
        /// <param name="bShowHidden"></param>
        /// <param name="bSuppressHoles">flag to request the polygon be defined completely with hidden lines</param>
        /// <param name="bSuppressOrientations"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(bool bShowHidden = false, bool bSuppressHoles = false, bool bSuppressOrientations = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
         => mdPolygons.SpliceAngle_View_Plan(this, bShowHidden, bSuppressHoles, bSuppressOrientations, aCenter, aRotation, aLayerName);
            

        /// <summary>
        ///returns a dxePolygon that is used to draw the sectional view of the angle.
        /// the section passes through the center of the angle.
        /// </summary>
        /// <param name="bSuppressFillets">1the scale factor to apply to the profile polygon</param>
        /// <param name="bLongSide"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bAddHolePoints"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(bool bSuppressFillets = false, bool bLongSide = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bAddHolePoints = false, bool bAddBolt = false)
       =>  mdPolygons.SpliceAngle_View_Profile(this, bSuppressFillets, bLongSide, aCenter, aRotation, aLayerName, bAddHolePoints, bAddBolt);
          
        /// <summary>
        /// the washer used to attach the angle between deck panels 
        /// material and size follow the bolt
        /// </summary>
      

        public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null) => new uopHoleArray(GenHolesV(aTag));
        

        public override bool IsEqual(uopPart aPart)
        {
             if (aPart == null)  return false;
             if (aPart.PartType != PartType)  return false;
             return IsEqual((mdSpliceAngle)aPart);
        }

        public  bool IsEqual(mdSpliceAngle aPart)
        {
            if (aPart == null) return false;

            if (aPart.PartType != PartType) return false;
            if(aPart.BoltCount != BoltCount) return false;
            if (!TVALUES.CompareNumeric(aPart.Length, Length, 3)) return false;
            if (!TVALUES.CompareNumeric(aPart.Width, Width, 3)) return false;
            if (!TVALUES.CompareNumeric(aPart.Height, Height, 3)) return false;
            if(PartType == uppPartTypes.ManwaySplicePlate)
            {
                if (!TVALUES.CompareNumeric(aPart.JoggleDepth, JoggleDepth, 3)) return false;
            }
            return aPart.SheetMetal.IsEqual(SheetMetal);

        }

        #endregion Methods

        #region Shared Methods
        public static int DefaultBoltCount(double aLength)
        {
            if (aLength < 10)
            { return 2; }
            else if (aLength < 18)
            { return 3; }
            else if (aLength < 26)
            { return 4; }
            
            return 5;
            
        }

        #endregion
    }
}