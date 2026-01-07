using System;
using System.Collections.Generic;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.InventorDrawing;
using WW.Cad.Model.Tables;

namespace UOP.DXFGraphicsControl
{
    /// <summary>
    /// Class for extracting the handle to entity mapping.
    /// </summary>
    public class EntityHandleCollector : IEntityVisitor
	{
		private readonly bool includeVertices;
		private readonly IDictionary<ulong, DxfEntity> handleToEntityMap = new Dictionary<ulong, DxfEntity>();
		private readonly ISet<ulong> visitedBlocks = new SortedSet<ulong>();


		/// <summary>
		/// Initializes a new instance of the <see cref='EntityHandleCollector'/> class.
		/// </summary>
		/// <param name='includeVertices'>if set to <c>true</c> include VERTEX and SEQEND entities, which are part of POLYLINEs.</param>
		public EntityHandleCollector(bool includeVertices)
		{
			this.includeVertices = includeVertices;
		}

		/// <summary>
		/// Get the handle to entity mapping for a given model.
		/// </summary>
		/// <remarks>
		/// This will include the VERTEX entities belonging to POLYLINEs.
		/// </remarks>
		/// <param name='model'>The model for which the handle to entity mapping is requested.</param>
		/// <returns>The handle to entity mapping.</returns>
		public static IDictionary<ulong, DxfEntity> GetHandleToEntityMap(DxfModel model)
		{
			return GetHandleToEntityMap(model, true);
		}

		/// <summary>
		/// Get the handle to entity mapping for a given model.
		/// </summary>
		/// <param name='model'>The model for which the handle to entity mapping is requested.</param>
		/// <param name='includeVertices'>Include VERTEX entities belonging to POLYLINEs?</param>
		/// <returns>The handle to entity mapping.</returns>
		public static IDictionary<ulong, DxfEntity> GetHandleToEntityMap(DxfModel model, bool includeVertices)
		{
			if (!model.Header.Handling)
			{
				throw new ArgumentException("File does not have handles!");
			}
			EntityHandleCollector collector = new EntityHandleCollector(includeVertices);
			BasicEntityVisitor.Visit(model, collector);
			return collector.handleToEntityMap;
		}

		/// <summary>
		/// Register a simple entity.
		/// </summary>
		/// <param name='entity'>Simple entity.</param>
		private void Register(DxfEntity entity)
		{
			handleToEntityMap[entity.Handle] = entity;
		}

		/// <summary>
		/// Register the entities in a block.
		/// </summary>
		/// <param name='block'>The block.</param>
		private void Register(DxfBlock block)
		{
			if (block != null)
			{
				ulong handle = block.Handle;
				if (!visitedBlocks.Contains(handle))
				{
					visitedBlocks.Add(handle);
					BasicEntityVisitor.Visit(block.Entities, this);
				}
			}
		}

		/// <summary>
		/// Register an entity which inserts a block,
		/// </summary>
		/// <param name='insert'></param>
		private void Register(DxfInsertBase insert)
		{
			Register((DxfEntity)insert);
			Register(insert.Block);
		}

		/// <summary>
		/// Register a dimension.
		/// </summary>
		/// <remarks>
		/// Dimensions reference a block with their representation.
		/// </remarks>
		/// <param name='dimension'>The dimension.</param>
		private void Register(DxfDimension dimension)
		{
			Register((DxfEntity)dimension);
			Register(dimension.Block);
		}

		/// <summary>
		/// Register a polyline.
		/// </summary>
		/// <remarks>
		/// Polylines include vertices as entities of their own.
		/// </remarks>
		/// <param name='polyline'>The polyline.</param>
		/// <param name='vertices'>The polyline's vertices.</param>
		private void Register(DxfPolylineBase polyline, IEnumerable<DxfEntity> vertices)
		{
			Register(polyline);
			if (includeVertices)
			{
				BasicEntityVisitor.Visit(vertices, this);
				Register(polyline);
			}
		}
		#region Implementation of IEntityVisitor

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(Dxf3DFace face)
		{
			Register(face);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(Dxf3DSolid solid)
		{
			Register(solid);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfArc arc)
		{
			Register(arc);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfAttribute attribute)
		{
			Register(attribute);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfAttributeDefinition attributeDefinition)
		{
			Register(attributeDefinition);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfCircle circle)
		{
			Register(circle);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfDimension.Aligned dimension)
		{
			Register(dimension);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfDimension.Angular3Point dimension)
		{
			Register(dimension);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfDimension.Angular4Point dimension)
		{
			Register(dimension);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfDimension.Diametric dimension)
		{
			Register(dimension);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfDimension.Linear dimension)
		{
			Register(dimension);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfDimension.Ordinate dimension)
		{
			Register(dimension);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfDimension.Radial dimension)
		{
			Register(dimension);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfEllipse ellipse)
		{
			Register(ellipse);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfHatch hatch)
		{
			Register(hatch);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfImage image)
		{
			Register(image);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfInsert insert)
		{
			Register(insert);
			BasicEntityVisitor.Visit(insert.Attributes, this);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfLeader leader)
		{
			Register(leader);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfLine line)
		{
			Register(line);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfLwPolyline polyline)
		{
			Register(polyline);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfMeshFace meshFace)
		{
			Register(meshFace);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfMLine mline)
		{
			Register(mline);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfMText mtext)
		{
			Register(mtext);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfPoint point)
		{
			Register(point);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfPolyfaceMesh mesh)
		{
			Register(mesh, mesh.Vertices);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfPolygonMesh mesh)
		{
			if (!includeVertices)
			{
				Register(mesh);
				return;
			}
			Register(mesh, mesh.Vertices);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfPolygonSplineMesh mesh)
		{
			if (!includeVertices)
			{
				Register(mesh);
				return;
			}
			IList<DxfVertex3D> approxPoints = mesh.ApproximationPoints;
			IList<DxfVertex3D> controlPoints = mesh.ControlPoints;
			List<DxfVertex3D> vertexList = new List<DxfVertex3D>();
			vertexList.AddRange(approxPoints);
			vertexList.AddRange(controlPoints);
			Register(mesh, vertexList);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfPolyline2D polyline)
		{
			Register(polyline, polyline.Vertices);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfPolyline2DSpline polyline)
		{
			if (!includeVertices)
			{
				Register(polyline);
				return;
			}
			List<DxfVertex2D> vertexList = new List<DxfVertex2D>(polyline.ApproximationPoints.Count + polyline.ControlPoints.Count);
			vertexList.AddRange(polyline.ApproximationPoints);
			vertexList.AddRange(polyline.ControlPoints);
			Register(polyline, vertexList);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfPolyline3D polyline)
		{
			Register(polyline, polyline.Vertices);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfPolyline3DSpline polyline)
		{
			if (!includeVertices)
			{
				Register(polyline);
				return;
			}
			List<DxfVertex3D> vertexList = new List<DxfVertex3D>(polyline.ApproximationPoints.Count + polyline.ControlPoints.Count);
			vertexList.AddRange(polyline.ApproximationPoints);
			vertexList.AddRange(polyline.ControlPoints);
			Register(polyline, vertexList);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfRay ray)
		{
			Register(ray);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfShape shape)
		{
			Register(shape);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfRegion region)
		{
			Register(region);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfSequenceEnd sequenceEnd)
		{
			Register(sequenceEnd);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfSolid solid)
		{
			Register(solid);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfSpline spline)
		{
			Register(spline);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfTable table)
		{
			Register(table);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfText text)
		{
			Register(text);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfTolerance tolerance)
		{
			Register(tolerance);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfVertex2D vertex)
		{
			Register(vertex);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfVertex3D vertex)
		{
			Register(vertex);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfViewport viewport)
		{
			Register(viewport);
		}

		/// <summary>
		/// Visits the specified entity.
		/// See the <see cref='IEntityVisitor'/> interface for more details.
		/// </summary>
		public void Visit(DxfXLine xline)
		{
			Register(xline);
		}

		public void Visit(DxfProxyEntity proxyEntity)
		{
			Register(proxyEntity);
		}

		public void Visit(DxfWipeout image)
		{
			Register(image);
		}

		public void Visit(DxfIDBlockReference insert)
		{
			Register(insert);
		}

	

		public void Visit(DxfBody body)
		{
			Register(body);
		}

		public void Visit(DxfOle ole)
		{
			Register(ole);
		}

		public void Visit(DxfDimension.Arc dimension)
		{
            Register( dimension );
        }

        void IEntityVisitor.Visit( DxfHelix helix )
        {
            Register( helix );
        }

        void IEntityVisitor.Visit( DxfMesh line )
        {
            Register( line );
        }

        public void Visit( DxfMLeader mleader )
        {
			Register( mleader );
        }

        #endregion
    }
}
