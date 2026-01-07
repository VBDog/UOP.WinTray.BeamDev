using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;

namespace UOP.WinTray.Projects.Parts
{
    public class hdwHexBolt : uopHardware
    {
        public override uppPartTypes BasePartType => uppPartTypes.HexBolt;

        #region Constructors

        public hdwHexBolt(uppHardwareSizes aSize = uppHardwareSizes.M10, uopHardwareMaterial aMaterial = null) : base(uppHardwareTypes.HexBolt, aSize, aMaterial) { }

        public hdwHexBolt(uopPart aParent) : base(uppHardwareTypes.HexBolt, (aParent.Bolting == uppUnitFamilies.English) ? uppHardwareSizes.ThreeEights : uppHardwareSizes.M10, aParent.HardwareMaterial) { }


        internal hdwHexBolt(uopHardware aParent, int aQuantity = 0) : base(uppHardwareTypes.SetScrew, aParent.Size, aParent.HardwareMaterial, (aQuantity > 0) ? aQuantity : aParent.Quantity)
        {
            SubPart(aParent);
            if (aParent.Type == Type) HardwareStructure = aParent.HardwareStructure;
        }

        #endregion

        #region Subparts  

        /// <summary>
        ///returns a nut with the same material and size as the bolt
        /// </summary>
        public hdwHexNut GetNut(int aQuantity = 0) => new hdwHexNut(this, aQuantity);

        /// <summary>
        ///returns a lock washer with the same material and size as the bolt
        /// </summary>
        public hdwLockWasher GetLockWasher(int aQuantity = 0) => new hdwLockWasher(this, aQuantity);

        /// <summary>
        ///returns a washer with the same material and size as the bolt
        /// </summary>
        public hdwFlatWasher GetWasher(int aQuantity = 0) => new hdwFlatWasher(this, aQuantity);

        #endregion


         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }

        public double HeadDiameter => uopHardware.BoltHeadDiameter(Size);
        
        public double HeadHeight => uopHardware.BoltHeadHeight(Size);
      
        public override double Length { get => base.BoltLength; set => base.BoltLength = value; }

        public override double ThreadedLength { get => base.ThreadedLength; set => base.ThreadedLength = value; }


        public override uopHardwareMaterial HardwareMaterial { get => base.HardwareMaterial; set => base.HardwareMaterial = value; }


        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public hdwHexBolt Clone(int aQuantity = 0) => new hdwHexBolt(this, aQuantity);

        //string that describes the hardware
        //like "M10 Lock Washer" or "3/8'' x 1.000 Hex Head Bolt"
        public override string GetFriendlyName(bool AllCaps = true) => AllCaps ? base.FriendlyName.ToUpper() : base.FriendlyName;



        public bool IsEqual(hdwHexBolt aPart) => uopHardware.Compare(this, aPart);
      


        public override void UpdatePartProperties() => PartNumber = GetFriendlyName(false);


        public override void UpdatePartWeight() { base.Weight = Weight; }

        public dxePolygon View_Profile(double aScaleFactor = 1, dxfVector aInsertionPt = null, dxfPlane aPlane = null, dxxOrthoDirections aDirection = dxxOrthoDirections.Up, bool bShowThreads = false, bool bShowHeadHidden = false, bool bSuppressShaft = false, bool bIncludeCenterLine = false, string aLayerName = "GEOMETRY", bool bAddWelds = false)
        {
            // ^returns the polygon used to draw the bolts profile (elevation) view
            return uopPolygons.HexBolt_Profile(HardwareStructure, aScaleFactor, aInsertionPt, aPlane, aDirection, bShowThreads, bShowHeadHidden, bSuppressShaft, bIncludeCenterLine, aLayerName, bAddWelds);
            //    var _rVal = new dxePolygon();

            //THARDWARE aBlt = HardwareStructure;
            //dxfVector v1;
            //dxfVector v2;
            //double d1;
            //int idx;
            //double d2;
            //dxeLine aLn;
      
            //double x1;
            //double x2;
            //double y1;
            //double y2;
      
            //dxfPlane aPln;
            //colDXFVectors verts;
            //dxfVector ip;

            //try
            //{
            //    if (aScaleFactor == 0) aScaleFactor = 1;
            //    aScaleFactor = Math.Abs(aScaleFactor);

            
            //    aBlt.UpdateDimensions();
            //    aPln = aPlane == null ? new dxfPlane(new dxfVector(aBlt.X, aBlt.Y)) : aPlane.Clone();

            //    aPln.SetCoordinates(aBlt.X, aBlt.Y);
            //    ip = aPln.Origin;
            //    if (aInsertionPt != null)
            //    {
            //        ip = aInsertionPt.ProjectedToPlane(aPln);
            //        aPln.Origin = ip;
            //    }

            //    if (bSuppressShaft)
            //    {
            //        aBlt.Length = 0;
            //        aBlt.dia = 0;
            //    }
            //    else
            //    {
            //        aBlt.Length *= aScaleFactor;
            //        aBlt.dia *= aScaleFactor;
            //    }
            //    aBlt.ThreadedLength = Math.Abs(aBlt.ThreadedLength) * aScaleFactor;
            //    d2 = Math.Abs(aBlt.ObscuredLength) * aScaleFactor;
            //    verts = uopPolygons.HexHeadVerts_3Sides(ip, aPln, aDirection, aBlt.H, aBlt.G, aBlt.f, aBlt.dia, false, aScaleFactor);
            //    if (bShowHeadHidden)
            //    {
            //        verts.SetDisplayVariable(dxxDisplayProperties.Linetype , dxfLinetypes.Hidden);
            //    }

            //    // insert the shaft outline
            //    if (aBlt.Length > 0 && aBlt.dia > 0 && verts.Count > 0)
            //    {
            //        d1 = 0.085 * aBlt.dia;
            //        idx = 4;
            //        v1 = verts.Item(idx);
            //        if (d2 >= aBlt.Length - d1)
            //        {
            //            d2 = aBlt.Length - 1.1 * d1;
            //        }
            //        x1 = v1.WithRespectToPlane(aPln).X;
            //        x2 = x1 + d1;
            //        y2 = -aBlt.Length;
            //        y1 = y2 + d1;

            //        if (bShowHeadHidden)
            //        {
            //            v1.Linetype = dxfLinetypes.ByLayer;
            //        }
            //        if (d2 > 0)
            //        {
            //            v1.Tag = "OBSCURED";
            //            v1.Linetype = dxfLinetypes.Hidden;
            //            idx++;

            //            verts.Add(aPln, x1, -d2, aTag: "SHAFT", aBeforeIndex: idx);
            //            idx++;
            //            verts.Add(aPln, x1, y1, aTag: "TAPER", aBeforeIndex: idx);
            //            idx++;
            //            verts.Add(aPln, x2, y2, aTag: "TIP", aBeforeIndex: idx);
            //            idx++;
            //            verts.Add(aPln, -x2, y2, aTag: "TAPER", aBeforeIndex: idx);
            //            idx++;
            //            verts.Add(aPln, -x1, y1, aTag: "SHAFT", aBeforeIndex: idx);
            //            idx++;
            //            verts.Add(aPln, -x1, -d2, aTag: "OBSCURED", aBeforeIndex: idx, aLineType: dxfLinetypes.Hidden);
            //        }
            //        else
            //        {
            //            v1.Tag = "SHAFT";
            //            v1.Linetype = dxfLinetypes.ByLayer;
            //            idx++;
            //            verts.Add(aPln, x1, y1, aTag: "TAPER", aBeforeIndex: idx);
            //            idx++;
            //            verts.Add(aPln, x2, y2, aTag: "TIP", aBeforeIndex: idx);
            //            idx++;
            //            verts.Add(aPln, -x2, y2, aTag: "TAPER", aBeforeIndex: idx);
            //            idx++;
            //            verts.Add(aPln, -x1, y1, aTag: "SHAFT", aBeforeIndex: idx);
            //        }

            //        idx += 2;
            //        v2 = verts.Item(idx);
            //        aLn = _rVal.AdditionalSegments.AddLine(v1, v2, aLayerName: aLayerName);

            //        if (bShowHeadHidden)
            //        {
            //            aLn.Linetype = dxfLinetypes.Hidden;
            //        }

            //        // draw the threads
            //        if (bShowThreads)
            //        {
            //            int i = 1;
            //            do
            //            {
            //                if (aBlt.ThreadedLength > 0)
            //                {
            //                    if (y1 >= -aBlt.Length + aBlt.ThreadedLength)
            //                    {
            //                        break;
            //                    }
            //                }
            //                if (d2 > 0)
            //                {
            //                    if (y1 >= -d2)
            //                    {
            //                        break;
            //                    }
            //                }

            //                if (i == 1)
            //                {
            //                    _rVal.AdditionalSegments.Add(aPln.CreateLine(x2, y1, -x2, y1, aLayerName: aLayerName, aTag: "THREADS"));
            //                }
            //                else
            //                {
            //                    _rVal.AdditionalSegments.Add(aPln.CreateLine(x1, y1, -x1, y1, aLayerName: aLayerName, aTag: "THREADS"));
            //                }

            //                y1 += d1;
            //                i = -i;
            //            } while (y1 < 0);
            //        }
            //        else
            //        {
            //            _rVal.AdditionalSegments.AddPlanarLine(aPln, x2, y2, x2, -d2, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: "THREADS");
            //            _rVal.AdditionalSegments.AddPlanarLine(aPln, -x2, y2, -x2, -d2, aLayerName, aLineType: dxfLinetypes.Hidden, aTag: "THREADS");
            //            _rVal.AdditionalSegments.AddPlanarLine(aPln, x1, y2 + d1, -x1, y2 + d1, aLayerName);
            //        }

            //        if (bIncludeCenterLine)
            //        {
            //            d1 = 0.5 * aBlt.H;
            //            _rVal.AdditionalSegments.Add(aPln.CreateLine(0, aBlt.H + d1, 0, -aBlt.Length - d1, aLayerName: aLayerName, aLineType: "Center", aTag: "CENTERLINE"));
            //        }
            //    }

            //    if (bAddWelds)
            //    {
            //        double w = 0.0925;
            //        v1 = verts.Item(3, true);
            //        v2 = v1 + aPln.XDirection * -w;
            //        dxfVector v3 = v1 + aPln.YDirection * w;
            //        _rVal.AdditionalSegments.Add(new dxeSolid(v1, v2, v3, aPlane: aPln) { Tag = "WELD_1" });

            //        v1 = verts.Item(12, true);
            //        v2 = v1 + aPln.XDirection * w;
            //        v3 = v1 + aPln.YDirection * w;
            //        _rVal.AdditionalSegments.Add(new dxeSolid(v1, v2, v3, aPlane: aPln) { Tag = "WELD_2" });

            //    }

            //    _rVal.LayerName = aLayerName;
            //    _rVal.Plane = aPln;
            //    _rVal.InsertionPt = aPln.Origin;
            //    _rVal.Vertices = verts;
            //}
            //catch (Exception)
            //{
            //}

            //return _rVal;
        }

      

        public dxfBlock Block_Profile(dxfImage aImage, string aName = "BOLT", bool bIncludeCenterLine = false, string aLayerName = "GEOMETRY", dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined, bool bThreeFaces = false, string aHeadLT = "")
        {
            dxePolyline aPl;
            dxfImage bImage;
          
            double oln;
            dxeLine l1;
            dxeLine l2;
            double lng;
            dxePolyline bPl;
            double d1;

            dxfVector v1;
            dxfVector v2;
            oln = ObscuredLength;
            bImage = aImage ?? new dxfImage();
            var blockProfile = new dxfBlock();
            lng = Length;

            blockProfile.Name = aName;
            blockProfile.LayerName = aLayerName;
            aPl = new dxfPrimatives(bImage.GUID).HexHeadProfile(H, G, f, bThreeFaces, bImage.UCS); // It was "goPrimatives.HexHeadProfile(H, G, f, bThreeFaces, bImage.UCS)" in VB but the corresponding C# class is not static. I need to instantiate it, however, I am not sure if I should pass the GUID.
            aPl.Tag = "HEAD";
            aPl.LayerName = aLayerName;
            blockProfile.LayerName = aLayerName;
            if (!string.IsNullOrWhiteSpace(aHeadLT))
            {
                aPl.Linetype = aHeadLT;
            }
            blockProfile.Entities.Add(aPl);
            
            if (lng > 0 && Diameter > 0) // It was "dia" in VB code
            {
                d1 = 0.1 * Diameter;
                bPl = (dxePolyline)aPl.Clone();

                l1 = new dxeLine();
                l1.StartPt.X = 0.5 * Diameter;
                l1.EndPt.X = 0.5 * Diameter;
                l1.Linetype = dxfLinetypes.Hidden;
                l1.LayerName = aLayerName;
                l2 = (dxeLine)l1.Clone();
                l2.Move(-Diameter);
                if (oln > 0)
                {
                    l1.EndPt.Y = -oln;
                    l2.EndPt.Y = -oln;
                    blockProfile.Entities.Add(l1);
                    blockProfile.Entities.Add(l2);
                }

                bPl.Linetype = dxfLinetypes.ByLayer;
                bPl.Closed = false;

                bPl.Vertices.Clear();
                bPl.Vertices.Add(l2.EndPt, bAddClone: true);
                v1 = bPl.Vertices.AddRelative(aY: -(lng - d1 - oln)); // It was "bPl.Vertices.AddRelative(aY: -(lng - d1 - oln))" in VB but the signature has changed and it returns an "int" in C#, instead of a "dxfVector"
                bPl.Vertices.AddRelative(d1, -d1);
                bPl.Vertices.AddRelative(Diameter - 2 * d1);

                v2 = bPl.Vertices.AddRelative(d1, d1); // It was "bPl.Vertices.AddRelative(aY: -(lng - d1 - oln))" in VB but the signature has changed and it returns an "int" in C#, instead of a "dxfVector"
                bPl.Vertices.Add(v1.Clone());
                bPl.Vertices.Add(v2.Clone());
                bPl.Vertices.AddRelative(aY: lng - d1 - oln);
                blockProfile.Entities.Add(bPl);

                l1 = new dxeLine
                {
                    Linetype = dxfLinetypes.Hidden,
                    LayerName = aLayerName
                };
                l1.StartPt.SetCoordinates(-Diameter / 2 + d1, -oln);
                l1.EndPt.SetCoordinates(-Diameter / 2 + d1, -lng);
                blockProfile.Entities.Add(l1);

                l2 = (dxeLine)l1.Clone();
                l2.StartPt.X = Diameter / 2 - d1;
                l2.EndPt.X = Diameter / 2 - d1;
                blockProfile.Entities.Add(l2);

                if (bIncludeCenterLine)
                {
                    d1 = 0.75 * H;
                    l1 = new dxeLine
                    {
                        Linetype = "Center",
                        LayerName = bImage.LinetypeLayers.LineLayer("Center")
                    };
                    l1.StartPt.Y = H + d1;
                    l1.EndPt.Y = -lng - d1;
                    blockProfile.Entities.Add(l1);
                }
            }

            aImage.LinetypeLayers.ApplyTo(blockProfile.Entities, aLTLSetting, aImage:  bImage); // In VB it does not have the last parameter but here it is not optional. So, I passed the bImage.
            
            return blockProfile;
        }

    }
}
