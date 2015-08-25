using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Correct
{
    public class NetDebugConsole
    {
        static Socket consoleServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static LinkedList<String> messageBuf = new LinkedList<string>();

        static int BufferCount=1000;
        public static int Port = 4445;


    
        static public void WriteLine(String format,params object[] objs)
        {
            String message=null;
            try
            {
                message = String.Format(format, objs);
                message += "\r\n";
                if (messageBuf.Count > BufferCount)
                {
                    LinkedListNode<String> node = messageBuf.First;
                    messageBuf.RemoveFirst();
                    node.Value = message;
                    messageBuf.AddLast(node);

                }
                else
                {
                    messageBuf.AddLast(message);
                }
                
            }
            catch
            {
                return;
            }
           
        }

        static public void WriteLine(String message)
        {
          
            try
            {
                message += "\r\n";
                if (messageBuf.Count > BufferCount)
                {
                    LinkedListNode<String> node = messageBuf.First;
                    messageBuf.RemoveFirst();
                    node.Value = message;
                    messageBuf.AddLast(node);

                }
                else
                {
                    messageBuf.AddLast(message);
                }

            }
            catch
            {
                return;
            }

        }

       
    }
}

