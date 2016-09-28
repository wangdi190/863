using System.Windows;
using System.Windows.Media.Media3D;

namespace MyControlLibrary.FlipPanel
{
    internal class Plane
    {
        public static MeshGeometry3D AddPlaneToMesh(MeshGeometry3D mesh, Vector3D normal, Point3D upperLeft, Point3D lowerLeft, Point3D lowerRight, Point3D upperRight)
        {
            int offset = mesh.Positions.Count;

            mesh.Positions.Add(upperLeft);
            mesh.Positions.Add(lowerLeft);
            mesh.Positions.Add(lowerRight);
            mesh.Positions.Add(upperRight);

            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            mesh.TextureCoordinates.Add(new Point(0,0));
            mesh.TextureCoordinates.Add(new Point(0,1));
            mesh.TextureCoordinates.Add(new Point(1,1));
            mesh.TextureCoordinates.Add(new Point(1,0));

            mesh.TriangleIndices.Add(offset + 0);
            mesh.TriangleIndices.Add(offset + 1);
            mesh.TriangleIndices.Add(offset + 2);
            mesh.TriangleIndices.Add(offset + 0);
            mesh.TriangleIndices.Add(offset + 2);
            mesh.TriangleIndices.Add(offset + 3);

            return mesh;
        }

        public static MeshGeometry3D Create(Vector3D normal, Point3D upperLeft, Point3D lowerLeft, Point3D lowerRight, Point3D upperRight)
        {
            return AddPlaneToMesh(new MeshGeometry3D(), normal, upperLeft, lowerLeft, lowerRight, upperRight); 
        }

        public static MeshGeometry3D CreateXY(Vector3D normal, double width, double height)
        {
            double w = width / 2.0;
            double h = height / 2.0;

            return Create(normal, new Point3D(-w, h, 0), new Point3D(-w, -h, 0), new Point3D(w, -h, 0), new Point3D(w, h, 0)); 
        }
    }
}
