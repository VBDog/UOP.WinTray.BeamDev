using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    public class mdBeamSupport : uopPart
    {
     
        public override uppPartTypes BasePartType => uppPartTypes.BeamSupport;

        #region Constructors
        public mdBeamSupport() : base( uppPartTypes.BeamSupport, uppProjectFamilies.uopFamMD, "", "", false ) { InitializeProperties(); }
        public mdBeamSupport( mdBeam aBeam, bool isLeft = true ) : base( uppPartTypes.BeamSupport, uppProjectFamilies.uopFamMD, "", "", false ) { InitializeProperties( aBeam, isLeft ); }

        internal mdBeamSupport( mdBeamSupport aPartToCopy, uopPart aParent = null ) : base( uppPartTypes.BeamSupport, uppProjectFamilies.uopFamMD, "", "", false )
        {
            InitializeProperties(aPartToCopy != null ? aPartToCopy.Beam : null );
            if (aPartToCopy != null) base.Copy( aPartToCopy );
            SubPart( aParent );
        }

        public override bool IsEqual( uopPart aPart )
        {
            if (aPart == null) return false;
            if (aPart.GetType() != typeof( mdBeamSupport )) return false;
            return IsEqual( (mdBeamSupport)aPart );
        }

        public bool IsEqual( mdBeamSupport aPart )
        {
            if (aPart == null) return false;
            if (mzUtils.CompareVal( aPart.Width, Width, 4 ) != mzEqualities.Equals) return false;
            if (mzUtils.CompareVal( aPart.Height, Height, 4 ) != mzEqualities.Equals) return false;
            if (mzUtils.CompareVal( aPart.Length, Length, 4 ) != mzEqualities.Equals) return false;
            return true;
        }

        public void InitializeProperties( mdBeam aBeam = null, bool aIsLeft = true )
        {
            Rotation = 45;

            if(aBeam == null) return;
            
            //double supportHeight = aBeam.RingSpacing - (aBeam.Height + aBeam.TrayRange.GetMDTrayAssembly().WeirHeight + 3);
            //if (supportHeight < 0)
            //    supportHeight = aBeam.RingSpacing;

            AddProperty( "Width", aBeam.Width, aUnitType: uppUnitTypes.SmallLength );
            //AddProperty( "Height", supportHeight, aUnitType: uppUnitTypes.SmallLength );
            AddProperty( "BeamX", aBeam.X, aUnitType: uppUnitTypes.SmallLength );
            AddProperty( "BeamY", aBeam.Y, aUnitType: uppUnitTypes.SmallLength );

            SubPart( aBeam );
            Beam = aBeam;

            Rotation = aBeam.Rotation;
            Side = aIsLeft ? uppSides.Left : uppSides.Right;
            //define origin as mid point between beam holes on the left end in the beam's coordinate system
            uopHoleArray holes = aBeam.GenHoles( "BEAM" );
            var beamHoles = holes.Item( 1 );
            uopVectors ctrs = beamHoles.Centers;

            //align holes with beam plane
            ctrs.Move( -aBeam.Center.X, -aBeam.Center.Y );
            ctrs.Rotate( uopVector.Zero, -aBeam.Rotation );
            var vlsp = ctrs.GetVector( dxxPointFilters.GetLeftTop ).ToDXFVector();
            var vlep = ctrs.GetVector( dxxPointFilters.GetBottomLeft ).ToDXFVector();

            var vl = new dxeLine( vlsp, vlep );
            Center = new uopVector( vl.MidPt );

            if (!aIsLeft)
            {
                Center.Rotate( uopVector.Zero, 180 );
            }
        }
        #endregion  Constructors

        #region Properties

        private WeakReference<mdBeam> _BeamRef;
        internal mdBeam Beam 
        {
            get
            {
                mdBeam _rVal = null;
                if (_BeamRef != null)
                {
                    if (!_BeamRef.TryGetTarget(out _rVal)) _BeamRef = null;

                }
               
                return _rVal;

            }

            set
            {
                if (value == null)
                    _BeamRef = null;
                else 
                    _BeamRef = new WeakReference<mdBeam>(value);
            }
        }
        public double EdgeOffset => 0.75;
        public double HoleCenterOffset => 2.0;
        public double LegThk => 0.7087;
        public double FootLen => 0.9843;
        public double PlateThk => RingThickness;
        internal double _Height { get; set; } = 0;
        public override double Height 
        {
            get 
            {
                double gap = 3.0 / 25.4; //3 mm gap
                if (_Height == 0)
                {
                    mdBeam beam = Beam;
                    if(beam!= null)
                    _Height = beam.RingSpacing - (beam.GetMDTrayAssembly().WeirHeight + gap);
                }
                return _Height;
            }
        }

        public double BeamX { get => PropValD( "BeamX" ); set => PropValSet( "BeamX", value ); }
        public double BeamY { get => PropValD( "BeamY" ); set => PropValSet( "BeamY", value ); }

        #endregion  Properties

        #region Methods
        public mdBeamSupport Clone() => new mdBeamSupport( this );
        public override uopPart Clone( bool aFlag = false ) => (uopPart)this.Clone();

        public uopVectors PlateVerticies()
        {
            uopVectors _rVal = new uopVectors();

            //define the top and bottom edges of the plate
            var btopl = new dxeLine( new dxfVector( 0, Beam.Width / 2 ), new dxfVector( 10, Beam.Width / 2 ) );
            var bbotl = new dxeLine( new dxfVector( 0, Beam.Width / -2 ), new dxfVector( 10, Beam.Width / -2 ) );

            //find the intersections of the pate edges with the shell to define the plate
            dxeArc shell = new dxeArc( new dxfVector(0, -Beam.Offset), ShellID / 2 );
            var ints = shell.Intersections( btopl, theEntity_IsInfinite: true);
            var lt = ints.GetVector(dxxPointFilters.GetBottomLeft );
            ints = shell.Intersections( bbotl, theEntity_IsInfinite: true );
            var lb = ints.GetVector( dxxPointFilters.GetBottomLeft );

            //move the intersections to the origin from the beam's center so that they are relative to 0,0
            lt.Move( -Center.X, -Center.Y );
            lb.Move( -Center.X, -Center.Y );

            //return the plate profile in WCS coordinates for use in a block
            _rVal.Add( lt.X, lt.Y );
            _rVal.Add( HoleCenterOffset, lt.Y );
            _rVal.Add( HoleCenterOffset, lb.Y );
            _rVal.Add( lb.X, lb.Y, aRadius: -ShellRadius );

            return _rVal;
        }

        private uopVectors GetLegVerticies(dxeLine aTopLine, dxeLine aBottomLine)
        {
            uopVectors _rVal = new uopVectors();

            //find the intersections of the leg edges with the shell to define the leg
            dxeArc shell = new dxeArc( new dxfVector( 0, -Beam.Offset ), ShellID / 2 );
            var ints = shell.Intersections( aTopLine, theEntity_IsInfinite: true );
            var lt = ints.GetVector( dxxPointFilters.GetBottomLeft );
            ints = shell.Intersections( aBottomLine, theEntity_IsInfinite: true );
            var lb = ints.GetVector( dxxPointFilters.GetBottomLeft );

            //move the intersections to the origin from the beam's center so that they are relative to 0,0
            lt.Move( -Center.X, -Center.Y );
            lb.Move( -Center.X, -Center.Y );

            double footX = Math.Max( lt.X, lb.X ) + FootLen;

            //return the leg profile in WCS coordinates for use in a block
            _rVal.Add( lt.X, lt.Y );
            _rVal.Add( footX, lt.Y, aTag: "TOP_FOOT" );
            _rVal.Add( HoleCenterOffset, lt.Y );
            _rVal.Add( HoleCenterOffset, lb.Y );
            _rVal.Add( footX, lb.Y, aTag: "BOTTOM_FOOT" );
            _rVal.Add( lb.X, lb.Y );

            return _rVal;
        }

        public uopVectors MiddleLegVerticies ()
        {
            uopVectors _rVal = new uopVectors();

            //define the top and bottom edges of the leg
            var topl = new dxeLine( new dxfVector( 0, LegThk / 2.0 ), new dxfVector( 10, LegThk / 2.0 ) );
            var botl = new dxeLine( new dxfVector( 0, - LegThk / 2.0 ), new dxfVector( 10, - LegThk / 2.0 ) );

            return GetLegVerticies( topl, botl );
        }

        public uopVectors TopLegVerticies()
        {
            uopVectors _rVal = new uopVectors();

            //define the top and bottom edges of the leg
            var topl = new dxeLine(new dxfVector(0, (Beam.Width / 2) - EdgeOffset), new dxfVector( 10, (Beam.Width / 2) - EdgeOffset ) );
            var botl = new dxeLine( new dxfVector( 0, topl.Y - LegThk ), new dxfVector( 10, topl.Y - LegThk ) );

            return GetLegVerticies( topl, botl );
        }
        public uopVectors BottomLegVerticies()
        {
            uopVectors _rVal = new uopVectors();

            //define the top and bottom edges of the leg
            var botl = new dxeLine( new dxfVector( 0, -(Beam.Width / 2) + EdgeOffset ), new dxfVector( 10, -(Beam.Width / 2) + EdgeOffset ) );
            var topl = new dxeLine( new dxfVector( 0, botl.Y + LegThk ), new dxfVector( 10, botl.Y + LegThk ) );

            return GetLegVerticies( topl, botl );
        }

        public uopVectors GetBottomLegElevationVerticies()
        {
            uopVectors _rVal = new uopVectors();
            uopHoleArray holes = Beam.GenHoles( "BEAM" );

            var beamHoles = holes.Item( 1 );
            uopHole slot = beamHoles.Member;

            var legVs = TopLegVerticies(); //need top in liue of bottom for 2 beam systems as it's the shortes leg
            // simple outer profile
            _rVal.Add( legVs[ 0 ].X, -PlateThk );
            _rVal.Add( legVs[ 0 ].X, -Height, aTag: "RingSideBottom" );
            _rVal.Add( legVs[ 1 ].X, -Height, aTag: "AngleStart" );
            _rVal.Add( legVs[ 2 ].X, -(FootLen + PlateThk), aTag: "AngleEnd" );
            _rVal.Add( legVs[ 2 ].X, -PlateThk );

            // plate lines
            _rVal.Add( slot.Length / 2.0, -PlateThk, aTag: "Bottom_Right_Hole_Limit" );
            _rVal.Add( -slot.Length / 2.0, -PlateThk, aTag: "Bottom_Left_Hole_Limit" );
            _rVal.Add( _rVal.First() );
            _rVal.Add( legVs[ 0 ].X, 0, aTag: "RingSideTop" );
            _rVal.Add( -slot.Length / 2.0, 0, aTag: "Top_Left_Hole_Limit" );
            _rVal.Add(slot.Length / 2.0, 0, aTag: "Top_Right_Hole_Limit" );
            _rVal.Add( legVs[ 2 ].X, 0 );
            _rVal.Add( legVs[ 2 ].X, -PlateThk );

            return _rVal;
        }

        public uopVectors GetMidLegElevationVerticies()
        {
            uopVectors _rVal = new uopVectors();
            var botVerts = GetBottomLegElevationVerticies();
            var midVerts = MiddleLegVerticies();

            _rVal.Add( midVerts[ 0 ].X, -PlateThk, aTag: "RingSideTop" );
            _rVal.Add( midVerts[ 0 ].X, -Height, aTag: "RingSideBottom" );

            double xdif = Math.Abs(_rVal.Min( v => v.X ) - botVerts.Min( v => v.X));
            var ringpt = _rVal[ 1 ];

            // get angled points
            var bl = botVerts.GetByTag( "AngleStart" ).ToDXFVectors().First();
            var tr = botVerts.GetByTag( "AngleEnd" ).ToDXFVectors().First();
            var aline = new dxeLine( bl, tr );

            double dst = ringpt.DistanceTo( bl );
            if (xdif > ringpt.DistanceTo(bl))
            {
                //no angled segment, gap is too small
                _rVal.Add( new dxfVector( ringpt.X + xdif, ringpt.Y ), "MidLegVisbileBottom" );
            }
            else
            {
                // get trimer
                var legtl = botVerts.GetByTag( "RingSideTop" ).ToDXFVectors().First();
                var legbl = botVerts.GetByTag( "RingSideBottom" ).ToDXFVectors().First();
                var tline = new dxeLine( legtl, legbl );

                _rVal.Add( midVerts[ 1 ].X, -Height, aTag: "AngleStart" );

                // move the angled line to the bottom of the leg
                aline.MoveFromTo( bl, _rVal.Last() );
                var ints = tline.Intersections( aline );
                if (ints.Count > 0)
                    _rVal.Add( ints[ 0 ].X, ints[ 0 ].Y, aTag: "AngleEnd" );
            }

            return _rVal;
        }

        public uopVectors GetTopLegElevationVerticies()
        {
            if (Beam.OccuranceFactor == 1)
                return GetBottomLegElevationVerticies();

            uopVectors _rVal = new uopVectors();
            var botVerts = GetBottomLegElevationVerticies();
            var midVerts = GetMidLegElevationVerticies();
            var topVerts = BottomLegVerticies();

            // get angled points
            var bl = botVerts.GetByTag( "AngleStart" ).ToDXFVectors().First();
            var tr = botVerts.GetByTag( "AngleEnd" ).ToDXFVectors().First();
            var aline = new dxeLine( bl, tr );

            // get trimer
            var legtl = midVerts.GetByTag( "RingSideTop" ).ToDXFVectors().First();
            var legbl = midVerts.GetByTag( "RingSideBottom" ).ToDXFVectors().First();
            var tline = new dxeLine( legtl, legbl );

            _rVal.Add( topVerts[ 0 ].X, -PlateThk, aTag: "RingSideTop" );
            _rVal.Add( topVerts[ 0 ].X, -Height, aTag: "RingSideBottom" );
            _rVal.Add( topVerts[ 1 ].X, -Height );

            // move the angled line to the bottom of the leg
            aline.MoveFromTo( bl, _rVal.Last() );
            var ints = tline.Intersections( aline );
            _rVal.Add( ints[ 0 ].X, ints[ 0 ].Y );

            return _rVal;
        }

        public uopVectors PlateEndViewVerticies ()
        {
            uopVectors _rVal = new uopVectors();

            double leftX = -Beam.Width / 2.0;
            double rightX = Beam.Width / 2.0;
            double bottomY = -PlateThk;

            uopHoleArray holes = Beam.GenHoles( "BEAM" );
            var beamHoles = holes.Item( 1 );
            uopHole slot = beamHoles.Member;
            double hSpacing = (from v1 in beamHoles.Centers
                               from v2 in beamHoles.Centers
                               where v1 != v2
                               select v1.DistanceTo( v2 )).Min();

            double leftHoleLeftX = -((hSpacing / 2.0) + slot.Radius);
            double leftHoleRightX = leftHoleLeftX + slot.Diameter;
            double rightHoleLeftX = (hSpacing / 2.0) - slot.Radius;
            double rightHoleRightX = rightHoleLeftX + slot.Diameter;

            _rVal.Add( leftX, 0 );
            _rVal.Add(leftHoleLeftX, 0, aTag: "Left_Hole_Left" );
            _rVal.Add(leftHoleLeftX + slot.Radius, 0, aTag: "Left_Hole_Center" );
            _rVal.Add(leftHoleRightX, 0, aTag: "Left_Hole_Right" );
            _rVal.Add(rightHoleLeftX, 0, aTag: "Right_Hole_Left" );
            _rVal.Add( rightHoleLeftX + slot.Radius, 0, aTag: "Right_Hole_Center" );
            _rVal.Add( rightHoleRightX, 0, aTag: "Right_Hole_Right" );
            _rVal.Add( rightX, 0 );
            _rVal.Add( rightX, bottomY );
            _rVal.Add( rightX - EdgeOffset, bottomY, aTag: "Right_Leg_Right" );
            _rVal.Add(_rVal.Last().X - LegThk, bottomY, aTag: "Right_Leg_Left" );
            _rVal.Add( rightHoleRightX, bottomY, aTag: "Right_Hole_Right_Bottom" );
            _rVal.Add( rightHoleLeftX, bottomY, aTag: "Right_Hole_Left_Bottom" );
            _rVal.Add(LegThk / 2.0, bottomY, aTag: "Middle_Leg_Right" );
            _rVal.Add(-LegThk / 2.0, bottomY, aTag: "Middle_Leg_Left" );
            _rVal.Add(leftHoleRightX, bottomY, aTag: "Left_Hole_Right_Bottom" );
            _rVal.Add( leftHoleLeftX, bottomY, aTag: "Left_Hole_Left_Bottom" );
            _rVal.Add(leftX + EdgeOffset + LegThk, bottomY, aTag: "Left_Leg_Right" );
            _rVal.Add( leftX + EdgeOffset, bottomY, aTag: "Left_Leg_Left" );
            _rVal.Add( leftX, bottomY );

            return _rVal;
        }

        public uopVectors LegEndViewVerticies()
        {
            uopVectors _rVal = new uopVectors();
            double leftX = -LegThk / 2.0;
            double rightX = LegThk / 2.0;
            double footY = -(FootLen + PlateThk);

            _rVal.Add( leftX, -PlateThk );
            _rVal.Add( leftX, footY, aTag: "Foot_Left" );
            _rVal.Add( leftX, -Height );
            _rVal.Add( rightX, -Height );
            _rVal.Add( rightX, footY, aTag: "Foot_Right" );
            _rVal.Add( rightX, -PlateThk );
            return _rVal;
        }


        #endregion  Methods
    }
}
