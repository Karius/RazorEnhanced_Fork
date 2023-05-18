using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Assistant;
namespace RazorEnhanced
{

    public class TimerCount
    {
        public long m_startMilliseconds = DateTime.Now.Ticks / 10000;
        public long m_timer_out_milliseconds;

        public TimerCount(long milliseconds)
        {
            m_timer_out_milliseconds = milliseconds;
        }

        public bool isTimeOut()
        {
            if (DateTime.Now.Ticks / 10000 - m_startMilliseconds >= m_timer_out_milliseconds)
                return true;
            return false;
        }

        // 经过多少时间，单位仍然是毫秒
        public long ElapsedTime()
        {
            return DateTime.Now.Ticks / 10000 - m_startMilliseconds;
        }

        // 还剩多少时间，单位仍然是毫秒
        public long RemainTimers()
        {
            return m_timer_out_milliseconds - ElapsedTime();
        }
    }

	public class PolygonArea
	{
        private List<List<Point2D>> m_polygon_area_list = new List<List<Point2D>> ();
        private int m_polygon_area_index = -1;

        public int Polygon_Area_Index
        {
            get { return m_polygon_area_index; }
        }

        public List<List<Point2D>> Polygon_Area_List
        {
            get
            {
                return m_polygon_area_list;
            }
            set
            {
                m_polygon_area_list = value;
            }
        }

        public bool IsInPolygonAreaList(Point2D checkPoint)
        {
            m_polygon_area_index = IsInPolygonList(checkPoint, m_polygon_area_list);
            return m_polygon_area_index >= 0;
        }

        public static List<List<Point2D>> BuildPolygonAreaList ()
        {
            return new List<List<Point2D>>();
        }

        public static List<Point2D> BuildPolygonPointList()
        {
            return new List<Point2D> ();
        }

        public static int IsInPolygonList(Point2D checkPoint, List<List<Point2D>> polygonAreaLists)
        {
            foreach (var polygonArea in polygonAreaLists)
            {
                if (IsInPolygon(checkPoint, polygonArea))
                    return polygonAreaLists.IndexOf (polygonArea);
            }
            return -1;
        }

        public static int IsInPolygonList(int x, int y, List<List<Point2D>> polygonAreaLists)
        {
            foreach (var polygonArea in polygonAreaLists)
            {
                if (IsInPolygon(x, y, polygonArea))
                    return polygonAreaLists.IndexOf(polygonArea);
            }
            return -1;
        }

        /// <summary>
        /// 判断点是否在多边形内.
        /// ----------原理----------
        /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，
        /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。
        /// </summary>
        /// <param name="checkPoint">要判断的点</param>
        /// <param name="polygonPoints">多边形的顶点</param>
        /// <returns></returns>
        public static bool IsInPolygon(int x, int y, List<Point2D> polygonPoints)
        {
            bool inside = false;
            int pointCount = polygonPoints.Count;
            Point2D p1, p2;
            for (int i = 0, j = pointCount - 1; i < pointCount; j = i, i++)//第一个点和最后一个点作为第一条线，之后是第一个点和第二个点作为第二条线，之后是第二个点与第三个点，第三个点与第四个点...
            {
                p1 = polygonPoints[i];
                p2 = polygonPoints[j];
                if (y < p2.Y)
                {//p2在射线之上
                    if (p1.Y <= y)
                    {//p1正好在射线中或者射线下方
                        if ((y - p1.Y) * (p2.X - p1.X) > (x - p1.X) * (p2.Y - p1.Y))//斜率判断,在P1和P2之间且在P1P2右侧
                        {
                            //射线与多边形交点为奇数时则在多边形之内，若为偶数个交点时则在多边形之外。
                            //由于inside初始值为false，即交点数为零。所以当有第一个交点时，则必为奇数，则在内部，此时为inside=(!inside)
                            //所以当有第二个交点时，则必为偶数，则在外部，此时为inside=(!inside)
                            inside = (!inside);
                        }
                    }
                }
                else if (y < p1.Y)
                {
                    //p2正好在射线中或者在射线下方，p1在射线上
                    if ((y - p1.Y) * (p2.X - p1.X) < (x - p1.X) * (p2.Y - p1.Y))//斜率判断,在P1和P2之间且在P1P2右侧
                    {
                        inside = (!inside);
                    }
                }
            }
            return inside;
        }

        public static bool IsInPolygon(Point2D checkPoint, List<Point2D> polygonPoints)
        {
            return IsInPolygon(checkPoint.X, checkPoint.Y, polygonPoints);
        }
        // 原文链接：https://blog.csdn.net/xxdddail/article/details/49093635
    }


    internal class MapQueue
    {
        private static MapQueue m_mapQueue = new MapQueue();
        internal static MapQueue Maps { get { return m_mapQueue; } }

        internal class MapInfo
        {
            private uint serial;

            private int Upper_Left_X, Upper_Left_Y;
            private int Lower_Right_X, Lower_Right_Y;
            private int m_GumpWidth, m_GumpHeight;

            internal MapInfo(uint ser, ushort gump_art,
                             int upper_left_x, int upper_left_y,
                             int lower_right_x, int lower_right_y,
                             int gump_width, int gump_height)
            {
                serial = ser;
                Upper_Left_X = upper_left_x;
                Upper_Left_Y = upper_left_y;

                Lower_Right_X = lower_right_x;
                Lower_Right_Y = lower_right_y;

                m_GumpWidth = gump_width;
                m_GumpHeight = gump_height;
            }


            internal Point2D Upper_Left { get { return new Point2D(Upper_Left_X, Upper_Left_Y); } }
            internal Point2D Lower_Right { get { return new Point2D(Lower_Right_X, Lower_Right_Y); } }
            internal int GumpWidth { get { return m_GumpWidth; } }
            internal int GumpHeight { get { return m_GumpHeight; } }

            public class Pin
            {
                public Pin(byte id, int x, int y)
                {
                    m_id = id;
                    m_x = x;
                    m_y = y;
                }

                private byte m_id;
                private int m_x, m_y;

                internal byte ID
                {
                    get { return m_id; }
                    set { m_id = value; }
                }
                internal int X
                {
                    get { return m_x; }
                    set { m_x = value; }
                }
                internal int Y
                {
                    get { return m_y; }
                    set { m_y = value; }
                }
            }

            private ConcurrentDictionary<byte, Pin> m_pinDict = new ConcurrentDictionary<byte, Pin>();

            internal ConcurrentDictionary<byte, Pin> PinDict
            {
                get { return m_pinDict; }
            }

            internal List<Pin> PinList
            {
                get
                {
                    List<Pin> pin_list = new List<Pin>();
                    foreach (var key_value in PinDict)
                    {
                        pin_list.Add(key_value.Value);
                    }
                    return pin_list;
                }
            }

            internal void HandlePin(byte action, byte id, int x, int y)
            {
                if (action >= 1 && action <= 3) // add pin
                {
                    this.UpdatePin(new Pin(id, x, y));
                }
                else if (action == 4) // remove pin
                {
                    this.UpdatePin(new Pin(id, x, y), true);
                }
                else if (action == 5) // clear
                {
                    m_pinDict.Clear();
                }
            }

            private void UpdatePin(Pin pin, bool remove = false)
            {
                Pin oldPin = null;
                m_pinDict.TryGetValue(pin.ID, out oldPin);

                if (oldPin != null)
                {
                    if (remove)
                    {
                        m_pinDict.TryRemove(oldPin.ID, out oldPin);
                    }
                    else
                    {
                        oldPin.X = pin.X;
                        oldPin.Y = pin.Y;
                    }
                    return;
                }
                else
                {
                    m_pinDict[pin.ID] = pin;
                }
            }

        } // MapInfo end

        private ConcurrentDictionary<uint, MapInfo> m_mapDict = new ConcurrentDictionary<uint, MapInfo>();

        internal ConcurrentDictionary<uint, MapInfo> MapDict
        {
            get { return m_mapDict; }
        }

        internal List<MapInfo> MapList
        {
            get
            {
                List<MapInfo> map_list = new List<MapInfo>();
                foreach (var key_value in MapDict)
                {
                    map_list.Add(key_value.Value);
                }
                return map_list;
            }
        }

        internal MapInfo FindMap(uint ser)
        {
            MapInfo map = null;
            m_mapDict.TryGetValue(ser, out map);

            return map;
        }

        // 处理新藏宝图数据包
        internal void HandleMap(uint serial, ushort gump_art,
                             int upper_left_x, int upper_left_y,
                             int lower_right_x, int lower_right_y,
                             int gump_width, int gump_height)
        {
            MapInfo newMap = new MapInfo(serial, gump_art, upper_left_x, upper_left_y, lower_right_x, lower_right_y, gump_width, gump_height);

            MapInfo oldMap = null;
            m_mapDict.TryGetValue(serial, out oldMap);

            if (oldMap == null)
            {
                m_mapDict[serial] = newMap;
                return;
            }

        }

        // 处理藏宝图的pin数据包，并把相关坐标信息显示出来
        internal void HandleMapPin(uint s, byte action, byte id, int x, int y)
        {
            MapInfo mapInfo = null;
            m_mapDict.TryGetValue(s, out mapInfo);

            if (mapInfo != null)
            {
                mapInfo.HandlePin(action, id, x, y);

                if (action >= 1 && action <= 3)
                {
                    // 解读藏宝图的位置
                    //MapQueue.MapInfo mapInfo = null;
                    //World.Maps.MapDict.TryGetValue(ser, out mapInfo);

                    //if (mapInfo != null)
                    {
                        string cord_line = "";
                        foreach (var key_value in mapInfo.PinDict)
                        {
                            MapQueue.MapInfo.Pin pin = key_value.Value;
                            cord_line += String.Format("[{0}, {1}]:({2},{3}:{4},{5}) / GumpWidth/Height:[{6},{7}] Pin.Loc[{8},{9}]) ",
                                // 这里乘上2是因为，游戏中展示的地图是个缩略图，pin.X和.Y是pin在缩略图中距离缩略图左上角像素的偏移点
                                // 因为是缩略图，所以直接加上pin.X .Y最终坐标不对，所以乘以2之后缩略图的像素坐标就基本吻合实际坐标了
                                // 虽然还有些差距，但总的是差不多了
                                mapInfo.Upper_Left.X + pin.X * 2, mapInfo.Upper_Left.Y + pin.Y * 2,
                                                        mapInfo.Upper_Left.X, mapInfo.Upper_Left.Y,
                                                        mapInfo.Lower_Right.X, mapInfo.Lower_Right.Y,
                                                        mapInfo.GumpWidth, mapInfo.GumpHeight,
                                                        pin.X, pin.Y
                                                        );
                        }
                        // 显示藏宝图的信息
                        World.Player.SendMessage(cord_line);
                    }

                }
            }
        }
    }

    public class TreasureMap
	{
		public class MapInfo
        {
			private int m_upper_left_x;
			private int m_upper_left_y;
			private int m_lower_right_x;
			private int m_lower_right_y;

			private int m_gump_width;
			private int m_gump_height;

			private List<Pin> m_pinList = new List<Pin>();

			public Point2D UpperLeft
            {
				get { return new Point2D(m_upper_left_x, m_upper_left_y); }
            }
			public Point2D LowerRight
			{
				get { return new Point2D(m_lower_right_x, m_lower_right_y); }
			}
			public int GumpWidth
            {
				get { return m_gump_width; }
            }
			public int GumpHeight
            {
				get { return m_gump_height; }
            }
			public List<Pin> PinList
			{ get { return m_pinList; } }

			public MapInfo (int upper_left_x, int upper_left_y, int lower_right_x, int lower_right_y, int gump_width, int gump_height)
            {
				m_upper_left_x = upper_left_x;
				m_upper_left_y = upper_left_y;

				m_lower_right_x = lower_right_x;
				m_lower_right_y = lower_right_y;

				m_gump_width = gump_width;
				m_gump_height = gump_height;
            }

			public class Pin
            {
				private int m_x, m_y;
				private int m_base_x, m_base_y;

				public int X { get { return m_x; } }
				public int Y { get { return m_y; } }

				public Point2D RealPos
                {
					get { return new Point2D(m_base_x + m_x*2, m_base_y + m_y*2); }
                }

				public Pin (int x, int y)
                {
					m_x = x;
					m_y = y;
                }

				public Pin SetBasePos (int x, int y)
                {
					m_base_x = x;
					m_base_y = y;
					return this;
                }
            }

			internal void AddPin (int x, int y)
            {
				m_pinList.Add (new Pin(x, y).SetBasePos (UpperLeft.X, UpperLeft.Y));
            }
        }

		internal static MapInfo FromMapInfo (MapQueue.MapInfo mapInfo)
        {
			if (mapInfo == null)
				return null;

			MapInfo map = new MapInfo(mapInfo.Upper_Left.X, mapInfo.Upper_Left.Y,
								mapInfo.Lower_Right.X, mapInfo.Lower_Right.Y,
								mapInfo.GumpWidth, mapInfo.GumpHeight);

			foreach (var key_value in mapInfo.PinDict)
			{
				map.AddPin(key_value.Value.X, key_value.Value.Y);
			}
			return map;
		}

		public static MapInfo FindMap (uint serial)
		{
            return FromMapInfo (MapQueue.Maps.FindMap(serial));
		}

		public static List<MapInfo> MapList ()
		{
			List<MapInfo> map_list = new List<MapInfo>();

			foreach (var map_key_value in MapQueue.Maps.MapDict)
            {
				map_list.Add(FromMapInfo(map_key_value.Value));
            }

			return map_list;
		}

		public static string What ()
        {
			return "WTF";
        }
    }


    public partial class Mobile : EnhancedEntity
    {
        public int DistanceTo(int x, int y)
        {
            return Utility.Distance(Position.X, Position.Y, x, y);
        }

        public int DistanceTo(Point3D pt)
        {
            return Utility.Distance(Position.X, Position.Y, pt.X, pt.Y);
        }

    }

    public partial class Item : EnhancedEntity
    {
        public int DistanceTo(int x, int y)
        {
            return Utility.Distance(Position.X, Position.Y, x, y);
        }

        public int DistanceTo(Point3D pt)
        {
            return Utility.Distance(Position.X, Position.Y, pt.X, pt.Y);
        }
    }

    public partial class Items
    { 
        // 返回当前记录的所有物品数量
        public static int Count()
        {
            return World.Items.Count;
            //return World.Mobiles.Count;
        }

        public static bool IsExists(int serial)
        {
            return Assistant.World.FindItem((Assistant.Serial)((uint)serial)) != null;
        }
    }

    public partial class Mobiles
    {

        // 返回当前记录的所有生物数量
        public static int Count()
        {
            return World.Mobiles.Count;
        }
        // 我加入的，通过序列号检测怪物是否还存在
        // 这种方式速度快，不用建立新的怪物对象
        public static bool IsExists(int serial)
        {
            return Assistant.World.Mobiles.ContainsKey((Assistant.Serial)((uint)serial));
        }
    }


    public partial class Player
    {
        // 返回玩家当前骑士剩余的奉献点数
        public static int Tithe { get { return World.Player.Tithe; } }

        public static int DistanceTo(int x, int y)
        {
            return Utility.Distance(Position.X, Position.Y, x, y);
        }

        public static int DistanceTo(Point3D pt)
        {
            return Utility.Distance(Position.X, Position.Y, pt.X, pt.Y);
        }

        // The code I added begin
        // 返回当前登录UO的帐号名
        public static string AccountName()
        {
            return World.AccountName;
        }

        public static string ShardName()
        {
            return World.ShardName;
        }
        // The code I added end
    }

    public partial class AutoLoot

    {
        // 供脚本使用
        //public static List<AutoLootItem> GetAutolootItemList(string autoloot_list_name)
        public static Dictionary<int, List<AutoLoot.AutoLootItem>> GetAutolootItemDict(string autoloot_list_name)
        {
            return Settings.AutoLoot.ItemsRead(autoloot_list_name);
        }
        public static List<AutoLoot.AutoLootItem> GetAutolootItemList(string autoloot_list_name)
        {
            List<AutoLoot.AutoLootItem> item_list = new List<AutoLoot.AutoLootItem>();

            Dictionary<int, List<AutoLoot.AutoLootItem>> items = GetAutolootItemDict (autoloot_list_name);
            foreach (KeyValuePair<int, List<AutoLoot.AutoLootItem>> entry in items)
            {
                foreach (AutoLootItem item in entry.Value)
                {
                    AutoLootItem new_item = new AutoLootItem(item.Name, item.Graphics, item.Color, item.Selected, item.LootBagOverride, item.Properties);
                    item_list.Add(new_item);
                }
            }
            return item_list;
                
        }
        // public static List<AutoLootItem> GetCurrentAutolootItemDict()
        public static Dictionary<int, List<AutoLoot.AutoLootItem>> GetCurrentAutolootItemDict()
        {
            return GetAutolootItemDict(ListName);
        }
        // public static List<AutoLootItem> GetCurrentAutolootItemList()
        public static List<AutoLoot.AutoLootItem> GetCurrentAutolootItemList()
        {
            return GetAutolootItemList(ListName);
        }

        public static string GetListName
        {
            get { return ListName; }
        }

        public static int GetCurrentMaxrange()
        {
            return MaxRange;
        }

        public static int GetCurrentDelay()
        {
            return AutoLootDelay;
        }

        public static int GetCurrentBag()
        {
            return AutoLootBag;
        }

        // 供脚本使用，增加新的列表
        public static bool AddNewList(string list_name, List<AutoLootItem> items_list)
        {
            // 已经存在同名列表则返回
            if (RazorEnhanced.Settings.AutoLoot.ListExists(list_name))
                return false;

            RazorEnhanced.Settings.AutoLoot.ListInsert(list_name, RazorEnhanced.AutoLoot.AutoLootDelay, RazorEnhanced.AutoLoot.AutoLootBag, RazorEnhanced.AutoLoot.NoOpenCorpse, RazorEnhanced.AutoLoot.MaxRange);

            foreach (AutoLootItem item in items_list)
            {
                //Settings.AutoLoot.ItemInsert(list_name, new AutoLootItem(item.Name, item.Graphics, item.Color, item.Selected, new List<AutoLootItem.Property>()));
                Settings.AutoLoot.ItemInsert(list_name, item);

                /*if (row.Cells[4].Value != null)
					Settings.AutoLoot.ItemInsert(list_name, new AutoLootItem((string)row.Cells[1].Value, Convert.ToInt32((string)row.Cells[2].Value, 16), color, check, (List<AutoLootItem.Property>)row.Cells[4].Value));
				  else
					Settings.AutoLoot.ItemInsert(list_name, new AutoLootItem((string)row.Cells[1].Value, Convert.ToInt32((string)row.Cells[2].Value, 16), color, check, new List<AutoLootItem.Property>()));
				*/
            }

            Settings.Save(); // Salvo dati

            RazorEnhanced.AutoLoot.RefreshLists();
            RazorEnhanced.AutoLoot.InitGrid();

            return true;
        }

        // 检测指定尸体序列号是否正在AutoLoot中
        public static bool IsCorpseLooting(int corpse_serial)
        {
            foreach (SerialToGrab item in SerialToGrabList)
                if (item.CorpseSerial == corpse_serial)
                    return true;

            return false;
        }

        // 检测指定尸体序列号列表是否正在AutoLoot中
        public static bool IsCorpseLooting(List<int> corpse_serial_list)
        {
            foreach (SerialToGrab item in SerialToGrabList)
                if (corpse_serial_list.Contains(item.CorpseSerial))
                    return true;

            return false;
        }

    }

    public partial class Misc
    {
        // Timer
        public static TimerCount BuildTimer(long milliseconds)
        {
            return new TimerCount(milliseconds);
        }

        // Point3D
        public static Point3D BuildPoint3D(int x, int y, int z)
        {
            return new Point3D(x, y, z);
        }

        public static Point3D BuildPoint3D(Assistant.Point3D point3D)
        {
            return new Point3D(point3D);
        }
        public static Point3D BuildPoint3D(RazorEnhanced.Point3D point3D)
        {
            return new Point3D(point3D.X, point3D.Y, point3D.Z);
        }

        // Point2D
        public static Point2D BuildPoint2D(int x, int y)
        {
            return new Point2D(x, y);
        }
        // Assistant.Point2D 是internal类，没办法暴漏给外面使用，我也不想对源码改动太多，只能注释掉这个方法
        //public static Point2D BuildPoint2D (Assistant.Point2D point2D)
        //{
        //    return new Point2D (point2D.X, point2D.Y);
        //}
        public static Point2D BuildPoint2D(RazorEnhanced.Point2D point2D)
        {
            return BuildPoint2D(point2D.X, point2D.Y);
        }


            // 返回当前一共发送了多少字节到服务器
        public static uint TotalOutBytes()
        {
            return DLLImport.Razor.TotalOut();
        }
        // 返回当前一共接收了多少字节从服务器
        public static uint TotalInBytes()
        {
            return DLLImport.Razor.TotalIn();
        }

        public static void KeyPress(int key)
        {
            //System.Windows.Forms.Keys key = 
            DLLImport.Win.PostMessage(DLLImport.Razor.FindUOWindow(), 0x100, (System.Windows.Forms.Keys)key, 0);
        }
    }

    public partial class Target
    {
        public static void SetTargetShow(bool noshow)
        {
            Assistant.Targeting.NoShowTarget = noshow;
        }
    }

}