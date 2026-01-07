using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Generators
{


    public class mdBlocks
    {

        public static dxfBlock Downcomer_View_Plan(dxfImage aImage, mdDowncomer aDowncomer, mdTrayAssembly aAssy, bool bSetInstances = false, bool bOutLineOnly = false, bool bSuppressHoles = false, bool bIncludeSpouts = false, bool bShowObscured = false, double aCenterLineLength = 0, string aLayerName = "GEOMETRY", bool bIncludeEndPlates = true, bool bIncludeEndSupports = true, bool bIncludeShelfAngles = true, bool bIncludeStiffeners = false, bool bIncludeBaffles = false, bool bIncludeSupDefs = true, bool bIncludeFingerClips = false, bool bIncludeEndAngles = false, bool bDontAddToImage = false, string aBlockNameSuffix = null)
        {

            string pn = aDowncomer != null ? aDowncomer.Index.ToString() : "0";

            dxfBlock _rVal = new dxfBlock($"DOWNCOMER_{pn}_PLAN_VIEW");
            if (aDowncomer == null) return _rVal;
            aAssy ??= aDowncomer.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;
            List<mdDowncomerBox> boxes = aDowncomer.Boxes.FindAll((x) => !x.IsVirtual);
            if (boxes.Count <= 0)
            {
                _rVal = null;
                return null;
            }

            mdDowncomerBox box = boxes[0];
            mdDowncomerBox primarybox = box;

            string blockname = boxes.Count == 1 ? $"DOWNCOMER_{box.PartNumber}_PLAN_VIEW" : $"DOWNCOMER_{aDowncomer.Index}_PLAN_VIEW";
            if (!string.IsNullOrWhiteSpace(aBlockNameSuffix)) blockname += aBlockNameSuffix;
            try
            {

                if (aAssy.ProjectType != uppProjectTypes.MDDraw)
                {
                    bIncludeStiffeners = false;
                    bIncludeFingerClips = false;
                    bIncludeEndAngles = false;
                    bIncludeSupDefs = false;
                }

                dxeInsert ins = null;
                uopVector ip = new uopVector(box.X, box.Y);

                dxfBlock boxblock = null;

                _rVal.Name = blockname;
                // multiple boxes    
                for (int i = 1; i <= boxes.Count; i++)
                {
                    box = boxes[i - 1];
                    boxblock = mdBlocks.DowncomerBox_View_Plan(aImage, box, aAssy, false, bOutLineOnly, bSuppressHoles, false, bShowObscured, aCenterLineLength, aLayerName, bIncludeEndPlates, bIncludeEndSupports, bIncludeShelfAngles, bIncludeStiffeners, bIncludeBaffles, bIncludeSupDefs, bIncludeFingerClips, bIncludeEndAngles);

                    if (i == 1)
                    {

                        if (boxes.Count == 1)
                        {

                            _rVal = boxblock;
                            break;

                        }
                        else
                        {
                            if (aImage != null) boxblock = aImage.Blocks.Add(boxblock);
                            ins = new dxeInsert(boxblock, new dxfVector(0, box.Y - ip.Y)) { Tag = box.PartNumber };
                            _rVal.Entities.Add(ins);
                        }
                    }

                    else
                    {
                        if (aImage != null) boxblock = aImage.Blocks.Add(boxblock);
                        ins = new dxeInsert(boxblock, new dxfVector(0, box.Y - ip.Y)) { Tag = box.PartNumber };
                        _rVal.Entities.Add(ins);
                    }
                }

                if (bIncludeSpouts)
                {
                    dxfDisplaySettings dsp = dxfDisplaySettings.Null("SPOUTS", dxxColors.ByLayer);
                    List<mdSpoutGroup> sgs = aAssy.SpoutGroups.GetByVirtual(aVirtualValue: false).FindAll((x) => x.DowncomerIndex == aDowncomer.Index);
                    foreach (var sg in sgs)
                    {
                        if (sg.SpoutCount(aAssy) <= 0) continue;
                        uopVector sgip = new uopVector(sg.X, sg.Y);
                        dxfBlock block = aImage.Blocks.Add(sg.Block(dsp: dsp, aImage: aImage));
                        if (block.Entities.Count <= 0) continue;

                        block.Instances.RemoveAll((x) => x.XOffset != 0);
                        dxeInsert sginsert = new dxeInsert(block, sgip - ip) { Tag = "SPOUT GROUP", Flag = sg.Handle, DisplaySettings = dsp };

                        //if(sg.MirrorY.HasValue)
                        //{
                        //    uopVector sgip2 = new uopVector(sg.X, sg.Y);
                        //    sgip2.Mirror(null, sg.MirrorY);

                        //    sginsert.Instances.Add(0, sgip2.Y-sgip.Y, aRotation:180, bLeftHanded:true);
                        //}
                        _rVal.Entities.Add(sginsert);
                    }
                }


            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);

            }
            finally
            {
                if (_rVal != null)
                {
                    _rVal.Tag = aDowncomer.PartName.ToUpper();  //DOWNCOMER(INDEX)
                    _rVal.Name = blockname;
                    if (aImage != null && !bDontAddToImage)
                    {
                        _rVal = aImage.Blocks.Add(_rVal);
                    }


                    _rVal.Instances = primarybox.Instances.ToDXFInstances();
                    if (!bSetInstances) _rVal.Instances.Clear();

                }
            }
            return _rVal;
        }
        public static dxfBlock SpliceAngle_View_Plan( dxfImage aImage, mdSpliceAngle aPart, mdTrayAssembly aAssy, string aLayerName = "SPLICE ANGLES",  bool bShowHidden = false, bool bSuppressHoles = false, bool bSetInstances = false)
        {

            if (aPart == null) return null;
            aAssy ??= aPart.GetMDTrayAssembly();
            if (aAssy == null) return null;
            if (aAssy.ProjectType != uppProjectTypes.MDDraw) return null;
         
            string bname = $"SPLICE_ANGLE_{aPart.PartNumber}_PLAN_VIEW";
            dxfBlock _rVal = aImage != null ? aImage.Blocks.Item(bname) : null;

            try
            {
                if (_rVal == null)
                {
                    _rVal = new dxfBlock(bname);
                       
                    dxePolygon pgon = mdPolygons.SpliceAngle_View_Plan(aPart, bShowHidden: bShowHidden, bSuppressHoles: bSuppressHoles, bSuppressOrientations: true, aCenter: uopVector.Zero, aRotation: 0, aLayerName: aLayerName);
                    _rVal = pgon.Block(bname, aImage: aImage, aLTLSettings: dxxLinetypeLayerFlag.ForceToColor, aLayerName: aLayerName);
                    if (aImage != null)
                        _rVal = aImage.Blocks.Add(_rVal);

                    pgon.Dispose();

         
                 
                }
                _rVal.Instances = aPart.Instances.ToDXFInstances();
                if (!bSetInstances) _rVal.Instances.Clear();
            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);
            }


            return _rVal;

        }

        public static dxfBlock Stiffener_View_Plan(dxfImage aImage, mdTrayAssembly aAssy, string aLayerName = "STIFFENERS", mdDowncomerBox aBox = null, bool bSetInstances = false, bool bSuppressHoles = false, bool bIncludeFillets = true)
        {

            if (aAssy == null) return null;
            if (aAssy.ProjectType != uppProjectTypes.MDDraw) return null;

            string bname = $"DCSTIFFENER_{aAssy.TrayName().ToUpper()}_PLAN_VIEW";
            dxfBlock _rVal = aImage != null ? aImage.Blocks.Item(bname) : null;

            try
            {
                if (_rVal == null)
                {
                    _rVal = new dxfBlock(bname);
                    aBox ??= aAssy.Downcomers.Boxes().FirstOrDefault();

                    mdStiffener stiff = new mdStiffener(aBox, 0);
                    dxePolygon pgon = mdPolygons.Stiffener_View_Plan(stiff, aBox, aAssy, bSuppressHoles: bSuppressHoles, bIncludeFillets: bIncludeFillets, aCenter: dxfVector.Zero, aRotation: 0, aLayerName: aLayerName);
                    _rVal = pgon.Block(bname, aImage: aImage, aLTLSettings: dxxLinetypeLayerFlag.ForceToColor, aLayerName: aLayerName);
                    if (aImage != null)
                        _rVal = aImage.Blocks.Add(_rVal);

                    pgon.Dispose();

                    if (bSetInstances)
                    {
                        uopVectors stifpts = aAssy.StiffenerCenters();
                        if (stifpts.Count > 0)
                        {
                            _rVal.Instances.Clear();
                            uopVector u1 = stifpts.Item(1);
                            _rVal.InsertionPt.SetCoordinates(u1.X, u1.Y);
                            _rVal.Instances.DefineWithVectors(stifpts, u1, false);
                            //for (int i = 1; i <= stifpts.Count; i++) { uopVector u2 = stifpts.Item(i); _rVal.Instances.Add(u2.X - u1.X, u2.Y - u1.Y); }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);
            }


            return _rVal;

        }
        public static dxfBlock FingerClip_View_Plan(dxfImage aImage, mdFingerClip aPart, bool bSuppressHoles = false, string aLayerName = "FINGER CLIPS", bool bDontAddToImage = false)
        {
            dxfBlock _rVal = null;
            if (aPart == null) return null;
            string pn = aPart.PartNumber;
            string bname = !bSuppressHoles ? $"FINGER_CLIP_{pn}_PLAN_VIEW" : $"FINGER_CLIP_{pn}_SIMPLE_VIEW";
            dxePolygon pg = null;
            try
            {
                if (aImage != null)
                {
                    _rVal = aImage.Blocks.Item(bname);
                }
                if (_rVal == null)
                {
                    pg = mdPolygons.md_FingerClip_View_Plan(aPart, bSuppressHoles, true, uopVector.Zero, 0, aLayerName);
                    _rVal = pg.Block(bname, aImage: aImage, aLTLSettings: dxxLinetypeLayerFlag.ForceToColor);

                }

            } catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);
            }
            finally { pg?.Dispose(); if (aImage != null && !bDontAddToImage) _rVal = aImage.Blocks.Add(_rVal); }
            return _rVal;
        }

        public static dxfBlock EndAngle_View_Plan(dxfImage aImage, mdEndAngle aPart, bool bSuppressHoles = false, string aLayerName = "END ANGLES", bool bDontAddToImage = false)
        {
            dxfBlock _rVal = null;
            if (aPart == null) return null;
            string pn = aPart.PartNumber;
            string bname = !bSuppressHoles ? $"END_ANGLE_{pn}_PLAN_VIEW" : $"END_ANGLE_{pn}_SIMPLE_VIEW";
            dxePolygon pg = null;
            try
            {
                if (aImage != null)
                {
                    _rVal = aImage.Blocks.Item(bname);
                }
                if (_rVal == null)
                {
                    pg = mdPolygons.EndAngle_View_Plan(aPart, uopVector.Zero, 0, bSuppressHoles, aLayerName, bIgnoreDirection: true);
                    _rVal = pg.Block(bname, aImage: aImage, aLTLSettings: dxxLinetypeLayerFlag.ForceToColor);
                }

            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);
            }
            finally { pg?.Dispose(); if (aImage != null && !bDontAddToImage) _rVal = aImage.Blocks.Add(_rVal); }
            return _rVal;
        }
        public static dxfBlock DowncomerBoxs_View_Plan(dxfImage aImage, mdTrayAssembly aAssy, string aBlockName = "", bool bSetInstances = false, bool bOutLineOnly = false, bool bSuppressHoles = false, bool bIncludeSpouts = false, bool bShowObscured = false, double aCenterLineLength = 0, string aLayerName = "GEOMETRY", bool bIncludeEndPlates = true, bool bIncludeEndSupports = true, bool bIncludeShelfAngles = true, bool bIncludeStiffeners = true, bool bIncludeBaffles = true, bool bIncludeSupDefs = true, bool bIncludeFingerClips = false, bool bIncludeEndAngles = false, double aRotation = 0)
        {
            if (aAssy == null) return null;
            List<mdDowncomerBox> boxes = aAssy.Downcomers.Boxes();
            dxfBlock _rVal = new dxfBlock(!string.IsNullOrWhiteSpace(aBlockName) ? aBlockName : $"TRAY_{aAssy.SpanName()}_DOWNCOMER_BOXES") { LayerName = aLayerName };
            foreach (var box in boxes)
            {
                dxfBlock boxblock = mdBlocks.DowncomerBox_View_Plan(aImage, box, aAssy, bSetInstances, bOutLineOnly, bSuppressHoles, bIncludeSpouts, bShowObscured, aCenterLineLength, aLayerName, bIncludeEndPlates, bIncludeEndSupports, bIncludeShelfAngles, bIncludeStiffeners, bIncludeBaffles, bIncludeSupDefs, bIncludeFingerClips, bIncludeEndAngles, aRotation: 0);

                dxfVector ip = new dxfVector(box.Center);

                if (aImage != null)
                {
                    boxblock = aImage.Blocks.Add(boxblock);
                    if (aRotation != 0)
                    {
                        ip = aImage.UCS.Vector(box.Y, box.X);

                    }


                }
                dxeInsert insert = new dxeInsert(boxblock, ip) { LayerName = aLayerName, RotationAngle = aRotation };
                insert.Instances = boxblock.Instances;

                _rVal.Entities.Add(insert);

            }
            // if (aRotation != 0) _rVal.Entities.RotateAbout(dxfDirection.WorldZ, aRotation);
            if (aImage != null) _rVal = aImage.Blocks.Add(_rVal);

            return _rVal;

        }

        public static dxfBlock DowncomerBox_View_Plan(dxfImage aImage, mdDowncomerBox aBox, mdTrayAssembly aAssy, bool bSetInstances = false, bool bOutLineOnly = false, bool bSuppressHoles = false, bool bIncludeSpouts = false, bool bShowObscured = false, double aCenterLineLength = 0, string aLayerName = "GEOMETRY", bool bIncludeEndPlates = true, bool bIncludeEndSupports = true, bool bIncludeShelfAngles = true, bool bIncludeStiffeners = true, bool bIncludeBaffles = true, bool bIncludeSupDefs = true, bool bIncludeFingerClips = false, bool bIncludeEndAngles = false, double aRotation = 0)
        {


            dxfBlock _rVal = null;

            if (aBox == null) return _rVal;
            aAssy ??= aBox.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;
            string pn = aBox.PartNumber;

            _rVal = new dxfBlock(bOutLineOnly ? $"DCBOX_{aBox.DowncomerIndex}_{aBox.Index}_OUTLINE_VIEW" : $"DCBOX_{pn}_PLAN_VIEW");

            mdProject proj = aAssy.MDProject;

            if (aAssy.ProjectType != uppProjectTypes.MDDraw || bOutLineOnly)
            {
                bIncludeBaffles = false;
                bIncludeSupDefs = false;
                bIncludeStiffeners = false;
                bIncludeFingerClips = false;
                bIncludeEndAngles = false;
            }

            try
            {
                dxfBlock subblock = null;
                dxeInsert ins = null;
                uopInstances uinsts = null;
                uopVectors ips = null;
                uopVector ip = new uopVector(aBox.Center);
                int cnt = 0;
                uopVector ip1 = null;
                uopVector ip2 = null;
                dxoInstances insts = null;
                bool stddesign = aAssy.DesignFamily.IsStandardDesignFamily();

                dxePolygon pgon = mdPolygons.DCBox_View_Plan(aPart: aBox, aAssy: aAssy, bOutLineOnly: bOutLineOnly, bSuppressHoles: bSuppressHoles, bIncludeSpouts: false, bShowObscured: bShowObscured, aCenterLineLength: aCenterLineLength, aCenter: dxfVector.Zero, aRotation: 0, aLayerName: aLayerName, aScale: 1, bIncludeEndPlates: bIncludeEndPlates, bIncludeEndSupports: bIncludeEndSupports, bIncludeShelfAngles: bIncludeShelfAngles, bIncludeStiffeners: false, bIncludeBaffles: false, bIncludeSupDefs: bIncludeSupDefs);
                _rVal = pgon.Block(_rVal.Name, aImage: aImage, aLTLSettings: dxxLinetypeLayerFlag.ForceToColor, aLayerName: aLayerName);
                pgon.Dispose();
                List<mdStiffener> stiffeners = bIncludeStiffeners || bIncludeFingerClips ? aBox.Stiffeners(true) : null;
                if (bIncludeStiffeners)
                {

                    // add these as inserts


                    if (stiffeners.Count > 0)
                    {
                        dxfBlock stiffblock = Stiffener_View_Plan(aImage, aAssy, aBox: aBox, aLayerName: "STIFFENERS");
                        mdStiffener stf1 = stiffeners[0];
                        uopVector u0 = stf1.Center - aBox.Center;
                        dxeInsert stfins = new dxeInsert(stiffblock, u0) { Tag = "STIFFENER" };
                        TINSTANCES instsT = stf1.Instances_Get();
                        dxoInstances dxinsts = stfins.Instances;
                        for (int i = 1; i <= instsT.Count; i++)
                        {
                            dxinsts.Add(aXOffset: 0, aYOffset: instsT.Item(i).DY);
                        }
                        stfins.Instances = dxinsts;
                        _rVal.Entities.Add(stfins);
                    }
                }


                if (bIncludeFingerClips)
                {
                    UVECTORS clippts = aBox.FingerClipPts(aAssy, null);
                    if (clippts.Count > 0)
                    {
                        clippts.Move(-ip.X, -ip.Y);
                        mdFingerClip clip = aAssy.FingerClip;

                        ip1 = new uopVector(clippts.Item(1));
                        subblock = mdBlocks.FingerClip_View_Plan(aImage, clip);
                        insts = null;
                        for (int i = 1; i <= clippts.Count; i++)
                        {
                            ip2 = new uopVector(clippts.Item(i));
                            if (i == 1)
                            {
                                ins = new dxeInsert(subblock, ip2) { Tag = "FINGER CLIP", Flag = clip.PartNumber, RotationAngle = ip2.X < 0 ? 0 : 180 };
                                insts = ins.Instances;
                                ip1 = new uopVector(ip2);
                            }
                            else
                            {

                                insts.Add(ip2.X - ip1.X, ip2.Y - ip1.Y, aRotation: ip2.X < 0 ? 0 : 180);
                            }

                            // ins.Instances.Add(ip2.X - ip1.X, ip2.Y - ip1.Y, aRotation: ip2.X <0 ? 0 : 180);
                        }
                        ins.Instances = insts;
                        _rVal.Entities.Add(ins);
                    }

                }

                if (bIncludeEndAngles)
                {

                    List<mdEndAngle> angles = aBox.EndAngles(aAssy != null ? aAssy.MDProject : proj).FindAll((x) => !x.IsVirtual); // mdPartGenerator.EndAngles_DCBox(aBox, false).FindAll((x)=> !x.IsVirtual);
                    foreach (mdEndAngle ea in angles)
                    {


                        uinsts = ea.Instances;
                        ips = uinsts.MemberPoints(bVirtual: false, aPartIndex: null);

                        if (ips.Count <= 0) continue;

                        ips.Move(-aBox.X, -aBox.Y);
                        cnt = 0;
                        double rightlim = aBox.Width / 2 + 1;
                        double leftlim = -rightlim;
                        for (int i = 1; i <= ips.Count; i++)
                        {
                            ip2 = new uopVector(ips.Item(i));

                            if (ip2.X < leftlim)
                                continue;
                            if (ip2.X > rightlim)
                                continue;
                            cnt++;
                            if (cnt == 1)
                            {
                                subblock = mdBlocks.EndAngle_View_Plan(aImage, ea, bSuppressHoles: bSuppressHoles);
                                ip1 = new uopVector(ip2);
                                ins = new dxeInsert(subblock, ip2) { Tag = "END ANGLE", Flag = ea.PartNumber, RotationAngle = ip2.X < 0 ? 0 : 180 };
                                insts = ins.Instances;
                            }
                            else
                            {

                                insts.Add(ip2.X - ip1.X, ip2.Y - ip1.Y, aRotation: ip2.X < 0 ? 0 : 180);
                            }

                        }
                        ins.Instances = insts;
                        _rVal.Entities.Add(ins);
                    }

                }

                if (bIncludeBaffles)
                {
                    List<mdBaffle> baffles = proj != null ? proj.GetParts().DeflectorPlates(aAssy.RangeGUID).FindAll((x) => x.BoxIndex == aBox.Index && !x.IsBlank) : mdPartGenerator.DeflectorPlates_DCBox(aBox, aAssy, bSuppressInstances: true);
                    foreach (mdBaffle b in baffles)
                    {
                        subblock = mdBlocks.DeflectorPlate_View_Plan(aImage, b, aAssy, out _, bSuppressHoles: false, aLayerName: "DEFLECTOR PLATES", bDontAddToImage: false);

                        ips = b.Instances.MemberPoints();
                        ips.Move(-aBox.X, -aBox.Y);
                        cnt = 0;
                        for (int i = 1; i <= ips.Count; i++)
                        {
                            ip2 = ips.Item(i);
                            if (Math.Round(ip2.X, 2) != 0) continue;
                            cnt++;
                            if (cnt == 1)
                            {
                                ip1 = ip2;
                                ins = new dxeInsert(subblock, ip1) { Tag = "DEFLECTOR PLATE", Flag = b.PartNumber, LayerName = "DEFLECTOR PLATES" };
                                insts = ins.Instances;

                            }
                            else
                            {
                                insts.Add(ip2.X - ip1.X, ip2.Y - ip1.Y, aRotation: ip2.Rotation);
                            }
                        }
                        if (cnt > 0) ins.Instances = insts;
                        _rVal.Entities.Add(ins);
                    }
                    if (bIncludeSpouts)
                    {
                        dxfDisplaySettings dsp = dxfDisplaySettings.Null("SPOUTS", dxxColors.ByLayer);
                        List<mdSpoutGroup> sgs = aAssy.SpoutGroups.GetByVirtual(aVirtualValue: false).FindAll((x) => x.DowncomerIndex == aBox.DowncomerIndex && x.BoxIndex == aBox.Index);
                        foreach (var sg in sgs)
                        {
                            if (sg.SpoutCount(aAssy) <= 0) continue;
                            uopVector sgip = new uopVector(sg.X, sg.Y);
                            dxfBlock block = aImage.Blocks.Add(sg.Block(dsp: dsp, aImage: aImage, bSuppressInstances: true)); // mdBlocks.SpoutGroup_View_Plan(aImage, sg, aAssy, out _, aLayerName: "SPOUTS", bDontAddToImage: false); // 
                            if (block.Entities.Count <= 0) continue;
                            dxeInsert sginsert = new dxeInsert(block, sgip - ip) { Tag = "SPOUT GROUP", Flag = sg.Handle, DisplaySettings = dsp };
                            if (stddesign)
                            {
                                List<uopInstance> sginsts = sg.Instances.FindAll((x) => x.DX == 0);
                                foreach (var sginst in sginsts)
                                {
                                    sginsert.Instances.Add(sginst.ToDXFInstance());
                                }
                            }

                            _rVal.Entities.Add(sginsert);
                        }
                    }
                    //    if (bIncludeSpouts)
                    //{
                    //    colMDSpoutGroups sgs = aBox.SpoutGroups(aAssy);
                    //    foreach(var sg in sgs)
                    //    {
                    //        subblock = mdBlocks.SpoutGroup_View_Plan(aImage, sg, aAssy, out _,  aLayerName: "SPOUTS", bDontAddToImage: false);

                    //        ips = sg.Instances.MemberPoints();
                    //        ips.Move(-sg.X, -sg.Y);
                    //        cnt = 0;
                    //        for (int i = 1; i <= ips.Count; i++)
                    //        {
                    //            ip2 = ips.Item(i);
                    //            if (Math.Round(ip2.X, 2) != 0) continue;
                    //            cnt++;
                    //            if (cnt == 1)
                    //            {
                    //                ip1 = ip2;
                    //                ins = new dxeInsert(subblock, ip1) { Tag = "SPOUTS", Flag = sg.Handle, LayerName = "SPOUTS" };
                    //                insts = ins.Instances;

                    //            }
                    //            else
                    //            {
                    //                insts.Add(ip2.X - ip1.X, ip2.Y - ip1.Y, aRotation: ip2.Rotation);
                    //            }
                    //        }
                    //        if (cnt > 0) ins.Instances = insts;
                    //        _rVal.Entities.Add(ins);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);
            }
            finally
            {
                if (_rVal != null)
                {
                    _rVal.Instances = aBox.Instances.ToDXFInstances();
                    if (!bSetInstances) _rVal.Instances.Clear();
                }
                if (aRotation != 0)
                {
                    _rVal.Entities.RotateAbout(dxfDirection.WorldZ, aRotation);
                }
            }

            return _rVal;


        }


        public static dxfBlock Slot_Zone(dxfImage aImage, mdSlotZone aZone, string aLayerName = null, bool bIncludeSectionPerimeter = false, bool bIncludeBounds = true, bool bIncludeBlockedAreas = true, bool bIncludeMirrorLine = false, bool bIncludeWeirLines = false,bool bIncludeOrginCircle = false, bool bIncludeHandle = false, bool bIncludeSupressed = false, bool bIncludeGridLines = false, bool bDontAddToImage = false, bool bAddCenterPoints = true)
        {

            if (aZone == null) return null;
            aZone.UpdateSlotPoints();

            dxfBlock _rVal = new dxfBlock($"SLOT ZONE {aZone.Handle.Replace(",", "_")}") { LayerName = string.IsNullOrWhiteSpace(aLayerName) ? "SLOT_ZONES" : aLayerName.Trim() };
            dxfDisplaySettings dsp = dxfDisplaySettings.Null(_rVal.LayerName, dxxColors.Blue);
            dxePolyline bounds = new dxePolyline(aZone, dsp) { Tag = "BOUNDS" };
            if(bIncludeBounds)_rVal.Entities.Add(bounds);
            if (bIncludeSectionPerimeter) _rVal.Entities.AddShape(aZone.BaseShape.SimplePerimeter, dxfDisplaySettings.Null("SECTION PERIMETERS", dxxColors.ByLayer), aTag:"SECTION PERIMETER");

           if (bIncludeBlockedAreas) _rVal.Entities.Append(aZone.Islands.ToDXFEntities("BLOCKED AREAS", dxxColors.Red), aTag: "ISLANDS");

            bool? supval = null;
            if (!bIncludeSupressed) supval = false;
            uopVectors gridpts = aZone.GridPoints(supval);
            if (bIncludeMirrorLine) _rVal.Entities.AddLine(aZone.MirrorLine, dsp, aTag: "MIRROR LINE");
            if (bIncludeOrginCircle && gridpts.Count > 0)
            {
                _rVal.Entities.Add(new dxeArc(aZone.GridOrigin, aImage != null ? 0.0325 * aImage.Display.PaperScale : 0.25, aDisplaySettings: dsp));
            }
            if(bIncludeHandle)
            {
                if (aImage != null)
                    _rVal.Entities.Add(aImage.EntityTool.Create_Text(aZone.Center, aZone.Handle, aImage.Display.PaperScale * 0.125, aAlignment: dxxMTextAlignments.MiddleCenter), aTag:"ZONE HANDLE");
                else
                    _rVal.Entities.Add(new dxeText(dxxTextTypes.Multiline, dsp) { InsertionPt = new dxfVector(aZone.Center), TextString = aZone.Handle, TextHeight = 1, Alignment = dxxMTextAlignments.MiddleCenter, Tag = "ZONE HANDLE" });
            }
            if (bIncludeGridLines) 
            {
                _rVal.Entities.AddLines(aZone.RowLines(), dxfDisplaySettings.Null("ROW_LINES", dxxColors.LightGreen), aTag:"ROW LINE");
                _rVal.Entities.AddLines(aZone.ColumnLines(), dxfDisplaySettings.Null("COL_LINES", dxxColors.LightGreen), aTag: "COLUMN LINE");
            }

            if(gridpts.Count > 0)
            {
                dxfBlock blok = null;
                dxeInsert insert1 = null;
                if (supval.HasValue)
                {
                    uopRectangles recs = aZone.SlotRectangles(supval);
                    if(recs.Count > 0)
                    {
                        insert1 = uopRectangles.BuildBlock(aImage, supval.Value == false ?  $"ECMD_SLOT" : $"SUPPRESSED_ECMD_SLOT", recs, recs.First(), ref blok, dxfDisplaySettings.Null(supval.Value == false ? "ECMD SLOTS" : "SUPPRESSED ECMD SLOTS", supval.Value == false ? dxxColors.Grey : dxxColors.Yellow, dxfLinetypes.Continuous), "SLOT RECTANGLE", null, bAddCenterPoints);
                        _rVal.Entities.Add(insert1);
                    }

            }
            else
            {
                    uopRectangles recs = aZone.SlotRectangles(null);
                    List<uopRectangle> unsupr = recs.FindAll(x => !x.Suppressed);
                    List<uopRectangle> supr = recs.FindAll(x => x.Suppressed);

                    if (unsupr.Count > 0)
                    {
                        insert1 = uopRectangles.BuildBlock(aImage, $"ECMD_SLOT", unsupr, unsupr.First(), ref blok, dxfDisplaySettings.Null("ECMD SLOTS",  dxxColors.Grey, dxfLinetypes.Continuous), "SLOT RECTANGLE", null, bAddCenterPoints);
                        _rVal.Entities.Add(insert1);
                    }
                    

             
                    if(supr.Count > 0)
                    {
                        blok = null;
                        insert1 = uopRectangles.BuildBlock(aImage, $"SUPPRESSED_ECMD_SLOT", supr, supr.First(), ref blok, dxfDisplaySettings.Null("SUPPRESSED ECMD SLOTS",  dxxColors.Yellow, dxfLinetypes.Continuous), "SLOT RECTANGLE", null, bAddCenterPoints);
                        _rVal.Entities.Add(insert1);
                    }

                }

              

            }

            if (bIncludeWeirLines)
            {
                uopLinePair weirs = aZone.WeirLines;
                uopLine weir = weirs.GetSide(uppSides.Left);
                if(weir != null)  _rVal.Entities.AddLine(weir, dxfDisplaySettings.Null("WEIR LINES", dxxColors.Blue));
                weir = weirs.GetSide(uppSides.Right);
                if (weir != null) _rVal.Entities.AddLine(weir, dxfDisplaySettings.Null("WEIR LINES", dxxColors.Magenta));
            }
            _rVal.Entities.Translate(aZone.Center * -1);


            if (!bDontAddToImage &&  aImage != null)  _rVal = aImage.Blocks.Add(_rVal);
            return _rVal;


        }

        public static dxfBlock DeckSection_View_Plan(dxfImage aImage, mdDeckSection aPart, mdTrayAssembly aAssy, string aBlockName, bool bSetInstances, bool bObscured = false, bool bIncludePromoters = true, bool bIncludeHoles = true, bool bIncludeSlotting = true, bool bRegeneratePerimeter = false, string aLayerName = "GEOMETRY", bool bHalfSideSlots = false, bool bRegenSlots = false, bool bSolidHoles = false, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor, double aRotation = 0, bool bShowPns = false, bool bShowQuantities = false, bool bDontAddToImage = false)
        {

            if (aPart == null) return null;
            aAssy ??= aPart.GetMDTrayAssembly();
            if (aAssy == null) return null;
            dxfBlock _rVal = null;
            string pn = aPart.PartNumber;

            try
            {
                if (!aAssy.DesignOptions.HasBubblePromoters) bIncludePromoters = false;
                bool hasslots = aAssy.IsECMD && bIncludeSlotting;
                uopVectors slPts = null;

                if (hasslots)
                {
                    slPts = aPart.SlotCenters(aAssy, out mdSlotZone aZone, bRegenSlots: bRegenSlots, bReturnClones: true);
                    hasslots = slPts.Count > 0;
                }
                bool slotsasinserts = bIncludeSlotting && aImage != null && hasslots;
                dxePolygon pg = mdPolygons.DeckSection_View_Plan(aPart, aAssy, bObscured, bIncludePromoters, bIncludeHoles, bIncludeSlotting: hasslots && !slotsasinserts, bRegeneratePerimeter: bRegeneratePerimeter, aCenter: dxfVector.Zero, aRotation: 0, aLayerName, bHalfSideSlots, bRegenSlots, bSolidHoles);
                aBlockName = string.IsNullOrWhiteSpace(aBlockName) ? $"DECK_SECTION_{pn}_PLAN_VIEW" : aBlockName.Trim();
                _rVal = pg.Block(aBlockName, aImage: aImage, aLTLSettings: aLTLSetting, aLayerName: aLayerName);
                pg.Dispose();

                if (slotsasinserts)
                {
                    uopVector cp = new uopVector(aPart.Center);
                    dxfBlock cslot = null;
                    string bname = $"FLOW_ SLOT";
                    if (!aImage.Blocks.TryGet(bname, ref cslot))
                        cslot = aImage.Blocks.Add(aAssy.FlowSlot.Block(uppPartViews.Top, aImage, aBlockName: bname, aLTLSetting: aLTLSetting, aLayer:"ECMD SLOTS"));
                    uopVector u1 = slPts.Item(1);

                    if (aImage.UsingDxfViewer)
                    {
                        dxeInsert sltinrt = new dxeInsert(cslot, u1 - cp);
                        dxoInstances insts = sltinrt.Instances;

                        for (int i = 2; i <= slPts.Count; i++)
                        {
                            uopVector u2 = slPts.Item(i);
                            insts.Add(u2.X - u1.X, u2.Y - u1.Y, aRotation: u2.Rotation);

                        }
                        _rVal.Entities.Add(sltinrt);

                    }
                    else
                    {
                        foreach (var v in slPts)
                        {
                            u1 = v - cp;
                            _rVal.Entities.Add(new dxeInsert(cslot, u1, 1, v.Rotation));

                        }

                    }

                }

                if (bIncludePromoters)
                {
                    List<dxeArc> promoters = _rVal.Entities.Arcs().FindAll((x) => string.Compare(x.Tag, "BUBBLE PROMOTER", true) == 0);
                    if (promoters.Count > 0)
                    {
                        //convert bubble promoter circles to inserts w/ instances
                        _rVal.Entities.RemoveMembers(promoters);

                        dxeArc promoter = new dxeArc(promoters[0]) { X = 0, Y = 0, LayerName = pg.LayerName };

                        dxfBlock subblock = new dxfBlock("BUBBLE PROMOTER", aEntities: new List<dxfEntity>() { promoter });
                        if (aImage != null) subblock = aImage.Blocks.Add(subblock);
                        promoter = promoters[0];
                        uopVector ip1 = new uopVector(promoter.X, promoter.Y);
                        dxeInsert bpinsert = new dxeInsert(subblock, ip1) { Tag = "BUBBLE PROMOTER", LayerName = pg.LayerName };
                        dxoInstances insts = bpinsert.Instances;
                        for (int i = 2; i <= promoters.Count; i++)
                        {
                            promoter = promoters[i - 1];
                            insts.Add(promoter.X - ip1.X, promoter.Y - ip1.Y);
                        }
                        bpinsert.Instances = insts;
                        _rVal.Entities.Add(bpinsert);
                    }
                }


            }
            catch (Exception ex) { if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex); }
            finally
            {
                //_rVal.InsertionPt = new dxfVector(aPart.Center);
                if (bShowPns || bShowQuantities)
                {
                    double tht = aImage == null ? aImage.Display.PaperScale * 0.75 : 0.75;
                    string label = bShowPns ? pn : string.Empty;
                    if (bShowQuantities)
                    {
                        label += $"\\POCCR: {aPart.OccuranceFactor}\\PQTY:{aPart.Quantity}";
                    }
                    dxeText mtext = aImage == null ? new dxeText(dxxTextTypes.Multiline) { InsertionPt = dxfVector.Zero, Alignment = dxxMTextAlignments.MiddleCenter, TextString = label, TextHeight = tht, Color = dxxColors.Cyan, LayerName = "PART_NUMBERS" } : aImage.EntityTool.Create_Text(null, label, tht, dxxMTextAlignments.MiddleCenter, aLayer: "PART_NUMBERS", aColor: aImage.TextSettings.Color,bSuppressUCS:true, bSuppressElevation:true);
                    _rVal.Entities.Add(mtext);
                }
            }

            _rVal.Instances = aPart.Instances.ToDXFInstances();
            if (!bSetInstances) _rVal.Instances.Clear();
            if (aRotation != 0) _rVal.Instances.BasePlane = new dxfPlane(_rVal.Instances.BasePlane.Origin, aRotation);
            if (!bDontAddToImage && aImage != null)
            {
                aImage.LinetypeLayers.ApplyTo(_rVal, dxxLinetypeLayerFlag.ForceToColor, aImage);
                _rVal = aImage.Blocks.Add(_rVal);
            }
            //{
            //    _rVal.Entities.RotateAbout(dxfDirection.WorldZ, aRotation);
            //}
            return _rVal;

        }
    
            
     


        /// <summary>
        /// Creates a block for the beam support view plan. Origin is the left end of the center of the beam
        /// </summary>
        /// <param name="aImage"></param>
        /// <param name="aPart"></param>
        /// <param name="aAssy"></param>
        /// <param name="aBlockName"></param>
        /// <param name="rInsertionPts"></param>
        /// <param name="bObscured"></param>
        /// <param name="aLayerName"></param>
        /// <param name="aLTLSetting"></param>
        /// <returns></returns>
        public static dxfBlock BeamSupport_View_Plan(
            dxfImage aImage,
            mdBeam aPart,
            mdTrayAssembly aAssy,
            string aBlockName,
            bool bSetInstances = false,
            bool bObscured = false,
            bool bSuppressHoles = false,
            string aLayerName = "BEAM SUPPORTS",
            dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor)
        {
            if (aPart == null) return null;
            aAssy ??= aPart.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return null;


            var pg = mdPolygons.BeamSupport_View_Plan(aPart.BeamSupport(), aAssy, bShowObscured: bObscured, bSuppressHoles: bSuppressHoles, aLayerName: aLayerName);
            if (string.IsNullOrWhiteSpace(aBlockName)) aBlockName = $"BEAM_{aPart.PartNumber}_SUPPORT_VIEW_PLAN";
            dxfBlock _rVal = pg.Block(aBlockName, aImage: aImage, aLTLSettings: aLTLSetting, aLayerName: aLayerName);
            _rVal.Instances = new dxoInstances(aPart.Center);
            if (bSetInstances)
            {
                uopVector p1 = aPart.BeamSupportInsertionPoint(bLeft: true);
                uopVector p2 = aPart.BeamSupportInsertionPoint(bLeft: false);
                _rVal.Instances.Add(p2.X -p1.X, p2.Y - p1.Y,  bInverted:true);
            }
            
            return _rVal;
        }

        public static dxfBlock BeamSupport_View_Elevation(
            dxfImage aImage,
            mdBeam aPart,
            mdTrayAssembly aAssy,
            string aBlockName,
            out uopVectors rInsertionPts,
            bool bObscured = false,
            bool bSuppressHoles = false,
            string aLayerName = "GEOMETRY",
            dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor)
        {
            rInsertionPts = new uopVectors();
            if (aPart == null) return null;
            aAssy ??= aPart.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return null;

            var pg = mdPolygons.BeamSupport_View_Elevation(aPart.BeamSupport(), aAssy, bShowObscured: bObscured, bSuppressHoles: bSuppressHoles, aLayerName: aLayerName);

            dxfBlock _rVal = pg.Block(aBlockName, aImage: aImage, aLTLSettings: aLTLSetting, aLayerName: aLayerName);

            rInsertionPts.Add(aPart.BeamSupportInsertionPoint(true).Rotated(-45));
            rInsertionPts.Add(aPart.BeamSupportInsertionPoint(false).Rotated(-45));
            return _rVal;
        }

        public static dxfBlock BeamSupport_View_Elevation_End(
            dxfImage aImage,
            mdBeam aPart,
            mdTrayAssembly aAssy,
            string aBlockName,
            bool bObscured = false,
            bool bSuppressHoles = false,
            string aLayerName = "GEOMETRY",
            dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor)
        {
            if (aPart == null) return null;
            aAssy ??= aPart.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return null;

            var pg = mdPolygons.BeamSupport_View_Elevation_End(aPart.BeamSupport(), aAssy, bShowObscured: bObscured, bSuppressHoles: bSuppressHoles, aLayerName: aLayerName);

            dxfBlock _rVal = pg.Block(aBlockName, aImage: aImage, aLTLSettings: aLTLSetting, aLayerName: aLayerName);

            return _rVal;
        }
    
    public static dxfBlock Beam_View_Plan(
            dxfImage aImage,
            mdBeam aPart,
            mdTrayAssembly aAssy,
            bool bSetInstances = false,
            bool bObscured = false,
            bool bSuppressHoles = false,
            double aCenterLineLength = 0,
            string aLayerName = "GEOMETRY",
            dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor,
            string aBlockNameSuffix = null,
            bool bDontAddToImage = false,
            bool bIncludeSupports = false)
        {
           
            if (aPart == null) return null;
           
            string bname = $"SUPPORT_BEAM_{aPart.PartNumber}_PLAN_VIEW";
            if (bIncludeSupports) bname += "_WITH_SUPPORTS";
            if (!string.IsNullOrWhiteSpace(aBlockNameSuffix)) bname += aBlockNameSuffix.Trim();

            dxfBlock _rVal = aImage == null ? null : aImage.Blocks.Item(bname);

            if (_rVal != null) return _rVal;
            dxePolygon pgon = null;


            try
            {
               pgon = mdPolygons.Beam_View_Plan(aPart, aAssy, aCenter: uopVector.Zero, bShowObscured: bObscured, bSuppressHoles: bSuppressHoles, aLayerName: aLayerName);
                  _rVal = pgon.Block(bname,aImage:aImage,aLTLSettings: aLTLSetting, aLayerName:aLayerName);

                if (bIncludeSupports)
                {
                    dxfBlock beamblock = _rVal;
                    beamblock.Name = $"SUPPORT_BEAM_{aPart.PartNumber}_PLAN_VIEW";
                    if (aImage != null)
                    {
                        aImage.LinetypeLayers.ApplyTo(beamblock, aLTLSetting);
                        if (!bDontAddToImage) beamblock = aImage.Blocks.Add(beamblock);
                    }

                    _rVal = new dxfBlock(bname);
                    dxeInsert insert = new dxeInsert(beamblock);
                    _rVal.Entities.Add(insert);

                    //define aPart support block
                    string subblockname = $"BEAM_{aPart.PartNumber}_SUPPORT_PLAN_VIEW";
                    dxfBlock block = null;
                    uopVectors ips = aPart.BeamSupportInsertionPoints;
                    if (aImage != null)
                    {
                        if (!aImage.Blocks.TryGet(subblockname, ref block))
                        {
                            block = mdBlocks.BeamSupport_View_Plan(aImage, aPart, aAssy, subblockname, bObscured: bObscured);
                            if (aImage != null)
                            {
                                aImage.LinetypeLayers.ApplyTo(block, aLTLSetting);
                                if (!bDontAddToImage) block = aImage.Blocks.Add(block);
                            }
                        }
                    }
                    else
                    {
                        block = mdBlocks.BeamSupport_View_Plan(aImage, aPart, aAssy, subblockname, bObscured: bObscured);

                    }

                    uopHole mount = aPart.MountSlot;
                    dxfPlane plane = new dxfPlane(aPart.Plane, uopVector.Zero);

                    var lip = plane.Vector(-0.5 * aPart.Length + mount.Inset, 0);
                    var rip = plane.Vector(0.5 * aPart.Length - mount.Inset, 0);

                    //lip.Y = 0;
                    //rip.Y = 0;

                    insert = new dxeInsert(block, lip, aRotation: aPart.Rotation) { Tag = "BEAM SUPPORT", DisplaySettings = dxfDisplaySettings.Null(block.LayerName) };
                    dxoInstances insts = insert.Instances;
//                    insts.Add(rip.X - lip.X, rip.Y - lip.Y, aRotation: aPart.Rotation + 180);
                    insts.Add(rip.X - lip.X, rip.Y - lip.Y, aRotation: aPart.Rotation, bLeftHanded:true);
                    insert.Instances = insts;
                    _rVal.Entities.Add(insert);
                }   


            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);

            }
            finally
            {
                pgon?.Dispose();
                if (_rVal != null)
                {

                    _rVal.Instances = new dxoInstances(aPart.Center);
                    if (bSetInstances && aPart.OccuranceFactor >1)
                      _rVal.Instances.Add(-2* aPart.X, -2 * aPart.Y, aRotation: 180);

                    _rVal.Name = bname;
                   if(aImage != null)
                    {
                        aImage.LinetypeLayers.ApplyTo(_rVal, aLTLSetting);
                        if (!bDontAddToImage) _rVal = aImage.Blocks.Add(_rVal);
                    }
                }


            }
            return _rVal;
        }

    
        public static dxfBlock DeflectorPlate_View_Plan(
            dxfImage aImage,
            mdBaffle aPart,
            mdTrayAssembly aAssy,
            out uopVectors rInsertionPts,
            bool bSuppressHoles = false,
            string aLayerName = "DEFLECTOR PLATES",
            dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor,
            string aBlockNameSuffix = null,
            bool bDontAddToImage = false)
        {
            rInsertionPts = uopVectors.Zero;
            if (aPart == null) return null;
            rInsertionPts = aPart.Instances.MemberPoints();

            string bname = $"DEFLECTOR_PLATE_{aPart.PartNumber}_PLAN_VIEW";
            if (!string.IsNullOrWhiteSpace(aBlockNameSuffix)) bname += aBlockNameSuffix.Trim();

            dxfBlock _rVal = aImage == null ? null : aImage.Blocks.Item(bname);

            if (_rVal != null) return _rVal;


            dxePolygon pgon = mdPolygons.Baffle_View_Plan(aPart, aAssy, bSuppressHoles: bSuppressHoles,aCenter:  uopVector.Zero,  aLayerName: aLayerName);
            try
            {
                _rVal = pgon.Block(bname, aImage: aImage, aLTLSettings: aLTLSetting, aLayerName: aLayerName);
            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);

            }
            finally
            {
                pgon?.Dispose();
                if (_rVal != null)
                {

                    _rVal.Name = bname;
                    if (aImage != null)
                    {
                        aImage.LinetypeLayers.ApplyTo(_rVal, aLTLSetting);
                        if (!bDontAddToImage) _rVal = aImage.Blocks.Add(_rVal);
                    }
                }


            }
            return _rVal;
        }

        public static dxfBlock DeckPanels_View_Plan(dxfImage aImage, mdTrayAssembly aAssy, string aLayerName = "DECK PANELS", dxxColors aColor = dxxColors.ByLayer, string aLineType = null, string aBlockNameSuffix = null,
           bool bDontAddToImage = false,
           bool bBothSides = true,
           bool bForTrayBelow = false,
           bool bIncludeClearance = false, List<uopPanelSectionShape> aPanels = null)
        {
            if (aAssy == null) return null;
            dxfBlock _rVal = null;
            uppMDDesigns family = aAssy.DesignFamily;
            string bname = $"DECK_PANELS_{aAssy.TrayName(false).ToUpper()}_PLAN_VIEW";
            if (string.IsNullOrWhiteSpace(aLineType)) aLineType = dxfLinetypes.ByLayer;
            if (!string.IsNullOrWhiteSpace(aBlockNameSuffix)) bname += aBlockNameSuffix.Trim();
            try
            {
                _rVal = new dxfBlock(bname) { LayerName = aLayerName};
                List<uopPanelSectionShape> panels = aPanels == null ? aAssy.DowncomerData.PanelShapes(bIncludeClearance) : aPanels;
                dxfDisplaySettings dsp = new dxfDisplaySettings(aLayerName, aColor, aLineType);
                foreach (var panel in panels)
                {
                    dxePolyline pl = new dxePolyline(panel, dsp);
                    if (bForTrayBelow)
                    {
                        pl.RotateAbout(dxfVector.Zero, family.IsStandardDesignFamily() ? 90 : - 90);
                        //if (!family.IsStandardDesignFamily())
                        //{
                        //    pl.RotateAbout(pl.Plane.YAxis(), 180);
                        //}
                    }
                    _rVal.Entities.Add(pl);
                    if (bBothSides && panel.OccuranceFactor  >1)
                    {
                        uopVectors verts = new uopVectors(panel);
                        uopInstance inst = panel.Instances[0];
                        verts = inst.ApplyTo(verts, panel.Center);
                        pl = new dxePolyline(verts,true,dsp);
                        if (bForTrayBelow)
                        {
                            pl.RotateAbout(dxfVector.Zero, family.IsStandardDesignFamily() ? 90 : -90);
                            if (!family.IsStandardDesignFamily())
                            {
                                //pl.RotateAbout(pl.Plane.YAxis(), 180);
                            }
                        }
                        _rVal.Entities.Add(pl);

                    }
                }
               
              

            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);

            }
            finally
            {
                if (_rVal != null)
                {

                    _rVal.Name = bname;
                    if (aImage != null)
                    {
                        if (!bDontAddToImage) _rVal = aImage.Blocks.Add(_rVal);
                    }
                }


            }

            return _rVal;
        }



        public static dxfBlock DowncomersBelow_View_Plan(
           dxfImage aImage,
           mdTrayAssembly aAssy,
            string aLayerName = "DOWNCOMERS_BELOW",
            dxxColors? aColor = null,
         string aLineType = "Hidden",
        string aBlockNameSuffix = null,
           bool bDontAddToImage = false,
           bool bBothSides = true)
        {
            if (aAssy == null) return null;
            dxfBlock _rVal = null;
            string bname = $"DOWCOMERS_BELOW_{aAssy.TrayName(false).ToUpper()}_PLAN_VIEW";
            try
            {
                uppMDDesigns family = aAssy.DesignFamily;
                List<mdDowncomerBox> boxes = aAssy.Downcomers.Boxes();
                dxfDisplaySettings dsp = null;
                if (string.IsNullOrWhiteSpace(aLayerName)) aLayerName = "DOWNCOMERS_BELOW"; else aLayerName = aLayerName.Trim();
                if (aImage != null)
                {
                    if (!aColor.HasValue) aColor = aImage.LinetypeLayers.LineColor("Hidden", aDefaultReturn: dxxColors.Green);
                    aImage.Layers.Add(aLayerName, aColor.Value, dxfLinetypes.Hidden);
                    dsp = dxfDisplaySettings.Null(aLayer: aLayerName, aColor: dxxColors.ByLayer, aLinetype: "ByLayer");

                }
                else
                {
                    if (!aColor.HasValue) aColor = dxxColors.Green;
                    if (string.IsNullOrWhiteSpace(aLineType)) aLineType = dxfLinetypes.Hidden;
                    dsp = dxfDisplaySettings.Null(aLayer: aLayerName, aColor: aColor.Value, aLinetype: aLineType);
                }

            

                if (!string.IsNullOrWhiteSpace(aBlockNameSuffix)) bname += aBlockNameSuffix.Trim();
                _rVal = new dxfBlock(bname) { LayerName = dsp.LayerName };


                dxeLine xaxis = new dxeLine(new dxfVector(-100, 0), new dxfVector(100, 0));
                if (!family.IsStandardDesignFamily()) xaxis.RotateAbout(dxfPlane.World.ZAxis(100), 90);
                if (family.IsDividedWallDesignFamily()) bBothSides = false;

                foreach (var box in boxes)
                {
                    colDXFVectors verts = box.Vertices(true);
                    if (family.IsBeamDesignFamily())
                    {
                        verts.Mirror(xaxis);
                        verts.MirrorPlanar(null, 0);
                    }

                    _rVal.Entities.Add(new dxePolyline(verts, true, aDisplaySettings: dsp));
                    if (!bBothSides) continue;
                    if (box.OccuranceFactor == 2)
                    {
                        if (family.IsBeamDesignFamily())
                        {
                            verts.Mirror(xaxis);
                            verts.MirrorPlanar(null, 0);
                        }
                        else
                        {
                            verts.Mirror(xaxis);
                        }
                        _rVal.Entities.Add(new dxePolyline(verts, true, aDisplaySettings: dsp));
                    }
                }
            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);

            }
            finally
            {
                if (_rVal != null)
                {
                    dxfUtils.ValidateBlockName(ref bname, bFixIt: true, false, true, aImage.Blocks);
                    _rVal.Name = bname;
                    if (aImage != null)
                    {
                        if (!bDontAddToImage) _rVal = aImage.Blocks.Add(_rVal);
                    }
                }


            }

            return _rVal;
        }

        public static dxfBlock SpoutGroup_View_Plan(
    dxfImage aImage,
    mdSpoutGroup aPart,
    mdTrayAssembly aAssy,
    bool bSetInstances = false,
    bool bSuppressCenterPoints = false,
    string aLayerName = "SPOUTS",
    dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.ForceToColor,
    string aBlockNameSuffix = null,
    bool bDontAddToImage = false)
        {
       
            if (aPart == null) return null;
       
            string bname = $"SPOUTGROUP_{aPart.Handle.Replace(',','_')}_PLAN_VIEW";
            if (!string.IsNullOrWhiteSpace(aBlockNameSuffix)) bname += aBlockNameSuffix.Trim();

            dxfBlock _rVal = aImage == null ? null : aImage.Blocks.Item(bname);

            if (_rVal != null) return _rVal;


            
            try
            {
                uopHoles spouts = aPart.Spouts;
               colDXFEntities blkEnts = spouts.Entities(aLayerName, dxxColors.ByLayer, dxfLinetypes.Continuous, aImage: aImage, bSuppressInstances: aPart.PatternType == uppSpoutPatterns.SStar,bSuppressCenterPoints:bSuppressCenterPoints);
                blkEnts.Move(-aPart.X,-aPart.Y);
                _rVal = new dxfBlock(bname, aEntities: blkEnts) ;
                return _rVal;

            }
            catch (Exception ex)
            {
                if (aImage != null) aImage.HandleError(MethodBase.GetCurrentMethod(), "mdBlocks", ex);

            }
            finally
            {
               
                if (_rVal != null)
                {

                    _rVal.Instances = aPart.Instances.ToDXFInstances();
                    if(!bSetInstances) _rVal.Instances.Clear();
                    _rVal.Name = bname;
                    if (aImage != null)
                    {
                        aImage.LinetypeLayers.ApplyTo(_rVal, aLTLSetting);
                        if (!bDontAddToImage) _rVal = aImage.Blocks.Add(_rVal);
                    }
                }


            }
            return _rVal;
        }
    }
}