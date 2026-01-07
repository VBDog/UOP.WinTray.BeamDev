using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.DXFGraphics.Utilities;
using System.Linq;

namespace UOP.WinTray.Projects.Generators
{
    /// <summary>
    /// the utitilty that is used to create deck panels and deck sections
    /// </summary>
    public class mdPanelGenerator
    {

        /// <summary>
        /// creates as collection of md deck panels based on the passed md tray assembly
        /// </summary>
        /// <param name="aAssy">the TrayAssembly to use in the definition of the colleciton of deck panels</param>
        /// <returns>creates as collection of md deck panels based on the passed collection of md dowmcomers</returns>
        public static List<mdDeckPanel> CreateDeckPanels(mdTrayAssembly aAssy)
        {
            List<mdDeckPanel> _rVal = new List<mdDeckPanel>();
            if (aAssy == null) return _rVal;

            DowncomerDataSet dcdata = aAssy.DowncomerData;
            double rad = dcdata.DeckRadius;
            if (rad <= 0 || dcdata.Count <= 0) return _rVal;
            double dcwd = dcdata.BoxWidth / 2;
            List<double> xvals_DC = dcdata.XValues_Downcomers;
            uppSpliceStyles sstyle = aAssy.DesignOptions.SpliceStyle;
            bool virtuals = aAssy.IsStandardDesign;
            mdDeckPanel parent;
            //get the free bubbling area shapes
            uopPanelSectionShapes panelshapes = dcdata.PanelShapes(bIncludeClearance: aAssy.ProjectType == uppProjectTypes.MDDraw);

            //get the free bubbling panels
            uopFreeBubblingPanels fbps = dcdata.CreateFreeBubblingPanels(aAssy);
            aAssy.FreeBubblingPanels = fbps;
            int iNp = panelshapes.MaxPanelIndex;
            int vircnt = 0;

            for (int p = 1; p <= iNp; p++)
            {
                bool moon = p == 1 || p > xvals_DC.Count;
                //set the left limit
                double xl = p <= xvals_DC.Count ? xvals_DC[p - 1] : -(rad + dcwd + 1);
                //set the right limit
                double xr = p == 1 ? rad + dcwd + 1 : xvals_DC[p - 2];
                double xc = xl + (xr - xl) / 2;
                // get the panel area shape(s)
                uopPanelSectionShapes pshapes =  panelshapes.PanelShapes(p,bGetClones:false);
                if (pshapes.Count <= 0) 
                    continue;

                var perim = pshapes[0].ParentShape;
                if (!perim.IsDefined)
                {
                    System.Console.WriteLine($"RAD:{rad}");
                    System.Console.WriteLine($"XL:{xl + dcwd}");
                    System.Console.WriteLine($"XR:{xr - dcwd}");
                    System.Console.WriteLine($"SPACE:{dcdata.Spacing}");


                }
                else
                {
                    DowncomerInfo rdc = p == 1 ? null : dcdata.Find(x => x.X == xr);
                    DowncomerInfo ldc = p <= xvals_DC.Count ? dcdata.Find(x => x.X == xl) : null;

                    mdDeckPanel newPanel = new mdDeckPanel(aAssy, perim); 
              
                    
                    //if (tPnl.Perimeter.IsDefined)
                    //{

                    if (virtuals && newPanel.X < 0)
                    {
                        vircnt++;
                        int pidx = uopUtils.OpposingIndex(newPanel.Index, aAssy.Downcomer().Count + 1);
                        parent = _rVal.Find(x => x.Index == pidx);
                        newPanel.Parent = parent;
                    }
                    else
                    {
                        if (virtuals)
                            newPanel.OccuranceFactor = newPanel.X != 0 ? 2 : 1;
                    }
                    newPanel.SubPart(aAssy);
                    newPanel.SetFBP(fbps.Find((x) => x.PanelIndex == p), pshapes);  //set the freebubbling areas and the sub shapes of the panel
                    _rVal.Add(newPanel);
                    //}
                    //else
                    //{

                    //}
                }



            }


            return _rVal;

        }

        /// <summary>
        /// the zones of the panels FBA that are supplied from above by a single spout group
        /// </summary>
        /// <param name="aPanel"></param>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public static List<mdFeedZone> CreatePanelFeedZones(mdDeckPanel aPanel, mdTrayAssembly aAssy)
        {
            List<mdFeedZone> _rVal = new List<mdFeedZone>();
            if (aPanel == null) return _rVal;
            aAssy ??= aPanel.GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            List<USHAPE> panelFBAs = new List<USHAPE>();
            List<uopFreeBubblingArea> FBAs = aAssy.FreeBubblingAreas.FindAll((x) => x.PanelIndex == aPanel.Index); ;
            foreach (uopFreeBubblingArea fba in FBAs)
            {
                panelFBAs.Add(new USHAPE(fba));
            }

            colMDDowncomers DComers = aAssy.Downcomers;
            double rad = aAssy.RingID / 2;
            // colDXFVectors yPts = new colDXFVectors();
            double aSpc = DComers.Spacing / 2;

            colMDSpoutGroups aSptsGrgs = aAssy.SpoutGroups;
            uopRectangle lims = aPanel.Limits;
            List<Tuple<double, double, double, int>> yVals = new List<Tuple<double, double, double, int>>();

            for (int i = 1; i <= DComers.Count; i++)
            {
                mdDowncomer dc = DComers.Item(i);
                double y1 = -dc.X;
                double yb = y1 - aSpc;
                double yt = y1 + aSpc;

                yVals.Add(new Tuple<double, double, double, int>(y1, yb, yt, dc.Index));


            }
            yVals.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            foreach (USHAPE fba in panelFBAs)
            {
                int idx = 0;
                URECTANGLE bounds = fba.Limits;
                double xl = bounds.Left;
                double xr = bounds.Right;
                for (int i = 1; i < yVals.Count; i++)
                {

                    double y1 = yVals[i - 1].Item1;
                    double yb = yVals[i - 1].Item2;
                    double yt = yVals[i - 1].Item3;
                    if (y1 <= 0 && yt <= bounds.Bottom) continue;
                    if (y1 > 0 && yb >= bounds.Top) break;

                    idx++;
                    if (idx == 1)
                    {
                        yb = lims.Bottom - 1;
                    }

                    mdDowncomer dc = DComers.Item(yVals[i - 1].Item4);

                    uopShape aBound = uopShape.CircleSection(new uopArc( rad), new uopRectangle( xl,yt, xr,  yb));
                    if (aBound.Vertices.Count > 1)
                    {
                        mdFeedZone aZone = new mdFeedZone()
                        {
                            BoundsV = new USHAPE(aBound),
                            PanelIndex = aPanel.Index,
                            DowncomerIndex = i,
                            DeckPanel = aPanel
                        };
                        _rVal.Add(aZone);
                    }

                }
            }



            //for (int i = 1; i < yVals.Count; i++)
            //{
            //    double xl = aPanel.Left() + 1;
            //    double xr = (aPanel.Index > 1) ? aPanel.Right() - 1 : rad;
            //    double yb = yVals[i - 1].Item1;
            //    double yt = yVals[i -1 ].Item2;
            //    mdDowncomer dc = DComers.Item( yVals[i -1].Item3);

            //    USHAPE aBound = USHAPE.CircleSection(0, 0, rad, 0, xl, xr, yt, yb);
            //    if (aBound.Vertices.Count > 1)
            //    {
            //        mdFeedZone aZone = new mdFeedZone()
            //        {
            //            BoundsV = aBound,
            //            PanelIndex = aPanel.Index,
            //            DowncomerIndex = i,
            //            DeckPanel = aPanel
            //        };
            //        _rVal.Add(aZone);
            //    }

            //}
            return _rVal;
        }

  


        /// <summary>
        /// returns the shape that is the open area of the deck panel
        /// </summary>
        /// <param name="aPanel"></param>
        /// <param name="cDowncomers"></param>

        /// <returns></returns>
        public static dxePolyline CreatePanelOpenArea(mdDeckPanel aPanel, colMDDowncomers cDowncomers) => CreatePanelOpenArea(aPanel, cDowncomers, out mdDowncomer LDC, out mdDowncomer RDC, null);


        /// <summary>
        /// returns the shape that is the open area of the deck panel
        /// </summary>
        /// <param name="aPanel"></param>
        /// <param name="cDowncomers"></param>
        /// <param name="DCLeft"></param>
        /// <param name="DCRight"></param>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public static dxePolyline CreatePanelOpenArea(mdDeckPanel aPanel, colMDDowncomers cDowncomers, out mdDowncomer rDCLeft, out mdDowncomer rDCRight, mdTrayAssembly aAssy = null)
        {
            rDCLeft = null;
            rDCRight = null;

            dxePolyline _rVal = null;
            if (aPanel == null) return _rVal;
            double rad = aPanel.Radius;
            if (rad == 0) return _rVal;
            aAssy = aPanel.GetMDTrayAssembly(aAssy);
            if (cDowncomers == null && aAssy != null) cDowncomers = aAssy.Downcomers;
            if (cDowncomers == null) return _rVal;

            //get the ring radius
            double rrad = aPanel.RingRadius;
            double xl = -2 * rad;
            double xr = 2 * rad;


            //get the left and right downcomers
            rDCRight = aPanel.RightDowncomerIndex > 0 ? cDowncomers.Item(aPanel.RightDowncomerIndex) : null;
            rDCLeft = aPanel.LeftDowncomerIndex > 0 ? cDowncomers.Item(aPanel.LeftDowncomerIndex) : null;

            //bail if there are no downcomers
            if (rDCLeft == null && rDCRight == null) return _rVal;

            //create the panels bounding perimeter
            aPanel.Y = 0;

            //set the left limit
            if (rDCLeft != null)
            {
                xl = rDCLeft.X + 0.5 * rDCLeft.BoxWidth + 1;
            }

            //set the right limit
            if (rDCRight != null)
            {
                xr = rDCRight.X - 0.5 * rDCRight.BoxWidth - 1;
            }

         

            //special case
            if (aPanel.IsCenter) xl = -xr;


            //create the open area polygon
            _rVal = (dxePolyline)dxfPrimatives.CreateCircleStrip(null, rrad, false, null, 0, xl, xr);
            return _rVal;
        }



     
       

    }
}
