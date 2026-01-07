using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// the end angles used to clamp down the deck panels at the ends of the downcomers
    /// </summary>
    public class mdEndAngle :  mdBoxSubPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.EndAngle;

        #region Constructors

        public mdEndAngle() : base(uppPartTypes.EndAngle) => Initialize();


        public mdEndAngle(mdDowncomerBox aBox, uppSides aSide, uppSides aEnd) : base(uppPartTypes.EndAngle)
        {
            Initialize(null, aBox: aBox);
            if(aSide != uppSides.Left && aSide != uppSides.Right)
            {
                throw new ArgumentException("mdEndAngle constructor requires a valid side (Left or Right).");
            }
            if (aEnd != uppSides.Top && aEnd != uppSides.Bottom)
            {
                throw new ArgumentException("mdEndAngle constructor requires a valid end (Top or Bottom).");
            }

            Side = aSide;
            End = aEnd;
            Direction = Side == uppSides.Left ? dxxOrthoDirections.Left : dxxOrthoDirections.Right;

            if (aBox != null)
            {
      

                Quantity = aBox.OccuranceFactor;
                if (End == uppSides.Top) // The end angle is at the top of the box
                {
                    if (aBox.IntersectionType_Top == uppIntersectionTypes.ToRing)
                    {
                       
                        if (aBox.X > 0)
                            Chamfered = Side == uppSides.Right;
                        else if (aBox.X < 0)
                            Chamfered = Side == uppSides.Left;
                        else
                            Chamfered = true; // The end angle at X = 0 is always chamfered when intersects the ring
                    }
                    else
                    {
                        Chamfered = Side == uppSides.Left; // The end angle which intersects the beam is chamfered when is on top left
                    }
                }

                if (aEnd == uppSides.Bottom) // The end angle is at the bottom of the box
                {
                    if (aBox.IntersectionType_Bot == uppIntersectionTypes.ToRing)
                    {
                        if (aBox.X > 0)
                            Chamfered = Side == uppSides.Right;
                        else if (aBox.X < 0)
                            Chamfered = Side == uppSides.Left;
                        else
                            Chamfered = true; // The end angle at X = 0 is always chamfered when intersects the ring
                    }
                    else
                    {
                        Chamfered = Side == uppSides.Right; // The end angle which intersects the beam is chamfered when is on bottom right
                    }
                }

            }
        }


        internal mdEndAngle(mdEndAngle aPartToCopy) : base(uppPartTypes.EndAngle) => Initialize(aPartToCopy);


        internal override void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox)
        {
            mdEndAngle copy = null;
            if (aPartToCopy != null && aPartToCopy.GetType() == typeof(mdEndAngle)) copy = (mdEndAngle)aPartToCopy;
            Initialize(copy, aBox);
        }

        private bool _Init;
        private void Initialize(mdEndAngle aPartToCopy = null,mdDowncomerBox aBox = null)
        {
            if (!_Init)
            {
                Side = uppSides.Right;
                SubType = 1;
                SparePercentage = 2;
                Height = 1;
                Width = 1;
                Length = 2;
                HoleSpan = 1;
                HoleInset = 1;
                RadialDirection = dxxRadialDirections.AwayFromCenter;
                Rotation = 0;
                Chamfered = false;
                ParentPartType = uppPartTypes.Downcomer;
                HoleDiameter = mdGlobals.gsBigHole;
                _Init = true;
                
            }
         
            if (aPartToCopy != null)
            {
                Copy(aPartToCopy);
                SubType = aPartToCopy.SubType;
                SparePercentage = aPartToCopy.SparePercentage;
                Height = aPartToCopy.Height;
                Width = aPartToCopy.Width;
                Length = aPartToCopy.Length;
                HoleSpan = aPartToCopy.HoleSpan;
                HoleInset = aPartToCopy.HoleInset;
                RadialDirection = aPartToCopy.RadialDirection;
                Rotation = aPartToCopy.Rotation;
                Chamfered = aPartToCopy.Chamfered;
                HoleDiameter = aPartToCopy.HoleDiameter;
                
                Instances = aPartToCopy.Instances;
                aBox ??= aPartToCopy.DowncomerBox;
              
            }

            if (aBox != null)
            {
                SubPart(aBox);
                Z = aBox.DeckThickness;
                OccuranceFactor = aBox.OccuranceFactor;
                
            }
        }

      
        #endregion Constructors

        #region Properties

        /// <summary>
        /// the diameter of the mounting holes
        /// </summary>
        public double HoleDiameter { get; set; }

        /// <summary>
        /// the distance the mounting holes are inset from then end if the angle
        /// </summary>
        public double HoleInset { get; set; }

        /// <summary>
        /// the distance between the two mount hole
        /// </summary>
        public double HoleSpan { get; set; }

       
        public hdwLockWasher LockWasher => Bolt.GetLockWasher();
        /// <summary>
        ///  the material of construction for the part
        ///follows the parent downcomers Material property
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
        /// the nut used to install the part
        /// </summary>
        public hdwHexNut Nut => Bolt.GetNut();


        /// <summary>
        ///returns the area of the top flange
        /// </summary>
        public double TopArea => Length * Width;

        /// <summary>
        /// the bolt used to install the part
        /// </summary>
        public hdwHexBolt Bolt => base.SmallBolt("End Angle Bolt", aQuantity: 2 * Quantity);

        private bool _Chamfered;
        /// <summary>
        /// true if the angle has chamfered ends
        /// </summary>

        public bool Chamfered { get => _Chamfered; set { _Chamfered = value; SubType = _Chamfered ? 2 : 1; } }

        
        /// <summary>
        /// the flat washer used to install the part
        /// </summary>
        public hdwFlatWasher Washer => Bolt.GetWasher();

        public override uopInstances Instances 
        {
            get { uopInstances _rVal = base.Instances; _rVal.PartIndex = BoxIndex; return _rVal; }
            set { base.Instances_Set( new TINSTANCES(value) { PartIndex = BoxIndex});  }
        }
        #endregion Properties

        #region Methods

        public override string ToString() { return Chamfered ? $"CHAMFERED END ANGLE {PartNumber}" : $"END ANGLE {PartNumber}"; }

        /// <summary>
        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdEndAngle Clone() => new mdEndAngle(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)Clone();
        
         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }

     

        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <returns></returns>
        public uopHoleArray GenHoles() => new uopHoleArray(GenHolesV());

        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <returns></returns>
        UHOLEARRAY GenHolesV()
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;
             UHOLES aHls = UHOLES.Null;
            double thk = Thickness;
            double x = (Direction == dxxOrthoDirections.Left) ? (X - 0.5 * thk) : (X + 0.5 * thk);
            double y = Y + 0.5 * Length - HoleInset;
            //end angle holes
            aHls.Member = new UHOLE(HoleDiameter,x,y,aDepth:thk,aElevation: Z + 0.625, aZDirection:"1,0,0");
            aHls.Centers.Add(aHls.Member.Center);
            y = Y - 0.5 * Length + HoleInset;
            aHls.Centers.Add(new UVECTOR(x,y,aElevation:Z + 0.625));
            _rVal.Add(aHls, "MOUNT");

            return _rVal;
        }

       
        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;
            return IsEqual((mdEndAngle)aPart);

        }

        public bool IsEqual(mdEndAngle aAngle, bool bSuppressMaterialCheck = false, double aLengthAllowance = 0.03125)
        {
         
            if (aAngle == null) return false;
            if (aAngle.Chamfered != Chamfered) return false;
            if (Math.Abs(Math.Round(aAngle.Length - Length, 4)) > Math.Abs(aLengthAllowance)) return false;

            if (Math.Abs(Math.Round(aAngle.Length - 2 * aAngle.HoleInset - (Length - 2 * HoleInset), 4)) > Math.Abs(aLengthAllowance)) return false;
            

            //    If Abs(Round(aAngle.HoleInset - HoleInset, 4)) > Abs(aLengthAllowance) Then Exit Function
             return (!bSuppressMaterialCheck) || Material.IsEqual(aAngle.Material);
     
        }



        public override void UpdatePartProperties()
        {
            Quantity = 2 * OccuranceFactor;
            DescriptiveName = "End Angle (" + string.Format("{0:0.000}", Length.ToString()) + ")";
            if (Chamfered) DescriptiveName = "Chamfered " + DescriptiveName;
            
        }

        public override void UpdatePartWeight()=> base.Weight = Weight();
 

        /// <summary>
        /// //^returns a dxePolygon that is used to draw the end view of the shelf angle
        /// </summary>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(bool bSuppressFillets = false, iVector aCenter = null, double aRotation = 0)
         => mdPolygons.EndAngle_View_Elevation(this, bSuppressFillets, aCenter, aRotation);
        
        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the angle
        /// </summary>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="bSuppressHoles"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(iVector aCenter = null, double aRotation = 0, bool bSuppressHoles = false)
        => mdPolygons.EndAngle_View_Plan(this, aCenter, aRotation, bSuppressHoles);

        /// <summary>
        ///returns a dxePolygon that is used to draw the profile view of the end angle
        /// </summary>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(iVector aCenter = null, double aRotation = 0)
        => mdPolygons.EndAngle_View_Profile(this, aCenter, aRotation);
        
           /// <summary>
        ///returns the weight of the part in english pounds
        /// </summary>
        /// <returns></returns>
        public new double Weight()
        {
                double thk = Thickness;
                //bottom area
                double sArea = Width * Length;
                sArea += (Width - thk) * Length;
                if (Chamfered) sArea -= Math.Pow(Width - thk, 2);
                sArea -= GenHoles().TotalArea();
            return  sArea * base.SheetMetalWeightMultiplier;
        }

       
        public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null) => new uopHoleArray(GenHolesV());

        #endregion Methods
    }
}
