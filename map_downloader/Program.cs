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


        static void Download_sateimg(double lat1, double lon1, double lat2, double lon2,int level)
        {
            int pixelX1, pixelY1, pixelX2, pixelY2;

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


        static void Gen_meta(string regionname)
        {
            StreamWriter sw = new StreamWriter(@"F:\data\"+ regionname+".txt");
            string path = @"F:\data\" + regionname;
            string[] files = Directory.GetFiles(path, "*.jpeg");
            int minpixelX = -1, maxpixelX = -1, minpixelY = -1, maxpixelY = -1;
            double minlon = -1, maxlon = -1, minlat = -1, maxlat = -1;

            int level = 19;
            foreach (var file in files)
            {
                int tileX,tileY;
                string quadKey = Path.GetFileNameWithoutExtension(file).Remove(0, 1);
                TileSystem.QuadKeyToTileXY(quadKey, out tileX, out tileY, out level);
                int pixelX, pixelY;
                double lon, lat;
                TileSystem.TileXYToPixelXY(tileX, tileY, out pixelX, out pixelY);
                TileSystem.PixelXYToLatLong(pixelX, pixelY, level, out lat, out lon);

                if (pixelX<minpixelX || minpixelX==-1)
                {
                    minpixelX = pixelX;
                }

                if (pixelX > maxpixelX || maxpixelX == -1)
                {
                    maxpixelX = pixelX;
                }

                if (pixelY < minpixelY || minpixelY == -1)
                {
                    minpixelY = pixelY;
                }

                if (pixelY > maxpixelY || maxpixelY == -1)
                {
                    maxpixelY = pixelY;
                }

                if (lon < minlon || minlon == -1)
                {
                    minlon = lon;
                }

                if (lon > maxlon || maxlon == -1)
                {
                    maxlon = lon;
                }

                if (lat < minlat || minlat == -1)
                {
                    minlat = lat;
                }

                if (lat > maxlat || maxlat == -1)
                {
                    maxlat = lat;
                }


                sw.WriteLine(quadKey + "," + tileX + "," + tileY + "," + level + ","+ pixelX + "," + pixelY + "," + lat + "," + lon);
            }
            sw.Close();

            sw = new StreamWriter(@"F:\data\" + regionname + "_meta.txt");
            sw.WriteLine(minpixelX + "," + minpixelY + "," + maxpixelX + "," + maxpixelY + "," + level);
            sw.WriteLine(minlat + "," + maxlat + "," + minlon + "," + maxlon + "," + level);
            sw.WriteLine((maxpixelX-minpixelX)+","+ (maxpixelY - minpixelY));
            sw.Close();
        }


        static void Main(string[] args)
        {
            int level = 19;

            int pixelX1, pixelY1, pixelX2, pixelY2;
            double lon1 = -91.1838;
            double lat1 = 34.9614;

            double lon2 = -90.6066;
            double lat2 = 34.4368;
            /*
            lat1 = float.Parse(args[0]);
            lon1 = float.Parse(args[1]);
            lat2 = float.Parse(args[2]);
            lon2 = float.Parse(args[3]);
            */
            //Get_TileBBox(34.9619345057769, -91.1844635009766, 34.4363631809337, -90.6063079833984, level);

            //Download_sateimg(lat1, lon1, lat2, lon2, level);
            Gen_meta("sat_imgs_41.2380599975586_-88.6040496826172_40.8813323974609_-88.1886291503906");
            Gen_meta("sat_imgs_47.3281211853027_-117.489852905273_46.8831939697266_-116.983795166016");
            Gen_meta("sat_imgs_34.9619331359863_-91.1844635009766_34.4363632202148_-90.6063079833984");
            //Gen_meta("sat_imgs_47.3281_-117.4898_46.8832_-116.9838");
        }
    }
}
