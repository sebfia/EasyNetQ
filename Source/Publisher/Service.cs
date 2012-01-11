using System;
using Contracts;
using EasyNetQ;

namespace Publisher
{
    public class Service : IWantToRunAtStartup, IWantTheBus
    {
        public void Start()
        {
            string text;

            do
            {
                Console.Write("Type the message to send: ");
                text = Console.ReadLine();

                try
                {
                    Bus.Publish(new Message {Text = text});
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error sending message!{0}{1}", Environment.NewLine, exception);
                }
            } while (text != "stop");
        }

        public void Stop()
        {
            
        }

        public IBus Bus { get; set; }
    }
}