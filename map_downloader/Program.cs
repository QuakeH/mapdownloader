using System;

namespace map_downloader
{


    //------------------------------------------------------------------------------  
    // <copyright company="Microsoft">  
    //     Copyright (c) 2006-2009 Microsoft Corporation.  All rights reserved.  
    // </copyright>  
    //------------------------------------------------------------------------------  

    using System;
    using System.IO;
    using System.Net;
    using System.Text;



    class TileSystem
    {
        private const double EarthRadius = 6378137;
        private const double MinLatitude = -85.05112878;
        private const double MaxLatitude = 85.05112878;
        private const double MinLongitude = -180;
        private const double MaxLongitude = 180;

        /// <summary>  
        /// Clips a number to the specified minimum and maximum values.  
        /// </summary>  
        /// <param name="n">The number to clip.</param>  
        /// <param name="minValue">Minimum allowable value.</param>  
        /// <param name="maxValue">Maximum allowable value.</param>  
        /// <returns>The clipped value.</returns>  
        private static double Clip(double n, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(n, minValue), maxValue);
        }

        /// <summary>  
        /// Determines the map width and height (in pixels) at a specified level  
        /// of detail.  
        /// </summary>  
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)  
        /// to 23 (highest detail).</param>  
        /// <returns>The map width and height in pixels.</returns>  
        public static uint MapSize(int levelOfDetail)
        {
            return (uint)256 << levelOfDetail;
        }

        /// <summary>  
        /// Determines the ground resolution (in meters per pixel) at a specified  
        /// latitude and level of detail.  
        /// </summary>  
        /// <param name="latitude">Latitude (in degrees) at which to measure the  
        /// ground resolution.</param>  
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)  
        /// to 23 (highest detail).</param>  
        /// <returns>The ground resolution, in meters per pixel.</returns>  
        public static double GroundResolution(double latitude, int levelOfDetail)
        {
            latitude = Clip(latitude, MinLatitude, MaxLatitude);
            return Math.Cos(latitude * Math.PI / 180) * 2 * Math.PI * EarthRadius / MapSize(levelOfDetail);
        }

        /// <summary>  
        /// Determines the map scale at a specified latitude, level of detail,  
        /// and screen resolution.  
        /// </summary>  
        /// <param name="latitude">Latitude (in degrees) at which to measure the  
        /// map scale.</param>  
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)  
        /// to 23 (highest detail).</param>  
        /// <param name="screenDpi">Resolution of the screen, in dots per inch.</param>  
        /// <returns>The map scale, expressed as the denominator N of the ratio 1 : N.</returns>  
        public static double MapScale(double latitude, int levelOfDetail, int screenDpi)
        {
            return GroundResolution(latitude, levelOfDetail) * screenDpi / 0.0254;
        }

        /// <summary>  
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)  
        /// into pixel XY coordinates at a specified level of detail.  
        /// </summary>  
        /// <param name="latitude">Latitude of the point, in degrees.</param>  
        /// <param name="longitude">Longitude of the point, in degrees.</param>  
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)  
        /// to 23 (highest detail).</param>  
        /// <param name="pixelX">Output parameter receiving the X coordinate in pixels.</param>  
        /// <param name="pixelY">Output parameter receiving the Y coordinate in pixels.</param>  
        public static void LatLongToPixelXY(double latitude, double longitude, int levelOfDetail, out int pixelX, out int pixelY)
        {
            latitude = Clip(latitude, MinLatitude, MaxLatitude);
            longitude = Clip(longitude, MinLongitude, MaxLongitude);

            double x = (longitude + 180) / 360;
            double sinLatitude = Math.Sin(latitude * Math.PI / 180);
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            uint mapSize = MapSize(levelOfDetail);
            pixelX = (int)Clip(x * mapSize + 0.5, 0, mapSize - 1);
            pixelY = (int)Clip(y * mapSize + 0.5, 0, mapSize - 1);
        }

        /// <summary>  
        /// Converts a pixel from pixel XY coordinates at a specified level of detail  
        /// into latitude/longitude WGS-84 coordinates (in degrees).  
        /// </summary>  
        /// <param name="pixelX">X coordinate of the point, in pixels.</param>  
        /// <param name="pixelY">Y coordinates of the point, in pixels.</param>  
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)  
        /// to 23 (highest detail).</param>  
        /// <param name="latitude">Output parameter receiving the latitude in degrees.</param>  
        /// <param name="longitude">Output parameter receiving the longitude in degrees.</param>  
        public static void PixelXYToLatLong(int pixelX, int pixelY, int levelOfDetail, out double latitude, out double longitude)
        {
            double mapSize = MapSize(levelOfDetail);
            double x = (Clip(pixelX, 0, mapSize - 1) / mapSize) - 0.5;
            double y = 0.5 - (Clip(pixelY, 0, mapSize - 1) / mapSize);

            latitude = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
            longitude = 360 * x;
        }

        /// <summary>  
        /// Converts pixel XY coordinates into tile XY coordinates of the tile containing  
        /// the specified pixel.  
        /// </summary>  
        /// <param name="pixelX">Pixel X coordinate.</param>  
        /// <param name="pixelY">Pixel Y coordinate.</param>  
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>  
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>  
        public static void PixelXYToTileXY(int pixelX, int pixelY, out int tileX, out int tileY)
        {
            tileX = pixelX / 256;
            tileY = pixelY / 256;
        }

        /// <summary>  
        /// Converts tile XY coordinates into pixel XY coordinates of the upper-left pixel  
        /// of the specified tile.  
        /// </summary>  
        /// <param name="tileX">Tile X coordinate.</param>  
        /// <param name="tileY">Tile Y coordinate.</param>  
        /// <param name="pixelX">Output parameter receiving the pixel X coordinate.</param>  
        /// <param name="pixelY">Output parameter receiving the pixel Y coordinate.</param>  
        public static void TileXYToPixelXY(int tileX, int tileY, out int pixelX, out int pixelY)
        {
            pixelX = tileX * 256;
            pixelY = tileY * 256;
        }

        /// <summary>  
        /// Converts tile XY coordinates into a QuadKey at a specified level of detail.  
        /// </summary>  
        /// <param name="tileX">Tile X coordinate.</param>  
        /// <param name="tileY">Tile Y coordinate.</param>  
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)  
        /// to 23 (highest detail).</param>  
        /// <returns>A string containing the QuadKey.</returns>  
        public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
        {
            StringBuilder quadKey = new StringBuilder();
            for (int i = levelOfDetail; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((tileX & mask) != 0)
                {
                    digit++;
                }
                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }

        /// <summary>  
        /// Converts a QuadKey into tile XY coordinates.  
        /// </summary>  
        /// <param name="quadKey">QuadKey of the tile.</param>  
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>  
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>  
        /// <param name="levelOfDetail">Output parameter receiving the level of detail.</param>  
        public static void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int levelOfDetail)
        {
            tileX = tileY = 0;
            levelOfDetail = quadKey.Length;
            for (int i = levelOfDetail; i > 0; i--)
            {
                int mask = 1 << (i - 1);
                switch (quadKey[levelOfDetail - i])
                {
                    case '0':
                        break;

                    case '1':
                        tileX |= mask;
                        break;

                    case '2':
                        tileY |= mask;
                        break;

                    case '3':
                        tileX |= mask;
                        tileY |= mask;
                        break;

                    default:
                        throw new ArgumentException("Invalid QuadKey digit sequence.");
                }
            }
        }
    }



    class Program
    {
        private static string outputPath = @"f:\data\sat_imgs";

        static void Download(string quadKey)
        {
            
            string url = String.Format("http://ecn.t2.tiles.virtualearth.net/tiles/a{0}.jpeg?g=6906", quadKey);
            string filepath = Path.Combine(outputPath, String.Format("a{0}.jpeg", quadKey));
            if (!File.Exists(filepath))
            {
                using (WebClient myWebClient = new WebClient())
                {
                    // Download the Web resource and save it into the current filesystem folder.
                    myWebClient.DownloadFile(url, filepath);
                }
            }
        }


        static void Get_TileBBox(double lat1, double lon1, double lat2, double lon2,int level)
        {
            int pixelX1, pixelY1, pixelX2, pixelY2;
            int tileX1, tileY1, tileX2, tileY2;

            TileSystem.LatLongToPixelXY(lat1, lon1, level, out pixelX1, out pixelY1);
            TileSystem.LatLongToPixelXY(lat2, lon2, level, out pixelX2, out pixelY2);
            TileSystem.PixelXYToTileXY(pixelX1, pixelY1, out tileX1, out tileY1);
            TileSystem.PixelXYToTileXY(pixelX2, pixelY2, out tileX2, out tileY2);


            TileSystem.TileXYToPixelXY(tileX1, tileY1, out pixelX1, out pixelY1);
            TileSystem.TileXYToPixelXY(tileX2+1, tileY2+1, out pixelX2, out pixelY2);


            double olat1, olon1, olat2, olon2;

            TileSystem.PixelXYToLatLong(pixelX1, pixelY1, level, out olat1, out olon1);
            TileSystem.PixelXYToLatLong(pixelX2, pixelY2, level, out olat2, out olon2);

            Console.WriteLine(olon1 + "," + olat1);
            Console.WriteLine(olon2 + "," + olat2);

        }


        static void Download_sateimg()
        {
            int level = 19;

            int pixelX1, pixelY1, pixelX2, pixelY2;
            double lon1 = -88.6034;
            double lat1 = 41.2379;

            double lon2 = -88.1891;
            double lat2 = 40.8815;

            Get_TileBBox(lat1, lon1, lat2, lon2, level);

            outputPath = outputPath + "_" + lat1 + "_" + lon1 + "_" + lat2 + "_" + lon2;
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            TileSystem.LatLongToPixelXY(lat1, lon1, level, out pixelX1, out pixelY1);
            TileSystem.LatLongToPixelXY(lat2, lon2, level, out pixelX2, out pixelY2);
            int tileX1, tileY1, tileX2, tileY2;
            TileSystem.PixelXYToTileXY(pixelX1, pixelY1, out tileX1, out tileY1);
            TileSystem.PixelXYToTileXY(pixelX2, pixelY2, out tileX2, out tileY2);

            for (int i = tileX1; i < tileX2; i++)
            {
                for (int j = tileY1; j < tileY2; j++)
                {
                    try
                    {
                        string quadKey = TileSystem.TileXYToQuadKey(i, j, level);
                        Download(quadKey);
                        int pixelX, pixelY;
                        TileSystem.TileXYToPixelXY(i, j, out pixelX, out pixelY);
                        Console.WriteLine(quadKey + " " + i + " " + j + " " + pixelX + " " + pixelY);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }
            }
        }

        static void Download_corp_static()
        {

        }


        static void Gen_KML()
        {
            string path = @"F:\sat_imgs";
            string[] files = Directory.GetFiles(path, "*.jpeg");
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
        }


        static void Main(string[] args)
        {
            int level = 19;

            int pixelX1, pixelY1, pixelX2, pixelY2;
            double lon1 = -91.1838;
            double lat1 = 34.9614;

            double lon2 = -90.6066;
            double lat2 = 34.4368;

            lat1 = float.Parse(args[0]);
            lon1 = float.Parse(args[1]);
            lat2 = float.Parse(args[2]);
            lon2 = float.Parse(args[3]);


            //Get_TileBBox(lat1, lon1, lat2, lon2, level);

            Download_sateimg();
        }
    }
}
