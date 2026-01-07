using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class uopArcRecs : List<iArcRec>, IEnumerable<iArcRec>, ICloneable
    {

        #region Constructors

        public uopArcRecs() { }

        public uopArcRecs(IEnumerable<iArcRec> aArcRecs, bool bCloneMembers = true) 
        {
            if(aArcRecs != null)
            {
                if (bCloneMembers)
                {
                    foreach(var mem in aArcRecs) base.Add(mem.Clone());
                }
                else
                {
                    base.AddRange(aArcRecs);
                }
            }
        }

        #endregion Constructors

        #region Properties

        public uopRectangles Rectangles(bool bGetClones = false)
        {
            uopRectangles _rVal = new uopRectangles();
            List<iArcRec> rectangles = FindAll(x => x.Type == uppArcRecTypes.Rectangle);
            foreach (var mem in rectangles)
            {
                if (mem is uopRectangle)
                    _rVal.Add(!bGetClones ? (uopRectangle)mem : new uopRectangle((uopRectangle)mem) );
                else
                _rVal.Add(mem.Rectangle);
            }
            return _rVal;

        }

        public List<uopArc> Arcs(bool bGetClones = false)
        {
            List<uopArc> _rVal = new List<uopArc>();
            List<iArcRec> arcs = FindAll(x => x.Type == Enums.uppArcRecTypes.Arc);
            foreach (var mem in arcs) _rVal.Add(!bGetClones ? mem.Arc : new uopArc(mem.Arc));
            return _rVal;

        }
        #endregion Properties

        #region Methods

        public uopArcRecs Clone() => new uopArcRecs(this);
        object ICloneable.Clone() => new uopArcRecs(this);
        public colDXFEntities ToDXFEntities(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLineType = "")
        {
            colDXFEntities _rVal = new colDXFEntities();
            if(Count == 0) return _rVal;
            dxfDisplaySettings dsp = dxfDisplaySettings.Null(aLayerName,aColor,aLineType);
            foreach(iArcRec arcrec in this)
            {
                if (arcrec == null) continue;
                if (arcrec.Type == Enums.uppArcRecTypes.Arc)
                {
                    uopArc arc = arcrec.Arc;
                    if (arc == null) continue;
                    _rVal.Add( new dxeArc( arc, aDisplaySettings: dsp));
                }
                else
                {
                    uopRectangle rec = arcrec.Rectangle;
                    if (rec == null) continue;
                    _rVal.AddShape(rec, aDisplaySettings: dsp);
                }
            }
      
            return _rVal;
        }
        public uopArcRecs GetContainers(iVector aVector, bool bOnIsIn = true, int aPrecis = 5, bool bGetArcs = true, bool bGetSlots= true, bool bGetRectangles = true, bool bJustOne = true)
        {
            if (aVector == null || Count <= 0 || (!bGetArcs && !bGetRectangles && !bGetSlots)) return uopArcRecs.Null;
            
            List<iArcRec> closebys = FindAll(x => x.Center.DistanceTo(aVector, aPrecis) <= 1.05 * x.Radius);
            if (closebys.Count == 0) return uopArcRecs.Null;
            uopArcRecs _rVal = uopArcRecs.Null;
            foreach (var ac in closebys)
            {
                if(!bGetArcs && ac.Type == uppArcRecTypes.Arc) continue;
                if (!bGetRectangles && ac.Type == Enums.uppArcRecTypes.Rectangle) continue;
                if (!bGetSlots && ac.Type == Enums.uppArcRecTypes.Slot) continue;

                if (ac.ContainsVector(aVector, bOnIsIn, aPrecis))
                {
                    _rVal.Add(ac);
                    if (bJustOne) break;
                }
                    
            }
            return _rVal;
        }
        public uopArcRecs GetContainers(iArcRec aArcRec, bool bOnIsIn = true, int aPrecis = 4, bool bGetArcs = true, bool bGetRectangles = true, bool bGetSlots = true, bool bMustBeCompletelyWithin = false, bool bJustOne = true, bool bReturnTrueByCenter = false)
        {
            if (aArcRec == null || Count <= 0 || (!bGetArcs && !bGetRectangles && !bGetSlots)) return uopArcRecs.Null;

            List<iArcRec> closebys = FindAll(x => x.Center.DistanceTo(aArcRec.Center, aPrecis) <= 1.05 * (aArcRec.Radius + x.Radius));
            if (closebys.Count == 0) return uopArcRecs.Null;
            uopArcRecs _rVal = uopArcRecs.Null;
            foreach (var ac in closebys)
            {
                if (!bGetArcs && ac.Type == uppArcRecTypes.Arc) continue;
                if (!bGetRectangles && ac.Type == uppArcRecTypes.Rectangle) continue;
                if (!bGetSlots && ac.Type == uppArcRecTypes.Slot) continue;

                if (ac.Contains(aArcRec, bOnIsIn, aPrecis, bMustBeCompletelyWithin, bReturnTrueByCenter))
                {
                    _rVal.Add(ac);
                    if (bJustOne) break;
                }
            }
            return _rVal;
        }

        public int GetIndex(iArcRec aArcRec) => base.IndexOf(aArcRec) + 1;

        
        #endregion Methods

        #region Shared Methods

        public static uopArcRecs Null => new uopArcRecs();
        public static uopArcRecs CloneCopy(uopArcRecs aArcRecs) => aArcRecs == null ? null : new uopArcRecs(aArcRecs);


        public static bool ArcRecContainsArcRec(iArcRec aArcRec, iArcRec bArcRec, out bool rCoincindent, out bool rIntersects, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false)
        {
            rCoincindent = false;
            rIntersects = false;
            if (aArcRec == null || bArcRec == null) return false;

            bool _rVal = false;
            // =========  ARC TO ARC
            if (aArcRec.Type == uppArcRecTypes.Arc && bArcRec.Type == uppArcRecTypes.Arc)
            {
                uopArc arc1 = aArcRec.Arc;
                uopArc arc2 = bArcRec.Arc;
                if (arc1 == null || arc1 == null) return false;
                aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15, 4);

                double rad1 = arc1.Radius;
                double rad2 = arc2.Radius;
                mzUtils.SortTwoValues(false, ref rad1, ref rad2);

                double d1 = Math.Sqrt(Math.Pow(arc1.X - arc2.X, 2) + Math.Pow(arc1.Y - arc2.Y, 2));

                if (Math.Round( d1, aPrecis) > Math.Round(rad1 + rad2, aPrecis)) return false; // to far away
                if (bReturnTrueByCenter && (arc1.ContainsVector(arc2.Center, bOnIsIn, aPrecis, true) || arc2.ContainsVector(arc1.Center, bOnIsIn, aPrecis, true)))
                    return true;

                if (Math.Round(d1, aPrecis) == 0 && Math.Round(rad1, aPrecis) > Math.Round(rad2, aPrecis) ) return true;  // the small one is inside the big one

                    if (Math.Round(rad1 - rad2, aPrecis) == 0) //equal radius
                {
                    rCoincindent = Math.Round(d1, aPrecis) == 0;
                    rIntersects = !rCoincindent;
                }
                else // one big one small 
                {
                    double d2 = Math.Round((d1 + rad2) - rad1, aPrecis);
                    rIntersects = d2 >= 0;
                }
                _rVal = bMustBeCompletelyWithin ? !rIntersects : true;
            }
            else if ((aArcRec.Type == uppArcRecTypes.Arc && bArcRec.Type == uppArcRecTypes.Rectangle) || (aArcRec.Type == uppArcRecTypes.Rectangle && bArcRec.Type == uppArcRecTypes.Arc))   // =========  ARC TO RECTANGLE
            {
                bool arcisfirst = (aArcRec.Type == uppArcRecTypes.Arc );

                uopArc arc = arcisfirst ? aArcRec.Arc : bArcRec.Arc;
                uopRectangle rec = !arcisfirst ? aArcRec.Rectangle : bArcRec.Rectangle;
                if (arc == null || rec == null) return false;
                aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15, 4);
                double d1 = Math.Sqrt(Math.Pow(arc.X - rec.X, 2) + Math.Pow(arc.Y - rec.Y, 2));
                double rad = arc.Radius;
                double diag = 0.5 * rec.Diagonal;
                double d2 = rad + diag;
                if (Math.Round(d1, aPrecis) > Math.Round(d2, aPrecis)) return false; // to far away

                if (Math.Round(d1, aPrecis) == 0 && Math.Round(rad, aPrecis) > Math.Round(diag, aPrecis)) return true;  // the rectangle is inside the arc
                if (bReturnTrueByCenter && (arc.ContainsVector(rec.Center, bOnIsIn, aPrecis, true) || rec.ContainsVector(arc.Center, bOnIsIn, aPrecis, true)))
                    return true;

                //check the corners
                uopVectors corners = rec.Corners;

                List<uopVector> interiors = new List<uopVector>();


                foreach (var corner in corners)
                {
                    if( arc.ContainsVector(corner, bOnIsIn: true, aPrecis: aPrecis, out bool within, out bool ison, out _, out _, bTreatAsInfinite: true))
                    {
                        interiors.Add(corner);
                        if (!bMustBeCompletelyWithin) break;
                    }
                }
                _rVal = bMustBeCompletelyWithin ? interiors.Count == 4 : interiors.Count > 0;
                if (interiors.Count == 0)
                {
                    uopLines edges = corners.LineSegments(true);
                    interiors = edges.Intersections(arc, false, true);
                    rIntersects = interiors.Count > 0;
                }
       
            }
            else if (aArcRec.Type == uppArcRecTypes.Rectangle && bArcRec.Type == uppArcRecTypes.Rectangle)   // =========  RECTANGLE TO RECTANGLE
            {


                uopRectangle rec1 = aArcRec.Rectangle;
                uopRectangle rec2 =  bArcRec.Rectangle;
                if (rec2 == null || rec1 == null) return false;
                aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15, 4);
                double d1 = Math.Sqrt(Math.Pow(rec2.X - rec1.X, 2) + Math.Pow(rec2.Y - rec1.Y, 2));
                double diag1 = 0.5 * rec1.Diagonal;
                double diag2 = 0.5 * rec2.Diagonal;

                mzUtils.SortTwoValues(false, ref diag1, ref diag2);

                double d2 =  diag1 + diag2;
                if (Math.Round( d1, aPrecis) > Math.Round(d2, aPrecis)) return false; // to far away

                if (bReturnTrueByCenter && (rec1.ContainsVector(rec2.Center, bOnIsIn, aPrecis, true) || rec2.ContainsVector(rec1.Center, bOnIsIn, aPrecis, true)))
                    return true;


                //check the corners
                uopVectors corners1 = rec1.Corners;
                uopVectors corners2= rec2.Corners;

                List<uopVector> interiors = corners1.FindAll(x => rec2.ContainsVector(x, bOnIsIn: true, out bool within, out bool ison, out bool corner, aPrecis: aPrecis));


                _rVal = bMustBeCompletelyWithin ? interiors.Count == 4 : interiors.Count > 0;
                if (interiors.Count == 0)
                {
                    uopLines edges1 = corners1.LineSegments(true);
                    uopLines edges2 = corners2.LineSegments(true);
                    interiors = edges1.Intersections(edges2,aLinesAreInfinite:false, bLinesAreInfinite: false);
                    rIntersects = interiors.Count > 0;
                    if (rIntersects && !bMustBeCompletelyWithin) _rVal = true;
                }

            }
            else     // =========  ARC TO SLOT
            if (aArcRec.Type == uppArcRecTypes.Arc && bArcRec.Type == uppArcRecTypes.Slot)
            {
                uopArc arc = aArcRec.Type == uppArcRecTypes.Arc ?aArcRec.Arc : bArcRec.Arc;
                uopShape slot = aArcRec.Type == uppArcRecTypes.Arc ? bArcRec.Slot : aArcRec.Slot;
                if (arc == null || slot == null) return false;
                aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15, 4);

                double rad1 = arc.Radius;
                double rad2 = slot.Width /2;

                double d1 = Math.Sqrt(Math.Pow(arc.X - slot.X, 2) + Math.Pow(arc.Y - slot.Y, 2));

                if (Math.Round(d1, aPrecis) > Math.Round(rad1 + rad2, aPrecis)) return false; // to far away
                if (bReturnTrueByCenter && (arc.ContainsVector(slot.Center, bOnIsIn, aPrecis, true) || slot.ContainsVector(arc.Center, bOnIsIn, aPrecis, true)))
                    return true;

                uopVectors extpts = slot.Segments.ExtentPoints();
                List<uopVector> interiors =extpts.FindAll(x => arc.ContainsVector(x,bOnIsIn,aPrecis,true)) ;
                if(interiors.Count > 0)
                {
                    if(interiors.Count == extpts.Count)
                        return true;
                    rIntersects = true;
                    return bMustBeCompletelyWithin ? false : true;
                }

                extpts = arc.PhantomPoints();
                interiors = extpts.FindAll(x => slot.ContainsVector(x, bOnIsIn, aPrecis, true));
                if (interiors.Count > 0)
                {
                    if (interiors.Count == extpts.Count)
                        return true;
                    rIntersects = true;
                    return bMustBeCompletelyWithin ? false : true;
                }

                extpts = arc.Intersections(slot.Segments);
                rIntersects = extpts.Count > 0;
                if (rIntersects && !bMustBeCompletelyWithin) _rVal = true;
            }  // =========  RECTANGLE TO SLOT
            else if (aArcRec.Type == uppArcRecTypes.Rectangle && bArcRec.Type == uppArcRecTypes.Slot)
            {
                uopRectangle rec = aArcRec.Type == uppArcRecTypes.Rectangle ? aArcRec.Rectangle : bArcRec.Rectangle;
                uopShape slot = aArcRec.Type == uppArcRecTypes.Rectangle ? bArcRec.Slot : aArcRec.Slot;
                if (rec == null || slot == null) return false;
                aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15, 4);

                double rad1 = rec.Diagonal/2;
                double rad2 = slot.Width / 2;

                double d1 = Math.Sqrt(Math.Pow(rec.X - slot.X, 2) + Math.Pow(rec.Y - slot.Y, 2));

                if (Math.Round(d1, aPrecis) > Math.Round(rad1 + rad2, aPrecis)) return false; // to far away
                if (bReturnTrueByCenter && (rec.ContainsVector(slot.Center, bOnIsIn, aPrecis, true) || slot.ContainsVector(rec.Center, bOnIsIn, aPrecis, true)))
                    return true;

                uopVectors extpts = slot.Segments.ExtentPoints();
                List<uopVector> interiors = extpts.FindAll(x => rec.ContainsVector(x, bOnIsIn, aPrecis, true));
                if (interiors.Count > 0)
                {
                    if (interiors.Count == extpts.Count)
                        return true;
                    rIntersects = true;
                    return bMustBeCompletelyWithin ? false : true;
                }

                extpts = rec.Corners;
                interiors = extpts.FindAll(x => slot.ContainsVector(x, bOnIsIn, aPrecis, true));
                if (interiors.Count > 0)
                {
                    if (interiors.Count == extpts.Count)
                        return true;
                    rIntersects = true;
                    return bMustBeCompletelyWithin ? false : true;
                }

                extpts = rec.Edges.Intersections(slot.Segments);
                rIntersects = extpts.Count > 0;
                if (rIntersects && !bMustBeCompletelyWithin) _rVal = true;
            }  // =========  SLOT TO SLOT
            else if (aArcRec.Type == uppArcRecTypes.Slot && bArcRec.Type == uppArcRecTypes.Slot)
            {
                uopShape slot1 = aArcRec.Slot;
                uopShape slot2 = bArcRec.Slot;
                if (slot1 == null || slot2 == null) return false;
                aPrecis = mzUtils.LimitedValue(aPrecis, 1, 15, 4);

                double rad1 = slot1.Width/2;
                double rad2 = slot2.Width / 2;

                double d1 = Math.Sqrt(Math.Pow(slot1.X - slot2.X, 2) + Math.Pow(slot1.Y - slot2.Y, 2));

                if (Math.Round(d1, aPrecis) > Math.Round(rad1 + rad2, aPrecis)) return false; // to far away
                if (bReturnTrueByCenter && (slot1.ContainsVector(slot2.Center, bOnIsIn, aPrecis, true) || slot2.ContainsVector(slot1.Center, bOnIsIn, aPrecis, true)))
                    return true;

                uopVectors extpts = slot1.Segments.ExtentPoints();
                List<uopVector> interiors = extpts.FindAll(x => slot2.ContainsVector(x, bOnIsIn, aPrecis, true));
                if (interiors.Count > 0)
                {
                    if (interiors.Count == extpts.Count)
                        return true;
                    rIntersects = true;
                    return bMustBeCompletelyWithin ? false : true;
                }

                extpts = slot2.Segments.ExtentPoints();
                interiors = extpts.FindAll(x => slot1.ContainsVector(x, bOnIsIn, aPrecis, true));
                if (interiors.Count > 0)
                {
                    if (interiors.Count == extpts.Count)
                        return true;
                    rIntersects = true;
                    return bMustBeCompletelyWithin ? false : true;
                }

                extpts = slot2.Segments.IntersectionPoints(slot1.Segments, bNoDupes:true, aNoDupesPrecis:3);
                rIntersects = extpts.Count > 0;
                if (rIntersects && !bMustBeCompletelyWithin) _rVal = true;
            }


            return _rVal;
         }
            #endregion Shared Methods
     }
}
