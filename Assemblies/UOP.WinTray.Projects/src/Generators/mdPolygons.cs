using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using System.Linq;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;
using System.Numerics;

namespace UOP.WinTray.Projects.Generators
{
    public class mdPolygons
    {


        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the shelf angle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bObscured"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="rPlane"></param>
        /// <returns></returns>
        public static colDXFVectors ShelfAngle_Vertices(mdShelfAngle aPart, bool bObscured, iVector aCenter, double aRotation, out dxfPlane rPlane)
        {
            colDXFVectors _rVal = new colDXFVectors();
            try
            {

                if (aPart == null)
                {
                    rPlane = dxfPlane.World;
                    return _rVal;
                }

                double thk = aPart.Thickness;
                double wd = aPart.Width;
                double cX = aPart.X;
                double cY = aPart.Y;
                double ht = aPart.Length;
                double gap = aPart.PanelClearance;
                string hiddenlt = dxfLinetypes.Hidden;
                //hiddenlt = string.Empty;
                double f1 = aPart.Side == uppSides.Left ? 1 : -1;

                rPlane = xPlane(cX, cY, aCenter, aRotation);

                if (!bObscured)
                {
                    _rVal.Add(rPlane, (0.5 * wd - thk) * f1, -0.5 * ht, aLineType: hiddenlt);
                    _rVal.AddRelative(aY: ht, aPlane: rPlane);
                    _rVal.AddRelative(aX: -(wd - thk) * f1, aPlane: rPlane);
                    _rVal.AddRelative(aY: -ht, aPlane: rPlane);
                    _rVal.AddRelative(aX: wd * f1, aPlane: rPlane);
                    _rVal.AddRelative(aY: ht, aPlane: rPlane);
                    _rVal.AddRelative(aX: -thk * f1, aPlane: rPlane);
                }
                else
                {
                    double d1 = wd - thk;
                    double d2 = (thk - gap);
                    string lt2 = gap < thk ? dxfLinetypes.Hidden : "";
                    _rVal.Add(rPlane, aX: (0.5 * wd - thk) * f1, -0.5 * ht, aLineType: dxfLinetypes.Hidden);
                    _rVal.AddRelative(aY: ht, aLineType: lt2, aPlane: rPlane);
                    _rVal.AddRelative(aX: (thk - gap) * f1, aLineType: "", aPlane: rPlane);
                    _rVal.AddRelative(aX: gap * f1, aLineType: "", aPlane: rPlane);
                    _rVal.AddRelative(aY: -ht, aLineType: "", aPlane: rPlane);
                    _rVal.AddRelative(aX: -gap * f1, aLineType: lt2, aPlane: rPlane);
                    _rVal.AddRelative(aX: -(thk - gap) * f1, aLineType: dxfLinetypes.Hidden, aPlane: rPlane);
                    _rVal.AddRelative(aX: -d1 * f1, aLineType: dxfLinetypes.Hidden, aPlane: rPlane);
                    _rVal.AddRelative(aY: ht, aLineType: dxfLinetypes.Hidden, aPlane: rPlane);
                    _rVal.AddRelative(aX: d1 * f1, aLineType: dxfLinetypes.Hidden, aPlane: rPlane);

                    //_rVal.AddRelative(aX: thk * f1, aLineType: "", aPlane: rPlane);
                    //_rVal.AddRelative(aY: -ht, aLineType: "", aPlane: rPlane);
                    //_rVal.AddRelative(aX: -(gap + thk) * f1, aLineType: hiddenlt, aPlane: rPlane);
                    //_rVal.AddRelative(aX: -d1 * f1, aLineType: hiddenlt, aPlane: rPlane);
                    //_rVal.AddRelative( aY: ht, aLineType: hiddenlt, aPlane: rPlane);
                    //_rVal.AddRelative(aX: d1 * f1, aLineType: "" , aPlane: rPlane);
                    //_rVal.AddRelative(aX: gap * f1, aLineType: "", aPlane: rPlane);

                }


            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }


        /// <summary>
        ///    //#1scale factor to apply to the returned polygon
        //^returns a dxePolygon that is used to draw the end view of the shelf angle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon md_ShelfAngle_View_Elevation(mdShelfAngle aPart, bool bSuppressFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon mdShelfAngle_View_Elevation;
            try
            {

                mdShelfAngle_View_Elevation = new dxePolygon();
                if (aPart == null)
                {
                    return mdShelfAngle_View_Elevation;
                }
                double wd = 0;

                double cX = 0;

                double cY = 0;

                dxfPlane aPln = null;

                double f1 = 0;

                double thk = 0;


                thk = aPart.Thickness;
                cX = aPart.X;
                cY = aPart.Z;
                wd = aPart.Width / 2;
                f1 = 1;
                if (cX > 0)
                {
                    if (aPart.Direction != dxxRadialDirections.AwayFromCenter)
                    {
                        f1 = -f1;
                    }
                }
                else
                {
                    if (aPart.Direction == dxxRadialDirections.AwayFromCenter)
                    {
                        f1 = -f1;
                    }
                }


                aPln = xPlane(cX, cY, aCenter, aRotation);
                mdShelfAngle_View_Elevation.LayerName = aLayerName;
                mdShelfAngle_View_Elevation.Plane = aPln;
                mdShelfAngle_View_Elevation.InsertionPt = aPln.Origin;
                if (bSuppressFillets)
                {
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, -wd * f1);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, wd * f1);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, wd * f1, -thk);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, (-wd + thk) * f1, -thk);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, (-wd + thk) * f1, -2 * wd);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, -wd * f1, -2 * wd);

                }
                else
                {
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, (-wd + 2 * thk) * f1);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, wd * f1);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, wd * f1, -thk);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, (-wd + 2 * thk) * f1, -thk);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, (-wd + thk) * f1, -2 * thk);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, (-wd + thk) * f1, -2 * wd);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, -wd * f1, -2 * wd);
                    mdShelfAngle_View_Elevation.Vertices.Add(aPln, -wd * f1, -2 * wd + 2 * thk);

                }

            }
            catch (Exception)
            {

                throw;
            }
            return mdShelfAngle_View_Elevation;
        }


        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the shelf angle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon ShelfAngle_View_Plan(mdShelfAngle aPart, iVector aCenter = null, double aRotation = 0, bool bShowObscured = true, string aLayerName = "GEOMETRY")
        {

            try
            {


                if (aPart == null) return new dxePolygon();


                dxePolygon _rVal = new dxePolygon(ShelfAngle_Vertices(aPart, bShowObscured, aCenter, aRotation, out dxfPlane aPln), bClosed: false)
                {
                    Plane = aPln,
                    LayerName = aLayerName,
                    InsertionPt = aPln?.Origin,
                    BlockName = $"BOX_DC _{aPart.DowncomerIndex}_BOX_{aPart.BoxIndex}_{aPart.PartName.ToUpper()}_{aPart.Side.ToString().ToUpper()}_PLAN"
                };

                //_rVal.AdditionalSegments.AddArc(aPart.X, aPart.Y, 0.25);
                return _rVal;

            }
            catch (Exception)
            {
                throw;

            }

        }


        /// <summary>
        ///returns a dxePolygon that is used to draw the profile view of the end angle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon md_ShelfAngle_View_Profile(mdShelfAngle aPart, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRZY")
        {
            dxePolygon mdShelfAngle_View_Profile;
            try
            {

                mdShelfAngle_View_Profile = new dxePolygon();
                if (aPart == null)
                {
                    return mdShelfAngle_View_Profile;

                }

                double thk = 0;

                double cX = 0;

                double cY = 0;

                double lg = 0;

                double ht = 0;

                dxfPlane aPln = null;


                cY = aPart.Z;
                cX = aPart.Y;
                aPln = xPlane(cX, cY, aCenter, aRotation);
                thk = aPart.Thickness;
                lg = aPart.Length / 2;
                ht = aPart.Height;


                mdShelfAngle_View_Profile.Closed = false;
                mdShelfAngle_View_Profile.Vertices.Add(aPln, -lg, -thk);
                mdShelfAngle_View_Profile.Vertices.Add(aPln, lg, -thk);
                mdShelfAngle_View_Profile.Vertices.Add(aPln, lg, -ht);
                mdShelfAngle_View_Profile.Vertices.Add(aPln, -lg, -ht);
                mdShelfAngle_View_Profile.Vertices.Add(aPln, -lg, 0);
                mdShelfAngle_View_Profile.Vertices.Add(aPln, lg, 0);

                mdShelfAngle_View_Profile.LayerName = aLayerName;
                mdShelfAngle_View_Profile.Plane = aPln;
                mdShelfAngle_View_Profile.InsertionPt = aPln.Origin;

            }
            catch (Exception)
            {

                throw;
            }
            return mdShelfAngle_View_Profile;
        }

        public static dxePolygon DCBox_View_EndDetail(mdDowncomerBox aPart, mdTrayAssembly aAssy, double aPaperScale, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bInvert = false)
        {
            dxePolygon _rVal = new dxePolygon() { LayerName = aLayerName };

            aAssy ??= aPart.GetMDTrayAssembly(aAssy);

            if (aPaperScale <= 0) aPaperScale = 1;
            try
            {
                dxfVector ctr = aPart.CenterDXF;
                double thk = aPart.Thickness;
                double cX = Math.Abs(ctr.X);
                double cY = ctr.Y;
                bool fldov = aPart.FoldOverWeirs;
                double fldHt = !fldov ? 0 : aPart.FoldOverHeight;
                uopHoleArray holearry = aPart.GenHoles(aAssy, "FINGER CLIP,STARTUP,END ANGLE,ENDPLATE", bSuppressSpouts: true);
                double wd = aPart.Height; //outside
                double ht = 0.75 * aAssy.FunctionalPanelWidth;
                double leng = aPart.Length; // the short side

                double wrht = aPart.WeirHeight;
                colDXFVectors verts = new colDXFVectors();
                mdShelfAngle aSA = aPart.ShelfAngle(bLeft: false);
                double dY = 0.5 * (leng - aSA.Length);
                double dX = 0;
                double f1 = !bInvert ? 1 : -1;
                double ang1 = !bInvert ? 0 : 180;

                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);

                string lt = dY < 0 ? dxfLinetypes.Hidden : "";

                verts.Add(aPln, 0, -ht);
                dxfVector v1 = verts.Add(aPln, 0, 0);
                verts.AddRelative(-wrht * f1, 0, aTag: "P1", aPlane: aPln, aLineType: lt);
                verts.AddRelative(-aSA.Height * f1, 0, aTag: "P2", aPlane: aPln);
                dxfVector v2 = verts.AddRelative(-f1 * (wd - wrht - aSA.Height), 0, aTag: "CORNER", aPlane: aPln);
                verts.AddRelative(0, -ht, aPlane: aPln);

                dxeLine l1 = new dxeLine(verts.Item(1), verts.LastVector());
                _rVal = new dxePolygon(verts, verts.Item(2), false, aPlane: aPln) { LayerName = aLayerName };

                _rVal.AddAdditionalSegment(new dxePoint(aPln.Origin) { Tag = "DIM1" });
                // the thickness line
                _rVal.AddAdditionalSegment(new dxeLine(v2 + aPln.XDirection * (f1 * thk), verts.LastVector() + aPln.XDirection * (f1 * thk)) { Linetype = dxfLinetypes.Hidden }, aTag: "THICKNESS");

                //the break line
                colDXFVectors pverts = new colDXFVectors();
                double d1 = 0.3333 * wrht;
                pverts.Add(verts.Item(1) + aPln.XDirection * d1 * f1);
                pverts.AddRelative((-d1 - wrht - aSA.Height - 0.25) * f1, aPlane: aPln);

                double d2 = wd / 3;
                double d3 = Math.Sin(Math.PI / 6) * (d2 / 2);
                pverts.AddPolar(180 + 60 + ang1, d3, aPlane: aPln, aPolarToIndex: 2);
                pverts.AddPolar(120 + ang1, 2 * d3, aPlane: aPln);
                pverts.AddPolar(180 + 60 + ang1, 2 * d3, aPlane: aPln);
                pverts.AddPolar(120 + ang1, d3, aPlane: aPln);
                pverts.Add(verts.LastVector() + aPln.XDirection * -d1 * f1);

                dxePolyline breakline = new dxePolyline(pverts, false) { LayerName = aLayerName, Tag = "BREAK_LINE" };


                _rVal.AddAdditionalSegment(breakline);

                //the short angle


                dxfVector v3 = _rVal.Vertex(3) - aPln.YDirection * dY;

                dxeLine l2 = new dxeLine(v3, v3 - aPln.YDirection * aSA.Length);
                dxfVector v4 = l1.IntersectPoint(l2);
                l2.EndPt = v4;

                dxeLine l3 = new dxeLine(v3 + aPln.XDirection * -f1 * aSA.Thickness, v4 + aPln.XDirection * -f1 * aSA.Thickness);


                dxeLine l4 = new dxeLine(v3 + aPln.XDirection * -f1 * aSA.Height, v4 + aPln.XDirection * -f1 * aSA.Height);
                _rVal.AddAdditionalSegment(l3, aTag: "ANGLE THICKNESS");

                if (dY >= 0)
                {

                    _rVal.AddAdditionalSegment(new dxePolyline(new colDXFVectors(l2.EndPt, l2.StartPt, l4.StartPt, l4.EndPt), false), aTag: "ANGLE");
                }
                else
                {

                    l2.StartPt += aPln.YDirection * dY;
                    l4.StartPt += aPln.YDirection * dY;

                    _rVal.AddAdditionalSegment(l2, aTag: "TOP OF ANGLE");
                    _rVal.AddAdditionalSegment(l4, aTag: "BOTTOM OF ANGLE");

                    _rVal.AddAdditionalSegment(new dxePolyline(new colDXFVectors(l2.StartPt, l2.StartPt + aPln.YDirection * -dY, l4.StartPt + aPln.YDirection * -dY, l4.StartPt), false), aTag: "ANGLE");

                }

                //the fold over line
                if (fldHt > 0)
                {

                    _rVal.AddAdditionalSegment(new dxeLine(v1 + aPln.XDirection * -fldHt * f1, verts.Item(1) + aPln.XDirection * -fldHt * f1), aTag: "THICKNESS");
                }


                //the holes
                //find the end angle hole
                uopHoles holes = holearry.Item("END ANGLE");
                uopHole hole = null;
                uopVectors ctrs = null;
                uopVector v0;
                dxfPlane pPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y));
                dxfVector corner = pPln.Vector(0.5 * aPart.Width - 0.5 * thk, leng * 0.5);
                double y1 = corner.Y;
                bool addpt = true;
                if (holes.Count > 0)
                {
                    hole = new uopHole(holes.Member) { ExtrusionDirection = "0,0,1", Rotation = 90 + aRotation };
                    dY = -hole.Inset;
                    dX = -wrht + holes.Elevation;
                    hole.CenterDXF = aPln.Vector(dX * f1, dY);

                    _rVal.AddAdditionalSegment(hole.BoundingEntity(aLayerName: _rVal.LayerName), bAddCenterPt: addpt);

                    addpt = false;
                }
                holes = holearry.Item("FINGER CLIP");
                if (holes.Count > 0)
                {
                    ctrs = holes.Centers;
                    hole = new uopHole(holes.Member) { ExtrusionDirection = "0,0,1" };

                    v0 = ctrs.GetVector(dxxPointFilters.GetRightTop);

                    dY = v0.Y - y1;
                    dX = -wrht + holes.Elevation;
                    hole.CenterDXF = aPln.Vector(dX * f1, dY);

                    dxfVector ip = hole.CenterDXF + aPln.YDirection * -hole.Radius;
                    dxfVector pp = ip.ProjectedToLine(l1);
                    dxfDirection dir = ip.DirectionTo(pp, ref d1);

                    if (dir.IsEqual(aPln.YDirection))
                    {
                        hole.CenterDXF += dir * (d1 + 0.0625 * aPaperScale);
                    }


                    _rVal.AddAdditionalSegment(hole.BoundingEntity(aLayerName: _rVal.LayerName), bAddCenterPt: addpt);

                }
                holes = holearry.Item("STARTUP");
                if (holes != null && holes.Count > 0)
                {
                    ctrs = holes.Centers;
                    hole = new uopHole(holes.Member) { ExtrusionDirection = "0,0,1", Rotation = 90 + aRotation };
                    v0 = ctrs.GetVector(dxxPointFilters.GetRightTop);

                    dY = v0.Y - corner.Y;
                    dX = -wrht + holes.Elevation;
                    hole.CenterDXF = aPln.Vector(dX * f1, dY);

                    dxfVector ip = hole.CenterDXF + aPln.YDirection * -hole.Length / 2;
                    dxfVector pp = ip.ProjectedToLine(l1);
                    dxfDirection dir = ip.DirectionTo(pp, ref d1);

                    if (dir.IsEqual(aPln.YDirection))
                    {
                        hole.CenterDXF += dir * (d1 + 0.0625 * aPaperScale);
                    }

                    _rVal.AddAdditionalSegment(hole.BoundingEntity(aLayerName: _rVal.LayerName), bAddCenterPt: true);
                }

                if (aPart.BoltOnEndplates)
                {
                    holes = holearry.Item("ENDPLATE");
                    if (holes.Count > 0)
                    {
                        ctrs = holes.Centers;
                        hole = new uopHole(holes.Member) { ExtrusionDirection = "0,0,1" };
                        v0 = ctrs.GetVector(dxxPointFilters.GetRightTop);
                        dY = v0.Y - corner.Y;

                        foreach (uopVector item in ctrs)
                        {
                            double elev = item.Elevation.HasValue ? item.Elevation.Value : 0;
                            dX = -wrht +elev;
                            hole.CenterDXF = aPln.Vector(dX * f1, dY);
                            _rVal.AddAdditionalSegment(hole.BoundingEntity(aLayerName: _rVal.LayerName), bAddCenterPt: true);
                        }





                    }

                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {


                _rVal.InsertionPt = _rVal.Vertex(2);
            }
            return _rVal;
        }

        /// <summary>
        //#2the parent tray assembly for this part
        //#3the parent downcomer for this part
        //#4flag to suppress then spout holes
        //^returns a dxePolygon that is used to draw the plan view of the box
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aAssy"></param>
        /// <param name="aDC"></param>
        /// <param name="bSuppressSpouts"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="rBottomNut"></param>
        /// <returns></returns>
        public static dxePolygon DCBox_View_Layout(mdDowncomerBox aPart, mdTrayAssembly aAssy, out hdwHexNut rBottomNut, bool bSuppressSpouts = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon();
            dxfPlane aPln = null;
            rBottomNut = null;

            try
            {
                if (aPart == null) return _rVal;
                aAssy ??= aPart.GetMDTrayAssembly();
                if (aAssy == null) return _rVal;


                dxfVector ctr = aPart.CenterDXF;

                double thk = aPart.Thickness;
                double wd = aPart.Width; //outside
                double rleng = aPart.RightLength;
                double lleng = aPart.LeftLength;
                double cX = ctr.X;
                double cY = ctr.Y;
                bool fldov = aPart.FoldOverWeirs;
                double fldHt = !fldov ? 0 : aPart.FoldOverHeight;

                double ht = aPart.Height; //outside
                double factr = uopGlobals.goGlobalVars().ValueD("BendFactor", 1.7);

                wd += 2 * ht - (2 * factr * thk);  // total width of flat plate
                double dkthk = aPart.DeckThickness;
                double d1 = 0;
            
                double wht = aPart.How;
                string tgs = string.Empty;
                double xl = -wd / 2 + (wht + dkthk); //top of angle on Left
                double xr = wd / 2 - (wht + dkthk); //top of angle on Right

                //aRotation += 90;

                dxfPlane bPln = new dxfPlane(new dxfVector(cX, cY, aPart.Z));
                aPln = xPlane(cX, cY, aCenter, aRotation);
                dxfDirection xDir = aPln.XDirection;
                dxfDirection yDir = aPln.YDirection;


                uopHoleArray aHls = aPart.GenHoles(aAssy, bSuppressSpouts: true);

                uopHole aHl = null;
                dxePolyline aPl = null;
                dxePolyline bPl = null;

                dxeArc aArc = null;
                dxeArc bArc = null;
                dxfVector v1 = null;
                dxfVector v2 = null;
       
                dxePolyline aNutPl = null;
                colDXFVectors verts = new colDXFVectors();
                double dX = aPln.X - bPln.X;
                double dY = aPln.Y - bPln.Y;
                List<uopHole> lHoles;

                if (fldov)
                {

                    d1 = wd;

                    if (fldHt > 0)
                    {
                        d1 = aPart.Width + ht + ht + fldHt + fldHt - (factr * 2 * thk) - (0.5 * thk * 2);
                        fldHt = (d1 - wd) / 2;
                        wd = d1;
                    }
                }

                wd /= 2;
                double W = wd - ht - fldHt;

                //find the Y values using the boxe's left and right lines.
                ULINE leftBx = aPart.BoxLns.GetSideValue( uppSides.Left );
                ULINE rightBx = aPart.BoxLns.GetSideValue( uppSides.Right );
                double lby = leftBx.ep.ToDXFVector().WithRespectToPlane(aPart.Plane).Y;
                double lty = leftBx.sp.ToDXFVector().WithRespectToPlane( aPart.Plane ).Y;
                double rby = rightBx.ep.ToDXFVector().WithRespectToPlane( aPart.Plane ).Y;
                double rty = rightBx.sp.ToDXFVector().WithRespectToPlane( aPart.Plane ).Y;

                //note HasTriangularEndPlate is not set for beam connected boxes, may need to revisit that for other functionality. The following
                //is a work around for that limitation
                if (aPart.HasTriangularEndPlate || (aPart.IntersectionType_Bot == uppIntersectionTypes.ToDivider && aPart.IntersectionType_Top == uppIntersectionTypes.ToDivider))
                {
                    //Both ends are triangular
                    verts.Add( aPln, -wd, lby ); //bl
                    verts.Add( aPln, -W, lby, aTag: "INFLECTION", aFlag: "NOH" ); //bml
                    verts.Add( aPln, W, rby, aTag: "INFLECTION", aFlag: "NOH" ); //bmr
                    verts.Add( aPln, wd, rby ); //br
                    verts.Add( aPln, wd, rty ); //tr
                    verts.Add( aPln, W, rty, aTag: "INFLECTION", aFlag: "NOH" ); //tmr
                    verts.Add( aPln, -W, lty, aTag: "INFLECTION", aFlag: "NOH" ); //tml
                    verts.Add( aPln, -wd, lty ); //tl
                }
                else if (!aPart.HasTriangularEndPlate)
                {
                    //assumed that no ring connected box end is  triangular if !HasTriangularEndPlate is false
                    if (aPart.IntersectionType_Bot == uppIntersectionTypes.ToRing && aPart.IntersectionType_Top == uppIntersectionTypes.ToDivider)
                    {
                        //Bottom (Right after rotation) is rectangular, top (left after rotation) is triangular
                        verts.Add( aPln, -wd, lby ); //bl
                        verts.Add( aPln, wd, rby ); //br
                        verts.Add( aPln, wd, rty ); //tr
                        verts.Add( aPln, W, rty, aTag: "INFLECTION", aFlag: "NOH" ); //tmr
                        verts.Add( aPln, -W, lty, aTag: "INFLECTION", aFlag: "NOH" ); //tml
                        verts.Add( aPln, -wd, lty ); //tl
                    }
                    else if (aPart.IntersectionType_Bot == uppIntersectionTypes.ToDivider && aPart.IntersectionType_Top == uppIntersectionTypes.ToRing)
                    {
                        //Bottom (Right after rotation) is triangular, top (left after rotation) is rectangular
                        verts.Add( aPln, -wd, lby ); //bl
                        verts.Add( aPln, -W, lby, aTag: "INFLECTION", aFlag: "NOH" ); //bml
                        verts.Add( aPln, W, rby, aTag: "INFLECTION", aFlag: "NOH" ); //bmr
                        verts.Add( aPln, wd, rby ); //br
                        verts.Add( aPln, wd, rty ); //tr
                        verts.Add( aPln, -wd, lty ); //tl
                    }
                    else
                    {
                        //Both ends are rectangular
                        verts.Add( aPln, -wd, lby ); //bl
                        verts.Add( aPln, wd, rby ); //br
                        verts.Add( aPln, wd, rty ); //tr
                        verts.Add( aPln, -wd, lty ); //tl
                    }
                }
                _rVal.Vertices = verts;


                if (aPart.WeldedBottomNuts)
                {
                    tgs = aHls.Names();

                    if (mzUtils.ListContains("APPAN_HOLE", tgs) || mzUtils.ListContains("CROSSBRACE", tgs))
                    {
                        rBottomNut = aAssy.SmallBolt("APPAN").GetNut();

                        aNutPl = uopPolygons.HexNutPlan(rBottomNut.HardwareStructure, aLayerName: aLayerName, aPlane: aPln);
                        aNutPl.Tag = "BOTTOM_NUT";
                        aNutPl.Translate(aPln.Origin * -1);
                    }
                }

                v1 = dxfVector.Zero;
                _rVal.LayerName = aLayerName;

                //the right shelf angle
                string aFlag = (lleng > rleng) ? "SHORT" : "LONG";
                mdShelfAngle aSA = aPart.ShelfAngle( bLeft: false );

                //find the Y values using the shelf angle.
                double sby = aSA.EdgeLn.ep.ToDXFVector().WithRespectToPlane( aPart.Plane ).Y;
                double sty = aSA.EdgeLn.sp.ToDXFVector().WithRespectToPlane( aPart.Plane ).Y;

                verts = new colDXFVectors
                    {
                        { aPln, xr - thk, sty },
                        { aPln, xr - thk, sby },
                        { aPln, xr, sby },
                        { aPln, xr, sty },
                        { aPln, xr - aSA.Height, sty },
                        { aPln, xr - aSA.Height, sby },
                        { aPln, xr - thk, sby }
                    };
                aPl = new dxePolyline( verts, true, new dxfDisplaySettings( aLayer: aLayerName, aLinetype: dxfLinetypes.Hidden ) );
                _rVal.AdditionalSegments.Add( (dxfEntity)aPl, aTag: "SHELF", aFlag: aFlag, aValue: aSA.Length );

                if (sby < rby)
                {
                    verts = new colDXFVectors
                        {
                            { aPln, xr, rby },
                            { aPln, xr, sby },
                            { aPln, xr - aSA.Height, sby },
                            { aPln, xr - aSA.Height, rby }
                        };
                    aPl = new dxePolyline( verts, false, new dxfDisplaySettings( aLayer: aLayerName, aLinetype: dxfLinetypes.Continuous ) );
                    _rVal.AdditionalSegments.Add( aPl, aTag: "SHELF", aFlag: aFlag, aValue: aSA.Length );
                }

                if (sty > rty)
                {
                    verts = new colDXFVectors
                        {
                            { aPln, xr, rty },
                            { aPln, xr, sty },
                            { aPln, xr - aSA.Height, sty },
                            { aPln, xr - aSA.Height, rty }
                        };
                    bPl = new dxePolyline( verts, false, new dxfDisplaySettings( aLayer: aLayerName ) );
                    _rVal.AdditionalSegments.Add( bPl, aTag: "SHELF", aFlag: aFlag, aValue: aSA.Length );
                }

                //the Left shelf angle
                aFlag = (lleng > rleng) ? "LONG" : "SHORT";
                aSA = aPart.ShelfAngle( bLeft: true );

                //find the Y values using the shelf angle.
                sby = aSA.EdgeLn.ep.ToDXFVector().WithRespectToPlane( aPart.Plane ).Y;
                sty = aSA.EdgeLn.sp.ToDXFVector().WithRespectToPlane( aPart.Plane ).Y;

                verts = new colDXFVectors
                    {
                        { aPln, xl + thk, sty },
                        { aPln, xl + thk, sby },
                        { aPln, xl, sby },
                        { aPln, xl, sty },
                        { aPln, xl + aSA.Height, sty },
                        { aPln, xl + aSA.Height, sby },
                        { aPln, xl + thk, sby }
                    };
                aPl = new dxePolyline( verts, true, new dxfDisplaySettings( aLayer: aLayerName, aLinetype: dxfLinetypes.Hidden ) );
                _rVal.AdditionalSegments.Add( (dxfEntity)aPl, aTag: "SHELF", aFlag: aFlag, aValue: aSA.Length );

                if (sby < lby)
                {
                    verts = new colDXFVectors
                        {
                            { aPln, xl, lby },
                            { aPln, xl, sby },
                            { aPln, xl + aSA.Height, sby },
                            { aPln, xl + aSA.Height, lby }
                        };
                    aPl = new dxePolyline( verts, false, new dxfDisplaySettings( aLayer: aLayerName, aLinetype: dxfLinetypes.Continuous ) );
                    _rVal.AdditionalSegments.Add( aPl, aTag: "SHELF", aFlag: aFlag, aValue: aSA.Length );
                }

                if (sty > lty)
                {
                    verts = new colDXFVectors
                        {
                            { aPln, xl, lty },
                            { aPln, xl, sty },
                            { aPln, xl + aSA.Height, sty },
                            { aPln, xl + aSA.Height, lty }
                        };
                    bPl = new dxePolyline( verts, false, new dxfDisplaySettings( aLayer: aLayerName ) );
                    _rVal.AdditionalSegments.Add( bPl, aTag: "SHELF", aFlag: aFlag, aValue: aSA.Length );
                }


                //d1 = 0.5 * aSA.Length;

                //if (Math.Round(aPart.X, 1) > 0) //on the right side of center
                //{

                //    verts = new colDXFVectors
                //    {
                //        { aPln, xr - thk, d1 },
                //        { aPln, xr - thk, -d1 },
                //        { aPln, xr, -d1 },
                //        { aPln, xr, d1 },
                //        { aPln, xr - aSA.Height, d1 },
                //        { aPln, xr - aSA.Height, -d1 },
                //        { aPln, xr - thk, -d1 }
                //    };
                //    aPl = new dxePolyline(verts, true, new dxfDisplaySettings(aLayer: aLayerName, aLinetype: dxfLinetypes.Hidden));
                //    _rVal.AdditionalSegments.Add((dxfEntity)aPl, aTag: "SHELF", aFlag: "SHORT", aValue: aSA.Length);

                //}
                //else //on the left side of center
                //{
                //    d1 = 0.5 * aSA.Length;
                //    d2 = 0.5 * rleng;
                //    verts = new colDXFVectors
                //    {
                //        { aPln, xr, d2 },
                //        { aPln, xr, d1 },
                //        { aPln, xr - aSA.Height, d1 },
                //        { aPln, xr - aSA.Height, d2 }
                //    };
                //    aPl = new dxePolyline(verts, false, new dxfDisplaySettings(aLayer: aLayerName, aLinetype: dxfLinetypes.Continuous));
                //    _rVal.AdditionalSegments.Add(aPl, aTag: "SHELF", aFlag: "SHORT", aValue: aSA.Length);


                //    verts = new colDXFVectors
                //    {
                //        { aPln, xr, -d2 },
                //        { aPln, xr, -d1 },
                //        { aPln, xr - aSA.Height, -d1 },
                //        { aPln, xr - aSA.Height, -d2 }
                //    };
                //    bPl = new dxePolyline(verts, false, new dxfDisplaySettings(aLayer: aLayerName));
                //    _rVal.AdditionalSegments.Add(bPl, aTag: "SHELF", aFlag: "SHORT", aValue: aSA.Length);


                //    aLn = new dxeLine(aPl.Vertex(1), bPl.Vertex(1), new dxfDisplaySettings(aLayer: aLayerName, aLinetype: dxfLinetypes.Hidden));
                //    _rVal.AdditionalSegments.Add(aLn, bAddClone: true, aTag: "SHELF", aFlag: "SHORT", aValue: aSA.Length);

                //    _rVal.AdditionalSegments.Add(aLn, bAddClone: true, aTag: "SHELF", aFlag: "SHORT", aValue: aSA.Length);
                //    aLn.Translate(aPln.XDirection * -thk);
                //    aLn.StartPt += aPln.YDirection * (d1 - d2);
                //    aLn.EndPt += aPln.YDirection * -(d1 - d2);

                //    _rVal.AdditionalSegments.Add(aLn, bAddClone: true, aTag: "SHELF", aFlag: "SHORT", aValue: aSA.Length);

                //    aLn.StartPt = aPl.LastVertex();
                //    aLn.EndPt = bPl.LastVertex();
                //    _rVal.AdditionalSegments.Add(aLn, bAddClone: true, aTag: "SHELF", aFlag: "SHORT", aValue: aSA.Length);

                //}


                //the long angle
                //aSA = aPart.ShelfAngle(bLeft: true);
                //d1 = 0.5 * aSA.Length;
                //d2 = 0.5 * lleng;

                //verts = new colDXFVectors
                //{
                //    { aPln, xl, d2 },
                //    { aPln, xl, d1 },
                //    { aPln, xl + 1, d1 },
                //    { aPln, xl + 1, d2 }
                //};
                //aPl = new dxePolyline(verts, false, new dxfDisplaySettings(aLayer: aLayerName));

                //_rVal.AdditionalSegments.Add(aPl, aTag: "SHELF", aFlag: "LONG", aValue: aSA.Length);

                //verts = new colDXFVectors
                //{
                //    { aPln, xl, -d2 },
                //    { aPln, xl, -d1 },
                //    { aPln, xl + 1, -d1 },
                //    { aPln, xl + 1, -d2 }
                //};
                //bPl = new dxePolyline(verts, false, new dxfDisplaySettings(aLayer: aLayerName));
                //_rVal.AdditionalSegments.Add(bPl, aTag: "SHELF", aFlag: "LONG", aValue: aSA.Length);


                //aLn = new dxeLine(aPl.Vertex(1), bPl.Vertex(1), new dxfDisplaySettings(aLayer: aLayerName, aLinetype: dxfLinetypes.Hidden));
                //_rVal.AdditionalSegments.Add(aLn, bAddClone: true, aTag: "SHELF", aFlag: "LONG", aValue: aSA.Length);
                //aLn.Translate(aPln.XDirection * thk);
                //aLn.StartPt += aPln.YDirection * (d1 - d2);
                //aLn.EndPt += aPln.YDirection * -(d1 - d2);

                //_rVal.AdditionalSegments.Add(aLn, bAddClone: true, aTag: "SHELF", aFlag: "LONG", aValue: aSA.Length);
                //aLn.StartPt = aPl.LastVertex();
                //aLn.EndPt = bPl.LastVertex();
                //_rVal.AdditionalSegments.Add(aLn, bAddClone: true, aTag: "SHELF", aFlag: "LONG", aValue: aSA.Length);

                if (aHls.Contains("END ANGLE"))
                {
                    lHoles = aHls.Item("END ANGLE").ToList;
                    foreach (uopHole hole in lHoles)
                    {

                        v1 = (hole.X < bPln.X) ? new dxfVector(xl - hole.Z, hole.Y) : new dxfVector(xr + hole.Z, hole.Y);
                        v2 = aPln.Vector(v1.X, v1.Y + dY);

                        if (hole.HoleType == uppHoleTypes.Slot)
                        {
                            aPl = (dxePolyline)dxfPrimatives.CreatePill(v2, hole.Length, hole.Diameter, 90, aPlane: aPln);
                            aPl.TFVSet(hole.Tag, hole.Flag, lHoles.Count);
                            _rVal.AddAdditionalSegment(aPl, bAddCenterPt: true);
                        }
                        else
                        {
                            aArc = new dxeArc(v2, hole.Radius) { LayerName = aLayerName, Tag = hole.Tag, Flag = hole.Flag, Value = lHoles.Count };
                            _rVal.AddAdditionalSegment(aArc, bAddCenterPt: true);
                        }
                    }
                }



                if (aHls.Contains("STARTUP"))
                {
                    lHoles = aHls.Item("STARTUP").ToList;
                    foreach (uopHole hole in lHoles)
                    {
                        v1 = (hole.X < bPln.X) ? new dxfVector(xl - hole.Z, hole.Y) : new dxfVector(xr + hole.Z, hole.Y);
                        v2 = aPln.Vector(v1.X, v1.Y + dY);
                        aPl = (dxePolyline)dxfPrimatives.CreatePill(v2, hole.Length, hole.Diameter, 90, aPlane: aPln);
                        aPl.TFVSet(hole.Tag, hole.Flag, lHoles.Count);
                        _rVal.AddAdditionalSegment(aPl, bAddCenterPt: true);

                    }
                }



                if (aHls.Contains("FINGER CLIP"))
                {
                    lHoles = aHls.Item("FINGER CLIP").ToList;
                    foreach (uopHole hole in lHoles)
                    {
                        v1 = (hole.X < bPln.X) ? new dxfVector(xl - hole.Z, hole.Y) : new dxfVector(xr + hole.Z, hole.Y);
                        v2 = aPln.Vector(v1.X, v1.Y + dY);
                        aArc = new dxeArc(v2, hole.Radius) { LayerName = aLayerName, Tag = hole.Tag, Flag = hole.Flag, Value = lHoles.Count };
                        _rVal.AddAdditionalSegment(aArc, bAddCenterPt: true);
                    }

                }


                if (aPart.BoltOnEndplates)
                {
                    lHoles = aHls.Item("ENDPLATE").ToList;
                    foreach (uopHole hole in lHoles)
                    {
                        v1 = (hole.X < bPln.X) ? new dxfVector(xl - hole.Z, hole.Y) : new dxfVector(xr + hole.Z, hole.Y);
                        v2 = aPln.Vector(v1.X, v1.Y + dY);
                        aArc = new dxeArc(v2, hole.Radius) { LayerName = aLayerName, Tag = hole.Tag, Flag = hole.Flag, Value = lHoles.Count };
                        _rVal.AddAdditionalSegment(aArc, bAddCenterPt: true);

                    }

                }

                //holes in the bottom

                if (!bSuppressSpouts)
                {
                    colMDSpoutGroups aSGs = aPart.SpoutGroups(aAssy);
                    mdSpoutGroup aSG = null;
                    colDXFEntities blkEnts = null;
                    dxeLine zAxs = aPln.ZAxis();
                    dxeLine xAxs = aPln.XAxis();
                    uopHoles sptHls;

                    v1 = aPln.Origin - bPln.Origin;
                    v1.Z = 0;
                    for (int i = 1; i <= aSGs.Count; i++)
                    {
                        aSG = aSGs.Item(i);
                        if (aSG.SpoutCount() > 0)
                        {
                            //                    Set v1 = aSG.Center.WithRespectToPlane(bPln)

                            sptHls = aSG.Spouts;
                            blkEnts = sptHls.Entities(aLayerName, dxxColors.ByLayer, dxfLinetypes.Continuous);
                            blkEnts.Translate(v1);

                            if (aRotation != 0)
                            {
                                blkEnts.RotateAbout(zAxs, aRotation);
                            }
                            _rVal.AdditionalSegments.Append(blkEnts, bAddClones: true, aTag: "SPOUT", aFlag: aSG.Handle);

                            if (Math.Abs(Math.Round(aSG.Y - aPart.Y, 1)) > 0 && aPart.DesignFamily.IsStandardDesignFamily() )
                            {
                                blkEnts = blkEnts.Clone();
                                blkEnts.Mirror(xAxs);
                                _rVal.AdditionalSegments.Append(blkEnts, aTag: "SPOUT", aFlag: aSG.Handle);
                            }

                        }
                    }
                }

                if (aAssy.DesignOptions.HasAntiPenetrationPans)
                {
                    if (aHls.Contains("APPAN_HOLE"))
                    {
                        lHoles = aHls.Item("APPAN_HOLE").ToList;
                        foreach (uopHole hole in lHoles)
                        {

                            v1 = hole.CenterDXF.WithRespectToPlane(bPln);
                            v2 = aPln.Vector(v1.X, v1.Y);
                            aArc = new dxeArc(v2, hole.Radius) { LayerName = aLayerName, Tag = hole.Tag, Flag = hole.Flag, Value = lHoles.Count };
                            _rVal.AddAdditionalSegment(aArc, bAddCenterPt: true);


                            //bArc.TFVSet(hole.Tag, hole.Flag, hole.Value);
                            if (aNutPl != null)
                            {

                                aPl = (dxePolyline)aNutPl.Clone();
                                aPl.MoveTo(v2);
                                _rVal.AdditionalSegments.Add(aPl);

                            }

                        }
                    }

                    if (aHls.Contains("APPAN_SLOT"))
                    {
                        lHoles = aHls.Item("APPAN_SLOT").ToList;
                        if (lHoles.Count > 0)
                        {
                            aHl = lHoles[0].Clone();
                            v1 = aHl.CenterDXF.WithRespectToPlane(bPln);
                            aHl.CenterDXF = dxfVector.Zero;

                            aPl = (dxePolyline)aHl.BoundingEntity(aLayerName: aLayerName);
                            aPl.Value = lHoles.Count;
                            if (aRotation != 0) aPl.Rotate(aRotation);

                            foreach (uopHole hole in lHoles)
                            {

                                v1 = hole.CenterDXF.WithRespectToPlane(bPln, aPln);

                                bPl = (dxePolyline)aPl.Clone();
                                bPl.MoveTo(v1);
                                bPl.TFVSet("APPAN_SLOT", hole.Flag, hole.Value);
                                bPl.Value = lHoles.Count;

                                _rVal.AddAdditionalSegment(bPl, bAddCenterPt: true);


                            }
                        }
                    }





                }

                if (aAssy.DesignOptions.CrossBraces)
                {
                    lHoles = aHls.Item("CROSSBRACE").ToList;
                    foreach (uopHole hole in lHoles)
                    {
                        hole.Value = lHoles.Count;
                        bArc = _rVal.AdditionalSegments.AddArc(aPln.Vector(hole.X - bPln.X, hole.Y - bPln.Y), hole.Radius);
                        bArc.TFVSet(hole.Tag, hole.Flag, hole.Value);
                        _rVal.AdditionalSegments.AddPoint(bArc, aHandlePt: dxxEntDefPointTypes.Center);
                        if (aNutPl != null)
                        {
                            aPl = (dxePolyline)aNutPl.Clone();
                            aPl.Move(bArc.X, bArc.Y);
                            _rVal.AddAdditionalSegment(aPl, bAddCenterPt: true);

                        }

                    }
                }


            }
            catch (Exception)
            {
                throw;
            }
            finally
            {


                _rVal.InsertionPt = aPln?.Origin;
            }
            return _rVal;
        }


        /// <summary>
        ///     returns a dxePolygon that is used to draw the plan view of the box
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aAssy" ></param>
        /// <param name="bOutLineOnly"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="bIncludeSpouts"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="aCenterLineLength"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aScale"></param>
        /// <param name="bIncludeEndPlates"> </param> 
        /// <param name="bIncludeEndSupports"></param>
        /// <param name="bIncludeShelfAngles"></param>
        /// <returns></returns>
        public static dxePolygon DCBox_View_Plan(
            mdDowncomerBox aPart, mdTrayAssembly 
            aAssy, bool bOutLineOnly = false, 
            bool bSuppressHoles = false,
            bool bIncludeSpouts = false, 
            bool bShowObscured = false, 
            double aCenterLineLength = 0, 
            iVector aCenter = null, 
            double aRotation = 0, 
            string aLayerName = "GEOMETRY", 
            double aScale = 1, bool 
            bIncludeEndPlates = false, 
            bool bIncludeEndSupports = false, 
            bool bIncludeShelfAngles = false, bool bIncludeStiffeners = false, bool bIncludeBaffles = false, bool bIncludeSupDefs = true)
        {
            dxePolygon _rVal = new dxePolygon() { LayerName = aLayerName };

            hdwHexNut rBottomNut;
            if (aPart == null) return _rVal;
            aAssy ??= aPart.GetMDTrayAssembly(aAssy);

            double thk = aPart.Thickness;
            double wd = aPart.Width / 2;
            bool fldov = aPart.FoldOverWeirs;
            double fldwd = fldov ? mdDowncomer.FoldOverMaxWidth : 0;
            string lt = bShowObscured ? dxfLinetypes.Invisible : "";
            double cX = aPart.X;
            double cY = aPart.Y;

            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation, aScale);
            dxfPlane bPln = aPart.Plane;

            dxeLine hcl = null;
            dxePolyline aNutPl = null;
            colDXFVectors verts;

            ULINE leftline = aPart.BoxLn(bLeft: true);
            ULINE rightline = aPart.BoxLn(bLeft: false);

            double leftT = leftline.MaxY - cY;
            double leftB = leftline.MinY - cY;
            double rightT = rightline.MaxY - cY;
            double rightB = rightline.MinY - cY;

            if(aAssy.ProjectType == uppProjectTypes.MDSpout)
            {
                bIncludeStiffeners = false;
                bIncludeBaffles = false;
                bIncludeSupDefs = false;
            }
            else
            {
                if (!aAssy.IsECMD) bIncludeBaffles = false;
            }

            try
            {
                double d1;
                if (bOutLineOnly)
                {
                    bIncludeEndPlates = false;
                    bIncludeEndSupports = false;
                    bIncludeShelfAngles = false;
                    bIncludeStiffeners = false;
                    if (!fldov)
                    {
                        verts = new colDXFVectors(aPln.Vector(wd, rightB), aPln.Vector(wd, rightT), aPln.Vector(-wd, leftT), aPln.Vector(-wd, leftB));
                    }
                    else
                    {
                        verts = new colDXFVectors(aPln.Vector(wd - thk + fldwd, rightB), aPln.Vector(wd - thk + fldwd, rightT), aPln.Vector(-wd + thk - fldwd, leftT), aPln.Vector(-wd + thk - fldwd, leftB));
                    }
                }
                else
                {
                    verts = new colDXFVectors();
                    lt = bIncludeEndSupports ? dxfLinetypes.Hidden : "";
                    if (!fldov)
                    {
                        verts.Add(aPln, -wd, leftB);
                        verts.Add(aPln, -wd, leftT);
                        verts.Add(aPln, -(wd - thk), leftT, aLineType: lt, aTag: "WALL1");
                        verts.Add(aPln, wd - thk, rightT, aTag: "WALL2");
                        verts.Add(aPln, wd, rightT);
                        verts.Add(aPln, wd, rightB);
                        verts.Add(aPln, wd - thk, rightB, aTag: "WALL1");
                        verts.Add(aPln, wd - thk, rightT, aTag: "WALL2");
                        verts.Add(aPln, wd - thk, rightB, aLineType: lt);

                        verts.Add(aPln, -(wd - thk), leftB);
                        verts.Add(aPln, -(wd - thk), leftT);
                        verts.Add(aPln, -(wd - thk), leftB);

                    }
                    else
                    {
                        d1 = fldwd - 2 * thk;
                        verts.Add(aPln, -wd - fldwd + thk, leftB);
                        verts.AddRelative(thk, aTag: "THK1", aPlane: aPln);
                        verts.AddRelative(d1, aTag: "THK2", aPlane: aPln);
                        verts.AddRelative(thk, aLineType: lt, aTag: "WALL1", aPlane: aPln);

                        verts.Add(aPln, wd - thk, rightB, aTag: "WALL2");

                        verts.AddRelative(thk, aTag: "THK3", aPlane: aPln);
                        verts.AddRelative(d1, aTag: "THK4", aPlane: aPln);
                        verts.AddRelative(thk, aPlane: aPln);

                        verts.AddRelative(aY: aPart.RightLength, aPlane: aPln);
                        verts.AddRelative(-thk, aTag: "THK4", aPlane: aPln);
                        verts.AddRelative(-d1, aTag: "THK3", aPlane: aPln);
                        verts.AddRelative(-thk, aLineType: lt, aTag: "WALL2", aPlane: aPln);
                        verts.Add(aPln, -wd + thk, leftT, aTag: "WALL1");
                        verts.AddRelative(-thk, aTag: "THK2", aPlane: aPln);
                        verts.AddRelative(-d1, aTag: "THK1", aPlane: aPln);
                        verts.AddRelative(-thk, aPlane: aPln);

                    }
                }
                _rVal.Vertices = verts;
                if (!bOutLineOnly)
                {

                    lt = bIncludeEndSupports ? dxfLinetypes.Hidden : "";
                    if (fldov)
                    {
                        _rVal.AddRelationsByTag("THK1", true, aLineType: dxfLinetypes.Hidden, aLayerName: aLayerName);
                        _rVal.AddRelationsByTag("THK2", true, aLineType: dxfLinetypes.Hidden, aLayerName: aLayerName);
                        _rVal.AddRelationsByTag("THK3", true, aLineType: dxfLinetypes.Hidden, aLayerName: aLayerName);
                        _rVal.AddRelationsByTag("THK4", true, aLineType: dxfLinetypes.Hidden, aLayerName: aLayerName);
                        _rVal.AdditionalSegments.AddLine(aPln.Vector(-(wd - thk), leftT), aPln.Vector(-(wd - thk), leftB), new dxfDisplaySettings(aLayerName), aTag: "WALL", aFlag: "LEFT");
                        _rVal.AdditionalSegments.AddLine(aPln.Vector(wd - thk, leftB), aPln.Vector(wd - thk, rightT), new dxfDisplaySettings(aLayerName), aTag: "WALL", aFlag: "RIGHT");
                    }

                    //_rVal.AdditionalSegments.AddLine(aPln.Vector(-(wd - thk), leftT), aPln.Vector(wd - thk, rightT), new dxfDisplaySettings(aLayerName, aLinetype: lt), aTag: "ENDLINE", aFlag: "TOP");
                    //_rVal.AdditionalSegments.AddLine(aPln.Vector(-(wd - thk), leftB), aPln.Vector(wd - thk, rightB), new dxfDisplaySettings(aLayerName, aLinetype: lt), aTag: "ENDLINE", aFlag: "BOTTOM");

                }

                dxfVector v1;
                if (bIncludeSpouts)
                {
                    colMDSpoutGroups aSGs = aPart.SpoutGroups(aAssy);
                    mdSpoutGroup aSG = null;
                    colDXFEntities blkEnts = null;
                    dxeLine zAxs = aPln.ZAxis();
                    dxeLine xAxs = aPln.XAxis();
                    dxeLine yAxs = aPln.YAxis();
                    v1 = new dxfVector(aPln.X - bPln.X, aPln.Y - bPln.Y);

                    for (int i = 1; i <= aSGs.Count; i++)
                    {
                        aSG = aSGs.Item(i);
                        if (aSG.IsVirtual || aSG.SpoutCount(aAssy) <= 0) continue;
                        //                    Set v1 = aSG.Center.WithRespectToPlane(bPln)
                        blkEnts = aSG.BlockEntities("SPOUTS", dxxColors.ByLayer, dxfLinetypes.ByLayer,  bSuppressInstances: true);

                        blkEnts.Translate(v1);

                        if (aRotation != 0) blkEnts.RotateAbout(zAxs, aRotation);
                        _rVal.AdditionalSegments.Append(blkEnts, aTag: "SPOUT", bAddClones: true, aFlag: aSG.Handle);
                        if (aAssy.DesignFamily.IsStandardDesignFamily() && Math.Round(aSG.Y, 1) != aPart.Y)
                            _rVal.AdditionalSegments.AppendMirrors(blkEnts, xAxs, aTag: "SPOUT", aFlag: aSG.Handle);

                    }
                }

                if (!bSuppressHoles && !bOutLineOnly)
                {
                    if (aCenterLineLength > 0) hcl = new dxeLine() { Linetype = "Center", LayerName = aLayerName };
                    double mxY = aPart.LimitLn(bTop: true).MidPt.Y;
                    dxeLine ln1 = new dxeLine() { Linetype = dxfLinetypes.Hidden, LayerName = aLayerName };

                    string holes = string.Empty; // "END ANGLE";
                    uopHoleArray aHls = new uopHoleArray(aPart.GenHolesV(aAssy, bSuppressSpouts: true, aTags: holes));

                    if (aPart.WeldedBottomNuts || aAssy.DesignOptions.BottomInstall)
                    {
                        string tgs = aHls.Names();

                        if (mzUtils.ListContains("APPAN_HOLE", tgs) || mzUtils.ListContains("CROSSBRACE", tgs))
                        {
                            rBottomNut = aPart.SmallBolt(true).GetNut(1);
                            aNutPl = uopPolygons.HexNutPlan(rBottomNut.HardwareStructure, aInsertionPt: dxfVector.Zero, aLayerName: aLayerName);
                            aNutPl.Tag = "BOTTOM_NUT";
                        }
                    }

                    foreach (uopHoles Hls in aHls)
                    {


                        for (int i = 1; i <= Hls.Count; i++)
                        {
                            uopHole aH = Hls.Item(i);
                            v1 = aH.CenterDXF.WithRespectToPlane(bPln);
                            dxfVector v2 = aPln.Vector(v1.X, v1.Y, aTag: aH.Tag, aFlag: aH.Flag); //the tag flag is passed to the created entity (circle or slot polyline)

                            double rad = aH.Radius;
                            //Set aH.Center = aH.CenterDXF.WithRespectToPlane(bPln, aPln, 0)
                            if (aH.ExtrusionDirection == "0,0,1")
                            {
                                //on the plane
                                lt = bShowObscured && Math.Abs(v1.Y) > mxY ? dxfLinetypes.Hidden : "";

                                dxePolyline aPl;
                                if (aH.HoleType == uppHoleTypes.Hole)
                                {
                                    _rVal.AdditionalSegments.AddArc(v2, rad, aDisplaySettings: new dxfDisplaySettings(aLayerName, aLinetype: lt));

                                    if (aNutPl != null)
                                    {
                                        aPl = (dxePolyline)aNutPl.Clone();
                                        aPl.LayerName = aLayerName;
                                        aPl.Translate(v2);
                                        _rVal.AdditionalSegments.Add(aPl, bAddClone: false);

                                    }
                                }
                                else
                                {

                                    if (aRotation != 0) aH.Rotation += aRotation;

                                    aH.CenterDXF = v2;
                                    aPl = (dxePolyline)aH.BoundingEntity(aLayerName);
                                    if (aH.Tag == "APPAN_SLOT" && bShowObscured)
                                    {
                                        aPl.Rotate(180);
                                        aPl.Closed = false;
                                        dxfVector v3 = aPl.LastVertex();
                                        v3.VertexRadius = 0;
                                        d1 = (aH.Length - aH.Diameter - 20 / 25.4) / 2;
                                        v3 = v3.Clone();
                                        v3 += aPl.Plane.XDirection * d1;
                                        v3.VertexRadius = 0;
                                        aPl.Vertices.Add(v3);


                                        v3 = aPl.Vertex(1);
                                        //v1.VertexRadius = 0;
                                        v3 = aPl.Vertices.Add(v3 + aPl.Plane.XDirection * -d1);
                                        v3.VertexRadius = 0;
                                        _rVal.AdditionalSegments.Add(aPl, aTag: "APPAN_SLOT");
                                        _rVal.AdditionalSegments.Add(new dxeLine(aPl.Vertices.FirstVector(), aPl.Vertices.LastVector()) { Linetype = dxfLinetypes.Hidden }, aTag: "APPAN_SLOT", aFlag: "TABLINE");

                                    }
                                    else
                                    {
                                        _rVal.AdditionalSegments.Add(aPl);
                                    }


                                }
                            }
                            else
                            {
                                d1 = 0.5 * (aH.Length != 0 ? aH.Length : aH.Diameter);
                                //hole in wall
                                ln1.TFVSet(aH.Tag, aH.Flag);
                                ln1.SetVectors(aPln.Vector(v1.X - thk / 2, v1.Y + d1), aPln.Vector(v1.X + thk / 2, v1.Y + d1));
                                _rVal.AdditionalSegments.Add(ln1, bAddClone: true);
                                ln1.SetVectors(aPln.Vector(v1.X - thk / 2, v1.Y - d1), aPln.Vector(v1.X + thk / 2, v1.Y - d1));
                                _rVal.AdditionalSegments.Add(ln1, bAddClone: true);
                                if (aCenterLineLength > 0)
                                {
                                    hcl.SetVectors(aPln.Vector(v1.X - aCenterLineLength / 2, v1.Y), aPln.Vector(v1.X + aCenterLineLength / 2, v1.Y));
                                    _rVal.AdditionalSegments.Add(hcl, bAddClone: true);
                                }
                            }
                        }
                    }

                }

                colDXFEntities subents = null;
                if (bIncludeEndPlates && !bOutLineOnly)
                {
                    mdEndPlate ep = aPart.EndPlate(bTop: true);

                    v1 = new dxfVector(ep.Center).WithRespectToPlane(bPln, aTransferPlane: aPln);
                    dxePolygon pg = ep.View_Plan(bApplyFillets: true, aCenter: v1, aRotation: aRotation, aLayerName: aLayerName, aScale: aScale);
                    subents = pg.SubEntities();
                    _rVal.AdditionalSegments.Append(subents);

                    ep = aPart.EndPlate(bTop: false);
                    v1 = new dxfVector(ep.Center).WithRespectToPlane(bPln, aTransferPlane: aPln);
                    pg = ep.View_Plan(bApplyFillets: true, aCenter: v1, aRotation: aRotation, aLayerName: aLayerName, aScale: aScale);
                    subents = pg.SubEntities();
                    _rVal.AdditionalSegments.Append(subents, aTag: "END PLATES");

                }
                if (bIncludeEndSupports && !bOutLineOnly)
                {
                    mdEndSupport es = aPart.EndSupport(bTop: true);
                    v1 = new dxfVector(es.Center).WithRespectToPlane(bPln, aTransferPlane: aPln);
                    dxePolygon pg = es.View_Plan(bSuppressHoles: false, aCenter: v1, aRotation: aRotation, aLayerName: aLayerName, aScale: aScale);
                    subents = pg.SubEntities();
                    _rVal.AdditionalSegments.Append(subents);
                    es = aPart.EndSupport(bTop: false);
                    v1 = new dxfVector(es.Center).WithRespectToPlane(bPln, aTransferPlane: aPln);
                    pg = es.View_Plan(bSuppressHoles: false, aCenter: v1, aRotation: aRotation, aLayerName: aLayerName, aScale: aScale);
                    subents = pg.SubEntities();
                    _rVal.AdditionalSegments.Append(subents, aTag: "END SUPPORTS");

                }

                if (bIncludeShelfAngles && !bOutLineOnly)
                {
                    mdShelfAngle sa = aPart.ShelfAngle(bLeft: true);
                    v1 = new dxfVector(sa.Center).WithRespectToPlane(bPln, aTransferPlane: aPln);

                    dxePolygon pg = sa.View_Plan(aCenter: v1, bShowObscured: bShowObscured, aLayerName: aLayerName);
                    subents = pg.SubEntities();
                    _rVal.AdditionalSegments.Append(subents);

                    sa = aPart.ShelfAngle(bLeft: false);

                    v1 = new dxfVector(sa.Center).WithRespectToPlane(bPln, aTransferPlane: aPln);
                    pg = sa.View_Plan(aCenter: v1, bShowObscured: bShowObscured, aLayerName: aLayerName);
                    subents = pg.SubEntities();
                    _rVal.AdditionalSegments.Append(subents, aTag: "SHELVES");
                }


                if (bIncludeStiffeners && !bOutLineOnly)
                {
                    List<mdStiffener> stiffeners = aPart.Stiffeners(bApplyInstances: true);
                    if(stiffeners.Count > 0)
                    {
                        mdStiffener stf = stiffeners[0];
                        dxePolygon pg = stf.View_Plan(aPart,aAssy,false,true,uopVector.Zero,0,aLayerName:"STIFFENERS");
                        colDXFEntities ents = pg.SubEntities();
                        if (aRotation != 0) ents.RotateAbout(pg.Plane.ZAxis(100), aRotation);
                        dxfVector v0 = new dxfVector(stf.Center).WithRespectToPlane(bPln, aTransferPlane: aPln);
                        uopVectors pts = stf.Instances.MemberPoints(stf.Center);
                        foreach(uopVector u1 in pts)
                        {
                            v1 = new dxfVector(u1).WithRespectToPlane(bPln, aTransferPlane: aPln);
                            subents = ents.Clone();
                            subents.Translate(v1 - v0);
                            _rVal.AdditionalSegments.Append(subents, aTag: "STIFFENERS");
                        }
                    }
                    
                }
                if (bIncludeBaffles && !bOutLineOnly)
                {

                    List<mdBaffle> baffles = mdPartGenerator.DeflectorPlates_DCBox(aPart, aAssy, bSuppressInstances:true ); 
                    foreach(mdBaffle b in baffles)
                    {
                        dxePolygon pg = b.View_Plan( aAssy, false, uopVector.Zero, 0, aLayerName: "DEFLECTOR PLATES");
                        v1 = new dxfVector(b.Center).WithRespectToPlane(bPln, aTransferPlane: aPln);
                        pg = b.View_Plan(aAssy, false, v1, 0, aLayerName: "DEFLECTOR PLATES"); 
                        subents = pg.SubEntities();
                        _rVal.AdditionalSegments.Append(subents, aTag: "BAFFLES");
                    }
                }

                if (bIncludeSupDefs && !bOutLineOnly && aPart.HasSupplementalDeflector )
                {
                    mdSupplementalDeflector supdef = aPart.SupplementalDeflector;
                    if(supdef != null)
                    {
                        v1 = new dxfVector(supdef.Center).WithRespectToPlane(bPln, aTransferPlane: aPln);
                        dxePolygon pg = supdef.View_Plan(aAssy,v1, aRotation,aLayerName:aLayerName);
                        subents = pg.SubEntities();
                        _rVal.AdditionalSegments.Append(subents, aTag: "SUPPLEMENTAL DEFLECTORS");

                    }
                }
                    return _rVal;
            }
            catch
            {
                return _rVal;
            }
            finally
            {
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
                _rVal.BlockName = $"DCBOX_{aPart.PartNumber}_PLAN";

                if (aScale != 1) _rVal.Rescale(aScale, aPln.Origin);
                



            }


        }

        /// <summary>
        ///#1the subject part
        ///#2the parent downcomer of the part
        ///#3the parent tray of the part
        ///^returns a dxePolygon that is used to draw the elevation view of the box
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aDC"></param>
        /// <param name="aAssy"></param>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aSuppressHoles"></param>
        /// <param name="aIncludeShelves"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="rBottomNut"></param>
        /// <returns></returns>
        public static dxePolygon DCBox_View_Elevation(mdDowncomerBox aPart, mdTrayAssembly aAssy = null, bool bSuppressFillets = false, bool aSuppressHoles = false, bool aIncludeShelves = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon();


            if (aPart == null) return _rVal;

            dxePolyline aPl;
            double thk = aPart.Thickness;
            dxfVector v1;
            double ht = aPart.Height;
            double cX = aPart.X;
            double cY = aPart.Y;
            double wd = aPart.Width;
            double hw = aPart.WeirHeight;
            bool fldvr = aPart.FoldOverWeirs;

            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);

            try
            {

                double fht = aPart.FoldOverHeight;
                double fwd = mdDowncomer.FoldOverMaxWidth;
                double frad = fwd / 2;
                double gap = fwd - thk * 2;

                dxfVector v2 = null;
                dxfVector v3 = null;
                dxfPlane bPln = null;
                uopHoleArray aHls = null;
                uopHoles Hls = null;
                uopHole aHl = null;
                string tgs = string.Empty;
                dxePolyline aNutPl = null;
                hdwHexNut nut = !aPart.WeldedBottomNuts ? null : aPart.SmallBolt("WELDED NUT").GetNut(); ;

                if (fldvr)
                {
                    double irad = gap / 2;
                    double d1 = fht - frad;
                    ht -= frad;
                    _rVal = (dxePolygon)dxfPrimatives.CreateChannel_EndView(aPln.Origin, ht, wd, thk, aSuppressFillets: bSuppressFillets, aRotation: -90, aPlane: aPln, aReturnPolygon: true);
                    _rVal.Project(aPln.YDirection, -ht + hw - frad);

                    v1 = _rVal.Vertices.LastVector();
                    v1.TFVSet("FOLDOVER", "RIGHT");
                    v3 = bSuppressFillets ? _rVal.Vertices.Item(4) : _rVal.Vertices.Item(6);
                    v1.VertexRadius = -frad;
                    //v2 = v1.Clone();

                    v2 = aPln.VectorRelative(v1, frad, frad);
                    v2.VertexRadius = -frad;
                    _rVal.Vertices.Add(v2, aTag: "FOLDOVER", aFlag: "RIGHT");

                    v2 = aPln.VectorRelative(v2, frad, -frad);
                    _rVal.Vertices.Add(v2, aTag: "FOLDOVER", aFlag: "RIGHT");

                    v2 = aPln.VectorRelative(v2, 0, -d1);
                    _rVal.Vertices.Add(v2, aTag: "FOLDOVER", aFlag: "RIGHT");

                    v2 = aPln.VectorRelative(v2, -thk, 0);
                    _rVal.Vertices.Add(v2, aTag: "FOLDOVER", aFlag: "RIGHT");

                    v2 = aPln.VectorRelative(v2, 0, d1);
                    v2.VertexRadius = irad;
                    _rVal.Vertices.Add(v2, aTag: "FOLDOVER", aFlag: "RIGHT");

                    v2 = aPln.VectorRelative(v2, -irad, irad);
                    v2.VertexRadius = irad;
                    _rVal.Vertices.Add(v2, aTag: "FOLDOVER", aFlag: "RIGHT");

                    //================ OTHER SIDE

                    v3.VertexRadius = irad;
                    v2.TFVSet(aTag: "FOLDOVER", aFlag: "LEFT");
                    v2 = aPln.VectorRelative(v3, -irad, irad);
                    v2.VertexRadius = irad;
                    v3 = _rVal.Vertices.AddAfter(v2, v3, aTag: "FOLDOVER", aFlag: "LEFT");

                    v2 = aPln.VectorRelative(v3, -irad, -irad);
                    v3 = _rVal.Vertices.AddAfter(v2, v3, aTag: "FOLDOVER", aFlag: "LEFT");

                    v2 = aPln.VectorRelative(v3, 0, -d1);
                    v3 = _rVal.Vertices.AddAfter(v2, v3, aTag: "FOLDOVER", aFlag: "LEFT");


                    v2 = aPln.VectorRelative(v3, -thk, 0);
                    v3 = _rVal.Vertices.AddAfter(v2, v3, aTag: "FOLDOVER", aFlag: "LEFT");

                    v2 = aPln.VectorRelative(v3, 0, d1);
                    v2.VertexRadius = -frad;
                    v3 = _rVal.Vertices.AddAfter(v2, v3, aTag: "FOLDOVER", aFlag: "LEFT");

                    v2 = aPln.VectorRelative(v3, frad, frad);
                    v2.VertexRadius = -frad;
                    v3 = _rVal.Vertices.AddAfter(v2, v3, aTag: "FOLDOVER", aFlag: "LEFT");

                    v2 = aPln.VectorRelative(v3, frad, -frad);
                    v3 = _rVal.Vertices.AddAfter(v2, v3, aTag: "FOLDOVER", aFlag: "LEFT");

                }
                else
                {
                    _rVal = (dxePolygon)dxfPrimatives.CreateChannel_EndView(aPln.Origin, ht, wd, thk, aSuppressFillets: bSuppressFillets, aRotation: -90, aPlane: aPln, aReturnPolygon: true, aXOffset: ht - hw);
                    //        DCBox_View_Elevation.Vertices.Project aPln.YDirection, ht - wht
                }
                wd = 0.5 * wd;


                if (aSuppressHoles)
                {


                    if (aPart.WeldedBottomNuts)
                    {
                        tgs = "CROSSBRACE,APPAN_HOLE";
                        aHls = aPart.GenHoles(aAssy, aTags: tgs);

                        if (aHls.Count > 0)
                        {


                            aNutPl = uopPolygons.HexNutProfile(nut.HardwareStructure, aPln.Vector(aY: -aPart.Height + aPart.WeirHeight + thk), dxxOrthoDirections.Up, aLayerName: aLayerName, aPlane: aPln);
                            _rVal.AdditionalSegments.Add(aNutPl, aTag: "BOTTOM_NUT");

                        }
                    }


                }
                else
                {
                    bPln = aPart.Plane;
                    tgs = "END ANGLE,STARTUP,CROSSBRACE,APPAN_HOLE,APPAN_SLOT";
                    aHls = aPart.GenHoles(aAssy, aTags: tgs);
                    tgs = aHls.Names();

                    if (mzUtils.ListContains("END ANGLE", tgs))
                    {
                        Hls = aHls.Item("END ANGLE");
                        aHl = Hls.Item(1);
                        frad = aHl.Radius;

                        _rVal.AdditionalSegments.Add(aPln.CreateLine(-wd, aHl.Z + frad, -wd + thk, aHl.Z + frad, aLineType: dxfLinetypes.Hidden));
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(-wd, aHl.Z - frad, -wd + thk, aHl.Z - frad, aLineType: dxfLinetypes.Hidden));

                        _rVal.AdditionalSegments.Add(aPln.CreateLine(wd - thk, aHl.Z + frad, wd, aHl.Z + frad, aLineType: dxfLinetypes.Hidden));
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(wd - thk, aHl.Z - frad, wd, aHl.Z - frad, aLineType: dxfLinetypes.Hidden));
                    }


                    if (mzUtils.ListContains("STARTUP", tgs))
                    {
                        Hls = aHls.Item("STARTUP");
                        aHl = Hls.Item(1);
                        frad = aHl.Radius;
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(-wd, aHl.Z + frad, -wd + thk, aHl.Z + frad, aLineType: dxfLinetypes.Hidden));
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(-wd, aHl.Z - frad, -wd + thk, aHl.Z - frad, aLineType: dxfLinetypes.Hidden));

                        _rVal.AdditionalSegments.Add(aPln.CreateLine(wd - thk, aHl.Z + frad, wd, aHl.Z + frad, aLineType: dxfLinetypes.Hidden));
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(wd - thk, aHl.Z - frad, wd, aHl.Z - frad, aLineType: dxfLinetypes.Hidden));

                    }
                    if (mzUtils.ListContains("CROSSBRACE", tgs))
                    {
                        Hls = aHls.Item("CROSSBRACE");
                        aHl = Hls.Item(1);
                        if (aHl != null)
                        {
                            frad = aHl.Radius;
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(-frad, aHl.Z + thk / 2, -frad, aHl.Z - thk / 2, aLineType: dxfLinetypes.Hidden));
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(frad, aHl.Z + thk / 2, frad, aHl.Z - thk / 2, aLineType: dxfLinetypes.Hidden));
                            if (nut != null)
                            {
                                aNutPl = (dxePolyline)uopPolygons.HexNutProfile(nut.HardwareStructure, aPln.Vector(aY: aHl.Z + thk / 2), dxxOrthoDirections.Up, aLayerName: aLayerName, aPlane: aPln);
                                _rVal.AdditionalSegments.Add(aNutPl, aTag: "BOTTOM_NUT");
                            }

                        }
                    }
                    else if (mzUtils.ListContains("APPAN_HOLE", tgs))
                    {

                        Hls = aHls.Item("APPAN_HOLE");
                        aHl = Hls.Item(1);
                        if (aHl != null)
                        {
                            frad = aHl.Radius;
                            v1 = aHl.CenterDXF.WithRespectToPlane(bPln);
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - frad, v1.Z + thk / 2, v1.X - frad, v1.Z - thk / 2, aLineType: dxfLinetypes.Hidden));
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X + frad, v1.Z + thk / 2, v1.X + frad, v1.Z - thk / 2, aLineType: dxfLinetypes.Hidden));
                            if (nut != null)
                            {
                                aNutPl = (dxePolyline)uopPolygons.HexNutProfile(nut.HardwareStructure, aPln.Vector(aY: aHl.Z + thk / 2), dxxOrthoDirections.Up, aLayerName: aLayerName, aPlane: aPln);
                                _rVal.AdditionalSegments.Add(aNutPl, aTag: "BOTTOM_NUT");
                            }
                        }
                    }
                    if (mzUtils.ListContains("APPAN_SLOT", tgs))
                    {
                        Hls = aHls.Item("APPAN_SLOT");

                        aHl = Hls.GetByPoint(dxxPointFilters.AtMinX);
                        if (aHl != null)
                        {
                            frad = aHl.Length / 2;
                            v1 = aHl.CenterDXF.WithRespectToPlane(bPln);
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - frad, v1.Z + thk / 2, v1.X - frad, v1.Z - thk / 2, aLineType: dxfLinetypes.Hidden));
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X + frad, v1.Z + thk / 2, v1.X + frad, v1.Z - thk / 2, aLineType: dxfLinetypes.Hidden));
                        }
                        aHl = Hls.GetByPoint(dxxPointFilters.AtMaxX);
                        if (aHl != null)
                        {
                            frad = aHl.Radius;
                            frad = aHl.Length / 2;
                            v1 = aHl.CenterDXF.WithRespectToPlane(bPln);
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - frad, v1.Z + thk / 2, v1.X - frad, v1.Z - thk / 2, aLineType: dxfLinetypes.Hidden));
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X + frad, v1.Z + thk / 2, v1.X + frad, v1.Z - thk / 2, aLineType: dxfLinetypes.Hidden));
                        }

                    }

                }


                //        hls.Append goDXFUtils.HolesAsViewedFrom(pHoles, "Y"), False

                //        Set pHoles = GenerateHoles(aAssy, "STARTUP").GetEntities(dxfGreaterThanX, X, False)
                //        If pHoles.Count > 0 Then hls.Add pHoles.HoleItem(1).AsViewedFrom("Y")
                //         Set pHoles = GenerateHoles(aAssy, "APPAN").GetByEntityType(dxf_Hole)
                //        If pHoles.Count > 0 Then
                //            hls.Add pHoles.HoleItem(1).AsViewedFrom("Y")

                //            Set pHoles = GenerateHoles(aAssy, "APPAN").GetByEntityType(dxf_Slot)
                //            If pHoles.Count > 0 Then hls.Add pHoles.HoleItem(1).AsViewedFrom("Y")
                //            If pHoles.Count > 0 Then hls.Add pHoles.HoleItem(2).AsViewedFrom("Y")
                //        End If

                //        Set pHoles = GenerateHoles(aAssy, "CROSSBRACE")
                //        If pHoles.Count > 0 Then
                //            hls.Add pHoles.HoleItem(1).AsViewedFrom("Y")
                //        End If
                //        hls.Move aXOffset, aYOffset
                //        utils_AddHolesToPGON DCBox_View_Elevation, hls, uopViewTop
                //    End If


            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
                _rVal.LayerName = aLayerName;

                if (aIncludeShelves)
                {
                    v1 = aPln.Vector(-wd, -0.5);
                    aPl = (dxePolyline)uopGlobals.UopGlobals.Primatives.Angle_End(v1, 1, thk, 1, bSuppressFillets, aPlane: aPln);
                    aPl.LayerName = _rVal.LayerName;
                    _rVal.AdditionalSegments.Add(aPl, aTag: "SHELF", aFlag: "LEFT");

                    v1 = aPln.Vector(wd, -0.5);
                    aPl = (dxePolyline)uopGlobals.UopGlobals.Primatives.Angle_End(v1, -1, thk, 1, bSuppressFillets, aPlane: aPln);
                    aPl.LayerName = _rVal.LayerName;
                    _rVal.AdditionalSegments.Add(aPl, aTag: "SHELF", aFlag: "RIGHT");

                }
            }

            return _rVal;
        }

        /// <summary>
        /// #1the subject part
        ///#2the parent tray of the part
        ///#3the parent downcomer of the part
        ///#4flag to add hidden lines where the profile is obscured
        ///^returns a dxePolygon that is used to draw the profile view of the box
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aAssy"></param>
        /// <param name="aDC"></param>
        /// <param name="aShowObscured"></param>
        /// <param name="aShowLongSide"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="rBottomNut"></param>
        /// <returns></returns>
        public static dxePolygon DCBox_View_Profile(mdDowncomerBox aPart, mdTrayAssembly aAssy, mdDowncomer aDC = null, bool aShowObscured = false, bool aShowLeftSide = true, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", hdwHexNut rBottomNut = null, Double aScale =1)
        {
            rBottomNut = null;
            dxePolygon _rVal = new dxePolygon();
            if (aPart == null) return _rVal;

            aAssy ??= aPart.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;
            if (aDC == null) aDC = aAssy.Downcomers.Item(aPart.DowncomerIndex);
            if (aDC == null) return _rVal;
            double thk = aDC.Thickness;
            double ht = aPart.Height;
            double cX = aPart.Y;
            double cY = aPart.Z;
            var boxLine = aPart.BoxLine(aShowLeftSide);
            double lengthBelowCenter = aPart.Y - boxLine.MinY; // This is the length below the center in the vertical view
            double lengthAboveCenter = boxLine.MaxY - aPart.Y; // This is the length above the center in the vertical view
            double lengthToTheLeftInView = aShowLeftSide ? lengthAboveCenter : lengthBelowCenter; // This is the length to the left of the center in profile/horizontal view
            double lengthToTheRightInView = aShowLeftSide ? lengthBelowCenter : lengthAboveCenter; // This is the length to the right of the center in profile/horizontal view

            string lt = string.Empty;
            string lt2 = string.Empty;
            dxfVector v1 = dxfVector.Zero;
            double dkthk = aDC.DeckThickness;
            double hw = aDC.How + dkthk;
            mdShelfAngle shelf = aPart.ShelfAngle(bLeft: aShowLeftSide);
            bool shelfExtendsBoxAbove = shelf.EdgeLn.MaxY >= aPart.BoxLine(aShowLeftSide).MaxY; // Shows if the shelf extrudes the box from above
            bool shelfExtendsBoxBelow = shelf.EdgeLn.MinY <= aPart.BoxLine(aShowLeftSide).MinY; // Shows if the shelf extrudes the box from below
            bool shelfExtendsBoxLeftInView = aShowLeftSide ? shelfExtendsBoxAbove : shelfExtendsBoxBelow;
            bool shelfExtendsBoxRightInView = aShowLeftSide ? shelfExtendsBoxBelow : shelfExtendsBoxAbove;

            uopHoleArray aHls = new uopHoleArray(aPart.GenHolesV(aAssy, bSuppressSpouts: true));


            dxfPlane bPln = aDC.Plane;
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
            

            dxePolyline aNutPl = null;
            colDXFVectors verts = new colDXFVectors();
            try
            {
                bool bBreakSA = shelfExtendsBoxAbove || shelfExtendsBoxBelow;
                if (aShowObscured)
                {
                    lt = dxfLinetypes.Hidden;
                    if (bBreakSA) lt2 = dxfLinetypes.Hidden;

                }


                verts.Add(aPln, -lengthToTheLeftInView, hw, aTag: "TOP");
                verts.Add(aPln, -lengthToTheLeftInView, dkthk + 1, aLineType: lt, aTag: "EA_TOP");
                verts.Add(aPln, -lengthToTheLeftInView, dkthk, aTag: "EA_BOT");
                if (shelfExtendsBoxLeftInView)
                {
                    verts.Add(aPln, -lengthToTheLeftInView, 0, aLineType: lt2, aTag: "SA_TOP");
                    verts.Add(aPln,-lengthToTheLeftInView, -1, aTag: "SA_BOT");
                }

                verts.Add(aPln, -lengthToTheLeftInView, -(ht - hw), aTag: "BOT");

                verts.Add(aPln, lengthToTheRightInView, -(ht - hw), aTag: "BOT");
                if (shelfExtendsBoxRightInView)
                {
                    verts.Add(aPln, lengthToTheRightInView, -1, aLineType: lt2, aTag: "SA_BOT");
                    verts.Add(aPln, lengthToTheRightInView, 0, aTag: "SA_TOP");
                }

                verts.Add(aPln, lengthToTheRightInView, dkthk, aLineType: lt, aTag: "EA_BOT");
                verts.Add(aPln, lengthToTheRightInView, dkthk + 1, aTag: "TOP");
                verts.Add(aPln, lengthToTheRightInView, hw, aTag: "TOP");

                _rVal.Vertices = verts;

                _rVal.AdditionalSegments.Add(aPln.CreateLine(-lengthToTheLeftInView, -(ht - hw) + thk, lengthToTheRightInView, -(ht - hw) + thk, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));

                if (aPart.FoldOverWeirs)
                {
                    _rVal.AdditionalSegments.Add(aPln.CreateLine(-lengthToTheLeftInView, hw - thk, lengthToTheRightInView, hw - thk, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));

                    _rVal.AdditionalSegments.Add(aPln.CreateLine(-lengthToTheLeftInView, hw - aDC.FoldOverHeight, lengthToTheRightInView, hw - aDC.FoldOverHeight, aLayerName: aLayerName));

                }


                dxeLine ln1 = new dxeLine() { LayerName = aLayerName, Linetype = dxfLinetypes.Hidden };

                if (aDC.WeldedBottomNuts || aAssy.DesignOptions.BottomInstall)
                {
                    string tgs = aHls.Names();

                    if (mzUtils.ListContains("APPAN_HOLE", tgs) || mzUtils.ListContains("CROSSBRACE", tgs))
                    {
                        rBottomNut = aAssy.SmallBolt("APPAN_NUT").GetNut();
                        aNutPl = uopPolygons.HexNutProfile(rBottomNut.HardwareStructure, null, aLinetype: dxfLinetypes.Hidden, aLayerName: aLayerName, aPlane: aPln);
                        aNutPl.Tag = "BOTTOM_NUT";

                    }
                }

                double direction = aShowLeftSide ? -1 : 1;
                for (int j = 1; j <= aHls.Count; j++)
                {
                    uopHoles Hls = aHls.Item(j);
                    for (int i = 1; i <= Hls.Count; i++)
                    {
                        uopHole aHl = Hls.Item(i);
                        v1 = aHl.CenterDXF.WithRespectToPlane(aPart.Plane, bMaintainZ: true);
                        if ((aShowLeftSide && Math.Round(v1.X, 4) <= 0) || (!aShowLeftSide && Math.Round(v1.X, 4) >= 0))
                        {
                            double rad = aHl.Radius;
                            v1 = aPln.Vector(v1.Y * direction, v1.Z);
                            if (aHl.ExtrusionDirection == "0,0,1")
                            {
                                //holes in the bottom
                                ln1.SetCoordinates2D(v1.X - rad, v1.Y + thk / 2, v1.X - rad, v1.Y - thk / 2);
                                _rVal.AdditionalSegments.Add(ln1, bAddClone: true, aTag: aHl.Tag);

                                ln1.SetCoordinates2D(v1.X + rad, v1.Y + thk / 2, v1.X + rad, v1.Y - thk / 2);
                                _rVal.AdditionalSegments.Add(ln1, bAddClone: true, aTag: aHl.Tag);

                                if (aHl.HoleType == uppHoleTypes.Hole)
                                {
                                    if (aNutPl != null)
                                    {
                                        dxePolyline pl = (dxePolyline)aNutPl.Clone();
                                        pl.Move(v1.X, v1.Y + thk / 2);
                                        _rVal.AdditionalSegments.Add(pl);
                                    }
                                }


                            }
                            else
                            {
                                if (aHl.HoleType == uppHoleTypes.Hole)
                                {
                                    dxeArc aArc = _rVal.AdditionalSegments.AddArc(v1.X, v1.Y, rad);
                                    aArc.TFVSet(aHl.Tag, aHl.Flag, aHl.Value);
                                    if (aShowObscured)
                                    {
                                        if (aHl.Tag == "ENDPLATE")
                                        {
                                            if (aHl.Elevation > 0 & aHl.Elevation < 1)
                                            {
                                                aArc.Linetype = dxfLinetypes.Hidden;
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    dxePolyline aPill = (dxePolyline)_rVal.AdditionalSegments.Add(dxfPrimatives.CreatePill(v1, aHl.Length, 2 * rad, 0, aPlane: aPln));
                                    aPill.TFVSet(aHl.Tag, aHl.Flag);
                                    aPill.LayerName = aLayerName;
                                    if (aShowObscured)
                                    {
                                        if (aHl.Tag == "END ANGLE") aPill.Linetype = dxfLinetypes.Hidden;

                                    }
                                }

                            }
                        }


                        //            .Holes.Add aHl
                    }
                }
                return _rVal;
            }
            catch
            {
                return _rVal;
            }
            finally
            {
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
                if (aScale != 1) _rVal.Rescale(aScale, aPln.Origin);
                _rVal.LayerName = aLayerName;



            }

        }
        /// <summary>
        ///  //#1scale factor to apply to the returned polygon
        //^returns a dxePolygon that is used to draw the elevation view of the angle

        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon SpliceAngle_View_Elevation(mdSpliceAngle aPart, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {

            if (aPart == null) return new dxePolygon();

            double thk = aPart.Thickness;
            double ht = aPart.Height;
            double cX = aPart.X;
            double cY = aPart.Z;
            double lg = aPart.Length / 2;
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
            uopHoles Hls = null;
            uopHoleArray aHls = aPart.GenHoles();
            string xVals = string.Empty;
            uopHole aHl = null;
            dxfVector v1 = null;
            dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y, aPart.Z));
            double rad = 0;
            bool bmancntr = aPart.PartType == uppPartTypes.ManwaySplicePlate;
            double jd = aPart.JoggleDepth / 2;
            colDXFVectors verts = new colDXFVectors();

            if (!bmancntr)
            {
                verts.Add(aPln, -lg, -thk);
                verts.Add(aPln, lg, -thk);
                verts.Add(aPln, lg, -ht);
                verts.Add(aPln, -lg, -ht);
                verts.Add(aPln, -lg, 0);
                verts.Add(aPln, lg, 0);
                verts.Add(aPln, lg, -thk);
            }
            else
            {
                verts.Add(aPln, -lg, -jd);
                verts.Add(aPln, lg, -jd);
                verts.Add(aPln, lg, jd);
                verts.Add(aPln, -lg, jd);
                verts.Add(aPln, -lg, -(jd + thk));
                verts.Add(aPln, lg, -(jd + thk));
                verts.Add(aPln, lg, jd + thk);
                verts.Add(aPln, -lg, jd + thk);
                verts.Add(aPln, -lg, -jd);
            }
            dxePolygon _rVal = new dxePolygon(verts, aPln.Origin, bClosed: false, aPlane: aPln) { LayerName = aLayerName };



            for (int j = 1; j <= aHls.Count; j++)
            {
                Hls = aHls.Item(j);
                for (int i = 1; i <= Hls.Count; i++)
                {
                    aHl = Hls.Item(i);
                    if (i == 1)
                    {
                        rad = aHl.Radius;
                    }
                    v1 = aHl.CenterDXF.WithRespectToPlane(bPln);
                    if (!bmancntr)
                    {
                        if (!mzUtils.ListContains(xVals, Math.Round(aHl.X, 4).ToString()))
                        {
                            mzUtils.ListAdd(ref xVals, Math.Round(v1.X, 4));
                            _rVal.AdditionalSegments.Add((dxfEntity)aPln.CreateLine(v1.X - rad, 0, v1.X - rad, -thk, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                            _rVal.AdditionalSegments.Add((dxfEntity)aPln.CreateLine(v1.X + rad, 0, v1.X + rad, -thk, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                        }
                    }
                    else
                    {
                        _rVal.AdditionalSegments.Add((dxfEntity)aPln.CreateLine(v1.X - rad, v1.Z + thk / 2, v1.X - rad, v1.Z - thk / 2, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                        _rVal.AdditionalSegments.Add((dxfEntity)aPln.CreateLine(v1.X + rad, v1.Z + thk / 2, v1.X + rad, v1.Z - thk / 2, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        ///      //^returns a dxePolygon that is used to draw the layout view of the angle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bShowHidden"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="bSuppressOrientations"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon SpliceAngle_View_Plan(mdSpliceAngle aPart, bool bShowHidden = false, bool bSuppressHoles = false, bool bSuppressOrientations = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {

                if (aPart == null) return _rVal;
                double thk = aPart.Thickness;
                double cX = aPart.X;
                double cY = aPart.Y;
                double wd = aPart.Width / 2;
                double leng = aPart.Length / 2;
                double rad = 0;
                string lt = string.Empty;

                uopHoleArray aHls = null;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
                bool bmancntr = aPart.PartType == uppPartTypes.ManwaySplicePlate;
                uopHole aHl = null;
                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y, aPart.Z));
                double f1 = 1;
                dxfVector v1 = null;
                colDXFVectors verts = new colDXFVectors();


                if (!bSuppressOrientations)
                {
                    if (Math.Round(aPart.Y, 1) < 0)
                    {
                        if (aPart.Direction == dxxRadialDirections.TowardsCenter) f1 *= -1;

                    }
                    else
                    {
                        if (aPart.Direction == dxxRadialDirections.AwayFromCenter) f1 *= -1;

                    }
                }


                if (bShowHidden) lt = dxfLinetypes.Hidden;
                _rVal.LayerName = aLayerName;
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
                _rVal.Closed = false;
                if (bShowHidden) _rVal.Linetype = dxfLinetypes.Hidden;

                if (bmancntr)
                {
                    verts.Add(aPln, -leng, 0);
                    verts.Add(aPln, -leng, wd * f1);
                    verts.Add(aPln, leng, wd * f1);

                    verts.Add(aPln, leng, 0, aLineType: lt);
                    verts.Add(aPln, leng, -wd * f1, aLineType: lt);
                    verts.Add(aPln, -leng, -wd * f1, aLineType: lt);

                    _rVal.Closed = true;
                }
                else
                {
                    verts.Add(aPln, -leng, (wd - thk) * f1, aLineType: dxfLinetypes.Hidden);
                    verts.Add(aPln, leng, (wd - thk) * f1, aLineType: lt);
                    verts.Add(aPln, leng, -wd * f1, aLineType: lt);

                    verts.Add(aPln, -leng, -wd * f1, aLineType: lt);
                    verts.Add(aPln, -leng, wd * f1, aLineType: lt);
                    verts.Add(aPln, leng, wd * f1, aLineType: lt);
                    verts.Add(aPln, leng, (wd - thk) * f1, aLineType: lt);

                }

                _rVal.Vertices = verts;
                if (bmancntr)
                {
                    _rVal.AdditionalSegments.AddPlanarLine(aPln, -leng, 0.125 * f1, leng, 0.125 * f1, aLineType: dxfLinetypes.Continuous);
                    _rVal.AdditionalSegments.AddPlanarLine(aPln, -leng, -0.125 * f1, leng, -0.125 * f1, aLineType: lt);

                    _rVal.AdditionalSegments.AddPlanarLine(aPln, -leng, -(wd - thk) * f1, leng, -(wd - thk) * f1, aLineType: dxfLinetypes.Hidden);
                }

                if (!bSuppressHoles)
                {

                    hdwHexBolt bolt = aPart.SmallBolt("");

                    dxePolyline boltPL = !bmancntr ? null : uopPolygons.HexNutPlan(bolt.HardwareStructure, dxfVector.Zero, aLinetype: dxfLinetypes.Hidden, aLayerName: aLayerName, aPlane: aPln);
                    dxePolyline nultPL = null;
                    aHls = aPart.GenHoles();
                    dxeArc holearc;
                    if (bmancntr)
                    {
                        lt = string.Empty;
                    }
                    else
                    {
                        lt = bShowHidden ? dxfLinetypes.Hidden : "";

                    }
                    dxfDisplaySettings dsp = new dxfDisplaySettings(aLayerName, aLinetype: lt);
                    foreach (uopHoles Hls in aHls)
                    {

                        for (int i = 1; i <= Hls.Count; i++)
                        {
                            aHl = Hls.Item(i);
                            v1 = aHl.CenterDXF.WithRespectToPlane(bPln, aPln, aYScale: f1);
                            if (i == 1) rad = aHl.Radius;
                            if (bmancntr)
                            {
                                dsp.Linetype = aHl.Y < bPln.Y ? string.Empty : dxfLinetypes.Hidden;

                            }



                            holearc = new dxeArc(v1, rad, aDisplaySettings: dsp) { Tag = aHl.Tag, Flag = aHl.Flag }; // aPln.CreateCircle(v1.X, v1.Y * f1, rad, aLayerName: aLayerName, aLineType: lt, aTag: aHl.Tag, aFlag: aHl.Flag);
                            _rVal.AdditionalSegments.Add(holearc);

                            if (bmancntr)
                            {
                                nultPL = (dxePolyline)boltPL.Clone();
                                nultPL.Translate(holearc.X, holearc.Y);
                                nultPL.Linetype = aHl.Y > bPln.Y ? string.Empty : dxfLinetypes.Hidden;
                                _rVal.AdditionalSegments.Add(nultPL);

                            }
                        }
                    }
                    

                }
                if (aPart.AngleType == uppSpliceAngleTypes.ManwayAngle)
                {
                    //adjust splice a
                    double f = 1;
                    if (aPart.Splice != null && aPart.Splice.IsTop) f *= -1;
                    _rVal.Vertices.Move(0, 0.25 * f);
                    _rVal.AdditionalSegments.Move(0, 0.25 * f);

                }
            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }

        /// <summary>
        /// #1the scale factor to apply to the profile polygon
        ///^returns a dxePolygon that is used to draw the sectional view of the angle.
        ///~the section passes through the center of the angle.
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bSuppressFillets"></param>
        /// <param name="bLongSide"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bAddHolePoints"></param>
        /// <returns></returns>
        public static dxePolygon SpliceAngle_View_Profile(mdSpliceAngle aPart, bool bSuppressFillets = false, bool bLongSide = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bAddHolePoints = false, bool bAddBolt = false)
        {
            dxePolygon _rVal;

            try
            {
                if (aPart == null) return new dxePolygon();
                double ht = aPart.Height;
                double wd = aPart.Width / 2;
                double thk = aPart.Thickness;
                double t = thk / 2;
                double cX = aPart.X;
                double cY = aPart.Z;
                dxfVector v1 = null;
                uopHoleArray aHls = aPart.GenHoles();
                uopHoles Hls = null;
                double rad = 0;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y, aPart.Z));
                bool bmancntr = aPart.PartType == uppPartTypes.ManwaySplicePlate;
                uopHole aHl = null;
                double f1 = 1;
                double jd = aPart.JoggleDepth / 2;
                double x1 = 0;
                colDXFVectors verts = new colDXFVectors();
                ht = aPart.Height;
                wd = aPart.Width / 2;

                if (Math.Round(bPln.Y, 1) < 0)
                {
                    if (aPart.Direction == dxxRadialDirections.TowardsCenter) f1 *= -1;
                }
                else
                {
                    if (aPart.Direction == dxxRadialDirections.AwayFromCenter) f1 *= -1;
                }
                if (bLongSide) f1 *= -1;

                if (!bmancntr)
                {
                    if (bSuppressFillets)
                    {
                        verts.Add(aPln, wd * f1, 0);
                        verts.Add(aPln, wd * f1, 0);
                        verts.Add(aPln, wd * f1, -thk);
                        verts.Add(aPln, (wd - thk) * f1, -thk);
                        verts.Add(aPln, (wd - thk) * f1, -ht);
                        verts.Add(aPln, wd * f1, -ht);
                    }
                    else
                    {
                        verts.Add(aPln, (wd - 2 * thk) * f1, 0);
                        verts.Add(aPln, -wd * f1, 0);
                        verts.Add(aPln, -wd * f1, -thk);
                        verts.Add(aPln, (wd - 2 * thk) * f1, -thk, aVertexRadius: thk);
                        verts.Add(aPln, (wd - thk) * f1, -2 * thk);
                        verts.Add(aPln, (wd - thk) * f1, -ht);
                        verts.Add(aPln, wd * f1, -ht);
                        verts.Add(aPln, wd * f1, -2 * thk, aVertexRadius: -2 * thk);
                    }
                }
                else
                {
                    verts.Add(aPln, -(wd - t) * f1, -(jd + t) - (ht - t));
                    verts.Add(aPln, -(wd - t) * f1, -(jd + t));
                    verts.Add(aPln, -thk * f1, -(jd + t));
                    verts.Add(aPln, thk * f1, jd + t);
                    verts.Add(aPln, wd * f1, jd + t);

                    verts = dxfPrimatives.CreateVertices_Trace(verts, thk, false, !bSuppressFillets, false);
                }

                _rVal = new dxePolygon(verts, aPln.Origin, bClosed: true, aPlane: aPln) { LayerName = aLayerName };


                if (Math.Round(bPln.Y, 1) < 0)
                {
                    if (aPart.Direction == dxxRadialDirections.TowardsCenter) f1 *= -1;

                }
                else
                {
                    if (aPart.Direction == dxxRadialDirections.AwayFromCenter) f1 *= -1;
                }
                x1 = -999;
                rad = 0;
                for (int j = 1; j <= aHls.Count; j++)
                {
                    Hls = aHls.Item(j);
                    if (j == 1) x1 = Hls.CentersDXF().GetOrdinate(dxxOrdinateTypes.MinX);

                    hdwHexBolt bolt = null;

                    if (bAddBolt)
                    {


                        bolt = aPart.SmallBolt(true, 1, $"{aPart.PartName} Bolt", thk);
                    }

                    if (aHls.Count > 0)
                    {
                        aHl = Hls.Item(1);
                        if (Math.Round(aHl.X, 3) == Math.Round(x1, 3))
                        {
                            if (rad == 0) rad = aHl.Radius;

                            v1 = aHl.CenterDXF.WithRespectToPlane(bPln);
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.Y * f1 + rad, v1.Z + t, v1.Y * f1 + rad, v1.Z - t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: aHl.Tag, aFlag: aHl.Flag));
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.Y * f1 - rad, v1.Z + t, v1.Y * f1 - rad, v1.Z - t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: aHl.Tag, aFlag: aHl.Flag));
                            if (bAddHolePoints || bAddBolt)
                            {
                                if (bAddHolePoints) _rVal.AdditionalSegments.AddPoint(aPln.Vector(v1.Y * f1, v1.Z), "DefPoints", aTag: aHl.Tag, aFlag: aHl.Flag);
                                if (bAddBolt)
                                {
                                    double oset = aPart.PartType == uppPartTypes.ManwaySplicePlate && j == 1 ? t : -t;
                                    v1 = aPln.Vector(v1.Y * f1, v1.Z + oset);
                                    dxxOrthoDirections dir = aPart.PartType == uppPartTypes.ManwaySplicePlate && j == 1 ? dxxOrthoDirections.Down : dxxOrthoDirections.Up;

                                    dxePolygon blt = bolt.View_Profile(1, v1, aPln, aDirection: dir, aLayerName: aLayerName, bAddWelds: true);

                                    colDXFEntities segs = blt.SubEntities(false);
                                    foreach (dxfEntity item in segs)
                                    {
                                        _rVal.AdditionalSegments.Add(item, aTag: $"BOLT_{j}", aFlag: item.Tag);
                                    }



                                    dxfRectangle bounds = blt.Vertices.BoundingRectangle(aPln);
                                    v1 = bounds.TopCenter + aPln.YDirection * 3 * thk;
                                    dxfVector v2 = bounds.BottomCenter + aPln.YDirection * -3 * thk;

                                    _rVal.AdditionalSegments.Add(new dxeLine(v1, v2) { LayerName = aLayerName, Linetype = "Center", Tag = $"BOLT_{j}", Flag = "CENTERLINE" });


                                }
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return _rVal;
        }
        /// <summary>
        ///returns a dxePolygon that is used to draw the side view of the baffle

        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon SupDef_View_Elevation(mdSupplementalDeflector aPart, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon() { LayerName = aLayerName };
            try
            {
                if (aPart == null) return _rVal;
                if (aPart.Height <= 0) return _rVal;
                double cX = aPart.X;
                double cY = aPart.Z;
                double ht = aPart.Height;
                double thk = aPart.Thickness;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);

                _rVal.Vertices.Add(aPln, -thk / 2, 0);
                _rVal.Vertices.Add(aPln, -thk / 2, -ht);
                _rVal.Vertices.Add(aPln, thk / 2, -ht);
                _rVal.Vertices.Add(aPln, thk / 2, 0);

                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;

            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }
        /// <summary>
        ///     //^returns a dxePolygon that is used to draw the layout view of the deflector
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aAssy_UNUSED"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon SupDef_View_Plan(mdSupplementalDeflector aPart, mdTrayAssembly aAssy = null, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon() { LayerName = aLayerName, Closed = true };
            try
            {


                if (aPart == null) return _rVal;
                if (aPart.Height <= 0) return _rVal;
                double thk = aPart.Thickness;
                double lg = aPart.Length;
                colDXFVectors verts = new colDXFVectors();
                dxfPlane aPln = xPlane(aPart.X, aPart.Y, aCenter, aRotation);
                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y));

                verts.Add(aPln, -0.5 * thk, -0.5 * lg);
                verts.AddRelative(thk, aPlane: aPln);
                verts.AddRelative(aY: lg, aPlane: aPln);
                verts.AddRelative(-thk, aPlane: aPln);
                verts.AddRelative(aY: -lg, aPlane: aPln);
                _rVal.Vertices = verts;
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
            }
            catch (Exception)
            {
                throw;
            }
            return _rVal;
        }
        /// <summary>
        /// ^returns a dxePolygon that is used to draw the side view of the baffle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon SupDef_View_Profile(mdSupplementalDeflector aPart, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon() { LayerName = aLayerName, Closed = true };
            try
            {

                if (aPart == null) return _rVal;
                if (aPart.Height <= 0) return _rVal;
                double cX = aPart.Y;
                double cY = aPart.Z;
                double ht = aPart.Height;
                double lng = aPart.Length;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);

                _rVal.Vertices.Add(aPln, -lng / 2, 0);
                _rVal.Vertices.Add(aPln, -lng / 2, ht);
                _rVal.Vertices.Add(aPln, lng / 2, ht);
                _rVal.Vertices.Add(aPln, lng / 2, 0);

                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;

            }
            catch (Exception)
            {
                throw;
            }
            return _rVal;
        }

        /// <summary>
        ///returns the vertices that describe the profile view of the baffle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bLongSide"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="rPlane"></param>
        /// <param name="bIncludeHoleCenters"></param>
        /// <param name="aCollector"></param>
        /// <param name="bSuppressRadii"></param>
        /// <param name="rNotchCount"></param>
        /// <returns></returns>
        internal static UVECTORS DeflectorPlateVertices(mdBaffle aPart, bool bLongSide, dxfVector aCenter, double aRotation, ref dxfPlane rPlane, ref int rNotchCount, bool bIncludeHoleCenters = false, colDXFVectors aCollector = null, bool bSuppressRadii = true)
        {
            UVECTORS _rVal = UVECTORS.Zero;

            double f1 = bLongSide ? -1 : 1;
            rNotchCount = 0;
            try
            {
                if (aPart == null) { return _rVal; }
                if (aPart.Height <= 0) { return _rVal; }


                List<double> ntchs = new List<double>();
                TVALUES dcctrs = aPart.DCCenters;

                UVECTOR u1;
                UVECTOR u2;
                double y0 = aPart.Y;
                double y2 = aPart.ZLimit;
                double y3 = aPart.Height;
                double x1 = aPart.Top - y0;
                double x2 = aPart.Bottom - y0;
                mzUtils.SortTwoValues(true, ref x1, ref x2);
                double xt = 0;
                double xb = 0;
                double ntch = 0;
                bool bFlg = false;
                bool aFlg = false;
                double cX = y0 * f1;
                double cY = aPart.Z;

                int cnt = 0;
                double rad = 0.25;


                dcctrs.SortNumeric(bRemoveDupes: true);

                rPlane = xPlane(cX, cY, aCenter, aRotation);

                for (int i = 1; i <= dcctrs.Count; i++)
                {
                    dcctrs.SetValue(i, dcctrs.Item(i) - y0);
                }


                if (y2 < y3 && dcctrs.Count > 0)
                {
                    if (dcctrs.BaseValue == null)
                        ntch = aPart.DowncomerBoxWidth + 1; //dc width + 1/2'' each side
                    else
                        ntch = dcctrs.BaseValue + 1; //dc width + 1/2'' each side


                    for (int i = 1; i <= dcctrs.Count; i++)
                    {
                        xt = dcctrs.Item(i) + ntch / 2;
                        xb = dcctrs.Item(i) - ntch / 2;
                        if ((xt >= x1 && xt <= x2) || (xb >= x1 && xb <= x2))
                        {
                            ntchs.Add(dcctrs.Item(i));
                        }
                    }


                }
                ntchs.Sort();
                ntchs.Reverse();


                _rVal.Add(UVECTOR.FromDXFVector(rPlane.Vector(x1 * f1)), aCollector: aCollector);
                _rVal.Add(UVECTOR.FromDXFVector(rPlane.Vector(x2 * f1)), aCollector: aCollector);
                _rVal.Add(UVECTOR.PlaneVector(rPlane, x2 * f1, y3), aCollector: aCollector);
                if (ntchs.Count > 0)
                {
                    aFlg = false;
                    bFlg = false;
                    for (int i = 1; i <= ntchs.Count; i++)
                    {
                        xt = ntchs[i - 1] + 0.5 * ntch;
                        xb = ntchs[i - 1] - 0.5 * ntch;
                        aFlg = xb > x1 + 1;
                        bFlg = xt < x2 - 1;
                        if (!aFlg && bFlg)
                        {
                            //partial notch on left/top end
                            rNotchCount++;
                            if (bSuppressRadii)
                            {
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xt * f1, y3), aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xt * f1, y2), aTag: "NOTCH", aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, x1 * f1, y2), aTag: "NOTCH", aCollector: aCollector);
                            }
                            else
                            {
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xt * f1, y3), aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xt * f1, y2 + rad, -f1 * rad), aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, (xt - rad) * f1, y2), "NOTCH", aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, x1 * f1, y2), "NOTCH", aCollector);

                            }


                        }
                        else if (aFlg && !bFlg)
                        {
                            //partial notch on right/bottom end
                            rNotchCount++;
                            _rVal.Add(UVECTOR.PlaneVector(rPlane, x2 * f1, y2), aCollector: aCollector);
                            if (bSuppressRadii)
                            {
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xb * f1, y2), aTag: "NOTCH", aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xb * f1, y3), aCollector: aCollector);
                            }
                            else
                            {
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, (xb + rad) * f1, y2, -f1 * rad), aTag: "NOTCH", aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xb * f1, y2 + rad), aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xb * f1, y3), aCollector: aCollector);
                            }


                        }
                        else if (!aFlg && !bFlg)
                        {
                            //the notch is width than the length of the baffle
                            if (_rVal.Count == 2)
                            {
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, x2 * f1, y2), aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, x1 * f1, y2), aCollector: aCollector);
                            }
                            break;

                        }
                        else
                        {
                            //full notch
                            rNotchCount++;
                            //if (cnt == 0)
                            //{
                            //    _rVal.Add(UVECTOR.PlaneVector(rPlane, x2 * f1, y3));
                            //}
                            if (bSuppressRadii)
                            {
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xt * f1, y3), aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xt * f1, y2), aTag: "NOTCH", aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xb * f1, y2), aTag: "NOTCH", aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xb * f1, y3), aCollector: aCollector);
                            }
                            else
                            {
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xt * f1, y3), aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xt * f1, y2 + rad, -f1 * rad), aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, (xt - rad) * f1, y2), "NOTCH", aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, (xb + rad) * f1, y2, -f1 * rad), "NOTCH", aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xb * f1, y2 + rad), aCollector: aCollector);
                                _rVal.Add(UVECTOR.PlaneVector(rPlane, xb * f1, y3), aCollector: aCollector);
                            }
                        }

                        cnt++;


                    }
                }

                u1 = _rVal.Item(_rVal.Count);
                u2 = UVECTOR.PlaneVector(rPlane, x1 * f1, y3);
                if (Math.Round(u1.X, 3) != Math.Round(u2.X, 3))
                {
                    _rVal.Add(u2, aCollector: aCollector);
                }


            }
            catch (Exception e)
            {

                throw e;
            }
            finally
            {

                if (bIncludeHoleCenters)
                {
                    UHOLES aHls;

                    UHOLE aHl;

                    dxfVector v1 = null;

                    dxfPlane bPlane = dxfPlane.World;
                    bPlane.SetCoordinates(aPart.X, aPart.Y, aPart.Z);
                    UHOLEARRAY uHOLEARRAY = aPart.GenHolesV(null);
                    aHls = uHOLEARRAY.Item(1);
                    for (int i = 1; i <= aHls.Count; i++)
                    {
                        aHl = aHls.Item(i);
                        v1 = aHl.Center.ToDXFVector(false, false, aHl.Elevation).WithRespectToPlane(bPlane);
                        v1 = rPlane.Vector(v1.Y * f1, aHl.Elevation - aPart.Z);
                        _rVal.Add(UVECTOR.FromDXFVector(v1));
                    }
                }

            }
            return _rVal;
        }

        /// <summary>
        ///returns the vertices that describe the profile view of the baffle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bLeftSide"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="bIncludeHoleCenters"></param>
        /// <param name="bSuppressRadii"></param>
        /// <param name="aPlane"></param>
        /// <param name="rHoles"></param>
        /// <param name="bOmmitNotches"></param>
        /// <returns></returns>
        internal static colDXFVectors DeflectorPlateVerts(mdBaffle aPart,out uopHoleArray rHoles, bool bLeftSide = false, iVector aCenter = null, double aRotation = 0, bool bIncludeHoleCenters = false, bool bSuppressRadii = true, dxfPlane aPlane = null,  bool bOmmitNotches = false)
        {
            rHoles = new uopHoleArray();
            colDXFVectors _rVal = colDXFVectors.Zero;
            if (aPart == null) return _rVal;
            if (aPart.Height <= 0) return _rVal;

            rHoles = new uopHoleArray();
            int rNotchCount = 0;

            List<double> ntchs = new List<double>();
       
            double f1 = bLeftSide ? -1 : 1;
            dxfVector u1;
            dxfVector u2;
            double y0 = aPart.Y;
            double y2 = aPart.ZLimit;
            double y3 = aPart.Height;
            double x1 = aPart.Top - y0;
            double x2 = aPart.Bottom - y0;
            mzUtils.SortTwoValues(true, ref x1, ref x2);
    
            double ntch = 0;
            double cX = y0 * f1;
            double cY = aPart.Z;
            dxfPlane aPln = aPlane ?? xPlane(cX, cY, aCenter, aRotation);
            int cnt = 0;
            double rad = 0.25;
            List<double> dcctrs = aPart.DCCenters.ToNumericList(6, mzSortOrders.LowToHigh,bNoDupes: true, aAdder:-y0);


            try
            {


                if (y2 < y3 && dcctrs.Count > 0)
                {
         
                   ntch = aPart.DowncomerBoxWidth + 1; //dc width + 1/2'' each side
         

                    for (int i = 1; i <= dcctrs.Count; i++)
                    {
                        double xt = dcctrs[i -1] + ntch / 2;
                        double xb = dcctrs[i - 1] - ntch / 2;
                        if ((xt >= x1 && xt <= x2) || (xb >= x1 && xb <= x2))
                        {
                            ntchs.Add(dcctrs[i - 1]);
                        }
                    }


                }
                ntchs.Sort();
                ntchs.Reverse();

                if (aPart.IsBlank || aPart.ForDistributor) bOmmitNotches = true;

                _rVal.Add(aPln, x1 * f1);
                _rVal.Add(aPln, x2 * f1);
                _rVal.Add(aPln, x2 * f1, y3);
                if (ntchs.Count > 0 && !bOmmitNotches)
                {
                    bool aFlg = false;
                    bool bFlg = false;
                    for (int i = 1; i <= ntchs.Count; i++)
                    {
                        double xt = ntchs[i - 1] + 0.5 * ntch;
                        double xb = ntchs[i - 1] - 0.5 * ntch;
                        aFlg = xb > x1 + 1;
                        bFlg = xt < x2 - 1;
                        if (!aFlg && bFlg)
                        {
                            //partial notch on left/top end
                            rNotchCount++;
                            if (bSuppressRadii)
                            {
                                _rVal.Add(aPln, xt * f1, y3);
                                _rVal.Add(aPln, xt * f1, y2, aTag: "NOTCH");
                                _rVal.Add(aPln, x1 * f1, y2, aTag: "NOTCH");
                            }
                            else
                            {
                                _rVal.Add(aPln, xt * f1, y3);
                                _rVal.Add(aPln, xt * f1, y2 + rad, aVertexRadius: -f1 * rad);
                                _rVal.Add(aPln, (xt - rad) * f1, y2, aTag: "NOTCH");
                                _rVal.Add(aPln, x1 * f1, y2, aTag: "NOTCH");

                            }


                        }
                        else if (aFlg && !bFlg)
                        {
                            //partial notch on right/bottom end
                            rNotchCount++;
                            _rVal.RemoveLast();
                            _rVal.Add(aPln, x2 * f1, y2);
                            if (bSuppressRadii)
                            {
                                _rVal.Add(aPln, xb * f1, y2, aTag: "NOTCH");
                                _rVal.Add(aPln, xb * f1, y3);
                            }
                            else
                            {
                                _rVal.Add(aPln, (xb + rad) * f1, y2, aVertexRadius: -f1 * rad, aTag: "NOTCH");
                                _rVal.Add(aPln, xb * f1, y2 + rad);
                                _rVal.Add(aPln, xb * f1, y3);
                            }


                        }
                        else if (!aFlg && !bFlg)
                        {
                            //the notch is width than the length of the baffle
                            if (_rVal.Count == 2)
                            {
                                _rVal.Add(aPln, x2 * f1, y2);
                                _rVal.Add(aPln, x1 * f1, y2);
                            }
                            break;

                        }
                        else
                        {
                            //full notch
                            rNotchCount++;
                            //if (cnt == 0)
                            //{
                            //    _rVal.Add(aPln, x2 * f1, y3));
                            //}
                            if (bSuppressRadii)
                            {
                                _rVal.Add(aPln, xt * f1, y3);
                                _rVal.Add(aPln, xt * f1, y2, aTag: "NOTCH");
                                _rVal.Add(aPln, xb * f1, y2, aTag: "NOTCH");
                                _rVal.Add(aPln, xb * f1, y3);
                            }
                            else
                            {
                                _rVal.Add(aPln, xt * f1, y3);
                                _rVal.Add(aPln, xt * f1, y2 + rad, aVertexRadius: -f1 * rad);
                                _rVal.Add(aPln, (xt - rad) * f1, y2, aTag: "NOTCH");
                                _rVal.Add(aPln, (xb + rad) * f1, y2, aVertexRadius: -f1 * rad, aTag: "NOTCH");
                                _rVal.Add(aPln, xb * f1, y2 + rad);
                                _rVal.Add(aPln, xb * f1, y3);
                            }
                        }

                        cnt++;


                    }
                }

                u1 = _rVal.Item(_rVal.Count);
                u2 = aPln.Vector(x1 * f1, y3);
                if (Math.Round(u1.X, 3) != Math.Round(u2.X, 3)) _rVal.Add(u2);



            }
            catch (Exception e)
            {

                throw e;
            }
            finally
            {

                if (bIncludeHoleCenters)
                {
                  
                  

                    dxfPlane bPlane = new dxfPlane(new dxfVector(aPart.X, aPart.Y, aPart.Z));
                    rHoles = aPart.GenHoles(null);
                    if (rHoles.Count > 0)
                    {
                        uopHoles aHls = rHoles.Item(1);
                        

                        for (int i = 1; i <= aHls.Count; i++)
                        {
                            uopHole aHl = aHls.Item(i);
                            dxfVector v1 = aHl.Center.ToDXFVector(false, false, aHl.Elevation).WithRespectToPlane(bPlane);
                            v1 = aPln.Vector(v1.Y * f1, aHls.Elevation - aPart.Z);
                            _rVal.Add(v1, aTag: "SLOT CENTER");
                        }
                    }

                }

            }
            return _rVal;
        }

        /// <summary>
        ///returns a dxePolygon that is used to draw the elevation view of the baffle

        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aAssy"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon Baffle_View_Elevation(mdBaffle aPart, mdTrayAssembly aAssy, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon() { LayerName = aLayerName, Closed = true };
            try
            {


                if (aPart == null) return _rVal;
                if (aPart.Height <= 0) return _rVal;

                double thk = aPart.Thickness;
                double ht = aPart.Height;
                double cX = aPart.X;
                double cY = aPart.Z;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);

                cX = aPart.X;
                cY = aPart.Z;
                ht = aPart.Height;
                thk = aPart.Thickness / 2;

                aPln = xPlane(cX, cY, aCenter, aRotation);

                _rVal.Vertices.Add(aPln, -thk, 0);
                _rVal.Vertices.Add(aPln, -thk, ht);
                _rVal.Vertices.Add(aPln, thk, ht);
                _rVal.Vertices.Add(aPln, thk, 0);

                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;

                var holesArray = aPart.GenHolesV(aAssy, true);
                if (holesArray.Count > 0)
                {
                    UHOLES Hls = holesArray.Item(1);
                    if (Hls.Centers.Count > 0)
                    {

                        UHOLE aHl = Hls.Item(1);
                        ht = aHl.Radius;
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(-thk, aHl.Elevation - aPart.Z + ht, thk, aHl.Elevation - aPart.Z + ht, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(-thk, aHl.Elevation - aPart.Z - ht, thk, aHl.Elevation - aPart.Z - ht, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }

        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the angle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aAssy_UNUSED"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon Baffle_View_Plan(mdBaffle aPart, mdTrayAssembly aAssy = null, bool bSuppressHoles = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon() { LayerName = aLayerName, Closed = true };
            try
            {
                if (aPart == null) return _rVal;
                if (aPart.Height <= 0) return _rVal;
                double thk = aPart.Thickness;
                double cX = aPart.X;
                double lg = aPart.Length;
                double cY = aPart.Y;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y));

                _rVal.LayerName = aLayerName;
                colDXFVectors verts = new colDXFVectors
                {
                    { aPln, -0.5 * thk, -0.5 * lg }
                };
                verts.AddRelative(thk, aPlane: aPln);

                verts.AddRelative(0, lg, aPlane: aPln);
                verts.AddRelative(-thk, aPlane: aPln);
                verts.AddRelative(0, -lg, aPlane: aPln);
                _rVal.Vertices = verts;

                if (!bSuppressHoles)
                {
                    UHOLEARRAY hray = aPart.GenHolesV(aAssy);
                    if (hray.Count > 0)
                    {
                        UHOLES Hls = hray.Item(1);
                        double d1 = Hls.Member.Length / 2;
                        dxfVector v1;
                        UHOLE h1;
                        double t = thk / 2;
                        for (int i = 1; i <= Hls.Count; i++)
                        {
                            h1 = Hls.Item(i);
                            v1 = h1.Center.ToDXFVector().WithRespectToPlane(bPln);
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - t, v1.Y + d1, v1.X + t, v1.Y + d1, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                            _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - t, v1.Y - d1, v1.X + t, v1.Y - d1, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));

                        }

                    }

                }
                _rVal.Plane = aPln;

                _rVal.InsertionPt = aPln.Origin;
            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }

        public static dxePolygon DC_View_ManholeFit(mdDowncomer aPart, mdTrayAssembly aAssy, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon();
            if (aPart == null) return _rVal;
            aAssy = aPart.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            try
            {
                mdDowncomerBox aBox = aPart.Boxes.FirstOrDefault(); // This needs to be modified to support multiple boxes
                if (aBox == null) return _rVal;
                dxfPlane aPln = null;
                double wht = aPart.How;
                double thk = aPart.Thickness;
                double wd = aBox.Width;
                dxePolyline aPl = null;
                dxePolygon aPg = null;

                string LName = string.Empty;
                mdStiffener aStf = null;
                dxfVector v1 = null;
                dxfPlane bPln = aPart.Plane;

                _rVal = DCBox_View_Elevation(aBox, aAssy, false, true, false, aCenter, aRotation, aLayerName);

                _rVal.AdditionalSegments.GetByTag("BOTTOM_NUT", bRemove: true);

                aPl = new dxePolyline() { LayerName = aLayerName, Plane = _rVal.Plane };

                aPln = _rVal.Plane;


                ////=========== STIFFENERS ==================================
                if (aAssy.ProjectType == uppProjectTypes.MDDraw)
                {
                    if (aAssy.DesignFamily.IsEcmdDesignFamily() && aAssy.DesignOptions.CDP > 0)
                    {
                        if (aAssy.DesignOptions.WeldedStiffeners || (!aAssy.DesignOptions.WeldedStiffeners && aPart.HasSupplementalDeflector))
                        {
                            LName = aLayerName;

                            aStf = aPart.Stiffener();
                            if (aStf != null)
                            {
                                v1 = aStf.Center.ToDXFVector().WithRespectToPlane(bPln);
                                aStf.SupplementalDeflectorHeight = aPart.SupplementalDeflectorHeight;

                                aPg = Stiffener_View_Elevation(aStf, false, true, aPln.Vector(v1.X, v1.Z), aRotation, LName);
                                aPl.Vertices = aPg.Vertices.Clone();

                                _rVal.AdditionalSegments.Add((dxfEntity)aPl, bAddClone: true, aTag: "STIFFENER");
                            }

                        }
                    }

                }


                //the shelf angles
                v1 = aPln.Vector(-wd / 2, -0.5);
                aPl = (dxePolyline)uopGlobals.UopGlobals.Primatives.Angle_End(v1, 1, thk, 1, true, aPlane: aPln);
                aPl.LayerName = _rVal.LayerName;
                _rVal.AdditionalSegments.Add((dxfEntity)aPl, aTag: "SHELF", aFlag: "LEFT");

                v1 = aPln.Vector(wd / 2, -0.5);
                aPl = (dxePolyline)uopGlobals.UopGlobals.Primatives.Angle_End(v1, -1, thk, 1, true, aPlane: aPln);
                aPl.LayerName = _rVal.LayerName;
                _rVal.AdditionalSegments.Add((dxfEntity)aPl, aTag: "SHELF", aFlag: "RIGHT");

                return _rVal;
            }
            catch (Exception)
            {
                return _rVal;
                throw;
            }



        }

        public static dxePolygon DC_View_Plan(mdDowncomer aPart, mdTrayAssembly aAssy, iVector aCenter = null, double aRotation = 0, bool bObscuredShelfs = false, bool bIncludeBoltOns = false, bool bSuppressHoles = false, bool bSuppressSpouts = false, double aCenterLineLength = 0, string aBoltOnList = "", string aLayerName = "GEOMETRY", string aWeldOnList = "", bool bOneLayer = true, bool bShowVirtual = true)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {
                if (aPart == null) return _rVal;
                aAssy = aPart.GetMDTrayAssembly(aAssy);
                if (aAssy == null) return _rVal;
           
          

                bool virtul = aPart.IsVirtual;
                if (virtul)
                {
                    aPart = aPart.Parent;
                    if (aPart == null) return _rVal;
                }
                List<mdDowncomerBox> boxes = aPart.Boxes.FindAll((x) => !x.IsVirtual);

                dxfPlane aPln = xPlane(aPart.X, aPart.Y, aCenter, aRotation);
                double dkthk = aAssy.Deck.Thickness;
                double thk = aPart.Thickness;
                mdDowncomerBox aBox;
                if (bShowVirtual)
                {
                    aBox = boxes.FirstOrDefault();
                }
                else
                {
                    aBox = boxes.FirstOrDefault(b => b.IsVirtual == false);
                }
                if (aBox == null)
                    return _rVal;
               
                colDXFVectors vrts = new colDXFVectors();
                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y));
                dxeLine ln1 = null;
                dxfVector v1 = null;
                dxfVector v2 = null;
                dxePolygon aPg = null;
                colDXFEntities aEnts = null;
                colDXFEntities bEnts = null;
                double wd = aPart.Width / 2 + thk;
                string LName = string.Empty;
                colUOPParts aPrts = null;
                List<mdStiffener> aStfs = mdPartGenerator.Stiffeners_DC(aPart, aAssy);
                mdEndAngle aEA = null;
                dxfDirection aDir = null;
                double d2 = 0;
                mdFingerClip aFC = null;
                bool aFlg = false;
                bool bCenterDC = mzUtils.CompareVal(aPart.X, 0, 3) == mzEqualities.Equals;
                colDXFVectors verts;
                string blockName = $"DC_{aPart.PartNumber}_PLAN";
           
                dxePolygon boxPolygon;
                Func<dxePolygon, dxfPlane, dxePolygon> polygonWithRespectToPlane = (dxePolygon polygon, dxfPlane plane) =>
                {
                    colDXFVectors vertices = new colDXFVectors();
                    foreach (var v in polygon.Vertices)
                    {
                        vertices.Add(v.WithRespectToPlane(plane));
                    }

                    dxePolygon result = new dxePolygon(polygon);
                    result.InsertionPt = result.InsertionPt.WithRespectToPlane(plane);
                    result.Plane = plane;
                    result.Vertices = vertices;
                    return result;
                };

                if (!string.IsNullOrWhiteSpace(aBoltOnList)) bIncludeBoltOns = true;

                aFlg = mzUtils.ListContains("END SUPPORTS", aWeldOnList, bReturnTrueForNullList: true);

                boxPolygon = aBox.View_Plan(aAssy, bOutLineOnly: false, bSuppressHoles: bSuppressHoles, bIncludeSpouts: !bSuppressSpouts, bShowObscured: aFlg, aCenterLineLength: aCenterLineLength, aCenter: aBox.Center, aRotation: 0, aLayerName: aLayerName);

                _rVal = polygonWithRespectToPlane(boxPolygon, bPln);
                if (aRotation != 0)
                {
                    _rVal.Rotate(aRotation);
                }
                boxPolygon.Dispose();

                if (boxes.Count > 1)
                {
                    for (int i = 0; i < boxes.Count; i++)
                    {
                        if (boxes[i] == aBox || (!bShowVirtual && boxes[i].IsVirtual))
                        {
                            continue;
                        }

                        boxPolygon = boxes[i].View_Plan( aAssy,  bOutLineOnly: false, bSuppressHoles: bSuppressHoles, bIncludeSpouts: !bSuppressSpouts, bShowObscured: aFlg, aCenterLineLength: aCenterLineLength, aCenter: boxes[i].Center, aRotation: 0, aLayerName: aLayerName);
                        aPg = polygonWithRespectToPlane(boxPolygon, bPln);
                        if (aRotation != 0)
                        {
                            aPg.Rotate(aRotation);
                        }
                        boxPolygon.Dispose();

                        aEnts = aPg.SubEntities();
                        _rVal.AdditionalSegments.Append(aEnts);
                        aPg.Dispose();
                    }
                }
                if (aFlg && !bIncludeBoltOns && aAssy.HasAntiPenetrationPans)
                {
                    _rVal.AdditionalSegments.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Continuous, aTagList: "APPAN_SLOT");

                }

                _rVal.BlockName = blockName;

                //=========== SHELF ANGLES ===========================================
                if (mzUtils.ListContains("SHELVES", aWeldOnList, bReturnTrueForNullList: true))
                {
                    LName = bOneLayer ? aLayerName : "SHELF_ANGLES";

                    List<mdShelfAngle> shelfAngles;
                    mdShelfAngle shortShelfAngle;
                    double clearance = aAssy.PanelClearance(true);

                    foreach (var box in boxes)
                    {
                        shelfAngles = box.ShelfAngles();
                        shortShelfAngle = shelfAngles[0].EdgeLn.Length >= shelfAngles[1].EdgeLn.Length ? shelfAngles[1] : shelfAngles[0];
                        
                        foreach (var shelfAngle in shelfAngles)
                        {
                            string flag = shelfAngle == shortShelfAngle ? "SHORT" : "LONG";

                            double topY = shelfAngle.EdgeLn.MaxY;
                            double bottomY = shelfAngle.EdgeLn.MinY;
                            double insideX = box.BoxLine(shelfAngle.Side == uppSides.Left).sp.X - box.CenterDXF.X;
                            double outsideX = shelfAngle.EdgeLn.sp.X - box.CenterDXF.X;
                            double clearanceX = insideX + (shelfAngle.Side == uppSides.Right ? clearance : -clearance);
                            double thicknessX = insideX + (shelfAngle.Side == uppSides.Right ? thk : -thk);

                            if (!bObscuredShelfs)
                            {


                                verts = new colDXFVectors(new dxfVector(insideX, topY), new dxfVector(outsideX, topY), new dxfVector(outsideX, bottomY), new dxfVector(insideX, bottomY));
                                if (aRotation != 0)
                                {
                                    verts.RotateAbout(dxfVector.Zero, aRotation);
                                }
                                if (!bIncludeBoltOns)
                                {
                                    _rVal.AdditionalSegments.Add(new dxePolyline(verts, bClosed: true, aPlane: aPln) { Linetype = dxfLinetypes.ByLayer, LayerName = LName }, aTag: "SHELVES", aFlag: flag);
                                }
                                else
                                {
                                    verts.Item(1).Linetype = dxfLinetypes.Hidden;
                                    verts.Item(3).Linetype = dxfLinetypes.Hidden;
                                    aPg = new dxePolygon(verts, aPlane: aPln) { LayerName = LName, Closed = true };
                                    aEnts = aPg.SubEntities();
                                    _rVal.AdditionalSegments.Append(aEnts, aTag: "SHELVES", aFlag: flag);
                                    aPg.Dispose();
                                }

                            }
                            else
                            {
                                verts = new colDXFVectors(new dxfVector(clearanceX, topY), new dxfVector(outsideX, topY), new dxfVector(outsideX, bottomY), new dxfVector(clearanceX, bottomY));
                                if (aRotation != 0)
                                {
                                    verts.RotateAbout(dxfVector.Zero, aRotation);
                                }

                                _rVal.AdditionalSegments.Add(new dxePolyline(verts, bClosed: false, aPlane: aPln) { Linetype = dxfLinetypes.Hidden, LayerName = LName }, aTag: "SHELVES", aFlag: flag);

                                verts = new colDXFVectors(new dxfVector(clearanceX, topY), new dxfVector(insideX, topY), new dxfVector(insideX, bottomY), new dxfVector(clearanceX, bottomY));
                                if (aRotation != 0)
                                {
                                    verts.RotateAbout(dxfVector.Zero, aRotation);
                                }
                                _rVal.AdditionalSegments.Add(new dxePolyline(verts, bClosed: false, aPlane: aPln) { Linetype = dxfLinetypes.ByLayer, LayerName = LName }, aTag: "SHELVES", aFlag: flag);


                            }

                            var thicknessLine = new dxeLine(new dxfVector(thicknessX, topY), new dxfVector(thicknessX, bottomY)) { Linetype = dxfLinetypes.Hidden, LayerName = LName };
                            if (aRotation != 0)
                            {
                                thicknessLine.RotateAbout(dxfVector.Zero, aRotation);
                            }
                            _rVal.AdditionalSegments.Add(thicknessLine, bAddClone: false, aTag: "SHELVES", aFlag: flag);
                        }
                    }
                    
                }

                ln1 = aPln.XAxis();

                //=========== END SUPPORT ==================================
                if (mzUtils.ListContains("END SUPPORTS", aWeldOnList, bReturnTrueForNullList: true))
                {
                    LName = bOneLayer ? aLayerName : "END SUPPORTS";

                    foreach (var box in boxes)
                    {
                        List<mdEndSupport> ess = box.EndSupports();
                        foreach (mdEndSupport es in ess)
                        {
                            if (!bShowVirtual && es.DowncomerBox.IsVirtual)
                            {
                                continue;
                            }

                            v1 = es.CenterDXF.WithRespectToPlane(bPln, aPln, 0);
                            aPg = EndSupport_View_Plan(es, bSuppressHoles: false, aCenterLineLength: aCenterLineLength, aCenter: v1, aRotation: aRotation, aLayerName: LName);
                            aEnts = aPg.SubEntities();

                            _rVal.AdditionalSegments.Append(aEnts, false, aTag: "END SUPPORTS", aFlag: es.Y > box.Y ? "TOP" : "BOT");
                            aPg.Dispose();
                        }
                    }

                    //mdEndSupport aES = boxes[0].EndSupport(bTop: true);
                    //v1 = aES.CenterDXF.WithRespectToPlane(bPln, aPln, 0);
                    //aPg = EndSupport_View_Plan(aES,bSuppressHoles: bSuppressHoles, aCenterLineLength: aCenterLineLength, aCenter: v1, aRotation: 0, aLayerName: LName);
                    //aEnts = aPg.SubEntities();


                    //_rVal.AdditionalSegments.Append(aEnts, true,aTag: "END SUPPORTS",aFlag: "TOP");
                    //aPg.Dispose();
                }

                //=========== END PLATE ==================================
                if (mzUtils.ListContains("END PLATES", aWeldOnList, bReturnTrueForNullList: true))
                {
                    LName = bOneLayer ? aLayerName : "END_PLATES";

                    foreach (var box in boxes)
                    {
                        if (!bShowVirtual && box.IsVirtual)
                        {
                            continue;
                        }

                        List<mdEndPlate> eplts = aPart.EndPlates(box.Index);
                        foreach (mdEndPlate eplt in eplts)
                        {
                            v1 = eplt.CenterDXF.WithRespectToPlane(bPln, aPln, 0);
                            aPg = EndPlate_View_Plan(eplt, true, v1, aRotation, LName, bSuppressHoles: bSuppressHoles);
                            aEnts = aPg.SubEntities();

                            _rVal.AdditionalSegments.Append(aEnts, bAddClones: false, aTag: "END PLATES", aFlag: eplt.Y > box.Y ? "TOP" : "BOT");
                            aPg.Dispose();
                        }
                    }




                }


                //=========== STIFFENERS ==================================

                if (mzUtils.ListContains("STIFFENERS", aWeldOnList, bReturnTrueForNullList: true))
                {
                    LName = bOneLayer ? aLayerName : "STIFFENERS";
                    mdStiffener stiffenerInstance = new mdStiffener(boxes[0], 0);
                    foreach (var box in boxes.Where(b => bShowVirtual || !b.IsVirtual))
                    {
                        var boxStiffenerOrdinates = box.StiffenerYs(aPart);
                        

                        if (boxStiffenerOrdinates != null && stiffenerInstance != null)
                        {
                            v1 = aPln.Origin;
                            aPg = Stiffener_View_Plan(stiffenerInstance, box, aAssy, false, true, v1, aRotation, LName);
                            aEnts = aPg.SubEntities();

                            foreach (var ordinate in boxStiffenerOrdinates)
                            {
                                v2 = aPln.Vector(0, ordinate);
                                _rVal.AdditionalSegments.Append(aEnts.ProjectedFromTo(v1, v2), false, "STIFFENERS");
                            }

                            aPg.Dispose();

                            if (aPart.SupplementalDeflectorHeight > 0)
                            {
                                mdSupplementalDeflector aDef = aPart.SupplementalDeflector();
                                v1 = aDef.CenterDXF.WithRespectToPlane(bPln, aPln, 0);
                                LName = bOneLayer ? aLayerName : "SUPL_DEF";
                                aPg = SupDef_View_Plan(aDef, aAssy, v1, aRotation, LName);
                                aEnts = aPg.SubEntities();

                                _rVal.AdditionalSegments.Append(aEnts, aTag: "SUPDEF");
                                aPg.Dispose();
                            }
                        }
                    }

                }

                if (bIncludeBoltOns)
                {
                    //=========== AP PANS ==================================
                    if (mzUtils.ListContains("APPANS", aBoltOnList, bReturnTrueForNullList: bIncludeBoltOns))
                    {
                        if (aAssy.DesignOptions.HasAntiPenetrationPans)
                        {
                            LName = bOneLayer ? aLayerName : "AP_PANS";

                            List<mdAPPan> pans = mdPartGenerator.APPans_DC(aAssy, aPart.Index); //  aPart.APPans(aAssy);
                            if (pans.Count > 0)
                            {
                                ln1 = aPln.ZAxis();
                                foreach (mdAPPan pan in pans)
                                {
                                    v1 = pan.CenterDXF;
                                    v2 = v1.WithRespectToPlane(bPln, aPln, 0);
                                    aPg = APPan_View_Plan(pan, bShowObscured: true, aCenter: v2, aRotation: aRotation, aLayerName: LName);
                                    aDir = aPg.InsertionPt.DirectionTo(v2, ref d2);
                                    aPg.Translate(aDir * d2);
                                    aEnts = aPg.SubEntities();
                                    if (pan.Y < aPart.Y)
                                        aEnts.RotateAbout(aPln.ZAxis(aStartPt: v2), 180);
                                    _rVal.AdditionalSegments.Append(aEnts, true, "APPANS");
                                    aPg.Dispose();
                                }

                            }
                        }
                    }

                    //=========== END ANGLES ==================================

                    if (mzUtils.ListContains("END ANGLES", aBoltOnList, bReturnTrueForNullList: bIncludeBoltOns))
                    {
                        LName = bOneLayer ? aLayerName : "END_ANGLES";

                        List<mdEndAngle> eangs;
                        foreach (var box in boxes.Where(b => bShowVirtual || !b.IsVirtual))
                        {
                            eangs = box.EndAngles();
                            if (eangs.Count > 0)
                            {

                                for (int i = 1; i <= eangs.Count; i++)
                                {
                                    aEA = eangs[i - 1];
                                    v1 = new dxfVector(aEA.X, aEA.Y);

                                    v2 = v1.WithRespectToPlane(bPln, aPln, 0);
                                    aPg = EndAngle_View_Plan(aEA, v2, aRotation, false, LName);
                                    aEnts = aPg.SubEntities();
                                    _rVal.AdditionalSegments.Append(aEnts, bAddClones: false, "END ANGLES", aEA.Y > box.Y ? "TOP" : "BOT");

                                    aPg.Dispose();

                                }

                            }
                        }

                    }
                    //=========== FINGER CLIPS ==================================

                    if (mzUtils.ListContains("FINGER CLIPS", aBoltOnList, bReturnTrueForNullList: bIncludeBoltOns))
                    {
                        LName = bOneLayer ? aLayerName :  "FINGER_CLIPS";

                        aPrts = mdUtils.FingerClips(aAssy, aStfs, aPart.Index, bTrayWide: true);
                        if (aPrts.Count > 0)
                        {

                            aFC = (mdFingerClip)aPrts.Item(1);
                            aPg = md_FingerClip_View_Plan(aFC, false, true, dxfVector.Zero, aRotation, LName);
                            aEnts = aPg.SubEntities();

                            for (int i = 1; i <= aPrts.Count; i++)
                            {
                                aFC = (mdFingerClip)aPrts.Item(i);
                                v2 = aFC.CenterDXF.WithRespectToPlane(bPln, aPln, 0);
                                bEnts = aEnts.Clone();
                                bEnts.Translate(v2);
                                if (aFC.Side == uppSides.Right)
                                {
                                    bEnts.RotateAbout(aPln.ZAxis(1, v2), 180);
                                }
                                _rVal.AdditionalSegments.Append(bEnts, false, "FINGER CLIPS");
                            }
                            aPg.Dispose();

                        }
                    }


                    //===========BAFFLES ==================================

                    if (mzUtils.ListContains("BAFFLES", aBoltOnList, bReturnTrueForNullList: bIncludeBoltOns))
                    {
                        if (aAssy.DesignFamily.IsEcmdDesignFamily() && aAssy.DesignOptions.CDP > 0)
                        {
                           

                            foreach (var box in boxes)
                            {
                               

                                List<mdBaffle> baffles = mdPartGenerator.DeflectorPlates_DC(aPart, aAssy);
                                LName = bOneLayer ? aLayerName : "DEFLECTORS";
                                foreach (mdBaffle baffle in baffles)
                                {
                                    baffle.SubPart(aAssy);
                                    v1 = box.IsVirtual ? new dxfVector(-baffle.CenterDXF.X, -baffle.CenterDXF.Y) : baffle.CenterDXF;
                                    v1 = v1.WithRespectToPlane(bPln, aPln, 0);
                                    aPg = Baffle_View_Plan(baffle, aAssy, false, v1, aRotation + (box.IsVirtual ? 180 : 0), LName);

                                    aEnts = aPg.SubEntities();
                                    _rVal.AdditionalSegments.Append(aEnts, aTag: "BAFFLES");

                                    List<uopInstance> partinsts = baffle.Instances.FindAll(x => x.DX == 0);
                                    foreach (uopInstance bafinst in partinsts)
                                    {
                                        aEnts = aPg.SubEntities();
                                        aEnts.Translate(aPln.YDirection * bafinst.DY);

                                        _rVal.AdditionalSegments.Append(aEnts, aTag: "BAFFLES");
                                    }

                                    aPg.Dispose();
                                }
                            }
                        }

                    }

                }

                if (virtul)
                {
                    _rVal.Rotate(180);
                }
                return _rVal;

            }
            catch (Exception e)
            {
                return _rVal;
                throw e;
            }


        }

   
        public static dxePolygon DC_View_Profile(mdDowncomer aPart, mdTrayAssembly aAssy, bool bLongSide = false, bool bIncludeCrossBraces = false, bool bIncludeBoltOns = false, iVector aCenter = null, double aRotation = 0, string aBoltOnList = "", string aLayerName = "GEOMETRY", bool bOneLayer = true)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {

                if (aPart == null) return _rVal;
                aAssy = aPart.GetMDTrayAssembly(aAssy);
                if (aAssy == null) return _rVal;

                string LName = string.Empty;
                dxfPlane aPln = null;
                double wht = aPart.How;
                double dkthk = aAssy.Deck.Thickness;
                double thk = aPart.Thickness;
                double lng = 0;
                colDXFVectors vrts = null;
                dxePolyline aPl = null;

                mdEndPlate aEP = null;
                dxePolygon aPg = null;
                mdEndSupport aES = null;
                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y));
                dxfVector v1 = null;
                dxfVector v2 = null;
                dxfDirection aDir = null;
                double d1 = 0;

                string sSide;
                colDXFEntities aEnts = null;
                colDXFEntities bEnts = null;
                dxeLine ln1 = null;
                bool bCenterDC = false;
                bool bEndAngs = false;
                List<mdStiffener> aStfs = aPart.Stiffeners();
                bCenterDC = Math.Round(aPart.X, 1) == 0;

                vrts = new colDXFVectors();
                aBoltOnList ??= string.Empty;

                dxfPlane downcomerPlane = new dxfPlane(aCenter != null ? aCenter : new dxfVector(aPart.Y, aPart.Z));
                _rVal.Plane = downcomerPlane;

                dxePolygon boxPolygon;

                if (!string.IsNullOrWhiteSpace(aBoltOnList)) bIncludeBoltOns = true;
               
                if (bIncludeBoltOns)
                {
                    bEndAngs = mzUtils.ListContains("END ANGLES", aBoltOnList, bReturnTrueForNullList: true);
                }

                bool showLeftSide = bLongSide ? aPart.X >= 0 : aPart.X < 0; // We want to show the left side when the caller needs to see the long side of the downcomer and the downcomer is on the right side of the tray, or the caller needs to see the short side of the downcomer and the downcomer resides on the left side of the tray.
                double f1 = showLeftSide ? -1 : 1;
                foreach (var box in aPart.Boxes)
                {
                    if (box.IsVirtual) continue;
                    //=========== DOWNCOMER BOX ==================================
                    dxfVector dcBoxCenter = new dxfVector(showLeftSide ? -box.CenterDXF.Y : box.CenterDXF.Y, box.CenterDXF.Z);
                    boxPolygon = DCBox_View_Profile(box, aAssy, aPart, true, showLeftSide, dcBoxCenter, aRotation, aLayerName);
                    if (bEndAngs)
                    {
                        boxPolygon.Vertex("EA_TOP").Linetype = dxfLinetypes.Invisible;
                        boxPolygon.Vertex("EA_BOT", aTagOccurance: 2).Linetype = dxfLinetypes.Invisible;
                    }
                    v1 = boxPolygon.Vertex("SA_TOP");
                    if (v1 != null)
                    {
                        v1.Linetype = dxfLinetypes.Invisible;
                        var withTwoOccurence = boxPolygon.Vertex("SA_BOT", aTagOccurance: 2);
                        if (withTwoOccurence != null)
                        {
                            withTwoOccurence.Linetype = dxfLinetypes.Invisible;
                        }
                        else
                        {
                            var withOneOccurence = boxPolygon.Vertex("SA_BOT", aTagOccurance: 1);
                            if (withOneOccurence != null)
                            {
                                withOneOccurence.Linetype = dxfLinetypes.Invisible;
                            }
                        }
                    }

                    boxPolygon.MoveTo(dcBoxCenter);
                    aEnts = boxPolygon.SubEntities();
                    _rVal.AdditionalSegments.Append(aEnts);
                    boxPolygon.Dispose();

                    aPln = new dxfPlane(dcBoxCenter);

                    //=========== SHELF ANGLE ==================================
                    mdShelfAngle shelf = showLeftSide ? box.ShelfAngle(bLeft: true) : box.ShelfAngle(bLeft: false);
                    double shelfLengthAboveBoxCenter = shelf.EdgeLn.MaxY - box.CenterDXF.Y;
                    double shelfLengthBelowBoxCenter = box.CenterDXF.Y - shelf.EdgeLn.MinY;
                    double shelfLengthToTheLeftOfBoxCenter = showLeftSide ? shelfLengthAboveBoxCenter : shelfLengthBelowBoxCenter;
                    double shelfLengthToTheRightOfBoxCenter = showLeftSide ? shelfLengthBelowBoxCenter : shelfLengthAboveBoxCenter;
                    lng = shelf.Length / 2;
                    double sw = aPart.ShelfWidth;
                    
                    colDXFVectors verts = new colDXFVectors(aPln.Vector(-shelfLengthToTheLeftOfBoxCenter, -thk), aPln.Vector(shelfLengthToTheRightOfBoxCenter, -thk), aPln.Vector(shelfLengthToTheRightOfBoxCenter), aPln.Vector(-shelfLengthToTheLeftOfBoxCenter), aPln.Vector(-shelfLengthToTheLeftOfBoxCenter, -sw), aPln.Vector(shelfLengthToTheRightOfBoxCenter, -sw), aPln.Vector(shelfLengthToTheRightOfBoxCenter, -thk));


                    aPl = new dxePolyline(verts, false, _rVal.DisplaySettings, aPln);
                    if (!bOneLayer) aPl.LayerName = "SHELF_ANGLES";

                    _rVal.AdditionalSegments.Add(aPl, bAddClone: false, aTag: "SHELF");

                    //=========== END PLATES ==================================
                    var endPlates = box.EndPlates();
                    for (int i = 0; i < endPlates.Count; i++)
                    {
                        aEP = endPlates[i];
                        ULINE limit = aEP.LimitLn;
                        v1 = showLeftSide ? new dxfVector(-limit.sp.ToDXFVector().Y) : new dxfVector(limit.ep.ToDXFVector().Y);

                        LName = bOneLayer ? aLayerName : "END_PLATES";

                        aPg = EndPlate_View_Profile(aEP, true, showLeftSide, true, v1, aRotation, LName);
                        if (bEndAngs) aPg.Vertex("TAB_BOT").Linetype = dxfLinetypes.Invisible;

                        aEnts = aPg.SubEntities();
                        _rVal.AdditionalSegments.Append(aEnts, true, "END PLATE", aEP.Y > box.Y ? "TOP" : "BOT");
                        aPg.Dispose();
                    }

                    ln1 = aPln.YAxis();

                    //=========== END SUPPORTS ==================================

                    LName = bOneLayer ? aLayerName : "END_SUPPORTS";
                    sSide = showLeftSide ? "LEFT" : "RIGHT";

                    aES = box.EndSupport(bTop: true);
                    v1 = aES.GenHoles("END ANGLE", sSide).Item(1).Item(1).CenterDXF;
                    v1 = new dxfVector(showLeftSide ? -v1.Y : v1.Y, v1.Z);

                    aPg = EndSupport_View_Profile(aES, aAssy, aPart, bEndAngs, showLeftSide, true, v1, aRotation, LName);

                    aEnts = aPg.SubEntities();
                    _rVal.AdditionalSegments.Append(aEnts, true, "END SUPPORT", "TOP");
                    aPg.Dispose();

                    aES = box.EndSupport(bTop: false);
                    v1 = aES.GenHoles("END ANGLE", sSide).Item(1).Item(1).CenterDXF;
                    v1 = new dxfVector(showLeftSide ? -v1.Y : v1.Y, v1.Z);

                    aPg = EndSupport_View_Profile(aES, aAssy, aPart, bEndAngs, showLeftSide, true, v1, aRotation, LName);

                    aEnts = aPg.SubEntities();
                    _rVal.AdditionalSegments.Append(aEnts, true, "END SUPPORT", "BOT");
                    aPg.Dispose();

                    if (bIncludeBoltOns)
                    {

                        colUOPParts aPrts = null;

                        //=========== END ANGLES ==================================
                        if (bEndAngs)
                        {
                            LName = bOneLayer ? aLayerName : "END_ANGLES";

                            List<mdEndAngle> eas = mdPartGenerator.EndAngles_ASSY(aAssy, bApplyInstances: false, box.DowncomerIndex);
                            eas = showLeftSide ? eas.FindAll(x => x.Side == uppSides.Left) : eas.FindAll(x => x.Side == uppSides.Right);

                            for (int i = 1; i <= eas.Count; i++)
                            {
                                mdEndAngle aEA = eas[i - 1];
                                v1 = new dxfVector(showLeftSide ? -aEA.Y : aEA.Y, aEA.Z);
                                aPg = EndAngle_View_Profile(aEA, v1, aRotation, LName);
                                aEnts = aPg.SubEntities();
                                _rVal.AdditionalSegments.Append(aEnts, true, "END ANGLE", aEA.CenterDXF.Y >= box.CenterDXF.Y ? "TOP" : "BOT");
                            }

                        }
                        //=========== FINGER CLIPS ==================================

                        if (mzUtils.ListContains("FINGER CLIPS", aBoltOnList, bReturnTrueForNullList: true))
                        {
                            mdFingerClip aFC = null;
                            LName = bOneLayer ? aLayerName : "FINGER_CLIPS";
                            
                            aPrts = mdUtils.FingerClips(aAssy, aStfs,aDCIndex: aPart.Index, aSide: showLeftSide ? uppSides.Left : uppSides.Right, bTrayWide: false);
                            if (aPrts.Count > 0)
                            {
                                aFC = (mdFingerClip)aPrts.Item(1);
                                v1 = new dxfVector(showLeftSide ? -aFC.Y : aFC.Y, aFC.Z);
                                aPg = md_FingerClip_View_Profile(aFC, false, v1, aRotation, LName);
                                aEnts = aPg.SubEntities(false);
                                if (!aFC.Suppressed)
                                {
                                    _rVal.AdditionalSegments.Append(aEnts, true, "FINGER CLIP");
                                }


                                for (int j = 2; j <= aPrts.Count; j++)
                                {
                                    aFC = (mdFingerClip)aPrts.Item(j);
                                    if (!aFC.Suppressed)
                                    {
                                        v2 = new dxfVector(showLeftSide ? -aFC.Y : aFC.Y, aFC.Z);
                                        aDir = v1.DirectionTo(v2, ref d1);
                                        bEnts = aEnts.Projected(aDir, d1);
                                        _rVal.AdditionalSegments.Append(bEnts, false, "FINGER CLIP");
                                    }
                                }

                            }
                        }

                        //=========== STIFFENERS ==================================
                        if (mzUtils.ListContains("STIFFENERS", aBoltOnList, bReturnTrueForNullList: true))
                        {
                            LName = bOneLayer ? aLayerName : "STIFFENERS";

                            var boxStiffenerOrdinates = box.StiffenerYs(aPart);
                            mdStiffener stiffenerInstance = new mdStiffener(box, 0);

                            if (boxStiffenerOrdinates != null && stiffenerInstance != null)
                            {
                                v1 = dxfVector.Zero;
                                aPg = Stiffener_View_Profile(stiffenerInstance, !bLongSide, bVisiblePartOnly: true, aCenter: v1, aRotation: aRotation, aLayerName: LName);
                                aEnts = aPg.SubEntities();

                                foreach (var ordinate in boxStiffenerOrdinates)
                                {
                                    v2 = new dxfVector(showLeftSide ? -ordinate : ordinate, stiffenerInstance.Z);
                                    aDir = v1.DirectionTo(v2, ref d1);
                                    bEnts = aEnts.Projected(aDir, d1);
                                    _rVal.AdditionalSegments.Append(bEnts, false, "STIFFENER");
                                }

                                aPg.Dispose();
                            }
                        }

                        //=========== BAFFLES ==================================
                        if (mzUtils.ListContains("BAFFLES", aBoltOnList, bReturnTrueForNullList: true))
                        {

                            if (aAssy.DesignFamily.IsEcmdDesignFamily() && aAssy.DesignOptions.CDP > 0)
                            {
                                LName = bOneLayer ? aLayerName : "DEFLECTORS";

                                var nonVirtualBox = box.GetNonVirtualCounterpart();
                                IEnumerable<mdBaffle> baffles = mdPartGenerator.DeflectorPlates_DC(aPart, aAssy); // .DeflectorPlates(aPart.Index);
                                foreach (mdBaffle baffle in baffles)
                                {
                                    v1 = baffle.CenterDXF;
                                    v2 = downcomerPlane.Vector(baffle.Y * f1, baffle.Z);
                                    aPg = Baffle_View_Profile(baffle, bLongSide, true, v2, aRotation, LName);
                                    if(aPg != null)
                                    {
                                        aEnts = aPg.SubEntities();
                                        _rVal.AdditionalSegments.Append(aEnts, false, "BAFFLE");
                                        List<uopInstance> partinsts = baffle.Instances.FindAll(x => x.DX == 0);
                                        foreach (uopInstance bafinst in partinsts)
                                        {
                                            aEnts = aPg.SubEntities();
                                            if (bafinst.Inverted)
                                            {
                                                aEnts.Mirror(aPg.Plane.YAxis());
                                            }
                                            aEnts.Translate(aPln.XDirection * bafinst.DY * f1);

                                            _rVal.AdditionalSegments.Append(aEnts, aTag: "BAFFLES");
                                        }

                                    }

                                }

                            }

                        }

                        //=========== AP PANS ==================================
                        if (mzUtils.ListContains("APPANS", aBoltOnList, bReturnTrueForNullList: true))
                        {
                            if (aAssy.DesignOptions.HasAntiPenetrationPans)
                            {
                                LName = bOneLayer ? aLayerName : "AP_PANS";
                                List<mdAPPan> pans = mdPartGenerator.APPans_DC(aAssy, aPart.Index);


                                if (pans.Count > 0)
                                {
                                    foreach (mdAPPan pan in pans)
                                    {
                                        v1 = aPln.Vector(pan.Y * f1, pan.Z);
                                        aPg = APPan_View_Profile(pan, aShowObscured: true, aCenter: v1, aRotation: aRotation, bLongSide: bLongSide, aLayerName: LName);
                                        aEnts = aPg.SubEntities();
                                        if (pan.Y < aPart.Y)
                                        {
                                            aEnts.Mirror(aPg.Plane.YAxis());
                                        }

                                        _rVal.AdditionalSegments.Append(aEnts, false, aTag: "APPAN");

                                    }


                                }
                            }

                        }
                        if (bIncludeCrossBraces)
                        {
                            if (aAssy.DesignOptions.CrossBraces)
                            {
                                LName = bOneLayer ? aLayerName : "CROSSBRACES";

                                mdCrossBrace aCB = null;

                                aCB = aAssy.CrossBrace;
                                if (aCB != null)
                                {
                                    if (aCB.X - aCB.Length / 2 < bPln.X && aCB.X + aCB.Length / 2 > 0)
                                    {
                                        v1 = aCB.CenterDXF.WithRespectToPlane(bPln);
                                        v1 = aPln.Vector(aCB.Y * f1, aCB.Z);
                                        aPg = md_CrossBrace_View_Profile(aCB, aAssy, bLongSide, false, v1, aRotation, LName);
                                        aEnts = aPg.SubEntities();
                                        _rVal.AdditionalSegments.Append(aEnts, false);


                                    }
                                    aCB.X -= aCB.X * 2;
                                    aCB.Y -= aCB.Y * 2;
                                    if (aCB.X - aCB.Length / 2 < bPln.X && aCB.X + aCB.Length / 2 > 0)
                                    {
                                        v1 = aCB.CenterDXF.WithRespectToPlane(bPln);
                                        v1 = aPln.Vector(aCB.Y * f1, aCB.Z);
                                        aPg = md_CrossBrace_View_Profile(aCB, aAssy, bLongSide, false, v1, aRotation, aLayerName);
                                        aEnts = aPg.SubEntities();
                                        _rVal.AdditionalSegments.Append(aEnts, false);
                                        aCB.X -= aCB.X * 2;
                                        aCB.Y -= aCB.Y * 2;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

                throw e;
            }
            return _rVal;
        }

        /// <summary>
        ///returns a dxePolygon that is used to draw the end view of the cross brace
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aAssy_UNUSED"></param>
        /// <param name="bLongSide"></param>
        /// <param name="aSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon md_CrossBrace_View_Profile(mdCrossBrace aPart, mdTrayAssembly aAssy, bool bLongSide = false, bool aSuppressFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "")
        {
            dxePolygon mdCrossBrace_View_Profile = null;
            try
            {
                if (aPart == null)
                {
                    return mdCrossBrace_View_Profile;
                }

                mdCrossBrace_View_Profile = new dxePolygon();

                double cX = 0;
                double cY = 0;
                double wd;
                dxfVector cp;
                uopHoles Hls;
                uopHole aHl;
                dxfPlane aPln;
                double f1;

                double lng;
                double thk;

                Hls = aPart.GenHoles(aAssy).Item(1);

                cp = new dxfVector(aPart.X, aPart.Y);

                wd = aPart.Width / 2;
                lng = aPart.Length / 2;
                thk = aPart.Thickness;
                double rZ = 0;
                cp.GetComponents(ref cX, ref cY, ref rZ);

                f1 = cY > 0 ? 1 : -1;
                aPln = xPlane(cX, cY, aCenter, aRotation);

                mdCrossBrace_View_Profile.LayerName = aLayerName;
                mdCrossBrace_View_Profile.Plane = aPln;
                mdCrossBrace_View_Profile.InsertionPt = aPln.Origin;
                mdCrossBrace_View_Profile.Closed = false;
                mdCrossBrace_View_Profile.Vertices.Add(aPln, -lng, (wd - thk) * f1, aLineType: dxfLinetypes.Hidden);
                mdCrossBrace_View_Profile.Vertices.Add(aPln, lng, (wd - thk) * f1);
                mdCrossBrace_View_Profile.Vertices.Add(aPln, lng, -wd * f1);
                mdCrossBrace_View_Profile.Vertices.Add(aPln, -lng, -wd * f1);
                mdCrossBrace_View_Profile.Vertices.Add(aPln, -lng, wd * f1);
                mdCrossBrace_View_Profile.Vertices.Add(aPln, lng, wd * f1);
                mdCrossBrace_View_Profile.Vertices.Add(aPln, lng, (wd - thk) * f1);

                for (int i = 1; i <= Hls.Count; i++)
                {
                    aHl = Hls.Item(i);
                    if (i == 1)
                    {
                        wd = aHl.Radius;
                        mdCrossBrace_View_Profile.AdditionalSegments.Add(aPln.CreateCircle(aHl.X - cp.X, aHl.Y - cp.Y, wd, aLayerName: aLayerName));
                    }
                }

                //mdCrossBrace_View_Profile = new dxePolygon();
                //if (aPart == null)
                //{
                //    return mdCrossBrace_View_Profile;

                //}

                //double cX = 0;

                //double cY = 0;

                //double wd = 0;

                //uopHole aHl = null;

                //dxfVector cp = null;

                //double f1 = 0;

                //dxfPlane aPln = null;

                //double ht = 0;

                //double thk = 0;

                //cp = aPart.CenterDXF;

                //wd = aPart.Width / 2;
                //ht = aPart.Height;
                //thk = aPart.Thickness;
                //double rX = 0.0, rY = 0.0, rZ = 0.0;
                //cp.GetComponents(ref rX, ref rY, ref rZ);
                //f1 = 1;
                //if (cp.Y < 0)
                //{
                //    f1 = -f1;
                //}
                //if (bLongSide)
                //{
                //    cY = -cY;
                //    f1 = -f1;
                //}

                //aPln = xPlane(cX, cY, aCenter, aRotation);


                //aHl = aPart.BoltHole;

                //mdCrossBrace_View_Profile.LayerName = aLayerName;
                //mdCrossBrace_View_Profile.Plane = aPln;
                //mdCrossBrace_View_Profile.InsertionPt = aPln.Origin;
                //if (aSuppressFillets)
                //{
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, -wd * f1);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, wd * f1);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, wd * f1, -ht);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, (wd - thk) * f1, -ht);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, (wd - thk) * f1, -thk);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, -wd * f1, -thk);
                //}
                //else
                //{
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, -wd * f1);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, (wd - 2 * thk) * f1, aVertexRadius: (2 * thk * -f1));
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, wd * f1, -2 * thk);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, wd * f1, -ht);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, (wd - thk) * f1, -ht);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, (wd - thk) * f1, -2 * thk);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, (wd - 2 * thk) * f1, -thk);
                //    mdCrossBrace_View_Profile.Vertices.Add(aPln, -wd * f1, -thk);

                //}

                //ht = aHl.Radius;
                //mdCrossBrace_View_Profile.AdditionalSegments.Add((dxfEntity)aPln.CreateLine((wd - aHl.Inset) * f1 - ht, 0, (wd - aHl.Inset) * f1 - ht, -thk, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: aHl.Tag));
                //mdCrossBrace_View_Profile.AdditionalSegments.Add((dxfEntity)aPln.CreateLine((wd - aHl.Inset) * f1 + ht, 0, (wd - aHl.Inset) * f1 + ht, -thk, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: aHl.Tag));

            }
            catch (Exception)
            {

                throw;
            }
            return mdCrossBrace_View_Profile;
        }
        /// <summary>
        /// //^returns a dxePolygon that is used to draw the profile view of the pan
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aShowObscured"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="bLongSide"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon APPan_View_Profile(mdAPPan aPart, bool aShowObscured = false, iVector aCenter = null, double aRotation = 0, bool bLongSide = true, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {


                if (aPart == null) return _rVal;

                dxfVector v0 = aPart.CenterDXF;
                double thk = aPart.Thickness;
                double ht = aPart.Height;
                double cX = v0.Y;
                if (bLongSide) cX = -cX;
                double cY = aPart.Z;

                double iset = aPart.HoleInset;
                colDXFVectors verts = new colDXFVectors();
                double tabht = aPart.TabHeight;
                double lg = aPart.Length;
                double halfthk = thk / 2;
                double tblen = aPart.TabLength;
                double flnglen = aPart.FlangeLength;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
                double f1 = (v0.Rotation == 0) ? -1 : 1;
                uopHole bHl = aPart.Hole;
                if (bLongSide) f1 *= -1;

                if (!aPart.OpenEnded)
                {

                    verts.Add(aPln, -iset * f1);
                    verts.AddRelative(aX: (flnglen - halfthk) * f1, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: -(ht - thk), aPlane: aPln);
                    verts.AddRelative(aX: (lg + thk) * f1, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: ht - halfthk + tabht + halfthk, aPlane: aPln);
                    verts.AddRelative(aX: (tblen - halfthk) * f1, aY: 0, aPlane: aPln);

                }
                else
                {
                    f1 = -f1;
                    verts.Add(aPln, iset * f1);
                    verts.AddRelative(aX: (-flnglen + halfthk) * f1, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: -(ht - thk), aPlane: aPln);
                    verts.AddRelative(aX: -(aPart.LongLength - thk) * f1, aY: 0, aPlane: aPln);

                }


                _rVal = (dxePolygon)dxfPrimatives.CreateTrace(verts, thk, aReturnPolygon: true);
                _rVal.LayerName = aLayerName;
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
                if (!aPart.OpenEnded)
                {
                    dxfVector v1 = _rVal.Vertex(4) + aPln.YDirection * ht;

                    _rVal.AdditionalSegments.AddLine(v1, v1 + aPln.XDirection * thk, new dxfDisplaySettings(aLayerName));
                }
                _rVal.AdditionalSegments.Add(new dxePoint(aPln.Vector(0, 0)) { LayerName = "DEFPOINTS", Tag = "BOLT_HOLE" });
                _rVal.AdditionalSegments.Add(aPln.CreateLine(-bHl.Radius, -halfthk, -bHl.Radius, halfthk, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                _rVal.AdditionalSegments.Add(aPln.CreateLine(bHl.Radius, -halfthk, bHl.Radius, halfthk, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                return _rVal;

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        ///    //^returns a dxePolygon that is used to draw the side view of the baffle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bLongSide"></param>
        /// <param name="aAssy"></param>
        /// <param name="bHiddenSot"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="rNotchCount"></param>
        /// <param name="bSuppressHoles"></param>
        /// <returns></returns>
        //public static dxePolygon Baffle_View_Profile( mdBaffle aPart, bool bLongSide, mdTrayAssembly aAssy, bool bHiddenSot = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bSuppressHoles = false )
        //{
        //    int ncnt = 0;
        //    return Baffle_View_Profile( aPart, bLongSide, aAssy, ref ncnt, bHiddenSot, aCenter, aRotation, aLayerName, bSuppressHoles );
        //}
        public static dxePolygon Baffle_View_Profile(mdBaffle aPart, bool bLongSide, bool bHiddenSot = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bSuppressHoles = false, bool bAddSlotPoints = false, bool bCenterElevationToMounts = false, bool bOmmitNotches = false)
        {
            dxePolygon _rVal = null;
            if (aPart == null) return _rVal;
            if (aPart.Height <= 0) return _rVal;

            double f1 = bLongSide ? -1 : 1;
            double y0 = aPart.Y;

            double x1 = aPart.Top - y0;
            double x2 = aPart.Bottom - y0;
            mzUtils.SortTwoValues(true, ref x1, ref x2);
            uopHoleArray holes = new uopHoleArray();
            double cX = y0 * f1;
            double cY = aPart.Z;
            double dY = 0;
       

            dxePoint pt;

            dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y, aPart.Z));
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
            dxfVector v1;

            colDXFVectors verts = DeflectorPlateVerts(aPart, out holes, bLongSide, aCenter, aRotation, aPlane: aPln, bIncludeHoleCenters: !bSuppressHoles || bAddSlotPoints, bSuppressRadii: false,  bOmmitNotches: bOmmitNotches);
            colDXFVectors slotcenters = (!bSuppressHoles) ? verts.GetByTag("SLOT CENTER", bRemove: true) : new colDXFVectors();

            _rVal = new dxePolygon(verts, bClosed: true) { LayerName = aLayerName, InsertionPt = aPln.Origin };


            if (!bSuppressHoles || bAddSlotPoints || bCenterElevationToMounts)
            {

                for (int j = 1; j <= holes.Count; j++)
                {
                    uopHoles Hls = holes.Item(j);
                    for (int i = 1; i <= Hls.Count; i++)
                    {
                        uopHole aHl = Hls.Item(i);
                        UVECTOR u1 = UVECTOR.FromDXFVector(aHl.Center.ToDXFVector(false, false, aHl.Elevation).WithRespectToPlane(bPln));
                        v1 = aPln.Vector(u1.Y * f1, Hls.Elevation - aPart.Z);

                        if (bCenterElevationToMounts && i == 1)
                        {
                            dY = v1.Y - aPln.Y;
                        }
                        dxePolyline aPl;
                        if (!bSuppressHoles)
                        {
                            aPl = (dxePolyline)dxfPrimatives.CreatePill(v1, aHl.Length, aHl.Radius * 2, 0, aPlane: aPln);
                            if (bHiddenSot) aPl.Linetype = dxfLinetypes.Hidden;
                            aPl.LayerName = aLayerName;
                            aPl.TFVSet(aHl.Tag, aHl.Flag);
                            _rVal.AdditionalSegments.Add(aPl);

                        }
                        if (bAddSlotPoints)
                        {
                            pt = new dxePoint(v1)
                            {
                                LayerName = aLayerName
                            };
                            pt.TFVSet(aHl.Tag, aHl.Flag);
                            _rVal.AdditionalSegments.Add(pt);
                        }
                    }
                }
            }
            if (dY != 0)
            {
                v1 = _rVal.Plane.YDirection * -dY;
                verts.Translate(v1);
                _rVal.AdditionalSegments.Translate(v1);
            }

            _rVal.Plane = aPln;

            if (aPln != null) _rVal.InsertionPt = aPln.Origin;


            return _rVal;
        }

        /// <summary>
        ///  //#1scale factor to apply to the returned polygon
        ///^returns a dxePolygon that is used to draw the side view of the stiffener
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="bVisiblePartOnly"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bCenterBaffleMount"></param>
        /// <returns></returns>
        public static dxePolygon Stiffener_View_Profile(mdStiffener aPart, bool bShowObscured = false, bool bVisiblePartOnly = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bCenterBaffleMount = false, bool bMirrored = false)
        {
            dxePolygon _rVal = new dxePolygon();
            if (aPart == null) return _rVal;
            mdTrayAssembly aAssy = aPart.GetMDTrayAssembly();
            if (aAssy == null) return _rVal;

            mdDowncomerBox box = aPart.DowncomerBox;
            box ??= aAssy.Downcomers.Boxes().FirstOrDefault();
            if (box == null) return _rVal;

            double thk = aPart.Thickness;
            double ht = aPart.Height;
            double cX = aPart.Y;
            double cY = aPart.Z;
            double f1 = bMirrored ? -1 : 1;
            double wd = aPart.FlangeWidth;
            uopHoleArray aHls = aPart.GenHoles(box);
            uopHole aHole = aHls.Hole("MOUNT", "RIGHT");
            bool bBaffs = aPart.SupportsBaffle;
            double bafthk = aPart.BaffleThickness;
            double d1 = 1 + bafthk + 0.625;
            double y5 = box.WeirHeight - cY;  // top of dc box relative to part plane
            uopHole aSlot = bBaffs ? aHls.Hole("BAFFLE MOUNT", 1) : null;
            if (aSlot == null) bCenterBaffleMount = false;
            double dy = (aSlot != null && bCenterBaffleMount) ? -(aSlot.Z - aPart.Z) : 0;
            colDXFVectors verts = new colDXFVectors();
            double mntHt = aPart.BaffleMountHeight;
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
            string lt = bShowObscured ? dxfLinetypes.Hidden : "";
            //if (bMirrored)
            //    aPln = new dxfPlane(aPln.Origin, aPln.XDirection.Inverse(), aPln.YDirection);
            _rVal.LayerName = aLayerName;
            //f1 = 1;

            if (!bVisiblePartOnly)
            {
                verts.Add(aPln, (aHole.Inset - wd + thk) * f1, -d1 + dy, aLineType: dxfLinetypes.Hidden);
                verts.AddRelative(0, ht, aPlane: aPln, aLineType: lt);
                verts.AddRelative(-thk * f1, 0, aPlane: aPln, aLineType: lt);
                verts.AddRelative(0, -ht, aPlane: aPln, aLineType: lt);
                verts.AddRelative(wd * f1, 0, aPlane: aPln, aLineType: lt);
                verts.AddRelative(0, ht, aPlane: aPln, aLineType: lt);
                verts.AddRelative(-(wd - thk) * f1, 0, aPlane: aPln, aLineType: lt);
            }

            if (bBaffs)
            {
                verts.Add(aPln, (aHole.Inset - wd) * f1, ht - d1 + dy);
                verts.AddRelative(0, 0.25, aPlane: aPln, aLineType: lt);
                verts.AddRelative(0, mntHt, aPlane: aPln, aLineType: lt);
                verts.AddRelative(wd * f1, 0, aPlane: aPln, aLineType: lt);
                verts.AddRelative(0, -mntHt, aPlane: aPln, aLineType: lt);
                verts.AddRelative(-(wd - thk) * f1, 0, aPlane: aPln);
                verts.AddRelative(0, -0.25, aPlane: aPln);
                verts.AddRelative(0, 0.25, aPlane: aPln);
                verts.AddRelative(-thk * f1, 0, aPlane: aPln);


                if (!bVisiblePartOnly)
                {
                    verts.AddRelative(thk * f1, 0, aPlane: aPln, aLineType: dxfLinetypes.Hidden);
                    verts.AddRelative(0, mntHt, aPlane: aPln);
                }

            }

            _rVal.Vertices = verts;
            if (!bVisiblePartOnly)
            {
                dxeArc hole = new dxeArc(aPln.Vector(0, dy), aHole.Radius) { LayerName = aLayerName };

                if (bShowObscured) hole.Linetype = dxfLinetypes.Hidden;

                hole.TFVSet(aHole.Tag, aHole.Flag);
                _rVal.AdditionalSegments.Add(hole);
            }

            if (aSlot != null)
            {
                dxfVector v1 = !bCenterBaffleMount ? aPln.Vector(0, aSlot.Z - aPart.Z) : aPln.Vector(0, 0);
                dxePolyline sltply = (dxePolyline)dxfPrimatives.CreatePill(v1, aSlot.Length, aSlot.Diameter, 90, aPlane: aPln, bReturnAsPolygon: false);
                if (bShowObscured) sltply.Linetype = dxfLinetypes.Hidden;

                sltply.LayerName = aLayerName;
                sltply.TFVSet(aSlot.Tag, aSlot.Flag);
                //_rVal.AdditionalSegments.Append(sltply.Segments,aTag:aSlot.Tag,aFlag:aSlot.Flag);
                _rVal.AdditionalSegments.Add(sltply);
                dxePoint sltctr = _rVal.AdditionalSegments.AddPoint(v1, aLayerName, aTag: aSlot.Tag, aFlag: aSlot.Flag);

                //if (bCenterBaffleMount)
                //{
                //    aPln.Origin = v1;
                //}

            }

            _rVal.Plane = aPln;
            _rVal.InsertionPt = aPln.Origin;

            _rVal.Closed = false;
            return _rVal;
        }



        private static dxfPlane xPlane(double aCX, double aCY, iVector aCenter = null, double aRotation = 0, double aScale = 1, dxfPlane aDirectionPlane = null)
        {
            dxfPlane _rVal = (aDirectionPlane == null) ? dxfPlane.World : new dxfPlane(aDirectionPlane);

            if (aCenter != null)
            {
                //          double aCZ = 0.0;
                aCX = aCenter.X;
                aCY = aCenter.Y;
            }

            _rVal.SetCoordinates(aCX * aScale, aCY * aScale);
            if (aRotation != 0)
                _rVal.Revolve(aRotation);

            return _rVal;
        }

        /// <summary>
        ///  //#1the parent tray assembly for this part
        ///#2the parent downcomer for this part
        ///#3flag to add hidden lines where the profile is obscured
        ///^returns a dxePolygon that is used to draw the profile view of the end plate
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aAssy"></param>
        /// <param name="aDC"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="bLongSide"></param>
        /// <param name="bVisiblePartOnly"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon EndSupport_View_Profile(mdEndSupport aPart, mdTrayAssembly aAssy, mdDowncomer aDC, bool bShowObscured = false, bool bLeftSide = true, bool bVisiblePartOnly = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            if (aPart == null) return null;

            if (aDC == null) aDC = aPart.MDDowncomer;
            if (aDC == null) return null;

            colDXFVectors verts = new colDXFVectors();
            uopHoleArray aHls = aPart.GenHoles();
            uopHoles Hls = aHls.Item("END ANGLE");
            uopHole aHl  = bLeftSide ? Hls.GetFlagged("LEFT") : Hls.GetFlagged("RIGHT") ;
            double thk = aPart.Thickness;
            double cY = aPart.X;
            double cX = aPart.Y;
            dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y));
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
            double f1 = bLeftSide ? -1 : 1;
            if (aPart.Side == uppSides.Bottom) f1 *= -1;
            uopLine weirLine = aPart.WeirLine(bLeftSide);
            double lng = weirLine.Length;
            dxfVector v1 = aHl.CenterDXF.WithRespectToPlane(bPln, aPln, 0);

            double ht = aPart.Height; // 1 + thk
            double y1 = ht - aHl.Z;
            double y4 = -aHl.Z;
            double y3 = y4 + 2 * thk;
            double y2 = y4 + 1 + thk;
            double x1 = aHl.Inset;
            double x3 = x1 - lng;
            double x2 = x3 + mdEndSupport.NotchDepth;

            if (bShowObscured)
            {
                verts.Add(aPln, x2 * f1, y4);
            }

            verts.Add(aPln, x1 * f1, y4);
            verts.Add(aPln, x1 * f1, y2);
            verts.Add(aPln, x1 * f1, y1);
            verts.Add(aPln, x3 * f1, y1);
            verts.Add(aPln, x3 * f1, y2);
            verts.Add(aPln, x3 * f1, y3);
            verts.Add(aPln, x2 * f1, y3);
            if (!bShowObscured)
            {
                verts.Add(aPln, x2 * f1, y4);
            }

            if (bShowObscured)
            {
                verts.Item(1).Linetype = dxfLinetypes.Hidden;
                verts.Item(5).Linetype = dxfLinetypes.Invisible;
            }


            dxePolygon _rVal = new dxePolygon(verts, aPln.Origin, !bShowObscured, aPlane: aPln) { LayerName = aLayerName };

            if (!bShowObscured)
            {
                _rVal.AdditionalSegments.AddArc(aPln.Origin, aHl.Radius);
            }
            else
            {
                _rVal.AdditionalSegments.AddArc(aPln.Origin, aHl.Radius, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden));
            }

            _rVal.AdditionalSegments.Add(aPln.CreateLine(x1 * f1, y4 + thk, x2 * f1, y4 + thk, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));

            return _rVal;
        }




        /// <summary>
        ///#1scale factor to apply to the returned polygon
        ///^returns a dxePolygon that is used to draw the elevation view of the finger clip
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bShowTabsBent"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon md_FingerClip_View_Profile(mdFingerClip aPart, bool bShowTabsBent = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            if (aPart == null) return null;
            dxePolygon _rVal = new dxePolygon();

            double thk = aPart.Thickness;
            double ht = aPart.Height;
            double wd = aPart.Width;
            double span = aPart.SlotSpan;
            double swd = aPart.SlotWidth;
            double gp = aPart.BendGap;
            uopHoleArray pholes = aPart.GenHoles(null);
            double cX = aPart.Y;
            double cY = aPart.Z;
            double lg = aPart.Length;
            double d1 = ht - thk;
            double d2 = (lg - span - swd) / 2;
            double d3 = span - swd;
            double d4 = gp - thk;
            colDXFVectors verts = new colDXFVectors();
            uopHole aHl = pholes.Count > 0 ? pholes.Item(1).Item(1) : null;
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
            dxfDisplaySettings dsp = new dxfDisplaySettings(aLayerName);

            try
            {

                if (bShowTabsBent)
                {
                    verts.Add(aPln, -d4, -0.5 * lg);

                    verts.Add(aPln.VectorRelative(verts.LastVector(), -thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: d2));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), d4));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: swd));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), -thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: d3));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: swd));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), -thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), -d4));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), -thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: d2));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), d1 + d4));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: -lg));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), -(d1 + d4)));


                    _rVal.AdditionalSegments.AddLine(verts.Item(1), verts.Item(4), dsp);
                    _rVal.AdditionalSegments.AddLine(verts.Item(13), verts.Item(16), dsp);
                    _rVal.AdditionalSegments.AddLine(verts.Item(7), verts.Item(10), dsp);
                    _rVal.AdditionalSegments.AddLine(verts.Item(5), verts.Item(8), dsp);
                    _rVal.AdditionalSegments.AddLine(verts.Item(9), verts.Item(12), dsp);
                    _rVal.Closed = true;

                }
                else
                {
                    _rVal.Closed = false;
                    verts.Add(aPln, -0.5 * lg, thk);
                    verts.Add(aPln.VectorRelative(verts.LastVector(), d2));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: -thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), swd));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: -thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), d3));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: -thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), swd));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: -thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), d2));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: -thk));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), -lg));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: wd));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), lg));
                    verts.Add(aPln.VectorRelative(verts.LastVector(), aYDisplacement: -d1));

                }
                _rVal.Vertices = verts;
                _rVal.LayerName = aLayerName;
                _rVal.Plane = aPln;

                _rVal.InsertionPt = aPln.Origin;
                if (aHl != null)
                    _rVal.AdditionalSegments.AddArc(aPln.Vector(0, aHl.Inset), aHl.Radius, aDisplaySettings: dxfDisplaySettings.Null(aLayer: aLayerName));

            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }
        /// <summary>
        /// #1the parent downcomer of the end plate
        ///#2flag to add hidden lines where the profile is obscured
        ///^returns a dxePolygon that is used to draw the profile view of the end plate
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon EndAngle_View_Profile(mdEndAngle aPart, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            if (aPart == null) return null;
            dxePolygon _rVal;
            try
            {

                double thk = aPart.Thickness;
                double cX = aPart.Y;
                double cY = aPart.Z;
                double lg = aPart.Length;
                double ht = aPart.Height;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
                double iset = aPart.HoleInset;
                double hdia = aPart.HoleDiameter / 2;

                _rVal = (dxePolygon)uopGlobals.UopGlobals.Primatives.Angle_Side(aPln.Origin, ht, lg, thk, false, 180, 0, aPln, true, aYOffset: -ht / 2);

                _rVal.LayerName = aLayerName;
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;

                _rVal.AdditionalSegments.AddArc(aPln.Vector(-((0.5 * lg) - iset), 0.625), hdia, aDisplaySettings: dxfDisplaySettings.Null(aLayer: aLayerName));
                _rVal.AdditionalSegments.AddArc(aPln.Vector((0.5 * lg) - iset, 0.625), hdia, aDisplaySettings: dxfDisplaySettings.Null(aLayer: aLayerName));

            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }

        /// <summary>
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aDC"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="bLongSide"></param>
        /// <param name="bVisiblePartOnly"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bSuppressHoles"></param>
        /// <returns></returns>
        public static dxePolygon EndPlate_View_Profile(mdEndPlate aPart, bool bShowObscured = false, bool bLeftSide = false, bool bVisiblePartOnly = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bSuppressHoles = false)
        {
            if (aPart == null) return null;
            dxePolygon _rVal = new dxePolygon();


            //    If Not aPart.IsTriangular Then bLongSide = False
            double thk = aPart.Thickness;
            double ht = aPart.Height;
            double ntch = aPart.NotchDim;
            double tblen = aPart.TabLength;
            double tabht = aPart.TabHeight;
            double gset = aPart.GussetLength;

            ULINE limline = aPart.LimitLn;
            double shelflength = bLeftSide ? aPart.LeftShelfLength : aPart.RightShelfLength;
       
            colDXFVectors verts = new colDXFVectors();
            double cY = aPart.Z;

            string lt = string.Empty;
            string lt2 = string.Empty;
            double bxLen = bLeftSide ? aPart.LeftBoxLength / 2 : aPart.RightBoxLength / 2;

            double f1 = bLeftSide ? -1 : 1;
            if (aPart.Side == uppSides.Bottom) f1 *= -1;
            dxeLine l1 = null;
            dxeLine l2 = null;
            List<double> xVals = new List<double>();
            double y1 = tabht;
            double y3 = -(ht - tabht);
            double y2 = y3 + ntch;
            dxfVector gpt = null;

            mdDowncomerBox dcBox = aPart.DowncomerBox;
            if (dcBox == null)
            {
                var dc = aPart.GetMDDowncomer( aIndex: aPart.DowncomerIndex );
                dcBox = dc.Boxes[ aPart.BoxIndex - 1 ];
            }

            double cX = bLeftSide ? -limline.sp.WithRespectToPlane(dcBox.Plane).Y : limline.ep.WithRespectToPlane(dcBox.Plane).Y;
            double distanceFromBoxEdge;
            uopLine boxLine = dcBox.BoxLine(bLeftSide);
            UVECTOR limitLinePointOnBox = bLeftSide ? limline.sp : limline.ep;
            if (aPart.Side == uppSides.Top)
            {
                distanceFromBoxEdge = boxLine.MaxY - limitLinePointOnBox.Y;
            }
            else
            {
                distanceFromBoxEdge = limitLinePointOnBox.Y - boxLine.MinY;
            }

            xVals.Add(0);
            xVals.Add(thk * f1);
            xVals.Add((distanceFromBoxEdge) * f1);
            xVals.Add((distanceFromBoxEdge + 0.25) * f1);
            xVals.Add((distanceFromBoxEdge + 0.25 + tblen) * f1);
            if (gset > 0) xVals.Add((distanceFromBoxEdge + 0.25 + tblen + gset) * f1);


            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);

            if (gset > 0)
            {
                l1 = aPln.CreateLine(xVals[3], y2, xVals[3], 0);
                l2 = aPln.CreateLine(xVals[5], 0, xVals[5] + 1000, 0);
                l2.RotateAbout(aPln.ZAxis(10, l2.StartPt), 45 * f1);
                gpt = l1.IntersectPoint(l2, true, true);
                l2.EndPt = gpt.Clone();

                gpt = gpt.WithRespectToPlane(aPln);
                if (gpt.Y < y2)
                {
                    gpt.Y = y2;
                    l2.EndPt = aPln.Vector(xVals[3], y2);
                }

                if (gpt.Y == y2)
                {
                    gpt = null;
                }
            }


            if (bVisiblePartOnly)
            {
                lt = dxfLinetypes.Invisible;
                lt2 = lt;
            }
            if (bShowObscured) lt2 = dxfLinetypes.Hidden;


            verts.Add(aPln, xVals[0], y1, aLineType: lt);
            verts.Add(aPln, xVals[0], y3, aLineType: lt);
            verts.Add(aPln, xVals[1], y3, aLineType: lt, aTag: "NOTCH_BOT");
            verts.Add(aPln, xVals[1], y2, aLineType: lt, aTag: "NOTCH_TOP");
            verts.Add(aPln, xVals[2], y2);
            verts.Add(aPln, xVals[3], y2, aTag: "TIP");
            if (gpt != null)
            {
                verts.Add(aPln, xVals[3], gpt.Y, aTag: "GUSSET");
            }
            if (shelflength / 2 > bxLen + 0.25)
            {
                verts.Add(aPln, xVals[3], -1, aLineType: lt, aTag: "SA_BOT");
            }

            verts.Add(aPln, xVals[3], 0, aLineType: lt2, aTag: "TAB_CORNER");
            verts.Add(aPln, xVals[4], 0, aLineType: lt2, aTag: "TAB_BOT");
            verts.Add(aPln, xVals[4], 1 + aPart.DeckThickness, aLineType: lt2);
            verts.Add(aPln, xVals[4], y1, aTag: "TAB_TOP");
            verts.Add(aPln, xVals[2], y1, aLineType: lt);
            verts.Add(aPln, xVals[1], y1, aLineType: lt);

            _rVal.LayerName = aLayerName;
            _rVal.Plane = aPln;
            _rVal.InsertionPt = aPln.Origin;
            _rVal.Vertices = verts;
            if (!bVisiblePartOnly)
            {
                _rVal.Closed = true;
                _rVal.AdditionalSegments.Add(aPln.CreateLine(xVals[1], y1, xVals[1], y2, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
            }
            else
            {
                _rVal.Closed = false;
            }
            if (gset > 0)
            {
                dxePolygon aPg = new dxePolygon() { Closed = false, LayerName = aLayerName, Tag = "GUSSET" };

                l1 = verts.LineSegment("TAB_CORNER");
                aPg.Vertices.Add(l1.EndPt);
                aPg.Vertices.Add(aPln, xVals[ 5 ], 0, aTag: "GUSSET TIP");
                if (bLeftSide && bVisiblePartOnly)
                {
                    aPg.Vertices.LastVector().Linetype = dxfLinetypes.Hidden;
                }

                l1.Project(aPln.YDirection, -1);
                aPg.Vertices.Add(l1.IntersectPoint(l2));

                aPg.Vertices.Add(l2.EndPt, bAddClone: true);


                _rVal.AdditionalSegments.Append(aPg.SubEntities());
                if (bLeftSide)
                {
                    dxfVector v1 = gpt == null ? _rVal.Vertex("TIP") : _rVal.Vertex("GUSSET");

                    v1.Linetype = dxfLinetypes.Invisible;
                    for (int j = verts.IndexOf(v1) + 1; j <= verts.Count; j++)
                    {
                        gpt = _rVal.Vertex(j);
                        if (gpt.Tag != "TAB_BOT")
                            gpt.Linetype = dxfLinetypes.Invisible;
                        else
                            break;
                    }

                }


            }

            if (aPart.BoltOn && !bVisiblePartOnly && !bSuppressHoles)
            {
                uopHoleArray aHls = bLeftSide ? aPart.GenHoles("LEFT") : aPart.GenHoles("RIGHT") ;
                List<uopHole> bHls;
                dxfVector v1;
                dxfPlane bPln = aPart.Plane;
                for (int i = 1; i <= aHls.Count; i++)
                {
                    bHls = aHls.Item(i).ToList;
                    foreach (uopHole hole in bHls)
                    {
                        v1 = hole.CenterDXF;
                        v1 = v1.WithRespectToPlane(bPln);
                        _rVal.AdditionalSegments.AddPlanarArc(aPln, hole.Radius, v1.Y * f1, v1.Z);
                    }
                }

            }

            return _rVal;
        }

        /// <summary>
        /// Returns the polygon for the profile view of the beam
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="aCenter"></param>
        /// <param name="aLayerName"></param>
        /// <param name="suppressRotation"></param>
        /// <param name="aCenterlineScalefactor"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static dxePolygon Beam_View_Profile(mdBeam beam, iVector aCenter = null, string aLayerName = "GEOMETRY", bool suppressRotation = true, double aScaleFactor = 1, double aCenterlineScalefactor = 1.05)
        {
            dxePolygon profilePolygon = new dxePolygon() { LayerName = aLayerName };


            if (beam == null) return profilePolygon;

            dxfPlane bPln = xPlane(beam.X, beam.Y, aCenter, suppressRotation ? 0 : beam.Rotation);

            try
            {
                double wd = beam.Width;
                double ht = beam.Height;
                double flngThk = beam.FlangeThickness;
                double webThk = beam.WebThickness;
                double x = 0;
                double y = 0;
                double opening = beam.WebOpeningCount > 0 ? beam.WebOpeningSize : 0;
                //double halfBeamHeight = ht / 2;
                //double halfBeamWidth = wd / 2;
                //double halfFlangeThickness = flngThk / 2;
                //double halfWebThinkness = webThk / 2;
                //double halfWebOpeningSize = opening / 2;


                colDXFVectors verts = new colDXFVectors(bPln.Vector(-wd / 2, ht / 2, aTag: "TOP", aFlag: "LEFT"));
                verts.AddRelative(wd, 0, aPlane: bPln, aTag: "TOP", aFlag: "RIGHT");
                verts.AddRelative(0, -flngThk, aPlane: bPln, aTag: "TOP FLANGE", aFlag: "LEFT OUTSIDE");
                verts.AddRelative(-(wd / 2 - webThk / 2), 0, aPlane: bPln, aTag: "TOP FLANGE", aFlag: "LEFT INSIDE");
                verts.AddRelative(0, -(ht - 2 * flngThk), aPlane: bPln, aTag: "BOTTOM FLANGE", aFlag: "LEFT INSIDE");
                verts.AddRelative((wd / 2 - webThk / 2), 0, aPlane: bPln, aTag: "BOTTOM FLANGE", aFlag: "LEFT OUTSIDE");
                verts.AddRelative(0, -flngThk, aPlane: bPln, aTag: "BOTTOM", aFlag: "RIGHT");
                verts.AddRelative(-wd, 0, aPlane: bPln, aTag: "BOTTOM", aFlag: "LEFT");
                verts.AddRelative(0, flngThk, aPlane: bPln, aTag: "BOTTOM FLANGE", aFlag: "RIGHT OUTSIDE");
                verts.AddRelative((wd / 2 - webThk / 2), 0, aPlane: bPln, aTag: "BOTTOM FLANGE", aFlag: "RIGHT INSIDE");
                verts.AddRelative(0, (ht - 2 * flngThk), aPlane: bPln, aTag: "TOP FLANGE", aFlag: "RIGHT INSIDE");
                verts.AddRelative(-(wd / 2 - webThk / 2), 0, aPlane: bPln, aTag: "TOP FLANGE", aFlag: "RIGHT OUTSIDE");

                // ****************************** Draw beam box ***************************************
                //double beamTopYIn = halfBeamHeight - beam.FlangeThickness;
                //double beamBottomYIn = -halfBeamHeight + beam.FlangeThickness;
                //var boxDisplaySetting = dxfDisplaySettings.Null(aLayer: aLayerName, aLinetype: dxfLinetypes.Continuous);

                //colDXFVectors topFlangeThicknessRectangle = new colDXFVectors();
                //topFlangeThicknessRectangle.Add(bPln.Vector(-halfBeamWidth, halfBeamHeight)); // Top left corner
                //topFlangeThicknessRectangle.Add(bPln.Vector(halfBeamWidth, halfBeamHeight)); // Top right corner
                //topFlangeThicknessRectangle.Add(bPln.Vector(halfBeamWidth, beamTopYIn)); // Bottom right corner
                //topFlangeThicknessRectangle.Add(bPln.Vector(-halfBeamWidth, beamTopYIn)); // Bottom left corner

                profilePolygon.Vertices = verts; // Top flange thickness rectangle
                profilePolygon.Closed = true;

                profilePolygon.AdditionalSegments.Add(new dxeLine(verts.Item(11), verts.Item(4))); // top flange connector line
                profilePolygon.AdditionalSegments.Add(new dxeLine(verts.Item(10), verts.Item(5))); // top flange connector line


                //profilePolygon.AdditionalSegments.AddLine(-halfWebThinkness, beamTopYIn, -halfWebThinkness, beamBottomYIn, boxDisplaySetting); // Left vertical web inset line
                //profilePolygon.AdditionalSegments.AddLine(halfWebThinkness, beamTopYIn, halfWebThinkness, beamBottomYIn, boxDisplaySetting); // Right vertical web inset line

                //// Bottom flange thickness rectangle
                //dxfVector centerOfBottomFlangeThickness = new dxfVector(0, -halfBeamHeight + halfFlangeThickness);
                //profilePolygon.AdditionalSegments.AddRectangle(centerOfBottomFlangeThickness, beam.Width, beam.FlangeThickness, aDisplaySettings: boxDisplaySetting);

                // ****************************** Draw beam centerline ********************************
                if (aCenterlineScalefactor > 0)
                {
                    profilePolygon.AdditionalSegments.Add(bPln.VerticalLine(0,  ht + (aCenterlineScalefactor * flngThk), bCenterOnOrigin: true, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: "Center"))).Tag = "CENTERLINE";
                }
                //double extrusion = beam.FlangeThickness / 4; // Inches
                //var centerLineDisplaySetting = dxfDisplaySettings.Null(aLayer: aLayerName, aColor: dxxColors.Red, aLinetype: "Center");

                //profilePolygon.AdditionalSegments.AddLine(0, halfBeamHeight + extrusion, 0, -halfBeamHeight - extrusion, aDisplaySettings: centerLineDisplaySetting);

                // ****************************** Draw web opening lines ******************************
                //if (beam.WebOpeningSize > 0 && beam.WebOpeningCount > 0)
                if (opening > 0)
                {
                    
                    profilePolygon.AdditionalSegments.Add(bPln.HorizontalLine(opening / 2, webThk, bCenterOnOrigin: true, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden))).Tag = "WEB OPENING";
                    profilePolygon.AdditionalSegments.Add(bPln.HorizontalLine(-opening / 2, webThk, bCenterOnOrigin: true, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden))).Tag = "WEB OPENING";
                    profilePolygon.AdditionalSegments.Add(new dxePoint(bPln.Vector(0, 0)) { LayerName = "DefPoints", Tag = "WEB OPENING" });
                    if (aCenterlineScalefactor > 0)
                    {
                        profilePolygon.AdditionalSegments.Add(bPln.HorizontalLine(0, 1.5 * webThk * aCenterlineScalefactor, bCenterOnOrigin: true, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: "Center"))).Tag = "WEB OPENING CENTERLINE";
                    }
                    //var webOpeningLineDisplaySetting = dxfDisplaySettings.Null(aLayer: aLayerName, aColor: dxxColors.Green, aLinetype: dxfLinetypes.Hidden);

                    //profilePolygon.AdditionalSegments.AddLine(-halfWebThinkness, halfWebOpeningSize, halfWebThinkness, halfWebOpeningSize, aDisplaySettings: webOpeningLineDisplaySetting); // Top of the web opening

                    //profilePolygon.AdditionalSegments.AddLine(-halfWebThinkness, -halfWebOpeningSize, halfWebThinkness, -halfWebOpeningSize, aDisplaySettings: webOpeningLineDisplaySetting); // Bottom of the web opening
                }

                // ****************************** Draw holes ******************************************
                uopHole mount = beam.MountSlot;

                //var beamHoles = beam.GenHoles("BEAM").Item(1);
                x = (wd / 2) - mount.DownSet;
                y = -ht / 2 + flngThk / 2;
                profilePolygon.AdditionalSegments.Add(new dxePoint(bPln.Vector(0, 0)) { LayerName = "DefPoints", Tag = "MOUNT SLOT", Flag = "RIGHT" });

                profilePolygon.AdditionalSegments.AddLine(x + mount.Radius, y + flngThk / 2, x + mount.Radius, y - flngThk / 2, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden), aPlane: bPln);
                profilePolygon.AdditionalSegments.AddLine(x - mount.Radius, y + flngThk / 2, x - mount.Radius, y - flngThk / 2, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden), aPlane: bPln);
                if (aCenterlineScalefactor > 0)
                {
                    profilePolygon.AdditionalSegments.AddLine(x, y + (1.25 * flngThk * aCenterlineScalefactor) / 2, x, y - (1.25 * flngThk * aCenterlineScalefactor) / 2, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: "Center"), aPlane: bPln).Tag = "MOUNT SLOT CENTERLINE";
                }
                x = -(wd / 2) + mount.DownSet;
                profilePolygon.AdditionalSegments.Add(new dxePoint(bPln.Vector(0, 0)) { LayerName = "DefPoints", Tag = "MOUNT SLOT", Flag = "LEFT" });

                profilePolygon.AdditionalSegments.AddLine(x + mount.Radius, y + flngThk / 2, x + mount.Radius, y - flngThk / 2, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden), aPlane: bPln);
                profilePolygon.AdditionalSegments.AddLine(x - mount.Radius, y + flngThk / 2, x - mount.Radius, y - flngThk / 2, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden), aPlane: bPln);
                if (aCenterlineScalefactor > 0)
                {
                    profilePolygon.AdditionalSegments.AddLine(x, y + (1.25 * flngThk * aCenterlineScalefactor) / 2, x, y - (1.25 * flngThk * aCenterlineScalefactor) / 2, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: "Center"), aPlane: bPln).Tag = "MOUNT SLOT CENTERLINE";
                }


                //double holeLength = beamHoles.Member.Length; // This is the whole length of the hole
                //double halfHoleLength = holeLength / 2;

                //dxfVector topLeftHoleCenterInWorld = beamHoles.Centers.First(c => c.Tag=="TOP" && c.Flag == "LEFT").ToDXFVector();
                //double holeOffsetFromCenterOfWeb = topLeftHoleCenterInWorld.WithRespectToPlane(beam.Plane).Y;
                //double leftHoleCenterX = -holeOffsetFromCenterOfWeb;
                //double rightHoleCenterX = holeOffsetFromCenterOfWeb;

                //// Draw holes side lines
                //dxfDisplaySettings holeVerticalSideLineDisplaySetting = dxfDisplaySettings.Null(aLayer: aLayerName, aColor: dxxColors.Green, aLinetype: "HIDDEN2");

                //profilePolygon.AdditionalSegments.AddLine(leftHoleCenterX - halfHoleLength, beamBottomYIn, leftHoleCenterX - halfHoleLength, -halfBeamHeight, aDisplaySettings: holeVerticalSideLineDisplaySetting); // Left side line of the left hole

                //profilePolygon.AdditionalSegments.AddLine(leftHoleCenterX + halfHoleLength, beamBottomYIn, leftHoleCenterX + halfHoleLength, -halfBeamHeight, aDisplaySettings: holeVerticalSideLineDisplaySetting); // Right side line of the left hole

                //profilePolygon.AdditionalSegments.AddLine(rightHoleCenterX - halfHoleLength, beamBottomYIn, rightHoleCenterX - halfHoleLength, -halfBeamHeight, aDisplaySettings: holeVerticalSideLineDisplaySetting); // Left side line of the right hole

                //profilePolygon.AdditionalSegments.AddLine(rightHoleCenterX + halfHoleLength, beamBottomYIn, rightHoleCenterX + halfHoleLength, -halfBeamHeight, aDisplaySettings: holeVerticalSideLineDisplaySetting); // Right side line of the right hole

                //// Draw hole center lines
                //dxfDisplaySettings holeVerticalCenterLineDisplaySetting = dxfDisplaySettings.Null(aLayer: aLayerName, aColor: dxxColors.Red, aLinetype: "CENTER");
                //extrusion = beam.FlangeThickness / 4; // Inch

                //profilePolygon.AdditionalSegments.AddLine(leftHoleCenterX, beamBottomYIn + extrusion, leftHoleCenterX, -halfBeamHeight - extrusion, aDisplaySettings: holeVerticalCenterLineDisplaySetting); // Left hole center line

                //profilePolygon.AdditionalSegments.AddLine(rightHoleCenterX, beamBottomYIn + extrusion, rightHoleCenterX, -halfBeamHeight - extrusion, aDisplaySettings: holeVerticalCenterLineDisplaySetting); // Right hole center line

                return profilePolygon;
            }
            catch
            {
                return profilePolygon;
            }
            finally
            {
                profilePolygon.Plane = bPln;
                profilePolygon.InsertionPt = bPln.Origin;
                if (aScaleFactor != 1 && aScaleFactor != 0) profilePolygon.Rescale(aScaleFactor);
            }
        }

        /// <summary>
        ///returns a dxePolygon that is used to draw the plan view of the part
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bSuppressHoles" flag to suppress the holes></param>
        /// <param name="aCenterLineLength"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aScale"></param>
        /// <returns></returns>
        public static dxePolygon EndSupport_View_Plan(mdEndSupport aPart, bool bSuppressHoles = false, double aCenterLineLength = 0, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", double aScale = 1, bool bIncludeFilletFoints = true, bool bSuppressChamfers = false)
        {
            if (aPart == null) return null;

            dxfPlane bPln = aPart.Plane;
            double thk = aPart.Thickness;
            double wd = aPart.Width;

            ULINE leftW = aPart.WeirLn(bLeft: true);
            ULINE rightW = aPart.WeirLn(bLeft: false);
            double cX = aPart.X;
            double cY = aPart.Y;
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation, aScale, bPln);
            ULINE limline = new ULINE(aPart.LimitLn);
            if (limline.sp.X > limline.ep.X) limline.Invert();
            double rad = aPart.DeckRadius;
            bool top = cY > aPart.BoxY;

            double fy = aPart.Side == uppSides.Bottom ? -1 : 1;
            double fx = 1;
            double gp = mdEndSupport.WeldGap;
            bool leftside = aPart.X < 0;
        
            //if (leftside)
            //    bPln.RotateAboutLine(bPln.YAxis(), 180);
            uopVectors eps = new uopVectors(limline.EndPoints);
            dxfVector v1 = new dxfVector(eps.GetVector(dxxPointFilters.AtMinX));
            dxfVector v2 = new dxfVector(eps.GetVector(dxxPointFilters.AtMaxX));


            double x1 = -wd / 2 - thk;
            double x2 = wd / 2 + thk;

            double y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(Math.Abs(cX) + (x1 + thk), 2)) - Math.Abs(cY);
            double y2 = (!aPart.IsSquare) ? Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(Math.Abs(cX) + x2, 2)) - Math.Abs(cY) : y1;
            y1 = leftW.MaxY - Math.Abs(cY);
            y2 = (!aPart.IsSquare) ? rightW.MaxY - Math.Abs(cY) - Math.Abs(cY) : y1;

            double y3 = (aPart.BoxLength(bLongSide: false) / 2) + gp - Math.Abs(cY);
            double y4 = (aPart.BoxLength(bLongSide: true) / 2) + gp - Math.Abs(cY);

            colDXFVectors verts = new colDXFVectors();

            //comput the chamfer lengths

            double cl_Left = aPart.LeftChamfer;
            dxfVector v3 = v1.Moved(aYChange: 100 * fy);
            dxeArc fArc = dxfPrimatives.CreateFilletArc(thk, v2, v1, v3);
            double d1 = v1.DistanceTo(fArc?.EndPt);
            d1 = uopUtils.RoundTo(d1, dxxRoundToLimits.Millimeter, true);
            if (d1 > cl_Left) cl_Left = d1;

            double cl_Right = cl_Left;
            if (!aPart.IsSquare && aPart.HasTriangularEndPlate)
            {
                v3 = v2.Moved(aYChange: 100 * fy);
                fArc = dxfPrimatives.CreateFilletArc(thk, v1, v2, v3);
                d1 = v2.DistanceTo(fArc?.StartPt);
                cl_Right = uopUtils.RoundTo(d1, dxxRoundToLimits.Millimeter, true);
            }


            aPart.LeftChamfer = cl_Left;
            aPart.RightChamfer = cl_Right;



            //v1 = v1.WithRespectToPlane(bPln, aPln, aXScale: fx, aYScale: fy);
            //v2 = v2.WithRespectToPlane(bPln, aPln, aXScale: fx, aYScale: fy);
            //dxfDirection lDir = v1.DirectionTo(v2);

            //verts.Add(aPln, x1 * fx, y1 * fy, aTag: "TOP_LEFT");
            //verts.Add(aPln, (x1 + thk) * fx, y1 * fy, aTag: "LAP_LINE");
            //verts.Add(aPln, (x2 - thk) * fx, y2 * fy);
            //verts.Add(aPln, x2 * fx, y2 * fy, aTag: "TOP_RIGHT");
            //verts.Add(aPln, x2 * fx, y3 * fy, aTag: "TAB1");
            //verts.Add(aPln, (x2 - thk) * fx, y3 * fy);
            //verts.Add(aPln, (x2 - thk) * fx, y2 * fy);
            //verts.Add(aPln, (x2 - thk) * fx, y3 * fy);
            //verts.Add(aPln, (x2 - thk) * fx, (y3 + mdEndSupport.NotchDepth) * fy, aTag: "NOTCH1"); //, , , , dxfLinetypes.Hidden
            //verts.Add(aPln, (x2 - (2 * thk)) * fx, (y3 + mdEndSupport.NotchDepth) * fy, aTag: "SHORT");
            //v3 = v2.WithRespectToPlane(aPln);
            //dxfVector champ1 = v3.Clone();
            //verts.Add(aPln, v3.X * fx, (v3.Y + cl_Right) * fy, aTag: "INSIDE_RIGHT");
            //v3 = v2.Projected(lDir, -cl_Right).WithRespectToPlane(aPln);
            //verts.Add(aPln, v3.X * fx, v3.Y * fy, aTag: "LIM_LINE");
            //v3 = v1.Clone();

            //v3 = v3.Projected(lDir, cl_Left).WithRespectToPlane(aPln);
            //verts.Add(aPln, v3.X * fx, v3.Y * fy);
            //v3 = v1.WithRespectToPlane(aPln);
            //verts.Add(aPln, v3.X * fx, (v3.Y + cl_Left) * fy, aTag: "INSIDE_LEFT");

            //verts.Add(aPln, v3.X * fx, (y4 + mdEndSupport.NotchDepth) * fy, aTag: "NOTCH2");
            //verts.Add(aPln, (v3.X - thk) * fx, (y4 + mdEndSupport.NotchDepth) * fy );

            //verts.Add(aPln, (x1 + thk) * fx, y1 * fy);

            //verts.Add(aPln, (v3.X - thk) * fx, y4 * fy, aTag: "TAB2");
            //verts.Add(aPln, (v3.X - 2 * thk) * fx, y4 * fy);

            verts.Clear();


            UVECTOR u1 = leftW.EndPoints.GetVector(top ? dxxPointFilters.AtMaxY : dxxPointFilters.AtMinY);
            UVECTOR u2 = u1.Moved(thk);
            UVECTOR u3 = rightW.EndPoints.GetVector(top ? dxxPointFilters.AtMaxY : dxxPointFilters.AtMinY).Moved(-thk);
            UVECTOR u4 = u3.Moved(thk);

            verts.Add(aPln, u1.X - cX, u1.Y - cY, aTag: "TOP_LEFT");
            verts.Add(aPln, u2.X - cX, u2.Y - cY, aTag: "LAP_LINE");
            verts.Add(aPln, u3.X - cX, u3.Y - cY, aTag: "");
            verts.Add(aPln, u4.X - cX, u4.Y - cY, aTag: "TOP_RIGHT");
            verts.AddRelative(aX: 0, aY: -rightW.Length * fy, aPlane: aPln, aTag: "TAB1");
            verts.AddRelative(aX: -thk, aY: 0, aPlane: aPln, aTag: "");
            verts.AddRelative(aX: 0, aY: mdEndSupport.NotchDepth * fy, aPlane: aPln, aTag: "");
            verts.Add(aPln, u3.X - cX, u3.Y - cY, aTag: "");
            verts.AddRelative(aX: 0, aY: (rightW.Length - mdEndSupport.NotchDepth) * -fy, aPlane: aPln, aTag: "NOTCH1");
            verts.AddRelative(aX: -thk, aY: 0, aPlane: aPln, aTag: "SHORT");

            u3 = new UVECTOR(eps.GetVector(dxxPointFilters.AtMaxX));

            UVECTOR u5 = u3.Moved(aYAdder: cl_Right * fy);
            verts.Add(aPln, u5.X - cX, u5.Y - cY, aTag: "INSIDE_RIGHT");
            u5 = u3 + limline.Direction() * -cl_Right;
            verts.Add(aPln, u5.X - cX, u5.Y - cY, aTag: "LIM_LINE");

            u3 = new UVECTOR(eps.GetVector(dxxPointFilters.AtMinX));

            u5 = u3 + limline.Direction() * cl_Left;
            verts.Add(aPln, u5.X - cX, u5.Y - cY, aTag: "");
            u5 = u3.Moved(aYAdder: cl_Left * fy);
            verts.Add(aPln, u5.X - cX, u5.Y - cY, aTag: "INSIDE_LEFT");

            u3 = new UVECTOR(u5.X, top ? leftW.MinY + mdEndSupport.NotchDepth : leftW.MaxY - mdEndSupport.NotchDepth);
            verts.Add(aPln, u3.X - cX, u3.Y - cY, aTag: "NOTCH2");
            verts.AddRelative(aX: -thk, aY: 0, aPlane: aPln, aTag: "");
            verts.Add(aPln, u2.X - cX, u2.Y - cY, aTag: "");
            verts.AddRelative(aX: 0, aY: -leftW.Length * fy, aPlane: aPln, aTag: "TAB2");
            verts.AddRelative(aX: -thk, aY: 0, aPlane: aPln, aTag: "");

            colDXFEntities addsegs = new colDXFEntities();
            if (!bSuppressHoles)
            {
                uopHoleArray aHls = aPart.GenHoles();

                //if (cX < 0) fx = -1;
                bPln.SetCoordinates(cX, cY);
                uopHoles Hls = aHls.Item("RING CLIP");
                uopHole aH;
                if (Hls != null)
                {
                    for (int i = 1; i <= Hls.Count; i++)
                    {
                        aH = Hls.Item(i);
                        v1 = new dxfVector(aH.Center).WithRespectToPlane(bPln, aPln, aXScale: fx);
                        addsegs.Add(new dxeArc(v1, aH.Radius, aPlane: aPln) { LayerName = aLayerName, Tag = aH.Tag, Flag = aH.Flag });
                    }

                }


                double t = thk / 2;
                double c = aCenterLineLength / 2;
                Hls = aHls.Item("END ANGLE");
                if (Hls != null)
                {
                    gp = 0.5 * Hls.Diameter;
                    for (int i = 1; i <= Hls.Count; i++)
                    {
                        aH = Hls.Item(i);

                        v1 = aH.CenterDXF.WithRespectToPlane(bPln);
                        addsegs.AddLine(aPln.Vector(v1.X - t, v1.Y + gp), aPln.Vector(v1.X + t, v1.Y + gp), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));
                        addsegs.AddLine(aPln.Vector(v1.X - t, v1.Y - gp), aPln.Vector(v1.X + t, v1.Y - gp), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));

                        if (aCenterLineLength > 0)
                        {
                            addsegs.AddLine(aPln.Vector(v1.X - c, v1.Y), aPln.Vector(v1.X + c, v1.Y), new dxfDisplaySettings(aLayerName, aLinetype: "Center"), aTag: aH.Tag, aFlag: aH.Flag);
                        }

                        v1 = aPln.Vector(v1.X, v1.Y);
                        addsegs.AddPoint(v1.X, v1.Y, aLayerName, aTag: aH.Tag, aFlag: aH.Flag, aValue: aH.Radius).Factor = aH.DownSet;
                        //addsegs.LastMember().TFVSet(aH.Tag, aH.Flag, aH.Radius);
                        //addsegs.AddPoint(v1.X, v1.Y, aLayerName).Factor = (aH.DownSet);

                    }
                }

            }




            dxePolygon _rVal = new dxePolygon(verts, aPln.Origin, bClosed: true, aPlane: aPln)
            {
                LayerName = aLayerName,
                AdditionalSegments = addsegs,
                BlockName = $"BOX_DC _{aPart.DowncomerIndex}_BOX_{aPart.BoxIndex}_{aPart.PartName.ToUpper()}_{aPart.Side.ToString().ToUpper()}_PLAN"
            };



            if (aScale != 1)
            {
                _rVal.Rescale(aScale, aPln.Origin);
            }
            if (bIncludeFilletFoints)
            {

                v1 = new dxfVector(eps.GetVector(dxxPointFilters.AtMinX)).WithRespectToPlane(bPln, aPln);
                _rVal.AdditionalSegments.AddPoint(v1.X, v1.Y, aLayerName, aTag: "CHAMFER PT", aFlag: "LEFT");
                v1 = new dxfVector(eps.GetVector(dxxPointFilters.AtMaxX)).WithRespectToPlane(bPln, aPln);
                _rVal.AdditionalSegments.AddPoint(v1.X, v1.Y, aLayerName, aTag: "CHAMFER PT", aFlag: "RIGHT");

            }

            return _rVal;
        }

        /// <summary>
        /// ^returns a dxePolygon that is used to draw the layout view of the end plate
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aDC"></param>
        /// <param name="bApplyFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aCenterLineLength"></param>
        /// <param name="aScale"></param>
        /// <returns></returns>
        public static dxePolygon EndPlate_View_Plan(mdEndPlate aPart, bool bApplyFillets, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", double aCenterLineLength = 0, double aScale = 1, bool bIncludeBendPoints = false, bool bSuppressHoles = false)
        {

            if (aPart == null) return new dxePolygon();

            ULINE lim = aPart.LimitLn;
            dxeLine limline = lim.ToDXFLine();
            double thk = aPart.Thickness;
            double t = thk / 2;
            double leftleg = aPart.LeftLegLength;
            double rightleg = aPart.RightLegLength;
            bool bTriang = aPart.IsTriangular;
            double tleng = aPart.TabLength;
            double cx = aPart.X;
            double cy = aPart.Y;
            dxeArc arc1 = null;
            dxeArc arc2 = null;
            dxeArc arc3 = null;
            dxeArc arc4 = null;

            if (aPart.BottomSide) aRotation += 180;
            colDXFVectors verts = new colDXFVectors();
            dxfPlane aPln = xPlane(cx, cy, aCenter, aRotation, aScale);
            dxfPlane bPln = aPart.Plane;

            dxfDirection ydir = aPln.YDirection;
            dxfDirection xdir = aPln.XDirection;
            limline.StartPt = aPln.Vector(limline.StartPt.X - cx, limline.StartPt.Y - cy);
            limline.EndPt = aPln.Vector(limline.EndPt.X - cx, limline.EndPt.Y - cy);

            //if (aPart.BottomSide) limline.RotateAbout(aPln.ZAxis(),180);
            dxfVector ip1 = limline.StartPt.Clone();
            dxfVector ip2 = limline.EndPt.Clone();

            dxeLine leg2_outside = new dxeLine(ip1 + ydir * (leftleg + tleng), ip1);
            dxeLine leg1_outside = new dxeLine(ip2, ip2 + ydir * (rightleg + tleng));

            dxeLine limline_inside = limline.Clone();
            dxeLine leg2_inside = leg2_outside.Clone();
            leg2_inside.Translate(xdir * thk);
            dxeLine leg1_inside = leg1_outside.Clone();
            leg1_inside.Translate(xdir * -thk);
            limline_inside.MoveOrthogonal(thk);

            limline_inside.Invert();
            leg2_inside.Invert();
            leg1_inside.Invert();


            leg1_inside.EndPt = limline_inside.IntersectPoint(leg1_inside);
            leg2_inside.StartPt = limline_inside.IntersectPoint(leg2_inside);

            if (!bApplyFillets)
            {
                verts.Add(ip1, aTag: "WEIR_PT", aFlag: "LONG");
                verts.Add(ip2, aTag: "WEIR_PT", aFlag: "SHORT");
                verts.Add(leg1_outside.EndPt, aTag: "TAB_PT", aFlag: "SHORT");
                verts.Add(leg1_inside.StartPt, aTag: "INSIDETAB_PT", aFlag: "SHORT");
                verts.Add(leg1_inside.EndPt, aTag: "INSIDEWEIR_PT", aFlag: "SHORT");
                verts.Add(leg2_inside.StartPt, aTag: "INSIDEWEIR_PT", aFlag: "LONG");
                verts.Add(leg2_inside.EndPt, aTag: "INSIDETAB_PT", aFlag: "LONG");
                verts.Add(leg2_outside.StartPt, aTag: "TAB_PT", aFlag: "LONG");
                bIncludeBendPoints = false;
            }
            else
            {
                string err = string.Empty;
                arc1 = dxfPrimatives.FilletArc(leg2_outside, limline, 2 * thk, ref err);
                arc2 = dxfPrimatives.FilletArc(limline, leg1_outside, 2 * thk, ref err);
                arc3 = dxfPrimatives.FilletArc(leg1_inside, limline_inside, thk, ref err);
                if (err !=  string.Empty)
                    Console.WriteLine(err);
                arc4 = dxfPrimatives.FilletArc(limline_inside, leg2_inside, thk, ref err);
                verts.Add(new dxfVector(arc1.StartPt) { VertexRadius = arc1.Radius }, aTag: "ARC_PT1", aFlag: "LONG");
                verts.Add(new dxfVector(arc1.EndPt) { VertexRadius = 0 }, aTag: "ARC_PT1", aFlag: "LONG");
                verts.Add(new dxfVector(arc2.StartPt) { VertexRadius = arc2.Radius }, aTag: "ARC_PT2", aFlag: "SHORT");
                verts.Add(new dxfVector(arc2.EndPt) { VertexRadius = 0 }, aTag: "ARC_PT2", aFlag: "SHORT");
                verts.Add(leg1_outside.EndPt, aTag: "TAB_PT", aFlag: "SHORT");
                verts.Add(leg1_inside.StartPt, aTag: "INSIDETAB_PT", aFlag: "SHORT");
                verts.Add(new dxfVector(arc3.StartPt) { VertexRadius = -arc3.Radius }, aTag: "WEIR_PT", aFlag: "LONG");
                verts.Add(new dxfVector(arc3.EndPt) { VertexRadius = 0 }, aTag: "WEIR_PT", aFlag: "LONG");
                verts.Add(new dxfVector(arc4.StartPt) { VertexRadius = -arc4.Radius }, aTag: "WEIR_PT", aFlag: "LONG");
                verts.Add(new dxfVector(arc4.EndPt) { VertexRadius = 0 }, aTag: "WEIR_PT", aFlag: "LONG");
                verts.Add(leg2_inside.EndPt, aTag: "INSIDETAB_PT", aFlag: "LONG");
                verts.Add(leg2_outside.StartPt, aTag: "TAB_PT", aFlag: "LONG");

            }



            dxePolygon _rVal = new dxePolygon(verts, aPln.Origin, bClosed: true, aName: $"{aPart.PartNumber}_PLAN_VIEW", aDisplaySettings: new dxfDisplaySettings(aLayer: aLayerName), aPlane: aPln);

            if (aPart.GussetLength > 0)
            {
                verts = new colDXFVectors()
                {
                    _rVal.Vertex(aPart.BottomSide ? 7 : 1, true)
                };
                verts.AddRelative(0, aPart.GussetLength * 1, aPlane: aPln);
                verts.AddRelative(thk, aPlane: aPln);
                verts.AddRelative(0, -aPart.GussetLength * 1, aPlane: aPln);

                _rVal.AdditionalSegments.Add(new dxePolyline(verts, bClosed: false, aDisplaySettings: new dxfDisplaySettings(aLayer: aLayerName, aLinetype: dxfLinetypes.Hidden), aPlane: aPln) { Tag = "GUSSET" });

            }


            _rVal.AdditionalSegments.AddLine(leg1_outside.EndPt + ydir * -tleng, leg1_inside.StartPt + ydir * -tleng, new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));


            dxfVector v1;

            if (aPart.GussetLength <= 0)
            {

                _rVal.AdditionalSegments.AddLine(leg2_outside.StartPt + ydir * -tleng, leg2_inside.EndPt + ydir * -tleng, new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));

            }

            if (bIncludeBendPoints)
            {
                _rVal.AdditionalSegments.Add(new dxePoint(ip1) { Tag = "BEND_POINT_1", LayerName = "DefPoints" });
                _rVal.AdditionalSegments.Add(new dxePoint(ip2) { Tag = "BEND_POINT_2", LayerName = "DefPoints" });
                v1 = !aPart.BottomSide ? arc2.Center + ydir * -2 * thk : arc1.Center + ydir * -2 * thk;
                _rVal.AdditionalSegments.Add(new dxePoint(v1) { Tag = "BEND_POINT_3", LayerName = "DefPoints" });
                v1 = !aPart.BottomSide ? arc4.Center : arc3.Center;

                _rVal.AdditionalSegments.Add(new dxePoint(v1) { Tag = "BEND_POINT_4", LayerName = "DefPoints" });

            }

            if (aPart.BoltOn && !bSuppressHoles)
            {

                uopHoles bHls = new uopHoles(aPart.BoltOnHoles);
                if (bHls.Count > 0)
                {

                    bPln = aPart.Plane;
                    bPln.Z = 0;

                    colDXFVectors aPts = bHls.CentersDXF();
                    uopHole aHl = bHls.Item(1);
                    v1 = aPts.GetVector(dxxPointFilters.AtMaxX);

                    v1 = v1.WithRespectToPlane(bPln);

                    double r = aHl.Radius;
                    double c = aCenterLineLength / 2;
                    dxeLine l1 = _rVal.AdditionalSegments.AddPlanarLine(aPln, v1.X - t, v1.Y + r, v1.X + t, v1.Y + r, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: aHl.Tag, aFlag: aHl.Flag);
                    dxeLine l2 = _rVal.AdditionalSegments.AddPlanarLine(aPln, v1.X - t, v1.Y - r, v1.X + t, v1.Y - r, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: aHl.Tag, aFlag: aHl.Flag);
                    if (aCenterLineLength > 0)
                    {
                        _rVal.AdditionalSegments.AddPlanarLine(aPln, v1.X - c, v1.Y, v1.X + c, v1.Y, aLayerName, aLineType: "Center", aTag: aHl.Tag, aFlag: aHl.Flag);
                    }


                    v1 = aPts.GetVector(dxxPointFilters.AtMinX);

                    v1 = v1.WithRespectToPlane(bPln);
                    l1 = _rVal.AdditionalSegments.AddPlanarLine(aPln, v1.X - t, v1.Y + r, v1.X + t, v1.Y + r, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: aHl.Tag, aFlag: aHl.Flag);
                    l2 = _rVal.AdditionalSegments.AddPlanarLine(aPln, v1.X - t, v1.Y - r, v1.X + t, v1.Y - r, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: aHl.Tag, aFlag: aHl.Flag);
                    if (aCenterLineLength > 0)
                    {
                        _rVal.AdditionalSegments.AddPlanarLine(aPln, v1.X - c, v1.Y, v1.X + c, v1.Y, aLayerName, aLineType: "Center", aTag: aHl.Tag, aFlag: aHl.Flag);
                    }
                }
            }


            _rVal.BlockName = $"BOX_DC _{aPart.DowncomerIndex}_BOX_{aPart.BoxIndex}_{aPart.PartName.ToUpper()}_{aPart.Side.ToString().ToUpper()}_PLAN";

            if (aScale != 1)
            {
                _rVal.Rescale(aScale, aPln.Origin);
            }

            return _rVal;
        }

        public static dxePolygon Endplate_View_LayoutLeft(mdEndPlate aPart, out dxePolygon rPlanView, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bSuppressHoles = false)
        {
            dxePolygon _rVal = Endplate_View_LayoutRight(aPart, out rPlanView, aCenter, aRotation, aLayerName, bSuppressHoles);
            if (_rVal is null) return null;

            dxeLine axis = _rVal.Plane.YAxis();
            //_rVal.Mirror(axis);

            dxePolyline gusset = _rVal.AdditionalSegments.Polylines().Find(x => string.Compare(x.Tag, "GUSSET", true) == 0);
            if (gusset != null)
            {
                dxfVector v1 = _rVal.Vertex("TAB_CORNER");
                v1.MoveTo(gusset.Vertex(3));
                v1.Linetype = dxfLinetypes.Invisible;
            }

            return _rVal;


        }
        public static dxePolygon Endplate_View_LayoutRight(mdEndPlate aPart, out dxePolygon rPlanView, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bSuppressHoles = false)
        {
            rPlanView = null;
            dxePolygon _rVal = new dxePolygon();
            if (aPart == null) return null;

            rPlanView = EndPlate_View_Plan(aPart, true, aCenter, bIncludeBendPoints: true);

            double cx = aPart.Y;
            double cy = aPart.Z;
            double thk = aPart.Thickness;
            double ht = aPart.Height;
            double tleng = aPart.TabLength;
            double ntch = aPart.NotchDim;
            double tht = aPart.TabHeight;



            dxePolyline aPl = null;
            colDXFEntities plansegs = rPlanView.Segments;
            dxfPlane aPln = rPlanView.Plane; // xPlane(cx, cy, aCenter, aRotation + 90);

            ; List<dxeArc> arcs = plansegs.Arcs();
            int i = 0;


            dxfPlane bPln = aPln.Clone();
            //if (aPart.Y > aPart.BoxY) bPln.Rotate(90); else bPln.Rotate(-90);

            bool leftViewMustBeDrawn = LeftSideViewMustBeDrawnForEndPlate(rPlanView);
            double fx = (leftViewMustBeDrawn && aPart.BottomSide) || !aPart.IsTriangular ? -1 : 1;
            double fy = leftViewMustBeDrawn && !aPart.BottomSide ? -1 : 1;

            dxePoint arcpt = rPlanView.AdditionalSegments.Points().Find(x => string.Compare(x.Tag, "BEND_POINT_3", true) == 0);
            dxfVector v1 = arcpt.Center.WithRespectToPlane(aPln);

            //v1 = v1.WithRespectToPlane(rPlanView.Plane);
            //v1 = aPln.Vector(v1.X, v1.Y);
            colDXFVectors verts = new colDXFVectors();


            double y1 = v1.Y * fy;
            dxeLine tiplinet = !aPart.BottomSide && !leftViewMustBeDrawn ? plansegs.Lines()[2] : plansegs.Lines()[6];


            dxfVector v2 = tiplinet.StartPt.Clone();
            v2 = v2.WithRespectToPlane(aPln);
            //y1 = 0;
            double y3 = v2.Y;
            double y2 = y3 - tleng;

            double x1 = -tht; //- aPart.DeckThickness;
            double x2 = 0;
            double x3 = ht + x1;
            verts.Add(aPln, x1 * fx, y1 * fy);
            verts.Add(aPln, x3 * fx, y1 * fy, aTag: "TIP");
            verts.Add(aPln, x3 * fx, (y1 + thk) * fy, aTag: "NOTCH_BOT");
            verts.Add(aPln, (x3 - ntch) * fx, (y1 + thk) * fy);
            verts.Add(aPln, (x3 - ntch) * fx, y2 * fy, aTag: "NOTCH_TOP");
            verts.Add(aPln, x2 * fx, y2 * fy, aTag: "TAB_CORNER");
            verts.Add(aPln, x2 * fx, y3 * fy, aTag: "TAB_BOT");
            verts.Add(aPln, x1 * fx, y3 * fy, aTag: "TAB_TOP");
            //if(aRotation != 0)
            //{
            //    verts.RotateAbout(aPln.ZAxis(), aRotation);
            //}

            _rVal = new dxePolygon(verts, aPln.Origin, true, aPlane: aPln);

            //_rVal.AdditionalSegments.AddArc(arcpt.Center, 0.125, aColor: dxxColors.Red);
            _rVal.AdditionalSegments.AddLine(aPln.Vector(x1 * fx, (y1 + thk) * fy), aPln.Vector((x3 - 0.25) * fx, (y1 + thk) * fy), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));

            if (aPart.IsTriangular)
            {
                arcpt = arcpt = rPlanView.AdditionalSegments.Points().Find(x => string.Compare(x.Tag, "BEND_POINT_4", true) == 0);
                v1 = arcpt.Center;
                v1 = v1.WithRespectToPlane(aPln);
                double y5 = y1;

                y1 = y3;
                double y4 = v1.Y * fy;

                tiplinet = !aPart.BottomSide && !leftViewMustBeDrawn ? plansegs.Lines()[6] : plansegs.Lines()[2];


                v2 = tiplinet.StartPt.Clone();
                v2 = v2.WithRespectToPlane(aPln);
                y3 = v2.Y;
                y2 = y3 - tleng;
                verts = new colDXFVectors
                {
                    { aPln, x1 * fx, y1 * fy }
                };
                verts.Add(aPln, x1 * fx, y3 * fy, aTag: "TIP");
                verts.Add(aPln, x2 * fx, y3 * fy, aTag: "NOTCH_BOT");
                verts.Add(aPln, x2 * fx, y2 * fy, aTag: "NOTCH_TOP");
                verts.Add(aPln, (x3 - ntch) * fx, y2 * fy);
                verts.Add(aPln, (x3 - ntch) * fx, y4 * fy);
                verts.Add(aPln, x3 * fx, y4 * fy);
                verts.Add(aPln, x3 * fx, y5 * fy);
                _rVal.AdditionalSegments.Add(new dxePolyline(verts, false));

                _rVal.AdditionalSegments.AddLine(aPln.Vector(x1 * fx, y4 * fy), aPln.Vector(x3 * fx, y4 * fy), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Phantom));


            }

            if (aPl != null)
            {
                //if (!bLaps)
                //{
                //    _rVal.AdditionalSegments.Add( bPln.CreateLine( x3, 0, x3, y1, aLineType: dxfLinetypes.Phantom ) );
                //    _rVal.AdditionalSegments.Add( bPln.CreateLine( x3, 0, x2, 0 ) );
                //}
                //else
                //{
                //    _rVal.AdditionalSegments.Add( bPln.CreateLine( x3, y2, x3, y1, aLineType: dxfLinetypes.Phantom ) );
                //}
                //_rVal.AdditionalSegments.Add( bPln.CreateLine( x3, -ht, x1 + thk, -ht ) );
                //_rVal.AdditionalSegments.Add( aPl );
            }


            if (aPart.GussetLength > 0)
            {

                v1 = verts.GetTagged("NOTCH_TOP", bReturnClone: true);
                v2 = verts.GetTagged("TAB_BOT", bReturnClone: true);
                dxfVector v3 = verts.Item(v1.Index + 1, bReturnClone: true);
                // _rVal.AdditionalSegments.Add(new dxeArc(v1, 0.25));
                dxeLine l1 = verts.LineSegment("NOTCH_TOP");
                colDXFVectors gverts = new colDXFVectors(v2.Clone());
                gverts.AddRelative(0, aPart.GussetLength, aPlane: bPln);
                dxeLine l2 = gverts.LineSegment(2, true);
                l2.RotateAbout(bPln.ZAxis(1, l2.StartPt), !aPart.BottomSide ? 45 : -45);
                l2.EndPt = l2.IntersectPoint(l1, true, true);


                if (l2.EndPt.X >= v1.X)
                {
                    l2.EndPt = v1.Clone();
                    _rVal.Vertex(v1.Index).Flag = "GUSSET";
                }
                else if (l2.EndPt.X < v1.X)
                {
                    verts.Add(l2.EndPt, aAfterIndex: i, aTag: "GUSSET");
                }
                gverts.Add(l2.EndPt, bAddClone: true);
                _rVal.AdditionalSegments.Add(new dxePolyline(gverts, false, aPlane: bPln) { Tag = "GUSSET", LayerName = aLayerName });
            }



            if (aPart.BoltOn && !bSuppressHoles)
            {
                bPln = _rVal.Plane;

                uopHoleArray aHls = aPart.GenHoles("RIGHT");


                for (i = 1; i <= aHls.Count; i++)
                {
                    uopHoles bHls = aHls.Item(i);
                    for (int j = 1; j <= bHls.Count; j++)
                    {
                        uopHole aHl = bHls.Item(j);
                        v1 = aHl.CenterDXF;
                        _rVal.AdditionalSegments.AddPlanarArc(bPln, aHl.Radius, v1.Y - aPart.Y, v1.Z, aLayerName: aLayerName, aTag: aHl.Tag, aFlag: aHl.Flag);
                    }
                }
            }
            if (aRotation != 0)
            {
                //verts.RotateAbout(_rVal.Plane.ZAxis(), aRotation);

                _rVal.Rotate(aRotation);
                // _rVal.AdditionalSegments.RotateAbout(_rVal.Plane.ZAxis(), aRotation); 
            }
            //if (aRotation != 0) _rVal.Rotate( aRotation );

            return _rVal;
        }

        private static bool LeftSideViewMustBeDrawnForEndPlate(dxePolygon planView)
        {
            var allVertices = planView.GetVertices();
            var leftTopVertex = allVertices.GetVector(dxxPointFilters.GetLeftTop);
            var rightTopVertex = allVertices.GetVector(dxxPointFilters.GetRightTop);

            var shouldDrawLeftView = leftTopVertex.Y < rightTopVertex.Y;

            return shouldDrawLeftView;
        }

        /// <summary>
        ///  //#2flag to just return the outline of the pan as view in the layout drawing
        ///^returns a dxePolygon that is used to draw the layout view of the pan
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon APPan_View_Plan(mdAPPan aPart = null, bool bShowObscured = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon();
            if (aPart == null) return _rVal;

            double thk = aPart.Thickness;
            double wd = aPart.Width;
            double leng = aPart.Length;
            double flngln = aPart.FlangeLength;
            dxfVector v0 = aPart.CenterDXF;
            dxfVector v1;
            dxfVector v2;

            double tw = aPart.TabWidth;
            double tl = aPart.TabLength;
            double d1 = aPart.TabInset; //- 0.5 * tw
            double d2 = wd - 2 * tw - 2 * d1;

            uopHoles Hls;
            uopHoleArray aHls;
            uopHole aHl;
            double hiset = aPart.HoleInset;
            dxfPlane aPln;
            dxfPlane bPln = new dxfPlane(v0);
            string lt1 = bShowObscured ? dxfLinetypes.Invisible : "";
            string lt2 = bShowObscured ? dxfLinetypes.Hidden : "";
            double f1 = (v0.Rotation != 0) ? -1 : 1;
            colDXFVectors verts = new colDXFVectors();
            if (aCenter != null) v0 = new dxfVector(aCenter);
            aPln = xPlane(v0.X, v0.Y, v0, aRotation);

            _rVal.LayerName = aLayerName;
            try
            {



                if (!aPart.OpenEnded)
                {

                    verts.Add(aPln, -0.5 * wd, hiset * f1, aLineType: bShowObscured ? dxfLinetypes.Hidden : "");
                    verts.AddRelative(aY: -(flngln - thk) * f1, aLineType: lt2, aPlane: aPln);
                    verts.AddRelative(aY: -thk * f1, aLineType: lt2, aPlane: aPln);

                    verts.AddRelative(aY: -leng * f1, aLineType: lt2, aPlane: aPln);
                    verts.AddRelative(aY: -thk * f1, aLineType: lt2, aPlane: aPln);

                    v1 = verts.AddRelative(aX: d1, aPlane: aPln);
                    verts.AddRelative(aY: thk * f1, aPlane: aPln);
                    verts.AddRelative(aY: -tl * f1, aPlane: aPln, aTag: "TAB_END");
                    verts.AddRelative(aX: tw, aPlane: aPln, aTag: "TAB_END");
                    verts.AddRelative(aY: tl * f1, aPlane: aPln);
                    v2 = verts.AddRelative(aY: -thk * f1, aLineType: lt2, aPlane: aPln);


                    if (!bShowObscured)
                    {
                        _rVal.AdditionalSegments.AddLine(v1, v2, new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));
                    }
                    v1 = verts.AddRelative(aX: d2, aPlane: aPln);
                    verts.AddRelative(aY: thk * f1, aPlane: aPln);
                    verts.AddRelative(aY: -tl * f1, aPlane: aPln, aTag: "TAB_END");
                    verts.AddRelative(aX: tw, aPlane: aPln, aTag: "TAB_END");
                    verts.AddRelative(aY: tl * f1, aPlane: aPln);
                    v2 = verts.AddRelative(aY: -thk * f1, aPlane: aPln, aLineType: bShowObscured ? dxfLinetypes.Hidden : "");
                    verts.AddRelative(aX: d1, aLineType: lt1, aPlane: aPln);
                    verts.AddRelative(aY: thk * f1, aPlane: aPln, aLineType: bShowObscured ? dxfLinetypes.Hidden : "");

                    verts.Add(aPln, 0.5 * wd, (hiset - flngln) * f1, aLineType: lt2);
                    verts.Add(aPln, 0.5 * wd, (hiset - flngln + thk) * f1, aLineType: lt2);
                    verts.Add(aPln, 0.5 * wd, hiset * f1, aLineType: lt2);

                    if (!bShowObscured)
                    {
                        _rVal.AdditionalSegments.AddLine(v1, v2, new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));
                    }

                    //if (!bShowObscured)
                    //{
                    _rVal.AdditionalSegments.AddLine(verts.Item(2), verts.Item(21), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));
                    //}

                    _rVal.AdditionalSegments.AddLine(verts.Item(3), verts.Item(20), new dxfDisplaySettings(aLayerName, aLinetype: bShowObscured ? dxfLinetypes.Hidden : ""));
                    _rVal.AdditionalSegments.AddLine(verts.Item(4), verts.Item(19), new dxfDisplaySettings(aLayerName, aLinetype: bShowObscured ? dxfLinetypes.Hidden : ""));

                }
                else
                {
                    f1 = -f1;
                    verts.Add(aPln, -0.5 * wd, -hiset * f1, aLineType: lt1);
                    verts.Add(aPln, -0.5 * wd, -(hiset - flngln - (aPart.LongLength - thk)) * f1, aLineType: lt2);
                    verts.Add(aPln, 0.5 * wd, -(hiset - flngln - (leng - thk)) * f1, aLineType: lt1);
                    verts.Add(aPln, 0.5 * wd, -hiset * f1, aLineType: lt2);


                }

                _rVal.Vertices = verts;
                if (aPart.OpenEnded) f1 = -f1;


                v1 = verts.LastVector(true);
                //_rVal.AdditionalSegments.AddLine(verts.Item(1) + aPln.YDirection * -flngln * f1, v1 + aPln.YDirection * -flngln * f1, aLayerName: aLayerName, aLineType: bShowObscured ? dxfLinetypes.Hidden : "");
                //_rVal.AdditionalSegments.AddLine(verts.Item(1) + aPln.YDirection * (-flngln + thk) * f1 , v1 + aPln.YDirection * (-flngln + thk) * f1, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden);


                aHls = aPart.GenHoles("");

                for (int j = 1; j <= aHls.Count; j++)
                {
                    Hls = aHls.Item(j);
                    for (int i = 1; i <= Hls.Count; i++)
                    {
                        aHl = Hls.Item(i);
                        v1 = aHl.CenterDXF.WithRespectToPlane(bPln, aPln, 0);


                        dxeArc hole = _rVal.AdditionalSegments.AddArc(v1, aHl.Radius);
                        hole.Linetype = bShowObscured ? dxfLinetypes.Hidden : dxfLinetypes.ByLayer;
                    }
                }



            }
            catch (Exception)
            {

                throw;
            }
            finally

            {


                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
                _rVal.LayerName = aLayerName;
            }
            return _rVal;
        }
        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the stiffener
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aDC"></param>
        /// <param name="aAssy"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="bIncludeFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bHoleCenterLines"></param>
        /// <returns></returns>
        public static dxePolygon Stiffener_View_Plan(mdStiffener aPart, mdDowncomerBox aBox, mdTrayAssembly aAssy, bool bSuppressHoles = false, bool bIncludeFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bHoleCenterLines = false)
        {
            dxePolygon _rVal;
            try
            {
                if (aPart == null) return null;


                _rVal = new dxePolygon(Stiffener_Vertices(aPart, bIncludeFillets, aCenter, aRotation, out dxfPlane aPln), aPln.Origin, false, aPlane: aPln) { LayerName = aLayerName };

                if (!bSuppressHoles)
                {


                    dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y));
                    double rad = 0;

                    dxfVector v1 = null;


                    double t = aPart.Thickness / 2;
                    uopHoleArray aHls = aPart.GenHoles(aBox);

                    foreach (uopHoles Hls in aHls)
                    {
                        for (int i = 1; i <= Hls.Count; i++)
                        {
                            uopHole aH = Hls.Item(i);
                            v1 = new dxfVector(aH.X,aH.Y);
                            //Console.WriteLine(v1.ToString());
                            dxfVector v2 = v1.WithRespectToPlane(bPln);
                            rad = aH.Radius;
                            dxeLine l1 = aPln.CreateLine(v2.X - t, v2.Y + rad, v2.X + t, v2.Y + rad, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden,aTag:aH.Tag,aFlag: aH.Flag);
                            _rVal.AdditionalSegments.Add( l1);
                            dxeLine l2 = aPln.CreateLine(v2.X - t, v2.Y - rad, v2.X + t, v2.Y - rad, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: aH.Tag, aFlag: aH.Flag);
                            _rVal.AdditionalSegments.Add(l2);
                            if (bHoleCenterLines)
                            {
                                dxeLine cl = aPln.CreateLine(v2.X - 4 * t, v2.Y, v2.X + 4 * t, v2.Y, aLayerName: aLayerName, aLineType: dxfLinetypes.Center, aTag: aH.Tag, aFlag: aH.Flag);
                                _rVal.AdditionalSegments.Add(cl);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }

        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the angle

        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon EndAngle_View_Plan(mdEndAngle aPart, iVector aCenter = null, double aRotation = 0, bool bSuppressHoles = false, string aLayerName = "GEOMETRY",bool bIgnoreDirection = false)
        {
            dxePolygon _rVal;
            try
            {

                double thk = aPart.Thickness;
                double wd = aPart.Width;
                double lg = aPart.Length;
                double cX = 0;
                double cY = 0;
                double cZ = 0.0;
                aPart.CenterDXF.GetComponents(ref cX, ref cY, ref cZ);


                double d1 = (wd - thk) / Math.Cos(0.25 * Math.PI);
                double d2 = d1 * Math.Sin(0.25 * Math.PI);
                dxfVector v2 = null;

                double cang = aPart.Chamfered ? 45 : 0;
                double f1 = (aPart.Direction == dxxOrthoDirections.Right) ? 1 : -1;
                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y));
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
                f1 = 1;

                if (aPart.Direction == dxxOrthoDirections.Right && !bIgnoreDirection)
                {
                    aPln.Rotate(180);
                    bPln.Rotate(180);
                }


                _rVal = (dxePolygon)uopGlobals.UopGlobals.Primatives.Angle_Top(aPln.Origin, wd, lg, aPart.Thickness, aChamferAngle: cang, aSegmentWidth: 0, aPlane: aPln, aReturnPolygon: true);
                _rVal.Vertex(1).Linetype = dxfLinetypes.ByLayer;
                _rVal.LayerName = aLayerName;
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
                if (!bSuppressHoles)
                {

                    uopHole aH;
                    uopHoles Hls;
                    uopHoleArray aHls = aPart.GenHoles();
                    for (int j = 1; j <= aHls.Count; j++)
                    {
                        Hls = aHls.Item(j);
                        for (int i = 1; i <= Hls.Count; i++)
                        {
                            aH = Hls.Item(i);
                            v2 = aH.CenterDXF.WithRespectToPlane(bPln);
                            _rVal.AdditionalSegments.Add(aPln.CreateLine((v2.X - thk / 2) * f1, v2.Y + aH.Radius, (v2.X + thk / 2) * f1, v2.Y + aH.Radius, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                            _rVal.AdditionalSegments.Add(aPln.CreateLine((v2.X - thk / 2) * f1, v2.Y - aH.Radius, (v2.X + thk / 2) * f1, v2.Y - aH.Radius, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));

                            _rVal.AdditionalSegments.Add(aPln.CreateLine((v2.X - thk * 2) * f1, v2.Y, (v2.X + thk * 2) * f1, v2.Y, aLayerName: aLayerName, aLineType: "Center"));

                            //_rVal.AdditionalSegments.AddArc(aH.Center, aH.Radius);

                        }
                    }


                }

            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }

        /// <summary>
        ///^returns a dxePolygon that is used to draw the layout view of the part
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="bSuppressOrientation"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon md_FingerClip_View_Plan(mdFingerClip aPart, bool bSuppressHoles = false, bool bSuppressOrientation = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {

                if (aPart == null) return _rVal;


                double thk = 0;

                double wd = 0;

                double cX = 0;

                double cY = 0;

                double lg = 0;

                double d1 = 0;

                double d2 = 0;

                double d3 = 0;

                double d4 = 0;

                double span = 0;

                double swd = 0;

                double slg = 0;

                double rad = 0;

                double swap = 0;


                bool bLeft = false;

                dxfPlane aPln = null;

                double vrad = 0;

                cX = aPart.X;
                cY = aPart.Y;
                wd = aPart.Width;
                thk = aPart.Thickness;
                lg = aPart.Length;
                slg = aPart.SlotLength;
                swd = aPart.SlotWidth;
                span = aPart.SlotSpan;

                if (!bSuppressOrientation)
                {
                    bLeft = aPart.Side == uppSides.Left;
                }
                else
                {
                    bLeft = true;
                }
                aPln = xPlane(cX, cY, aCenter, aRotation);


                if (bLeft)
                {
                    swap = -1;
                }
                else
                {
                    swap = 1;
                }

                rad = swd / 2;
                d1 = wd; //- thk
                d2 = (lg - span - swd) / 2;
                d3 = span - swd;
                d4 = slg - rad;
                if (bLeft)
                {
                    vrad = rad;
                }
                else
                {
                    vrad = -rad;
                }

                _rVal.LayerName = aLayerName;
                _rVal.Vertices.Add(aPln, 0, -0.5 * lg);

                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), d1 * swap));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), aYDisplacement: d2));

                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), -d4 * swap, aVertexRadius: vrad));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), -rad * swap, rad, aVertexRadius: vrad));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), rad * swap, rad));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), d4 * swap));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), aYDisplacement: d3));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), -d4 * swap, aVertexRadius: vrad));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), -rad * swap, rad, aVertexRadius: vrad));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), rad * swap, rad));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), d4 * swap));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), aYDisplacement: d2));
                _rVal.Vertices.Add(aPln.VectorRelative(_rVal.Vertices.LastVector(), -d1 * swap, aTag: "THK"));


                _rVal.AdditionalSegments.Add((dxfEntity)aPln.CreateLine(thk * swap, -0.5 * lg, thk * swap, 0.5 * lg, aLayerName: aLayerName));
                if (!bSuppressHoles)
                {
                    _rVal.AdditionalSegments.Add((dxfEntity)aPln.CreateLine(0, mdGlobals.gsSmallHole / 2, thk * swap, mdGlobals.gsSmallHole / 2, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                    _rVal.AdditionalSegments.Add((dxfEntity)aPln.CreateLine(0, -mdGlobals.gsSmallHole / 2, thk * swap, -mdGlobals.gsSmallHole / 2, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                }

                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;


            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }

        public static dxePolygon BeamSupport_View_Plan(
            mdBeamSupport aPart,
            mdTrayAssembly aAssy = null,
            iVector aCenter = null,
            bool bShowObscured = false,
            bool bSuppressHoles = false,
            string aLayerName = "GEOMETRY",
            bool suppressRotation = false)
        {
            if (aPart == null) return new dxePolygon();

            double cX = aPart.X;
            double cY = aPart.Y;

            colDXFVectors verts = new colDXFVectors(aPart.PlateVerticies());
           if(bShowObscured) verts.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Hidden);
            dxePolygon _rVal = new dxePolygon( verts, dxfVector.Zero, bClosed: true ) { Flag = "PLATE" };

            dxfPlane bPln = xPlane( cX, cY, aCenter, suppressRotation ? 0 : aPart.Rotation );

            if (!bSuppressHoles)
            {
                uopHoleArray holes = aAssy.Beam.GenHoles( "BEAM" );

                var beamHoles = holes.Item( 1 );
                uopHole slot = beamHoles.Member;
                slot.Rotation = 0;
                colDXFVectors ctrs = beamHoles.Centers.ToDXFVectors( true );
                //nullify the rotation of the beam
                ctrs.RotateAbout( aAssy.Beam.Plane, dxxAxisDescriptors.Z, -45 );
                //move the points to the mid point between the left holes aligns with the support's center (0,0)
                ctrs.Translate( new dxfVector( -ctrs[ 0 ].X, -(ctrs[ 0 ].Y - ((ctrs[ 0 ].Y - ctrs[ 2 ].Y) / 2)) ) );
                dxePolyline aPl = (dxePolyline)slot.BoundingEntity( _rVal.LayerName, aLinetype: bShowObscured ? dxfLinetypes.Hidden : "" );

                var centerDs = new dxfDisplaySettings( aLayerName, aLinetype: "Center" );

                foreach (var v1 in ctrs.Where(v => v.Flag == "LEFT"))
                {
                    //dxfVector v1 = item.WithRespectToPlane( aAssy.Beam.Plane, bPln );
                    dxePolyline bPl = new dxePolyline( aPl ) { Tag = beamHoles.Name };
                    bPl.Translate( v1 );
                    _rVal.AdditionalSegments.Add( bPl );
                    _rVal.AdditionalSegments.Add( new dxePoint( v1 ) { LayerName = "DefPoints", Tag = v1.Tag, Flag = v1.Flag } );

                    _rVal.AdditionalSegments.AddLine( v1.X - slot.Length, v1.Y, v1.X + slot.Length, v1.Y, centerDs );
                    _rVal.AdditionalSegments.AddLine( v1.X, v1.Y - 2 * slot.Radius, v1.X, v1.Y + 2 * slot.Radius, centerDs );
                }
            }

            var hiddenDs = new dxfDisplaySettings( aLayerName, aLinetype: dxfLinetypes.Hidden );

            colDXFVectors vertstleg = aPart.TopLegVerticies().ToDXFVectors();
            var tf = vertstleg.GetTagged( "TOP_FOOT" );
            var bf = vertstleg.GetTagged( "BOTTOM_FOOT" );
            _rVal.AdditionalSegments.AddPolyline( vertstleg, false, hiddenDs );
            _rVal.AdditionalSegments.AddLine( tf.X, tf.Y, bf.X, bf.Y, hiddenDs );

            //Add a middle leg by default. User can remove if not needed
            colDXFVectors vertsmleg = aPart.MiddleLegVerticies().ToDXFVectors();
            tf = vertsmleg.GetTagged( "TOP_FOOT" );
            bf = vertsmleg.GetTagged( "BOTTOM_FOOT" );
            _rVal.AdditionalSegments.AddPolyline( vertsmleg, false, hiddenDs );
            _rVal.AdditionalSegments.AddLine( tf.X, tf.Y, bf.X, bf.Y, hiddenDs );

            colDXFVectors vertsbleg = aPart.BottomLegVerticies().ToDXFVectors();
            tf = vertsbleg.GetTagged( "TOP_FOOT" );
            bf = vertsbleg.GetTagged( "BOTTOM_FOOT" );
            _rVal.AdditionalSegments.AddPolyline( vertsbleg, false, hiddenDs );
            _rVal.AdditionalSegments.AddLine( tf.X, tf.Y, bf.X, bf.Y, hiddenDs );

            _rVal.BlockName = $"BEAM_SUPPORT_PLAN_VIEW";

            return _rVal;
        }

        public static dxePolygon BeamSupport_View_Elevation(
            mdBeamSupport aPart,
            mdTrayAssembly aAssy = null,
            iVector aCenter = null,
            bool bShowObscured = false,
            bool bSuppressHoles = false,
            string aLayerName = "GEOMETRY",
            bool suppressRotation = false )
        {
            if (aPart == null) return new dxePolygon();

            double cX = aPart.X;
            double cY = aPart.Y;

            colDXFVectors verts = aPart.GetBottomLegElevationVerticies().ToDXFVectors();
            dxePolygon _rVal = new dxePolygon( verts, dxfVector.Zero );

            var verts2 = aPart.GetMidLegElevationVerticies().ToDXFVectors();
            _rVal.AdditionalSegments.AddPolyline( verts2, false );

            var verts3 = aPart.GetTopLegElevationVerticies().ToDXFVectors();
            _rVal.AdditionalSegments.AddPolyline( verts3, false );

            //close the plate graphics
            var p1 = verts.GetTagged( "RingSideTop" );
            var p2 = verts3.GetTagged( "RingSideTop" );
            if (p1 == p2)
                p2 = verts2.GetTagged( "RingSideTop" );

            colDXFVectors verts4 = new colDXFVectors( p1, new dxfVector( p2.X, p1.Y ), p2, new dxfVector( p1.X, p2.Y ) );
            _rVal.AdditionalSegments.AddPolyline( verts4, false );

            dxfPlane bPln = xPlane( cX, cY, aCenter, suppressRotation ? 0 : aPart.Rotation );
            var hiddenDs = new dxfDisplaySettings( aLayerName, aLinetype: dxfLinetypes.Hidden );
            var centerDs = new dxfDisplaySettings( aLayerName, aLinetype: "Center" );

            if (!bSuppressHoles)
            {
                var leftTopH = verts.GetTagged( "Top_Left_Hole_Limit" );
                var rightTopH = verts.GetTagged( "Top_Right_Hole_Limit" );
                var leftBotH = verts.GetTagged( "Bottom_Left_Hole_Limit" );
                var rightBotH = verts.GetTagged( "Bottom_Right_Hole_Limit" );

                _rVal.AdditionalSegments.AddLine( leftTopH.X, leftTopH.Y, leftBotH.X, leftBotH.Y, hiddenDs );
                _rVal.AdditionalSegments.AddLine( rightTopH.X, rightTopH.Y, rightBotH.X, rightBotH.Y, hiddenDs );

                _rVal.AdditionalSegments.AddLine( 0, -aPart.PlateThk * 2.5, 0, aPart.PlateThk * 1.5, centerDs );
            }

            _rVal.BlockName = $"BEAM_SUPPORT_ELEVATION_VIEW";

            return _rVal;
        }

        public static dxePolygon BeamSupport_View_Elevation_End(
            mdBeamSupport aPart,
            mdTrayAssembly aAssy = null,
            iVector aCenter = null,
            bool bShowObscured = false,
            bool bSuppressHoles = false,
            bool bAddWelds = true,
            string aLayerName = "GEOMETRY",
            bool suppressRotation = false )
        {
            if (aPart == null) return new dxePolygon();

            double cX = aPart.X;
            double cY = aPart.Y;

            colDXFVectors verts = aPart.PlateEndViewVerticies().ToDXFVectors();
            dxePolygon _rVal = new dxePolygon( verts, dxfVector.Zero, bClosed: true );

            //legs
            //middle
            var legVerts = aPart.LegEndViewVerticies().ToDXFVectors();
            _rVal.AdditionalSegments.AddPolyline( legVerts, false );
            var f1 = legVerts.GetTagged( "Foot_Left" );
            var f2 = legVerts.GetTagged( "Foot_Right" );
            _rVal.AdditionalSegments.AddLine( f1.X, f1.Y, f2.X, f2.Y );

            //left
            legVerts = legVerts.Clone();
            legVerts.MoveFromTo( legVerts[ 0 ], verts.GetTagged( "Left_Leg_Left" ) );
            _rVal.AdditionalSegments.AddPolyline( legVerts, false );
            f1 = legVerts.GetTagged( "Foot_Left" );
            f2 = legVerts.GetTagged( "Foot_Right" );
            _rVal.AdditionalSegments.AddLine( f1.X, f1.Y, f2.X, f2.Y );

            //right
            legVerts = legVerts.Clone();
            legVerts.MoveFromTo( legVerts[ 0 ], verts.GetTagged( "Right_Leg_Left" ) );
            _rVal.AdditionalSegments.AddPolyline( legVerts, false );
            f1 = legVerts.GetTagged( "Foot_Left" );
            f2 = legVerts.GetTagged( "Foot_Right" );
            _rVal.AdditionalSegments.AddLine( f1.X, f1.Y, f2.X, f2.Y );

            dxfPlane bPln = xPlane( cX, cY, aCenter, suppressRotation ? 0 : aPart.Rotation );
            var hiddenDs = new dxfDisplaySettings( aLayerName, aLinetype: dxfLinetypes.Hidden );
            var centerDs = new dxfDisplaySettings( aLayerName, aLinetype: "Center" );

            if (!bSuppressHoles)
            {
                var lhl = verts.GetTagged( "Left_Hole_Left" );
                var lhlb = verts.GetTagged( "Left_Hole_Left_Bottom" );
                var lhr = verts.GetTagged( "Left_Hole_Right" );
                var lhrb = verts.GetTagged( "Left_Hole_Right_Bottom" );

                var rhl = verts.GetTagged( "Right_Hole_Left" );
                var rhlb = verts.GetTagged( "Right_Hole_Left_Bottom" );
                var rhr = verts.GetTagged( "Right_Hole_Right" );
                var rhrb = verts.GetTagged( "Right_Hole_Right_Bottom" );

                _rVal.AdditionalSegments.AddLine( lhl.X, lhl.Y, lhlb.X, lhlb.Y, hiddenDs );
                _rVal.AdditionalSegments.AddLine( lhr.X, lhr.Y, lhrb.X, lhrb.Y, hiddenDs );

                _rVal.AdditionalSegments.AddLine( rhl.X, rhl.Y, rhlb.X, rhlb.Y, hiddenDs );
                _rVal.AdditionalSegments.AddLine( rhr.X, rhr.Y, rhrb.X, rhrb.Y, hiddenDs );

                var lhc = verts.GetTagged( "Left_Hole_Center" );
                var rhc = verts.GetTagged( "Right_Hole_Center" );

                _rVal.AdditionalSegments.AddLine( lhc.X, aPart.PlateThk, lhc.X, -aPart.PlateThk * 2, centerDs );
                _rVal.AdditionalSegments.AddLine( rhc.X, aPart.PlateThk, rhc.X, -aPart.PlateThk * 2, centerDs );
            }

            _rVal.AdditionalSegments.AddLine( 0, aPart.PlateThk, 0, -(aPart.Height + aPart.PlateThk), centerDs );

            if (bAddWelds)
            {
                double w = 12.7 / 25.4;
                var v1 = verts.GetTagged( "Left_Leg_Left" );
                var v2 = new dxfVector( v1.X - w, v1.Y );
                var v3 = new dxfVector( v1.X, v1.Y - w );
                _rVal.AdditionalSegments.Add( new dxeSolid( v1, v2, v3 ) { Tag = "WELD_1" } );

                v1 = verts.GetTagged( "Left_Leg_Right" );
                v2 = new dxfVector( v1.X + w, v1.Y );
                v3 = new dxfVector( v1.X, v1.Y - w );
                _rVal.AdditionalSegments.Add( new dxeSolid( v1, v2, v3 ) { Tag = "WELD_2" } );

                v1 = verts.GetTagged( "Middle_Leg_Left" );
                v2 = new dxfVector( v1.X - w, v1.Y );
                v3 = new dxfVector( v1.X, v1.Y - w );
                _rVal.AdditionalSegments.Add( new dxeSolid( v1, v2, v3 ) { Tag = "WELD_3" } );

                v1 = verts.GetTagged( "Middle_Leg_Right" );
                v2 = new dxfVector( v1.X + w, v1.Y );
                v3 = new dxfVector( v1.X, v1.Y - w );
                _rVal.AdditionalSegments.Add( new dxeSolid( v1, v2, v3 ) { Tag = "WELD_4" } );

                v1 = verts.GetTagged( "Right_Leg_Left" );
                v2 = new dxfVector( v1.X - w, v1.Y );
                v3 = new dxfVector( v1.X, v1.Y - w );
                _rVal.AdditionalSegments.Add( new dxeSolid( v1, v2, v3 ) { Tag = "WELD_5" } );

                v1 = verts.GetTagged( "Right_Leg_Right" );
                v2 = new dxfVector( v1.X + w, v1.Y );
                v3 = new dxfVector( v1.X, v1.Y - w );
                _rVal.AdditionalSegments.Add( new dxeSolid( v1, v2, v3 ) { Tag = "WELD_6" } );

            }


            _rVal.BlockName = $"BEAM_SUPPORT_ELEVATION_END_VIEW";

            return _rVal;
        }


        /// <summary>
        /// Returns the polygon for the plan view of the beam
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCenter"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon Beam_View_Plan(
            mdBeam aPart,
            mdTrayAssembly aAssy = null,
            iVector aCenter = null,
            bool bShowObscured = false,
            bool bSuppressHoles = false,
            string aLayerName = "GEOMETRY",
            double aCenterLineLength = 0,
            bool suppressRotation = false)
        {
            dxePolygon _rVal = new dxePolygon() { LayerName = aLayerName };

            if (aPart == null) return _rVal;
            aAssy ??= aPart.GetMDTrayAssembly(aAssy);

            dxfPlane aPln = aPart.Plane;
            double thk = aPart.WebThickness;
            double wd = aPart.Width / 2;
            double leng = aPart.Length / 2;

            double cX = aPart.X;
            double cY = aPart.Y;

            string lt = bShowObscured ? dxfLinetypes.Hidden : "";

            dxfPlane bPln = xPlane(cX, cY, aCenter, suppressRotation ? 0 : aPart.Rotation);
            colDXFVectors verts;

            try
            {
            
                verts = new colDXFVectors(bPln.Vector(-leng,wd), bPln.Vector(-leng,-wd), bPln.Vector( leng,-wd), bPln.Vector(leng,wd));
                
                _rVal.Vertices = verts;
                _rVal.Closed = true;
                _rVal.Plane = bPln;
                if (bShowObscured) _rVal.Linetype = dxfLinetypes.Hidden;
                if (aCenterLineLength != 0)
                {
                    _rVal.AdditionalSegments.AddPlanarLine(bPln, -aCenterLineLength / 2, 0, aCenterLineLength / 2, 0, aColor: dxxColors.Red, aLineType: "Center", aTag: "CENTERLINE");
                }

                //the web
                wd = thk / 2;
                leng -= aPart.WebInset;
                lt = dxfLinetypes.Hidden;
                verts = new colDXFVectors(bPln.Vector(-leng, wd, aLineType: lt), bPln.Vector(-leng, -wd, aLineType: lt), bPln.Vector(leng, -wd, aLineType: lt), bPln.Vector(leng, wd, aLineType: lt));
                dxePolyline web = new dxePolyline(verts, true, dxfDisplaySettings.Null(_rVal.LayerName,aLinetype: lt), bPln);
                _rVal.AdditionalSegments.Add(web);
                if (!bSuppressHoles)
                {
                    uopHoleArray holes = aPart.GenHoles("BEAM");

                    var beamHoles = holes.Item(1);
                    uopHole slot = beamHoles.Member;
                    slot.Rotation = suppressRotation ? 0 : aPart.Rotation;
                    colDXFVectors ctrs = beamHoles.Centers.ToDXFVectors(true);
                    dxePolyline aPl = (dxePolyline)slot.BoundingEntity(_rVal.LayerName, aLinetype: dxfLinetypes.Hidden);

                    foreach (var item in ctrs)
                    {
                        dxfVector v1 = item.WithRespectToPlane(aPln, bPln);
                        dxePolyline bPl = new dxePolyline(aPl) { Tag = beamHoles.Name };
                        bPl.Translate(v1);
                        _rVal.AdditionalSegments.Add(bPl);
                        _rVal.AdditionalSegments.Add(new dxePoint(v1) { LayerName = "DefPoints", Tag = v1.Tag, Flag = v1.Flag });
                    }




                    //double radius = slot.Diameter / 2;
                    //double holeLength = slot.Length; // This is the whole length of the hole
                    //double halfHoleLength = holeLength / 2;
                    //double centersOffset = holeLength - beamHoles.Member.Diameter; // This is the distance between the centers of the two half circles of the hole
                    //double halfCentersOffset = centersOffset / 2;

                    //dxfVector holeCenterWithRespectToBeamPlane;
                    //dxfVector redLineStart = new dxfVector();
                    //dxfVector redLineEnd = new dxfVector();
                    //foreach (var holeCenter in beamHoles.Centers)
                    //{
                    //    holeCenterWithRespectToBeamPlane = new dxfVector(holeCenter).WithRespectToPlane(aPart.Plane);

                    //    // Hole red lines
                    //    double horizontalRedLineHalfLength = 1.4173;
                    //    redLineStart = bPln.Vector(holeCenterWithRespectToBeamPlane.X - horizontalRedLineHalfLength, holeCenterWithRespectToBeamPlane.Y);
                    //    redLineEnd = bPln.Vector(holeCenterWithRespectToBeamPlane.X + horizontalRedLineHalfLength, holeCenterWithRespectToBeamPlane.Y);
                    //    dxeLine horizontalRedLine = new dxeLine(redLineStart, redLineEnd, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Continuous, aColor: dxxColors.Red));
                    //    _rVal.AdditionalSegments.Add(horizontalRedLine);

                    //    double verticalRedLineHalfLength = 1.0236;
                    //    redLineStart = bPln.Vector(holeCenterWithRespectToBeamPlane.X, holeCenterWithRespectToBeamPlane.Y + verticalRedLineHalfLength);
                    //    redLineEnd = bPln.Vector(holeCenterWithRespectToBeamPlane.X, holeCenterWithRespectToBeamPlane.Y - verticalRedLineHalfLength);
                    //    dxeLine verticalRedLine = new dxeLine(redLineStart, redLineEnd, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Continuous, aColor: dxxColors.Red));
                    //    _rVal.AdditionalSegments.Add(verticalRedLine);

                    //    // Hole shape
                    //    verts = new colDXFVectors();
                    //    verts.Add(bPln.Vector(holeCenterWithRespectToBeamPlane.X - halfCentersOffset, holeCenterWithRespectToBeamPlane.Y + radius, aVertexRadius: radius)); // Top left corner of the hole (start of the arc)
                    //    verts.Add(bPln.Vector(holeCenterWithRespectToBeamPlane.X - halfHoleLength, holeCenterWithRespectToBeamPlane.Y, aVertexRadius: radius)); // Left arc mid point
                    //    verts.Add(bPln.Vector(holeCenterWithRespectToBeamPlane.X - halfCentersOffset, holeCenterWithRespectToBeamPlane.Y - radius)); // Bottom left corner of the hole
                    //    verts.Add(bPln.Vector(holeCenterWithRespectToBeamPlane.X + halfCentersOffset, holeCenterWithRespectToBeamPlane.Y - radius, aVertexRadius: radius)); // Bottom right corner of the hole (start of the arc)
                    //    verts.Add(bPln.Vector(holeCenterWithRespectToBeamPlane.X + halfHoleLength, holeCenterWithRespectToBeamPlane.Y, aVertexRadius: radius)); // Right arc mid point
                    //    verts.Add(bPln.Vector(holeCenterWithRespectToBeamPlane.X + halfCentersOffset, holeCenterWithRespectToBeamPlane.Y + radius)); // Top right corner of the hole
                    //    dxePolyline holePolyline = new dxePolyline(verts, true, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Continuous), bPln);
                    //    _rVal.AdditionalSegments.Add(holePolyline);
                    //}
                }

                return _rVal;
            }
            catch
            {
                return _rVal;
            }
            finally
            {
                _rVal.Plane = bPln;
                _rVal.InsertionPt = bPln.Origin;

            }
        }

        /// <summary>
        /// ^returns a dxePolygon that is used to draw the elevation view of the stiffener
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="bVisiblePartOnly"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="bHoleCenterLines"></param>
        /// <returns></returns>
        public static dxePolygon Stiffener_View_Elevation(mdStiffener aPart, bool bShowObscured = false, bool bVisiblePartOnly = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bHoleCenterLines = false)
        {
            dxePolygon _rVal = new dxePolygon();

            if (aPart == null) return _rVal;
            mdTrayAssembly aAssy = aPart.GetMDTrayAssembly();
            if (aAssy == null) return _rVal;
            mdDowncomer aDC = aAssy.Downcomers.Item(aPart.DowncomerIndex, bSuppressIndexError: true);
            if (aDC == null) aDC = aAssy.Downcomer(1);
            if (aDC == null) return _rVal;
            double thk = aPart.Thickness;
            double ht = aPart.Height;
            double cX = aDC.X;
            double cY = aPart.Z;
            string lt = bShowObscured ? dxfLinetypes.Hidden : dxfLinetypes.ByLayer;
            dxfVector v1;
            double wd = 0.5 * aPart.Width;
            double supHt = aPart.SupplementalDeflectorHeight;
            double bafthk = aPart.BaffleThickness;
            uopHoleArray aHls = aPart.GenHoles(aDC.Boxes.First(b => b.Index == aPart.BoxIndex));

            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
            dxfPlane bPln = aDC.Plane;

            double d1 = 1 + bafthk + 0.625;
            double y1 = -d1;
            double y2 = ht + y1;
            double y3 = 0;
            double y4 = 0;
            double y5 = aDC.How + aDC.DeckThickness - cY;  // top of dc box relative to part plane
            double x1 = 0;
            double x2;
            double supgag = 0;
            bool bBaffl = aPart.SupportsBaffle;
            colDXFVectors verts = new colDXFVectors();
            if (supHt > 0) supgag = 1 / 16 + thk;


            if (!bBaffl)
            {
                verts.Add(aPln, -wd + thk, y1, aLineType: lt);
                verts.Add(aPln, -wd, y1, aLineType: lt);
                verts.Add(aPln, -wd, y2);
                verts.Add(aPln, -(wd - thk), y2);
                if (supHt > 0)
                {
                    //notch for supl. deflector
                    verts.Add(aPln, -supgag / 2, y2, aTag: "SUPDEF", aLineType: lt);
                    verts.AddRelative(aY: -supHt, aPlane: aPln, aLineType: lt, aTag: "SUPDEF");
                    verts.AddRelative(supgag, aPlane: aPln, aLineType: lt, aTag: "SUPDEF");
                    verts.AddRelative(aY: supHt, aPlane: aPln, aLineType: lt, aTag: "SUPDEF");
                }

                verts.Add(aPln, wd - thk, y2);
                verts.Add(aPln, wd, y2, aLineType: lt);
                verts.Add(aPln, wd, y1, aLineType: lt);

            }
            else
            {

                double rad = 0.125;
                uopHole aHl = aHls.Item("BAFFLE MOUNT").Item(1);
                v1 = aHl.CenterDXF.WithRespectToPlane(bPln, bMaintainZ: true);
                y4 = v1.Z - aPart.Z + aHl.DownSet;
                y3 = y4 - aPart.BaffleMountHeight;
                x1 = v1.X + 0.5 * thk;
                x2 = x1 - 0.5 + rad;

                verts.Add(aPln, -wd, y1);
                verts.Add(aPln, -wd, y5);
                verts.Add(aPln, -wd + 0.25, y5);
                verts.Add(aPln, x1 - 0.75, y4);
                verts.Add(aPln, x1, y4);
                verts.Add(aPln, x1, y3);
                verts.Add(aPln, x2, y3, aVertexRadius: rad);
                verts.Add(aPln, x2 - rad, y3 - rad, aVertexRadius: rad);
                verts.Add(aPln, x2, y5);
                if (supHt > 0)
                {
                    //notch for supl. deflector
                    verts.Add(aPln, x1 + 0.0625, y5, aTag: "SUPDEF", aLineType: lt);
                    verts.AddRelative(aY: -supHt, aPlane: aPln, aLineType: lt, aTag: "SUPDEF");
                    verts.AddRelative(aX: supgag, aPlane: aPln, aLineType: lt, aTag: "SUPDEF");
                    verts.AddRelative(aY: supHt, aPlane: aPln, aLineType: lt, aTag: "SUPDEF");
                }

                verts.Add(aPln, wd, y5);
                verts.Add(aPln, wd, y1);


            }
            _rVal.Vertices = verts;

            lt = (aPart.X > 0) ? string.Empty : dxfLinetypes.Hidden;


            if (!bVisiblePartOnly)
            {
                _rVal.AdditionalSegments.Add(aPln.CreateLine(-wd + thk, y1, -wd + thk, y5, aLayerName: aLayerName, aLineType: lt, aTag: "THK"));
                _rVal.AdditionalSegments.Add(aPln.CreateLine(wd - thk, y1, wd - thk, y5, aLayerName: aLayerName, aLineType: lt, aTag: "THK"));


            }
            if (aPart.SupportsBaffle && y3 < y4)
            {
                _rVal.AdditionalSegments.Add(aPln.CreateLine(x1 - thk, y3, x1 - thk, y4, aLayerName: aLayerName, aLineType: lt, aTag: "THK"));
            }

            foreach(uopHoles Hls in aHls)
            {
          
                for (int i = 1; i <= Hls.Count; i++)
                {
                    uopHole aHl = Hls.Item(i);
                    v1 = aHl.CenterDXF.WithRespectToPlane(bPln, bMaintainZ: true);
                    v1.Z -= aPart.Z;
                    double rad =aHl.IsElongated ?  0.5 * aHl.Length : aHl.Radius;
                    _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - thk / 2, v1.Z + rad, v1.X + thk / 2, v1.Z + rad, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden), aTag: aHl.Tag, aFlag: aHl.Flag, aValue: aHl.Value);
                    _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - thk / 2, v1.Z - rad, v1.X + thk / 2, v1.Z - rad, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden), aTag: aHl.Tag, aFlag: aHl.Flag, aValue: aHl.Value);
                    if (bHoleCenterLines)
                    {
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - 4 * thk, v1.Z, v1.X + 4 * thk, v1.Z, aLayerName: aLayerName, aLineType: "Center"), aTag: aHl.Tag, aFlag: aHl.Flag, aValue: aHl.Value);

                    }
                }
            }


            _rVal.Plane = aPln;
            _rVal.InsertionPt = aPln.Origin;
            _rVal.LayerName = aLayerName;


            return _rVal;
        }


        public static dxePolygon DC_View_Elevation(mdDowncomer aPart, mdTrayAssembly aAssy, bool bCrossSection = false, bool bIncludeBoltOns = false, iVector aCenter = null, double aRotation = 0, string aBoltOnList = "", string aLayerName = "GEOMETRY", bool bOneLayer = true)
        {
            dxePolygon _rVal = new dxePolygon(); ;
            try
            {

                if (aPart == null) return _rVal;

                aAssy = aPart.GetMDTrayAssembly(aAssy);
                if (aAssy == null) return new dxePolygon();
                dxfPlane aPln = null;

                mdDowncomerBox aBox = null;
                colDXFVectors verts = new colDXFVectors();


                mdEndPlate aEP = null;
                dxePolygon aPg = null;
                mdEndSupport aES = null;
                mdSupplementalDeflector aSup = null;
                dxfPlane bPln = aPart.Plane;
                dxfVector v1 = null;



                colDXFEntities aEnts = null;

                string LName = string.Empty;
                aBox = aPart.Boxes.FirstOrDefault();
                if (aBox == null)
                {
                    return _rVal;
                }

                if (!string.IsNullOrWhiteSpace(aBoltOnList))
                {
                    bIncludeBoltOns = true;
                }
                double wht = aPart.How;
                double thk = aPart.Thickness;
                double dkthk = aAssy.Deck.Thickness;


                _rVal = DCBox_View_Elevation(aBox, aAssy, false, false, true, aCenter, aRotation, aLayerName);

                aPln = _rVal.Plane;
                if (!bCrossSection)
                {
                    //=========== END PLATES ==================================
                    aEP = aPart.Boxes[0].EndPlate(bTop: true);

                    v1 = aPln.Origin;
                    LName = bOneLayer ? aLayerName : "END_PLATES";

                    aPg = EndPlate_View_Elevation(aEP, v1, aRotation, LName);
                    aEnts = aPg.SubEntities();

                    _rVal.AdditionalSegments.Append(aEnts, false, "END PLATE");

                    //=========== END SUPPORTS ==================================
                    aES = aPart.Boxes[0].EndSupport(bTop: true);
                    v1 = aPln.Origin;
                    LName = bOneLayer ? aLayerName : "END_SUPPORTS";

                    aPg = EndSupport_View_Elevation(aES, false, v1, aRotation, LName);
                    aEnts = aPg.SubEntities();

                    _rVal.AdditionalSegments.Append(aEnts, false, "END SUPPORT");

                }


                if (bIncludeBoltOns)
                {
                    mdEndAngle aEA = null;
                    mdFingerClip aFC = null;
                    mdStiffener aStf = null;
                    colUOPParts aPrts = null;
              
                    mdAPPan aPan = null;
                    string flag = string.Empty;

                    //=========== END ANGLES ==================================
                    if (mzUtils.ListContains("END ANGLES", aBoltOnList, bReturnTrueForNullList: true))
                    {
                        LName = bOneLayer ? aLayerName : "END_ANGLES";

                        List<mdEndAngle> eas = mdPartGenerator.EndAngles_DC(aPart, aAssy, bTrayWide: false);
                        for (int i = 1; i <= eas.Count; i++)
                        {
                            aEA = eas[i - 1];
                            v1 = aEA.CenterDXF;
                            if (v1.Y > aEA.DowncomerBox.Y)
                            {
                                v1 = v1.WithRespectToPlane(bPln, bMaintainZ: true);
                                v1 = aPln.Vector(v1.X, v1.Z);
                                aPg = EndAngle_View_Elevation(aEA, false, v1, aRotation, LName);
                                flag = (v1.X > aPart.X) ? "RIGHT" : "LEFT";
                                _rVal.AdditionalSegments.Append(aPg.SubEntities(), false, "END ANGLE", flag);

                            }


                        }

                    }
                    else
                    {
                        //=========== FINGER CLIPS ==================================

                        if (mzUtils.ListContains("FINGER CLIPS", aBoltOnList, bReturnTrueForNullList: true))
                        {
                            LName = bOneLayer ? aLayerName : "FINGER_CLIPS";

                            aPrts = mdUtils.FingerClips(aAssy, null, aPart.Index, 0, uppSides.Left);
                            if (aPrts.Count > 0)
                            {
                                aFC = (mdFingerClip)aPrts.Item(1);

                                v1 = aFC.CenterDXF.WithRespectToPlane(bPln, bMaintainZ: true);
                                v1 = aPln.Vector(v1.X, v1.Z);
                                aPg = md_FingerClip_View_Elevation(aFC, false, v1, aRotation, LName);
                                _rVal.AdditionalSegments.Append(aPg.SubEntities(), false, "FINGER CLIP", "LEFT");

                            }
                            aPrts = mdUtils.FingerClips(aAssy, null, aPart.Index, 0, uppSides.Right);
                            if (aPrts.Count > 0)
                            {
                                aFC = (mdFingerClip)aPrts.Item(1);

                                v1 = aFC.CenterDXF.WithRespectToPlane(bPln, bMaintainZ: true);
                                v1 = aPln.Vector(v1.X, v1.Z);
                                aPg = md_FingerClip_View_Elevation(aFC, false, v1, aRotation, LName);
                                _rVal.AdditionalSegments.Append(aPg.SubEntities(), false, "FINGER CLIP", "RIGHT");

                            }


                        }
                    }


                    //=========== STIFFENERS ==================================
                    if (mzUtils.ListContains("STIFFENERS", aBoltOnList, bReturnTrueForNullList: true))
                    {
                        LName = bOneLayer ? aLayerName : "STIFFENERS";

                        var boxStiffenerOrdinates = aBox.GetStiffenerOrdinates();
                        aStf = aPart.Stiffener(0, aAssy);
                        if (boxStiffenerOrdinates.Any())
                        {
                            if (aBox.IsVirtual)
                            {
                                // When the box is virtual it doesn't have any stiffener. To find an stiffener instance we use its non-virtual counterpart.
                                // However, the instance does not have the correct coordinates. That is why we need to create the correct coordinates manually here.
                                v1 = new dxfVector(aBox.X, boxStiffenerOrdinates.First(), aStf.Z).WithRespectToPlane(bPln, bMaintainZ: true);
                            }
                            else
                            {
                                v1 = aStf.Center.ToDXFVector(aZ: aStf.Z).WithRespectToPlane(bPln, bMaintainZ: true);
                            }

                            v1 = aPln.Vector(v1.X, v1.Z);
                            aPg = Stiffener_View_Elevation(aStf, !bCrossSection, !bCrossSection, v1, aRotation, LName);

                            aEnts = aPg.SubEntities();
                            _rVal.AdditionalSegments.Append(aEnts, true, "STIFFENER");

                            if (aPart.SupplementalDeflectorHeight > 0)
                            {
                                LName = bOneLayer ? aLayerName : "SUPL_DEFL";

                                aSup = aPart.SupplementalDeflector();
                                v1 = aSup.CenterDXF.WithRespectToPlane(bPln);
                                v1 = aPln.Vector(v1.X, v1.Z);
                                aPg = SupDef_View_Elevation(aSup, v1, aRotation, LName);
                                aEnts = aPg.SubEntities();
                                _rVal.AdditionalSegments.Append(aEnts, true, "SUPPL. DEFL.");


                            }


                        }


                    }

                    //=========== BAFFLES ==================================
                    if (mzUtils.ListContains("BAFFLES", aBoltOnList, bReturnTrueForNullList: true))
                    {
                        if (aAssy.DesignFamily.IsEcmdDesignFamily() && aAssy.DesignOptions.CDP > 0)
                        {
                            LName = bOneLayer ? aLayerName : "DEFLECTORS";

                            mdDowncomerBox nonVirtualBox;

                            var box = aPart.Boxes.First();
                            nonVirtualBox = box.IsVirtual ? box.GetNonVirtualCounterpart() : box;

                            List<mdBaffle> baffles = mdPartGenerator.DeflectorPlates_DC(aPart, aAssy, bSuppressInstances: true);
                            foreach (var boxBaffle in baffles)
                            {
                                v1 = boxBaffle.CenterDXF.WithRespectToPlane(nonVirtualBox.Downcomer.Plane, bMaintainZ: true);
                                v1 = aPln.Vector(box.IsVirtual ? -v1.X : v1.X, v1.Z);

                                aPg = Baffle_View_Elevation(boxBaffle, aAssy, v1, aRotation, LName);

                                aEnts = aPg.SubEntities();

                                _rVal.AdditionalSegments.Append(aEnts, true, "BAFFLE");
                            }
                        }

                    }

                    //=========== AP PANS ==================================
                    if (mzUtils.ListContains("APPANS", aBoltOnList, bReturnTrueForNullList: true))
                    {
                        if (aAssy.DesignOptions.HasAntiPenetrationPans)
                        {
                            LName = bOneLayer ? aLayerName : "AP_PANS";

                            List<mdAPPan> pans = mdPartGenerator.APPans_DC(aAssy, aPart.Index);
                            if (pans.Count > 0)
                            {
                                aPan = pans[0];
                                v1 = aPan.CenterDXF;
                                v1 = v1.WithRespectToPlane(bPln);
                                v1 = aPln.Vector(v1.X, v1.Z);
                                aPg = APPan_View_Elevation(aPan, false, true, v1, aRotation, LName);
                                aEnts = aPg.SubEntities();
                                _rVal.AdditionalSegments.Append(aEnts, false);
                            }


                        }

                    }
                }


            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }

        /// <summary>
        ///returns a dxePolygon that is used to draw the elevation view of the pan
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aTabEnd"></param>
        /// <param name="bShowObscured"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aScale"></param>
        /// <returns></returns>
        public static dxePolygon APPan_View_Elevation(mdAPPan aPart, bool aTabEnd = true, bool bShowObscured = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", double aScale = 1)
        {
            dxePolygon _rVal = new dxePolygon();
            try
            {


                if (aPart == null) return _rVal;

                double thk = aPart.Thickness * aScale;
                double ht = aPart.Height * aScale;
                double tabwd = aPart.TabWidth * aScale;
                dxfVector v0 = aPart.CenterDXF;
                double cX = v0.X;
                double cY = v0.Y;
                double wd = aPart.Width * aScale;
                double d1 = aPart.TabSideHeight * aScale - ht;
                double d2 = aPart.TabInset * aScale;
                double d3 = wd - 2 * aPart.TabWidth * aScale - 2 * d2;
                string lt1 = dxfLinetypes.ByLayer;
                string lt2 = dxfLinetypes.Hidden;
                string lt3 = dxfLinetypes.Hidden;
                double tabht = aPart.TabHeight * aScale;


                dxfVector v1 = null;
                dxfVector v2 = null;


                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y));
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation, aScale);
                colDXFVectors verts = new colDXFVectors();

                if (!aTabEnd)
                {
                    lt1 = dxfLinetypes.Hidden;
                    lt2 = dxfLinetypes.ByLayer;
                    lt3 = dxfLinetypes.ByLayer;
                }

                if (!aPart.OpenEnded)
                {


                    verts.Add(aPln, -0.5 * wd, thk / 2 - (ht - thk), aLineType: dxfLinetypes.Hidden);
                    verts.AddRelative(aX: wd, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: ht - thk, aPlane: aPln);
                    verts.AddRelative(aX: -d2, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: tabht, aPlane: aPln, aLineType: lt1);
                    verts.AddRelative(aX: -tabwd, aY: 0, aPlane: aPln);
                    v1 = verts.AddRelative(aX: 0, aY: thk, aPlane: aPln);
                    verts.AddRelative(aX: tabwd, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: -(tabht + thk), aPlane: aPln, aLineType: lt2);
                    verts.AddRelative(aX: -tabwd, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: tabht, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: -tabht, aPlane: aPln);
                    verts.AddRelative(aX: -d3, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: tabht, aPlane: aPln, aLineType: lt1);
                    verts.AddRelative(aX: -tabwd, aY: 0, aPlane: aPln);
                    v2 = verts.AddRelative(aX: 0, aY: thk, aPlane: aPln);
                    verts.AddRelative(aX: tabwd, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: -(tabht + thk), aPlane: aPln, aLineType: lt2);
                    verts.AddRelative(aX: -tabwd, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: tabht, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: -tabht, aPlane: aPln);
                    verts.AddRelative(aX: -d2, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: -ht, aPlane: aPln);
                    verts.AddRelative(aX: wd, aY: 0, aPlane: aPln);
                    verts.AddRelative(aX: 0, aY: ht - thk, aPlane: aPln, aLineType: lt3);
                    verts.AddRelative(aX: -wd, aY: 0, aPlane: aPln);


                }
                else
                {

                }



                _rVal = new dxePolygon(verts, aPln.Origin, false, aPlane: aPln) { LayerName = aLayerName };

                if (!bShowObscured)
                {
                    v1 = v1.Moved(0.5 * tabwd, -thk / 2, aPlane: aPln);
                    _rVal.AdditionalSegments.AddPoint(v1, aLayerName: "DEFPOINTS", aTag: "TAB_CENTER");
                    v2 = v2.Moved(0.5 * tabwd, -thk / 2, aPlane: aPln);
                    _rVal.AdditionalSegments.AddPoint(v2, aLayerName: "DEFPOINTS", aTag: "TAB_CENTER");
                }
                if (aPart.OpenFaced)
                {
                    _rVal.AdditionalSegments.AddRectangle(new dxfVector(aPln.X, aPln.Y - 0.5 * ht + 0.25 * aScale), wd - 2, ht - 1.5 * aScale).LayerName = aLayerName;
                }


                int hcnt = !aPart.OpenEnded ? 1 : 2;

                double xleft = hcnt == 1 ? 0 : -0.5 * aPart.Width + aPart.TabInset + aPart.TabWidth / 2;
                double xright = hcnt == 1 ? 0 : 0.5 * aPart.Width - aPart.TabInset - aPart.TabWidth / 2;
                ht = aPart.HoleV.Radius * aScale;
                double t = thk / 2;
                _rVal.AdditionalSegments.AddPoint(aPln.Vector(xleft, 0), aLayerName: "DEFPOINTS", aTag: "APPAN_HOLE", aFlag: "LEFT");
                _rVal.AdditionalSegments.Add(aPln.CreateLine(xleft - ht, -t, xleft - ht, t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: "APPAN_HOLE"));
                _rVal.AdditionalSegments.Add(aPln.CreateLine(xleft + ht, -t, xleft + ht, t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: "APPAN_HOLE"));
                if (hcnt > 1)
                {
                    _rVal.AdditionalSegments.AddPoint(aPln.Vector(xright, 0), aLayerName: "DEFPOINTS", aTag: "APPAN_HOLE", aFlag: "RIGHT");
                    _rVal.AdditionalSegments.Add(aPln.CreateLine(xright - ht, -t, xright - ht, t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: "APPAN_HOLE"));
                    _rVal.AdditionalSegments.Add(aPln.CreateLine(xright + ht, -t, xright + ht, t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: "APPAN_HOLE"));

                }

            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }

        /// <summary>
        ///  ^returns a dxePolygon that is used to draw the end view of the shelf angle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon md_FingerClip_View_Elevation(mdFingerClip aPart, bool bSuppressFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {

            try
            {

                if (aPart == null) return null;



                double thk = 0;

                double cX = 0;

                double cY = 0;

                double wd = 0;

                uopHole aHl = null;

                dxfPlane aPln = null;

                dxfPlane bPln = null;

                dxfVector v1 = null;

                double f1 = 0;

                double d1 = 0;

                double d2 = 0;

                double rad2 = 0;

                double rad1 = 0;
                dxePolygon _rVal = new dxePolygon(); ;

                cX = aPart.X;
                cY = aPart.Z;

                thk = aPart.Thickness;
                rad2 = 2 * thk;
                rad1 = thk;
                wd = aPart.Width;
                aPln = xPlane(cX, cY, aCenter, aRotation);
                bPln = dxfPlane.World; ;
                bPln.SetCoordinates(aPart.X, aPart.Y, aPart.Z);


                if (aPart.Side == uppSides.Left)
                {
                    f1 = -1;
                }
                else
                {
                    f1 = 1;
                }
                aHl = aPart.GenHoles(null).Item(0).Item(0);
                v1 = aHl.CenterDXF.WithRespectToPlane(bPln);
                d2 = v1.Z;
                if (!bSuppressFillets)
                {
                    _rVal.Vertices.Add(aPln, 0, wd);
                    _rVal.Vertices.Add(aPln, 0, rad2);
                    _rVal.Vertices.Add(aPln, rad2 * f1, 0);
                    _rVal.Vertices.Add(aPln, wd * f1, 0);
                    _rVal.Vertices.Add(aPln, wd * f1, thk);
                    _rVal.Vertices.Add(aPln, (thk + rad1) * f1, thk, aVertexRadius: rad1 * -f1);
                    _rVal.Vertices.Add(aPln, thk * f1, thk + rad1);
                    _rVal.Vertices.Add(aPln, thk * f1, wd);
                }
                else
                {
                    _rVal.Vertices.Add(aPln, 0, wd);
                    _rVal.Vertices.Add(aPln, 0, 0);
                    _rVal.Vertices.Add(aPln, wd * f1, 0);
                    _rVal.Vertices.Add(aPln, wd * f1, thk);
                    _rVal.Vertices.Add(aPln, thk * f1, thk);
                    _rVal.Vertices.Add(aPln, thk * f1, wd);

                }


                _rVal.LayerName = aLayerName;
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
                d1 = aHl.Radius;
                _rVal.AdditionalSegments.Add((dxfEntity)aPln.CreateLine(0, d2 + d1, thk * f1, d2 + d1, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                _rVal.AdditionalSegments.Add((dxfEntity)aPln.CreateLine(0, d2 - d1, thk * f1, d2 - d1, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                return _rVal;
            }
            catch (Exception)
            {

                throw;

            }


        }

        /// <summary>
        ///returns a dxePolygon that is used to draw the end view of the shelf angle
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public static dxePolygon EndAngle_View_Elevation(mdEndAngle aPart, bool bSuppressFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {

            try
            {

                if (aPart == null) return null;

                double thk = aPart.Thickness;
                double cX = aPart.X;
                double cY = aPart.Z;
                double wd = aPart.Width;
                uopHole aHl = aPart.GenHoles().Item(1).Item(1);
                double f1 = (aPart.Direction == dxxOrthoDirections.Left) ? -1 : 1;
                double rad2 = 2 * thk;
                double rad1 = thk;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y, aPart.Z));
                dxfVector v1 = aHl.CenterDXF.WithRespectToPlane(bPln, bMaintainZ: true);

                double d1 = aHl.Radius;
                double d2 = v1.Z - cY;
                colDXFVectors verts = new colDXFVectors();

                if (!bSuppressFillets)
                {
                    verts.Add(aPln, 0, wd);
                    verts.Add(aPln, 0, rad2, aVertexRadius: rad2 * f1);
                    verts.Add(aPln, rad2 * f1, 0);
                    verts.Add(aPln, wd * f1, 0);
                    verts.Add(aPln, wd * f1, thk);
                    verts.Add(aPln, (thk + rad1) * f1, thk, aVertexRadius: rad1 * -f1);
                    verts.Add(aPln, thk * f1, thk + rad1);
                    verts.Add(aPln, thk * f1, wd);
                }
                else
                {
                    verts.Add(aPln, 0, wd);
                    verts.Add(aPln, 0, 0);
                    verts.Add(aPln, wd * f1, 0);
                    verts.Add(aPln, wd * f1, thk);
                    verts.Add(aPln, thk * f1, thk);
                    verts.Add(aPln, thk * f1, wd);

                }
                dxePolygon _rVal = new dxePolygon(verts, aInsertPt: aPln.Origin, bClosed: true, aPlane: aPln);

                _rVal.AdditionalSegments.Add(aPln.CreateLine(0, d2 + d1, thk * f1, d2 + d1, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
                _rVal.AdditionalSegments.Add(aPln.CreateLine(0, d2 - d1, thk * f1, d2 - d1, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));

                return _rVal;

            }
            catch (Exception)
            {

                throw;

            }

        }

        /// <summary>
        /// ^returns a dxePolygon that is used to draw the elevation view of the end plate
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aCenterLineLength"></param>
        /// <returns></returns>
        public static dxePolygon EndPlate_View_Elevation(mdEndPlate aPart, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", double aCenterLineLength = 0)
        {

            if (aPart == null) return null;
            dxePolygon _rVal;
            try
            {


                double thk = aPart.Thickness;
                double ht = aPart.Height;
                double ntch = aPart.NotchDim;
                double tht = aPart.TabHeight;

                double cX = aPart.X;
                double cY = aPart.Z;

                double wd = aPart.Width / 2;
                double y1 = tht;
                double y3 = y1 - ht;
                double y2 = y3 + ntch;
                double x1 = wd - ntch;
                colDXFVectors verts = new colDXFVectors();

                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);

                verts.Add(aPln, -wd + thk, y2, aTag: "THK");

                verts.Add(aPln, -wd + thk, y1);
                verts.Add(aPln, -wd, y1);
                verts.Add(aPln, -wd, y2);
                verts.Add(aPln, -x1, y2);
                verts.Add(aPln, -x1, y3);
                verts.Add(aPln, x1, y3);
                verts.Add(aPln, x1, y2);
                verts.Add(aPln, wd, y2);
                verts.Add(aPln, wd, y1);
                verts.Add(aPln, wd - thk, y1, aTag: "THK");
                verts.Add(aPln, wd - thk, y2);

                _rVal = new dxePolygon(verts, aPln.Origin, false, aPlane: aPln) { LayerName = aLayerName, };
                _rVal.AdditionalSegments.Add(aPln.CreateLine(-wd + thk, y1, wd - thk, y1, aLayerName: aLayerName));
                _rVal.AdditionalSegments.Add(aPln.CreateLine(-wd, y1 - tht, -wd + thk, y1 - tht, aLayerName: aLayerName, aTag: "TAB"));
                _rVal.AdditionalSegments.Add(aPln.CreateLine(wd - thk, y1 - tht, wd, y1 - tht, aLayerName: aLayerName, aTag: "TAB"));

                if (aPart.BoltOn)
                {
                    uopHoleArray aHls = aPart.GenHoles();
                    dxfPlane bPln = aPart.Plane;
                    dxfVector v1;
                    double t = thk / 2;
                    double c = aCenterLineLength / 2;
                    for (int i = 1; i <= aHls.Count; i++)
                    {
                        List<uopHole> bHls = aHls.Item(i).ToList;
                        foreach (uopHole hole in bHls)
                        {

                            v1 = hole.CenterDXF.WithRespectToPlane(bPln);
                            _rVal.AdditionalSegments.AddPlanarLine(aPln, v1.X - t, v1.Z + hole.Radius, v1.X + t, v1.Z + hole.Radius, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: hole.Tag, aFlag: hole.Flag);
                            _rVal.AdditionalSegments.AddPlanarLine(aPln, v1.X - t, v1.Z - hole.Radius, v1.X + t, v1.Z - hole.Radius, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: hole.Tag, aFlag: hole.Flag);
                            if (aCenterLineLength > 0)
                            {
                                _rVal.AdditionalSegments.AddPlanarLine(aPln, v1.X - c, v1.Z, v1.X + c, v1.Z, aLayerName, aLineType: "Center", aTag: hole.Tag, aFlag: hole.Flag);
                            }

                        }
                    }
                }




            }
            catch (Exception)
            {

                throw;
            }
            return _rVal;
        }
        /// <summary>
        ///returns a dxePolygon that is used to draw the elevation view of the end support
        /// </summary>
        /// <param name="aPart"></param>

        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aHoleClines"></param>
        /// <returns></returns>
        public static dxePolygon EndSupport_View_Elevation(mdEndSupport aPart, bool bSuppressFillets = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", string aHoleClines = "")
        {
            dxePolygon _rVal;
            try
            {
                if (aPart == null) return null;
                double thk = aPart.Thickness;
                double cX = aPart.X;
                double cY = aPart.Z;
                double wd = aPart.Width + 2 * thk;
                uopHoleArray aHls = aPart.GenHoles();
                uopHole aHl = null;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
                dxfPlane bPln = aPart.Plane;
                double rad = 0;
                double t = thk / 2;
                double ht = aPart.Height;
                dxfVector v1 = aPln.Vector(-0.5 * wd + t, ht);
                dxfVector v2 = aPln.Vector(-0.5 * wd + t, t);
                dxfVector v3 = aPln.Vector(0.5 * wd - t, t);
                dxfVector v4 = aPln.Vector(0.5 * wd - t, ht);
                colDXFVectors verts = dxfPrimatives.CreateVertices_Trace(new colDXFVectors(v1, v2, v3, v4), thk, false, aApplyFillets: !bSuppressFillets, aPlane: aPln);
                _rVal = new dxePolygon(verts, aPln.Origin, true, aPlane: aPln)
                {
                    LayerName = aLayerName
                };


                if (!bSuppressFillets)
                {
                    v1 = _rVal.Vertex(2, true);
                    v2 = v1.Moved(-thk);
                    _rVal.AdditionalSegments.AddLine(v1, v2, new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));

                    v1 = _rVal.Vertex(5, true);
                    v2 = v1.Moved(thk);
                    _rVal.AdditionalSegments.AddLine(v1, v2, new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden));
                }


                uopHoles Hls = aHls.Item("RING CLIP");

                for (int i = 1; i <= Hls.Count; i++)
                {
                    aHl = Hls.Item(i);
                    rad = aHl.Radius;
                    v1 = aHl.CenterDXF.WithRespectToPlane(bPln, bMaintainZ: true);
                    _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - rad, v1.Z - t, v1.X - rad, v1.Z + t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden), aTag: aHl.Tag);
                    _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X + rad, v1.Z - t, v1.X + rad, v1.Z + t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden), aTag: aHl.Tag);
                    if (mzUtils.ListContains(aHl.Tag, aHoleClines))
                    {
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X, v1.Z - 4 * thk, v1.X, v1.Z + 4 * thk, aLayerName: aLayerName, aLineType: "Center"), aTag: aHl.Tag);
                    }
                    v1 = aPln.Vector(v1.X, v1.Z);
                    dxePoint pt = _rVal.AdditionalSegments.AddPoint(v1.X, v1.Y, aLayerName);
                    pt.TFVSet(aHl.Tag, aHl.Flag, aHl.Radius);
                    pt.Factor = aHl.DownSet;
                }
                Hls = aHls.Item("END ANGLE");
                for (int i = 1; i <= Hls.Count; i++)
                {
                    aHl = Hls.Item(i);
                    v1 = aHl.CenterDXF.WithRespectToPlane(bPln, bMaintainZ: true);
                    rad = aHl.Radius;
                    _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - t, v1.Z + rad, v1.X + t, v1.Z + rad, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden), aTag: aHl.Tag);
                    _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - t, v1.Z - rad, v1.X + t, v1.Z - rad, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden), aTag: aHl.Tag);
                    if (mzUtils.ListContains(aHl.Tag, aHoleClines))
                    {
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.X - 4 * thk, v1.Z, v1.X + 4 * thk, v1.Z, aLayerName: aLayerName, aLineType: "Center"), aTag: aHl.Tag);
                    }
                    v1 = aPln.Vector(v1.X, v1.Z);
                    dxePoint pt = _rVal.AdditionalSegments.AddPoint(v1.X, v1.Y, aLayerName);
                    pt.TFVSet(aHl.Tag, aHl.Flag, aHl.Radius);
                    pt.Factor = aHl.DownSet;
                }

                //_rVal.AdditionalSegments.AddArc(_rVal.InsertionPt, 0.125, aColor: dxxColors.Red);
            }
            catch (Exception)
            {
                throw;
            }
            return _rVal;
        }

        private static double DimAt45(double aDim)
        {
            return Math.Sqrt((aDim * aDim) / 2);
        }

        /// <summary>
        /// Returns the polygon for the elevation view of the beam
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bSuppressFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static dxePolygon Beam_View_Elevation(
            mdBeam beam,
            bool bSuppressFillets = false,
            iVector aCenter = null,
            string aLayerName = "GEOMETRY",
            bool suppressRotation = true,
            bool showHoles = true,
            bool angledView = false,
            dxfDisplaySettings beamDisplaySettings = null)
        {
            dxePolygon elevationPolygon = new dxePolygon() { LayerName = aLayerName };

            if (beam == null) return elevationPolygon;

            beamDisplaySettings ??= dxfDisplaySettings.Null(aLayer: aLayerName, aLinetype: dxfLinetypes.Continuous);

            dxfPlane bPln = xPlane(beam.X, beam.Y, aCenter, suppressRotation ? 0 : beam.Rotation);

            try
            {

                double flngThickness = beam.FlangeThickness;
                double ht = beam.Height;
                double lng = beam.Length;

                double halfBeamHeight = ht / 2;
                double halfBeamLength = lng / 2;
                double wInset = beam.WebInset;
                double bWidth = beam.Width;
                double webThickness = beam.WebThickness;

                if (angledView)
                {
                    //adjust the dimensions for the angled view
                    halfBeamLength = DimAt45(beam.Length) / 2;
                    bWidth = DimAt45(bWidth);
                    wInset = (bWidth / 2) + DimAt45(wInset);
                    webThickness = DimAt45(webThickness);
                }

                // ****************************** Draw the beam box **********************************
                //double halfFlangeThickness = flngThickness / 2;
                double beamLeftWebInsetX = -halfBeamLength + wInset;
                double beamRightWebInsetX = halfBeamLength - wInset;
                double beamTopYIn = halfBeamHeight - flngThickness;
                double beamBottomYIn = -halfBeamHeight + flngThickness;

                //colDXFVectors topFlangeThicknessRectangle = new colDXFVectors();
                //topFlangeThicknessRectangle.Add(bPln, -halfBeamLength, halfBeamHeight); // Top left corner
                //topFlangeThicknessRectangle.Add(bPln, halfBeamLength, halfBeamHeight); // Top right corner
                //topFlangeThicknessRectangle.Add(bPln, halfBeamLength, halfBeamHeight - beam.FlangeThickness); // Bottom right corner
                //topFlangeThicknessRectangle.Add(bPln, -halfBeamLength, halfBeamHeight - flngThickness); // Bottom left corner

                //elevationPolygon.Vertices = topFlangeThicknessRectangle; // Top flange thickness rectangle
                //elevationPolygon.Closed = true;
                elevationPolygon.DisplaySettings = beamDisplaySettings;

                //// Bottom flange thickness rectangle
                //dxfVector centerOfBottomFlangeThickness = new dxfVector(0, -halfBeamHeight + halfFlangeThickness);
                //elevationPolygon.AdditionalSegments.AddRectangle(centerOfBottomFlangeThickness, halfBeamLength * 2,flngThickness);

                //elevationPolygon.AdditionalSegments.AddLine(beamLeftWebInsetX, beamTopYIn, beamLeftWebInsetX, beamBottomYIn); // Left vertical web inset line

                colDXFVectors verts = new colDXFVectors(bPln.Vector(-halfBeamLength, halfBeamHeight), bPln.Vector(halfBeamLength, halfBeamHeight));

                verts.AddRelative(0, -flngThickness, aPlane: bPln);
                verts.AddRelative(-wInset, 0, aPlane: bPln, aTag: "WEB", aFlag: "TOP RIGHT");
                verts.AddRelative(0, -(ht - 2 * flngThickness), aPlane: bPln, aTag: "WEB", aFlag: "BOTTOM RIGHT");
                verts.AddRelative(wInset, 0, aPlane: bPln);
                verts.AddRelative(0, -flngThickness, aPlane: bPln);
                verts.AddRelative(-(2 * halfBeamLength), 0, aPlane: bPln);
                verts.AddRelative(0, flngThickness, aPlane: bPln);
                verts.AddRelative(wInset, 0, aPlane: bPln, aTag: "WEB", aFlag: "BOTTOM LEFT");
                verts.AddRelative(0, (ht - 2 * flngThickness), aPlane: bPln, aTag: "WEB", aFlag: "TOP LEFT");
                verts.AddRelative(-wInset, 0, aPlane: bPln);
                elevationPolygon.Vertices = verts;

                dxeLine l1 = (dxeLine)elevationPolygon.AdditionalSegments.Add(new dxeLine(verts.Item(4), verts.Item(11)) {LayerName = aLayerName });
                dxeLine l2 = (dxeLine)elevationPolygon.AdditionalSegments.Add(new dxeLine(verts.Item(5), verts.Item(10)) { LayerName = aLayerName });

                if (angledView)
                {
                    //Show the web thickness visible on the left side of the beam
                    elevationPolygon.AdditionalSegments.AddLine(beamLeftWebInsetX - webThickness, beamTopYIn, beamLeftWebInsetX - webThickness, beamBottomYIn, aPlane:bPln); // Left vertical web inset line
                }

                elevationPolygon.AdditionalSegments.AddLine(beamRightWebInsetX, beamTopYIn, beamRightWebInsetX, beamBottomYIn,aPlane:bPln); // Right vertical web inset line

                if (angledView)
                {
                    //show the flange lines on the left side of the beam visible in the angled view
                    var vt = new dxfVector(verts.X(1) + bWidth, verts.Y(1), 0);
                    var vb = new dxfVector(vt.X, vt.Y - flngThickness, 0);
                    elevationPolygon.AdditionalSegments.AddLine(vt, vb, beamDisplaySettings);
                    vt.Move(aYChange: -(ht - flngThickness));
                    vb.Move(aYChange: -(ht - flngThickness));
                    elevationPolygon.AdditionalSegments.AddLine(vt, vb, beamDisplaySettings);

                    //show beam supports for the angled view only
                    double plateThk = 0.4785;
                    double legThk = 0.7087;
                    double footLength = 0.9843;
                    double width = DimAt45(beam.Width);
                    double height = 6.8116;
                    double shortHeight = 1.7544;
                    double edgeOffset = 0.75;
                    double overlap = beam.Width / 2;
                    overlap = DimAt45(overlap);
                    legThk = DimAt45(legThk);
                    edgeOffset = DimAt45(edgeOffset);
                    footLength = DimAt45(footLength);

                    double x1 = verts.X(1) - overlap;
                    double x2 = x1 + width;
                    double x3 = x2 + width;
                    double x4 = x3 - edgeOffset;
                    double y1 = l2.MidPt.Y - flngThickness/2;
                    double y2 = y1 - plateThk;
                    double y3 = y1 - height;
                    double lx1 = x1 + edgeOffset;
                    double lx2 = lx1 + legThk;
                    double lx3 = x2 - edgeOffset;
                    double lx4 = lx3 - legThk;
                    double fx1 = lx3 + footLength;
                    double fx2 = lx2 + footLength;
                    double ly = y1 - shortHeight;

                    //left top plate
                    elevationPolygon.AdditionalSegments.AddRectangle(x1, x2, y1, y2);
                    //Right top plate
                    elevationPolygon.AdditionalSegments.AddRectangle(x2, x3, y1, y2);
                    //left leg end
                    elevationPolygon.AdditionalSegments.AddRectangle(lx1, lx2, y2, y3);
                    //right leg end
                    elevationPolygon.AdditionalSegments.AddRectangle(lx4, lx3, y2, y3);

                    //left foot
                    elevationPolygon.AdditionalSegments.AddLine(lx2, y3, fx2, y3);
                    //right foot
                    elevationPolygon.AdditionalSegments.AddLine(lx3, y3, fx1, y3);

                    //right leg short end
                    elevationPolygon.AdditionalSegments.AddLine(x4, y2, x4, ly);

                    //right leg diagonal
                    var ln = elevationPolygon.AdditionalSegments.AddLine(fx1, y3, x4, ly);

                    var ln2 = ln.Clone();
                    ln2.MoveTo(new dxfVector(fx2, y3));
                    ln2.TrimBetweenLines(new dxeLine(new dxfVector(fx2, y3), new dxfVector(fx2, y2)), new dxeLine(new dxfVector(lx4, y3), new dxfVector(lx4, y2)));
                    elevationPolygon.AdditionalSegments.Add(ln2);

                    //invert points for use on the right side of the beam
                    //left top plate
                    elevationPolygon.AdditionalSegments.AddRectangle(-x1, -x2, y1, y2);
                    //Right top plate
                    elevationPolygon.AdditionalSegments.AddRectangle(-x2, -x3, y1, y2);
                    //left leg end
                    elevationPolygon.AdditionalSegments.AddLine(-lx1, y2, -lx1, y3);
                    //right leg end
                    var lLegLine = elevationPolygon.AdditionalSegments.AddLine(-lx4, y2, -lx4, y3);

                    //left foot
                    var lfoot = elevationPolygon.AdditionalSegments.AddLine(-lx1, y3, -fx2, y3);
                    //right foot
                    elevationPolygon.AdditionalSegments.AddLine(-lx4, y3, -fx1, y3);

                    //left leg short end
                    elevationPolygon.AdditionalSegments.AddRectangle(-x4, -(x4 - legThk), y2, ly);
                    //right leg short end
                    double x5 = -x2 - edgeOffset - legThk;
                    elevationPolygon.AdditionalSegments.AddRectangle(x5, x5 + legThk, y2, ly);

                    //left leg diagonal
                    elevationPolygon.AdditionalSegments.AddLine(-x4, ly, -fx1, y3);
                    elevationPolygon.AdditionalSegments.AddLine(-(x4 - legThk), ly, -(fx1 - legThk), y3);
                    //right leg diagonal
                    var tedge = elevationPolygon.AdditionalSegments.AddLine(x5, ly, -fx2, y3);
                    elevationPolygon.AdditionalSegments.AddLine(x5 + legThk, ly, -(fx2 - legThk), y3);

                    lLegLine.TrimBetweenLines(lfoot, tedge);
                    elevationPolygon.AdditionalSegments.CopyDisplayValues(beamDisplaySettings);
                }

                if (showHoles)
                {
                    // ****************************** Draw the web openings ******************************
                    uopHole opening = beam.WebOpening;
                    opening.ExtrusionDirection = "0,0,1";
                    double cornerRadius = beam.WebOpeningCornerRadius; // In inches
                    IEnumerable<dxfVector> webOpeningCenters = FindWebOpeningCenters(beam.WebOpeningSize, beam.WebOpeningCount, beam.WebOpeningSize);
                    dxePolyline webOpeningPolyline = new dxePolyline(opening.DXFVertices, true, aPlane:bPln ) { Tag = "WEB OPENING", LayerName = aLayerName };

                    foreach (var webOpeningCenter in webOpeningCenters)
                    {
                        dxePolyline pl = new dxePolyline(webOpeningPolyline);
                        pl.Translate(webOpeningCenter);
                        elevationPolygon.AdditionalSegments.Add(pl);
                        //webOpeningPolyline = new dxePolyline( webOpeningPolylineVertices, true, webOpeningDisplaySetting ) { Tag = "WEB OPENING"};
                        //elevationPolygon.AdditionalSegments.AddPolyline( webOpeningPolyline ) ;
                        elevationPolygon.AdditionalSegments.Add(new dxePoint(webOpeningCenter ) { Tag = "WEB OPENING", LayerName = aLayerName });
                    }

                    // ****************************** Draw holes *****************************************
                    var beamHoles = beam.GenHoles("BEAM").Item(1);
                    uopHole slot = beamHoles.Member;
                    double holeLength = slot.Length; // This is the whole length of the hole
                    double halfHoleLength = holeLength / 2;

                    double leftHoleCenterXInPlane = beamHoles.Centers.First(c => c.Flag.Contains("LEFT")).ToDXFVector().WithRespectToPlane(beam.Plane).X;
                    double rightHoleCenterXInPlane = beamHoles.Centers.First(c => c.Flag.Contains("RIGHT")).ToDXFVector().WithRespectToPlane(beam.Plane).X;

                    double verticalLineX, verticalLineStartY;

                    // Draw holes side lines
                    dxfDisplaySettings holeVerticalSideLineDisplaySetting = dxfDisplaySettings.Null(aLayer: aLayerName, aLinetype: dxfLinetypes.Hidden);

                    verticalLineStartY = -halfBeamHeight + flngThickness;

                    verticalLineX = leftHoleCenterXInPlane - halfHoleLength;
                    elevationPolygon.AdditionalSegments.AddLine(verticalLineX, verticalLineStartY, verticalLineX, -halfBeamHeight, holeVerticalSideLineDisplaySetting, aTag: slot.Tag); // Left side line of the left hole

                    verticalLineX = leftHoleCenterXInPlane + halfHoleLength;
                    elevationPolygon.AdditionalSegments.AddLine(verticalLineX, verticalLineStartY, verticalLineX, -halfBeamHeight, holeVerticalSideLineDisplaySetting, aTag: slot.Tag); // Right side line of the left hole

                    verticalLineX = rightHoleCenterXInPlane - halfHoleLength;
                    elevationPolygon.AdditionalSegments.AddLine(verticalLineX, verticalLineStartY, verticalLineX, -halfBeamHeight, holeVerticalSideLineDisplaySetting, aTag: slot.Tag); // Left side line of the right hole

                    verticalLineX = rightHoleCenterXInPlane + halfHoleLength;
                    elevationPolygon.AdditionalSegments.AddLine(verticalLineX, verticalLineStartY, verticalLineX, -halfBeamHeight, holeVerticalSideLineDisplaySetting, aTag: slot.Tag); // Right side line of the right hole



                    // Draw hole center lines
                    dxfDisplaySettings holeVerticalCenterLineDisplaySetting = dxfDisplaySettings.Null(aLayer: aLayerName, aColor: dxxColors.Red, aLinetype: "Center");
                    double extrusion = 0.3937; // Inch

                    verticalLineStartY = -halfBeamHeight + flngThickness/2 + extrusion;
                    elevationPolygon.AdditionalSegments.AddLine(leftHoleCenterXInPlane, verticalLineStartY, leftHoleCenterXInPlane, -halfBeamHeight - extrusion, holeVerticalCenterLineDisplaySetting); // Left hole center line

                    elevationPolygon.AdditionalSegments.AddLine(rightHoleCenterXInPlane, verticalLineStartY, rightHoleCenterXInPlane, -halfBeamHeight - extrusion, holeVerticalCenterLineDisplaySetting); // Right hole center line
                }

                return elevationPolygon;
            }
            catch
            {
                return elevationPolygon;
            }
            finally
            {
                elevationPolygon.Plane = bPln;
                elevationPolygon.InsertionPt = bPln.Origin;
            }
        }

        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the stiffener
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="bIncludeFillets"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="rPlane"></param>
        /// <returns></returns>
        public static colDXFVectors Stiffener_Vertices(mdStiffener aPart, bool bIncludeFillets, iVector aCenter, double aRotation, out dxfPlane rPlane)
        {
            colDXFVectors _rVal = new colDXFVectors();
            rPlane = null;

            try
            {

                if (aPart == null) return _rVal;
                double thk = aPart.Thickness;
                double wd = aPart.Width;
                double fwd = aPart.FlangeWidth;
                double W = wd / 2;
                double y1 = -0.5;
                double y2 = y1 + fwd;
                double x1 = -W;
                double cX = 0;
                double cY = 0;
                double cZ = 0;
                double d1 = fwd - thk;
                double d2 = wd - 2 * thk;
                double d3 = 0;
                double bafthk = aPart.BaffleThickness;
                double rad1 = thk;
                double rad2 = 2 * thk;
                dxfVector v1 = null;

                aPart.CenterDXF.GetComponents(ref cX, ref cY, ref cZ);
                rPlane = xPlane(cX, cY, aCenter, aRotation);



                if (aPart.SupportsBaffle)
                {
                    d2 = W - 2 * thk - 0.5 * bafthk;
                    d3 = W - thk + 0.5 * bafthk;
                    _rVal.Add(rPlane, -0.75, y2);
                    _rVal.Add(rPlane, -0.5 * bafthk, y2);
                    _rVal.Add(rPlane, -0.5 * bafthk, y1);
                    _rVal.AddRelative(-thk, aPlane: rPlane);
                    _rVal.AddRelative(0, d1, aPlane: rPlane);
                    _rVal.AddRelative(-d2, aPlane: rPlane);
                    _rVal.AddRelative(0, -d1, aPlane: rPlane);
                    _rVal.AddRelative(-thk, aPlane: rPlane);
                    _rVal.AddRelative(0, fwd, aPlane: rPlane);
                    _rVal.AddRelative(wd, aPlane: rPlane);
                    _rVal.AddRelative(0, -fwd, aPlane: rPlane);
                    _rVal.AddRelative(-thk, aPlane: rPlane);
                    _rVal.AddRelative(0, d1, aPlane: rPlane);
                    _rVal.AddRelative(-d3, aPlane: rPlane);
                    if (bIncludeFillets)
                    {
                        v1 = _rVal.Item(2).Clone();
                        _rVal.Item(2).Project(rPlane.XDirection, -rad2);
                        _rVal.Item(2).VertexRadius = rad2;
                        v1.Project(rPlane.YDirection, -rad2);
                        _rVal.Item(2).Inverted = true;
                        _rVal.Add(v1, aAfterIndex: 2);

                        v1 = _rVal.Item(6).Clone();
                        _rVal.Item(6).Project(rPlane.YDirection, -rad1);
                        _rVal.Item(6).VertexRadius = rad1;
                        v1.Project(rPlane.XDirection, -rad1);
                        _rVal.Add(v1, aAfterIndex: 6);

                        v1 = _rVal.Item(8).Clone();
                        _rVal.Item(8).Project(rPlane.XDirection, rad1);
                        _rVal.Item(8).VertexRadius = rad1;
                        v1.Project(rPlane.YDirection, -rad1);
                        _rVal.Add(v1, aAfterIndex: 8);

                        v1 = _rVal.Item(12).Clone();
                        _rVal.Item(12).Project(rPlane.YDirection, -rad2);
                        _rVal.Item(12).VertexRadius = rad2;
                        v1.Project(rPlane.XDirection, rad2);
                        _rVal.Item(12).Inverted = true;
                        _rVal.Add(v1, aAfterIndex: 12);


                        v1 = _rVal.Item(14).Clone();
                        _rVal.Item(14).Project(rPlane.XDirection, -rad2);
                        _rVal.Item(14).VertexRadius = rad2;
                        v1.Project(rPlane.YDirection, -rad2);
                        _rVal.Item(14).Inverted = true;
                        _rVal.Add(v1, aAfterIndex: 14);

                        v1 = _rVal.Item(18).Clone();
                        _rVal.Item(18).Project(rPlane.YDirection, -rad1);
                        _rVal.Item(18).VertexRadius = rad1;
                        v1.Project(rPlane.XDirection, -rad1);
                        _rVal.Add(v1, aAfterIndex: 18);

                        _rVal.LastVector().Project(rPlane.XDirection, -0.03);
                    }

                }
                else
                {
                    _rVal.Add(rPlane, x1, y1);
                    _rVal.AddRelative(0, fwd, aPlane: rPlane);
                    _rVal.AddRelative(wd, aPlane: rPlane);
                    _rVal.AddRelative(0, -fwd, aPlane: rPlane);
                    _rVal.AddRelative(-thk, 0, aPlane: rPlane);
                    _rVal.AddRelative(0, d1, aPlane: rPlane);
                    _rVal.AddRelative(-d2, aPlane: rPlane);
                    _rVal.AddRelative(0, -d1, aPlane: rPlane);
                    _rVal.Add(rPlane, x1, y1);
                    if (bIncludeFillets)
                    {
                        v1 = _rVal.Item(2).Clone();
                        _rVal.Item(2).Project(rPlane.YDirection, -rad2);
                        _rVal.Item(2).VertexRadius = rad2;
                        v1.Project(rPlane.XDirection, rad2);
                        _rVal.Item(2).Inverted = true;
                        _rVal.Add(v1, aAfterIndex: 2);

                        v1 = _rVal.Item(4).Clone();
                        _rVal.Item(4).Project(rPlane.XDirection, -rad2);
                        _rVal.Item(4).VertexRadius = rad2;
                        v1.Project(rPlane.YDirection, -rad2);
                        _rVal.Item(4).Inverted = true;
                        _rVal.Add(v1, aAfterIndex: 4);

                        v1 = _rVal.Item(8).Clone();
                        _rVal.Item(8).Project(rPlane.YDirection, -rad1);
                        _rVal.Item(8).VertexRadius = rad1;
                        v1.Project(rPlane.XDirection, -rad1);
                        _rVal.Add(v1, aAfterIndex: 8);

                        v1 = _rVal.Item(10).Clone();
                        _rVal.Item(10).Project(rPlane.XDirection, rad1);
                        _rVal.Item(10).VertexRadius = rad1;
                        v1.Project(rPlane.YDirection, -rad1);
                        _rVal.Add(v1, aAfterIndex: 10);
                    }

                }



                //if (cX < 0)
                //{
                //    _rVal.MirrorPlanar(0);
                //    _rVal.MirrorPlanar(cY);
                //}

            }
            catch (Exception e)
            {

                throw e;
            }
            return _rVal;
        }

        /// <summary>
        ///the parent tray assembly for this part
        ///the parent downcomer for this part
        ///returns a dxePolygon that is used to draw the right view of the end plate
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aPlanView"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public static dxePolygon EndSupport_View_LayoutRight(mdEndSupport aPart, dxePolygon aPlanView = null, bool bSuppressHoles = false, iVector aCenter = null, double aRotation = 0)
        {

            if (aPart == null) return null;

            aPlanView ??= EndSupport_View_Plan(aPart, bSuppressHoles, 0, aCenter, aRotation);
            if (aPlanView.Vertices.Count <= 0) return null;

            dxfVector v1 = aPlanView.GetVertex("TOP_RIGHT", bReturnClone: true);
            dxfVector v2 = aPlanView.GetVertex(dxxPointFilters.AtMinY);
            dxfVector v3 = aPlanView.GetVertex("TAB1");
            dxfVector v4 = aPlanView.GetVertex("NOTCH1");

            double ht = aPart.WeirHeight;
            double thk = aPart.Thickness;

            dxfPlane aPln = aPlanView.Plane;

            colDXFVectors verts = new colDXFVectors(v1.Clone(), v1.Moved(ht, 0), new dxfVector(v1.X + ht, v2.Y));

            dxfVector v0 = verts.AddRelative(-thk);
            dxfVector vn = verts.Add(v0.X, v4.Y);
            v0 = verts.AddRelative(-thk);
            verts.Add(v0.X, v3.Y);
            verts.Add(v1.X, v3.Y);
            dxePolygon _rVal = new dxePolygon(verts, v1, true, aPlane: aPlanView.Plane);

            _rVal.AdditionalSegments.AddLine(vn.X, vn.Y, vn.X + thk, vn.Y);
            _rVal.AdditionalSegments.AddLine(vn.X, vn.Y, vn.X, v1.Y, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden));

            if (!aPart.IsSquare)
            {
                v3 = new dxfVector(v1.X + ht - thk, v1.Y + 4 * thk);
                v4 = new dxfVector(v1.X + ht, v1.Y + 5 * thk);
                _rVal.AdditionalSegments.AddLine(v3.X, v1.Y, v3.X, v3.Y);
                _rVal.AdditionalSegments.AddLine(v4.X, v1.Y, v4.X, v4.Y);

                verts = new colDXFVectors(v3.Moved(-thk, thk), v3.Clone(), v4.Clone(), v4.Moved(thk, -thk));
                _rVal.AdditionalSegments.Add(new dxePolyline(verts, false, aPlane: aPlanView.Plane));
            }


            if (!bSuppressHoles)
            {
                dxePoint aPt = (dxePoint)aPlanView.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Point).GetTagged("END ANGLE", "RIGHT");


                if (aPt != null)
                {
                    v2 = aPt.Center.Clone();

                    v2.X = v1.X + ht - aPart.DeckThickness - aPt.Factor;
                    _rVal.AdditionalSegments.AddArc(v2, (double)aPt.Value).TFVCopy(aPt);
                }
            }




            return _rVal;
        }

        /// <summary>
        /// #1the parent tray assembly for this part
        ///#2the parent downcomer for this part
        ///returns a dxePolygon that is used to draw the left view of the end plate
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aPlanView"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public static dxePolygon EndSupport_View_LayoutLeft(mdEndSupport aPart, dxePolygon aPlanView = null, bool bSuppressHoles = false, iVector aCenter = null, double aRotation = 0)
        {


            if (aPart == null) return null;

            double ht = aPart.WeirHeight;
            double thk = aPart.Thickness;
            bool bTria = aPart.HasTriangularEndPlate;


            aPlanView ??= EndSupport_View_Plan(aPart, bSuppressHoles, 0, aCenter, aRotation);
            if (aPlanView.Vertices.Count <= 0) return null;



            dxfVector v1 = aPlanView.GetVertex("TOP_LEFT", bReturnClone: true);
            dxfVector v2 = aPlanView.GetVertex(dxxPointFilters.AtMinY, bReturnClone: true);

            dxfVector v3 = aPlanView.GetVertex("TAB2");
            dxfVector v4 = aPlanView.GetVertex("NOTCH2");
            if (bTria) v2.Y = v3.Y - 0.75;

            colDXFVectors verts = new colDXFVectors(v1)
            {
                { v1.X - ht, v1.Y }
            };
            dxfVector v5 = verts.Add(v1.X - ht, v2.Y);
            dxfVector v6 = !bTria ? verts.AddRelative(thk) : verts.AddRelative(thk, thk);
            dxfVector vn = verts.Add(v6.X, v4.Y);
            dxfVector v0 = verts.AddRelative(thk);
            verts.Add(v0.X, v3.Y);
            verts.Add(v1.X, v3.Y);

            dxePolygon _rVal = new dxePolygon(verts, v1, true);

            _rVal.AdditionalSegments.AddLine(vn.X, vn.Y, vn.X - thk, vn.Y);
            _rVal.AdditionalSegments.AddLine(vn.X, vn.Y, vn.X, v1.Y, dxfDisplaySettings.Null(aLinetype: dxfLinetypes.Hidden));

            if (bTria)
            {

                verts = new colDXFVectors(v5.Moved(-thk, thk), v5.Clone(), v6.Clone(), v6.Moved(thk, -thk));

                _rVal.AdditionalSegments.Add(new dxePolyline(verts, false));
            }

            if (!bSuppressHoles)
            {
                dxePoint aPt = (dxePoint)aPlanView.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Point).GetTagged("END ANGLE", "LEFT");

                if (aPt != null)
                {
                    v2 = aPt.Center.Clone();
                    v2.X = v1.X - ht + aPart.DeckThickness + aPt.Factor;
                    _rVal.AdditionalSegments.AddArc(v2, (double)aPt.Value).TFVCopy(aPt);
                }
            }
            return _rVal;
        }

        /// <summary>
        /// #1the parent tray assembly for this part
        ///#2the parent downcomer for this part
        ///returns a dxePolygon that is used to draw the left view of the end plate
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aPlanView"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <returns></returns>
        public static dxePolygon EndSupport_View_Layout(mdEndSupport aPart, dxePolygon aPlanView, bool bRightSide, bool bSuppressHoles = false, iVector aCenter = null, double aRotation = 0)
        {


            if (aPart == null) return null;

            double ht = aPart.WeirHeight;
            double thk = aPart.Thickness;
            bool bTria = aPart.HasTriangularEndPlate;
            double cX = aPart.X;
            double cY = aPart.Y;
            dxfVector passedcenter = new dxfVector(aCenter) ?? null;
            colDXFEntities addsegs = new colDXFEntities();
            double f1 = !bRightSide ? -1 : 1;

            aPlanView ??= EndSupport_View_Plan(aPart, bSuppressHoles, 0, aCenter, aRotation);
            if (aPlanView.Vertices.Count <= 0) return null;
            dxfDisplaySettings dsp = aPlanView.DisplaySettings;

            dxfRectangle bounds = aPlanView.Vertices.BoundingRectangle(dxfPlane.World);
            dxfPlane plane = new dxfPlane(aPlanView.InsertionPt);
            dxfDirection xdir = plane.XDirection.Inverse();

            dxeLine l1 = !bRightSide ? new dxeLine(bounds.TopRight, bounds.BottomRight) : new dxeLine(bounds.TopLeft, bounds.BottomLeft);
            l1.Translate(plane.XDirection * ht * -f1);

            List<dxeLine> segs = aPlanView.Segments.Lines();
            dxeLine limline = segs.Find(x => string.Compare(x.Tag, "LIM_LINE", true) == 0);
            dxeLine notchline = bRightSide ? segs.Find(x => x.X > bounds.X && x.Tag.ToUpper().StartsWith("NOTCH")) : segs.Find(x => x.X < bounds.X && x.Tag.ToUpper().StartsWith("NOTCH"));
            dxxPointFilters fltr = bRightSide ? dxxPointFilters.GetRightTop : dxxPointFilters.GetLeftTop;
            dxfVector v1 = aPlanView.GetVertex(fltr, aPrecis: 1).ProjectedToLine(l1);
            fltr = bRightSide ? dxxPointFilters.GetRightBottom : dxxPointFilters.GetLeftBottom;

            dxfVector v2 = v1 + xdir * ht * f1;
            dxfVector v3 = aPlanView.GetVertex(fltr, aPrecis: 1).ProjectedToLine(l1);
            dxfVector v4 = v3 + xdir * (2 * thk * f1);
            v3 += xdir * ht * f1;
            dxfVector v6 = notchline.StartPt.ProjectedToLine(l1);
            dxfVector v5 = v6 + xdir * (2 * thk * f1);

            dxfVector v0 = aPlanView.InsertionPt.ProjectedToLine(l1);

            plane.Origin = v0;

            v4.Tag = "SLOT_POINT";
            colDXFVectors verts = new colDXFVectors(new dxfVector(v1), new dxfVector(v2), new dxfVector(v3), new dxfVector(v4), new dxfVector(v5), new dxfVector(v6));

            dxePolygon _rVal = new dxePolygon(verts, plane.Origin, true, aPlane: plane);
            dxfRectangle tabbounds = verts.BoundingRectangle(plane);


            v6 += xdir * thk * f1;
            v1 += xdir * thk * f1;

            addsegs.AddLine(v1, v6, new dxfDisplaySettings(aLinetype: dxfLinetypes.Hidden));

            colDXFVectors limendpts = new colDXFVectors(limline.EndPoints().ProjectedToLine(l1));
            v2 = limendpts.GetVector(dxxPointFilters.AtMinY);
            double d1;
            bool breakit;
            if (v2.Y < v6.Y)
            {
                v3 = v2 + xdir * thk * f1;
                v4 = _rVal.Vertices.LastVector(true);
                d1 = v6.DistanceTo(v3);
                breakit = d1 > tabbounds.Height;
                if (breakit)
                {
                    d1 -= 0.35 * tabbounds.Height;
                    v3 += plane.YDirection * (d1 + thk);
                    v2 += plane.YDirection * d1;

                    addsegs.Add(new dxeLine(v6, v3, aDisplaySettings: dsp));
                    addsegs.Add(new dxeLine(v2, v4, aDisplaySettings: dsp));

                    dxfDirection dir = bRightSide ? plane.AngularDirection(45) : plane.AngularDirection(135);
                    addsegs.Add(new dxePolyline(new colDXFVectors(v2 + dir * thk, v2, v3, v3 + dir * -thk), false, aDisplaySettings: dsp) { Tag = "BREAK_LINE", Linetype = dxfLinetypes.Phantom });
                    // addsegs.Add(new dxePolyline(tabbounds.Corners(), true, aDisplaySettings: dsp));

                }
                else
                {
                    verts = new colDXFVectors(v6.Clone(), v3.Clone(), v2.Clone(), v4.Clone());
                    addsegs.Add(new dxePolyline(verts, false, aDisplaySettings: dsp));

                }

            }


            v2 = bounds.TopLeft.ProjectedToLine(l1);
            if (v2.Y > v1.Y + 0.1)
            {
                d1 = 0.75;
                v3 = v1 + plane.YDirection * (d1 - thk);
                v4 = _rVal.Vertex(1, true);
                v2 = v4 + plane.YDirection * d1;

                addsegs.Add(new dxeLine(v1, v3, aDisplaySettings: dsp));
                addsegs.Add(new dxeLine(v2, v4, aDisplaySettings: dsp));
                dxfDirection dir = bRightSide ? plane.AngularDirection(315) : plane.AngularDirection(225);
                addsegs.Add(new dxePolyline(new colDXFVectors(v2 + dir * thk, v2, v3, v3 + dir * -thk), false, aDisplaySettings: dsp) { Tag = "BREAK_LINE", Linetype = dxfLinetypes.Phantom });



            }


            // addsegs.AddLine(l1.StartPt, l1.EndPt, aColor: dxxColors.Magenta);
            if (!bSuppressHoles)
            {


                List<dxePoint> eanglepts = aPlanView.AdditionalSegments.Points().FindAll(x => string.Compare(x.Tag, "END ANGLE", true) == 0);
                eanglepts = bRightSide ? eanglepts.FindAll(x => x.X > bounds.X) : eanglepts.FindAll(x => x.X < bounds.X);
                if (eanglepts.Count > 0)
                {
                    UHOLE hole = aPart.EndAngleHoleU;
                    List<dxeLine> eanglelines = aPlanView.AdditionalSegments.Lines().FindAll(x => string.Compare(x.Linetype, dxfLinetypes.Hidden, true) == 0);
                    v1 = eanglepts[0].Center.ProjectedToLine(l1);
                    v1 += xdir * hole.Elevation * f1;
                    dxeArc arc = new dxeArc(v1, hole.Radius, aDisplaySettings: dsp);
                    addsegs.Add(arc);


                }




            }
            _rVal.AdditionalSegments.Populate(addsegs, false);
            _rVal.DisplaySettings = dsp;
            // addsegs.AddArc(v0, 0.1, aColor: dxxColors.Magenta);


            if (passedcenter != null)
            {
                _rVal.MoveTo(passedcenter);
            }

            return _rVal;
        }

        public static dxePolygon DeckSection_View_Plan(mdDeckSection aPart, mdTrayAssembly aAssy, bool bObscured = false, bool bIncludePromoters = false, bool bIncludeHoles = false, bool bIncludeSlotting = false, bool bRegeneratePerimeter = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY", bool bHalfSideSlots = false, bool bRegenSlots = false, bool bSolidHoles = false, bool bIgnoreParent = false, bool bInvert = false)
        {
            //returns a dxePolygon that is used to draw the layout view of the deck section


            if (aPart == null) return null;

            aAssy ??= aPart.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return null;
            dxePolygon _rVal = null;



            try
            {
                dxePolygon perim = aPart.GeneratePerimeter(aAssy, bRegeneratePerimeter);


                //if (aPart.AlternateRingType == uppAlternateRingTypes.AtlernateRing2 && !bIgnoreParent)
                //{
                //    mdDeckSection parent = aPart.Parent;
                //    if(parent != null)
                //    {
                //        aCenter ??= parent.Center.Mirrored(null, parent.PanelV.Y).ToDXFVector();
                //        _rVal = mdPolygons.DeckSection_View_Plan(parent, aAssy, bObscured, bIncludePromoters, bIncludeHoles, bIncludeSlotting, bRegeneratePerimeter, aCenter, aRotation , aLayerName, bHalfSideSlots, bRegenSlots, bSolidHoles, true, true);
                //       // _rVal.AdditionalSegments.RotateAbout(_rVal.Plane.ZAxis(100, _rVal.InsertionPt), -180);
                //        // _rVal.RotateAbout(_rVal.Plane.ZAxis(100, _rVal.InsertionPt), -180);
                //        return _rVal;

                //    }


                //}

                _rVal = perim.Clone();
                dxfVector v1;

                if (aCenter != null)
                {
                    v1 = new dxfVector(aCenter.X - _rVal.X, aCenter.Y - _rVal.Y);
                    _rVal.Translate(v1);
                }

                double cX = aPart.X;
                double cY = aPart.Y;
                dxfPlane aPln = xPlane(_rVal.X, _rVal.Y, aCenter);
                dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y, aPart.Z));
                dxeLine mirrorAxis = bInvert ? _rVal.Plane.XAxis(100) : null;
                if (aPart.Inverted)
                {

                    //_rVal.Vertices.MirrorAboutLine(mirrorAxis);
                    //_rVal.AdditionalSegments.MirrorAboutLine(mirrorAxis);
                    // _rVal.Vertices.RotateAbout(_rVal.Plane.ZAxis(100,_rVal.InsertionPt),-180);
                    //_rVal.AdditionalSegments.RotateAbout(_rVal.Plane.ZAxis(100, _rVal.InsertionPt), -180);
                    //aPln.Tip(180, bLocal: true);
                }
                _rVal.Tag = aPart.Handle;
                _rVal.LayerName = aLayerName;
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;


                colDXFEntities addsegs = _rVal.AdditionalSegments;
                  colDXFEntities promoters = addsegs.RemoveByTag("BUBBLE PROMOTER");

                if (!bObscured)
                {
                    _rVal.Vertices.RemoveAll(x => string.Compare(x.Tag, "OBSCURED", true) == 0); // .GetByTag("OBSCURED", bRemove: true);
                    _rVal.Vertices.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.ByLayer);

                    addsegs.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.ByLayer, dxfLinetypes.Hidden);
                    addsegs.GetByTag("THICKNESS", bReturnInverse: true).SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.ByLayer, aTagList: "THICKNESS");
                    addsegs.GetByTag("THICKNESS").SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Hidden);
                }

                if (bIncludePromoters && aAssy.DesignOptions.HasBubblePromoters && promoters.Count > 0)
                {
                    double bpdrad = mdGlobals.BPRadius;
                    colDXFVectors BPs = aPart.BPSites.ToDXFVectors();
                    foreach (dxfVector bpcenter in BPs)
                    {
                        v1 = bpcenter.WithRespectToPlane(bPln, aPln);
                        if (bInvert) v1.Mirror(mirrorAxis);
                        _rVal.AddAdditionalSegment(new dxeArc(v1, bpdrad, aDisplaySettings: dxfDisplaySettings.Null(aLayer: "BUBBLE PROMOTERS")) { Value = 0 }, false, false, aTag: "BUBBLE PROMOTER");
                    }

                }

                if (bIncludeHoles)
                {

                    uopHoleArray aHls = aPart.GenHoles(aAssy);
                    foreach (uopHoles holes in aHls)
                    {
                        for (int i = 1; i <= holes.Count; i++)
                        {
                            uopHole aHl = holes.Item(i);
                            v1 = aHl.CenterDXF.WithRespectToPlane(bPln, aPln);
                            if (bInvert) v1.Mirror(mirrorAxis);
                            aHl.SetCoordinates(v1.X, v1.Y);
                            dxfEntity hbound = aHl.BoundingEntity(aLayerName);
                            if(holes.Name == "BOLT")
                            {
                                hbound.Linetype = dxfLinetypes.Hidden;
                            }

                            if (aHl.HoleType == uppHoleTypes.Hole)
                            {
                                hbound.Value = (double)uppHoleTypes.Hole;



                            }
                            else
                            {
                                hbound.Value = (double)uppHoleTypes.Slot;
                                if (bObscured) hbound.Linetype = bObscured && !bSolidHoles ? dxfLinetypes.Hidden : dxfLinetypes.Continuous;
                            }
                            _rVal.AddAdditionalSegment(hbound);
                        }
                    }


                }

                if (bIncludeSlotting && aAssy.IsECMD)
                {
                    uopVectors slPts = aPart.SlotCenters(aAssy, out mdSlotZone aZone, bRegenSlots: bRegenSlots, bReturnClones: bInvert, bInvert: bInvert);

                    if (slPts.Count > 0)
                    {
                        dxePolyline aPl = aAssy.FlowSlot.FootPrint(true, true, false, new dxfDisplaySettings("SLOTS", dxxColors.ByLayer, dxfLinetypes.Continuous));
                        aPl.Plane = aPln;


                        double rot;
                        dxeLine aAxis = bPln.ZAxis(10, aPln.Origin);

                        for (int i = 1; i <= slPts.Count; i++)
                        {
                            uopVector u1 = slPts.Item(i);
                            rot = 0;
                            if (bInvert && u1.Value != 0 && u1.Value != 180)
                            {
                                if (u1.Y > cY)
                                {
                                    rot = u1.X < cX ? 90 : -90;
                                }
                                else
                                {
                                    rot = u1.X < cX ? -90 : 90;
                                }

                            }


                            v1 = u1.ToDXFVector(bValueAsRotation: true);

                            dxfVector v2 = v1.WithRespectToPlane(bPln, aPln);

                            if (!bHalfSideSlots || (bHalfSideSlots && v1.X < bPln.X))
                            {
                                dxePolyline bPl = aPl.Clone();
                                bPl.RotateAbout(aAxis, u1.Value + rot);
                                bPl.Translate(v2.X, v2.Y);
                                _rVal.AddAdditionalSegment(bPl, false, true);

                            }

                        }
                    }



                }
                if (aRotation != 0)
                {
                    _rVal.RotateAbout(_rVal.Plane.ZAxis(), aRotation);
                }

                _rVal.BlockName = $"DECKSECTION_{aPart.PanelIndex}_{aPart.SectionIndex}_PLAN";

            }
            catch { }
            finally
            {


            }


            return _rVal;
        }

        public static dxePolygon DeckSection_View_Elevation(
            mdDeckSection aPart,
            mdTrayAssembly aAssy,
            ref bool rJoggleAngleIncluded,
            bool bIncludePromoters = false,
            bool bSupporessHoles = false,
            bool bIncludeBolts = false,
            iVector aCenter = null,
            double aRotation = 0,
            string aLayerName = "GEOMETRY")
        {

            if (aPart == null) return null;
            aAssy ??= aPart.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return null;
            if (aPart.BaseVertices.Count <= 0) return null;
            dxePolygon perim = aPart.GeneratePerimeter(aAssy, true);

            dxfRectangle aRec = perim.BoundingRectangle();
            uopDeckSplice tSplice;
            uopDeckSplice bSplice;
            dxfVector v1;
            uopHoleArray aHls;
            uopHoles Hls;
            uopHoles RCHoles;
            uopHole Hl;
            dxfDirection aDir;
            dxePolyline aPl;
            hdwHexBolt aBlt;
            colDXFVectors bltPts = null;

            uppSpliceIndicators tSpliceType = aPart.TopSpliceType;
            uppSpliceIndicators bSpliceType = aPart.BottomSpliceType;
            double d1;
            double d2;
            double joght = 0;
            int i;
            int j;
            double cX = aPart.X;
            double cY = aPart.Z;
            double thk = aPart.Thickness;
            double wd = aRec.Width;
            bool bFlng = false;
            bool tFlng = false;
            uopLine flnLn = null;
            double flng;
            string xVals = string.Empty;
            dxfPlane bPln = aPart.Plane;
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);

            dxePolygon _rVal = new dxePolygon()
            {
                LayerName = aLayerName,
                Plane = aPln,
                InsertionPt = aPln.Origin,
                Tag = aPart.Handle,
                Closed = true
            };


            if (aAssy.DesignOptions.SpliceStyle != uppSpliceStyles.Angle)
            {
                tSplice = aPart.TopSplice;
                bSplice = aPart.BottomSplice;

                if (tSplice != null) tSpliceType = tSplice.SpliceIndicator;
                if (bSplice != null) bSpliceType = bSplice.SpliceIndicator;

                if (bSpliceType == uppSpliceIndicators.JoggleMale || bSpliceType == uppSpliceIndicators.TabFemale)
                {
                    flnLn = bSplice.FlangeLine;
                    bFlng = true;
                }

                if (tSpliceType == uppSpliceIndicators.JoggleMale || tSpliceType == uppSpliceIndicators.TabFemale)
                {
                    flnLn = tSplice.FlangeLine;
                    tFlng = true;
                }
            }
            colDXFVectors verts = new colDXFVectors(aPln.Vector(-wd / 2, thk / 2), aPln.Vector(wd / 2, thk / 2));


            if (aPart.PanelIndex > 1)
            {
                flng = flnLn!= null ? Math.Abs(flnLn.ep.X - flnLn.sp.X): 0;
                _rVal.Vertices = dxfPrimatives.CreateVertices_Trace(verts, thk, false, true, false);

                if ((bFlng || tFlng) && flng > 0)
                {
                    d1 = flnLn.sp.X - bPln.X;
                    d2 = flnLn.ep.X - bPln.X;
                    aPl = new dxePolyline(new colDXFVectors(aPln.Vector(d1, thk), aPln.Vector(d1, -(joght - thk)), aPln.Vector(d2, -(joght - thk)), aPln.Vector(d2, thk)), false, aPlane: aPln) { LayerName = aLayerName };
                    _rVal.AdditionalSegments.Add(aPl, aTag: "JOGGLE ANGLE");
                    rJoggleAngleIncluded = true;
                }
            }
            else
            {
                aDir = (aAssy.DesignOptions.SpliceStyle != uppSpliceStyles.Tabs) ? aPln.YDirection : aPln.YDirection.Inverse();

                joght = aAssy.DesignOptions.JoggleAngle;
                switch (bSpliceType)
                {
                    case uppSpliceIndicators.TabMale:
                        xJoggleIt(verts, aDir, true, thk, 0.76);
                        break;
                    case uppSpliceIndicators.TabFemale:
                        xJoggleIt(verts, aDir, true, thk, 1.28, joght);
                        break;
                    case uppSpliceIndicators.JoggleFemale:
                        xJoggleIt(verts, aDir, true, thk, 1.375);
                        break;
                    case uppSpliceIndicators.JoggleMale:
                        xJoggleIt(verts, aDir.Inverse(), true, thk, 0, joght);
                        break;
                }
                switch (tSpliceType)
                {
                    case uppSpliceIndicators.TabMale:
                        xJoggleIt(verts, aDir, false, thk, 0.76);
                        break;
                    case uppSpliceIndicators.TabFemale:
                        xJoggleIt(verts, aDir, false, thk, 1.28, joght);
                        break;
                    case uppSpliceIndicators.JoggleFemale:
                        xJoggleIt(verts, aDir, false, thk, 1.375);
                        break;
                    case uppSpliceIndicators.JoggleMale:
                        xJoggleIt(verts, aDir.Inverse(), false, thk, 0, joght);
                        break;
                }
                _rVal.Vertices = dxfPrimatives.CreateVertices_Trace(verts, thk, aClosed: false, aApplyFillets: true, aFilletEnds: false);
            }

            if (!bSupporessHoles)
            {
                var addSegs = _rVal.AdditionalSegments;
                aHls = aPart.GenHoles(aAssy);
                if (aPart.PanelIndex == 1)
                    RCHoles = aHls.Item("ring clip", true);
                else
                    RCHoles = new uopHoles();

                for (j = 1; j <= aHls.Count; j++)
                {
                    Hls = aHls.Item(j);
                    xVals = string.Empty;
                    for (i = 1; i <= Hls.Count; i++)
                    {
                        Hl = Hls.Item(i);
                        v1 = Hl.CenterDXF.WithRespectToPlane(aPln, bPln);
                        if (Hl.Y >= bPln.Y)
                        {
                            if (!mzUtils.ListContains(Math.Round(Hl.X, 2), xVals))
                            {
                                mzUtils.ListAdd(ref xVals, Math.Round(Hl.X, 2));
                                d1 = Math.Round(Hl.Rotation, 1);
                                if (d1 == 0 || d1 == 180)
                                    d1 = Hl.Length / 2;
                                else
                                    d1 = Hl.Radius;

                                addSegs.Add(aPln.CreateLine(v1.X - d1, v1.Z + thk / 2, v1.X - d1, Hl.Z - thk / 2, 0, aLayerName, dxxColors.Undefined, dxfLinetypes.Hidden, Hls.Name));
                                addSegs.Add(aPln.CreateLine(v1.X + d1, v1.Z + thk / 2, v1.X + d1, Hl.Z - thk / 2, 0, aLayerName, dxxColors.Undefined, dxfLinetypes.Hidden, Hls.Name));
                                if (bIncludeBolts && Hl.Tag == "BOLT")
                                {
                                    if (bltPts == null) bltPts = new colDXFVectors();
                                    bltPts.Add(aPln, v1.X, v1.Z - thk / 2);
                                }
                            }
                        }
                    }
                }

                if (RCHoles.Count > 0)
                {
                    if (tSpliceType == uppSpliceIndicators.ToRing)
                    {
                        Hl = RCHoles.GetByPoint(dxxPointFilters.AtMaxX);
                        v1 = Hl.CenterDXF.WithRespectToPlane(bPln, aPln);
                        if (!mzUtils.ListContains(Math.Round(v1.X, 2), xVals))
                        {
                            mzUtils.ListAdd(ref xVals, Math.Round(v1.X, 2));
                            d1 = Hl.Radius;
                            addSegs.Add(aPln.CreateLine(v1.X - d1, v1.Z + thk / 2, v1.X - d1, v1.Z - thk / 2, 0, aLayerName, dxxColors.Undefined, dxfLinetypes.Hidden, RCHoles.Name));
                            addSegs.Add(aPln.CreateLine(v1.X + d1, v1.Z + thk / 2, v1.X + d1, v1.Z - thk / 2, 0, aLayerName, dxxColors.Undefined, dxfLinetypes.Hidden, RCHoles.Name));
                        }
                    }
                }

                if (bltPts != null)
                {
                    if (bltPts.Count > 0)
                    {
                        aBlt = aPart.JoggleBolt;
                        for (i = 1; i <= bltPts.Count; i++)
                        {
                            addSegs.Append(aBlt.View_Profile(1, bltPts.Item(i), aPln, dxxOrthoDirections.Up, bIncludeCenterLine: true, aLayerName: aLayerName).SubEntities(false), false, "WELDED BOLT");
                        }
                    }
                }

                if (bIncludePromoters && aAssy.DesignOptions.HasBubblePromoters)
                {
                    xVals = string.Empty;
                    UVECTORS bPts;
                    UVECTOR u1;

                    bPts = aPart.BPSites;
                    if (bPts.Count > 0)
                    {
                        v1 = dxfVector.Zero;
                        for (i = 1; i <= bPts.Count; i++)
                        {
                            u1 = bPts.Item(i);
                            if (!mzUtils.ListContains(Math.Round(u1.X, 2), xVals))
                            {
                                mzUtils.ListAdd(ref xVals, Math.Round(u1.X, 2));
                                v1.SetCoordinates(u1.X, u1.Y);
                                v1 = v1.WithRespectToPlane(bPln, aPln);
                                addSegs.Add(aPln.CreateArc(v1.X, -(1.3756 - thk), 1.8756, 47.174, 132.823, aLayerName: aLayerName));
                            }
                        }
                    }
                }
            }
            return _rVal;
        }


        public static dxePolygon DeckSection_View_Profile(mdDeckSection aPart, mdTrayAssembly aAssy, bool bLongSide = false, bool bIncludePromoters = false,
                                                            bool bSupporessHoles = false, bool bIncludeBolts = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            dxePolygon _rVal = null;

            if (aPart == null) return null;
            aAssy ??= aPart.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return null;
            if (aPart.BaseVertices.Count <= 0) return null;

            try
            {

                double f1 = !bLongSide ? -1 : 1;
                double thk = aPart.Thickness;
                dxfRectangle aRec = aPart.Perimeter.BoundingRectangle();
                double cX = aPart.Y;
                double cY = aPart.Z;
                int pid = aPart.PanelIndex;
                dxfPlane bPln = aPart.Plane;
                int sID = aPart.SectionIndex;
                uopHoleArray aHls = !bSupporessHoles ? aPart.GenHoles(aAssy) : null;
                uopDeckSplice tSplice = aPart.TopSplice;
                uopDeckSplice bSplice = aPart.BottomSplice;
                uppSpliceIndicators tSpliceType = tSplice == null ? uppSpliceIndicators.ToRing : tSplice.SpliceIndicator;
                uppSpliceIndicators bSpliceType = bSplice == null ? uppSpliceIndicators.ToRing : bSplice.SpliceIndicator;
                dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
                double x1 = -aRec.Height / 2;
                double x2 = aRec.Height / 2;
                double jht = aAssy.DesignOptions.JoggleAngle;
                dxfDirection aDir = aAssy.DesignOptions.SpliceStyle == uppSpliceStyles.Joggle ? aPln.YDirection : aPln.YDirection.Inverse();
                colDXFVectors bltPts = null;
                colDXFVectors verts = new colDXFVectors(aPln.Vector(x1 * f1, thk / 2), aPln.Vector(x2 * f1, thk / 2));
                List<double> xvals = new List<double>();

                if (pid > 1)
                {
                    switch (bSpliceType)
                    {
                        case uppSpliceIndicators.TabMale:
                            {
                                xJoggleIt(verts, aDir, false, thk, 0.76);
                                break;
                            }
                        case uppSpliceIndicators.TabFemale:
                            {
                                xJoggleIt(verts, aDir, false, thk, 1.28, jht);
                                break;
                            }
                        case uppSpliceIndicators.JoggleFemale:
                            {
                                xJoggleIt(verts, aDir.Inverse(), false, 0, 1.375);
                                break;
                            }
                        case uppSpliceIndicators.JoggleMale:
                            {
                                xJoggleIt(verts, aDir, false, thk, 0, jht);
                                break;
                            }
                    }
                    switch (tSpliceType)
                    {
                        case uppSpliceIndicators.TabFemale:
                            {
                                xJoggleIt(verts, aDir, true, thk, 1.28, jht);
                                break;
                            }
                        case uppSpliceIndicators.TabMale:
                            {
                                xJoggleIt(verts, aDir, true, thk, 0.76);
                                break;
                            }
                        case uppSpliceIndicators.JoggleFemale:
                            {
                                xJoggleIt(verts, aDir.Inverse(), true, 0, 1.375);
                                break;
                            }
                        case uppSpliceIndicators.JoggleMale:
                            {
                                xJoggleIt(verts, aDir, true, thk, 0, jht);
                                break;
                            }
                    }
                }
                else
                {
                    verts.Add(aPln, x1 * f1, thk / 2);
                    verts.Add(aPln, x2 * f1, thk / 2);
                }

                verts = dxfPrimatives.CreateVertices_Trace(verts, thk, false, true, false, 0);
                var maxYVertex = aPart.Perimeter.Vertices.GetVector(dxxPointFilters.AtMaxY);
                var maxYVertexInBPlane = maxYVertex.WithRespectToPlane(bPln);

                _rVal = new dxePolygon(verts, aPln.Origin, true, aPlane: aPln) { LayerName = aLayerName };
                if (!bSupporessHoles)
                {
                    uopHoles RCHoles = (pid == 1) ? aHls.Item("RING CLIP", true) : new uopHoles();
                    uopHole Hl;
                    double t = thk / 2;
                    xvals = new List<double>();
                    double d1;

                    for (int j = 1; j <= aHls.Count; j++)
                    {
                        uopHoles Hls = aHls.Item(j);

                        for (int i = 1; i <= Hls.Count; i++)
                        {
                            Hl = Hls.Item(i);
                            dxfVector v1 = Hl.CenterDXF.WithRespectToPlane(bPln, bMaintainZ: true);
                            if (!xvals.Contains(Math.Round(v1.Y, 2)))
                            {
                                xvals.Add(Math.Round(v1.Y, 2));
                                d1 = Math.Round(Hl.Rotation, 1);
                                d1 = (d1 != 0 && d1 != 180) ? Hl.Length / 2 : Hl.Radius;
                                _rVal.AdditionalSegments.Add(aPln.CreateLine((x2 - maxYVertexInBPlane.Y + v1.Y) * -f1 - d1, v1.Z + t, (x2 - maxYVertexInBPlane.Y + v1.Y) * -f1 - d1, v1.Z - t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: Hls.Name));
                                _rVal.AdditionalSegments.Add(aPln.CreateLine((x2 - maxYVertexInBPlane.Y + v1.Y) * -f1 + d1, v1.Z + t, (x2 - maxYVertexInBPlane.Y + v1.Y) * -f1 + d1, v1.Z - t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: Hls.Name));

                                if (bIncludeBolts && Hl.Tag == "BOLT")
                                {
                                    if (bltPts == null) bltPts = new colDXFVectors();
                                    bltPts.Add(aPln, (x2 - maxYVertexInBPlane.Y + v1.Y) * -f1, v1.Z - t);
                                }
                            }
                        }
                    }

                    if (RCHoles.Count > 0)
                    {
                        if (tSpliceType == uppSpliceIndicators.ToRing)
                        {
                            Hl = RCHoles.GetByPoint(dxxPointFilters.AtMaxX);
                            dxfVector v1 = Hl.CenterDXF.WithRespectToPlane(bPln);
                            if (!xvals.Contains(Math.Round(v1.X, 2)))
                            {
                                xvals.Add(Math.Round(v1.X, 2));
                                d1 = Hl.Radius;
                                _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.Y * -f1 - d1, v1.Z + t, v1.Y * -f1 - d1, v1.Z - t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: RCHoles.Name));
                                _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.Y * -f1 + d1, v1.Z + t, v1.Y * -f1 + d1, v1.Z - t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: RCHoles.Name));


                            }
                        }
                        if (bSpliceType == uppSpliceIndicators.ToRing)
                        {
                            Hl = RCHoles.GetByPoint(dxxPointFilters.AtMinX);
                            dxfVector v1 = Hl.CenterDXF.WithRespectToPlane(bPln);
                            if (!xvals.Contains(Math.Round(v1.X, 2)))
                            {
                                xvals.Add(Math.Round(v1.X, 2));
                                d1 = Hl.Radius;
                                _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.Y * -f1 - d1, v1.Z + t, v1.Y * -f1 - d1, v1.Z - t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: RCHoles.Name));
                                _rVal.AdditionalSegments.Add(aPln.CreateLine(v1.Y * -f1 + d1, v1.Z + t, v1.Y * -f1 + d1, v1.Z - t, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden, aTag: RCHoles.Name));


                            }
                        }

                        if (bltPts != null)
                        {
                            if (bltPts.Count > 0)
                            {
                                hdwHexBolt bolt = aPart.JoggleBolt;
                                for (int i = 1; i <= bltPts.Count; i++)
                                {
                                    _rVal.AdditionalSegments.Append(bolt.View_Profile(1, bltPts.Item(i), aPln, dxxOrthoDirections.Up, bIncludeCenterLine: true, aLayerName: aLayerName).SubEntities(false), aTag: "WELDED BOLT");
                                }
                            }
                        }
                    }



                }
                if (bIncludePromoters && aAssy.DesignOptions.HasBubblePromoters)
                {
                    xvals = new List<double>();
                    UVECTORS bPts = aPart.BPSites;
                    if (bPts.Count > 0)
                    {
                        for (int i = 1; i <= bPts.Count; i++)
                        {
                            UVECTOR u1 = bPts.Item(i);
                            if (!xvals.Contains(Math.Round(u1.X, 2)))
                            {
                                xvals.Add(Math.Round(u1.X, 2));
                                dxfVector v1 = new dxfVector(u1.X, u1.Y);
                                v1 = v1.WithRespectToPlane(bPln);
                                _rVal.AdditionalSegments.Add(aPln.CreateArc(v1.X, -(1.3756 - thk), 1.8756, 47.174, 132.823, aLayerName: aLayerName));
                            }
                        }
                    }

                }
            }
            catch { }


            return _rVal;
        }

        private static void xJoggleIt(colDXFVectors aVectors, dxfDirection aYDirection, bool bFirst, double aThickness, double aInset, double aAngleHeight = 0, double aBendAdder = 0)
        {
            aInset = Math.Abs(aInset);
            if (aThickness == 0) return;

            dxfDirection xDir;
            dxfDirection yDir;
            dxfVector v1;
            dxfVector v2;
            dxfVector v3;
            double d1 = 0;
            double d2;
            double t;
            double bw;
            double ang1;
            double ang2;

            if (bFirst)
            {
                v1 = aVectors.Item(1);
                v2 = aVectors.Item(2);
                if (v1 == null || v2 == null) return;
            }
            else
            {
                v1 = aVectors.Item(aVectors.Count);
                v2 = aVectors.Item(aVectors.Count - 1);
                if (v1 == null || v2 == null) return;
            }

            xDir = v1.DirectionTo(v2, ref d1);
            if (d1 <= 0 || d1 <= aInset) return;
            if (aYDirection == null)
            {
                d2 = 1;
                if (bFirst) d2 = -1;
                yDir = xDir.Rotated(90 * d2, false, dxfPlane.World);
            }
            else
            {
                yDir = aYDirection;
            }

            t = 0.5 * Math.Abs(aThickness);

            if (aInset != 0)
            {
                ang1 = Math.PI / 4;
                ang2 = ang1 / 2;
                d2 = 2 * t;
                if (aBendAdder != 0)
                {
                    d2 += Math.Abs(aBendAdder);
                    bw = ang1 - Math.Atan(2 * t / d2);
                    ang1 -= bw;
                    ang2 += bw;
                }
                d1 = t * Math.Tan(ang2);
                bw = d1 + d2;

                v3 = v1.Clone();
                v1.Project(yDir, 2 * t);
                v2 = v1.Projected(xDir, aInset + d1);
                v3.Project(xDir, aInset + bw);
                if (bFirst)
                {
                    aVectors.Add(v2, aAfterIndex: 1);
                    aVectors.Add(v3, aAfterIndex: 2);
                }
                else
                {
                    aVectors.Add(v3, aVectors.Count);
                    aVectors.Add(v2, aVectors.Count);
                }
            }

            if (aAngleHeight != 0)
            {
                v1.Project(xDir, t);
                v2 = v1.Clone();
                v2.Project(yDir, Math.Abs(aAngleHeight) - t);
                if (bFirst)
                    aVectors.Add(v2, 1);
                else
                    aVectors.Add(v2);
            }
        }

        public static IEnumerable<dxfVector> FindWebOpeningCenters(double webOpeningSize, int numberOfWebOpenings, double webOpeningGap)
        {
            double lastLeftCenterX, lastRightCenterX;
            double centersOffset = webOpeningSize + webOpeningGap;

            bool odd = numberOfWebOpenings % 2 == 1;
            int halfNumberOfWebOpenings = numberOfWebOpenings / 2;

            List<dxfVector> centers = new List<dxfVector>();

            if (odd)
            {
                centers.Add(new dxfVector(0, 0));
                lastLeftCenterX = -centersOffset;
                lastRightCenterX = centersOffset;
            }
            else
            {
                lastLeftCenterX = -centersOffset / 2;
                lastRightCenterX = centersOffset / 2;
            }

            for (int i = 0; i < halfNumberOfWebOpenings; i++)
            {
                centers.Add(new dxfVector(lastLeftCenterX, 0));
                centers.Add(new dxfVector(lastRightCenterX, 0));
                lastLeftCenterX -= centersOffset;
                lastRightCenterX += centersOffset;
            }

            centers = centers.OrderBy(c => c.X).ToList(); // Make sure the centers are from left to right

            return centers;
        }

    }
}


