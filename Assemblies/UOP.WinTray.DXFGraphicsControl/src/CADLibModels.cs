using System;
using System.Collections.Generic;
using System.Windows.Media;
using UOP.DXFGraphics;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;

namespace UOP.DXFGraphicsControl
{
    public class CADLibModels :IDisposable
    {
        private CADLibModels( 
            ModelWithHandles modelwh, 
            ModelWithHandles overlay, 
            ModelWithHandles staticOverlay,
            ZoomData LastZooms,
            ZoomData PrevZooms,
            string guid, 
            Brush background,
            List<DxfEntity> TempEntities,
            int UCSMode,
            double UCSSize,
            dxxColors UCScolor,
            bool TempApplied)
        {
            this.modelwh = modelwh;
            this.overlay = overlay;
            this.staticOverlay = staticOverlay;
            this.GUID = guid;
            this.Background = background;
            this.LastZoom = LastZooms;
            this.PrevZoom = PrevZooms;
            this.TempEntities = new List<DxfEntity>( TempEntities );
            this.UCSMode = UCSMode;
            this.UCSSize = UCSSize;
            this.UCSColor = UCSColor;
            this.TempApplied = TempApplied;
        }

        public CADLibModels()
        {
            modelwh = new ModelWithHandles();
            overlay = new ModelWithHandles();
            staticOverlay = new ModelWithHandles();
            GUID = "";
            Background = new SolidColorBrush( System.Windows.Media.Colors.White );
            LastZoom = new ZoomData();
            PrevZoom = new ZoomData();
            TempEntities = new List<DxfEntity>();
            UCSMode = 0;
            UCSSize = -1 * 2;
            UCSColor = dxxColors.BlackWhite;
            TempApplied = false;
        }

        public ModelWithHandles modelwh { get; set; }
        public ModelWithHandles overlay { get; set; }
        public ModelWithHandles staticOverlay { get; set; }
        public string GUID { get; set; }
        public Brush Background { get; set; }
        public ZoomData LastZoom { get; set; }
        public ZoomData PrevZoom { get; set; }
        public List<DxfEntity> TempEntities { get; set; }
        public int UCSMode { get; set; }
        public double UCSSize { get; set; } //as screen fraction
        public dxxColors UCSColor { get; set; }
        public DxfUcs UCS { get { return modelwh?.Model.Header.Ucs; } }
        public bool TempApplied { get; set; }

        public int entityCount
        {
            get
            {
                int ret = 0;
                if (modelwh != null)
                    ret += modelwh.Model.Entities.Count;
                if (overlay != null)
                    ret += overlay.Model.Entities.Count;
                if (staticOverlay != null)
                    ret += staticOverlay.Model.Entities.Count;

                return ret;
            }
        }

        public CADLibModels Clone()
        {
            foreach (var ent in TempEntities)
            {
                modelwh.Model.Entities.Remove( ent );
            }

            return new CADLibModels(
            new ModelWithHandles( new DxfModel( modelwh.Model ) ),
            new ModelWithHandles( new DxfModel( overlay.Model ) ),
            new ModelWithHandles( new DxfModel( staticOverlay.Model ) ),
            LastZoom.Clone(),
            PrevZoom.Clone(),
            GUID,
            Background,
            new List<DxfEntity>( TempEntities ),
            UCSMode,
            UCSSize,
            UCSColor,
            false);
        }

        public void Clear()
        {
            TempEntities?.Clear();
            GUID = "";
        }

        public void StoreTemp( IEnumerable<DxfEntity> ents, bool saveToFile )
        {
            if (saveToFile) return;
            TempEntities?.AddRange( ents );
        }

        public void ResetOverlays()
        {
           
            overlay = new ModelWithHandles();
            staticOverlay = new ModelWithHandles();
        }

        public void Dispose()
        {
            this.modelwh?.Model?.Dispose();
            this.overlay?.Model?.Dispose();
            this.staticOverlay?.Model?.Dispose();
            TempEntities?.Clear();
            TempEntities = null;
            this.modelwh = null;
            this.overlay = null;
            this.staticOverlay = null;
        }
    }
}
