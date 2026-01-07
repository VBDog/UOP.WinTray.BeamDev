using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using dxxRoundToLimits = UOP.DXFGraphics.dxxRoundToLimits;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Generators
{
    public class mdStartUps
    {



        /// <summary>
        /// the polygon that is internal to the perimeter polygon and is the absolute max limits of the area
        /// where startup spouts can be placed
        /// same as the max bound less the 4 inch buffer at the ends of the downcomers
        /// </summary>
        /// <param name="aSpoutGroup"></param>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        internal static USHAPE Bounds(mdSpoutGroup aSpoutGroup, mdTrayAssembly aAssy)
        {
            if (aSpoutGroup == null) return USHAPE.Null;

            USHAPE _rVal = new USHAPE(aSpoutGroup.Handle);

            aAssy ??= aSpoutGroup.GetMDTrayAssembly();
            if (aAssy == null) return _rVal;

            mdDowncomerBox box = aSpoutGroup.DowncomerBox;
            if (box == null) return _rVal;
            double thk = box.Thickness;
            ULINEPAIR limlines = box.StartUpLimitLns;
            double wd =  box.Width / 2;
            double cY = aSpoutGroup.PanelY;
            

            ULINE toplim = limlines.GetSideValue(uppSides.Top);
            ULINE botlim = limlines.GetSideValue(uppSides.Bottom);

            ULINE left = new ULINE(new UVECTOR( box.X - wd, aSpoutGroup.PanelBottom - wd), new UVECTOR(box.X - wd, aSpoutGroup.PanelTop + wd));
            ULINE right = new ULINE(new UVECTOR(box.X + wd, aSpoutGroup.PanelBottom - wd), new UVECTOR(box.X + wd, aSpoutGroup.PanelTop + wd));

            
            
            UVECTOR u1 = UVECTOR.Zero;
            UVECTOR u2 = UVECTOR.Zero;
            UVECTOR u3 = UVECTOR.Zero;
            UVECTOR u4 = UVECTOR.Zero;

            u2 = left.IntersectionPt(toplim,out _, out _, out bool onLeft, out _, out bool leftExists) ;
            if (leftExists && onLeft) left.ep.Y = u2.Y;
            u2 = left.IntersectionPt(botlim, out _, out _, out onLeft, out _, out leftExists);
            if (leftExists && onLeft) left.sp.Y = u2.Y;

            u2 = right.IntersectionPt(toplim, out _, out _, out bool onRight, out _, out bool rightExists);
            if (rightExists && onRight) right.ep.Y = u2.Y;
            u2 = right.IntersectionPt(botlim, out _, out _, out onRight, out _, out rightExists);
            if (rightExists && onRight) right.sp.Y = u2.Y;

            UVECTORS verts = new UVECTORS(left.ep, right.ep, right.sp,left.sp);


            _rVal =new USHAPE(verts) { Value = thk};
           
            verts = UVECTORS.Zero;
            if(aSpoutGroup.Direction == dxxOrthoDirections.Up)
            {
                u1 = new UVECTOR(left.X(), left.MaxY) { Tag = "UL"};
                u2 = new UVECTOR(u1.X, cY) { Tag = "LL" };
                u3 = new UVECTOR(u1.X, left.MinY) { Tag = string.Empty };

                if (u1.Y > u2.Y && u2.Y > u3.Y)
                {
                    verts.Add(u1);
                    verts.Add(u2);
                    verts.Add(u3);
                }
                else
                {
                    verts.Add(u2);
                    verts.Add(u3);
                }
                u1 = new UVECTOR(right.X(), right.MinY) { Tag = "LR" };
                u2 = new UVECTOR(u1.X, cY) { Tag = "UR" };
                u3 = new UVECTOR(u1.X, right.MaxY) { Tag = string.Empty };

                if (u1.Y < u2.Y && u2.Y < u3.Y)
                {
                   
                    verts.Add(u1);
                    verts.Add(u2);
                    verts.Add(u3);
                }
                else
                {
                    u2.Y = u1.Y;
                    verts.Add(u2);
                    verts.Add(u3);
                }
            }
            else
            {
                u1 = new UVECTOR(left.X(), left.MaxY) { Tag = "UL" };
                u2 = new UVECTOR(u1.X, cY) { Tag = "LL" };
                u3 = new UVECTOR(u1.X, left.MinY) { Tag = string.Empty };

                if (u1.Y > u2.Y && u2.Y > u3.Y)
                {
                    verts.Add(u1);
                    verts.Add(u2);
                    verts.Add(u3);
                }
                else
                {
                    u2.Y = u1.Y;
                    verts.Add(u2);
                    verts.Add(u3);
                }

                u1 = new UVECTOR(right.X(), right.MinY) { Tag = "LR" };
                u2 = new UVECTOR(u1.X, cY) { Tag = "UR" };
                u3 = new UVECTOR(u1.X, right.MaxY) { Tag = string.Empty };


                if (u1.Y < u2.Y && u2.Y < u3.Y)
                {
                   
                    verts.Add(u1);
                    verts.Add(u2);
                    verts.Add(u3);
                }
                else
                {
                    u2.Y = u1.Y;
                    verts.Add(u2);
                    verts.Add(u3);
                }
            }



                _rVal.Tag = aSpoutGroup.Handle;
            _rVal.Vertices = verts;
            _rVal.Update();
            return _rVal;
        }

      
        /// <summary>
        /// #1the tray assembly to generate startup spouts for
        /// #2the aTarget area for the total startup spouts area
        /// #3the maximum below spec deviation

        /// ^creates the startup spouts for the passed tray assembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTarget"></param>
        /// <param name="aConfiguration"></param>
        /// <param name="aOverrideLength"></param>
        /// <param name="aExistingSpouts"></param>
        /// <param name="bPreserveLocations"></param>
        /// <returns></returns>
        public static mdStartupSpouts Generate(mdTrayAssembly aAssy, double aTarget, ref uppStartupSpoutConfigurations aConfiguration, double aOverrideLength, mdStartupSpouts aExistingSpouts = null, bool bPreserveLocations = false)
        {
            mdStartupSpouts _rVal = new mdStartupSpouts();
            if (aAssy == null) return _rVal;
           
            _rVal.SubPart(aAssy);
            if (aTarget <= 0)  aTarget = aAssy.IdealStartupArea;
        
            if (aTarget <= 0) return _rVal;
            bool bMetric = aAssy.MetricSpouting;

            _rVal.TargetArea = aTarget;
            colMDSpoutGroups sGroups = aAssy.SpoutGroups;
            mdStartupLines SULines;
            double ht  = !bMetric ? 0.375 : 10 / 25.4;
            double rad = 0.5 * ht;
            int sitecnt = 0;
            int sitecnt2; 
            double lg = 0;
            double lg2;
            uppStartupSpoutConfigurations aconfig = aConfiguration;
            
            string curspout = null;
            Dictionary<string,double> aLocations = null;
            if (aExistingSpouts != null && aExistingSpouts.Count > 0)
            {
                curspout = aExistingSpouts.SelectedMember.Handle;
                aLocations = bPreserveLocations ? aExistingSpouts.CurrentOrdinates(): null;
            }
            //initialize

            _rVal.Height = ht;
             
         
            dxxRoundToLimits rndTo = !bMetric ? dxxRoundToLimits.Sixteenth : dxxRoundToLimits.Millimeter;
            if (aconfig == uppStartupSpoutConfigurations.Undefined) aconfig = aAssy.StartupConfiguration;
          
            if (aconfig < uppStartupSpoutConfigurations.None && aconfig > uppStartupSpoutConfigurations.TwoByFour) aconfig = uppStartupSpoutConfigurations.TwoByTwo;
            if (aExistingSpouts != null)
            {
                //create the sites base on the passed spouts
                SULines = Lines_Basic(sGroups, aAssy, uppStartupSpoutConfigurations.Undefined);
                 SULines.ToggleByStartups(sGroups, aExistingSpouts);
            }
            else
            {
                //create the sites base on the configuration
                SULines = Lines_Basic(sGroups, aAssy, aconfig, bRegenBoundsAndLines:true);
            }
            if (aOverrideLength < ht)
            {
                //find out how many sites we have (including the tray wide numbers)
                if (aconfig == uppStartupSpoutConfigurations.None && aExistingSpouts != null)
                {
                    mdStartupLines usLines = SULines.GetBySuppressed(false);
                  
                    if (usLines.Count > 0)
                    {
                       lg = uopUtils.RoundTo(usLines.ShortestMember(true).Length, rndTo, false, true);
                        sitecnt = SULines.GetTotalOccurance(true, lg);
                        lg2 = uopUtils.IdealSpoutLength(aTarget, sitecnt, rad, bMetric, null, false);
                        if (lg2 < lg) lg = lg2;
                     
                    }
                    else
                    {
                        sitecnt = 0;
                    }
                }
                else
                {
                    if (SULines != null) 
                    {
                        sitecnt = SULines.GetTotalOccurance(bUnspressedOnly: true, aLongerThan: 0);
                    }
                       
           
                }

                if (sitecnt > 0)
                {
                    //get the count based on the sites regardless of their length
                    lg = uopUtils.IdealSpoutLength(aTarget, sitecnt, rad, bMetric, null, false);
                    //get the number of sites that will acommodate the tabulate length
                    sitecnt2 = SULines.GetTotalOccurance(bUnspressedOnly: true, aLongerThan: lg);

                    if (sitecnt2 < sitecnt)
                    {
                        //suppress the sites that were too small
                        SULines.SetMinLength(aMinLength: lg, bSuppressTheLessThan: true);
                        lg = uopUtils.IdealSpoutLength(aTarget, sitecnt2, rad, bMetric, null, false);
                        //sitecnt = sitecnt2
                    }
                }
            }
            else
            {
                lg = aOverrideLength;
            }
            //MakeSpouts:
            if (lg < ht) lg = ht;
        
            SULines.SpoutHeight = ht;
            SULines.SpoutLength = lg;

            _rVal.GenerateByLines(aAssy, SULines, sGroups);

            if(!string.IsNullOrWhiteSpace(curspout))
            {
                mdStartupSpout select = _rVal.GetByHandle(curspout);
                if (select != null)
                    _rVal.SelectedIndex = select.Index;
            }

            aConfiguration = aconfig;

            if(aLocations != null)
            {
                foreach (mdStartupSpout spout in _rVal)
                {
                    if(aLocations.TryGetValue(spout.Handle, out double ord))
                        spout.Y = ord;
                }
            }
            return _rVal;
        }

        /// <summary>
        /// #1the spout groups to create the startup control lines for
        //^creates the basic set of startup control lines for the passed spout groups
        /// </summary>
        /// <param name="aSpoutGroups"></param>
        /// <param name="aAssy"></param>
        /// <param name="aConfiguration"></param>
        /// <param name="bRegenBoundsAndLines"></param>
        /// <returns></returns>
        public static mdStartupLines Lines_Basic(colMDSpoutGroups aSpoutGroups, mdTrayAssembly aAssy, uppStartupSpoutConfigurations aConfiguration = uppStartupSpoutConfigurations.None, bool bRegenBoundsAndLines = false)
        {
            mdStartupLines _rVal = new mdStartupLines();
            if (aSpoutGroups == null)  return _rVal;
            
            aAssy ??= aSpoutGroups.GetMDTrayAssembly();
            if (aAssy == null) return _rVal;

            uppMDDesigns family = aAssy.DesignFamily;
            bool symmetric = family.IsStandardDesignFamily();
      
            //loop on spout groups and add their control ines
            foreach (mdSpoutGroup group in aSpoutGroups)
            {
                if (group.IsVirtual || group.SpoutCount(aAssy) <= 0) continue;

                UpdateSpoutObjects(group, aAssy, bRegenBoundsAndLines);
                mdStartupLines aEdges = group.StartupLines;
                if (aEdges != null && aEdges.Count > 0)
                {

                    //if(group.Handle == "3,4")
                    //{
                    //    Console.WriteLine("HERE");
                    //}

                    mdSpoutGroup below = symmetric ? aSpoutGroups.Item(aSpoutGroups.IndexOf(group) + 1, bSuppressIndexError: true) : null;
                        aEdges = aEdges.ToggleByConfiguration(family,  aConfiguration, group,below);
                        _rVal.Append(aEdges);
                    
                }
            }

           
            return _rVal;
        }


        /// <summary>
        /// returns the suppression property of the first line that has the passed tag
        /// </summary>
        /// <param name="aSpouts">the subject spouts</param>
        /// <param name="aTag">the tag to search for</param>
        /// <param name="aDefault">the default reutn value if the member is not found</param>
        /// <returns></returns>
        public static bool GetTaggedSupression(List<mdStartupSpout> aSpouts,  string aTag, bool aDefault)
        {
            if (aSpouts == null) return aDefault;

            mdStartupSpout mem = aSpouts.Find((x) => string.Compare(x.Tag,aTag,true) ==0);
            return (mem == null) ? aDefault : mem.Suppressed;

        }
        public static void UpdateSpoutObjects(mdSpoutGroup aSpoutGroup, mdTrayAssembly aAssy, bool bRegenBoundsAndLines = false)
        {
            if (aSpoutGroup == null) return;
            aAssy ??= aSpoutGroup.GetMDTrayAssembly();
            if (aAssy == null) return;

            bool bmirrors = aAssy.DesignFamily.IsStandardDesignFamily();
          
            mdStartupLine UL = null;
            mdStartupLine UR = null;
            mdStartupLine LL = null;
            mdStartupLine LR = null;
            int occr = aSpoutGroup.OccuranceFactor;
            int sgidx = aSpoutGroup.Index;
          
           // int idx = 0;
            double sMin  =aAssy.StartupDiameter;
            bool bHasBound = false;
            bool bHasLines = false;
            
       
            string sghndl = aSpoutGroup.Handle;
            double sArea = aSpoutGroup.TargetArea(aAssy);
            int dcid = aSpoutGroup.DowncomerIndex;
            double thk = aSpoutGroup.Thickness;
            int bxid = aSpoutGroup.BoxIndex;
            USHAPE suBnd ;
          
            if (!bRegenBoundsAndLines)
            {
                aSpoutGroup.HasStartupObjects(out bHasBound,out bHasLines);
            }
            
          
            suBnd = Bounds(aSpoutGroup, aAssy);
            if (!bHasBound)
                aSpoutGroup.StartupBoundV = new USHAPE(suBnd);
           


            if (!bHasLines)
            {
                aSpoutGroup.GetMirrorValues(out double? mirrX, out double? mirrY);
                if (!bmirrors) { mirrX = null; mirrY = null; }
                    mdStartupLines SULines = new mdStartupLines();
                List<uopLine> edges = suBnd.Vertices.ToLines();
                //Segs = new USEGMENTS(suBnd.Segments);
                //make sure the endpoints point down and set some properties
                uopLine aLine = edges.Find((x) => string.Compare(x.Tag, "UL", true) == 0);
                if (aLine != null)
                {
                    aLine.sp.X -= thk / 2;
                    aLine.ep.X = aLine.sp.X;
                    UL = new mdStartupLine(aLine)
                    {
                        Tag = "UL",
                        Occurs = occr,
                        SpoutGroupHandle = sghndl,
                        DowncomerIndex = dcid,
                        BoxIndex = bxid,
                        ReferencePt = new uopVector(aLine.MidPt),
                        MinLength = sMin,
                        SpoutGroupArea = sArea,
                        MirrorY = mirrY,
                        MirrorX = mirrX
                    };
                    UL.Z = UL.StartPt.Z;
                    UL.Rectify();
                }


                aLine = edges.Find((x) => string.Compare(x.Tag, "LL", true) == 0); ;
                if (aLine != null)
                {
                    aLine.sp.X -= thk / 2;
                    aLine.ep.X = aLine.sp.X;
                 
                    LL = new mdStartupLine(aLine)
                    {
                        Tag = "LL",
                        Occurs = occr,
                        SpoutGroupHandle = sghndl,
                        DowncomerIndex = dcid,
                        BoxIndex = bxid,
                        ReferencePt = new uopVector(aLine.MidPt),
                        MinLength = sMin,
                        SpoutGroupArea = sArea,
                        MirrorY = mirrY,
                        MirrorX = mirrX

                    };
                     LL.Z = LL.StartPt.Z;
                    LL.Rectify();
                }
                aLine = edges.Find((x) => string.Compare(x.Tag, "UR", true) == 0); ;
                if (aLine != null)
                {
                    aLine.sp.X += thk / 2;
                    aLine.ep.X = aLine.sp.X;
                    UR = new mdStartupLine(aLine)
                    {
                        Tag = "UR",
                        Occurs = occr,
                        SpoutGroupHandle = sghndl,
                        DowncomerIndex = dcid,
                        BoxIndex = bxid,
                        ReferencePt = new uopVector(aLine.MidPt),
                        MinLength = sMin,
                        SpoutGroupArea = sArea,
                        MirrorY = mirrY,
                        MirrorX = mirrX
                    };
                    UR.Z = UR.StartPt.Z;
                    UR.Rectify();


                  
                }

                aLine = edges.Find((x) => string.Compare(x.Tag, "LR", true) == 0); ;
                if (aLine != null)
                {
                    aLine.sp.X += thk / 2;
                    aLine.ep.X = aLine.sp.X;
                    LR = new mdStartupLine(aLine)
                    {
                        Tag = "LR",
                        Occurs = occr,
                        SpoutGroupHandle = sghndl,
                        DowncomerIndex = dcid,
                        BoxIndex = bxid,
                        ReferencePt = new uopVector(aLine.MidPt),
                        MinLength = sMin,
                        SpoutGroupArea = sArea,
                        MirrorY = mirrY,
                        MirrorX = mirrX
                    };
                    LR.Z = LR.StartPt.Z;
                    LR.Rectify();
                   
                    
                    
                }
                if (UL != null)
                {
                    if (UL.Length< sMin)
                    {
                        if (LL != null)
                        {
                            UL.EPT = LL.EPT;
                            LL = null;
                        }
                    }
                }
                if (UR != null)
                {
                    if (UR.Length < sMin)
                    {
                        if (LR != null)
                        {
                            LR.SPT = UR.SPT;
                            UR = null;
                        }
                    }
                }

                SULines.Add(UL);
                SULines.Add(LL);
                SULines.Add(UR);
                SULines.Add(LR);
                if (UL != null && LL != null)
                {
                    UL.NeightborBelow = LL;
                    LL.NeightborAbove = UL;
                    if (bRegenBoundsAndLines)
                    {
                        //if(!UL.Suppressed && LL.Suppressed)
                        //    UL.ep.Y = LL.ep.Y;
                        //if (UL.Suppressed && !LL.Suppressed)
                        //    LL.sp.Y = UL.ep.Y;

                    }
                }
                if (UR != null && LR != null)
                {
                    UR.NeightborBelow = LR;
                    LR.NeightborAbove = UR;
                    if (bRegenBoundsAndLines)
                    {
                        //if (!UR.Suppressed && LR.Suppressed)
                        //    UR.ep.Y = LR.ep.Y;
                        //if (UR.Suppressed && !LR.Suppressed)
                        //    LR.sp.Y = UR.ep.Y;

                    }
                }

                //set the spout groups  lines to the new ones
                aSpoutGroup.StartupLines = SULines;
            }
        }
       

        /// <summary>
        ///  //^#1the subject tray assembly
        //~creates the startups based on the locations ans sizes stored in the assemblies downcomer properties

        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public static mdStartupSpouts ByLocation(mdTrayAssembly aAssy)
        {
            mdStartupSpouts _rVal = new mdStartupSpouts();
            if (aAssy == null) return _rVal;
            
            _rVal.TargetArea = aAssy.IdealStartupArea;
            _rVal.SubPart(aAssy);
            colMDSpoutGroups sGroups = aAssy.SpoutGroups;
            double ht = aAssy.StartupDiameter;
            double lg = aAssy.OverrideStartupLength;
            //initialize

            //the size and length is stored in the assy's global downcomer
            if (lg <= 0) lg = aAssy.StartupLength;

            mdStartupLines SULines = Lines_Basic(sGroups, aAssy, uppStartupSpoutConfigurations.Undefined);
            SULines = SULines.ToggleByDowncomers(sGroups, aAssy.Downcomers, lg);

            SULines.SpoutHeight = ht;
            SULines.SpoutLength = lg;
            _rVal.GenerateByLines(aAssy, SULines, sGroups, bDontSetYVals: true);
            _rVal.Invalid = false;
            return _rVal;
        }

        #region Shared Methods
        public static double DeviationLimit => 5.0;
        #endregion Shared Methods
    }


}