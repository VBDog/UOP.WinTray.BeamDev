using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects.Utilities.ExtensionMethods
{

    public static class UppSpliceAngleTypeExtensionMethods
    {
        public static uppPartTypes PartType(this uppSpliceAngleTypes aHardwareType)
        {
            return aHardwareType switch
            {
                uppSpliceAngleTypes.SpliceAngle => uppPartTypes.SpliceAngle,
                uppSpliceAngleTypes.ManwayAngle => uppPartTypes.ManwayAngle,
                uppSpliceAngleTypes.ManwaySplicePlate => uppPartTypes.ManwaySplicePlate,
                _ => uppPartTypes.Undefined
            };
        }

    }
    public static class UppHardWareTypeExtensionMethods
    {
        public static uppPartTypes PartType(this uppHardwareTypes aHardwareType)
        {
            return aHardwareType switch
            {
                uppHardwareTypes.CarriageBolt => uppPartTypes.CarriageBolt,
                uppHardwareTypes.Stud => uppPartTypes.Stud,
                uppHardwareTypes.HexNut => uppPartTypes.HexNut,
                uppHardwareTypes.HexBolt=> uppPartTypes.HexBolt,
                uppHardwareTypes.ShavedStud => uppPartTypes.ShavedStud,
                uppHardwareTypes.FlatWasher => uppPartTypes.FlatWasher,
                uppHardwareTypes.LockWasher => uppPartTypes.LockWasher,
                uppHardwareTypes.LargeODWasher => uppPartTypes.HoldDownWasher,
                _ => uppPartTypes.Undefined
            };
        }
    }
    public static class UppCaseOwnerTypeExtensionMethods
    {
        public static uppPartTypes PartType(this uppCaseOwnerOwnerTypes aType)
        {
            return aType switch
            {
                uppCaseOwnerOwnerTypes.ChimneyTray => uppPartTypes.ChimneyTray,
                uppCaseOwnerOwnerTypes.Distributor => uppPartTypes.Distributor,
                _ => uppPartTypes.Undefined
            };
        }
    }
    public static class UppProjectFamilyExtensionMethods
    {
        public static uppProjectTypes ProjectType(this uppProjectFamilies aType)
        {
            return aType switch
            {
                uppProjectFamilies.uopFamXF => uppProjectTypes.CrossFlow,
                uppProjectFamilies.uopFamMD => uppProjectTypes.MDSpout,
                _ => uppProjectTypes.Undefined
            };
        }
    }
    public static class UppProjectTypeExtensionMethods
    {
        public static uppProjectFamilies Family(this uppProjectTypes aType)
        {
            return aType switch
            {
                uppProjectTypes.MDSpout => uppProjectFamilies.uopFamMD,
                uppProjectTypes.MDDraw => uppProjectFamilies.uopFamMD,
                uppProjectTypes.CrossFlow => uppProjectFamilies.uopFamXF,
                _ => uppProjectFamilies.Undefined
            };
        }
    }


    public static class UppRoundingMethodExtensionMethods
    {
        public static dxxRoundToLimits Units(this uppMDRoundToLimits method)
        {
            return method switch
            {
                uppMDRoundToLimits.Sixteenth => dxxRoundToLimits.Sixteenth,
                uppMDRoundToLimits.Millimeter => dxxRoundToLimits.Millimeter,
                _ => dxxRoundToLimits.Undefined
                };
            }
    }
    public static class UppFlowSlotTypExtensionMethods
    {
        /// <summary>
        ///the area of a single flow slot in sqr. inches dependant on on the SlotType
        /// </summary>
        public static double  SlotArea(this uppFlowSlotTypes aSlotType)
        {


            return aSlotType switch
            {
                uppFlowSlotTypes.FullC => 0.0396,
                uppFlowSlotTypes.HalfC => 0.018,
                _ =>0
            };
        }
    }
    public static class UppSpliceTypeExtensionMethods
    {
        public static double GapValue(this uppSpliceTypes splicetype)
        {
            //if (sType == uppSpliceTypes.SpliceWithTabs)
            //{ return 0.0625; }
            //else if (sType == uppSpliceTypes.SpliceWithAngle)
            //{
            //    return (!bForManway) ? 0.0625 : 0.125;
            //}
            //else if (sType == uppSpliceTypes.SpliceManwayCenter)
            //{ return 0.314 / 2; }
            //else if (sType == uppSpliceTypes.SpliceWithJoggle)
            //{ return 0.125; }

            //return 0;
            return splicetype switch
            {
                uppSpliceTypes.SpliceWithTabs => 0.0625,
                uppSpliceTypes.SpliceManwayCenter => 0.314 / 2,
                uppSpliceTypes.SpliceWithJoggle => 0.125,
                uppSpliceTypes.SpliceWithAngle => 0.125,
                _ => 0
            };

            
        }
    }

   
    public static class UppMdDesignsExtensionMethods
    {
        public static bool IsMdDesignFamily(this uppMDDesigns designFamily, bool includeMDBeamDesign = true)
        {
            return designFamily == uppMDDesigns.MDDesign || (includeMDBeamDesign && designFamily == uppMDDesigns.MDBeamDesign);
        }

        public static bool IsEcmdDesignFamily(this uppMDDesigns designFamily, bool includeECMDBeamDesign = true)
        {
            return designFamily == uppMDDesigns.ECMDDesign || (includeECMDBeamDesign && designFamily == uppMDDesigns.ECMDBeamDesign);
        }

        public static bool IsBeamDesignFamily(this uppMDDesigns designFamily)
        {
            return (int)designFamily >= 100 && (int)designFamily < 200;
        }

        public static bool IsDividedWallDesignFamily(this uppMDDesigns designFamily)
        {
            return (int)designFamily >= 200 && (int)designFamily < 300;
        }

        public static bool IsStandardDesignFamily(this uppMDDesigns designFamily)
        {
            return (int)designFamily < 100;
        }
    }
    public static class UppPartTypesExtensionMethods
    {
        public static uppDisplayTableTypes DisplayTableType(this uppPartTypes aPartType)
        {
            return aPartType switch
            {
                uppPartTypes.SpoutGroup => uppDisplayTableTypes.SpoutGroupsProperties,
                uppPartTypes.SpoutGroups => uppDisplayTableTypes.SpoutGroupsProperties,
                uppPartTypes.Downcomer => uppDisplayTableTypes.DowncomersProperties,
                uppPartTypes.Downcomers => uppDisplayTableTypes.DowncomersProperties,
                uppPartTypes.DeckPanel => uppDisplayTableTypes.DeckPanelsProperties,
                uppPartTypes.DeckPanels => uppDisplayTableTypes.DeckPanelsProperties,
                uppPartTypes.DeckSection => uppDisplayTableTypes.DeckPanelsProperties,
                uppPartTypes.DeckSections => uppDisplayTableTypes.DeckPanelsProperties,

                _ => uppDisplayTableTypes.Undefined
            };
        }

        public static uppDisplayTableTypes DisplayListType(this uppPartTypes aPartType)
        {
            return aPartType switch
            {
                uppPartTypes.Project => uppDisplayTableTypes.ProjectProperties,
                uppPartTypes.Deck => uppDisplayTableTypes.DeckProperties,
                uppPartTypes.Downcomer => uppDisplayTableTypes.DowncomerProperties,
                uppPartTypes.DesignOptions => uppDisplayTableTypes.DesignOptions,
                uppPartTypes.Materials => uppDisplayTableTypes.Materials,
                uppPartTypes.TrayRanges => uppDisplayTableTypes.RangeProperties,
                uppPartTypes.TrayRange => uppDisplayTableTypes.RangeProperties,
                uppPartTypes.StartupSpout => uppDisplayTableTypes.StartupSpouts,
                uppPartTypes.StartupSpouts => uppDisplayTableTypes.StartupSpouts,
                uppPartTypes.TraySupportBeam => uppDisplayTableTypes.BeamProperties,
                _ => uppDisplayTableTypes.Undefined
            };
        }

    }


    public static class UppSpoutPatternsExtensionMethods
    {
        public static bool UsesSlots(this uppSpoutPatterns aPatternType, bool bIncludeSStar =false)
        {
            if(!bIncludeSStar)
                return (int)aPatternType >= 6 && (int)aPatternType <= 8;  //S1, S2 or S3
            else
                return (int)aPatternType >= 6 && (int)aPatternType <= 9;  //S1, S2 or S3
        }
        public static bool TriangularPitch(this uppSpoutPatterns aPatternType)
        {
            return aPatternType == uppSpoutPatterns.A || aPatternType == uppSpoutPatterns.Astar;  //S1, S2 or S3
        }

        public static dxxPitchTypes PitchType(this uppSpoutPatterns aPatternType)
        {
            return aPatternType == uppSpoutPatterns.A || aPatternType == uppSpoutPatterns.Astar ? dxxPitchTypes.Triangular : dxxPitchTypes.Rectangular;
        }


        /// <summary>
        /// the distance between spouts dictated by the pattern type.
        /// read only
        /// </summary>
        public static double HorizontalSpoutGap(this uppSpoutPatterns aPatternType)
        {

            switch (aPatternType)
            {
                case uppSpoutPatterns.A:
                case uppSpoutPatterns.Astar:
                case uppSpoutPatterns.B:
                    return 0.1875;

                case uppSpoutPatterns.C:
                    return 0.375;

                case uppSpoutPatterns.D:
                    return 1;

                case uppSpoutPatterns.S1:
                    return 0;

                case uppSpoutPatterns.S2:
                    return 0.25;

                default:
                    return 0.1875;

            }

        }
        /// <summary>
        /// the minimum vertical distance between spouts
        /// read only
        /// </summary>
        public static double VerticalSpoutGap(this uppSpoutPatterns aPatternType)
        {
            return aPatternType switch
            {
                uppSpoutPatterns.A => 1.0 / 16,
                uppSpoutPatterns.Astar => 1.0 / 16,
                _ => 3.0 / 16
            };

        }

    }

    public static class UppGridAlignmentsExtensionMethods
    {

        public static uppVerticalAlignments VerticalAlignment(this uppGridAlignments aAlignment)
        {
            if (aAlignment == uppGridAlignments.TopLeft || aAlignment == uppGridAlignments.TopCenter || aAlignment == uppGridAlignments.TopRight) return uppVerticalAlignments.Top;
            if (aAlignment == uppGridAlignments.MiddleLeft || aAlignment == uppGridAlignments.MiddleCenter || aAlignment == uppGridAlignments.MiddleRight) return uppVerticalAlignments.Center;
            if (aAlignment == uppGridAlignments.BottomLeft || aAlignment == uppGridAlignments.BottomCenter || aAlignment == uppGridAlignments.BottomRight) return uppVerticalAlignments.Bottom;

            return uppVerticalAlignments.Undefined;
        }
        public static uppHorizontalAlignments HorizontalAlignment(this uppGridAlignments aAlignment)
        {
            if (aAlignment == uppGridAlignments.TopLeft || aAlignment == uppGridAlignments.MiddleLeft || aAlignment == uppGridAlignments.BottomLeft) return uppHorizontalAlignments.Left;
            if (aAlignment == uppGridAlignments.TopCenter || aAlignment == uppGridAlignments.MiddleCenter || aAlignment == uppGridAlignments.BottomCenter) return uppHorizontalAlignments.Center;
            if (aAlignment == uppGridAlignments.TopRight || aAlignment == uppGridAlignments.MiddleRight || aAlignment == uppGridAlignments.BottomRight) return uppHorizontalAlignments.Right;

            return uppHorizontalAlignments.Undefined;
        }

    }

    public static class EnumsExtenstionMethods
    {
        public static string GetEnumValueNameList(this Type enumType, string valueNameSeparator = "=", string pairSeparator = ",")
        {
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException($"\"{enumType.FullName}\" is not an enum");
            }

            List<string> pairs = new List<string>();

            foreach (var value in Enum.GetValues(enumType))
            {
                pairs.Add($"{(int)value}{valueNameSeparator}{value}");
            }

            return string.Join(pairSeparator, pairs);
        }
    }
}
