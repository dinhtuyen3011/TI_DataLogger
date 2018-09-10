using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using Serial_Oscilloscope;

namespace Data_logger
{
    public partial class Form1 : Form
    {
        DateTime realtime = DateTime.Now;
        //kiem tra start bit
        bool startBit_isTrue = false;

        //khai báo bo dem
        private string asciiBuf = "";

        private bool data_enough_IsTrue = false;
        private float[] channels = new float[4];

        //khai báo biến vẽ đồ thị
        Series ADC1 = new Series();
        Series ADC2 = new Series();
        Series ADC3 = new Series();
        Series ADC4 = new Series();

        private double Point_axisX = 0;
        private double y_axisMax = 3.5;
        private double y_axisMin = 0;

        private double x_axisMax = 100;
        private double x_axisMin = 0;
        
        private CsvFileWriter csvFileWriter = null;
        List<string> stringListDataToWrite = new List<string>();

        private List<float> Data_list_ADC1 = new List<float>();
        private List<float> Data_list_ADC2 = new List<float>();
        private List<float> Data_list_ADC3 = new List<float>();
        private List<float> Data_list_ADC4 = new List<float>();   
        //khai bao bien dem để vẽ đồ thị
        private int count = 0;
        private const int count_max = 10;

        List<string> add_item = new List<string>();

        private int Sample_Received = 0;
        public Form1()
        {
            InitializeComponent();
            chart1.ChartAreas[0].AxisY.Maximum = y_axisMax;
            chart1.ChartAreas[0].AxisY.Minimum = y_axisMin;

            chart1.ChartAreas[0].AxisX.Maximum = x_axisMax;
            chart1.ChartAreas[0].AxisX.Minimum = x_axisMin;

            //chart1.Series[0].XValueType = ChartValueType.DateTime;

            //CheckForIllegalCrossThreadCalls = false;
            

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string[] myPort = SerialPort.GetPortNames();
                
                
                Com.PortName = myPort[0];
                Com.BaudRate = 115200;
                Com.Parity = Parity.None;
                Com.DataBits = 8;
                Com.StopBits = StopBits.One;
                Com.Open();
                button3.Enabled = false;
                button4.Enabled = true;
                Com.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                checkedListBox1.Enabled = true;
                Com.DiscardOutBuffer();
                Com.DiscardInBuffer();
            }
            catch (Exception)
            {
                MessageBox.Show("thiet bi chua ket noi");
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (Com.IsOpen)
            {
                Com.Close();
            }
            button3.Enabled = true;
            button4.Enabled = false;            
            checkedListBox1.Enabled = false;
           // Sample_Received = 0;
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = Com.BytesToRead;
                byte[] readbuff = new byte[bytesToRead];
                Com.Read(readbuff, 0, bytesToRead);
                foreach (byte b in readbuff)
                {
                    if (b == '*')
                    {
                        startBit_isTrue = true;
                    }
                    else
                    {
                        if ((startBit_isTrue == true) && (b != '#'))
                        {
                            asciiBuf += (char)b;
                        }
                        if ((startBit_isTrue == true) && (b == '#'))
                        {
                            startBit_isTrue = false;
                            if (asciiBuf != "" && asciiBuf != null)
                            {
                                string[] data = asciiBuf.Split(',');
                                int channelIndex = 0;
                                for (int i = 0; i < data.Length; i++)
                                {
                                    if (data[i] != "" && channelIndex < 4)
                                    {
                                        channels[channelIndex] = float.Parse(data[i]);
                                        channelIndex++;
                                    }
                                }
                                asciiBuf = "";
                                if (channelIndex > 0)
                                {
                                    Sample_Received++;
                                    Data_list_ADC1.Add(channels[0]);
                                    Data_list_ADC2.Add(channels[1]);
                                    Data_list_ADC3.Add(channels[2]);
                                    Data_list_ADC4.Add(channels[3]);
                                    if (csvFileWriter != null)
                                    {
                                        realtime = DateTime.Now;                                        
                                        string stringDataToWrite = String.Concat(realtime.ToLongTimeString(), "," + channels[0].ToString(), "," + channels[1].ToString(), "," + channels[2].ToString(), "," + channels[3].ToString());
                                        csvFileWriter.WriteCSVline(stringListDataToWrite);
                                        stringListDataToWrite.Clear();
                                        stringListDataToWrite.Add(stringDataToWrite);
                                    }
                                    count++;
                                    //data_enough_IsTrue = true;
                                    if (count == count_max)
                                    {
                                        count = 0;
                                        data_enough_IsTrue = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                DisplayValue(asciiBuf);
            }
        }
        private delegate void DlDisplay(string s);
        private void DisplayValue(string s)
        {
        }
        private void Activate_chart_serial(bool state)
        {
            chart1.Series[0].Enabled = state;
            chart1.Series[1].Enabled = state;
            chart1.Series[2].Enabled = state;
            chart1.Series[3].Enabled = state;
        }      
        private void data_listview()
        {
            for (int i = 0; i < Data_list_ADC1.Count; i++)
            {
                realtime = DateTime.Now;
                ListViewItem lvi = new ListViewItem(realtime.ToString());
                lvi.SubItems.Add(Data_list_ADC1[i].ToString());
                lvi.SubItems.Add(Data_list_ADC2[i].ToString());
                lvi.SubItems.Add(Data_list_ADC3[i].ToString());
                lvi.SubItems.Add(Data_list_ADC4[i].ToString());
                listView1.Items.Add(lvi);
            }
            listView1.Items[listView1.Items.Count - 1].EnsureVisible();
        }
        private void graph_draw()
        {
             
            for (int i = 0; i < Data_list_ADC1.Count; i++)
            {
                
                ADC1 = chart1.Series[0];
                ADC2 = chart1.Series[1];
                ADC3 = chart1.Series[2];
                ADC4 = chart1.Series[3];

                //chart1.Series[0].Points.AddXY(Point_axisX, Data_list_ADC1[i]);
                chart1.Series[0].Points.AddXY(Point_axisX, Data_list_ADC1[i]);
                chart1.Series[1].Points.AddXY(Point_axisX, Data_list_ADC2[i]);
                chart1.Series[2].Points.AddXY(Point_axisX, Data_list_ADC3[i]);
                chart1.Series[3].Points.AddXY(Point_axisX, Data_list_ADC4[i]);
                //sec.AddSeconds(1);
                Point_axisX++;

                if (Point_axisX == 101)
                {
                    Point_axisX = 0;
                    chart1.Series[0].Points.Clear();
                    chart1.Series[1].Points.Clear();
                    chart1.Series[2].Points.Clear();
                    chart1.Series[3].Points.Clear();

                }
            }
        }
        private void timer_chart_tick(object sender, EventArgs e)
        {

            if (Com.IsOpen)
            {
                
               
                if (data_enough_IsTrue == true)
                {
                    data_listview();
                    graph_draw();
                    Sample_received.Text = Sample_Received.ToString();
                    
                    Data_list_ADC1.Clear();
                    Data_list_ADC2.Clear();
                    Data_list_ADC3.Clear();
                    Data_list_ADC4.Clear();
                    data_enough_IsTrue = false;
                }
            }
        }
        private void SaveData_btn_Click(object sender, EventArgs e)
        {
            SaveData_btn.Enabled = false;
            stop_save_data.Enabled = true;
            string[] dateTimeToFileName = new string[6];
            dateTimeToFileName[0] = "data_logger_";
            dateTimeToFileName[1] = DateTime.Now.Day.ToString();
            dateTimeToFileName[2] = "_" + DateTime.Now.Month.ToString();
            dateTimeToFileName[3] = "_" + DateTime.Now.Year.ToString();
            dateTimeToFileName[4] = "_" + DateTime.Now.Hour.ToString();
            dateTimeToFileName[5] = "_" + DateTime.Now.Minute.ToString();
           
            string fileName = string.Concat(dateTimeToFileName);
            string String_FilePath = "";
            realtime = DateTime.Now;
            String_FilePath = String.Concat("F:\\", fileName, ".csv");
            string filePath = @String_FilePath;
            string fileFormat = "Thời gian, ADC1, ADC2, ADC3, ADC4";
            try
            {
                csvFileWriter = new CsvFileWriter(filePath, realtime.ToLongDateString(), fileFormat);

            }
            catch (Exception ex)
            {

            }
        }

        private void stop_save_data_Click(object sender, EventArgs e)
        {
            if (csvFileWriter != null)
            {
                csvFileWriter.CloseFile();
                csvFileWriter = null;
            }
            SaveData_btn.Enabled = true;
            stop_save_data.Enabled = false;
        }
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {        
            if (e.NewValue == CheckState.Checked)
            {
                add_item.Add(checkedListBox1.Items[e.Index].ToString());
            }
            if (e.NewValue == CheckState.Unchecked)
            {
                add_item.Remove(checkedListBox1.Items[e.Index].ToString());
            }
            select_channel();
        }
        private void select_channel()
        {
            if (add_item.Contains("All"))
            {
                Activate_chart_serial(true);
            }
            else
            {
                if (add_item.Contains("ADC1"))
                {
                    ADC1.Enabled = true;
                }
                else
                {
                    ADC1.Enabled = false;
                }

                if (add_item.Contains("ADC2"))
                {
                    ADC2.Enabled = true;
                }
                else
                {
                    ADC2.Enabled = false;
                }

                if (add_item.Contains("ADC3"))
                {
                    ADC3.Enabled = true;
                }
                else
                {
                    ADC3.Enabled = false;
                }

                if (add_item.Contains("ADC4"))
                {
                    ADC4.Enabled = true;
                }
                else
                {
                    ADC4.Enabled = false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].CursorX.AutoScroll = true;
            chart1.ChartAreas[0].CursorY.AutoScroll = true;
            int blockSize = 10;
            double blocksizeY = 0.2;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScaleView.SizeType = DateTimeIntervalType.Number;
            chart1.ChartAreas[0].AxisY.ScaleView.SizeType = DateTimeIntervalType.Number;
            int position = 0;
            int size = blockSize;
            double sizeY = blocksizeY;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoom(position, size);
            chart1.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollSize = blockSize;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoom(position, sizeY);
            chart1.ChartAreas[0].AxisY.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            chart1.ChartAreas[0].AxisY.ScaleView.SmallScrollSize = blocksizeY;
        }
    }
    

}

