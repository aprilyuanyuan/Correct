using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Correct;
using System.Threading;

namespace TstProtocl
{
    class Program
    {
        static void Main(string[] args)
        {
            Device dev = new Device();

            dev.IniPort("COM21", 57600);

            byte[] cmd = new byte[] { };

            if (dev.WriteChannelInformation(cmd, 3000))
            {
                Console.WriteLine("设置成功");

            }
            else
            {
                Console.WriteLine("设置失败");
            }

            AutoResetEvent[] arr=new AutoResetEvent[5];
            switch (AutoResetEvent.WaitAny(arr))
            {
 
            }
         
        }
    }
}
