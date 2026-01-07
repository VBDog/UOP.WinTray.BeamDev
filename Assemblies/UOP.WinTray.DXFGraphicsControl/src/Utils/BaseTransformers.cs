using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D;
using UOP.DXFGraphics;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Tables;
using WW.Math;
using Vector3D = WW.Math.Vector3D;

namespace UOP.DXFGraphicsControl.Utils
{
    public partial class TransformUtil : IDisposable
    {
        public List<DxfEntity> TransformToDXF( dxfEntity entity )
        {
            return TransformToDXF( entity, model.Entities, null );
        }

        public List<DxfEntity> TransformToDXF( dxfEntity entity, dxfImage iImage, bool dontsaveit = false )
        {
            return TransformToDXF( entity, model.Entities, iImage,  dontsaveit );
        }

        public List<DxfEntity> TransformToDXF( dxfEntity entity, DxfEntityCollection Entities, dxfImage iImage, bool dontsaveit = false)
        {
            if (entity == null)
                return null;

            DxfGroup mgroup = null;
            ulong? ghandle = null;
            if (!string.IsNullOrEmpty(entity.GroupName) && !dontsaveit) 
            {
                ghandle = ConfirmGroupHandle(entity, iImage, out mgroup);
            }

            List<DxfEntity> addedDxfEntities = null;
            List<DxfEntity> updatedDxfEntities = null;
            List<DxfEntity> allDxfEntities = null;
            List<DxfBlock> addedDxfBlocks = null;
            List<DxfTableStyle> addedDxfTableStyles = null;

            (addedDxfEntities, updatedDxfEntities, addedDxfBlocks) = ConvertToCADLibObjects( entity, iImage );

            if (addedDxfEntities != null && addedDxfEntities.Count > 0)
            {
                if (entity.DxfHandles == null)
                    entity.DxfHandles = new ulong[ 0 ];
                var handles = entity.DxfHandles.ToList();
                if (entity.DxfHandles != null && entity.DxfHandles.Count() > 0)
                {
                    for (int i = 1; i < handles.Count(); i++)
                    {
                        var ent = modelwh.GetEntity( handles[i] );
                        if (ent != null)
                            model.Entities.Remove( ent );
                    }
                }

                if (Entities != null && !dontsaveit)
                {
                    var newEntities = addedDxfEntities.Where( e => e != null && !Entities.Contains( e ) ).ToList();
                    Entities.AddRange( newEntities );
                  
                    if(entity.DxfGroupHandle.HasValue)
                        AssociateToGroup(entity, newEntities);

                }
                //foreach (var ent in addedDxfEntities)
                //{
                //    if (ent is DxfDimension && viewer.ActiveDimOverrides != null)
                //    {
                //        DxfDimension dim = ent as DxfDimension;
                //        viewer.SetDimStyleOverrides( dim );
                //        dim.GenerateBlock();
                //    }
                //}

                entity.DxfHandles = new ulong[ addedDxfEntities.Count + ((handles.Count > 0) ? 1 : 0) ];
                if (handles.Count > 0)
                    entity.DxfHandles[ 0 ] = handles[ 0 ];

                for (int i = 0; i < addedDxfEntities.Count; i++)
                {
                    if (addedDxfEntities[ i ] == null) continue;
                    entity.DxfHandles[ i ] = addedDxfEntities[ i ].Handle;
                }
            }

            if (addedDxfBlocks != null && addedDxfBlocks.Count > 0)
            {
                try
                {
                    foreach (var block in addedDxfBlocks)
                    {
                        model.Blocks.Add( block );
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e.Message);
                    return null;
                }
            }

            if (addedDxfTableStyles != null && addedDxfTableStyles.Count > 0)
            {
                try
                {
                    foreach (var tableStyle in addedDxfTableStyles)
                    {
                        model.TableStyles.Add( tableStyle );
                    }
                }
                catch (Exception e)
            {
                    System.Diagnostics.Debug.Write(e.Message);
                    return null;
                }
            }

            if (addedDxfEntities != null || updatedDxfEntities != null)
            {
                allDxfEntities = new List<DxfEntity>();

                if (addedDxfEntities != null)
                {
                    allDxfEntities.AddRange( addedDxfEntities );
                }

                if (updatedDxfEntities != null)
                {
                    allDxfEntities.AddRange( updatedDxfEntities );
                }
            }

            

            return allDxfEntities;
        }

        private void AssociateToGroup(dxfEntity entity , List<DxfEntity> newEntities)
        {
            if (entity == null || newEntities == null) return;
            if (!entity.DxfGroupHandle.HasValue) return;
            DxfGroup egroup = model.Groups.Where((x) => x.Handle == entity.DxfGroupHandle.Value).FirstOrDefault();
            if (egroup == null) return;
            foreach (var ent in newEntities)
            {
                egroup.AddPersistentReactor(ent);
                ent.AddPersistentReactor(egroup);
            }


            
        }

        private DxfBlock TransformToBlock(dxePolyline polyline, string name, dxfImage iImage)
        {
            DxfBlock dxfBlock = null;
            DxfBlock existing = ReadDxfBlockUsingName( name );
            if (existing == null)
            {
                dxfBlock = new DxfBlock( name );
            }
            else
            {
                dxfBlock = (DxfBlock)existing;
            }
            var entities = polyline.Segments;
            if (entities != null && entities.Count > 0)
            {
                dxfEntity entity;
                for (int i = 1; i <= entities.Count; i++)
                {
                    entity = entities.Item( i );
                    var vtx = polyline.Vertices.Item( i );
                    if (vtx.Color != dxxColors.ByLayer && vtx.Color != dxxColors.ByBlock) entity.Color = vtx.Color;
                    if (vtx.Linetype != "ByLayer" && vtx.Linetype != "ByBlock")
                    {
                        entity.Linetype = vtx.Linetype;
                        entity.LTScale = vtx.LTScale;
                    }
                    if (!string.IsNullOrEmpty( vtx.LayerName )) entity.LayerName = vtx.LayerName;
                    (List<DxfEntity> added, List<DxfEntity> updated, List<DxfBlock> addedNestedBlocks) = ConvertToCADLibObjects( entity, iImage );
                    dxfBlock.Entities.AddRange( added );
                }
            }

            if (!model.Blocks.Contains( name ))
                model.Blocks.Add( dxfBlock );

            return dxfBlock;
        }

        private DxfBlock TransformToBlock( dxfBlock block, dxfImage iImage )
        {
            if (block == null)
            {
                return null;
            }

            DxfBlock dxfBlock = null;

            string name = block.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = block.GUID;
            }

           

            DxfBlock existing = ReadDxfBlockUsingName( name );
            bool exists = existing != null;

            if (block.Tag != null && block.Tag.Contains("\\") && !exists)
            {
                //load from file
                DxfModel sourceModel = CadReader.Read( block.Tag );
                if (sourceModel.Blocks.Contains(block.Name))
                {
                    CloneContext cloneContext = new CloneContext( sourceModel, model, ReferenceResolutionType.CloneMissing );
                    DxfBlock sourceBlock = sourceModel.Blocks[ block.Name ];
                    dxfBlock = new DxfBlock( name );
                    foreach (DxfEntity entity in sourceBlock.Entities)
                    {
                        DxfEntity clonedEntity = (DxfEntity)entity.Clone( cloneContext );
                        dxfBlock.Entities.Add( clonedEntity );
                    }
                    cloneContext.ResolveReferences();
                }
            }
            else
            {
                if (!exists)
                {
                    dxfBlock = new DxfBlock( name );
                }
                else
                {
                    dxfBlock = (DxfBlock)existing;
                }
            }

            colDXFEntities ients = block.Entities;

            if (ients != null && ients.Count > 0 && !exists)
            {
                if (name == "MDFUNCTIONAL_TABLE")
                {
                    Console.WriteLine(name);
                }
                for (int i = 1; i <= ients.Count; i++)
                {
                    dxfEntity entity = block.Entities.Item( i );
                    if(entity.IsText)
                    {
                        if(entity.TextType == dxxTextTypes.AttDef || entity.TextType == dxxTextTypes.Attribute)
                        {
                            Console.WriteLine(entity.EntityTypeName);
                        }
                    }
                    if(entity != null)
                    {
                        (List<DxfEntity> added, List<DxfEntity> updated, List<DxfBlock> addedNestedBlocks) = ConvertToCADLibObjects(entity, iImage);
                        if (added != null) dxfBlock.Entities.AddRange(added);

                    }
                }
            }

            if (!exists)
                model.Blocks.Add( dxfBlock );

            return dxfBlock;
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToPrimitives( dxfEntity entity, dxoInstances instances, List<dxfVector> basePoints, double baseLength, List<double> angles,
            Action<List<DxfEntity>, List<DxfEntity>, dxfEntity, List<dxfVector>, double, dxfVector, List<double>, ulong[]> Converter )
        {
            if (IsDimension( entity ))
            {
                return (null, null);
            }

            List<DxfEntity> added = new List<DxfEntity>();
            List<DxfEntity> updated = new List<DxfEntity>();

            //Add the first one
            Converter( added, updated, entity, basePoints, baseLength, null, angles, entity.DxfHandles );

            if (instances != null && instances.Count > 0)
            {
                dxfPlane plane = instances.BasePlane;
                if (entity.HasVertices || entity.GraphicType == dxxGraphicTypes.Solid)
                {
                   

                    //Add any additional instances
                    for (int i = 1; i <= instances.Count; i++)
                    {
                        colDXFVectors verts = instances.Apply(entity.Vertices, true,aIndex:i);

                        Converter(added, updated, entity, verts, baseLength, plane.Origin, angles, null);
                    }
                }
                else
                {
                    //Add any additional instances
                    for (int i = 1; i <= instances.Count; i++)
                    {
                        var iBasePoints = new List<dxfVector>();
                        foreach (var p in basePoints)
                        {
                            dxfVector v1 = instances.MemberPoint(i, p, bApplyRotations: entity.HasVertices);
                            iBasePoints.Add(v1);

                        }

                        Converter(added, updated, entity, iBasePoints, baseLength, plane.Origin, angles, null);
                    }
                }
                    
            }

            return (added, updated);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToInserts( dxfEntity entity, dxfBlock block, dxeInsert insert, dxoInstances instances, dxfVector basePoint, dxfVector insPoint, double rotation, Vector3D scale, dxfImage iImage,
            Action<List<DxfEntity>, List<DxfEntity>, dxfEntity, DxfBlock, dxeInsert, dxfVector, dxfVector, double, Vector3D, ulong[]> Converter )
        {
            DxfBlock dxfBlock = TransformToBlock( block, iImage );
            return TransformToInserts( entity, dxfBlock, insert, instances, basePoint, insPoint, rotation, scale, Converter );
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToInserts( dxfEntity entity, DxfBlock dxfBlock, dxeInsert insert, dxoInstances instances, dxfVector basePoint, dxfVector insPoint, double rotation, Vector3D scale,
            Action<List<DxfEntity>, List<DxfEntity>, dxfEntity, DxfBlock, dxeInsert, dxfVector, dxfVector, double, Vector3D, ulong[]> Converter )
        {
            if (dxfBlock == null || IsDimension( entity ))
            {
                return (null, null);
            }
            else
                dxfBlock.BasePoint = new Vector3D( insPoint.X, insPoint.Y, insPoint.Z );

            List<DxfEntity> added = new List<DxfEntity>();
            List<DxfEntity> updated = new List<DxfEntity>();

            //Add the first one
            Converter( added, updated, entity, dxfBlock, insert, basePoint, null, rotation, scale, entity.DxfHandles );

            if (instances != null && instances.Count > 0)
            {
                //Add any additional instances
                for (int i = 1; i <= instances.Count; i++)
                {
                    var insts = instances.Item(i);
                    var iBasePoint = instances.MemberPoint( i, basePoint );
                    Vector3D instscale = new Vector3D(scale.X * insts.ScaleFactor, scale.Y * insts.ScaleFactor, scale.Z * insts.ScaleFactor);
                    if (insts.Inverted) instscale.Y *= -1;
                    if (insts.LeftHanded) instscale.X *= -1;
                    Converter( added, updated, entity, dxfBlock, insert, iBasePoint, iBasePoint, rotation, instscale, null );
                }
            }

            return (added, updated);
        }

        private (List<DxfEntity>, List<DxfEntity>) TransformToTexts( dxfEntity entity, dxoInstances instances, List<dxfVector> basePoints, double baseLength, DxfTextStyle style, 
            string textString, string attributeTag, TextHorizontalAlignment hAlign, TextVerticalAlignment vAlign, double widthFactor, AttachmentPoint attachment,
            Action<List<DxfEntity>, List<DxfEntity>, dxfEntity, List<dxfVector>, double, DxfTextStyle, string, string, TextHorizontalAlignment, TextVerticalAlignment, double, dxfVector, AttachmentPoint, ulong[]> Converter )
        {
            if (IsDimension( entity ))
            {
                return (null, null);
            }

            List<DxfEntity> added = new List<DxfEntity>();
            List<DxfEntity> updated = new List<DxfEntity>();

            //Add the first one
            Converter( added, updated, entity, basePoints, baseLength, style, textString, attributeTag, hAlign, vAlign, widthFactor, null, attachment, entity.DxfHandles );

            if (instances != null && instances.Count > 0)
            {
                //Add any additional instances
                for (int i = 1; i <= instances.Count; i++)
                {
                    var iBasePoints = new List<dxfVector>();
                    foreach (var p in basePoints)
                    {
                        iBasePoints.Add( instances.MemberPoint( i, p ) );
                    }
                    Converter( added, updated, entity, iBasePoints, baseLength, style, textString, attributeTag, hAlign, vAlign, widthFactor, iBasePoints[0], attachment, null );
                }
            }

            return (added, updated);
        }


        public ulong? ConfirmGroupHandle(dxfEntity aEntity, dxfImage iImage, out DxfGroup rDxFGroup)
        {
            rDxFGroup = null;
            if (aEntity == null || iImage == null) return null;
            aEntity.DxfGroupHandle = null;
            string gname = aEntity.GroupName;
            if (string.IsNullOrEmpty(gname)) return null;
            dxfoGroup igroup = iImage.Groups.Find((x) => string.Compare(x.Name, gname, true) == 0);
            if (igroup == null) return null;

            rDxFGroup = model.Groups.Where((x) => string.Compare(x.Name, gname, true) == 0).FirstOrDefault();
            if (rDxFGroup == null)
            {
                rDxFGroup = new DxfGroup() { Name = gname, Selectable = igroup.Selectable };
                model.SetHandle(rDxFGroup);
                model.Groups.Add(rDxFGroup);

            }
            aEntity.DxfGroupHandle = rDxFGroup.Handle;
            return rDxFGroup.Handle;
        }
    }


}
