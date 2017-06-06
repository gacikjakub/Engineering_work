using System;
using System.Collections;
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
        private Thread WriteThread;              //  Writing data to device on another thread
        private String SecretCode;
        public Boolean Servo1Status;               // True - servo Attach
        public Boolean Servo2Status;               // False - servo Detach
        private Boolean SendingStatus;
        private Queue SendingQueue;

        Form Owner;                             // For modyfying labels and bars in window

        private void SendMessage(String message)
        {
            SendingQueue.Enqueue(message);
            /*while (SendingStatus) { ExtensionMethods.Wait(5); }
            SendingStatus = true;
            Port.WriteLine(message);
            ExtensionMethods.Wait(10);
            SendingStatus = false;
            */
        }

        //Constructior:
        public ArduinoConnection(String  code, Form frm)
        {
            Owner = (Form1)frm;            // set window for showing captured data
            Application.ApplicationExit += new EventHandler(OnProcessExit);
            Owner.FormClosing += OnProcessExit;
            Port = new SerialPort();        
            Port.BaudRate = 9600;           // BuadRate in transmission
            int timeout = Port.ReadTimeout;
            SendingQueue = new Queue();
            Port.ReadTimeout = 30;
            SecretCode = code + '\r';
            string[] ports = SerialPort.GetPortNames();
            bool cnt = false;
            bool br = false;
            foreach (string port1 in ports)
            {
                //Console.WriteLine("I am in" + port1);
                try
                {
                    Port = new SerialPort();
                    Port.BaudRate = 230400;           // BuadRate in transmission
                    Port.PortName = port1;        // "COM__"
                    Port.ReadTimeout = 1000;
                    Port.Open();                    // Opening Port             
                    Console.WriteLine("Opening port " + Port.PortName);
                    for (int i = 0; i < 2; i++)
                    {
                        String receivedCode = Port.ReadLine();
                        byte[] code1 = Encoding.ASCII.GetBytes(receivedCode);
                        byte[] code2 = Encoding.ASCII.GetBytes(SecretCode);
                        foreach (byte n in code1) Console.Write(n); Console.WriteLine();
                        foreach (byte n in code2) Console.Write(n); Console.WriteLine();
                        //Console.WriteLine(code1);
                        //Console.WriteLine(code2);
                        Console.WriteLine();
                        if (!(receivedCode.Equals(SecretCode)))
                        {
                            Console.WriteLine("Read from port");
                            cnt = true;
                            continue;
                        }
                        else
                        {
                            SecretCode = code + '\n';
                            Port.WriteLine(SecretCode);
                            cnt = false;
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
            if (br)
            {
                ReadStatus = false;             // Stop reading data on start
                ReadThread = new Thread(new ThreadStart(this.CreateReadThread));        //Initialize the tread
                WriteThread = new Thread(new ThreadStart(this.CreateWriteThread));        //Initialize the tread
                ReadThread.IsBackground = true;                     // Thread is closing with closing form
                WriteThread.IsBackground = true;                     // Thread is closing with closing form
                WriteThread.Start();
                Port.ReadTimeout = timeout;
                Servo1Status = true;
                Servo2Status = true;
                SendingStatus = true;

            } else
            {
                MessageBox.Show("Device couldn't pair with PC",
                                "Device Error",
                                MessageBoxButtons.OK,
                               // MessageBoxIcon.Warning // for Warning  
                               MessageBoxIcon.Error // for Error 
                           //MessageBoxIcon.Information  // for Information
                           //MessageBoxIcon.Question // for Question
                    );
                                                                // Sequence of procedures is important
                System.Environment.Exit(Environment.ExitCode);          // First of all Envronment Exit - it terminates all threads
                Application.ExitThread();
                Application.Exit();
                Owner.Close();
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                SendMessage("Exit");
                System.Environment.Exit(Environment.ExitCode);
                Application.ExitThread();
                Application.Exit();
                Owner.Close();
                Port.Close();
                
            } 
            catch (Exception)
            {

            }
        }

        private void CreateWriteThread ()            // Thread to capturing darta
        {
            while (true)
            while (SendingStatus)
            {
                    if (SendingQueue.Count != 0) Port.WriteLine((String)SendingQueue.Dequeue());
                     Thread.Sleep(2);    
            }
             
        }

        private void CreateReadThread()            // Thread to capturing darta
        {
            while (true)
                while (ReadStatus)
                {
                    String data = Port.ReadLine();          // write received line to var
                    if (data.Contains("D1:"))            // when line is beginig by "D1" it concerns servo1
                    {
                        ((Form1)Owner).SetDiffVal1(data.Substring(3));          // show overload value in form without  "D1:"
                    }
                    else
                    if (data.Contains("D2:"))           // when line is beginig by "D2" it concerns servo2
                    {
                        ((Form1)Owner).SetDiffVal2(data.Substring(3));           // show overload value in form  without  "D2:"
                    }
                    Thread.Sleep(10);
                }

        }

        public void DetachServo(int num)
        {
            SendMessage("S" + num.ToString() + ":F");
            if (num == 1) Servo1Status = false;
            if (num == 2) Servo2Status = false;
        }


        public void AttachServo(int num)
        {
            SendMessage("S" + num.ToString() + ":T" );
            if (num == 1) Servo1Status = true;
            if (num == 2) Servo2Status = true;
        }

        

        public void SetServo(int num, int pos)              // method for set servo in arduino, type in Numer of servo and position
        {
            if (num == 1 & Servo1Status == true)
            SendMessage("S1:" + pos.ToString());  
            else
               if (num == 2 & Servo2Status == true)
                SendMessage("S2:" + pos.ToString());
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
