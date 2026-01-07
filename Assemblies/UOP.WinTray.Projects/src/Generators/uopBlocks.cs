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
    public static class uopBlocks
    {
        public static dxfBlock Attachements_Elevation_View(
            dxfImage aImage,
            mdBeam aPart,
            mdTrayAssembly aAssy,
            string aBlockName,
            out colDXFVectors refPoints,
            bool bShowBothTrays = true,
            bool bShowSupportEndView = false
           )
        {
            double ShellRad = aAssy.ShellRadius;
            refPoints = new colDXFVectors();
            colDXFEntities entities = new colDXFEntities();
            double sectionHt = ShellRad * 0.35;
            double ty = sectionHt / 2;
            double by = -ty;
            double gap = 3.0 / 25.4; //3 mm gap

            //left top ellipse
            double lx = -(ShellRad / 2);
            double majD = ShellRad;
            double minD = majD * 0.06;
            double majR = majD / 2;
            double minR = minD / 2;
            var elpl =  new dxeEllipse( new dxfVector( lx, ty ), majR, minR, 360, 180 );
            entities.Add( elpl );

            //right ellipse X
            double rx = lx + ShellRad;
            var elpr = new dxeEllipse( new dxfVector( rx, ty ), majR, minR );
            entities.Add( elpr );

            var elpbl = new dxeEllipse( new dxfVector( lx, by ), majR, minR );
            entities.Add( elpbl );
            var elpbr = new dxeEllipse( new dxfVector( rx, by ), majR, minR, 180, 360 );
            entities.Add( elpbr );

            entities.AddLine( -ShellRad, ty, -ShellRad, by );
            entities.AddLine( ShellRad, ty, ShellRad, by );

            //bottom phantom lines
            if (bShowBothTrays)
            {
                entities.AddLine( -ShellRad, gap, ShellRad, gap, new dxfDisplaySettings( aLinetype: dxfLinetypes.Phantom ) );
                entities.AddLine( -ShellRad, aAssy.RingThickness + gap, ShellRad, aAssy.RingThickness + gap, new dxfDisplaySettings( aLinetype: dxfLinetypes.Phantom ) );
            }
            //top phantom lines
            double topTrayY = gap + aAssy.RingThickness + aPart.Height;
            double topTrayY2 = topTrayY + aAssy.RingThickness;
            entities.AddLine( -ShellRad, topTrayY, ShellRad, topTrayY, new dxfDisplaySettings( aLinetype: dxfLinetypes.Phantom ) );
            entities.AddLine( -ShellRad, topTrayY2, ShellRad, topTrayY2, new dxfDisplaySettings( aLinetype: dxfLinetypes.Phantom ) );

            //beam supports
            if (bShowSupportEndView)
            {
                //define beam support block
                string bname = "BEAM_SUPPORT_ELEVATION_END_VIEW";
                dxfBlock block = null;

                if (!aImage.Blocks.TryGet( bname, ref block ))
                {
                    block = mdBlocks.BeamSupport_View_Elevation_End( aImage, aPart, aAssy, bname, false, false );
                    aImage.Blocks.Add( block );
                }

                if (aPart.OccuranceFactor > 1)
                {
                    var bref = new dxeInsert( block, new dxfVector( -aPart.Offset, 0 ), 0 );
                    entities.Add( bref );
                    bref = new dxeInsert( block, new dxfVector( aPart.Offset, 0 ), 0 );
                    entities.Add( bref );
                }
                else
                {
                    var bref = new dxeInsert( block, dxfVector.Zero, 1, 0 );
                    entities.Add( bref );
                }
            }
            else
            {
                //define beam support block
                string bname = "BEAM_SUPPORT_ELEVATION_VIEW";
                dxfBlock block = null;
                uopVectors ips = null;

                if (!aImage.Blocks.TryGet( bname, ref block ))
                {
                    block = mdBlocks.BeamSupport_View_Elevation( aImage, aPart, aAssy, bname, out ips, false, false );
                    aImage.Blocks.Add( block );
                }
                else
                {
                    ips = aPart.BeamSupportInsertionPoints;
                    ips.Rotate( dxfVector.Zero, -aPart.Rotation );
                }

                var lip = new dxfVector(ips[0].X, 0);
                var rip = new dxfVector(ips[1].X, 0);

                var bref = new dxeInsert( block, lip, 1, 0 );
                entities.Add( bref );
                bref = new dxeInsert( block, rip );
                bref.XScaleFactor = -1;
                entities.Add( bref );
            }

            refPoints.Add( new dxfVector( -ShellRad, by, aTag: "BOTTOMLEFTDIM" ) );
            refPoints.Add( new dxfVector( ShellRad, by, aTag: "BOTTOMRIGHTDIM" ) );
            refPoints.Add( new dxfVector( -ShellRad, topTrayY2, aTag: "TOP_OF_TRAY" ) );

            return new dxfBlock( entities, aBlockName );
        }
    }
}
