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

        public TimerCount (long milliseconds)
        {
            m_timer_out_milliseconds = milliseconds;
        }

        public bool isTimeOut ()
        {
            if (DateTime.Now.Ticks / 10000 - m_startMilliseconds >= m_timer_out_milliseconds)
                return true;
            return false;
        }

        // 经过多少时间，单位仍然是毫秒
        public long ElapsedTime ()
        {
            return DateTime.Now.Ticks / 10000 - m_startMilliseconds;
        }

        // 还剩多少时间，单位仍然是毫秒
        public long RemainTimers ()
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
}