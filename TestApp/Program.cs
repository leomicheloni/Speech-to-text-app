using CognitiveLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new SpeechToTextService("ee304a2438c44724a98a3d45724b61e8");
            service.Start("it works.wav", "");

            Console.WriteLine("Ready....");

            Console.ReadLine();
        }
    }
}
