using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace ServoMonitoring_with_Control
{
   public  class ArduinoConnection              // Class with tools for communication with Arduino
    {
        private SerialPort Port;                // Serial Port for communication
        private Boolean ReadStatus;             // Resume or Pause reading data
        private Thread ReadThread;              //  Reading data from device on another thread
        private String SecretCode;

        Form Owner;                             // For modyfying labels and bars in window

        //Constructior:
        public ArduinoConnection(String code, Form frm)
        {
            Port = new SerialPort();        
            Port.BaudRate = 9600;           // BuadRate in transmission
            int timeout = Port.ReadTimeout;
            Port.ReadTimeout = 30;
            SecretCode = code + '\n';
            string[] ports = SerialPort.GetPortNames();
            
            foreach (string port1 in ports)
            {
                bool cnt = false;
                bool br = false;
                //Console.WriteLine("I am in" + port1);
                try
                {
                    Port = new SerialPort();
                    Port.BaudRate = 9600;           // BuadRate in transmission
                    Port.PortName = port1;        // "COM__"
                    Port.ReadTimeout = 1000;
                    Port.Open();                    // Opening Port             TODO:     refactor is required !!!!!!!  
                    Console.WriteLine("Opening port " + Port.PortName);
                    for (int i = 0; i < 2; i++)
                    {
                        code = Port.ReadLine();
                        Console.WriteLine(code);
                        if (code != SecretCode)
                        {
                            Console.WriteLine("Read from port");
                            cnt = true;
                            continue;
                        }
                        else
                        {
                            Port.WriteLine(SecretCode);
                            br = true;
                            break;
                        }
                    }
                    if (cnt) continue;
                    if (br) break;
                } catch (Exception)
                {
                    Console.WriteLine("Couldn't open port " + Port.PortName);
                    Port.Close();
                    continue;
                }
            }
            ReadStatus = false;             // Stop reading data on start
            ReadThread = new Thread(new ThreadStart(this.CreateReadThread));        //Initialize the tread
            ReadThread.IsBackground = true;                     // Thread is closing with closing form
            Port.ReadTimeout = timeout;
            Owner = (Form1) frm;            // set window for showing captured data
        }

        private void CreateReadThread ()            // Thread to capturing darta
        {
            while (true)
            while (ReadStatus)
            {
                String data = Port.ReadLine();          // write received line to var
                    if(data.Contains("D1:"))            // when line is beginig by "D1" it concerns servo1
                    {
                        ((Form1)Owner).SetDiffVal1(data.Substring(3));          // show overload value in form without  "D1:"
                    } else
                    if (data.Contains("D2:"))           // when line is beginig by "D2" it concerns servo2
                    {
                        ((Form1)Owner).SetDiffVal2(data.Substring(3));           // show overload value in form  without  "D2:"
                    }
                    Thread.Sleep(10);
            }
             
        }

        

        public void SetServo(int num, int pos)              // method for set servo in arduino, type in Numer of servo and position
        {
            Port.WriteLine("S" + num.ToString() + ":" + pos.ToString());            
          //  System.Console.WriteLine("S" + num.ToString() + ":" + pos.ToString());     // DEBUG
        }

       

        public void StartRead()             // Start or resume reading thread
        {
            if (!ReadThread.IsAlive) ReadThread.Start();
            ReadStatus = true;
        }

        public void StopRead()               // Pause reading thread
        {
            ReadStatus = false;
        }

    }
}
