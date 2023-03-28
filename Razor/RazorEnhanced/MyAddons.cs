using System;
using System.Collections.Generic;
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

        // ��������ʱ�䣬��λ��Ȼ�Ǻ���
        public long ElapsedTime()
        {
            return DateTime.Now.Ticks / 10000 - m_startMilliseconds;
        }

        // ��ʣ����ʱ�䣬��λ��Ȼ�Ǻ���
        public long RemainTimers()
        {
            return m_timer_out_milliseconds - ElapsedTime();
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
        // ���ص�ǰ��¼��������Ʒ����
        public static int Count()
        {
            return World.Items.Count;
            //return World.Mobiles.Count;
        }
    }

    public partial class Mobiles
    {

        // ���ص�ǰ��¼��������������
        public static int Count()
        {
            return World.Mobiles.Count;
        }
    }


    public partial class Player
    {
        // ������ҵ�ǰ��ʿʣ��ķ��׵���
        public static int Tithe { get { return World.Player.Tithe; } }

        public int DistanceTo(int x, int y)
        {
            return Utility.Distance(Position.X, Position.Y, x, y);
        }

        public int DistanceTo(Point3D pt)
        {
            return Utility.Distance(Position.X, Position.Y, pt.X, pt.Y);
        }

        // The code I added begin
        // ���ص�ǰ��¼UO���ʺ���
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
        // ���ű�ʹ��
        //public static List<AutoLootItem> GetAutolootItemList(string autoloot_list_name)
        public static Dictionary<int, List<AutoLoot.AutoLootItem>> GetAutolootItemList(string autoloot_list_name)
        {
            return Settings.AutoLoot.ItemsRead(autoloot_list_name);
        }

        // public static List<AutoLootItem> GetCurrentAutolootItemList()
        public static Dictionary<int, List<AutoLoot.AutoLootItem>> GetCurrentAutolootItemList()
        {
            return GetAutolootItemList(ListName);
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

        // ���ű�ʹ�ã������µ��б�
        public static bool AddNewList(string list_name, List<AutoLootItem> items_list)
        {
            // �Ѿ�����ͬ���б��򷵻�
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

        // ���ָ��ʬ�����к��Ƿ�����AutoLoot��
        public static bool IsCorpseLooting(int corpse_serial)
        {
            foreach (SerialToGrab item in SerialToGrabList)
                if (item.CorpseSerial == corpse_serial)
                    return true;

            return false;
        }

        // ���ָ��ʬ�����к��б��Ƿ�����AutoLoot��
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
        // Assistant.Point2D ��internal�࣬û�취��©������ʹ�ã���Ҳ�����Դ��Ķ�̫�ֻ࣬��ע�͵��������
        //public static Point2D BuildPoint2D (Assistant.Point2D point2D)
        //{
        //    return new Point2D (point2D.X, point2D.Y);
        //}
        public static Point2D BuildPoint2D(RazorEnhanced.Point2D point2D)
        {
            return BuildPoint2D(point2D.X, point2D.Y);
        }


            // ���ص�ǰһ�������˶����ֽڵ�������
        public static uint TotalOutBytes()
        {
            return DLLImport.Razor.TotalOut();
        }
        // ���ص�ǰһ�������˶����ֽڴӷ�����
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

}