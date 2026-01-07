using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.DXFGraphics.Utilities;

namespace UOP.WinTray.Projects.Generators
{
    public static class uopPolygons
    {
        public static dxePolygon RingClip_View_Plan(uopRingClip aPart, bool bShowBolt = false, bool bShowBottom = false, bool bIncludeLengthCL = false, bool bIncludeWidthCL = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            //^returns a dxePolygon that is used to draw the top view of the ring clip

            if (aPart == null) return new dxePolygon();


            double thk = aPart.Thickness;

            dxeHole aHl = aPart.BoltHole;
            double leng = aPart.Length;
            double wd = aPart.Width;
            double nib = aPart.NibWidth;
            double cX = aPart.X;
            double cY = aPart.Y;
            double iset = aHl.Inset;
            double d1 = wd - (2 * nib) - (2 * thk);
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);


            colDXFVectors verts = new colDXFVectors();

            verts.Add(aPln, -iset - nib, 0.5 * wd - thk - nib, aVertexRadius: nib);
            verts.AddRelative(nib, nib, aPlane: aPln);
            verts.AddRelative(0, thk, aPlane: aPln);
            verts.AddRelative(leng, 0, aPlane: aPln);
            verts.AddRelative(0, -thk, aPlane: aPln, aVertexRadius: nib);
            verts.AddRelative(nib, -nib, aPlane: aPln);
            verts.AddRelative(0, -d1, aPlane: aPln, aVertexRadius: nib);
            verts.AddRelative(-nib, -nib, aPlane: aPln);
            verts.AddRelative(0, -thk, aPlane: aPln);
            verts.AddRelative(-leng, 0, aPlane: aPln);
            verts.AddRelative(0, thk, aPlane: aPln, aVertexRadius: nib);
            verts.AddRelative(-nib, nib, aPlane: aPln);

            dxePolygon _rVal = new dxePolygon(verts, aPln.Origin) { LayerName = aLayerName, Plane = aPln };
            //'create the polygon
            dxeLine aLn = new dxeLine(verts.Item(2), verts.Item(5)) { LayerName = aLayerName };
            if (!bShowBottom) aLn.Linetype = dxfLinetypes.Hidden;
            _rVal.AdditionalSegments.Add(aLn);
            aLn = new dxeLine(verts.Item(8), verts.Item(1)) { LayerName = aLayerName };

            _rVal.AdditionalSegments.Add(aLn);
            if (bIncludeLengthCL)
            {
                _rVal.AdditionalSegments.Add(aPln.CreateLine(-iset - 2 * nib, 0, leng - iset + 2 * nib, aLayerName: aLayerName, aLineType: "Center", aTag: "CENTERLINE_LENGTH"));
            }
            if (bIncludeWidthCL)
            {
                _rVal.AdditionalSegments.Add(aPln.CreateLine(0, 0.5 * wd + nib, 0, -0.5 * wd - nib, aLayerName: aLayerName, aLineType: "Center", aTag: "CENTERLINE_LENGTH"));
            }

            return _rVal;

        }

        public static dxePolygon RingClip_View_Profile(uopRingClip aPart, bool bShowBolt = false, bool bShowThreads = true, bool bIncludeBoltCenterLine = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            //^returns a dxePolygon that is used to draw the side view of the ring clip
            if (aPart == null) return new dxePolygon();


            double cX = aPart.Y;
            double cY = aPart.Z;
            double thk = aPart.Thickness;
            dxeHole aHl = aPart.BoltHole;

            dxfVector v1 = dxfVector.Zero;

           

            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
            //dxeLine aLn As dxeLine
            //double d1 As Single
            if (!bShowBolt) bShowThreads = false;
            colDXFVectors verts = new colDXFVectors();
           

            if ( aPart.Size == Enums.uppRingClipSizes.ThreeInchRC) {

                verts.Add(aPln, 2.125);
                verts.Add(aPln, 2.25);
                verts.Add(aPln, 2.25, thk);
                verts.Add(aPln, 2.125, thk);
                verts.Add(aPln, 2.125, 0.625, aVertexRadius: 1.25);
                verts.Add(aPln, 1.7321, 0.5616, aVertexRadius: -1.906);
                verts.Add(aPln, 0.0713, 0.7881, aVertexRadius: 1.25);
                verts.Add(aPln, -0.625, 1);
                verts.Add(aPln, -0.625, thk);
                verts.Add(aPln, -0.75, thk);
                verts.Add(aPln, -0.75, 0);
                verts.Add(aPln, -0.625, 0);

            }
             else
            {
            verts.Add(aPln, 3, 0);
                verts.Add(aPln, 3.125, 0);
                verts.Add(aPln, 3.125, thk);
                verts.Add(aPln, 3, thk);
                verts.Add(aPln, 3, 0.75, aVertexRadius: 1.5);
                verts.Add(aPln, 2.4997, 0.6641, aVertexRadius: -3);
                verts.Add(aPln, -0.167, 0.9974, aVertexRadius: 1.5);
                verts.Add(aPln, -1, 1.25);
                verts.Add(aPln, -1, thk);
                verts.Add(aPln, -1.125, thk);
                verts.Add(aPln, -1.125, 0);
                verts.Add(aPln, -1, 0);


            }
            dxePolygon _rVal = new dxePolygon(verts, aPln.Origin,true) { LayerName = aLayerName, Plane = aPln };
            _rVal.AdditionalSegments.AddLine(verts.Item(4), verts.Item(9), new dxfDisplaySettings(aLayerName,aLinetype:dxfLinetypes.Hidden));
            _rVal.AdditionalSegments.AddLine(verts.Item(4), verts.Item(1), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.ByLayer));
            _rVal.AdditionalSegments.AddLine(verts.Item(9), verts.Item(12), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.ByLayer));

            if (bShowBolt)
            {
                hdwHexBolt aBlt = aPart.Bolt;
                aBlt.ObscuredLength = (aPart.Size == Enums.uppRingClipSizes.FourInchRC) ? 1.015 : 0.9233;
                dxePolygon aPG = aBlt.View_Profile( 1, aPln.Origin, aPln, dxxOrthoDirections.Up, bShowThreads, false, false, bIncludeBoltCenterLine, aLayerName);
                _rVal.AdditionalSegments.Append(aPG.SubEntities());
            }
            return _rVal;

        }

        public static dxePolygon RingClip_View_Elevation(uopRingClip aPart , bool bShowBolt = false, bool bShowThreads = true, bool bIncludeBoltCenterLine = false, iVector aCenter = null, double aRotation  = 0, string aLayerName = "GEOMETRY")
        {

            //^returns a dxePolygon that is used to draw the elevation view of the ring clip

            dxePolygon _rVal = new dxePolygon();
            if (aPart == null) return _rVal;

            double thk = aPart.Thickness;
            double wd = aPart.Width;
            double cX = aPart.X;
            double cY = aPart.Z;
            double h1 = aPart.BigHeight;
            double h2 = aPart.SmallHeight;
            double r1 = 1.5 * thk;
            double r2 = 0.5 * thk;
            double d1 = h1 - h2;
            double d2 = wd - (2 * thk) - (2 * r2);
            double d3 = wd - (2 * r1);

            dxeHole aHl = aPart.BoltHole;
            dxfPlane aPln = xPlane(cX, cY, aCenter, aRotation);
            dxfPlane bPln = new dxfPlane(new dxfVector(aPart.X, aPart.Y, aPart.Z));
            
            if (!bShowBolt) bShowThreads = false;
            aHl.Center = aHl.Center.WithRespectToPlane(bPln);

            colDXFVectors verts = new colDXFVectors
            {
                { aPln, -0.5 * wd, h1 }
            };
            verts.AddRelative(thk,0,aPlane:aPln);
            verts.AddRelative(0, -d1, aPlane: aPln, aTag:"SHORT1");
            verts.AddRelative(0, -(h2 - r2 - thk), aPlane: aPln,aVertexRadius:r2);
            verts.AddRelative(r2, -r2, aPlane: aPln);
            verts.AddRelative(d2, 0, aPlane: aPln,aVertexRadius:r2);
            verts.AddRelative(r2, r2, aPlane: aPln);
            verts.AddRelative(0, h2 - r2 - thk, aPlane: aPln, aTag: "SHORT2");
            verts.AddRelative(0, d1, aPlane: aPln, aVertexRadius: r2);
            verts.AddRelative(thk, 0, aPlane: aPln);
            verts.AddRelative(0, -d1, aPlane: aPln, aTag: "SHORT2");
            verts.AddRelative(0, -(h2 - r1), aPlane: aPln, aVertexRadius: -r1);
            verts.AddRelative(-r1, -r1, aPlane: aPln);
            verts.AddRelative(-d3, 0, aPlane: aPln, aVertexRadius: -r1); 
            verts.AddRelative(-r1, r1, aPlane: aPln);
            verts.AddRelative(0, h2 - r1, aPlane: aPln, aTag: "SHORT1");

            _rVal.Vertices = verts;
            _rVal.AdditionalSegments.Add(new dxeLine(verts.Item(3), verts.LastVector()) { LayerName = aLayerName });
            _rVal.AdditionalSegments.Add(new dxeLine(verts.Item(8), verts.Item(11)) { LayerName = aLayerName });
            r1 = aHl.Radius;
            _rVal.AdditionalSegments.Add(aPln.CreateLine(aHl.X - r1, aHl.Z + thk / 2, aHl.X - r1, aHl.Z - thk / 2,aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
            _rVal.AdditionalSegments.Add(aPln.CreateLine(aHl.X + r1, aHl.Z + thk / 2, aHl.X + r1, aHl.Z - thk / 2, aLayerName: aLayerName, aLineType: dxfLinetypes.Hidden));
            if (bShowBolt)
            {
                dxePolygon bPGon = aPart.Bolt.View_Profile( 1, aPln.Origin, aPln, aDirection: dxxOrthoDirections.Up, bShowThreads: bShowThreads, bIncludeCenterLine: bIncludeBoltCenterLine, aLayerName: aLayerName);
                _rVal.AdditionalSegments.Append(bPGon.SubEntities());
            }

            return _rVal;
        }


        internal static dxePolyline HexNutProfile(THARDWARE aStructure, iVector aInsertionPt = null, dxxOrthoDirections aDirection = dxxOrthoDirections.Up, 
            bool bTwoFaces = false, string aLinetype = "", string aLayerName = "GEOMETRY",  dxfPlane aPlane = null, double aScaleFactor = 1)
        {

            dxfPlane plane = new dxfPlane(aPlane);
            plane.Origin = new dxfVector(aInsertionPt, plane);
            if(aDirection == dxxOrthoDirections.Down)
            {
                plane.Rotate(180);
            }
            else if(aDirection == dxxOrthoDirections.Left)
            {
                plane.Rotate(90);
            }
            else if (aDirection == dxxOrthoDirections.Right)
            {
                plane.Rotate(-90);
            }

            dxePolyline aPl = dxfPrimatives.CreateHexHeadProfile(aStructure.H, aStructure.G, aStructure.f, !bTwoFaces, plane);
            aPl.LCLSet(aLayerName: aLayerName, aLineType: aLinetype);
           
            return aPl;

         
        }

        public static dxePolyline HexNutPlan(hdwHexNut aNut, dxfVector aInsertionPt = null, double aRotation = 0,
         string aLinetype = "", string aLayerName = "GEOMETRY", dxfPlane aPlane = null, double aScaleFactor = 1)
        {
            if (aNut == null) return null;
            return HexNutPlan(aNut.HardwareStructure, aInsertionPt, aRotation,  aLinetype, aLayerName, aPlane, aScaleFactor);
        }

        internal static dxePolyline HexNutPlan(THARDWARE aStructure, dxfVector aInsertionPt = null, double aRotation = 0,
          string aLinetype = "", string aLayerName = "GEOMETRY", dxfPlane aPlane = null, double aScaleFactor = 1)
        {
            
        
            return dxfPrimatives.CreateHexHeadPlan(aInsertionPt, aStructure.f * aScaleFactor, aRotation, aPlane, new dxfDisplaySettings(aLayerName, aLinetype: aLinetype));
            
       
        }

        public static dxePolygon CreateAngle(iVector aTopCenter, double aHeight, double aWidth, double aThickness, double aBendRadius = 0, bool bReturnPolyline = false)
        {
            dxePolygon _rVal = null;
            aHeight = Math.Abs(aHeight);
            aWidth = Math.Abs(aWidth);
            aThickness = Math.Abs(aThickness);
            aBendRadius = Math.Abs(aBendRadius);
            try
            {
                if (aHeight == 0) throw new Exception("[uopPolygons.CreateAngle] The passed Height is Invalid");
                if (aWidth == 0) throw new Exception("[uopPolygons.CreateAngle] The passed Width is Invalid");
                if (aThickness == 0) throw new Exception("[uopPolygons.CreateAngle] The passed Thickness is Invalid");

                dxfVector ip = new dxfVector(aTopCenter);
                colDXFVectors verts = new colDXFVectors
                {
                    { ip.X + 0.5 * aWidth, ip.Y }
                };
                verts.AddRelative(aX:-aWidth);
                verts.AddRelative(aY:-aHeight);
                verts.AddRelative(aX: -aThickness);
                verts.AddRelative(aY: aHeight-aThickness);
                verts.AddRelative(aX: aWidth - aThickness);

                _rVal = new dxePolygon(verts, ip, true);
                
                if (aBendRadius > 0)
                {
                    if (_rVal.FilletAtVertex(5, aBendRadius)) _rVal.FilletAtVertex(2, aBendRadius + aThickness);
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return _rVal;
        }

        public static dxePolygon GenericSpliceAngle_Plan(uppPartTypes aPartType, dxfVector aTopLeft = null,  string aLayerName = "GEOMETRY" )
        {
            dxePolygon _rVal;
            dxfDisplaySettings dsp = new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Continuous);
            aLayerName = dsp.LayerName;

            string suf = aPartType == uppPartTypes.ManwayAngle ? "MANGLE" : "SPLANGLE";
            if (aPartType == uppPartTypes.ManwaySplicePlate) suf = "MANSPLICE";

            colDXFVectors verts = new colDXFVectors(new dxfVector(2.8, 0), new dxfVector(0, 0), new dxfVector(0, -0.75), new dxfVector(2.8, -0.75) { Linetype = dxfLinetypes.Invisible })
            {
                { 3, -0.75 },
                { 3.75, -0.75 },
                { 3.75, 0 },
                { 3, 0 }
            };


            _rVal = new dxePolygon(verts, dxfVector.Zero, false, $"GENERIC_{suf}_PLAN", dsp);


            //break lines
            verts = new colDXFVectors(new dxfVector(2.8, 0.125), new dxfVector(2.8, -0.244), new dxfVector(2.706, -0.291), new dxfVector(2.894, -0.384), new dxfVector(2.8, -0.431), new dxfVector(2.8, -0.875));
            _rVal.AdditionalSegments.Add(new dxePolyline(verts, false, dsp));
            verts.Move(0.2);
            _rVal.AdditionalSegments.Add(new dxePolyline(verts, false, dsp));

            verts = new colDXFVectors(new dxfVector(0.385, -0.538), new dxfVector(1.385, -0.538), new dxfVector(2.384, -0.538), new dxfVector(3.365, -0.538));
             _rVal.AdditionalSegments.AddCircles(verts, 0.05, aDisplaySettings: dsp);
            _rVal.AdditionalSegments.AddLine(new dxfVector(0, -0.72), new dxfVector(2.8, -0.72), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden), aTag: "THICKNESS_1");
            _rVal.AdditionalSegments.AddLine(new dxfVector(3, -0.72), new dxfVector(3.75, -0.72), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden), aTag: "THICKNESS_2");
            _rVal.AdditionalSegments.AddLine(new dxfVector(-0.1, -0.538), new dxfVector(3.85, -0.538), new dxfDisplaySettings(aLayerName, aLinetype: "Center"), aTag: "HOR_CENTERLINE",aFlag: "BOTTOM");
            if (aPartType == uppPartTypes.ManwaySplicePlate)
            {
                _rVal.AdditionalSegments.AddLine(new dxfVector(0, -0.357), new dxfVector(2.839, -0.357), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Continuous), aTag: "BEND_1A");
                _rVal.AdditionalSegments.AddLine(new dxfVector(3.039, -0.357), new dxfVector(3.75, -0.357), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Continuous), aTag: "BEND_1B");
                _rVal.AdditionalSegments.AddLine(new dxfVector(0, -0.405), new dxfVector(2.853, -0.405), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Continuous),aTag: "BEND_2A");
                _rVal.AdditionalSegments.AddLine(new dxfVector(3.053, -0.405), new dxfVector(3.75, -0.405), new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Continuous), aTag: "BEND_2B");

            }

            foreach (dxfVector v in verts)
            {
                _rVal.AdditionalSegments.AddLine(new dxfVector(v.X, 0.11), new dxfVector(v.X, -0.85), new dxfDisplaySettings(aLayerName, aLinetype: "Center"), aTag: "VER_CENTERLINE");
            }
            if (aPartType == uppPartTypes.ManwayAngle)
            {

            }
            else if (aPartType == uppPartTypes.ManwaySplicePlate)
            {
                verts.Move(0, 0.35);
                dsp.Linetype = dxfLinetypes.Hidden;
                _rVal.AdditionalSegments.AddCircles(verts, 0.05, aDisplaySettings: dsp);

                _rVal.AdditionalSegments.AddLine(new dxfVector(-0.1, -0.188), new dxfVector(3.85, -0.188), new dxfDisplaySettings(aLayerName, aLinetype: "Center"),aTag: "HOR_CENTERLINE", aFlag: "TOP");

            }
            else
            {
                verts.Move(0, 0.35);

                _rVal.AdditionalSegments.AddCircles(verts, 0.05, aDisplaySettings: dsp);

                _rVal.AdditionalSegments.AddLine(new dxfVector(-0.1, -0.188), new dxfVector(3.85, -0.188), new dxfDisplaySettings(aLayerName, aLinetype: "Center"),aTag: "HOR_CENTERLINE", aFlag: "TOP");

            }

            List<dxeArc> circs = _rVal.AdditionalSegments.Arcs();
            //verts = dxfPrimatives.CreatePolygonVertices(null, 6, 0.1);
            dxfVector v1 = null;
            dsp.Linetype = dxfLinetypes.Hidden;
            for (int i = 1; i <= circs.Count; i++)
            {
                dxeArc c = circs[i - 1];
                if (i == 1)
                {
                    v1 = c.Center.Clone();
                    verts = dxfPrimatives.CreatePolygonVertices(v1, 6, 0.085,bXScribed: true);

                }
                else
                {
                   
                    verts.Translate(c.Center - v1);
                    v1 = c.Center.Clone();
                }
                if (i > 4 && aPartType == uppPartTypes.ManwaySplicePlate) dsp.Linetype = dxfLinetypes.Continuous;
                _rVal.AdditionalSegments.Add(new dxePolyline(verts, true, dsp) { Tag = "HEX_HEAD" });
                _rVal.AdditionalSegments.Add(new dxeArc(v1, 0.085,aDisplaySettings:dsp));
            }

            if (aTopLeft != null) 
                _rVal.Translate(aTopLeft);
            return _rVal;
        }

        public static dxePolygon GenericSpliceAngle_Profile(uppPartTypes aPartType, out dxePolygon rPlanView, dxfVector aTopLeft = null, string aLayerName = "GEOMETRY")
        {

            rPlanView = GenericSpliceAngle_Plan(aPartType, aTopLeft, aLayerName);
            aLayerName = rPlanView.LayerName;

            dxePolygon _rVal;
            dxfDisplaySettings dsp = new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Continuous);
           
            string suf = aPartType == uppPartTypes.ManwayAngle ? "MANGLE" : "SPLANGLE";
            if (aPartType == uppPartTypes.ManwaySplicePlate) suf = "MANSPLICE";

            dxfRectangle rec = rPlanView.Vertices.BoundingRectangle(dxfPlane.World);
            double h = rec.Height;
            double t = h + rPlanView.AdditionalSegments.GetTagged("THICKNESS_2").Y;

            colDXFVectors verts = new colDXFVectors(new dxfVector(0.5 * t, 0));
            if(aPartType != uppPartTypes.ManwaySplicePlate)
            {
                verts.Add(0.5 * t, -h + t);
                verts.Add(h, -h + t);
            }
            else
            {
                verts.Add(0.5 * t, (-h / 2) + (0.75 * t));
                verts.Add(3 * t, (-h / 2) - (0.75 * t));

                verts.Add(3 * t, -h + t);
                verts.Add(h, -h + t);

            }

            verts = dxfPrimatives.CreateVertices_Trace(verts, t, aApplyFillets: true);


            _rVal = new dxePolygon(verts, dxfVector.Zero, true, $"GENERIC_{suf}_PROFILE", dsp);
            List<dxeLine> clines = rPlanView.AdditionalSegments.GetByTag("HOR_CENTERLINE").Lines();
            colDXFEntities holes = rPlanView.AdditionalSegments.GetByGraphicType(dxxGraphicTypes.Arc);
            dxeArc hole = holes.Count > 0 ? (dxeArc)holes.Item(1) : new dxeArc(dxfVector.Zero, t);
            double rad = hole.Radius;
            double y1 = clines.Count >= 1 ? clines[0].Y : 0;
            double y2 = clines.Count >= 2 ? clines[1].Y : 0;

            hdwHexBolt bolt = new hdwHexBolt(aSize: uppHardwareSizes.M10) { Length = 25 / 25.4, ObscuredLength = t };
            bolt.UpdateDimensions();
            double scaler = rad / (bolt.Diameter / 2);
            bolt.ObscuredLength = t/scaler;
            dxePolygon boltpg = uopPolygons.HexBolt_Profile( bolt.HardwareStructure, scaler, aDirection: dxxOrthoDirections.Left,bIncludeCenterLine:true, aLayerName: aLayerName, bAddWelds: true);
            //boltpg.Rescale(scaler, dxfVector.Zero);
            dxfVector v1;
            colDXFEntities boltview = boltpg.SubEntities();
            dxfDisplaySettings dsp1 = new dxfDisplaySettings(aLayerName, aLinetype: dxfLinetypes.Hidden);
            if (y1 != 0)
            {
                v1 = new dxfVector(t, y1);
                _rVal.AdditionalSegments.AddLine(0, y1 + rad, t, y1 + rad, dsp1);
                _rVal.AdditionalSegments.AddLine(0, y1 - rad, t, y1 - rad, dsp1);

                if (aPartType == uppPartTypes.ManwaySplicePlate) v1.X += 2.5 * t;
                    colDXFEntities boltviewsegs = boltview.Clone();
                boltviewsegs.Translate(v1);
               
                _rVal.AdditionalSegments.Append(boltviewsegs);
            }
            if (y2 != 0)
            {
                v1 = new dxfVector(t, y2);

                _rVal.AdditionalSegments.AddLine(0, y2 + rad, t, y2 + rad, dsp1);
                _rVal.AdditionalSegments.AddLine(0, y2 - rad, t, y2 - rad, dsp1);
                colDXFEntities boltviewsegs = boltview.Clone();
                if (aPartType == uppPartTypes.ManwaySplicePlate)
                {
                    v1.X -= t;
                    boltviewsegs.RotateAbout(dxfPlane.World.ZAxis(), 180);
                }
                boltviewsegs.Translate(v1);

                _rVal.AdditionalSegments.Append(boltviewsegs);
            }

        

            if (aTopLeft != null) _rVal.Translate(aTopLeft);
            return _rVal;
        }

        internal static dxePolygon HexBolt_Profile(THARDWARE aBlt, double aScaleFactor = 1, dxfVector aInsertionPt = null, dxfPlane aPlane = null, dxxOrthoDirections aDirection = dxxOrthoDirections.Up, bool bShowThreads = false, bool bShowHeadHidden = false, bool bSuppressShaft = false, bool bIncludeCenterLine = false, string aLayerName = "GEOMETRY", bool bAddWelds = false)
        {
            // ^returns the polygon used to draw the bolts profile (elevation) view

            var _rVal = new dxePolygon();

            dxfVector v1;
            dxfVector v2;
            double d1;
            int idx;
            double d2;
            dxeLine aLn;

            double x1;
            double x2;
            double y1;
            double y2;

            dxfPlane aPln;
            colDXFVectors verts;
            dxfVector ip;

            try
            {
                if (aScaleFactor == 0) aScaleFactor = 1;
                aScaleFactor = Math.Abs(aScaleFactor);
                THARDWARE.SetHexDimensions(ref aBlt);

                aPln = aPlane == null ? new dxfPlane(new dxfVector(aBlt.X, aBlt.Y)) : aPlane.Clone();

                aPln.SetCoordinates(aBlt.X, aBlt.Y);
                ip = aPln.Origin;
                if (aInsertionPt != null)
                {
                    ip = aInsertionPt.ProjectedToPlane(aPln);
                    aPln.Origin = ip;
                }

                if (bSuppressShaft)
                {
                    aBlt.Length = 0;
                    aBlt.dia = 0;
                }

                if (aScaleFactor != 1) aBlt = THARDWARE.ScaleDimensions(aBlt, aScaleFactor);

                aBlt.ThreadedLength = Math.Abs(aBlt.ThreadedLength) ;
                d2 = Math.Abs(aBlt.ObscuredLength);
                verts = uopPolygons.HexHeadVerts_3Sides(ip, aPln, aDirection, aBlt.H, aBlt.G, aBlt.f, aBlt.dia, false);
                if (bShowHeadHidden)
                {
                    verts.SetDisplayVariable(dxxDisplayProperties.Linetype, dxfLinetypes.Hidden);
                }

                // insert the shaft outline
                if (aBlt.Length > 0 && aBlt.dia > 0 && verts.Count > 0)
                {
                    d1 = 0.085 * aBlt.dia;
                    idx = 4;
                    v1 = verts.Item(idx);
                    if (d2 >= aBlt.Length - d1)
                    {
                        d2 = aBlt.Length - 1.1 * d1;
                    }
                    x1 = v1.WithRespectToPlane(aPln).X;
                    x2 = x1 + d1;
                    y2 = -aBlt.Length;
                    y1 = y2 + d1;

                    if (bShowHeadHidden)
                    {
                        v1.Linetype = dxfLinetypes.ByLayer;
                    }
                    if (d2 > 0)
                    {
                        v1.Tag = "OBSCURED";
                        v1.Linetype = dxfLinetypes.Hidden;
                        idx++;

                        verts.Add(aPln, x1, -d2, aTag: "SHAFT", aBeforeIndex: idx);
                        idx++;
                        verts.Add(aPln, x1, y1, aTag: "TAPER", aBeforeIndex: idx);
                        idx++;
                        verts.Add(aPln, x2, y2, aTag: "TIP", aBeforeIndex: idx);
                        idx++;
                        verts.Add(aPln, -x2, y2, aTag: "TAPER", aBeforeIndex: idx);
                        idx++;
                        verts.Add(aPln, -x1, y1, aTag: "SHAFT", aBeforeIndex: idx);
                        idx++;
                        verts.Add(aPln, -x1, -d2, aTag: "OBSCURED", aBeforeIndex: idx, aLineType: dxfLinetypes.Hidden);
                    }
                    else
                    {
                        v1.Tag = "SHAFT";
                        v1.Linetype = dxfLinetypes.ByLayer;
                        idx++;
                        verts.Add(aPln, x1, y1, aTag: "TAPER", aBeforeIndex: idx);
                        idx++;
                        verts.Add(aPln, x2, y2, aTag: "TIP", aBeforeIndex: idx);
                        idx++;
                        verts.Add(aPln, -x2, y2, aTag: "TAPER", aBeforeIndex: idx);
                        idx++;
                        verts.Add(aPln, -x1, y1, aTag: "SHAFT", aBeforeIndex: idx);
                    }

                    idx += 2;
                    v2 = verts.Item(idx);
                    aLn = _rVal.AdditionalSegments.AddLine(v1, v2, new dxfDisplaySettings( aLayerName));

                    if (bShowHeadHidden)
                    {
                        aLn.Linetype = dxfLinetypes.Hidden;
                    }

                    // draw the threads
                    if (bShowThreads)
                    {
                        int i = 1;
                        do
                        {
                            if (aBlt.ThreadedLength > 0)
                            {
                                if (y1 >= -aBlt.Length + aBlt.ThreadedLength)
                                {
                                    break;
                                }
                            }
                            if (d2 > 0)
                            {
                                if (y1 >= -d2)
                                {
                                    break;
                                }
                            }

                            if (i == 1)
                            {
                                _rVal.AdditionalSegments.Add(aPln.CreateLine(x2, y1, -x2, y1, aLayerName: aLayerName, aTag: "THREADS"));
                            }
                            else
                            {
                                _rVal.AdditionalSegments.Add(aPln.CreateLine(x1, y1, -x1, y1, aLayerName: aLayerName, aTag: "THREADS"));
                            }

                            y1 += d1;
                            i = -i;
                        } while (y1 < 0);
                    }
                    else
                    {
                        _rVal.AdditionalSegments.AddPlanarLine(aPln, x2, y2, x2, -d2, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: "THREADS");
                        _rVal.AdditionalSegments.AddPlanarLine(aPln, -x2, y2, -x2, -d2, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: "THREADS");
                        _rVal.AdditionalSegments.AddPlanarLine(aPln, x1, y2 + d1, -x1, y2 + d1, aLayerName);
                    }

                    if (bIncludeCenterLine)
                    {
                        d1 = 0.5 * aBlt.H;
                        _rVal.AdditionalSegments.Add(aPln.CreateLine(0, aBlt.H + d1, 0, -aBlt.Length - d1, aLayerName: aLayerName, aLineType: "Center", aTag: "CENTERLINE"));
                    }
                }

                if (bAddWelds)
                {
                    double w = 0.25 * aBlt.H;
                    v1 = verts.Item(3, true);
                    v2 = v1 + aPln.XDirection * -w;
                    dxfVector v3 = v1 + aPln.YDirection * w;
                    _rVal.AdditionalSegments.Add(new dxeSolid(v1, v2, v3, aPlane: aPln) { Tag = "WELD_1" });

                    v1 = verts.Item(12, true);
                    v2 = v1 + aPln.XDirection * w;
                    v3 = v1 + aPln.YDirection * w;
                    _rVal.AdditionalSegments.Add(new dxeSolid(v1, v2, v3, aPlane: aPln) { Tag = "WELD_2" });

                }

                _rVal.LayerName = aLayerName;
                _rVal.Plane = aPln;
                _rVal.InsertionPt = aPln.Origin;
                _rVal.Vertices = verts;
            }
            catch (Exception)
            {
            }

            return _rVal;
        }
        public  static colDXFVectors HexHeadVerts_3Sides(dxfVector aInsertionPt, dxfPlane arPlane, dxxOrthoDirections aDirection, double aHeight, double aAcrossPoints, double aAcrossFlats, double aShaftDia = 0, bool bBothSides = false, double aScaler = 1)
        {
            arPlane ??= dxfPlane.World;
            aInsertionPt ??= arPlane.Origin;
            colDXFVectors _rVal = new colDXFVectors();

            if (aDirection == dxxOrthoDirections.Up)
            { arPlane.Rotate(180); }
            else if (aDirection == dxxOrthoDirections.Left) { arPlane.Rotate(-90); }
            else if (aDirection == dxxOrthoDirections.Right) { arPlane.Rotate(90); }
            double aFC = 0.5 * aAcrossFlats / Math.Cos(30 * Math.PI / 180);

            double x1 = 0.5 * Math.Abs(aAcrossPoints);
            double x2 = Math.Abs(0.5 * aFC);
            double x3 = 0.5 * Math.Abs(aShaftDia);
            double y1 = Math.Abs(aHeight);
            double d1 = x1 - x2;
            double x4 = x2 + d1 / 2;
            double d2 = 0.5 * d1 * Math.Tan(25 * Math.PI / 180);
            double y2 = y1 - d2 * aScaler;

            _rVal.Add(arPlane, -x4 * aScaler, y1);
            _rVal.Add(arPlane, -x1 * aScaler, y2);
            _rVal.Add(arPlane, -x1 * aScaler, 0);
            if (x3 != 0) {

                _rVal.Add(arPlane, -x3 * aScaler, 0);
                _rVal.Add(arPlane, x3 * aScaler, 0);
            }
            _rVal.Add(arPlane, x1 *aScaler, 0);
            _rVal.Add(arPlane, x1 *aScaler, y2);
            _rVal.Add(arPlane, x4 *aScaler, y1);
            _rVal.Add(arPlane, x2 *aScaler, y2);
            _rVal.Add(arPlane, x2 *aScaler, 0);
            _rVal.Add(arPlane, x2 *aScaler, y2);
            _rVal.Add(arPlane, 0, y1* aScaler);
            _rVal.Add(arPlane, -x2 * aScaler, y2);
            _rVal.Add(arPlane, -x2 * aScaler, 0);
            _rVal.Add(arPlane, -x2 * aScaler, y2);
            _rVal.Add(arPlane, -x4 * aScaler, y1,aTag: "TOP");
            _rVal.Add(arPlane, x4 *aScaler, y1);

            return _rVal;
        }
  
        public static dxePolygon ManwayClip_Plan(uopManwayClip aPart, bool bIncludeWasher = false, bool bSuppressAngle = false, bool bObscured = false, bool bSuppressHole = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "GEOMETRY")
        {
            var _rVal = new dxePolygon();
            if (aPart == null)
            {
                return _rVal;
            }

            dxfVector v1 = dxfVector.Zero;
            dxePolygon aPGon;
            dxfPlane aPln;

            double leng = aPart.Length;
            double wd = aPart.Width / 2;
            double thk = aPart.Thickness;
            double cX = aPart.X;
            double cY = aPart.Y;

            string lt = bObscured ?dxfLinetypes.Hidden : dxfLinetypes.Continuous;
            double d1 = wd - 0.5 * aPart.TabWidth;

            // initialize

            if (bSuppressAngle)
            {
                aPln = xPlane(cX, cY, aCenter, aRotation);
            }
            else
            {
                aPln = xPlane(cX, cY, aCenter, aRotation + aPart.Angle);
            }

            


            //================ TOP VIEW ============================
           

            colDXFVectors verts = new colDXFVectors();


            verts.Add(aPln, -1.062 + thk, wd, aLineType: lt);
            if (!bObscured)
            {
                verts.Add(aPln, -1.062 + thk, wd - d1, aLineType: lt);
            }
            else
            {
                verts.Add(aPln, -1.062 + thk, wd - d1, aLineType: dxfLinetypes.Continuous);
            }
            verts.Add(aPln, -1.062, wd - d1, aLineType: lt);
            verts.Add(aPln, -1.062, wd, aLineType: lt);
            verts.Add(aPln, 0.988, wd, aLineType: lt);
            verts.Add(aPln, 0.988, -wd, aLineType: lt);
            verts.Add(aPln, -1.062, -wd, aLineType: lt);

            if (!bObscured)
            {
                verts.Add(aPln, -1.062, -wd + d1, aLineType: lt);
            }
            else
            {
                verts.Add(aPln, -1.062, -wd + d1, aLineType: dxfLinetypes.Continuous);
            }
            verts.Add(aPln, -1.062 + thk, -wd + d1, aLineType: lt);
            verts.Add(aPln, -1.062 + thk, -wd, aLineType: lt);

            colDXFEntities addsegs = new colDXFEntities();

            addsegs.AddPlanarLine(aPln, -1.062, wd - d1, -1.062, -wd + d1, aLayerName, aLineType: dxfLinetypes.Continuous);
            addsegs.AddPlanarLine(aPln, -1.062 + thk, wd - d1, -1.062 + thk, -wd + d1, aLayerName, aLineType: dxfLinetypes.Continuous);
            addsegs.AddPlanarLine(aPln, 0.813, wd, 0.813, -wd, aLayerName, aLineType: dxfLinetypes.Hidden);

            if (!bSuppressHole)
            {
                addsegs.AddPlanarArc(aPln, mdGlobals.gsBigHole / 2, aLayerName: aLayerName, aLineType: lt, aTag: "CLIP HOLE");
            }
            _rVal = new dxePolygon(verts, aPln.Origin, false, aPlane: aPln) { AdditionalSegments = addsegs, Linetype = lt };


            if (bIncludeWasher)
            {
                aPGon = ManwayWasher_Plan(aPart.Washer,true, false, false, dxfLinetypes.Continuous);
                _rVal.AdditionalSegments.Append(aPGon.SubEntities(false));
            }

            


            return _rVal;
        }

        public static dxePolygon ManwayWasher_Plan(uopManwayClipWasher aPart, bool bSuppressAngle, bool bSuppressHole = false, bool bSuppressSlot = false, string aLineType = "")
        {
            if (aPart == null) return null;

            //double thk = aPart.Thickness;
            double cX = aPart.X;
            double cY = aPart.Y;
            colDXFVectors verts = new colDXFVectors();
            verts.Add(cX - 2.375, cY + 0.5 * aPart.Width, aLineType: aLineType);
            verts.Add(cX - 2.375, cY - 0.5 * aPart.Width, aLineType: aLineType);
            verts.Add(cX + (aPart.Length - 2.375), cY - 0.5 * aPart.Width, aLineType: aLineType);
            verts.Add(cX + (aPart.Length - 2.375), cY + 0.5 * aPart.Width, aLineType: aLineType);
            dxePolygon _rVal = new dxePolygon(verts, new dxfVector(cX, cY), bClosed: true);
            if (!string.IsNullOrWhiteSpace(aLineType)) _rVal.Linetype = aLineType;
            if (!bSuppressHole) _rVal.AdditionalSegments.AddArc(cX, cY, mdGlobals.gsBigHole / 2, aDisplaySettings: dxfDisplaySettings.Null(aLinetype: aLineType), aTag: "WASHER HOLE");
            if (!bSuppressSlot)
            {
                dxePolyline aPl = (dxePolyline)uopGlobals.UopGlobals.Primatives.Pill(new dxfVector(cX - 1, cY), mdGlobals.gsBigHole, 0.25, 90);
                aPl.Tag = "WASHER SLOT";
                aPl.Linetype = aLineType;
                _rVal.AdditionalSegments.Add(aPl);
            }

            if (!bSuppressAngle) _rVal.RotateAbout(_rVal.InsertionPt, aPart.Angle);
            return _rVal;

        }


        public static dxePolygon ManwayWasher_Profile(uopManwayClipWasher aPart, double ScaleFactor = 1)
        {

            if (aPart == null) return null;
            double thk = aPart.Thickness;
            double cX = aPart.X;
            double cY = aPart.Y;
            dxfVector v1 = new dxfVector(cX - 0.5 * thk, cY - 0.5 * aPart.Hole.Inset + 0.5 * aPart.Length);

            dxePolygon _rVal = (dxePolygon)uopGlobals.UopGlobals.Primatives.Rectangle(v1, true, aPart.Length, thk);
            colDXFEntities Hls = new colDXFEntities(aPart.Hole.AsViewedFrom("Y"), aPart.Slot.AsViewedFrom("Y"));
            uopUtils.AddHolesToPGON(_rVal, new colDXFEntities(aPart.Hole.AsViewedFrom("Y"), aPart.Slot.AsViewedFrom("Y")), Enums.uppPartViews.Top);
            _rVal.InsertionPt.SetCoordinates(cX, cY);
            return _rVal;

        }


        private static dxfPlane xPlane(double aCX, double aCY, iVector aCenter = null, double aRotation = 0)
        {
            var _rVal = dxfPlane.World;
            if (aCenter != null)
            {
         
                aCX = aCenter.X; aCY = aCenter.Y;
            }

            _rVal.SetCoordinates(aCX, aCY);
            if (aRotation != 0)
            {
                _rVal.Rotate(aRotation);
            }

            return _rVal;
        }

    }
}
