using System;
using System.Drawing;
using System.Threading;
using System.Management;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PC_Info
{
    public partial class MainForm : Form
    {
        BackgroundWorker bw;
        private string tmpInfo = string.Empty;
        public MainForm()
        {
            InitializeComponent();
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress= true;
        }

        private void GetCPUTemp()
        {
            tmpInfo = string.Empty;
            Visitor visitor = new Visitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled= true;
            computer.Accept(visitor);

            for(int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {
                            tmpInfo += $"{ computer.Hardware[i].Sensors[j].Name} : {computer.Hardware[i].Sensors[j].Value.ToString()} \n";
                        }
                        else
                        {
                            tmpInfo += $"{computer.Hardware[i].Sensors[j].Name} : {computer.Hardware[i].Sensors[j].Value.ToString()} \n";
                        }
                    }
                }
            }
            Invoke((MethodInvoker)delegate
            {
                richTextBox1.Text = tmpInfo;

            });
            //Task.Run(() =>
            //{
                
            //});
            computer.Close();
        }

        private void GetHardWareInfo(string Key, ListView list)
        {
            list.Items.Clear();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM " + Key);
            try
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    ListViewGroup listViewGroup;

                    try
                    {
                        listViewGroup = list.Groups.Add(obj["Name"].ToString(), obj["Name"].ToString());
                    }
                    catch (Exception ex)
                    {
                        listViewGroup = list.Groups.Add(obj.ToString(), obj.ToString());
                    }

                    if(obj.Properties.Count == 0)
                    {
                        MessageBox.Show("Failed to get information", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        foreach (PropertyData data in obj.Properties)
                        {
                            ListViewItem item = new ListViewItem(listViewGroup);

                            if (list.Items.Count % 2 == 0)
                            {
                                item.BackColor = Color.WhiteSmoke;
                            }
                            else
                            {
                                item.BackColor = Color.White;
                            }
                            item.Text = data.Name;
                            if (data.Value != null && !string.IsNullOrEmpty(data.Value.ToString()))
                            {
                                switch (data.Value.GetType().ToString())
                                {
                                    case "System.String[]":
                                        string[] stringData = data.Value as string[];
                                        string resStr1 = string.Empty;

                                        foreach (string str in stringData)
                                        {
                                            resStr1 += $"{str} ";
                                            item.SubItems.Add(resStr1);
                                        }
                                        break;

                                    case "System.UInt16[]":
                                        ushort[] ushorts = data.Value as ushort[];
                                        string resStr2 = string.Empty;
                                        foreach (ushort u in ushorts)
                                        {
                                            resStr2 += $"{Convert.ToString(u)}";
                                        }
                                        item.SubItems.Add(resStr2);
                                        break;
                                    default:
                                        item.SubItems.Add(data.Value.ToString());
                                        break;
                                }
                                list.Items.Add(item);
                            }
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
                CPU
                GPU
                Battery
                BIOS
                RAM
                Cache
                USB
                Disk
                Logical drivers
                Keyboard
                Netvork
                Users
             */

            string key = string.Empty;
            switch (toolStripComboBox.SelectedItem.ToString())
            {
                case "CPU":
                    key = "Win32_Processor";
                    break;
                case "GPU":
                    key = "Win32_VideoController";
                    break;
                case "Battery":
                    key = "Win32_Battery";
                    break;
                case "BIOS":
                    key = "Win32_BIOS";
                    break;
                case "RAM":
                    key = "Win32_PhysicalMemory";
                    break;
                case "Cache":
                    key = "Win32_CacheMemory";
                    break;
                case "USB":
                    key = "Win32_USBController";
                    break;
                case "Disk":
                    key = "Win32_DiskDrive";
                    break;
                case "Logical drivers":
                    key = "Win32_LogicalDisk";
                    break;
                case "Keyboard":
                    key = "Win32_Keyboard";
                    break;
                case "Netvork":
                    key = "Win32_NetworkAdapter";
                    break;
                case "Users":
                    key = "Win32_Account";
                    break;
                default:
                    key = "Win32_Processor";
                    break;
            }
            GetHardWareInfo(key, listView1);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            toolStripComboBox.SelectedIndex = 0;
        }

        private void MainForm_AutoSizeChanged(object sender, EventArgs e)
        {
            listView1.Columns[1].Width =  Size.Width - listView1.Columns[0].Width;
        }


        //private async void Bw_DoWork()
        //{
        //    while (true)
        //    {
        //        richTextBox1.Text = GetCPUTemp();
        //        Thread.Sleep(100);
        //    }
        //}

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControl1.SelectedTab.Text == "Temp")
            {
                //backgroundWorker1.DoWork += (obj, ea) => Bw_DoWork();
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                backgroundWorker1.CancelAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                GetCPUTemp();
                Thread.Sleep(100);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }
}
