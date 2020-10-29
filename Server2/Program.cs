using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ClassLibrary;

namespace ServerTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerAsync serverA = new ServerAsync(IPAddress.Parse("127.0.0.1"), 8080);
            try
            {
                serverA.Start();
            }
            catch (System.IO.IOException e) { }
        }
    }
}
