using System;
using System.Collections.Generic;
using System.Linq;
using Assistant;
using Ultima;

namespace RazorEnhanced
{

    // Pathfind Core

    /// <summary>
    /// Class representing an (X,Y) coordinate. Optimized for pathfinding tasks.
    /// </summary>
    public class Tile_My
    {
        /// <summary>
        /// Create a Tile starting from X,Y coordinates (see PathFindig.GetPath)
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Returns a Tile object</returns>
        public Tile_My(int x, int y)
        {
            X = x;
            Y = y;
            Conflict = false;
        }

        /// <summary>Coordinate X.</summary>
        public int X { get; set; }
        /// <summary>Coordinate Y.</summary>
        public int Y { get; set; }

        public bool Conflict { get; set; }

        public override bool Equals(Object obj)
        {
            var loc = obj as Tile_My;
            return X == loc.X && Y == loc.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            // return $"{X};{Y}";
            return Convert.ToString(X) + ";" + Convert.ToString(Y);
        }
    }

    public static class PathMove_My
    {
        /// <summary>
        /// This function will get the path from actual position to a X,Y coordinate of same map
        /// </summary>
        /// <param name="x">X coordinate of same map</param>
        /// <param name="y">Y coordinate of same map</param>
        /// <param name="scanMaxRange">Max range to scan a path (x, y) should be included in this max range</param>
        /// <param name="ignoremob">consider mobs as obsticles or not</param>
        /// <returns></returns>
        public static List<Tile_My> GetPath(int x, int y, int scanMaxRange, bool ignoremob, PathFinding_My.Route.AdditionalProperties add_prop)
        {
            scanMaxRange += add_prop.ScanAdditionalRange;

            var playerPosition = Player.Position;
            //var squareGrid = new SquareGrid(playerPosition.X, playerPosition.Y, x, y);
            var squareGrid = new SquareGrid_My(playerPosition.X, playerPosition.Y, scanMaxRange);

            if (!squareGrid.Tiles.Any(l => l.X == x && l.Y == y))
            {
                //Logging.Log($"Tiles does not contain goal: {playerPosition.X};{playerPosition.Y} to {x};{y}");
                return null;
            }

            SquareGrid_My temp = squareGrid;
            var aStarSearch = new AStarSearch_My(squareGrid, new Tile_My(playerPosition.X, playerPosition.Y), new Tile_My(x, y), playerPosition.Z, ignoremob, add_prop);
            var result = aStarSearch.FindPath();

            if (result == null)
            {
                //Logging.Log($"The result is null, path not found from {playerPosition.X};{playerPosition.Y} to {x};{y}");
                return null;
            }
            else
            {
                if (result.Count == 0)
                    result.Add(new Tile_My(x, y));

                /*Logging.Log($"Path found from {playerPosition.X};{playerPosition.Y} to {x};{y}");
                foreach (var tile in result)
                {
                    Logging.Log($"{tile.X},{tile.Y}");
                }*/
                return result;
            }

        }

        /// <summary>
        /// This function will get the path from actual position to a X,Y coordinate of same map, getting as scanMaxRange the max difference between positions + 2
        /// </summary>
        /// <param name="x">X coordinate of same map</param>
        /// <param name="y">Y coordinate of same map</param>
        /// <param name="ignoremob">consider mobs as obsticles or not</param>
        /// <returns></returns>
        public static List<Tile_My> GetPath(int x, int y, bool ignoremob, PathFinding_My.Route.AdditionalProperties add_prop)
        {
            var position = Player.Position;

            var distanceX = Math.Abs(position.X - x);
            var distanceY = Math.Abs(position.Y - y);
            var scanMaxRange = Math.Max(distanceX, distanceY) + 2;

            return GetPath(x, y, scanMaxRange, ignoremob, add_prop);
        }
    }

    // 这里注释掉，这个直接用PathFinding 中的 TileExtensions，不然编译会报冲突
    //internal static class TileExtensions_My
    //{
    //    public static bool Ignored(this Ultima.Tile tile, bool no)
    //    {
    //        return (tile.ID == 2 || tile.ID == 0x1DB || (tile.ID >= 0x1AE && tile.ID <= 0x1B5));
    //    }
    //}

    internal class SquareGrid_My
    {
        public const int BigCost = int.MaxValue/2;

        private const int PersonHeight = 16;
        private const int StepHeight = 2;
        private const TileFlag ImpassableSurface = TileFlag.Impassable | TileFlag.Surface;

        // DIRS is directions
        public static readonly Tile_My[] Dirs =
        {
            new Tile_My(1, 0), // east 
            new Tile_My(-1, 0), // west
            new Tile_My(0, 1), // south
            new Tile_My(0, -1), // north
            new Tile_My(-1, -1), // NW
            new Tile_My(1, 1), // SE
            new Tile_My(-1, 1), // SW
            new Tile_My(1, -1) // NE
        };

        public SquareGrid_My(int x, int y, int squareSize)
        {
            Tiles = new List<Tile_My>();
            TilesRect = new Rectangle2D(x - squareSize, y - squareSize, x + squareSize, y + squareSize);
            for (var i = x - squareSize; i < x + squareSize; i++)
            {
                for (var j = y - squareSize; j < y + squareSize; j++)
                {
                    Tiles.Add(new Tile_My(i, j));
                }
            }


        }

        public List<Tile_My> Tiles;
        public Rectangle2D TilesRect;

        // Check if a location is within the bounds of this grid.
        public bool InBounds(Tile_My id)
        {
            //X == loc.X && Y == loc.Y
            Assistant.Point2D point = new Assistant.Point2D(id.X, id.Y);
            bool result = TilesRect.Contains(point);
            //bool result2 = Tiles.Any(x => x.Equals(id));
            return result; 
        }


        static HashSet<int> RoadIds = new HashSet<int>() { 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7c,
            0x3cf, 0x3d0, 0x3d1, 0x3d2, 0x3d3, 0x3d4, 0x3d5, 0x3d6, 0x3d7, 0x3d8, 0x3d9, 0x3da,
            0x3db, 0x3dc, 0x3dd, 0x3de, 0x3df, 0x3e0, 0x3e1, 0x3e2, 0x3e3, 0x3e4, 0x3e5, 0x3e6, 0x3e7, 0x3e8, 0x3e9, 0x3ea,
            0x3eb, 0x3ec, 0x3ed, 0x3ee, 0x3ef, 0x3f0, 0x3f1, 0x3f2, 0x3f3, 0x3f4, 0x3f5, 0x3f6, 0x3f7, 0x3f8, 0x3f9, 0x3fa,
            0x3fb, 0x3fc, 0x3fd, 0x3fe, 0x3ff, 0x400, 0x401, 0x402, 0x403, 0x404, 0x405 };

        public int Cost(List<Assistant.Item> items, Point3D loc, Map map, Tile_My b, bool ignoremob, PathFinding_My.Route.AdditionalProperties add_prop, out int bZ)
        {
            int xForward = b.X, yForward = b.Y;
            
            //var xRange = Enumerable.Range(xForward - 1, xForward + 1);
            //var yRange = Enumerable.Range(yForward - 1, yForward + 1);
            //var items = World.Items.Values.Where(item => item.OnGround && (xRange.Contains(item.Position.X) && yRange.Contains(item.Position.Y)));
            int newZ = 0;
            // note: if cost is > 1 the number of calls to cost grows huge, and thus the time to compute path is long
            int cost = 1;
            GetStartZ(loc, map, items.Where(x => x.Position.X == loc.X && x.Position.Y == loc.Y), out var startZ, out var startTop);
            var moveIsOk = Check(map, items.Where(x => x.Position.X == xForward && x.Position.Y == yForward), xForward, yForward, startTop, startZ, ignoremob, add_prop, out newZ);
            if (b.X == loc.X && b.Y > loc.Y) //North
            {
                if (moveIsOk)
                {
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward && x.Position.Y == yForward - 1), xForward, yForward - 1, startTop, startZ, ignoremob, add_prop, out newZ);
                }
            }
            else if (b.X == loc.X && b.Y < loc.Y) //South
            {
                if (moveIsOk)
                {
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward && x.Position.Y == yForward + 1), xForward, yForward + 1, startTop, startZ, ignoremob, add_prop, out newZ);
                }
            }
            else if (b.X > loc.X && b.Y == loc.Y) //West
            {
                if (moveIsOk)
                {
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward - 1 && x.Position.Y == yForward), xForward - 1, yForward, startTop, startZ, ignoremob, add_prop, out newZ);
                }

            }
            else if (b.X > loc.X && b.Y == loc.Y) //East
            {
                if (moveIsOk)
                {
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward + 1 && x.Position.Y == yForward), xForward + 1, yForward, startTop, startZ, ignoremob, add_prop, out newZ);
                }
            }
            
            else if (b.X > loc.X && b.Y > loc.Y) //Down
            {
                if (moveIsOk)
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward && x.Position.Y == yForward - 1), xForward, yForward - 1, startTop, startZ, ignoremob, add_prop, out newZ);
                if (moveIsOk)
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward - 1 && x.Position.Y == yForward), xForward - 1, yForward, startTop, startZ, ignoremob, add_prop, out newZ);
            }
            else if (b.X < loc.X && b.Y < loc.Y) //UP
            {
                if (moveIsOk)
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward && x.Position.Y == yForward + 1), xForward, yForward + 1, startTop, startZ, ignoremob, add_prop, out newZ);
                if (moveIsOk)
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward + 1 && x.Position.Y == yForward), xForward + 1, yForward, startTop, startZ, ignoremob, add_prop, out newZ);
            }
            else if (b.X > loc.X && b.Y < loc.Y) //Right
            {
                if (moveIsOk)
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward && x.Position.Y == yForward + 1), xForward, yForward + 1, startTop, startZ, ignoremob, add_prop, out newZ);
                if (moveIsOk)
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward - 1 && x.Position.Y == yForward), xForward - 1, yForward, startTop, startZ, ignoremob, add_prop, out newZ);
            }
            else if (b.X < loc.X && b.Y > loc.Y) //Left
            {
                if (moveIsOk)
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward && x.Position.Y == yForward - 1), xForward, yForward - 1, startTop, startZ, ignoremob, add_prop, out newZ);
                if (moveIsOk)
                    moveIsOk = Check(map, items.Where(x => x.Position.X == xForward + 1 && x.Position.Y == yForward), xForward + 1, yForward, startTop, startZ, ignoremob, add_prop, out newZ);
            }
            
            if (moveIsOk)
            {
                // following the road didn't reduce time
                //var landTile = map.Tiles.GetLandTile(b.X, b.Y);
                //if (RoadIds.Contains(landTile.ID))
                //    cost -= 1;
                bZ = newZ;
                return cost;
            }

            bZ = startZ;
            return BigCost;
        }

        private static bool CheckUserCustom (int x, int y, PathFinding_My.Route.AdditionalProperties add_prop)
        {
            if (add_prop == null)
                return false;

            // 检查坐标是否在用户强制允许区域列表中
            if (add_prop.ForceEnablePolygonAreaList != null && add_prop.ForceEnablePolygonAreaList.Count > 0)
            {
                // 强制允许该位置，即使此位置有玩家自建房屋
                if (PolygonArea.IsInPolygonList(x, y, add_prop.ForceEnablePolygonAreaList) >= 0)
                    return false;
            }

            // 检查坐标是否在用户强制禁止区域列表中
            if (add_prop.ForceDisablePolygonAreaList != null && add_prop.ForceDisablePolygonAreaList.Count > 0)
            {
                // 如果坐标在禁止列表中则返回
                if (PolygonArea.IsInPolygonList(x, y, add_prop.ForceDisablePolygonAreaList) >= 0)
                    return true;
            }

            // 检查用户自定义障碍物
            if (add_prop.obstacle_list.Count > 0)
            {
                foreach (Point2D loc in add_prop.obstacle_list)
                {
                    if (loc.X == x && loc.Y == y)
                        return true; // 该点有用户自定义障碍物
                }
            }
            return false;
        }

        private static bool Check(Map map, IEnumerable<Assistant.Item> items, int x, int y, int startTop, int startZ, bool ignoremob, PathFinding_My.Route.AdditionalProperties add_prop, out int newZ)
        {
            newZ = 0;

            //Ultima.HuedTile[] tiles = map.Tiles.GetStaticTiles(x, y, true);
            List<Statics.TileInfo> tiles = Statics.GetStaticsTileInfo(x, y, Player.Map);
            Ultima.Tile landTile = (Ultima.Tile)map.Tiles.GetLandTile(x, y);
            var landData = TileData.LandTable[landTile.ID & (TileData.LandTable.Length - 1)];
            var landBlocks = (landData.Flags & TileFlag.Impassable) != 0;
            var considerLand = !landTile.Ignored();

            int landZ = 0, landCenter = 0, landTop = 0;

            GetAverageZ(map, x, y, ref landZ, ref landCenter, ref landTop);

            var moveIsOk = false;

            var stepTop = startTop + StepHeight;
            var checkTop = startZ + PersonHeight;

            var ignoreDoors = false;

            if (Player.IsGhost || Engine.MainWindow.AutoOpenDoors.Checked)
                ignoreDoors = true;

            const bool ignoreSpellFields = true;

            int itemZ, itemTop, ourZ, ourTop, testTop;
            ItemData itemData;
            TileFlag flags;

            // 检查用户自定义的障碍点
            if (CheckUserCustom (x, y, add_prop))
                return false;

            // Check For mobiles
            if (!ignoremob)
            {
                //var mobs = World.Mobiles.Values;
                List<Assistant.Mobile> result = new List<Assistant.Mobile>();
                foreach (var entry in World.Mobiles)
                {
                    Assistant.Mobile m = entry.Value;
                    if (m.Position.X == x && m.Position.Y == y && m.Serial != Player.Serial)
                        result.Add(m);
                }
                if (result.Count > 0) // mob present at this spot.
                {
                    if (World.Player.Stam < World.Player.StamMax) // no max stam, avoid this location
                        return false;
                }
            }
            // Check for deed player house
            if (Statics.CheckDeedHouse(x, y))
            {
                return false;
            }

            #region Tiles
            foreach (var tile in tiles)
            {
                itemData = TileData.ItemTable[tile.ID & (TileData.ItemTable.Length - 1)];

                flags = itemData.Flags;

                if ((flags & ImpassableSurface) != TileFlag.Surface)
                {
                    continue;
                }

                itemZ = tile.Z;
                itemTop = itemZ;
                ourZ = itemZ + itemData.CalcHeight;
                ourTop = ourZ + PersonHeight;
                testTop = checkTop;

                if (moveIsOk)
                {
                    var cmp = Math.Abs(ourZ - Player.Position.Z) - Math.Abs(newZ - Player.Position.Z); // TODO: Check this

                    if (cmp > 0 || (cmp == 0 && ourZ > newZ))
                    {
                        continue;
                    }
                }

                if (ourTop > testTop)
                {
                    testTop = ourTop;
                }

                if (!itemData.Bridge)
                {
                    itemTop += itemData.Height;
                }

                if (stepTop < itemTop)
                {
                    continue;
                }

                var landCheck = itemZ;

                if (itemData.Height >= StepHeight)
                {
                    landCheck += StepHeight;
                }
                else
                {
                    landCheck += itemData.Height;
                }

                if (considerLand && landCheck < landCenter && landCenter > ourZ && testTop > landZ)
                {
                    continue;
                }

                if (!IsOk(ignoreDoors, ignoreSpellFields, ourZ, testTop, tiles, items))
                {
                    continue;
                }

                newZ = ourZ;
                moveIsOk = true;
            }
            #endregion

            #region Items

            foreach (var item in items)
            {
                itemData = TileData.ItemTable[item.ItemID & (TileData.ItemTable.Length - 1)];
                flags = itemData.Flags;

                if (item.Movable)
                {
                    continue;
                }

                if ((flags & ImpassableSurface) != TileFlag.Surface)
                {
                    continue;
                }

                itemZ = item.Position.Z;
                itemTop = itemZ;
                ourZ = itemZ + itemData.CalcHeight;
                ourTop = ourZ + PersonHeight;
                testTop = checkTop;

                if (moveIsOk)
                {
                    var cmp = Math.Abs(ourZ - Player.Position.Z) - Math.Abs(newZ - Player.Position.Z);

                    if (cmp > 0 || (cmp == 0 && ourZ > newZ))
                    {
                        continue;
                    }
                }

                if (ourTop > testTop)
                {
                    testTop = ourTop;
                }

                if (!itemData.Bridge)
                {
                    itemTop += itemData.Height;
                }

                if (stepTop < itemTop)
                {
                    continue;
                }

                var landCheck = itemZ;

                if (itemData.Height >= StepHeight)
                {
                    landCheck += StepHeight;
                }
                else
                {
                    landCheck += itemData.Height;
                }

                if (considerLand && landCheck < landCenter && landCenter > ourZ && testTop > landZ)
                {
                    continue;
                }

                if (!IsOk(ignoreDoors, ignoreSpellFields, ourZ, testTop, tiles, items))
                {
                    continue;
                }

                newZ = ourZ;
                moveIsOk = true;
            }

            #endregion

            if (!considerLand || landBlocks ) // Felix || stepTop < landZ)
            {
                return moveIsOk;
            }

            ourZ = landCenter;
            ourTop = ourZ + PersonHeight;
            testTop = checkTop;

            if (ourTop > testTop)
            {
                testTop = ourTop;
            }

            var shouldCheck = true;

            if (moveIsOk)
            {
                var cmp = Math.Abs(ourZ - Player.Position.Z) - Math.Abs(newZ - Player.Position.Z);

                if (cmp > 0 || (cmp == 0 && ourZ > newZ))
                {
                    shouldCheck = false;
                }
            }

            if (!shouldCheck || !IsOk(ignoreDoors, ignoreSpellFields, ourZ, testTop, tiles, items))
            {
                return moveIsOk;
            }

            newZ = ourZ;
            return true;
        }

        private static bool IsOk(Statics.TileInfo tile, int ourZ, int ourTop)
        {
            var itemData = TileData.ItemTable[tile.ID & (TileData.ItemTable.Length - 1)];

            return tile.Z + itemData.CalcHeight <= ourZ || ourTop <= tile.Z || (itemData.Flags & ImpassableSurface) == 0;
        }

        // This is nasty, seems like could be optimized .. Credzba
        private static bool IsOk(bool ignoreDoors, bool ignoreSpellFields, int ourZ, int ourTop, List<Statics.TileInfo> tiles, IEnumerable<Assistant.Item> items)
        {
            bool result = tiles.All(t => IsOk(t, ourZ, ourTop)) && items.All(i => IsOk(i, ourZ, ourTop, ignoreDoors, ignoreSpellFields));
            return result;
            /*
            foreach (var tile in tiles)
            {
                bool result = IsOk(tile, ourZ, ourTop);
                if (result == false)
                    return false;
            }
            foreach (var item in items)
            {
                bool result = IsOk(item, ourZ, ourTop, ignoreDoors, ignoreSpellFields);
                if (result == false)
                    return false;
            }
            return true;
            */
        }

        private static bool IsOk(Assistant.Item item, int ourZ, int ourTop, bool ignoreDoors, bool ignoreSpellFields)
        {
            var itemID = item.ItemID & (TileData.ItemTable.Length - 1);
            var itemData = TileData.ItemTable[itemID];

            if ((itemData.Flags & ImpassableSurface) == 0)
            {
                return true;
            }

            if (((itemData.Flags & TileFlag.Door) != 0 || itemID == 0x692 || itemID == 0x846 || itemID == 0x873 ||
                 (itemID >= 0x6F5 && itemID <= 0x6F6)) && ignoreDoors)
            {
                return true;
            }

            if ((itemID == 0x82 || itemID == 0x3946 || itemID == 0x3956) && ignoreSpellFields)
            {
                return true;
            }

            return item.Position.Z + itemData.CalcHeight <= ourZ || ourTop <= item.Position.Z;
        }

        private static void GetStartZ(Point3D loc, Map map, IEnumerable<Assistant.Item> itemList, out int zLow, out int zTop)
        {
            int xCheck = loc.X, yCheck = loc.Y;
            Ultima.Tile landTile = (Ultima.Tile)map.Tiles.GetLandTile(xCheck, yCheck);
            var landData = TileData.LandTable[landTile.ID & (TileData.LandTable.Length - 1)];
            var landBlocks = (landData.Flags & TileFlag.Impassable) != 0;


            int landZ = 0, landCenter = 0, landTop = 0;

            GetAverageZ(map, xCheck, yCheck, ref landZ, ref landCenter, ref landTop);

            var considerLand = !landTile.Ignored();

            var zCenter = zLow = zTop = 0;
            var isSet = false;

            if (considerLand && !landBlocks && loc.Z >= landCenter)
            {
                zLow = landZ;
                zCenter = landCenter;
                zTop = landTop;
                isSet = true;
            }

            var staticTiles = map.Tiles.GetStaticTiles(xCheck, yCheck, true);

            foreach (var tile in staticTiles)
            {
                var tileData = TileData.ItemTable[tile.ID & (TileData.ItemTable.Length - 1)];
                var calcTop = (tile.Z + tileData.CalcHeight);

                if (isSet && calcTop < zCenter)
                {
                    continue;
                }

                if ((tileData.Flags & TileFlag.Surface) == 0)
                {
                    continue;
                }

                if (loc.Z < calcTop)
                {
                    continue;
                }

                zLow = tile.Z;
                zCenter = calcTop;

                var top = tile.Z + tileData.Height;

                if (!isSet || top > zTop)
                {
                    zTop = top;
                }

                isSet = true;
            }

            foreach (var item in itemList)
            {
                var itemData = TileData.ItemTable[item.ItemID & (TileData.ItemTable.Length - 1)];

                var calcTop = item.Position.Z + itemData.CalcHeight;

                if (isSet && calcTop < zCenter)
                {
                    continue;
                }

                if ((itemData.Flags & TileFlag.Surface) == 0)
                {
                    continue;
                }

                if (loc.Z < calcTop)
                {
                    continue;
                }

                zLow = item.Position.Z;
                zCenter = calcTop;

                var top = item.Position.Z + itemData.Height;

                if (!isSet || top > zTop)
                {
                    zTop = top;
                }

                isSet = true;
            }

            if (!isSet)
            {
                zLow = zTop = loc.Z;
            }
            else if (loc.Z > zTop)
            {
                zTop = loc.Z;
            }
        }

        private static void GetAverageZ(Map map, int x, int y, ref int z, ref int avg, ref int top)
        {
            var zTop = map.Tiles.GetLandTile(x, y).Z;
            var zLeft = map.Tiles.GetLandTile(x, y + 1).Z;
            var zRight = map.Tiles.GetLandTile(x + 1, y).Z;
            var zBottom = map.Tiles.GetLandTile(x + 1, y + 1).Z;

            z = zTop;
            if (zLeft < z)
                z = zLeft;
            if (zRight < z)
                z = zRight;
            if (zBottom < z)
                z = zBottom;

            top = zTop;
            if (zLeft > top)
                top = zLeft;
            if (zRight > top)
                top = zRight;
            if (zBottom > top)
                top = zBottom;

            if (Math.Abs(zTop - zBottom) > Math.Abs(zLeft - zRight))
                avg = FloorAverage(zLeft, zRight);
            else
                avg = FloorAverage(zTop, zBottom);
        }

        private static int FloorAverage(int a, int b)
        {
            var v = a + b;

            if (v < 0)
                --v;

            return (v / 2);
        }

        // Check the tiles that are next to, above, below, or diagonal to
        // this tile, and return them if they're within the game bounds and passable
        public IEnumerable<Tile_My> Neighbors(Point3D id)
        {
            foreach (var dir in Dirs)
            {
                var next = new Tile_My(id.X + dir.X, id.Y + dir.Y);
                if (InBounds(next))
                {
                    yield return next;
                }
            }
        }
    }

    internal class AStarSearch_My
    {
        public Dictionary<string, Tile_My> CameFrom = new Dictionary<string, Tile_My>();
        public Dictionary<string, int> CostSoFar = new Dictionary<string, int>();

        private readonly Tile_My _start;
        private readonly Tile_My _goal;

        public static int Heuristic(Tile_My a, Tile_My b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        // Conduct the A* search
        public AStarSearch_My(SquareGrid_My graph, Tile_My start, Tile_My goal, int startZ, bool ignoremob, PathFinding_My.Route.AdditionalProperties add_prop)
        {
            // start is current sprite Location
            _start = start;
            // goal is sprite destination eg tile user clicked on
            _goal = goal;

            var frontier = new PriorityQueue_My<Point3D>();
            // Add the starting location to the frontier with a priority of 0
            frontier.Enqueue(new Point3D(start.X, start.Y, startZ), 0);

            CameFrom.Add(start.ToString(), start); // is set to start, None in example
            CostSoFar.Add(start.ToString(), 0);

            Map map = null;
            switch (Player.Map)
            {
                case 0:
                    map = Ultima.Map.Felucca;
                    break;
                case 1:
                    map = Ultima.Map.Trammel;
                    break;
                case 2:
                    map = Ultima.Map.Ilshenar;
                    break;
                case 3:
                    map = Ultima.Map.Malas;
                    break;
                case 4:
                    map = Ultima.Map.Tokuno;
                    break;
                case 5:
                    map = Ultima.Map.TerMur;
                    break;
                default:
                    break;
            }

            if (map != null)
            {
                while (frontier.Count > 0)
                {
                    // Get the Location from the frontier that has the lowest
                    // priority, then remove that Location from the frontier
                    var current = frontier.Dequeue();

                    // If we're at the goal Location, stop looking.
                    if (current.X == goal.X && current.Y == goal.Y)
                    {
                        break;
                    }

                    // Neighbors will return a List of valid tile Locations
                    // that are next to, diagonal to, above or below current
                    List<Assistant.Item> items = World.Items.Values.Where(x => x.OnGround).ToList();
                    foreach (var neighbor in graph.Neighbors(current))
                    {
                        string neigh = neighbor.ToString();
                        int cost = graph.Cost(items, current, map, neighbor, ignoremob, add_prop, out var neighborZ);
                        var newCost = CostSoFar[new Tile(current.X, current.Y).ToString()] + cost;
                        if (newCost < SquareGrid_My.BigCost)
                        {                            
                            if (!CostSoFar.ContainsKey(neigh) || newCost < CostSoFar[neigh])
                            {
                                // If we're replacing the previous cost, remove it
                                if (CostSoFar.ContainsKey(neigh))
                                {
                                    CostSoFar.Remove(neigh);
                                    CameFrom.Remove(neigh);
                                }

                                CostSoFar.Add(neigh, newCost);
                                CameFrom.Add(neigh, new Tile_My(current.X, current.Y));
                                var priority = newCost + Heuristic(neighbor, goal);
                                var neighborTile = new Point3D(neighbor.X, neighbor.Y, neighborZ);
                                frontier.Enqueue(neighborTile, priority);
                            }
                        }
                    }
                }
            }
        }

        // Return a List of Locations representing the found path
        public List<Tile_My> FindPath()
        {
            var path = new List<Tile_My>();
            var current = _goal;
            path.Add(current);

            while (!current.Equals(_start))
            {
                if (!CameFrom.ContainsKey(current.ToString()))
                {
                    return null;
                }
                path.Add(current);
                current = CameFrom[current.ToString()];
            }
            // path.Add(start);
            path.Reverse();
            return path;
        }
    }

    internal class PriorityQueue_My<T>
    {
        // From Red Blob: I'm using an unsorted array for this example, but ideally this
        // would be a binary heap. Find a binary heap class:
        // * https://bitbucket.org/BlueRaja/high-speed-priority-queue-for-c/wiki/Home
        // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
        // * http://xfleury.github.io/graphsearch.html
        // * http://stackoverflow.com/questions/102398/priority-queue-in-net

        private readonly List<KeyValuePair<T, float>> _elements = new List<KeyValuePair<T, float>>();

        public int Count => _elements.Count;

        public void Enqueue(T item, float priority)
        {
            _elements.Add(new KeyValuePair<T, float>(item, priority));
        }

        // Returns the Location that has the lowest priority
        public T Dequeue()
        {
            var bestIndex = 0;

            for (var i = 0; i < _elements.Count; i++)
            {
                if (_elements[i].Value < _elements[bestIndex].Value)
                {
                    bestIndex = i;
                }
            }

            var bestItem = _elements[bestIndex].Key;
            _elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }

    /// <summary>
    /// This class implements the PathFinding algorithm using A-Star. 
    /// </summary>
    public class PathFinding_My
    {
        public PathFinding_My()
        {
            m_Path = new List<Tile_My>();
        }

        List<Tile_My> m_Path;

        PathFinding_My(List<Tile_My> path)
        {
            m_Path = path;
        }

        /// <summary>
        /// The Route class is used to configure the PathFinding_My.
        /// </summary>
        public class Route
        {
            /// <summary>
            /// Create an empty Route object.
            /// </summary>
            public Route() { }

            /// <summary>Sets the destination position X. (default: 0)</summary>
            public int X = 0;

            /// <summary>Sets the destination position Y. (default: 0)</summary>
            public int Y = 0;

            /// <summary>Outputs a debug message. (default: False)</summary>
            public bool DebugMessage = false;

            /// <summary>Halts the pathfinding fail to walk the path. (default: 0)</summary>
            public bool StopIfStuck = false;

            /// <summary>Ignores any mobiles with the path calculation. (default: 0)</summary>
            public bool IgnoreMobile = false;

            /// <summary>ReSyncs the path calculation. (default: False)</summary>
            public bool UseResync = false;

            /// <summary>Number of attempts untill the path calculation is halted. (default: -1, no limit)</summary>
            public int MaxRetry = -1;

            /// <summary>Maximum amount of time to run the path. (default: -1, no limit)</summary>
            public float Timeout = -1;

            /// <summary>Maximum amount of time to run the path. (default: -1, no limit)</summary>
            public bool Run = true;

            public class AdditionalProperties
            {
                public int ScanAdditionalRange = 0; // 用户可自定地图扫描范围扩大多少步。
                public List<Point2D> obstacle_list = new List<Point2D>();
                public List<List<Point2D>> ForceEnablePolygonAreaList = null;  // 用户自定义允许步行通过的区域的列表
                public List<List<Point2D>> ForceDisablePolygonAreaList = null; // 用户自定义禁止步行通过的区域的列表
            }

            public AdditionalProperties AdditProps = new AdditionalProperties ();
        }

        /// <summary>
        /// Go to the given coordinates using Razor pathfinding.
        /// </summary>
        public static void PathFindTo(RazorEnhanced.Point3D destination)
        {
            PathFindTo(destination.X, destination.Y, destination.Z);           
        }        

        /// <summary>
        /// Go to the given coordinates using Razor pathfinding.
        /// </summary>
        /// <param name="x">X map coordinates or Point3D</param>
        /// <param name="y">Y map coordinates</param>
        /// <param name="z">Z map coordinates</param>
        public static void PathFindTo(int x, int y, int z=0)
        {
            Route r = new Route();
            r.X = x; 
            r.Y = y; 
            if (Assistant.Client.IsOSI)
                r.Run = false; // because run on OSI screws with the window focus
            else
                r.Run = true;
            r.UseResync = true;
            Go(r);
            return;
        }

        /// <summary>
        /// Create a Tile starting from X,Y coordinates (see PathFindig.RunPath)
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Returns a Tile object</returns>
        public static Tile Tile(int x, int y){
            return new Tile(x,y);
        }

        /// <summary>
        /// Check if a destination is reachable.
        /// </summary>
        /// <param name="r">A customized Route object.</param>
        /// <returns>True: if a destination is reachable.</returns>
        public static bool Go(Route r)
        {
            try
            {


                if (r.StopIfStuck) { r.MaxRetry = 1; }

                DateTime timeStart, timeEnd;
                timeStart = DateTime.Now;
                timeEnd = (r.Timeout < 0) ? timeStart.AddDays(1) : timeStart.AddSeconds(r.Timeout);

                float timeLeft;
                List<Tile_My> road;
                bool success;
                while (r.MaxRetry == -1 || r.MaxRetry > 0)
                {
                    if (r.X == Player.Position.X && r.Y == Player.Position.Y)
                        return true;
                    road = PathMove_My.GetPath(r.X, r.Y, r.IgnoreMobile, r.AdditProps);
                    if (road == null)
                    {
                        if (r.X == Player.Position.X && r.Y == Player.Position.Y)
                            return true;
                        else
                            return false;
                    }
                    PathFinding_My pf = new PathFinding_My(road);

                    timeLeft = (int)timeEnd.Subtract(DateTime.Now).TotalSeconds;
                    if (r.Run)
                        success = pf.runPath(timeLeft, r.DebugMessage, r.UseResync);
                    else
                        success = pf.walkPath(timeLeft, r.DebugMessage, r.UseResync);
                    if (r.MaxRetry > 0) { r.MaxRetry -= 1; }
                    if (success) { return true; }
                    if (DateTime.Now.CompareTo(timeEnd) > 0) { return false; }
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return false;

        }

        /// <summary>
        /// Compute the path for the given destination and returns a list of Tile (coordinates).
        /// </summary>
        /// <param name="dst_x">Destination X.</param>
        /// <param name="dst_y">Destination Y.</param>
        /// <param name="ignoremob">Ignores any mobiles with the path calculation.</param>
        /// <returns>List of Tile objects, each holds a .X and .Y coordinates.</returns>
        public static List<Tile_My> GetPath(int dst_x, int dst_y, bool ignoremob, PathFinding_My.Route.AdditionalProperties add_prop) {
            return PathMove_My.GetPath(dst_x, dst_y, ignoremob, add_prop);
        }

        /// <summary>
        /// Run a given path, represented as list of Tile (see PathFindig.GetPath).
        /// </summary>
        /// <param name="path">List of coordinates as Tile objects.</param>
        /// <param name="timeout">Maximum amount of time to run the path. (default: -1, no limit)</param>
        /// <param name="debugMessage">Outputs a debug message.</param>
        /// <param name="useResync">ReSyncs the path calculation.</param>
        /// <returns>True: if it finish the path in time. False: otherwise</returns>
        public static bool RunPath(List<Tile_My> path, float timeout = -1, bool debugMessage = false, bool useResync = true)
        {
            PathFinding_My pf = new PathFinding_My(path);
            return pf.followPath(true, timeout, debugMessage, useResync);
        }

        public static bool WalkPath(List<Tile_My> path, float timeout = -1, bool debugMessage = false, bool useResync = true)
        {
            PathFinding_My pf = new PathFinding_My(path);
            return pf.followPath(false, timeout, debugMessage, useResync);
        }


        /// <param name="path">List of coordinates as Tile objects.</param>
        /// <param name="timeout">Maximum amount of time to run the path. (default: -1, no limit)</param>
        /// <param name="debugMessage">Outputs a debug message.</param>
        /// <param name="useResync">ReSyncs the path calculation.</param>
        /// <returns>True: if it finish the path in time. False: otherwise</returns>
        public bool runPath(float timeout = -1, bool debugMessage = false, bool useResync = true)
        {
            return followPath(true, timeout, debugMessage, useResync);
        }
        public bool walkPath(float timeout = -1, bool debugMessage = false, bool useResync = true)
        {
            return followPath(false, timeout, debugMessage, useResync);
        }

        internal static List<Tile_My> BypassItem(List<Tile_My> path, int i)
        {
            int j = i;
            for (; j < path.Count; j++)
            {
                if (path[j].Conflict == false)
                    break;
            }
            List<Tile_My> bypass = PathMove_My.GetPath(path[j+2].X, path[j+2].Y, false, null);
            return bypass;
        }
        internal static List<Tile_My> BypassHouse(List<Tile_My> path, int start)
        {
            int i = start;
            for (; i < path.Count; i++)
            {
                if (!Statics.CheckDeedHouse(path[i].X, path[i].Y))
                    break;
            }
            // +1 to get a little past the house
            if (i < path.Count-1)
                return PathMove_My.GetPath(path[i + 1].X, path[i + 1].Y, false, null);
            
            // effectively do nothing
            return PathMove_My.GetPath(path[start].X, path[start].Y, false, null);
        }

        internal void PatchPath(PacketReader p, PacketHandlerEventArgs args)
        {
            ushort _unk1 = p.ReadUInt16();
            byte _artDataID = p.ReadByte();
            uint serial = p.ReadUInt32();
            ushort graphic = p.ReadUInt16();
            byte graphic_inc = p.ReadByte();
            ushort _amount = p.ReadUInt16();
            _amount = p.ReadUInt16(); // weird I know

            ushort x = p.ReadUInt16();
            ushort y = p.ReadUInt16();
            short z = p.ReadSByte();

            Assistant.Item item = World.FindItem(serial & 0x7FFFFFFF);
            if (item != null && item.IsMulti)
            {
                Ultima.MultiComponentList multiinfo = Ultima.Multis.GetComponents(item.ItemID);

                /*int xMin = 0;
                int yMin = 0;
                foreach (var m in multiinfo.SortedTiles)
                {
                    xMin = Math.Min(xMin, m.m_OffsetX);
                    yMin = Math.Min(yMin, m.m_OffsetY);
                }
                */

                //RazorEnhanced.Multi.MultiData data = World.Multis[(int)serial]; 
                Rectangle2D area = new Rectangle2D(item.Position.X - multiinfo.Max.X, item.Position.Y - multiinfo.Max.Y,
                                                    multiinfo.Max.X * 2, multiinfo.Max.Y * 2);

                foreach (var tile in m_Path)
                {
                    Assistant.Point2D point = new Assistant.Point2D(tile.X, tile.Y);
                    if (area.Contains(point))
                        {
                        //if (tile.X == x && tile.Y == y)
                        tile.Conflict = true;
                    }
                }
            }

        }

        internal bool followPath(bool run, float timeout, bool debugMessage, bool useResync)
        {
            try
            {
                PacketHandler.RegisterServerToClientViewer(0xF3, new PacketViewerCallback(this.PatchPath));
                
                if (m_Path == null) { return false; }
                DateTime timeStart, timeEnd;
                timeStart = DateTime.Now;
                timeEnd = (timeout < 0) ? timeStart.AddDays(1) : timeStart.AddSeconds(timeout);

                Tile_My dst = m_Path.Last();
                for (int i = 0; i < m_Path.Count; i++)
                {
                    if (Player.Position.X == dst.X && Player.Position.Y == dst.Y)
                    {
                        Misc.SendMessage("PathFind: Destination reached", 66);
                        return true;
                    }

                    bool walkok = false;
                    Tile_My step = m_Path[i];

                    if (step.Conflict)
                    {
                        List<Tile_My> bypass = BypassItem(m_Path, i);
                        if (bypass != null && bypass.Count > 0)
                        {
                            for (; i < m_Path.Count; i++)
                                if (!m_Path[i].Conflict)
                                    break;
                            m_Path.InsertRange(i, bypass);
                        }
                        else
                        {
                        }
                        // insert so the continue will start with the insert
                        continue;
                    }

                    if (Statics.CheckDeedHouse(step.X, step.Y))
                    {
                        List<Tile_My> bypass = BypassHouse(m_Path, i);
                        for (; i < m_Path.Count; i++)
                        {
                            if (!Statics.CheckDeedHouse(m_Path[i].X, m_Path[i].Y))
                                break;
                        }
                        if (bypass != null && bypass.Count > 0)
                        {
                            m_Path.InsertRange(i + 1, bypass);
                            // insert so the continue will start with the insert
                            continue;
                        }
                    }

                    foreach (var item in World.Items)
                    {
                        if (item.Value.Position.X == step.X && item.Value.Position.Y == step.Y)
                        {
                            if (step.Conflict)
                            {
                                List<Tile_My> bypass = BypassItem(m_Path, i);
                                if (bypass != null && bypass.Count > 0)
                                {
                                    for (; i < m_Path.Count; i++)
                                        if (!m_Path[i].Conflict)
                                            break;
                                    m_Path.InsertRange(i-1, bypass);
                                }
                                else 
                                { 
                                }
                                // insert so the continue will start with the insert
                                continue;
                            }
                        }
                    }
                    if (step.X > Player.Position.X && step.Y == Player.Position.Y) //East
                    {
                        Rotate(Direction.east, debugMessage);
                        walkok = Move(Direction.east, run, debugMessage);
                    }
                    else if (step.X < Player.Position.X && step.Y == Player.Position.Y) // West
                    {
                        Rotate(Direction.west, debugMessage);
                        walkok = Move(Direction.west, run, debugMessage);
                    }
                    else if (step.X == Player.Position.X && step.Y < Player.Position.Y) //North
                    {
                        Rotate(Direction.north, debugMessage);
                        walkok = Move(Direction.north, run, debugMessage);
                    }
                    else if (step.X == Player.Position.X && step.Y > Player.Position.Y) //South
                    {
                        Rotate(Direction.south, debugMessage);
                        walkok = Move(Direction.south, run, debugMessage);
                    }
                    else if (step.X > Player.Position.X && step.Y > Player.Position.Y) //Down
                    {
                        Rotate(Direction.down, debugMessage);
                        walkok = Move(Direction.down, run, debugMessage);
                    }
                    else if (step.X < Player.Position.X && step.Y < Player.Position.Y) //UP
                    {
                        Rotate(Direction.up, debugMessage);
                        walkok = Move(Direction.up, run, debugMessage);
                    }
                    else if (step.X > Player.Position.X && step.Y < Player.Position.Y) //Right
                    {
                        Rotate(Direction.right, debugMessage);
                        walkok = Move(Direction.right, run, debugMessage);
                    }
                    else if (step.X < Player.Position.X && step.Y > Player.Position.Y) //Left
                    {
                        Rotate(Direction.left, debugMessage);
                        walkok = Move(Direction.left, run, debugMessage);
                    }
                    else if (Player.Position.X == step.X && Player.Position.Y == step.Y) // no action
                        walkok = true;

                    Map map = null;
                    switch (Player.Map)
                    {
                        case 0:
                            map = Ultima.Map.Felucca;
                            break;
                        case 1:
                            map = Ultima.Map.Trammel;
                            break;
                        case 2:
                            map = Ultima.Map.Ilshenar;
                            break;
                        case 3:
                            map = Ultima.Map.Malas;
                            break;
                        case 4:
                            map = Ultima.Map.Tokuno;
                            break;
                        case 5:
                            map = Ultima.Map.TerMur;
                            break;
                        default:
                            break;
                    }
                    if (map != null)
                    {
                        var zTop = map.Tiles.GetLandTile(step.X, step.Y).Z;
                        if ( Math.Abs(Player.Position.Z - zTop) > 2)
                            RazorEnhanced.Misc.Resync();
                    }

                    if (timeout >= 0 && DateTime.Now.CompareTo(timeEnd) > 0)
                    {
                        if (debugMessage)
                            Misc.SendMessage("PathFind: RunPath run TIMEOUT", 33);
                        return false;
                    }

                    if (!walkok)
                    {
                        if (debugMessage)
                            Misc.SendMessage("PathFind: Move action FAIL", 33);

                        if (useResync)
                        {
                            Misc.Resync();
                            Misc.Pause(200);
                            // If position gets off, try to fix it
                            if (Misc.Distance(Player.Position.X, Player.Position.Y, step.X, step.Y) > 2)
                            {
                                Route fixit = new Route();
                                fixit.X = step.X;
                                fixit.Y = step.Y;
                                Go(fixit);
                            }
                        }

                        //return false;
                    }
                    else
                    {
                        if (debugMessage)
                            Misc.SendMessage("PathFind: Move action OK", 66);
                    }
                }

                if (Player.Position.X == dst.X && Player.Position.Y == dst.Y)
                {
                    Misc.SendMessage("PathFind: Destination reached", 66);
                    Misc.Resync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                PacketHandler.RemoveServerToClientViewer(0xF3, new PacketViewerCallback(this.PatchPath));
            }
        }

        private static void Rotate(Direction d, bool debug)
        {
            if ((World.Player.Direction & Direction.mask) != d)
            {
                Player.Walk(d.ToString());

                if (debug)
                    Misc.SendMessage("PathFind: Rotate in direction: " + d.ToString(), 55);
            }
        }

        private static bool Move(Direction d, bool run, bool debug)
        {
            if (debug)
                Misc.SendMessage("PathFind: Move to direction: " + d.ToString(), 55);

            bool result = false;
            int retry = 5;
            for (int i = 0; i < 5; i++)
            {
                if (run)
                    result = Player.Run(d.ToString());
                else
                    result = Player.Walk(d.ToString());
                if (result == true)
                    break;
                Misc.Pause(150);
            }
            return result;
        }
    }
}


