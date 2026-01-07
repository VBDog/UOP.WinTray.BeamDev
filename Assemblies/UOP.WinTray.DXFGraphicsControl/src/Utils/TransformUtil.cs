using System;
using System.Collections.Generic;
using System.Linq;
using UOP.DXFGraphics;
using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Tables;
using WW.Math;


namespace UOP.DXFGraphicsControl.Utils
{
    public partial class TransformUtil : IDisposable
    {
        private DXFViewer viewer;
        private DxfModel model;
        private ModelWithHandles modelwh;
        private bool disposedValue;
        private readonly bool skipDispose = false;

        public List<DxfMessage> messages { get; set; }

        public bool isDisposed
        {
            get
            {
                return disposedValue;
            }
        }

        public TransformUtil( DXFViewer viewer )
        {
            this.viewer = viewer;
            modelwh = viewer.modelwh;
            model = modelwh.Model;
            messages = new List<DxfMessage>();
            Extensions.ClearArrowBlocksCache();
        }

        public TransformUtil( DXFViewer viewer, bool skipDispose )
        {
            this.skipDispose = skipDispose;
            this.viewer = viewer;
            modelwh = viewer.modelwh;
            model = modelwh.Model;
            messages = new List<DxfMessage>();
            Extensions.ClearArrowBlocksCache();
        }

        public TransformUtil( DXFViewer viewer, ModelWithHandles Modelwh )
        {
            this.viewer = viewer;
            modelwh = Modelwh;
            model = modelwh.Model;
            messages = new List<DxfMessage>();
            Extensions.ClearArrowBlocksCache();
        }

        public void UpdateModels(ModelWithHandles ModelWh)
        {
            if (model != null)
            {
                modelwh.ClearHandlers();
                model.Dispose();
                model = null;
                modelwh = null;
            }
            modelwh = ModelWh;
            model = modelwh.Model;
        }

        private bool IsDimension(dxfEntity entity)
        {
            return entity.OwnerGUID != null && entity.OwnerGUID.StartsWith("DIMENSION");
        }

        public void MatchEntitySettings(DxfEntity targetEnt, dxfEntity sourceEnt, dxfVector vector = null)
        {
            vector = null;//instances should always match original entity per Mike Z
            if (vector != null && vector.Color != dxxColors.ByBlock)
                targetEnt.Color = EntityColor.CreateFromColorIndex( (short)vector.Color );
            else
                targetEnt.Color = EntityColor.CreateFromColorIndex( (short)sourceEnt.Color );

            if (vector != null && !string.IsNullOrWhiteSpace( vector.LayerName))
                targetEnt.Layer = viewer.GetLayer( vector.LayerName );
            else
                targetEnt.Layer = viewer.GetLayer( sourceEnt );

            if (vector != null && !string.IsNullOrWhiteSpace(vector.Linetype) && vector.Linetype != "ByBlock")
                targetEnt.LineType = viewer.GetLineType( vector.Linetype );
            else
                targetEnt.LineType = viewer.GetLineType( sourceEnt );

            if (vector != null && vector.LTScale != 0)
                SetLineTypeScaleForDXFEntity( targetEnt, vector.LTScale );
            else
                SetLineTypeScaleForDXFEntity( targetEnt, sourceEnt.LTScale );

            targetEnt.LineWeight = (short)sourceEnt.LineWeight;
            
       
        }

        private void InvertEntity(DxfEntity entity)
        {
            BoundsCalculator boundsCalculator = new BoundsCalculator();
            boundsCalculator.GetBounds( model, entity );
            var ebounds = boundsCalculator.Bounds;
            var center = ebounds.Center;
            var tconfig = new TransformConfig();

            var trans = Transformation4D.Translation( (Vector3D)ebounds.Center ) *
            Transformation4D.RotateZ( DegreeToRadian( 180 ) ) *
            Transformation4D.Translation( -(Vector3D)ebounds.Center );

            entity.TransformMe( tconfig, trans );
        }

        public void RemoveDxfEntity(dxfEntity entity)
        {
            if (entity.DxfHandles == null)
                entity.DxfHandles = new ulong[ 0 ];
            var handles = entity.DxfHandles.ToList();
            if (entity.DxfHandles != null && entity.DxfHandles.Count() > 0)
            {
                for (int i = 0; i < handles.Count(); i++)
                {
                    var ent = modelwh.GetEntity( handles[ i ] );
                    model.Entities.Remove( ent );
                }
            }
        }

        public (List<DxfEntity>, List<DxfEntity>, List<DxfBlock>) ConvertToCADLibObjects(dxfEntity entity, dxfImage iImage)
        {
            if (entity == null)
            {
                return (null, null, null);
            }
            entity.UpdatePath(bRegen: false,aImage: iImage);

            List<DxfEntity> addedDxfEntities = null;
            List<DxfEntity> updatedDxfEntities = null;
            List<DxfBlock> addedDxfBlocks = null;

            switch (entity.EntityType)
            {
                case dxxEntityTypes.SequenceEnd:
                    //Not supported / used
                    break;
                case dxxEntityTypes.EndBlock:
                    //Not supported / used
                    break;
                case dxxEntityTypes.Undefined:
                    //Not supported / used
                    break;
                case dxxEntityTypes.Line:
                    (addedDxfEntities, updatedDxfEntities) = TransformToLine(entity, iImage);
                    break;
                case dxxEntityTypes.Polyline:
                    (addedDxfEntities, updatedDxfEntities) = TransformToPolyline(entity, iImage);
                    break;
                case dxxEntityTypes.Arc:
                    (addedDxfEntities, updatedDxfEntities) = TransformToArc(entity, iImage);
                    break;
                case dxxEntityTypes.Circle:
                    (addedDxfEntities, updatedDxfEntities) = TransformToCircle(entity, iImage);
                    break;
                case dxxEntityTypes.Ellipse:
                    (addedDxfEntities, updatedDxfEntities) = TransformToEllipse(entity);
                    break;
                case dxxEntityTypes.Bezier:
                    (addedDxfEntities, updatedDxfEntities) = TransformToBezier( entity );
                    break;
                 case dxxEntityTypes.Point:
                    (addedDxfEntities, updatedDxfEntities) = TransformToPoint(entity);
                    break;
                case dxxEntityTypes.Trace:
                case dxxEntityTypes.Solid:
                    (addedDxfEntities, updatedDxfEntities) = TransformToSolid(entity);
                    break;
              
                case dxxEntityTypes.Insert:
                    (addedDxfEntities, updatedDxfEntities) = TransformToInsert(entity, iImage);
                    break;
                case dxxEntityTypes.Table:
                    (addedDxfEntities, updatedDxfEntities) = TransformToTable(entity, iImage);
                    break;
                case dxxEntityTypes.Polygon:
                    (addedDxfEntities, updatedDxfEntities) = TransformToPolygon(entity, iImage);
                    break;

                case dxxEntityTypes.Hole:
                    (addedDxfEntities, updatedDxfEntities) = TransformToHoleBlock(entity, iImage);
                    break;
                case dxxEntityTypes.Slot:
                    (addedDxfEntities, updatedDxfEntities) = TransformToSlotBlock(entity, iImage);
                    break;
                case dxxEntityTypes.Text:
                    (addedDxfEntities, updatedDxfEntities) = TransformToText(entity);
                    break;
                case dxxEntityTypes.Attdef:
                    (addedDxfEntities, updatedDxfEntities) = TransformToAttDef( entity );
                    break;
                case dxxEntityTypes.Attribute:
                    (addedDxfEntities, updatedDxfEntities) = TransformToAttrib(entity);
                    break;
                case dxxEntityTypes.MText:
                    (addedDxfEntities, updatedDxfEntities) = TransformToMText(entity);
                    break;
                case dxxEntityTypes.Character:
                    //Not supported / used
                    break;
                case dxxEntityTypes.Hatch:
                    (addedDxfEntities, updatedDxfEntities) = TransformToHatch( entity );
                    break;
                case dxxEntityTypes.DimLinearH:
                    (addedDxfEntities, updatedDxfEntities) = TransformToHorizontalLinearDimension(entity, iImage);
                    break;
                case dxxEntityTypes.DimLinearV:
                    (addedDxfEntities, updatedDxfEntities) = TransformToVerticalLinearDimension(entity, iImage);
                    break;
                case dxxEntityTypes.DimLinearA:
                    (addedDxfEntities, updatedDxfEntities) = TransformToLinearAlignedDimension(entity, iImage);
                    break;
                case dxxEntityTypes.DimOrdinateH:
                    (addedDxfEntities, updatedDxfEntities) = TransformToHorizontalOrdinateDimension(entity, iImage);
                    break;
                case dxxEntityTypes.DimOrdinateV:
                    (addedDxfEntities, updatedDxfEntities) = TransformToVerticalOrdinateDimension(entity, iImage);
                    break;
                case dxxEntityTypes.DimRadialR:
                    (addedDxfEntities, updatedDxfEntities) = TransformToRadiusRadialDimension(entity, iImage);
                    break;
                case dxxEntityTypes.DimRadialD:
                    (addedDxfEntities, updatedDxfEntities) = TransformToDiametericRadialDimension(entity, iImage);
                    break;
                case dxxEntityTypes.DimAngular:
                    (addedDxfEntities, updatedDxfEntities) = TransformToAngular4PDimension(entity, iImage);
                    break;
                case dxxEntityTypes.DimAngular3P:
                    (addedDxfEntities, updatedDxfEntities) = TransformToAngular3PDimension(entity, iImage);
                    break;
                case dxxEntityTypes.LeaderText:
                    (addedDxfEntities, updatedDxfEntities) = TransformToLeader( entity );
                    break;
                case dxxEntityTypes.LeaderTolerance:
                    //Not supported / used
                    break;
                case dxxEntityTypes.LeaderBlock:
                case dxxEntityTypes.Leader:
                    (addedDxfEntities, updatedDxfEntities) = TransformToLeader( entity );
                    break;
                case dxxEntityTypes.Symbol:
                    (addedDxfEntities, updatedDxfEntities) = TransformToSymbol( entity, iImage );
                    break;
                case dxxEntityTypes.Shape:
                    (addedDxfEntities, updatedDxfEntities) = TransformToShape(entity, iImage);
                    break;
                default:
                    break;
            }

            return (addedDxfEntities, updatedDxfEntities, addedDxfBlocks);
        }

        public DxfEntity ReadDxfEntityUsingHandle(ulong[] handles)
        {
            if (handles != null && handles.Count() > 0)
                return ReadDxfEntityUsingHandle( handles[ 0 ] );

            return null;
        }

        public DxfEntity ReadDxfEntityUsingHandle(ulong handle)
        {
            return modelwh?.GetEntity( handle );
        }

        private DxfBlock ReadDxfBlockUsingName(string blockName)
        {
            return model.GetBlockWithName(blockName);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToCircle(dxfEntity entity, dxfImage iImage)
        {
            dxeArc circle = entity as dxeArc;
            if (circle == null || IsDimension(entity))
            {
                return (null, null);
            }
            if (circle.HasWidth)
            {
                return TransformToPolyline( circle.ConvertToPolyline(), iImage);

            }

            return TransformToPrimitives(entity, circle.Instances, new List<dxfVector>() { circle.Center }, circle.Radius, null, (added, updated, ent, points, radius, vector, angles, handles) => {
                DxfCircle dxfCircle;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfCircle = new DxfCircle();
                    added.Add( dxfCircle );
                }
                else
                {
                    dxfCircle = (DxfCircle)existing;
                    updated.Add( dxfCircle );
                }

                dxfCircle.Center = ConvertToPoint3D( points[ 0 ] );
                if (vector != null && vector.Radius > 0)
                {
                    dxfCircle.Radius = vector.Radius;
                }
                else
                {
                    dxfCircle.Radius = radius;
                }

                dxfCircle.ZAxis = ConvertDirection3D(circle.ZDirection);

                MatchEntitySettings( dxfCircle, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToLine( dxfEntity entity, dxfImage iImage)
        {
            dxeLine line = entity as dxeLine;
            if (line == null || IsDimension( entity ))
            {
                return (null, null);
            }

            if(line.HasWidth)
            {
                return TransformToPolyline(line.ConvertToPolyline(),iImage);
            }

            return TransformToPrimitives( entity, line.Instances, new List<dxfVector>() { line.StartPt, line.EndPt }, 0.0, null, ( added, updated, ent, points, radius, vector, angles, handles ) =>
            {
                DxfLine dxfLine;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfLine = new DxfLine();
                    added.Add( dxfLine );
                }
                else
                {
                    dxfLine = (DxfLine)existing;
                    updated.Add( dxfLine );
                }

                dxfLine.ZAxis = ConvertDirection3D(line.ZDirection);
                dxfLine.Start = ConvertToPoint3D( points[ 0 ] );
                dxfLine.End = ConvertToPoint3D( points[ 1 ] );

                MatchEntitySettings( dxfLine, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToHatch( dxfEntity entity )
        {
            dxeHatch dhatch = entity as dxeHatch;
         
            if (dhatch == null) return (null, null);
            colDXFEntities bounds = dhatch.BoundingEntities;
            if (bounds.Count <=0) 
                return (null, null);

            return TransformToPrimitives( entity, dhatch.Instances, new List<dxfVector>(), 0.0, null, ( added, updated, ent, unused, radius, vector, angles, handles ) =>
            {
                DxfHatch dxfHatch;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );

                //List<Point2D> points = new List<Point2D>();

                dxfEntity dxloop = bounds.Count > 0 ? bounds.Item( 1 ) : null;
               
                List<DxfEntity> loops = TransformToDXF(dxloop, null,null ,true);

                DxfHatch.BoundaryPath boundaryPath = null;
                DxfPolyline2D loop = null;
                if ( loops != null)
                {
                    // added.AddRange( loops );
                    loop = loops.FirstOrDefault() as DxfPolyline2D;
                    if (loop == null) return; //can only process polylines for now

                    var points = new List<DxfHatch.BoundaryPath.Polyline.Vertex>();
                    foreach (var vtx in loop.Vertices)
                    {
                        points.Add(new DxfHatch.BoundaryPath.Polyline.Vertex(vtx.X, vtx.Y, vtx.Bulge));
                    }

                    boundaryPath = new DxfHatch.BoundaryPath
                    {
                        Type = BoundaryPathType.External | BoundaryPathType.Polyline | BoundaryPathType.Derived,
                        PolylineData = new DxfHatch.BoundaryPath.Polyline(true, points.ToArray()),
                        BoundaryObjects = {
                        loop
                    }
                    };

                }

                if (existing == null)
                {
                    dxfHatch = new DxfHatch()
                    {
                        Associative = true,
                        HatchStyle = WW.Cad.Model.Entities.HatchStyle.Outer,
                        BoundaryPaths = {
                            boundaryPath
                        }
                    };
                    dxfHatch.SeedPoints.Add( new Point2D( dhatch.Center.X, dhatch.Center.Y ) );
                    // This is also part of the association.
                    if (loop != null) loop.AddPersistentReactor( dxfHatch );
                    added.Add( dxfHatch );
                }
                else
                {
                    dxfHatch = (DxfHatch)existing;
                    updated.Add( dxfHatch );
                }
                dxfHatch.ZAxis = ConvertDirection3D(dhatch.ZDirection);
                dxfHatch.Pattern = new DxfPattern();
                if (dhatch.HatchStyle == dxxHatchStyle.dxfHatchSolidFill)
                {
                   // dxfHatch.Pattern.
                } 
                else
                {
                    DxfPattern.Line patternLine = new DxfPattern.Line();
                    dxfHatch.Pattern.Lines.Add(patternLine);
                    patternLine.Angle = DegreeToRadian(dhatch.Rotation);

                    Vector2D offset = Vector2D.FromAngle(patternLine.Angle + Math.PI * 0.5) * dhatch.LineStep;
                    patternLine.Offset = offset;

                }
                dxfHatch.Scale = dhatch.ScaleFactor;
                if(dhatch.HatchStyle == dxxHatchStyle.dxfHatchUserDefined)
                {
                    dxfHatch.IsDouble = dhatch.Doubled;
                }
                MatchEntitySettings( dxfHatch, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToBezier( dxfEntity entity )
        {
            dxeBezier bezier = entity as dxeBezier;
            if (bezier == null || IsDimension( entity ))
            {
                return (null, null);
            }

            var bpoints = new List<dxfVector>()
            {
                bezier.StartPt,
                bezier.ControlPt1,
                bezier.ControlPt2,
                bezier.EndPt
            };

            return TransformToPrimitives( entity, bezier.Instances, bpoints, 0.0, null, ( added, updated, ent, points, radius, vector, angles, handles ) =>
            {
                DxfSpline dxfBezier;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfBezier = new DxfSpline( 3 );
                    added.Add( dxfBezier );
                }
                else
                {
                    dxfBezier = (DxfSpline)existing;
                    updated.Add( dxfBezier );
                }

                dxfBezier.ControlPoints.Add( ConvertToPoint3D( points[ 0 ] ) );
                dxfBezier.ControlPoints.Add( ConvertToPoint3D( points[ 1 ] ) );
                dxfBezier.ControlPoints.Add( ConvertToPoint3D( points[ 2 ] ) );
                dxfBezier.ControlPoints.Add( ConvertToPoint3D( points[ 3 ] ) );
                dxoProperties bezprops = bezier.ActiveProperties();

                for (int k = 1; k <= dxfBezier.ExpectedKnotCount; k++)
                {
                    dxoProperty prop = bezprops.Member( "Knot Value", k);
                    if(prop != null)
                        dxfBezier.Knots.Add( Convert.ToDouble( prop.Value ) );
                    else
                        dxfBezier.Knots.Add(0);
                }
                dxfBezier.KnotParameterization = KnotParameterization.Chord;
                dxfBezier.Flags = SplineFlags.Planar;
                dxfBezier.UpdateSplineFromFitPoints();
              
                dxfBezier.ZAxis = ConvertDirection3D(bezier.ZDirection);

                MatchEntitySettings( dxfBezier, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToPolyline(dxfEntity entity, dxfImage iImage)
        {
            dxePolyline polyline = entity as dxePolyline;

            if (polyline == null || IsDimension(entity))
            {
                return (null, null);
            }
          
            var bpoints = new List<dxfVector>();
            bool colorOrLtChange = false;
            bool ispgon = entity.GraphicType == dxxGraphicTypes.Polygon;
            
            for (int i = 1; i <= polyline.VertexCount; i++)
            {
                var pt = polyline.Vertices.Item( i );
                
                if (ispgon)
                {
                    if (pt.Color != dxxColors.ByLayer && pt.Color != dxxColors.ByBlock && pt.Color != polyline.Color) colorOrLtChange = true;
                    if (pt.Linetype != "ByLayer" && !string.IsNullOrEmpty(pt.Linetype) && pt.Linetype != polyline.Linetype) colorOrLtChange = true;
                    bpoints.Add(pt);
                }
                else
                {
                    bpoints.Add(pt);
                }
            }
            if (bpoints.Count <= 1)
            {
                return (null, null);
            }

            if(colorOrLtChange)
            {
                return TransformToInsert( polyline, iImage );
            }

            return TransformToPrimitives( entity, polyline.Instances, bpoints, 0.0, null, ( added, updated, ent, points, radius, vector, angles, handles ) =>
            {
                DxfPolyline2D dxfPolyline2D;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfPolyline2D = new DxfPolyline2D();
                    added.Add( dxfPolyline2D );
                }
                else
                {
                    dxfPolyline2D = (DxfPolyline2D)existing;
                    updated.Add( dxfPolyline2D );
                }

                var verticies = new List<DxfVertex2D>();
                dxfPolyline2D.ZAxis = ConvertDirection3D(polyline.ZDirection);
                dxfPolyline2D.Vertices.Clear();
                dxfPolyline2D.Vertices.AddRange( GetVertices(points) );
                dxfPolyline2D.Closed = polyline.Closed;
                dxfPolyline2D.DefaultStartWidth = polyline.SegmentWidth;
                dxfPolyline2D.DefaultEndWidth = polyline.SegmentWidth;

                if (vector != null && vector.Inverted)
                {
                    InvertEntity( dxfPolyline2D );
                }

                MatchEntitySettings( dxfPolyline2D, ent, vector );
                foreach (var vtx in dxfPolyline2D.Vertices)
                {
                    vtx.LineType = dxfPolyline2D.LineType;
                    vtx.Color = dxfPolyline2D.Color;
                }
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToPolygon(dxfEntity entity, dxfImage iImage)
        {
            dxePolygon polygon = entity as dxePolygon;
            var pblock = polygon.Block("", false, iImage);

            return TransformToInserts( entity, pblock, null, polygon.Instances, polygon.HandlePt, new dxfVector(), 0.0, new Vector3D(1, 1, 1), iImage, ( added, updated, ent, block, insert, point, vector, rotation, scale, handles ) =>
            {
                DxfInsert dxfInsert;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfInsert = new DxfInsert( block, ConvertToPoint3D(point) );
                    added.Add( dxfInsert );
                }
                else
                {
                    dxfInsert = existing as DxfInsert;
                    dxfInsert.Block = block;
                    updated.Add( dxfInsert );
                }
                dxfInsert.ZAxis = ConvertDirection3D(polygon.ZDirection);
                dxfInsert.Rotation = rotation;
                dxfInsert.ScaleFactor = scale;

                if (vector != null)
                {
                    if (vector.Inverted)
                    {
                        InvertEntity( dxfInsert );
                    }
                    dxfInsert.Rotation = DegreeToRadian(vector.Rotation);
                }

                MatchEntitySettings( dxfInsert, ent, vector );
            } );
        }
        private (List<DxfEntity>, List<DxfEntity>) TransformToShape(dxfEntity entity, dxfImage iImage)
        {
            dxeShape shape = entity as dxeShape;


            var pblock = shape.GetBlock(iImage);


            return TransformToInserts(entity, pblock, null, shape.Instances, shape.InsertionPt, shape.HandlePt, 0.0, new Vector3D(1, 1, 1), iImage,
                (added, updated, ent, block, insert, point, vector, rotation, scale, handles) =>
                {
                    DxfInsert dxfInsert;
                    DxfEntity existing = ReadDxfEntityUsingHandle(handles);
                    if (existing == null)
                    {
                        dxfInsert = new DxfInsert(block, new Point3D(shape.InsertionPt.X, shape.InsertionPt.Y, shape.InsertionPt.Z));
                        added.Add(dxfInsert);
                    }
                    else
                    {
                        dxfInsert = existing as DxfInsert;
                        dxfInsert.Block = block;
                        updated.Add(dxfInsert);
                    }

                    dxfInsert.ZAxis = ConvertDirection3D(shape.ZDirection);
                    dxfInsert.Rotation = DegreeToRadian(rotation);
                    dxfInsert.ScaleFactor = scale;

                    if (vector != null)
                    {
                        if (vector.Inverted)
                        {
                            InvertEntity(dxfInsert);
                        }
                        dxfInsert.Rotation = DegreeToRadian(vector.Rotation);
                    }

                    MatchEntitySettings(dxfInsert, ent, vector);
                });
        }
        private (List<DxfEntity>, List<DxfEntity>) TransformToSymbol( dxfEntity entity, dxfImage iImage )
        {
            dxeSymbol symbol = entity as dxeSymbol;
            dxfAttributes atts = null;
            colDXFEntities eatts = null;
            bool hasLeader = false;

            var pblock = symbol.GetBlock(  iImage, ref atts, ref eatts, ref hasLeader);

            return TransformToInserts( entity, pblock, null, symbol.Instances, symbol.InsertionPt, symbol.HandlePt, 0.0, new Vector3D( 1, 1, 1 ), iImage,
                ( added, updated, ent, block, insert, point, vector, rotation, scale, handles ) =>
            {
                DxfInsert dxfInsert;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfInsert = new DxfInsert( block, new Point3D( symbol.InsertionPt.X, symbol.InsertionPt.Y, symbol.InsertionPt.Z ) );
                    added.Add( dxfInsert );
                }
                else
                {
                    dxfInsert = existing as DxfInsert;
                    dxfInsert.Block = block;
                    updated.Add( dxfInsert );
                }
                dxfInsert.ZAxis = ConvertDirection3D(symbol.ZDirection);
                dxfInsert.Rotation = DegreeToRadian(rotation);
                dxfInsert.ScaleFactor = scale;

                if (vector != null)
                {
                    if (vector.Inverted)
                    {
                        InvertEntity( dxfInsert );
                    }
                    dxfInsert.Rotation = DegreeToRadian( vector.Rotation );
                }

                MatchEntitySettings( dxfInsert, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToEllipse(dxfEntity entity)
        {
            dxeEllipse ellipse = entity as dxeEllipse;
            if (ellipse == null || IsDimension(entity))
            {
                return (null, null);
            }

            return TransformToPrimitives( entity, ellipse.Instances, new List<dxfVector>() { ellipse.Center, ellipse.MajorAxis.StartPt, ellipse.MajorAxis.EndPt }, ellipse.MinorDiameter / ellipse.MajorDiameter, null, 
                ( added, updated, ent, points, minorToMajorRatio, vector, angles, handles ) =>
            {
                Vector3D majorAxisEndPoint = new Vector3D( (points[2].X - points[ 1 ].X) / 2, (points[ 2 ].Y - points[ 1 ].Y) / 2, (points[ 2 ].Z - points[ 1 ].Z) / 2 );

                DxfEllipse dxfEllipse;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfEllipse = new DxfEllipse();
                    added.Add( dxfEllipse );
                }
                else
                {
                    dxfEllipse = (DxfEllipse)existing;
                    updated.Add( dxfEllipse );
                }

                dxfEllipse.Center = ConvertToPoint3D( points[0] );
                dxfEllipse.MajorAxisEndPoint = majorAxisEndPoint;
                dxfEllipse.MinorToMajorRatio = minorToMajorRatio;
                dxfEllipse.ZAxis = ConvertDirection3D(ellipse.ZDirection);
                if (Math.Abs(ellipse.EndAngle - ellipse.StartAngle) < 360 && ellipse.StartAngle != ellipse.EndAngle )
                {
                    dxfEllipse.StartParameter = DxfEllipse.AngleToParameter( DegreeToRadian( ellipse.StartAngle ), ellipse.Ratio );
                    dxfEllipse.EndParameter = DxfEllipse.AngleToParameter( DegreeToRadian( ellipse.EndAngle ), ellipse.Ratio );
                }
                MatchEntitySettings( dxfEllipse, ent, vector );
            } );
        }
     

        private (List<DxfEntity>, List<DxfEntity>) TransformToSolid(dxfEntity entity)
        {
            dxeSolid solid = entity as dxeSolid;
            if (solid == null || IsDimension(entity))
            {
                return (null, null);
            }

            return TransformToPrimitives( entity, solid.Instances, GetSolidVertices(solid), 0.0, null, ( added, updated, ent, points, radius, vector, angles, handles ) =>
            {
                DxfSolid dxfSolid;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfSolid = new DxfSolid();
                    added.Add( dxfSolid );
                }
                else
                {
                    dxfSolid = (DxfSolid)existing;
                    updated.Add( dxfSolid );
                }

                dxfSolid.Points.Clear();
               
                foreach (var v in points)
                {
                    dxfSolid.Points.Add( ConvertToPoint3D(v) );
                    if (dxfSolid.Points.Count == 4) break;
                }
                if(dxfSolid.Points.Count == 3)
                {
                    dxfSolid.Points.Add(ConvertToPoint3D(points.Count > 0 ? points[0] : dxfVector.Zero));
                }
                dxfSolid.ZAxis = ConvertDirection3D(solid.ZDirection);

                MatchEntitySettings( dxfSolid, ent, vector );
            } );
        }

        private List<DxfEntity> AddLinearDimension( dxeDimension dimension, dxfImage iImage)
        {
            if (dimension == null)
            {
                return null;
            }

            var dstyle = viewer.GetDimensionStyle( dimension.DimStyleName );
            double rotation = DegreeToRadian( dimension.Angle );
            if (rotation == 0 && dimension.LinearDimensionType == dxxLinearDimTypes.LinearVertical)
                rotation = DegreeToRadian( 90 );

            DxfDimension.Linear dxfDimension;
            DxfEntity existing = ReadDxfEntityUsingHandle( dimension.DxfHandles );
            if (existing == null)
            {
                dxfDimension = new DxfDimension.Linear(model.CurrentDimensionStyle)
                {
                    DimensionStyle = dstyle
                };
                viewer.SetDimStyleOverrides( dxfDimension, dimension,iImage );
                viewer.modelwh.Model.Entities.Add( dxfDimension );
                dimension.DxfHandles = new ulong[ 1 ];
                dimension.DxfHandles[ 0 ] = dxfDimension.Handle;
            }
            else
            {
                dxfDimension = (DxfDimension.Linear)existing;
                dxfDimension.DimensionStyle = dstyle;
                viewer.SetDimStyleOverrides( dxfDimension, dimension,iImage );
            }

            if (!string.IsNullOrWhiteSpace( dimension.OverideText ))
            {
                dxfDimension.Text = dimension.OverideText;
            }
            dxfDimension.ZAxis = ConvertDirection3D(dimension.ZDirection);
            dxfDimension.Rotation = rotation;
            dxfDimension.ExtensionLine1StartPoint = ConvertToPoint3D( dimension.DefPt13 );
            dxfDimension.ExtensionLine2StartPoint = ConvertToPoint3D( dimension.DefPt14 );
            dxfDimension.DimensionLineLocation = ConvertToPoint3D( dimension.DefPt10 );
            //dxfDimension.TextRotation = DegreeToRadian(dimension.TextRotation);
            MatchEntitySettings( dxfDimension, dimension );
            UpdateDimensionBlock(dxfDimension, dimension, iImage);

            //var tpt = dxfDimension.TextMiddlePointWcs;
            //if (!tpt.IsEqual( ConvertToPoint3D( dimension.DefPt11 ), 0.01 ))
            //{
            //    dxfDimension.TextMiddlePoint = ConvertToPoint3D( dimension.DefPt11 );
            //    dxfDimension.UseTextMiddlePoint = true;
            //}
            //dxfDimension.GenerateBlock();

            return new List<DxfEntity>() { dxfDimension };
        }

        private List<DxfEntity> AddAlignedDimension( dxeDimension dimension, dxfImage iImage)
        {
            if (dimension == null)
            {
                return null;
            }

            var dstyle = viewer.GetDimensionStyle( dimension.DimStyleName );
            double rotation = DegreeToRadian( dimension.Angle );

            DxfDimension.Aligned dxfDimension;
            DxfEntity existing = ReadDxfEntityUsingHandle( dimension.DxfHandles );
            if (existing == null)
            {
                dxfDimension = new DxfDimension.Aligned(model.CurrentDimensionStyle)
                {
                    DimensionStyle = dstyle
                };
                viewer.SetDimStyleOverrides( dxfDimension, dimension , iImage);
                viewer.modelwh.Model.Entities.Add( dxfDimension );
                dimension.DxfHandles = new ulong[ 1 ];
                dimension.DxfHandles[ 0 ] = dxfDimension.Handle;
            }
            else
            {
                dxfDimension = (DxfDimension.Linear)existing;
                dxfDimension.DimensionStyle = dstyle;
                viewer.SetDimStyleOverrides( dxfDimension, dimension, iImage );
            }

            if (!string.IsNullOrWhiteSpace( dimension.OverideText ))
            {
                dxfDimension.Text = dimension.OverideText;
            }
            dxfDimension.ExtensionLine1StartPoint = ConvertToPoint3D( dimension.DefPt13 );
            dxfDimension.ExtensionLine2StartPoint = ConvertToPoint3D( dimension.DefPt14 );
            dxfDimension.DimensionLineLocation = ConvertToPoint3D( dimension.DefPt10 );
            //dxfDimension.TextRotation = DegreeToRadian(dimension.TextRotation);
            MatchEntitySettings( dxfDimension, dimension );
            UpdateDimensionBlock(dxfDimension, dimension, iImage);

            //var tpt = dxfDimension.TextMiddlePointWcs;
            //if (!tpt.IsEqual( ConvertToPoint3D( dimension.DefPt11 ), 0.01 ))
            //{
            //    dxfDimension.TextMiddlePoint = ConvertToPoint3D( dimension.DefPt11 );
            //    dxfDimension.UseTextMiddlePoint = true;
            //}
            //dxfDimension.GenerateBlock();

            return new List<DxfEntity>() { dxfDimension };
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToHorizontalLinearDimension(dxfEntity entity, dxfImage iImage)
        {
            dxeDimension dimension = entity as dxeDimension;
            var addedEntities = AddLinearDimension( dimension, iImage );

            return (addedEntities, null);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToVerticalLinearDimension(dxfEntity entity, dxfImage iImage)
        {
            dxeDimension dimension = entity as dxeDimension;
           

            var addedEntities = AddLinearDimension( dimension, iImage);

            return (addedEntities, null);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToLinearAlignedDimension(dxfEntity entity, dxfImage iImage)
        {
            dxeDimension dimension = entity as dxeDimension;
            var addedEntities = AddAlignedDimension( dimension, iImage);

            return (addedEntities, null);
        }

        private List<DxfEntity> AddOrdinateDimension( dxeDimension dimension, bool horizontal, dxfImage iImage)
        {
            if (dimension == null)
            {
                return null;
            }

            var dstyle = viewer.GetDimensionStyle( dimension.DimStyleName );

            DxfDimension.Ordinate dxfDimension;
            DxfEntity existing = ReadDxfEntityUsingHandle( dimension.DxfHandles );
            if (existing == null)
            {
                dxfDimension = new DxfDimension.Ordinate(model.CurrentDimensionStyle)
                {
                    DimensionStyle = dstyle
                };
                viewer.SetDimStyleOverrides( dxfDimension, dimension , iImage);
                viewer.modelwh.Model.Entities.Add( dxfDimension );
                dimension.DxfHandles = new ulong[ 1 ];
                dimension.DxfHandles[ 0 ] = dxfDimension.Handle;
            }
            else
            {
                dxfDimension = (DxfDimension.Ordinate)existing;
                dxfDimension.DimensionStyle = dstyle;
                viewer.SetDimStyleOverrides( dxfDimension, dimension , iImage);
            }

           
            if (!string.IsNullOrWhiteSpace( dimension.OverideText ))
            {
                dxfDimension.Text = dimension.OverideText;
            }
            dxfDimension.ZAxis = ConvertDirection3D(dimension.ZDirection);
            dxfDimension.ShowX = horizontal;
            dxfDimension.FeaturePosition = ConvertToPoint3D( dimension.DefPt13 );
            dxfDimension.LeaderEndPoint = ConvertToPoint3D( dimension.DefPt14 );
            dxfDimension.UcsOrigin = ConvertToPoint3D( dimension.DefPt10 );
            if(dimension.TextPrimary != null)
                dxfDimension.TextMiddlePointWcs = ConvertToPoint3D(dimension.TextPrimary.BoundingRectangle().Center);
            dxfDimension.InsertionPointWcs = ConvertToPoint3D(dimension.DefPt10);
            dxfDimension.TextRotation = DegreeToRadian(dimension.TextRotation);
            MatchEntitySettings( dxfDimension, dimension );
            dxfDimension.GenerateBlock();

            var tpt = dxfDimension.TextMiddlePointWcs;
            if (!tpt.IsEqual( ConvertToPoint3D( dimension.DefPt11 ), 0.01 ))
            {
                dxfDimension.TextMiddlePoint = ConvertToPoint3D( dimension.DefPt11 );
                dxfDimension.UseTextMiddlePoint = true;
            }
            dxfDimension.UseTextMiddlePoint = false;
            UpdateDimensionBlock(dxfDimension, dimension, iImage);
            WW.Cad.Model.Tables.DxfBlock blk = dxfDimension.Block;
            
            return new List<DxfEntity>() { dxfDimension };
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToHorizontalOrdinateDimension(dxfEntity entity, dxfImage iImage)
        {
            dxeDimension dimension = entity as dxeDimension;
            var addedEntities = AddOrdinateDimension( dimension, true , iImage);

            return (addedEntities, null);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToVerticalOrdinateDimension(dxfEntity entity, dxfImage iImage)
        {
            dxeDimension dimension = entity as dxeDimension;
            var addedEntities = AddOrdinateDimension( dimension, false, iImage);

            return (addedEntities, null);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToRadiusRadialDimension(dxfEntity entity, dxfImage iImage)
        {
            dxeDimension dimension = entity as dxeDimension;
            if (dimension == null)
            {
                return (null, null);
            }

            var dstyle = viewer.GetDimensionStyle( dimension.DimStyleName );
            double rotation = DegreeToRadian( dimension.Angle );

            DxfDimension.Radial dxfDimension;
            DxfEntity existing = ReadDxfEntityUsingHandle( dimension.DxfHandles );
            if (existing == null)
            {
                dxfDimension = new DxfDimension.Radial(model.CurrentDimensionStyle)
                {
                    DimensionStyle = dstyle
                };
                viewer.SetDimStyleOverrides( dxfDimension, dimension,iImage );
                viewer.modelwh.Model.Entities.Add( dxfDimension );
                dimension.DxfHandles = new ulong[ 1 ];
                dimension.DxfHandles[ 0 ] = dxfDimension.Handle;
            }
            else
            {
                dxfDimension = (DxfDimension.Radial)existing;
                dxfDimension.DimensionStyle = dstyle;
                viewer.SetDimStyleOverrides( dxfDimension, dimension,iImage );
            }

            if (!string.IsNullOrWhiteSpace( dimension.OverideText ))
            {
                dxfDimension.Text = dimension.OverideText;
            }
            dxfDimension.ZAxis = ConvertDirection3D(dimension.ZDirection);
            dxfDimension.ArcLineIntersectionPoint = ConvertToPoint3D( dimension.DefPt15 );
            dxfDimension.Center = ConvertToPoint3D( dimension.DefPt10 );
            dxfDimension.TextRotation = DegreeToRadian(dimension.TextRotation);
            MatchEntitySettings( dxfDimension, dimension );
            dxfDimension.GenerateBlock();

            var tpt = dxfDimension.TextMiddlePointWcs;
            if (!tpt.IsEqual( ConvertToPoint3D( dimension.DefPt11 ), 0.01 ))
            {
                dxfDimension.TextMiddlePoint = ConvertToPoint3D( dimension.DefPt11 );
                dxfDimension.UseTextMiddlePoint = true;
            }
            UpdateDimensionBlock(dxfDimension, dimension, iImage);

            return (new List<DxfEntity>() { dxfDimension }, null);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToDiametericRadialDimension(dxfEntity entity, dxfImage iImage)
        {
            dxeDimension dimension = entity as dxeDimension;
            if (dimension == null)
            {
                return (null, null);
            }

            var dstyle = viewer.GetDimensionStyle( dimension.DimStyleName );
            double rotation = DegreeToRadian( dimension.Angle );

            DxfDimension.Diametric dxfDimension;
            DxfEntity existing = ReadDxfEntityUsingHandle( dimension.DxfHandles );
            if (existing == null)
            {
                dxfDimension = new DxfDimension.Diametric(model.CurrentDimensionStyle)
                {
                    DimensionStyle = dstyle
                };
                viewer.SetDimStyleOverrides( dxfDimension, dimension, iImage );
                viewer.modelwh.Model.Entities.Add( dxfDimension );
                dimension.DxfHandles = new ulong[ 1 ];
                dimension.DxfHandles[ 0 ] = dxfDimension.Handle;
            }
            else
            {
                dxfDimension = (DxfDimension.Diametric)existing;
                dxfDimension.DimensionStyle = dstyle;
                viewer.SetDimStyleOverrides( dxfDimension, dimension,iImage );
            }

            if (!string.IsNullOrWhiteSpace( dimension.OverideText ))
            {
                dxfDimension.Text = dimension.OverideText;
            }
            dxfDimension.ZAxis = ConvertDirection3D(dimension.ZDirection);
            dxfDimension.ArcLineIntersectionPoint1 = ConvertToPoint3D( dimension.DefPt15 );
            dxfDimension.ArcLineIntersectionPoint2 = ConvertToPoint3D( dimension.DefPt10 );
            dxfDimension.TextRotation = DegreeToRadian(dimension.TextRotation);
            dxfDimension.ZAxis = ConvertDirection3D(dimension.ZDirection);
            MatchEntitySettings( dxfDimension, dimension );
            dxfDimension.GenerateBlock();

            var tpt = dxfDimension.TextMiddlePointWcs;
            if (!tpt.IsEqual( ConvertToPoint3D( dimension.DefPt11 ), 0.01 ))
            {
                dxfDimension.TextMiddlePoint = ConvertToPoint3D( dimension.DefPt11 );
                dxfDimension.UseTextMiddlePoint = true;
            }


            UpdateDimensionBlock(dxfDimension, dimension, iImage);


            return (new List<DxfEntity>() { dxfDimension }, null);
        }

        private void UpdateDimensionBlock( DxfDimension mDimension,  dxeDimension dimension, dxfImage iImage)
        {
            mDimension.GenerateBlock();

            double ttxtang = 360-dimension.TextRotation;
            mDimension.TextRotation = ttxtang * Math.PI /180 ;
            DxfBlock m_block = mDimension.Block;
            colDXFEntities i_ents = dimension.Entities;
            DxfEntityCollection newents = new DxfEntityCollection();
            m_block.Entities.Clear();
            newents = m_block.Entities;
            dxfVector v1 = null;
            dxxDimensionTypes fam = dimension.DimensionFamily;
            if (fam == dxxDimensionTypes.Ordinate)
            {
                v1 = dimension.DimensionPt1 * -1;
            }

            
           // v1 = null;
            dxfPlane dimplane = dimension.Plane;
            foreach (var ent in   i_ents)
            {
                if (v1 != null)
                    ent.Translate(v1);
                if(ent.IsText)
                {
                    dxeText txt = (dxeText)ent;
                    double tang = txt.DimensionTextAngle + ttxtang;
                    if (tang != 0) 
                        txt.Rotation += tang;

                    dxfVector v2 = txt.DefinitionPoint(dxxEntDefPointTypes.Center);
                    mDimension.TextMiddlePointWcs = new Point3D(v2.X, v2.Y, v2.Z);
                    mDimension.UseTextMiddlePoint = true;
                    //Console.WriteLine(txt.ToString());
                }
                TransformToDXF(ent, newents, iImage);

            }

            //dxfVector v3 = new dxfVector(dimension.HandlePt);
            //dxfVector v2 = v3 + dimplane.XDirection * 2;
            //dxeArc circ = new dxeArc(v3, 0.1);
            //circ.Translate(v1);
            //TransformToDXF(circ, newents, iImage);
            //circ = new dxeArc(v2, 0.2);
            //circ.Translate(v1);
            //TransformToDXF(circ, newents, iImage);
            
          //  m_block.Entities.AddRange(newents);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToAngular3PDimension(dxfEntity entity, dxfImage iImage)
        {
            dxeDimension dimension = entity as dxeDimension;
            if (dimension == null)
            {
                return (null, null);
            }

            var dstyle = viewer.GetDimensionStyle( dimension.DimStyleName );
            double rotation = DegreeToRadian( dimension.Angle );

            DxfDimension.Angular3Point dxfDimension;
            DxfEntity existing = ReadDxfEntityUsingHandle( dimension.DxfHandles );
            if (existing == null)
            {
                dxfDimension = new DxfDimension.Angular3Point(model.CurrentDimensionStyle)
                {
                    DimensionStyle = dstyle
                };
                viewer.SetDimStyleOverrides( dxfDimension, dimension,  iImage);
                viewer.modelwh.Model.Entities.Add( dxfDimension );
                dimension.DxfHandles = new ulong[ 1 ];
                dimension.DxfHandles[ 0 ] = dxfDimension.Handle;
            }
            else
            {
                dxfDimension = (DxfDimension.Angular3Point)existing;
                dxfDimension.DimensionStyle = dstyle;
                viewer.SetDimStyleOverrides( dxfDimension, dimension, iImage);
            }

            if (!string.IsNullOrWhiteSpace( dimension.OverideText ))
            {
                dxfDimension.Text = dimension.OverideText;
            }
            dxfDimension.ZAxis = ConvertDirection3D(dimension.ZDirection);
            dxfDimension.DimensionLineArcPoint = dimension.DefPt10.ConvertToPoint3D();
            dxfDimension.AngleVertex = ConvertToPoint3D( dimension.DefPt15 );
            dxfDimension.ExtensionLine1StartPoint = ConvertToPoint3D( dimension.DefPt13 );
            dxfDimension.ExtensionLine2StartPoint = ConvertToPoint3D( dimension.DefPt14 );
            dxfDimension.TextRotation = DegreeToRadian(dimension.TextRotation);
            MatchEntitySettings( dxfDimension, dimension );
            dxfDimension.GenerateBlock();

            var tpt = dxfDimension.TextMiddlePointWcs;
            if (!tpt.IsEqual( ConvertToPoint3D( dimension.DefPt11 ), 0.01 ))
            {
                dxfDimension.TextMiddlePoint = ConvertToPoint3D( dimension.DefPt11 );
                dxfDimension.UseTextMiddlePoint = true;
            }
            dxfDimension.GenerateBlock();

            return (new List<DxfEntity>() { dxfDimension }, null);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToAngular4PDimension(dxfEntity entity, dxfImage iImage)
        {
            dxeDimension dimension = entity as dxeDimension;
            if (dimension == null)
            {
                return (null, null);
            }

            var dstyle = viewer.GetDimensionStyle( dimension.DimStyleName );
            double rotation = DegreeToRadian( dimension.Angle );

            DxfDimension.Angular4Point dxfDimension;
            DxfEntity existing = ReadDxfEntityUsingHandle( dimension.DxfHandles );
            if (existing == null)
            {
                dxfDimension = new DxfDimension.Angular4Point(model.CurrentDimensionStyle)
                {
                    DimensionStyle = dstyle
                };
                viewer.SetDimStyleOverrides( dxfDimension, dimension, iImage);
                viewer.modelwh.Model.Entities.Add( dxfDimension );
                dimension.DxfHandles = new ulong[ 1 ];
                dimension.DxfHandles[ 0 ] = dxfDimension.Handle;
            }
            else
            {
                dxfDimension = (DxfDimension.Angular4Point)existing;
                dxfDimension.DimensionStyle = dstyle;
                viewer.SetDimStyleOverrides( dxfDimension, dimension, iImage);
            }

            if (!string.IsNullOrWhiteSpace( dimension.OverideText ))
            {
                dxfDimension.Text = dimension.OverideText;
            }
            dxfDimension.ZAxis = ConvertDirection3D(dimension.ZDirection);
            dxfDimension.ExtensionLine1StartPoint = ConvertToPoint3D( dimension.DefPt13 );
            dxfDimension.ExtensionLine1EndPoint = ConvertToPoint3D( dimension.DefPt14 );
            dxfDimension.ExtensionLine2StartPoint = ConvertToPoint3D( dimension.DefPt10 );
            dxfDimension.ExtensionLine2EndPoint = ConvertToPoint3D( dimension.DefPt15 );
            dxfDimension.DimensionLineArcPoint = ConvertToPoint3D( dimension.DefPt16 );
            dxfDimension.TextRotation = DegreeToRadian(dimension.TextRotation);
            MatchEntitySettings( dxfDimension, dimension );
            dxfDimension.GenerateBlock();

            var tpt = dxfDimension.TextMiddlePointWcs;
            if (!tpt.IsEqual( ConvertToPoint3D( dimension.DefPt11 ), 0.01 ))
            {
                dxfDimension.TextMiddlePoint = ConvertToPoint3D( dimension.DefPt11 );
                dxfDimension.UseTextMiddlePoint = true;
            }
            dxfDimension.GenerateBlock();

            return (new List<DxfEntity>() { dxfDimension }, null);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToText( dxfEntity entity )
        {
            dxeText text = entity as dxeText;
            if (text == null || IsDimension( entity ))
            {
                return (null, null);
            }

            return TransformToTexts( entity, text.Instances, new List<dxfVector>() { text.AlignmentPt1, text.AlignmentPt2 }, text.TextHeight, viewer.GetTextStyle( text ), text.TextString, "",
                (TextHorizontalAlignment)text.HorizontalAlignment, (TextVerticalAlignment)text.VerticalAlignment, text.WidthFactor, AttachmentPoint.BottomLeft,
                ( added, updated, ent, points, textHeight, style, textString, attributeTag, hAlign, vAlign, widthFactor, vector, attachment, handles ) =>
                {
                    DxfText dxfText;
                    DxfEntity existing = ReadDxfEntityUsingHandle( entity.DxfHandles );
                    if (existing == null)
                    {
                        dxfText = new DxfText();
                        added.Add( dxfText );
                    }
                    else
                    {
                        dxfText = (DxfText)existing;
                        updated.Add( dxfText );
                    }

                    dxeText iText = (dxeText)entity;

                    dxfText.ZAxis = ConvertDirection3D(text.ZDirection);
                    dxfText.Text = textString.Replace( "\\\\", "\\" ); //Correct for intent of showing a single \
                    dxfText.Height = textHeight;
                    if (iText.Alignment != dxxMTextAlignments.Fit && iText.Alignment != dxxMTextAlignments.Aligned)
                    {
                        dxfText.AlignmentPoint1 = ConvertToPoint3D(points[0]);
                        dxfText.AlignmentPoint2 = ConvertToPoint3D(points[0]);

                    }
                    else
                    {
                        dxfText.AlignmentPoint1 = ConvertToPoint3D(points[1]);
                        dxfText.AlignmentPoint2 = ConvertToPoint3D(points[0]);
                    }
                 
                    dxfText.HorizontalAlignment = hAlign;
                    dxfText.VerticalAlignment = vAlign;
                    dxfText.Style = style;
                    dxfText.WidthFactor = widthFactor;
                    dxfText.Rotation = text.Rotation * Math.PI/180;
                   
                    MatchEntitySettings( dxfText, ent, vector );
                } );
        }

        private Vector3D ConvertDirection3D(dxfDirection direction)
        {
            if (direction == null) return new Vector3D();
            //dxfDirections are always unit vectors (magnitude = 1)
            return new Vector3D(direction.X, direction.Y, direction.Z);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToAttDef( dxfEntity entity )
        {
            dxeText text = entity as dxeText;
            if (text == null)
            {
                return (null, null);
            }

            return TransformToTexts( entity, text.Instances, new List<dxfVector>() { text.AlignmentPt1, text.AlignmentPt2 }, text.TextHeight, viewer.GetTextStyle( text ), text.TextString, text.AttributeTag,
                (TextHorizontalAlignment)text.HorizontalAlignment, (TextVerticalAlignment)text.VerticalAlignment, text.WidthFactor, AttachmentPoint.BottomLeft,
                ( added, updated, ent, points, textHeight, style, textString, attributeTag, hAlign, vAlign, widthFactor, vector, attachment, handles ) =>
                {
                    DxfAttributeDefinition dxfAttDef;
                    DxfEntity existing = ReadDxfEntityUsingHandle( entity.DxfHandles );
                    if (existing == null)
                    {
                        dxfAttDef = new DxfAttributeDefinition();
                        added.Add( dxfAttDef );
                    }
                    else
                    {
                        dxfAttDef = (DxfAttributeDefinition)existing;
                        updated.Add( dxfAttDef );
                    }

                    dxfAttDef.Text = textString.Replace( "\\\\", "\\" ); //Correct for intent of showing a single \
                    dxfAttDef.Height = textHeight;
                    dxfAttDef.AlignmentPoint1 = ConvertToPoint3D( points[ 0 ] );
                    dxfAttDef.AlignmentPoint2 = ConvertToPoint3D( points[ 1 ] );
                    dxfAttDef.HorizontalAlignment = hAlign;
                    dxfAttDef.VerticalAlignment = vAlign;
                    dxfAttDef.Style = style;
                    dxfAttDef.WidthFactor = widthFactor;
                    dxfAttDef.TagString = attributeTag;
                    dxfAttDef.Rotation = text.Rotation;
                    dxfAttDef.PromptString = ((dxeText)entity).Prompt;
                    dxfAttDef.ZAxis = ConvertDirection3D(text.ZDirection);
                    MatchEntitySettings( dxfAttDef, ent, vector );
                } );
        }
        private (List<DxfEntity>, List<DxfEntity>) TransformToAttrib(dxfEntity entity)
        {
            dxeText text = entity as dxeText;
            if (text == null)
            {
                return (null, null);
            }

            return TransformToTexts(entity, text.Instances, new List<dxfVector>() { text.AlignmentPt1, text.AlignmentPt2 }, text.TextHeight, viewer.GetTextStyle(text), text.TextString, text.AttributeTag,
                (TextHorizontalAlignment)text.HorizontalAlignment, (TextVerticalAlignment)text.VerticalAlignment, text.WidthFactor, AttachmentPoint.BottomLeft,
                (added, updated, ent, points, textHeight, style, textString, attributeTag, hAlign, vAlign, widthFactor, vector, attachment, handles) =>
                {
                    DxfAttribute dxfAttrib;
                    DxfEntity existing = ReadDxfEntityUsingHandle(entity.DxfHandles);
                    if (existing == null)
                    {
                        dxfAttrib = new DxfAttribute();
                        added.Add(dxfAttrib);
                    }
                    else
                    {
                        dxfAttrib = (DxfAttribute)existing;
                        updated.Add(dxfAttrib);
                    }

                    dxfAttrib.Text = textString.Replace("\\\\", "\\"); //Correct for intent of showing a single \
                    dxfAttrib.Height = textHeight;
                    dxfAttrib.AlignmentPoint1 = ConvertToPoint3D(points[0]);
                    dxfAttrib.AlignmentPoint2 = ConvertToPoint3D(points[1]);
                    dxfAttrib.HorizontalAlignment = hAlign;
                    dxfAttrib.VerticalAlignment = vAlign;
                    dxfAttrib.Style = style;
                    dxfAttrib.WidthFactor = widthFactor;
                    dxfAttrib.TagString = attributeTag;
                    dxfAttrib.Rotation = text.Rotation;
                    //dxfAttrib.PromptString = ((dxeText)entity).Prompt;
                    dxfAttrib.ZAxis = ConvertDirection3D(text.ZDirection);
                    MatchEntitySettings(dxfAttrib, ent, vector);
                });
        }
        private (List<DxfEntity>, List<DxfEntity>) TransformToAttrib(dxfAttribute attribute)
        {
            dxeText text = new dxeText(attribute);
            if (text == null)
            {
                return (null, null);
            }

            return TransformToTexts(text, text.Instances, new List<dxfVector>() { text.AlignmentPt1, text.AlignmentPt2 }, text.TextHeight, viewer.GetTextStyle(text), text.TextString, text.AttributeTag,
                (TextHorizontalAlignment)text.HorizontalAlignment, (TextVerticalAlignment)text.VerticalAlignment, text.WidthFactor, AttachmentPoint.BottomLeft,
                (added, updated, ent, points, textHeight, style, textString, attributeTag, hAlign, vAlign, widthFactor, vector, attachment, handles) =>
                {
                    DxfAttribute dxfAttrib;
                    DxfEntity existing = ReadDxfEntityUsingHandle(text.DxfHandles);
                    if (existing == null)
                    {
                        dxfAttrib = new DxfAttribute();
                        added.Add(dxfAttrib);
                    }
                    else
                    {
                        dxfAttrib = (DxfAttribute)existing;
                        updated.Add(dxfAttrib);
                    }

                    dxfAttrib.Text = textString.Replace("\\\\", "\\"); //Correct for intent of showing a single \
                    dxfAttrib.Height = textHeight;
                    dxfAttrib.AlignmentPoint1 = ConvertToPoint3D(points[0]);
                    dxfAttrib.AlignmentPoint2 = ConvertToPoint3D(points[1]);
                    dxfAttrib.HorizontalAlignment = hAlign;
                    dxfAttrib.VerticalAlignment = vAlign;
                    dxfAttrib.Style = style;
                    dxfAttrib.WidthFactor = widthFactor;
                    dxfAttrib.TagString = attributeTag;
                    dxfAttrib.Rotation = text.Rotation;
                    //dxfAttrib.PromptString = ((dxeText)entity).Prompt;
                    dxfAttrib.ZAxis = ConvertDirection3D(text.ZDirection);
                    MatchEntitySettings(dxfAttrib, ent, vector);
                });
        }
        private (List<DxfEntity>, List<DxfEntity>) TransformToMText(dxfEntity entity)
        {
            dxeText mtext = entity as dxeText;
            if (mtext == null || IsDimension(entity))
            {
                return (null, null);
            }

            return TransformToTexts( entity, mtext.Instances, new List<dxfVector>() { mtext.InsertionPt }, mtext.TextHeight, viewer.GetTextStyle( mtext ), mtext.TextString, "",
                (TextHorizontalAlignment)mtext.HorizontalAlignment, (TextVerticalAlignment)mtext.VerticalAlignment, mtext.WidthFactor, (AttachmentPoint)(short)mtext.DXFAlignment,
                ( added, updated, ent, points, textHeight, style, textString, attributeTag, hAlign, vAlign, widthFactor, vector, attachment, handles ) =>
            {
                DxfMText dxfMText;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfMText = new DxfMText();
                    added.Add( dxfMText );
                }
                else
                {
                    dxfMText = (DxfMText)existing;
                    updated.Add( dxfMText );
                }

                if ((short)attachment > 0 && (short)attachment > 9 && (short)attachment < 13)
                    attachment = (AttachmentPoint)(short)attachment - 3;
                else if ((short)attachment < 0 || (short)attachment > 12)
                    attachment = (AttachmentPoint)0;

                if (widthFactor != 1 && !textString.Contains( @"\W" ))
                    textString = $"\\W{widthFactor};" + textString;

                dxfMText.Text = textString;
                dxfMText.Height = textHeight;
                dxfMText.InsertionPoint = ConvertToPoint3D( points[0] );
                dxfMText.AttachmentPoint = attachment;
                dxfMText.Style = style;
                dxfMText.ZAxis = ConvertDirection3D(mtext.ZDirection);
                dxfMText.XAxis = ConvertDirection3D(mtext.XDirection);
                dxfMText.LineSpacingStyle = mtext.LineSpacingStyle == dxxLineSpacingStyles.Exact ? LineSpacingStyle.Exact : LineSpacingStyle.AtLeast;
                dxfMText.LineSpacingFactor = mtext.LineSpacingFactor;
                //dxfRectangle refrec = mtext.BoundingRectangle();
                //dxfMText.ReferenceRectangleWidth = refrec.Width * 2;
                //dxfMText.ReferenceRectangleHeight = refrec.Height * 2;


                MatchEntitySettings( dxfMText, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToInsert( dxePolyline polyline, dxfImage iImage )
        {
            if (polyline == null) return (null, null);
            DxfBlock block = TransformToBlock( polyline, polyline.GUID, iImage );

            return TransformToInserts( polyline, block, null, polyline.Instances, polyline.Center(), polyline.HandlePt, 0, new Vector3D( 1, 1, 1 ),
                ( added, updated, ent, blockdef, dinsert, point, vector, rotation, scale, handles ) =>
                {
                    DxfInsert dxfInsert;
                    DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                    if (existing == null)
                    {
                        dxfInsert = new DxfInsert( blockdef, ConvertToPoint3D( point ) );
                        added.Add( dxfInsert );
                    }
                    else
                    {
                        dxfInsert = existing as DxfInsert;
                        dxfInsert.Block = blockdef;
                        updated.Add( dxfInsert );
                    }
                    dxfInsert.ZAxis = ConvertDirection3D(polyline.ZDirection);
                    dxfInsert.Rotation = rotation;
                    dxfInsert.ScaleFactor = scale;

                    if (vector != null)
                    {
                        if (vector.Inverted)
                        {
                            InvertEntity( dxfInsert );
                        }
                        dxfInsert.Rotation = DegreeToRadian( vector.Rotation );
                    }

                    MatchEntitySettings( dxfInsert, ent, vector );
                } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToInsert(dxfEntity entity, dxfImage iImage)
        {
            dxeInsert insert = entity as dxeInsert;
            dxfBlock block = null;
            insert.GetBlock(ref iImage,ref block);
            DxfBlock blk = model.GetBlockWithName(insert.BlockName);
            if (insert == null || block == null || IsDimension(entity))
            {
                return (null, null);
            }

            return TransformToInserts( entity, block, insert, insert.Instances, insert.InsertionPt, block.HandlePt, DegreeToRadian( insert.RotationAngle ), new Vector3D( insert.XScaleFactor, insert.YScaleFactor, insert.ZScaleFactor ), iImage,
                ( added, updated, ent, blockdef, dinsert, point, vector, rotation, scale, handles ) =>
            {
                DxfInsert dxfInsert;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfInsert = new DxfInsert( blockdef, ConvertToPoint3D( point ) );
                    added.Add( dxfInsert );
                }
                else
                {
                    dxfInsert = existing as DxfInsert;
                    dxfInsert.Block = blockdef;
                    updated.Add( dxfInsert );
                }

                dxfInsert.Rotation = rotation;
                dxfInsert.ScaleFactor = scale;

                if (vector != null)
                {
                    if (vector.Inverted)
                    {
                        InvertEntity( dxfInsert );
                    }
                    dxfInsert.Rotation = DegreeToRadian( vector.Rotation );
                }

                List<DxfAttributeDefinition> atts = dxfInsert.Block.Entities.Where(e => e is DxfAttributeDefinition).OfType<DxfAttributeDefinition>().ToList();
                
                if(atts.Count > 0)
                {

                    List<dxeText> iattributes = dinsert.GetAttributes(iImage, block, bForFileOutput: true);
                    if (iattributes.Count > 0)
                    {
                        foreach (dxeText attrib in iattributes)
                        {
                           
                            (List<DxfEntity>, List<DxfEntity>) newats = TransformToAttrib(attrib);
                            foreach (var item in newats.Item1)
                            {
                                DxfAttribute newat = item as DxfAttribute;
                                string tag = newat.TagString;
                                if (atts.FindIndex((x) => string.Compare(x.TagString, tag, true) == 0) >= 0)
                                {
                                    dxfInsert.Attributes.Add(newat);
                                }
                            }
                        }

                    }

                }
                
               

                MatchEntitySettings( dxfInsert, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToArc(dxfEntity entity, dxfImage iImage)
        {
            dxeArc arc = entity as dxeArc;
            if (arc == null || entity.OwnerGUID.StartsWith("DIMENSION"))
            {
                return (null, null);
            }

            if (arc.HasWidth)
            {
                return TransformToPolyline(arc.ConvertToPolyline(), iImage);
            }

            return TransformToPrimitives( entity, arc.Instances, new List<dxfVector>() { arc.Center }, arc.Radius, new List<double>() { arc.ClockWise ? DegreeToRadian(arc.EndAngle) : DegreeToRadian( arc.StartAngle ), arc.ClockWise ? DegreeToRadian(arc.StartAngle) : DegreeToRadian( arc.EndAngle ) }, 
                ( added, updated, ent, points, radius, vector, angles, handles ) =>
            {
                DxfArc dxfArc;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfArc = new DxfArc();
                    added.Add( dxfArc );
                }
                else
                {
                    dxfArc = (DxfArc)existing;
                    updated.Add( dxfArc );
                }
                dxfArc.ZAxis = ConvertDirection3D(arc.ZDirection);
                Point3D ctr = ConvertToPoint3D(points[0]);
                dxfArc.Center = ctr;
                dxfArc.Radius = radius;
                dxfArc.StartAngle = angles[0];
                dxfArc.EndAngle = angles[1];
                if (vector != null && vector.Radius > 0)
                {
                    dxfArc.Radius = vector.Radius;
                }
                else
                {
                    dxfArc.Radius = radius;
                }
                ctr = dxfArc.Center;
                MatchEntitySettings( dxfArc, ent, vector );
            } );
        }

  
        private (List<DxfEntity>, List<DxfEntity>) TransformToPoint(dxfEntity entity)
        {
            dxePoint point = entity as dxePoint;
            if (point == null || IsDimension(entity))
            {
                return (null, null);
            }

            return TransformToPrimitives( entity, point.Instances, new List<dxfVector>() { point.Center }, 0.0, null,
            ( added, updated, ent, points, radius, vector, angles, handles ) =>
            {
                DxfPoint dxfPoint;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfPoint = new DxfPoint();
                    added.Add( dxfPoint );
                }
                else
                {
                    dxfPoint = (DxfPoint)existing;
                    updated.Add( dxfPoint );
                }

                dxfPoint.Position = ConvertToPoint3D( points[0] );

                MatchEntitySettings( dxfPoint, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToHoleBlock(dxfEntity entity, dxfImage iImage)
        {
            dxeHole hole = entity as dxeHole;
            DxfBlock dxfBlock = ConvertHoleToBlock( hole, iImage );
            if (hole == null || IsDimension(entity))
            {
                return (null, null);
            }

            return TransformToInserts( entity, dxfBlock, null, hole.Instances, hole.HandlePt, new dxfVector(), 0.0, new Vector3D( 1, 1, 1 ), ( added, updated, ent, block, insert, point, vector, rotation, scale, handles ) =>
            {
                DxfInsert dxfInsert;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfInsert = new DxfInsert( block, ConvertToPoint3D( point ) );
                    added.Add( dxfInsert );
                }
                else
                {
                    dxfInsert = existing as DxfInsert;
                    dxfInsert.Block = block;
                    updated.Add( dxfInsert );
                }

                dxfInsert.Rotation = rotation;
                dxfInsert.ScaleFactor = scale;
                dxfInsert.ZAxis =ConvertDirection3D( hole.ZDirection);
                if (vector != null)
                {
                    if (vector.Inverted)
                    {
                        InvertEntity( dxfInsert );
                    }
                    dxfInsert.Rotation = DegreeToRadian( vector.Rotation );
                }

                MatchEntitySettings( dxfInsert, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToSlotBlock( dxfEntity entity, dxfImage iImage )
        {
            dxeHole slot = entity as dxeHole;
            DxfBlock dxfBlock = ConvertHoleToBlock( slot, iImage );
            if (slot == null)
            {
                return (null, null);
            }

            return TransformToInserts( entity, dxfBlock, null, slot.Instances, slot.HandlePt, new dxfVector(), 0.0, new Vector3D( 1, 1, 1 ), ( added, updated, ent, block, insert, point, vector, rotation, scale, handles ) =>
            {
                DxfInsert dxfInsert;
                DxfEntity existing = ReadDxfEntityUsingHandle( handles );
                if (existing == null)
                {
                    dxfInsert = new DxfInsert( block, ConvertToPoint3D( point ) );
                    added.Add( dxfInsert );
                }
                else
                {
                    dxfInsert = existing as DxfInsert;
                    dxfInsert.Block = block;
                    updated.Add( dxfInsert );
                }
                dxfInsert.ZAxis = ConvertDirection3D(slot.ZDirection);
                dxfInsert.Rotation = rotation;
                dxfInsert.ScaleFactor = scale;

                if (vector != null)
                {
                    if (vector.Inverted)
                    {
                        InvertEntity( dxfInsert );
                    }
                    dxfInsert.Rotation = DegreeToRadian( vector.Rotation );
                }

                MatchEntitySettings( dxfInsert, ent, vector );
            } );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToTable(dxfEntity entity, dxfImage iImage)
        {
            dxeTable table = entity as dxeTable;

            if (!table.SaveAsBlock) 
            {
                List<DxfEntity> rAdded = new List<DxfEntity>();
                List<DxfEntity> rUpdated = new List<DxfEntity>();

                colDXFEntities subents = table.SubEntities;
                dxfEntity ent;
                for (int i = 1; i <= subents.Count; i++)
                {
                    ent = subents.Item(i);
                    if (ent != null)
                    {
                        (List<DxfEntity> added, List<DxfEntity> updated, List<DxfBlock> addedNestedBlocks) = ConvertToCADLibObjects(ent, iImage);
                        rAdded.AddRange(added);
                        rUpdated.AddRange(updated);
                       
                    }
                }
                return (rAdded, rUpdated);

            }
            else
            {
                var tblock = table.ConvertToBlock();
                DxfBlock dxfBlock = TransformToBlock(tblock, iImage);

                if (dxfBlock == null)
                {
                    return (null, null);
                }

                return TransformToInserts(entity, dxfBlock, null, null, new dxfVector(), new dxfVector(), 0.0, new Vector3D(1, 1, 1), (added, updated, ent, block, insert, point, vector, rotation, scale, handles) =>
                {
                    DxfInsert dxfInsert;
                    DxfEntity existing = ReadDxfEntityUsingHandle(handles);
                    if (existing == null)
                    {
                        dxfInsert = new DxfInsert(block, ConvertToPoint3D(point));
                        added.Add(dxfInsert);
                    }
                    else
                    {
                        dxfInsert = existing as DxfInsert;
                        dxfInsert.Block = block;
                        updated.Add(dxfInsert);
                    }
                    dxfInsert.ZAxis = ConvertDirection3D(table.ZDirection);
                    dxfInsert.Rotation = rotation;
                    dxfInsert.ScaleFactor = scale;

                    if (vector != null)
                    {
                        if (vector.Inverted)
                        {
                            InvertEntity(dxfInsert);
                        }
                        dxfInsert.Rotation = DegreeToRadian(vector.Rotation);
                    }

                    MatchEntitySettings(dxfInsert, ent, vector);
                });
            }



 
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToLeader( dxfEntity entity )
        {
            dxeLeader leader = entity as dxeLeader;
            if (leader == null)
            {
                return (null, null);
            }

            var annotationEnts = new List<DxfEntity>();
            
            switch (leader.LeaderType)
            {

                case UOP.DXFGraphics.dxxLeaderTypes.LeaderText:
                    annotationEnts = TransformToDXF(leader.MText);
                    break;
                case UOP.DXFGraphics.dxxLeaderTypes.NoReactor:
                    break;
                case UOP.DXFGraphics.dxxLeaderTypes.LeaderBlock:
                    annotationEnts = TransformToDXF(leader.Insert);
                  
                    break;
            }
          
            var dstyle = viewer.GetDimensionStyle(leader.DimStyleName);
            foreach (var ent in annotationEnts)
            {
                if (leader.LeaderType == dxxLeaderTypes.LeaderText)
                {
                    DxfMText mtx = ent as DxfMText;
                    if (mtx == null) continue;

                    mtx.Color = EntityColor.CreateFrom(dstyle.TextColor);
                    mtx.Height = dstyle.TextHeight * leader.FeatureScaleFactor;
                    mtx.Style = dstyle.TextStyle;
                    if (!mtx.Text.Contains(@"\W") && leader.MText.WidthFactor != 1)
                        mtx.Text = $"\\W{leader.MText.WidthFactor};" + mtx.Text;
                }
               

            }


            DxfLeader dxfLeader;
            DxfEntity existing = ReadDxfEntityUsingHandle( leader.DxfHandles );
            if (existing == null)
            {
                dxfLeader = new DxfLeader(model)
                {
                    DimensionStyle = dstyle
                };
                if (annotationEnts.Count > 0)
                {
                    dxfLeader.AssociatedAnnotation = annotationEnts[ 0 ];
                    dxfLeader.CreateEditInteractor();
                    //dxfLeader.IsAnnotative = true;
                }
                viewer.SetDimStyleOverrides( dxfLeader, leader );
                viewer.modelwh.Model.Entities.Add( dxfLeader );
                leader.DxfHandles = new ulong[ 1 ];
                leader.DxfHandles[ 0 ] = dxfLeader.Handle;
            }
            else
            {
                dxfLeader = (DxfLeader)existing;
                dxfLeader.DimensionStyle = dstyle;
                if (annotationEnts.Count > 0)
                    dxfLeader.AssociatedAnnotation = annotationEnts[ 0 ];
                viewer.SetDimStyleOverrides( dxfLeader, leader );
            }
            dxfLeader.ZAxis = ConvertDirection3D(leader.ZDirection);
            dxfLeader.ArrowHeadEnabled = leader.HasArrowHead;
            var bpoints = new List<dxfVector>();
            for (int i = 1; i <= leader.Vertices.Count; i++)
            {
                var p =  leader.Vertices.Item( i );
                dxfLeader.Vertices.Add( new Point3D( p.X, p.Y, 0.0 ) );
            }

            var hookDir = HookLineDirection.Opposite;
            if (annotationEnts.Count > 0)
            {
                dxfLeader.CreationType = LeaderCreationType.CreatedWithTextAnnotation;

                double xoffset = dstyle.ExtensionLineOffset * leader.FeatureScaleFactor;
                double yoffset = 0.0;
                DxfMText mtx = annotationEnts[ 0 ] as DxfMText;
                var bc = new BoundsCalculator();
                bc.GetBounds( model, mtx );

                switch (mtx.AttachmentPoint)
                {
                    case AttachmentPoint.MiddleLeft:
                        xoffset *= -1;
                        yoffset = 0.5 * bc.Bounds.Height();
                        break;
                    case AttachmentPoint.TopLeft:
                        xoffset *= -1;
                        yoffset = 0.5 * mtx.Height;
                        break;
                    case AttachmentPoint.BottomLeft:
                        xoffset *= -1;
                        yoffset = -0.5 * mtx.Height;
                        break;
                    case AttachmentPoint.TopRight:
                        yoffset = 0.5 * mtx.Height;
                        hookDir = HookLineDirection.Same;
                        break;
                    case AttachmentPoint.MiddleRight:
                        yoffset = 0.5 * bc.Bounds.Height();
                        hookDir = HookLineDirection.Same;
                        break;
                    case AttachmentPoint.BottomRight:
                        yoffset = -0.5 * mtx.Height;
                        hookDir = HookLineDirection.Same;
                        break;

                }

                var vec = new Vector3D( xoffset, yoffset, 0 );
                mtx.InsertionPoint -= vec;

                dxfLeader.LastVertexOffsetFromAnnotation = vec;

                annotationEnts[ 0 ].AddPersistentReactor( dxfLeader );
            }
            else
                dxfLeader.CreationType = LeaderCreationType.CreatedWithoutAnnotation;

            dxfLeader.HookLineDirection = hookDir;
            //if (leader.HookLineDirection < 0)
            //    dxfLeader.HookLineDirection = HookLineDirection.Same;
            //else
            //    dxfLeader.HookLineDirection = HookLineDirection.Opposite;

            MatchEntitySettings( dxfLeader, leader );

            return (new List<DxfEntity>() { dxfLeader, dxfLeader.AssociatedAnnotation }, null);
        }

        private List<DxfVertex2D> GetVertices( List<dxfVector> points )
        {
            var ret = new List<DxfVertex2D>();
            foreach (var p in points)
            {
                ret.Add( new DxfVertex2D( p.X, p.Y ) { Bulge = p.Bulge, StartWidth = p.StartWidth, EndWidth = p.EndWidth } );
            }
            return ret;
        }

        private List<dxfVector> GetSolidVertices(dxeSolid solid)
        {
            List<dxfVector> vertices3D = new List<dxfVector>();
            colDXFVectors verts = solid.Vertices;
            int count = verts.Count;
            for (int i = 1; i <= count; i++)
            {
                vertices3D.Add(verts.Item( i ) );
            }
            if (vertices3D.Count() == 4)
            {
                //must invert last two points or we'll get a bowtie
                var lastv = vertices3D.Last();
                vertices3D[ 3 ] = vertices3D[ 2 ];
                vertices3D[ 2 ] = lastv;
            }
            else if (vertices3D.Count() == 3)
            {
                vertices3D.Add( vertices3D.Last() );
            }


            return vertices3D;
        }

        private DxfBlock ConvertHoleToBlock(dxeHole hole, dxfImage iImage)
        {
            List<DxfEntity> dxfEntities = new List<DxfEntity>();
            DxfBlock block;

            for (int i = 1; i < hole.Entities.Count; i++)
            {
                (List<DxfEntity> addedSubEntities, List<DxfEntity> updatedSubEntities, List<DxfBlock> addedSubBlocks) = ConvertToCADLibObjects(hole.Entities.Item(i), iImage);
                if (addedSubEntities != null)
                {
                    dxfEntities.AddRange(addedSubEntities);
                }
            }

            string blockName = hole.GUID;
            DxfBlock existing = ReadDxfBlockUsingName(blockName);
            if (existing == null)
            {
                block = new DxfBlock(blockName);
            }
            else
            {
                block = existing;
            }
            block.Entities.Clear();
            block.Entities.AddRange(dxfEntities);

            if (!model.Blocks.Contains( blockName ))
                model.Blocks.Add( block );

            return block;
        }

        private void SetLineTypeScaleForDXFEntity(DxfEntity dxfEntity, double lineTypeScale)
        {
            //dxfEntity.LineTypeScale = viewer.ValidateLTScale(lineTypeScale);
        }

        public double DegreeToRadian(double degree)
        {
            return Math.PI * degree / 180;
        }

        private Point3D ConvertToPoint3D(dxfVector vector)
        {
            return vector.ConvertToPoint3D();
        }

        protected virtual void Dispose( bool disposing )
        {
            if (!disposedValue && !skipDispose)
            {
                if (disposing)
                {
                    modelwh.ClearHandlers();
                    model.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                model = null;
                modelwh = null;
                viewer = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TransformUtil()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose( disposing: true );
            GC.SuppressFinalize( this );
        }
    }
}
