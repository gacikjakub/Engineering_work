using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ServoMonitoring_with_Control
{
    
    public partial class Form1 : Form
    {
        //Delegates:
        delegate void SetTextCallback(Label l, ProgressBar pb, string val);
        delegate void SetTextCallback2(Label lb, string text);
        delegate void SetChartPoint(Chart ch, String series, float val);

        // Variables and Consts
        ArduinoConnection ArduinoDevice;   // To Connect with Board with servos
        CustomQueue Servo1Que, Servo2Que;  // To keep measured values
        Thread CountingThread;          // it refresh charts
        bool CountingStatus;            // true - refreshing
        const int SecInChart = 7;       //  last seconds in charts
        const int NewValInterval = 20;      // Time interval in refreshing charts  ----   using in Counting Thread
        int ValPerSec;                  // is calculated based on const above
        JoystickController myJoystick;      // For steering servos by joystic

        //Constructors
        public Form1()
        {
            InitializeComponent();
            ValPerSec = 1000 / NewValInterval;
            Servo1Que = new CustomQueue(5);
            Servo2Que = new CustomQueue(5);
            CountingStatus = false;
            ArduinoDevice = new ArduinoConnection("I am GOD", this);
            CountingThread = new Thread(new ThreadStart(this.CreateCountingThread));
            CountingThread.IsBackground = true;
            myJoystick = new JoystickController();
            myJoystick.JoystickListener(onMoveJoystick);
        }

        //@Override -- what we want to do when joystick has been moved
        void onMoveJoystick()
        {
            int x = myJoystick.GetX();
            int y = myJoystick.GetY();
            ArduinoDevice.SetServo(2, Convert.ToInt32(ExtensionMethods.Remap(x, -100, 90, 10, 140))); // maping value and convert to int for sending on arduino
            SetLabel(label3, x.ToString());                     // show this value in label on window
            ArduinoDevice.SetServo(1, Convert.ToInt32(ExtensionMethods.Remap(y, -100, 100, 10, 140)));
            SetLabel(label8, y.ToString());
        }

        
        private void CreateCountingThread()   //  get overloading value with concrete time interval ...
        {
            while (true)
                while (CountingStatus)
                {
                   Servo1Que.Add(int.Parse(this.label4.Text));      // .. add it to que ...
                   Servo2Que.Add(int.Parse(this.label7.Text));
                    SetChart(chart1, "Series1", Servo1Que.Averange());    // .. and add averange from que to chart
                    SetChart(chart2, "Series1", Servo2Que.Averange());
                    Thread.Sleep(NewValInterval);                           // here is interval
                }
        }

        //Modify the chart
        private void SetChart(Chart ch, String series, float val) 
        {
            if (ch.InvokeRequired)
            {
                SetChartPoint d = new SetChartPoint(SetChart); //need delegate function
                this.Invoke(d, new object[] { ch,series,val });
            }
            else
            {
                if (ch.Series[series].Points.Count >= SecInChart*ValPerSec)    // delete the oldest values from chart if is full
                    ch.Series[series].Points.RemoveAt(ch.Series[series].Points.Count - SecInChart * ValPerSec);
                ch.Series[series].Points.AddY(val);     //  add new value
                ch.ChartAreas[0].RecalculateAxesScale();    // recalculate for resizing chart
            }
        }

        
        // For setting Value in progressBar another function is required
        private void setProgressBar(ProgressBar pb, int Val)
        {
            pb.Value = Val;                                     
            if (Val < 50) pb.ForeColor = Color.Lime;                        // Color of the bar is depending on value
            if (Val > 50) pb.ForeColor = Color.GreenYellow;                 // In practise we get only small value below 20 and 
            if (Val > 200) pb.ForeColor = Color.DarkGreen;                  // high above 500
            if (Val > 480) pb.ForeColor = Color.Red;
            pb.Style = ProgressBarStyle.Continuous;                         // is required for changing color
        }


        private void SetDiffValue(Label l, ProgressBar pb, string val)              // set value in concrete label and progress bar
        {
            if (l.InvokeRequired)
            {
                    SetTextCallback d = new SetTextCallback(SetDiffValue);      // deletagte is required
                    this.Invoke(d, new object[] {l,pb, val });
            }
            else
            {
                l.Text = val;
                int a = int.Parse(val);
                if(a>pb.Minimum && a <pb.Maximum)                   // different value is not expected by it is for secure
                setProgressBar(pb, a);
            }     
        }

        public void SetDiffVal1(string val)                         // this is the function used by other class to set value
        {                                                           // in concrete label and progressBar 
            SetDiffValue(this.label4, this.progressBar1, val);      // this solution is more safety
        }

        public void SetDiffVal2(String val)
        {
            SetDiffValue(this.label7, this.progressBar2, val);
        }

        public void SetLabel(Label lb,String text)
        {
            if (lb.InvokeRequired)
            {
                SetTextCallback2 d = new SetTextCallback2(SetLabel);        //delegate is required
                this.Invoke(d, new object[] { lb,text });
            }
            else
            {
                this.label3.Text = text;
            }
        }

        public void SetLabel3(string text)
        {
            SetLabel(label3, text);
        }



        private void button1_Click(object sender, EventArgs e)      // Button START
        {
            ArduinoDevice.StartRead();                              // Start reading data from device
            if (!CountingThread.IsAlive) CountingThread.Start();       // Start Counting Thread for capture data
            this.CountingStatus = true;                                 // also
            myJoystick.JoystickListenerStart();                     // Start reading moves from joystick
        }

        private void button2_Click(object sender, EventArgs e)      // Button STOP
        {
            ArduinoDevice.StopRead();                               // Stop reading data from device
            this.CountingStatus = false;                            // Pause Counting Thread

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
 
        }

        private void button3_Click(object sender, EventArgs e)
        {
           
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)       // Vertical Bar for controlling servo
        {
            this.label5.Text = this.vScrollBar1.Value.ToString();          // write set value to label below Bar
            ArduinoDevice.SetServo(1, vScrollBar1.Value);                // send this value to device
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)           // Horizontal Bar for controlling servo
        {
            this.label6.Text = this.hScrollBar1.Value.ToString();       // write set value to label below Bar
            ArduinoDevice.SetServo(2, hScrollBar1.Value);               // send this value to device
        }
    }
}
