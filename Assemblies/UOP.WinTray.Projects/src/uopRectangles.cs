using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
namespace UOP.WinTray.Projects
{
    public class uopRectangles : List<uopRectangle>, IEnumerable<uopRectangle>, ICloneable
    {

        #region Constructors

        public uopRectangles() {  base.Clear(); }

        internal uopRectangles(URECTANGLES aRectangles)
        {
            base.Clear();

            for (int i = 1; i <= aRectangles.Count; i++)
            { 
                Add(new uopRectangle(aRectangles.Item(i)));
            }
        }

        public uopRectangles(IEnumerable<uopRectangle> aRectangles)
        {
            base.Clear();
            if (aRectangles == null) return;
            foreach (var aRectangle in aRectangles) { if (aRectangle != null) Add(new uopRectangle(aRectangle)); }
        }
        public uopRectangles(IEnumerable<iArcRec> aRectangles)
        {
            base.Clear();
            if (aRectangles == null) return;
            foreach (var aRectangle in aRectangles) { if (aRectangle != null && aRectangle.Type == uppArcRecTypes.Rectangle) Add(new uopRectangle(aRectangle)); }
        }
        #endregion Constructors

        #region Methods
        object ICloneable.Clone() => new uopRectangles(this);

        public uopRectangles Clone() => new uopRectangles(this);

        public void Append(IEnumerable<uopRectangle> aRectangles, bool bAddClones = false, string aTag = null)
        {
            if(aRectangles == null || aRectangles.Count() ==0) return;

            if(!bAddClones && aTag == null)
            {
                base.AddRange(aRectangles);
                return;
            }

            foreach (var rec in aRectangles)
            {
                uopRectangle adder = bAddClones ? new uopRectangle(rec) : rec;
                Add(adder);
                if(aTag != null) adder.Tag = aTag;

            }
        }
        public new uopRectangle Add(uopRectangle aRectangle)
        {
            if (aRectangle != null)
            {
                base.Add(aRectangle);
                return aRectangle;
            }
            return null;
        }
        internal uopRectangle Add(URECTANGLE aRectangle) =>Add( new uopRectangle( aRectangle));

        public uopRectangle Add(iVector aCenter, double aWidth, double aHeight, string aTag = null, string aFlag = null) => Add(new uopRectangle(aCenter, aWidth, aHeight, aTag, aFlag));
        


        public new int IndexOf(uopRectangle aRectangle)
        {
            return aRectangle == null ? 0 : base.IndexOf(aRectangle) + 1;
        }

        public uopVectors Centers()
        {
            uopVectors _rVal = uopVectors.Zero;
            foreach (uopRectangle rec in this) _rVal.Add(rec.Center);
            return _rVal;
        }

        public bool ContainsVector(iVector aVector, bool bOnIsOut = false, bool bReturnJustOne = true, bool bHorizontalOnly = false) => aVector == null ? false: ContainsVector(new UVECTOR(aVector), bOnIsOut, bReturnJustOne, bHorizontalOnly);
        public bool ContainsVector(iVector aVector, out List<uopRectangle> rContainters, bool bOnIsOut = false, bool bReturnJustOne = true, bool bHorizontalOnly = false) { rContainters = new List<uopRectangle>(); return aVector == null ? false : ContainsVector(new UVECTOR(aVector), out rContainters, bOnIsOut, bReturnJustOne, bHorizontalOnly); }

        internal bool ContainsVector(UVECTOR aVector, bool bOnIsOut = false, bool bReturnJustOne = true, bool bHorizontalOnly = false) => ContainsVector(aVector, out List<uopRectangle> _, bOnIsOut, bReturnJustOne, bHorizontalOnly);

        internal bool ContainsVector(UVECTOR aVector, out List<uopRectangle> rContainters, bool bOnIsOut = false, bool bReturnJustOne = true, bool bHorizontalOnly = false)
         => uopRectangles.RectanglesContainVector(this,aVector,out rContainters, bOnIsOut, bReturnJustOne,bHorizontalOnly);
        public uopRectangle Item(int aIndex)
        {
            if (aIndex < 0 || aIndex > Count) return null;
            return base[aIndex - 1];
        }

        public List<uopRectangle> GetIntersections(uopRectangle aContainer, bool bOnIsOut = false) => uopRectangles.RectanglesWithinRectangle(this,aContainer, bOnIsOut);

        public int SetSuppressed(int? aIndex, bool aSuppressionVal) => uopRectangles.SetSuppressedValue(this, aIndex, aSuppressionVal);

        public int SetSelected(int aIndex, bool aSuppressionVal, bool bAllowMultiSelect)
        {
            if (aIndex < 0 || aIndex > Count) return 0;
        
            uopRectangle mem = base[aIndex - 1];
            return uopRectangles.SetSelectedValue(this, mem, aSuppressionVal, bAllowMultiSelect);
        }
        public int SetSelected(uopRectangle aMember, bool aSuppressionVal, bool bAllowMultiSelect)
        {
            if (aMember == null || base.IndexOf(aMember) < 0) return 0;
            return uopRectangles.SetSelectedValue(this, aMember, aSuppressionVal, bAllowMultiSelect);
        }

        #endregion Methods

        #region Shared Methods

        internal static bool RectanglesContainVector(IEnumerable<uopRectangle> aRectangles, UVECTOR aVector, out List<uopRectangle> rContainters, bool bOnIsOut = false, bool bReturnJustOne = true, bool bHorizontalOnly = false)
        {
            rContainters = new List<uopRectangle>();
            if (aRectangles == null) return false;
            foreach (uopRectangle item in aRectangles)
            {
                if (item.Contains(aVector, bOnIsOut, bHorizontalOnly))
                {
                    rContainters.Add(item);
                    if (bReturnJustOne) break;
                }
            }
            return rContainters.Count > 0;
        }

        /// <summary>
        /// returns all the rectangles that have at least one corner lying within the passed container
        /// </summary>
        /// <param name="aRectangles">the rectangles to search</param>
        /// <param name="aContainer">the container to test the subject rectangles</param>
        /// <param name="bOnIsOut">flag indicating that a corner lying on the container bounds should be considered out</param>
        /// <returns></returns>

        public static List<uopRectangle> RectanglesWithinRectangle(IEnumerable<uopRectangle> aRectangles, uopRectangle aContainer, bool bOnIsOut = false)
        {
            if (aRectangles == null || aContainer == null || aContainer.Area ==0) return new List<uopRectangle>();
            List<uopRectangle> _rVal = new List<uopRectangle>();

            double rad = Math.PI * Math.Pow(aContainer.Diagonal / 2, 2);
            foreach (uopRectangle item in aRectangles)
            {
                uopVectors corners = item.Corners;
                corners.Add(item.Center);
                if (aContainer.ContainedVectors(corners,bJustOne: true).Count > 0)
                {
                    _rVal.Add(item);
                    continue;
                }
                
            }
            return _rVal;
        }

        public static uopRectangles Copy(IEnumerable<uopRectangle> aRectangles)
        {
            if (aRectangles == null) return null;
            uopRectangles _rVal = new uopRectangles();
            foreach (uopRectangle item in aRectangles) _rVal.Add(new uopRectangle(item));
            return _rVal;
        }

        public static uopRectangles CloneCopy(uopRectangles aRectangles) => aRectangles == null ? null : new uopRectangles(aRectangles);

        public static dxeInsert BuildBlock( dxfImage aImage, string aName, List<uopRectangle> aRectangles, uopRectangle aRectangle, ref dxfBlock ioRectangleBlock,  dxfDisplaySettings aDisplaySettings = null, string aTag = null, iVector aTranslation = null, bool bAddPoint = true)
        {
            if (aRectangles == null || aRectangles.Count <= 0 || aImage == null) return null;

            aName = string.IsNullOrEmpty(aName) ? ioRectangleBlock == null ?  "RECBLOCK" : ioRectangleBlock.Name : aName.Trim() ;
             aRectangle??=  aRectangles[0];
            bool uniformrectangles = ioRectangleBlock != null || aRectangles.FindIndex(x => Math.Round(x.Height, 3) != Math.Round(aRectangle.Height, 3) && Math.Round(x.Width, 3) != Math.Round(aRectangle.Width, 3)) == -1;
      
            uopVector u0 = new uopVector(aRectangle.Center);
            aTag ??= aRectangle.Tag;
            double ang0 = aRectangle.Rotation;
            aDisplaySettings ??= new dxfDisplaySettings();
            if (ioRectangleBlock == null)
            {
                ioRectangleBlock = new dxfBlock(aName) { LayerName = aDisplaySettings.LayerName};
                if (uniformrectangles)
                {
                    uopRectangle baserec = new uopRectangle(aRectangle, uopVector.Zero) { Rotation = 0 };
                    ioRectangleBlock.Entities.Add(new dxePolyline(baserec, aDisplaySettings) { Tag = aTag });
                    if (bAddPoint) ioRectangleBlock.Entities.Add(new dxePoint(null, aDisplaySettings) { Tag = aTag });

                }

            }
          
            dxoInstances plinsts = new dxoInstances();

            //_rVal.Instances.BasePlane = new dxfPlane(u0);
            for (int i = 1; i <= aRectangles.Count; i++)
            {
                uopRectangle rec = aRectangles[i -1];
                uopVector trans = rec.Center - u0;
                if (!uniformrectangles)
                {
                    dxePolyline pl = new dxePolyline(rec, aDisplaySettings) { Tag = aTag };
                    ioRectangleBlock.Entities.Add(pl);
                    if (bAddPoint) ioRectangleBlock.Entities.Add(new dxePoint(rec.Center, aDisplaySettings) { Tag = aTag });
                }
                else
                {
                    if(i > 1)
                    {
                        double scale = rec.Area / aRectangle.Area;
                        double rot = rec.Rotation; // mzUtils.NormAng( ang0 - rec.Rotation,false,true,true);
                                                   //rot = 0;
                        plinsts.Add(trans.X, trans.Y, aScaleFactor: scale, aRotation: rot);

                    }

                }
               
            }


            ioRectangleBlock = aImage.Blocks.Add(ioRectangleBlock);
            dxeInsert _rVal = new dxeInsert(ioRectangleBlock, u0) { Instances = plinsts};
            if (aTranslation != null)
                _rVal.Translate(aTranslation);
            return _rVal;
        }

        public static int SetSuppressedValue(List<uopRectangle> aRectangles, int? aIndex, bool aSuppressionVal, bool? aMark = null)
        {
            if (aRectangles == null || aRectangles.Count == 0) return 0;
            int _rVal = 0;
            if (!aIndex.HasValue)
            {
                foreach (var mem in aRectangles) { if (mem.Suppressed != aSuppressionVal) _rVal++; mem.Suppressed = aSuppressionVal;  }
                return _rVal;
            }
            if (aIndex.Value < 0 || aIndex.Value > aRectangles.Count) return _rVal;
            if (aRectangles[aIndex.Value - 1].Suppressed != aSuppressionVal) _rVal = 1;
            aRectangles[aIndex.Value - 1].Suppressed = aSuppressionVal;
       
            return _rVal;
        }
        public static int SetSelectedValue(IEnumerable <uopRectangle> aRectangles, uopRectangle aMember,  bool aSelectedVal, bool bAllowMultiSelect = false)
        {
            if (aRectangles == null || aRectangles.Count() == 0) return 0;
            int _rVal = 0;
            foreach (var mem in aRectangles) 
            { 
                if(aMember == null)
                {
                    if (mem.Selected != aSelectedVal) _rVal++;
                    mem.Selected = aSelectedVal;
                }
                else
                {
                    if (mem == aMember)
                    {
                        if (mem.Selected != aSelectedVal) _rVal++;
                        mem.Selected = aSelectedVal;

                    }
                    else
                    {
                        if (!bAllowMultiSelect && aSelectedVal)
                        {
                            if (mem.Selected)
                            {
                                _rVal++;
                                mem.Selected = false;
                            }
                        }
                    }
                }
                
                
            }
            return _rVal;


        }

        #endregion Shared Methods
    }
}
