using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;

namespace UOP.WinTray.Projects
{
    public class uopRingClipSegment : iSegment, ICloneable
    {

        #region Constructors
        public uopRingClipSegment(iSegment aSegment) => Init(aSegment);


        private void Init(iSegment aSegment = null)
        {
            RingClipSpacing = mdGlobals.DefaultRingClipSpacing;
        

            if (aSegment != null) Copy(aSegment);
        }


        #endregion Constructors

        #region Properties

        public uppSegmentTypes SegmentType => Segment == null ? uppSegmentTypes.Undefined : Segment.SegmentType;

        public iSegment Segment { get;  set; }

        public double RingClipSpacing { get; set; }
        public uopLine Line { get =>  SegmentType == uppSegmentTypes.Line ? (uopLine)Segment : null; set => Segment = value; }

        public uopArc Arc {  get  =>  SegmentType == uppSegmentTypes.Arc? (uopArc) Segment : null; set => Segment = value; }

        public uopVector StartPt => SegmentType == uppSegmentTypes.Undefined ? null : SegmentType == uppSegmentTypes.Arc ? Arc.StartPoint: Line.sp;
        public uopVector EndPt => SegmentType == uppSegmentTypes.Undefined ? null : SegmentType == uppSegmentTypes.Arc ? Arc.EndPoint : Line.ep;
        public uopVector MidPt => SegmentType == uppSegmentTypes.Undefined ? null : SegmentType == uppSegmentTypes.Arc ? Arc.MidPoint : Line.MidPt;


        public uopVectors Points { get => SegmentType == uppSegmentTypes.Undefined? null : SegmentType == uppSegmentTypes.Line? Line.Points : Arc.Points; set { if (SegmentType != uppSegmentTypes.Undefined) return; if (SegmentType == uppSegmentTypes.Line) Line.Points = value; else Arc.Points = value; } }

        #endregion Properties

        #region Methods
        public uopVectors EndPoints(bool bGetClones = false) => SegmentType == uppSegmentTypes.Undefined ? uopVectors.Zero : new uopVectors(bGetClones ? new uopVector(StartPt) : StartPt, bGetClones ? new uopVector(EndPt) : EndPt);

        public override string ToString() => SegmentType == uppSegmentTypes.Undefined ? "Undefined Segment" : SegmentType == uppSegmentTypes.Arc ? Arc.ToString() : Line.ToString() ;
        
        public uopRingClipSegment Clone ()=> new uopRingClipSegment (this);
        object ICloneable.Clone() => (object)new uopRingClipSegment(this);

        public void Copy(iSegment aSegment)
        {

            Segment = uopSegments.CloneCopy(aSegment);

            if (Segment == null) return;
            if (Segment.IsArc)
            { 
                Segment.Arc.Center.X = Math.Round(Segment.Arc.Center.X, 3);
                Segment.Arc.Center.Y = Math.Round(Segment.Arc.Center.Y, 3);
            }
            if (aSegment.GetType() == typeof(uopRingClipSegment))
            {
                uopRingClipSegment rcseg = (uopRingClipSegment)aSegment;
                RingClipSpacing = rcseg.RingClipSpacing;
            }
            
            
        }


        #endregion Methods

        #region iSegment Implementation

        public bool IsArc =>SegmentType == uppSegmentTypes.Arc;
        public double Radius => SegmentType == uppSegmentTypes.Arc ? Arc.Radius : 0;

        public double Length => SegmentType == uppSegmentTypes.Undefined ? 0 : SegmentType == uppSegmentTypes.Arc ? Arc.Length : Line.Length;
       iSegment iSegment.Clone() =>new uopRingClipSegment(this);

        #endregion iSegment Implementation

        #region Shared Methods


        public static List<dxfEntity> ToDXFEntities(IEnumerable<uopRingClipSegment> aSegments, dxfDisplaySettings aDisplaySettings = null)
        {
            if(aSegments == null) return new List<dxfEntity> ();
            List < dxfEntity > _rVal = new List<dxfEntity>();
            aDisplaySettings ??= new dxfDisplaySettings("RING CLIP SEGMENTS");
            foreach (var rcseg in aSegments)
            {
                if (rcseg.IsArc)
                    _rVal.Add(new dxeArc(rcseg.Arc) { DisplaySettings = aDisplaySettings });
                else
                    _rVal.Add(new dxeLine(rcseg.Line) { DisplaySettings = aDisplaySettings });
            }
            
            return _rVal;
        }

        #endregion Shared Methos
    }
}
