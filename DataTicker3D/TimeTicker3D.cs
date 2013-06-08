using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DataTicker3D
{
   public class TimeTicker3D
   {
      public TimeTicker3D()
      {
         this.Brush = Brushes.MistyRose;
         this.TickerWidth = 2.0;
      }

      public GeometryModel3D TickerGeometryModel3D { get; protected set; }
      public Double TickerWidth { get; set; }
      public Brush Brush { get; set; }
      public TimeTicker3Dtransform transform { get; set; }

      private SortedDictionary<DateTime, Double> rawData_;
      public SortedDictionary<DateTime, Double> rawData
      {
         get { return rawData_; }
         set
         {
            rawData_ = value;
            setupTransform();
            setupGeometryModel();
         }
      }

      private void setupGeometryModel()
      {
         MeshGeometry3D meshGeom = new MeshGeometry3D();
         foreach (var reading in rawData_)
         {
            Double x = (reading.Key - transform.xStartDate).Days;
            Double y = (reading.Value - transform.yDatum) * transform.yExaggeration;
            Double z = transform.zAdjustment;
            meshGeom.Positions.Add(new Point3D(x, y, z - this.TickerWidth / 2));
            meshGeom.Positions.Add(new Point3D(x, y, z + this.TickerWidth / 2));
         }

         for (int i = 0; i < (rawData_.Count - 1) * 2; i += 2)
         {
            meshGeom.TriangleIndices.Add(i);
            meshGeom.TriangleIndices.Add(i + 1);
            meshGeom.TriangleIndices.Add(i + 2);

            meshGeom.TriangleIndices.Add(i + 1);
            meshGeom.TriangleIndices.Add(i + 3);
            meshGeom.TriangleIndices.Add(i + 2);
         }

         DiffuseMaterial material = new DiffuseMaterial();
         TickerGeometryModel3D = new GeometryModel3D();//(geometry, material);
         TickerGeometryModel3D.Geometry = meshGeom;
         TickerGeometryModel3D.Material = material;
         TickerGeometryModel3D.BackMaterial = material;
         //TickerGeometryModel3D.Transform = new Transform3DGroup();
      }

      private void setupTransform()
      {
         transform = new TimeTicker3Dtransform();
         transform.xStartDate = rawData_.FirstOrDefault().Key.Date;
         transform.yDatum = (from reading in rawData_
                             select reading.Value).Min();
      }

   }

   public class TimeTicker3Dtransform
   {
      public DateTime xStartDate { get; set; }
      public Double xExaggeration { get; set; }
      public Double yDatum { get; set; }
      public Double yExaggeration { get; set; }
      public Double zAdjustment { get; set; }

      public TimeTicker3Dtransform()
      {
         xStartDate = new DateTime(1, 1, 1);
         xExaggeration = 1.0;
         yDatum = 0.0;
         yExaggeration = 1.0;
         zAdjustment = 0.0;
      }

   }
}
