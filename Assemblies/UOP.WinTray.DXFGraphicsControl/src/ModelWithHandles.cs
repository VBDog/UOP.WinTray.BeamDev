using System.Collections.Generic;
using WW.Cad.Base;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Tables;

namespace UOP.DXFGraphicsControl
{
    public class ModelWithHandles
	{
		private readonly DxfModel model;
		private readonly IDictionary<ulong, DxfEntity> handleToEntityMap;

		public ModelWithHandles(DxfModel model)
		{
			IList<DxfMessage> messages = new List<DxfMessage>();
			model.Repair(messages);
			this.model = model;
			handleToEntityMap = EntityHandleCollector.GetHandleToEntityMap(model, false);

			// register event listeners for adding/removing entities to/from the model
			model.Entities.Added += Entities_Added;
			model.Entities.Removed += EntitiesOnRemoved;

			// register event listeners for adding/removing blocks to/from the model
			model.Blocks.Set += BlocksOnSet;
			model.Blocks.Removed += BlocksOnRemoved;

			// register event listeners for adding/removing entities to/from blocks
			foreach (DxfBlock block in model.Blocks)
			{
				block.Entities.Added += Entities_Added;
				block.Entities.Removed += EntitiesOnRemoved;
			}
		}

		public ModelWithHandles()
		{
			try { this.model = new DxfModel(); } catch { }
			this.model.SetHandles();
			handleToEntityMap = EntityHandleCollector.GetHandleToEntityMap(model, false);
		
			// register event listeners for adding/removing entities to/from the model
			model.Entities.Removed += EntitiesOnRemoved;
			model.Entities.Added += Entities_Added;

			// register event listeners for adding/removing blocks to/from the model
			model.Blocks.Set += BlocksOnSet;
			model.Blocks.Removed += BlocksOnRemoved;
		}

		public void ClearHandlers()
        {
			if (model.Entities == null) return;
            model.Entities.Removed -= EntitiesOnRemoved;
            model.Entities.Added -= Entities_Added;
            model.Blocks.Set -= BlocksOnSet;
            model.Blocks.Removed -= BlocksOnRemoved;
            foreach (DxfBlock block in model.Blocks)
            {
                block.Entities.Added -= Entities_Added;
                block.Entities.Removed -= EntitiesOnRemoved;
            }
        }

        private void Entities_Added(object sender, int index, DxfEntity item)
		{
			model.SetHandle(item);
			//*** Shouldn't happen but did
			if (handleToEntityMap.ContainsKey( item.Handle ))
				handleToEntityMap[ item.Handle ] = item;
			else
				handleToEntityMap.Add(item.Handle, item);
		}

		private void BlocksOnRemoved(object sender, int index, DxfBlock item)
		{
			// remove event listeners
			item.Entities.Added -= Entities_Added;
			item.Entities.Removed -= EntitiesOnRemoved;

			// remove block's entities from map
			foreach (DxfEntity entity in item.Entities)
			{
				handleToEntityMap.Remove(entity.Handle);
			}
		}

		private void BlocksOnSet(object sender, int index, DxfBlock oldItem, DxfBlock newItem)
		{
			if (oldItem != null)
			{
				BlocksOnRemoved(sender, index, oldItem);
			}
			// add event listeners
			newItem.Entities.Added += Entities_Added;
			newItem.Entities.Removed += EntitiesOnRemoved;
			// add entities to map
			foreach (DxfEntity entity in newItem.Entities)
			{
				model.SetHandle(entity);
				handleToEntityMap.Add(entity.Handle, entity);
			}
		}

		private void EntitiesOnRemoved(object sender, int index, DxfEntity item)
		{
			handleToEntityMap.Remove(item.Handle);
		}

		public DxfModel Model
		{
			get { return model; }
		}

		public DxfEntity GetEntity(ulong handle)
		{
			return GetEntity(handle, null);
		}
		public DxfEntity GetEntity(ulong handle,string ACClassFilter = null)
		{
	
			if (handleToEntityMap.TryGetValue(handle, out DxfEntity entity))
			{
				if (!string.IsNullOrWhiteSpace(ACClassFilter))
                {
				
					if (string.Compare(entity.AcClass,ACClassFilter,ignoreCase:true) != 0)
                    {
						return null;
                    }
                }
			}
			return entity;
		}
	}
}
