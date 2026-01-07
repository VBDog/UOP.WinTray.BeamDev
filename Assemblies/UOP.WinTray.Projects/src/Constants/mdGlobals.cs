

using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Constants
{
    public class mdGlobals
    {
        public bool gbFlag;
        /// <summary>
        /// the diameter of the bubble promoters on a deck section
        /// </summary>
        public static double gsMDBubblePromoterDiameter => 2.55;
        /// <summary>
        /// the holes size used for a bolted connection
        /// </summary>
        public static double gsBigHole => 10/25.4;  //0.4375 => 11mm;
        /// <summary>
        /// the holes size used for a welded bolted connection
        /// </summary>
        public static double gsSmallHole => 10/25.4; //.406 => 10mm;

        /// <summary>
        /// the hole used to attach a finger clip to a downcomer
        /// </summary>
        internal static UHOLE FingerClipHole (mdTrayAssembly aAssy)
        {
            UHOLE _rVal = new UHOLE(mdGlobals.gsBigHole, 0, 0, 0, "FINGER CLIP", aZDirection: "1,0,0", aInset: 0.625);
            if (aAssy != null)
            {
                mdDowncomer dc = aAssy.Downcomer();
                double dkthk = aAssy.Deck.Thickness;
                _rVal.Depth = dc.Thickness;
                _rVal.Elevation = dkthk + 0.625;
                _rVal.DownSet = dc.How + dkthk - _rVal.Elevation;
            }
            return _rVal;
            //new UHOLE(mdGlobals.gsBigHole, 0, 0, 0, "FINGER CLIP", Downcomer().Thickness, aElevation: Deck.Thickness + 0.625, aZDirection: "1,0,0", aInset: 0.625, aDownSet: Downcomer().How + Deck.Thickness - (Deck.Thickness + 0.625));
        }


        /// <summary>
        /// the hole used to attach a ring clips to a downcomer or a dec section
        /// </summary>
        public static uopHole RingClipHole(mdTrayAssembly aAssy, bool bForEndSupports)
        {
            uopHole _rVal = new uopHole(mdGlobals.gsBigHole, 0, 0, 0, "RING CLIP",aFlag:"HOLE"  );
            if (aAssy != null)
            {
                _rVal.Depth = !bForEndSupports ? aAssy.Deck.Thickness : aAssy.Downcomer().Thickness;
                _rVal.Elevation =0.5 * _rVal.Depth;
                if(bForEndSupports) _rVal.Diameter = aAssy.RingClipSize == uppRingClipSizes.ThreeInchRC ? mdGlobals.gsBigHole : 13 / 25.4;
            }
            return _rVal;
        }

        /// <summary>
        /// the hole used to attach the deflector plates to the assembly stiffener mounts
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aDepth"></param>
        /// <returns></returns>

        internal static UHOLE BaffleMountSlot(mdTrayAssembly aAssy, double? aDepth = null)
        {
            UHOLE _rVal = new UHOLE("BAFFLE MOUNT") { ZDirection = "1,0,0", Rotation = 90, Tag = "BAFFLE MOUNT", Radius = mdGlobals.gsBigHole / 2, Length = 0.875, Inset = 0.5, DownSet = 1 };

            if(aAssy != null)
            {
                _rVal = mdGlobals.BaffleMountSlot(aAssy, out _, out _, out _);
                if (!aDepth.HasValue) _rVal.Depth = aAssy.Downcomer().Thickness;
            }
            if (aDepth.HasValue) _rVal.Depth = aDepth.Value;
            return _rVal;
        }

        /// <summary>
        /// the hole used to attach the deflector plates to the assembly stiffener mounts
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="rTopZ">returns the top Z of the stiffener mount flange</param>
        /// <param name="rBaffleMountHeight">returns the height of the stiffener mount flange</param>
        /// <param name="rSupportsBaffle">returns true if  baffles are actually part of the assembly</param>
        /// <returns></returns>
        internal static UHOLE BaffleMountSlot(mdTrayAssembly aAssy, out double rTopZ, out double rBaffleMountHeight, out bool rSupportsBaffle)
        {
            rTopZ = 0;
            rBaffleMountHeight = 0;
            rSupportsBaffle = false;
            UHOLE _rVal = new UHOLE("BAFFLE MOUNT") { ZDirection = "1,0,0", Rotation = 90, Tag = "BAFFLE MOUNT", Radius = mdGlobals.gsBigHole / 2, Length = 0.875, Inset = 0.5, DownSet = 1 };

            if (aAssy != null)
            {
                rSupportsBaffle = aAssy.DesignFamily.IsEcmdDesignFamily() && aAssy.DesignOptions.CDP > 0;
                double dkthk = aAssy.Deck.Thickness;

                rTopZ = aAssy.Downcomer().How + dkthk;
                if (rSupportsBaffle)
                {
                    rBaffleMountHeight = mdGlobals.BaffleMountHeight(aAssy, out rTopZ, out double elev);
                    _rVal.Elevation = elev;

                }
            }

            return _rVal;
        }
        public static double BaffleMountHeight(mdTrayAssembly aAssy, out double rTopZ, out double rSlotElev)
        {
            rTopZ = 0;
            rSlotElev = 0;
            if (aAssy == null) return 0;

            rTopZ = aAssy.Downcomer().How + aAssy.Deck.Thickness;
            rSlotElev = rTopZ;
            if (!aAssy.DesignFamily.IsEcmdDesignFamily() || aAssy.DesignOptions.CDP <= 0) return 0;

            double _rVal = 0;
            double dkthk = aAssy.Deck.Thickness;
            double top = (aAssy.DesignOptions.BaffleMountPercentage / 100 * aAssy.BaffleHeight) + rTopZ;
            _rVal = top - rTopZ - 0.25;

            //mount face min = 2''
            if (_rVal < 2)
            {
                _rVal = 2;
                top = rTopZ + 2 + 0.25;

            }

            rSlotElev = top - 1;

            return _rVal;
        }
        /// <summary>
        /// the slot to use to attach the manway fasterner clips to a panel
        /// </summary>
        public static uopHole ManwayClipMountingSlot(mdTrayAssembly aAssy = null, double aAngle = 0) 
        {
            uopHole _rVal = new uopHole(mdGlobals.gsBigHole, 0, 0, aLength: 2.5, aTag: "MANWAY", aRotation:aAngle,  aFlag: "SLOT", aInset: 2, aDownSet: 2.375);

            if (aAssy!=null)
            {
                _rVal.Depth = aAssy.Deck.Thickness;
                _rVal.Elevation = 0.5 * _rVal.Depth;
            }
            return _rVal;
        }

        // <summary>
        /// the hole in the angle that receives the bolts
        /// </summary>
        internal static UHOLE SpliceBoltHole (uopPart aOwner)
        {
            double thk = aOwner != null ?aOwner.Thickness: 0;
            bool welded = aOwner != null ? aOwner is mdSpliceAngle : false;
            double f1 = aOwner != null ? aOwner is mdSpliceAngle ? -1 : 1: -1;
            double inset = aOwner != null ? aOwner is mdSpliceAngle ? 0.5 * aOwner.Width : 0.5 : 0;
            string tag =   aOwner != null ? aOwner is mdSpliceAngle ? "BOLT" : "SPLICE HOLE" : string.Empty;
            return   new UHOLE(welded ?mdGlobals.gsSmallHole: mdGlobals.SpliceHoleDiameter, 0, 0, aDepth: thk, aElevation: f1 * 0.5 * thk, aInset: inset, bWeldedBolt: true) { Tag = "BOLT" };
        }  
       
        /// <summary>
        /// the the extra space allowed for joggled deck panel splices
        /// ~constant = 0.25
        /// </summary>
        public static double JoggleGap => 0.25;

        /// <summary>
        /// the width of the lapped part of a joggle deck panel Joint
        /// ~constant = 1.5
        /// </summary>
        public static double JoggleLap => 1.5;


        public static double MinSlotXPitch => 1.0906;
        public static double MinSlotYPitch => 1.3356;

        public static double MinSpliceProximity=> 6;
        public static double MaxPanelSlotDeviation => 2;
        public static double SlotHeight => 0.7567;
        public static double SlotWidth => 0.3865;

        public static double SlotDieHeight => 1.25;
        public static double SlotDieWidth => 0.76;

        public static double SlotDieClearance => 0.7315;
        /// <summary>
        /// the gap between the shelf angle of a downcomer and the end of a flange formed into a deck section 
        /// </summary>
        public static double FormedFlangeDeckGap => 0.1875;
        /// <summary>
        /// the radius of the bubble promoters on a deck section
        /// </summary>
        public static double BPRadius => gsMDBubblePromoterDiameter / 2;

        /// <summary>
        /// the diameter of holes used in deck section for bolting to a splice angle
        /// </summary>
        public static double SpliceHoleDiameter => 0.5;

        /// <summary>
        /// the width of a standrd deck splice angle
        /// </summary>
        public static double SpliceAngleWidth = 2.5;
        /// <summary>
        /// the outside diameter of the large hold down washer used on deck section ring clips
        /// </summary>
        public static double HoldDownWasherDiameter =>1.75;
        /// <summary>
        /// the outside radius of the large hold down washer used on deck section ring clips
        /// </summary>
        public static double HoldDownWasherRadius=> HoldDownWasherDiameter/2;

        /// <summary>
        /// the threshold limit line length that determines if a downcomer end support get two ring clips or 1
        /// </summary>
        public static double LimitLineLengthLimitForTwoRingClips => 10;

        /// <summary>
        /// the closes a ring clip can be placed to a downcomer shelf angle
        /// </summary>
        public static double MinRingClipClearance => 1.5;

        public static double DefaultRingClipSpacing => 9;

        /// <summary>
        /// the width of a tab
        /// </summary>
        public static double DeckTabWidth => 2.0;

        /// <summary>
        /// the distance from the edge of a deck section to the tip of a tab
        /// </summary>
        public static double DeckTabHeight => 1.102;

        /// <summary>
        /// the distance from the edge of a deck section to the tip of a female flange tip
        /// </summary>
        public static double DeckTabFlangeHeight => 1.26;

        /// <summary>
        /// the distance from the outside edge of a deck section to the tart of the first tab
        /// </summary>
        public static double DeckTabFlangeInset => 1.102;

        /// <summary>
        /// the distance from the edge female flange to the center of the tab slot
        /// </summary>
        public static double DeckTabSlotInset => 1.89;

        /// <summary>
        /// the length of the slot that recieves the tabs
        /// </summary>
        public static double DeckTabSlotLength => 60 / 25.4;

        /// <summary>
        /// the radius of the slot used to bolt a deck section to a manways splice plate
        /// </summary>
        public static double DeckManwayAngleSlotRadius => 6/25.4;

        /// <summary>
        /// the overall length of the half slot cut into deck section to bolt a it to a manways splice plate
        /// </summary>
        public static double DeckManwayAngleSlotLength => 19/25.4 + DeckManwayAngleSlotRadius;

        /// <summary>
        /// the radius to apply to the cormers of a maway deck section
        /// </summary>
        public static double DeckManwayCornerRadius => 13 / 25.4;
        
        /// <summary>
        /// the limit the determines if a weir line can be allowed
        /// </summary>
        public static double MinWeirLength => 18;
        /// <summary>
        /// the limit the determines if a box line can be allowed
        /// </summary>
        public static double MinBoxLength => 16;
        /// <summary>
        /// the limit line length that controls when a DC end support will get two ringclips
        /// </summary>
        public static double TwoEndSupportBoltLimitLineLength => 10;

        /// <summary>
        /// initial inset of the end plate from the ends of a DC box
        /// </summary>
        public static double DefaultEndplateInset => 1;

        /// <summary>
        /// the default width of the deck support angles welded to the sides of downcomers
        /// </summary>
        public static double DefaultShelfAngleWidth => 1;

        /// <summary>
        /// the default clearance of a deck panel from its adjacent downcomers
        /// </summary>
        public static double DefaultPanelClearance => 0.0825;


        /// <summary>
        /// the width of the lapped part of a joggle deck panel Joint 
        /// constant = 1.5
        /// </summary>
        public static double JoggleDeckLap => 1.5;

        /// <summary>
        /// the additional clearance applied to deck sections if the assembly has any downcomers with folded weirs
        /// </summary>
        public static double FolderWeirPanelClearanceAdder => 0.125;

        /// <summary>
        /// the default offset of the deck tab slots from the splice center
        /// </summary>
        public static double TabSlotOffset=> 0.0825;
        /// <summary>
        /// the default clearance of a deck panel from its adjacent downcomers
        /// </summary>
        public static double DefaultSpliceAngleClearance => 0.1875;

        /// <summary>
        /// the methods used to round box lengths to (16ths, millimeters or none)
        /// </summary>
        /// 
        public static dxxRoundToLimits DefaultRoundingUnits => dxxRoundToLimits.Sixteenth;

        /// <summary>
        /// the methods used to round box lengths to (16ths, millimeters or none)
        /// </summary>
        public static uppMDRoundToLimits DefaultRoundingMethod => uppMDRoundToLimits.Sixteenth;

        /// <summary>
        /// the distance the endplate hangs past the end of the downcomer box
        /// </summary>
        public static double DefaultEndPlateOverhang =>  0.25;

        /// <summary>
        /// the default minimum distance allowed between a ring cip hole and an endplate in a downcomer end support
        /// </summary>
        public static double DefaultRingClipClearance => 1.375;

        /// <summary>
        /// The distance the ring clip holes in the end supports are inset from the left and right end plate inside edges when there are 2 ring clip holes
        /// </summary>
        public static double DCRingClipHoleInset => 1.5;

        /// <summary>
        /// the closest a downcomer can get to the ring without being invalidated
        /// </summary>
        public static double DCRingProximityClearance => 6;

        /// <summary>
        /// the distance to clip off of moon deck sections
        /// </summary>
        public static double MoonClipLength = 12 / 25.4;

        /// <summary>
        /// the radius to fillet the corners of a splice angle style maway
        /// </summary>
        public static double ManwayFilletRadius = 0.5;

    }
}
