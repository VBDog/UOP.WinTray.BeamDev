using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// represents a cross brace of an md downcomer assembly
    /// </summary>
    public class mdCrossBrace : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.CrossBrace;

        private TANGLEIRON tStruc;

        #region Constructors
        public mdCrossBrace() :base(uppPartTypes.CrossBrace, uppProjectFamilies.uopFamMD, "","",true)
        {
           
            tStruc = new TANGLEIRON(1,1);
            Quantity = 2;
            SparePercentage = 2;
        }

        internal mdCrossBrace(TANGLEIRON aStructure, uopPart aPartToCopy) : base(uppPartTypes.CrossBrace, uppProjectFamilies.uopFamMD, "", "", true)
        {
            tStruc = aStructure;
            if (aPartToCopy == null) return;
            if (aPartToCopy.PartType == PartType) base.Copy(aPartToCopy);
        }
        #endregion

        internal TANGLEIRON Structure { get => tStruc; set => tStruc = value; }


        /// <summary>
        /// the bolt used to attach the cross brace
        /// </summary>

        public hdwHexBolt Bolt { get => base.SmallBolt("Cross Brace Bolt", "Cross Brace Attachment"); }
     
        /// <summary>
        /// the hole in the angle that receives the bolts
        /// </summary>
        public uopHole BoltHole => new uopHole(BoltHoleV);
       
        /// <summary>
        /// the hole in the angle that receives the bolts
        /// </summary>
        internal UHOLE BoltHoleV => new UHOLE(aDiameter: mdGlobals.gsBigHole, aX:X, aY:Y, aTag: "BOLT", aDepth: Thickness, aElevation: -0.5 * Thickness, aInset: 0.625);

       

        /// <summary>
        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdCrossBrace Clone() => new mdCrossBrace(tStruc.Clone(""), this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }



        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public uopHoleArray GenHoles(mdTrayAssembly aAssy) => new uopHoleArray(GenHolesV(aAssy));

        public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null) 
        => GenHoles(aAssy != null && aAssy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)aAssy : null);
        
        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aX"></param>
        /// <returns></returns>
        UHOLEARRAY GenHolesV(mdTrayAssembly aAssy, dynamic aX = null)
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;
            UHOLE Hole = BoltHoleV;
            UHOLES Hls = UHOLES.Null;
             mdDowncomer aDC = null;
            
            bool bTestX = aX != null;
            double tX = (!bTestX)? 0: mzUtils.VarToDouble(aX, aPrecis: 2);
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;
            colMDDowncomers DCs = aAssy.Downcomers;


            Hole.Y = (Y > 0)? Y - 0.125 : Y + 0.125;
            
            //returns Holes laid out with respect to the top center of the angle
            if (aAssy.OddDowncomers)
            { aDC = DCs.Item(DCs.Count - 1); }
            else
            { aDC = DCs.LastMember(); }
            Hole.X = -aDC.X;
            Hls.Member = Hole;
            if (!bTestX)
            { Hls.Centers.Add(Hole.Center); }
            else
            {
                if (Math.Round(Hole.X, 2) == tX) Hls.Centers.Add(Hole.Center);
                
            }

            for (int i = DCs.Count; i >= 1; i--)
            {
                aDC = DCs.Item(i);
                Hole.X = aDC.X;
                if (!bTestX)
                {
                    Hls.Centers.Add(Hole.Center);
                }
                else
                {
                    if (Math.Round(Hole.X, 2) == tX)
                    {
                        Hls.Centers.Add(Hole.Center);
                    }
                }
            }
            _rVal.Add(Hls, "BOLT");
            return _rVal;
        }

       
        /// <summary>
        /// the height of the shelf
        /// default =1
        /// </summary>
        public override double Height { get => tStruc.Height; set => tStruc.Height = Math.Abs(value); }
           
       
        /// <summary>
        /// the length the cross brace
        /// </summary>
        public override double Length { get => tStruc.Length; set => tStruc.Length = Math.Abs(value); }

        /// <summary>
        /// the lnut used to attach the angle between downcomers
        /// material and size follow the bolt
        /// </summary>
        public hdwHexNut Nut => Bolt.GetNut();
       
        /// <summary>
        ///returns the area of the top flange
        /// </summary>
        public double TopArea => Length * Width;


        public override void UpdatePartProperties()
        {
            DescriptiveName = "Cross Brace (" + string.Format(Math.Round(Length, 3).ToString(), "0.000") + ")";
            PartNumber = RangeIndex + "70";
        }

        public override void UpdatePartWeight() => base.Weight = Weight;
   

        /// <summary>
        ///returns a dxePolygon that is used to draw the end view of the cross brace
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(mdTrayAssembly aAssy, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
         => throw new NotImplementedException();
  

        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the cross brace
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(mdTrayAssembly aAssy, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
         => mdPolygons.md_CrossBrace_View_Profile(this, aAssy, aCenter: aCenter, aRotation: aRotation, aLayerName: aLayerName);

        /// <summary>
        ///returns a dxePolygon that is used to draw the end view of the cross brace
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bLongSide"></param>
        /// <param name="aSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Profile()
        {
            //mdTrayAssembly aAssy, bool bLongSide = false, bool aSuppressFillets = false, iVector aCenter = null, double aRotatio = 0, string aLayerName = "GEOMETRY"
             throw new NotImplementedException();
        }
        

        /// <summary>
        /// the washer used to attach the angle between downcomers
        /// material and size follow the bolt
        /// </summary>
        public hdwFlatWasher Washer => Bolt.GetWasher();

        /// <summary>
        ///returns the weight of the part in english pounds
        /// </summary>
        public override double Weight => (Height + (Width - Thickness)) * Length * Material.WeightMultiplier;
        
        /// <summary>
        /// the top flange width of the angle
        /// default =1
        /// </summary>
        public override double Width { get => tStruc.Width; set => tStruc.Width = Math.Abs(value); }

        /// <summary>
        /// the X coordinate of the center of the part
        /// </summary>
        public override double X { get => tStruc.Center.X; set => tStruc.Center.X = value; }

        /// <summary>
        /// the Y coordinate of the center of the part
        /// </summary>
        public override double Y { get => tStruc.Center.Y; set => tStruc.Center.Y = value; }
       
        /// <summary>
        /// the Z coordinate of the center of the part
        /// </summary>
        public override double Z { get => tStruc.Z; set => tStruc.Z = value; }
        
      
        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;
            mdCrossBrace thePart = (mdCrossBrace)aPart;
            if (thePart.Width != Width || thePart.Height != Height || thePart.Length != Length) return false;
            return thePart.Material.IsEqual(Material);
        }
     

        
        
        

        

       
    }
}
