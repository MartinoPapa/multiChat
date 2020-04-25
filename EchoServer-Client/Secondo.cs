using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* per poter utilizzare questo package è necessario installare il pacchetto NuGet. 
 * vedi: http://docs.nuget.org/docs/start-here/installing-nuget
 * In visual studio 2012 nel menù PROGETTO --> Gestisci pacchetti NuGet 
 *  si trova il package Alchemy. Installarlo e poi usare il namespace Alchemy nel progetto.
 *          OPPURE
 * Copiare la DLL Alchemy nel progetto e inserirla nei Riferimenti; è quindi possibile
 * utilizzare la classe WebSocketServer dopo aver messo le using necessarie
 */
using Alchemy;
using Alchemy.Classes;
using System.Net;
using Newtonsoft.Json;

namespace secondo
{
    class Secondo
    {
        static void Main(string[] args)
        {

            // istanzia un nuovo server - imposta la porta e il range di IP,
            // e i metodi di risposta.

            var aServer = new WebSocketServer(8080, IPAddress.Any);
            aServer.OnReceive = OnReceive;
            aServer.OnSend = OnSend;
            aServer.OnConnect = OnConnect;
            aServer.OnConnected = OnConnected;
            aServer.OnDisconnect = OnDisconnect;
            aServer.TimeOut = new TimeSpan(0, 5, 0);

            //{
            //    OnReceive = OnReceive,
            //    OnSend = OnSend,
            //    OnConnect = OnConnect,
            //    OnConnected = OnConnected,
            //    OnDisconnect = OnDisconnect,
            //    TimeOut = new TimeSpan(0, 5, 0)
            //};

            aServer.Start();
            

            Console.WriteLine("ATTENDO CONNESSIONI oppure 'esci' PER USCIRE");
            string esci;
            
            do
            {
                esci = Console.ReadLine();
            } while (esci != "esci");

            aServer.Stop();

        }

        static void OnReceive(UserContext context)
        {
            Server.GestisciMessaggio(context);                          
        }

        static void OnSend(UserContext context)
        {
                
        }

        static void OnConnect(UserContext context)
        {
            Console.WriteLine("Client connesso");
        }

        static void OnConnected(UserContext context)
        {
            
        }

        static void OnDisconnect(UserContext context)
        {
            Console.WriteLine("Client disconnesso ");
        }

    }
}
