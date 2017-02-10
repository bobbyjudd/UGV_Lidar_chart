using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO.Ports;
using SCIP_library;

namespace UGV_Lidar_chart
{
    public partial class Form1 : Form
    {
        private List<long> distances = new List<long>();
        private SerialPort urg;
        private bool run;
        private const int GET_NUM = 10;
        private int start_step, end_step;
        private string port_name;
        private int baudrate;

        public Form1()
        {
            InitializeComponent();

            //Start running by default
            run = true;

            //Set polar graph specifications
            chart1.Series["LidarData"].ChartType = SeriesChartType.Polar;
            chart1.Series["LidarData"].Color = Color.Red;

            ChartArea ca = chart1.ChartAreas[0];
            Axis ax = ca.AxisX;
            Axis ay = ca.AxisY;

            ax.Minimum = 0;
            ax.Maximum = 360;
            ax.Interval = 15;   // 15° interval
            ax.Crossing = -90;    // start the segments at the top!

            //Open the serial connection to the lidar
            port_name = "COM3";
            baudrate = 9600;
            urg = new SerialPort(port_name, baudrate);
            urg.NewLine = "\n\n";
            urg.Open();
            urg.Write(SCIP_Writer.SCIP2());
            urg.ReadLine(); // ignore echo back
            
            Application.Idle += build_graph;
        }

        void build_graph(object sender, EventArgs e)
        {
            const int GET_NUM = 10;
            start_step = 180; // 0 degrees. Consider getting info from a text box
            end_step = 900;// 180 degrees. Consider getting info from a text box

            //Get current lidar data
            if (run)
            {
                try
                {
                    urg.Write(SCIP_Writer.MD(start_step, end_step));
                    urg.ReadLine(); // ignore echo back

                    List<long> distances = new List<long>();
                    long time_stamp = 0;
                    for (int i = 0; i < GET_NUM; ++i)
                    {
                        string receive_data = urg.ReadLine();
                        if (!SCIP_Reader.MD(receive_data, ref time_stamp, ref distances))
                        {
                            Console.WriteLine(receive_data);
                            break;
                        }
                        if (distances.Count == 0)
                        {
                            Console.WriteLine(receive_data);
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    //Write lidar data to the polar graph
                    chart1.Series["LidarData"].Points.Clear();

                    for (int i = 0; i < 180; i++)
                    {
                        chart1.Series["LidarData"].Points.AddXY(i, distances[i]);
                    }
                }
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Stop lidar measurement
            run = false;
            urg.Write(SCIP_Writer.QT()); // stop measurement mode
            urg.ReadLine(); // ignore echo back
            urg.Close();
        }
    }
}
