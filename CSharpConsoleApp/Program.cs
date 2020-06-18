using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace CSharpConsoleApp {
    class Program {
        static void Main (string[] args) {

            string pointsShpFilePath = "..\\..\\..\\data\\gis_osm_pois_free_1.shp"; 
            string polygonesShpFilePath = "..\\..\\..\\data\\Zones.shp";

            var points = ReadShpFile (pointsShpFilePath);
            var polygones = ReadShpFile (polygonesShpFilePath);

            var filteredPoints = points.FindAll (e => (string) e.Attributes["fclass"] == "atm");

            foreach (Feature polygone in polygones) {
                var pointsContainedInPolygons = checkAffiliationToPolygon (polygone, filteredPoints);

                CreateShpFile (polygone, pointsContainedInPolygons);
            }
            Console.WriteLine ("Please check data folder");
        }

        public static List<Feature> ReadShpFile (string pathToShpFile) {
            GeometryFactory factory = new GeometryFactory ();
            ShapefileDataReader shapeFileDataReader = new ShapefileDataReader (pathToShpFile, factory);
            var features = new List<Feature> ();

            string[] fieldNames = new string[shapeFileDataReader.FieldCount];
            for (int i = 0; i < fieldNames.Length; i++)
                fieldNames[i] = shapeFileDataReader.GetName (i);

            while (shapeFileDataReader.Read ()) {
                AttributesTable attributesTable = new AttributesTable ();

                for (int i = 1; i < fieldNames.Length; i++)
                    attributesTable.Add (fieldNames[i], shapeFileDataReader.GetValue (i));

                Feature feature = new Feature (shapeFileDataReader.Geometry, attributesTable);
                features.Add (feature);
            }
            return features;
        }

        public static List<Feature> checkAffiliationToPolygon (Feature polygone, List<Feature> points) {
            var pointsContainedInPolygon = new List<Feature> ();

            foreach (Feature point in points) {

                bool isContained = SimplePointInAreaLocator.IsContained (point.Geometry.Coordinate, polygone.Geometry);

                if (isContained) {
                    pointsContainedInPolygon.Add (point);
                }
            }
            return pointsContainedInPolygon;
        }

        public static void CreateShpFile (Feature polygone, List<Feature> points) {
            GeometryFactory outGeomFactory = new GeometryFactory ();
            string folderPath = Path.Combine("..\\..\\..\\data\\", polygone.Attributes["name"].ToString());
            System.IO.Directory.CreateDirectory(folderPath);
            ShapefileDataWriter writer = new ShapefileDataWriter ($"{folderPath}/{polygone.Attributes["name"]}", outGeomFactory);

            DbaseFileHeader outDbaseHeader = ShapefileDataWriter.GetHeader ((Feature) points[0], points.Count);
            writer.Header = outDbaseHeader;
            writer.Write (points);
        }
    }
}