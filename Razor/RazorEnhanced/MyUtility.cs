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

        // ��������ʱ�䣬��λ��Ȼ�Ǻ���
        public long ElapsedTime ()
        {
            return DateTime.Now.Ticks / 10000 - m_startMilliseconds;
        }

        // ��ʣ����ʱ�䣬��λ��Ȼ�Ǻ���
        public long RemainTimers ()
        {
            return m_timer_out_milliseconds - ElapsedTime();
        }
    }
}