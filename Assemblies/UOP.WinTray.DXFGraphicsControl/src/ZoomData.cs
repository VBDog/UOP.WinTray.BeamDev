using WW.Math;

namespace UOP.DXFGraphicsControl
{
    public class ZoomData
    {
        public double scale { get; set; }
        public Bounds3D bounds { get; set; }
        public Vector3D translation { get; set; }
        public bool IsValid { get { return bounds != null; } }

        public ZoomData(double scale, Bounds3D aBounds, Vector3D translation)
        {
            this.scale = scale;
            this.bounds = new Bounds3D(aBounds);
            this.translation = new Vector3D( translation );
        }

        public ZoomData() { }

        public ZoomData Clone()
        {
            if (IsValid)
                return new ZoomData( scale, bounds, translation );
            else
                return new ZoomData();
        }

        public void Set( double scale, Bounds3D aBounds, Vector3D translation )
        {
            if (aBounds == null) return;
            this.scale = scale;
          bounds = aBounds.Clone();
            this.translation = new Vector3D( translation );
        }
    }
}
