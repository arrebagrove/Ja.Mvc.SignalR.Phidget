using System;
using System.Configuration;
using System.IO;

// http://www.asp.net/signalr/overview/guide-to-the-api
// http://stackoverflow.com/questions/11140164/signalr-console-app-example

namespace Ja.SignalRConsole.PhidgetServer
{
    class Program
    {
        private static Ja.SignalR.PhidgetServer.PhidgetServer phidgetServerSignalRClient;
        static void Main(string[] args)
        {
            string url = ConfigurationManager.AppSettings["url"];
            //var writer = Console.Out;
            var writer = new StreamWriter(ConfigurationManager.AppSettings["PhidgetServerLog"]);
            writer.AutoFlush = true;

            phidgetServerSignalRClient = new Ja.SignalR.PhidgetServer.PhidgetServer(writer, ConfigurationManager.AppSettings["PhidgetModuleType"]);

            phidgetServerSignalRClient.RunAsync(url).Wait();

            Console.ReadKey();
        }

    }

}