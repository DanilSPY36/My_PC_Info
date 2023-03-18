using System;
using System.Drawing;
using System.Threading;
using System.Management;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OpenHardwareMonitor.Hardware.CPU;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace PC_Info
{
    public partial class MainForm : Form
    {
        private string tmpInfo = string.Empty;
        private Computer computer;

        public MainForm()
        {
            InitializeComponent();
        }

        private void TreeViewNodesAdd(object sender, EventArgs e)
        {
            int CPUCores = 0;
            computer.Open();
            computer.Accept(new Visitor());
            richTextBox1.Text = string.Empty;
            treeView1.BeginUpdate();
            foreach (IHardware hardware in computer.Hardware)
            {
                treeView1.Nodes.Add(hardware.HardwareType.ToString());
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total"))
                    {
                        treeView1.Nodes[1].Nodes.Add($"Total: " + ((float)sensor.Value.GetValueOrDefault()).ToString());
                    }
                    else if ( sensor.Name.Contains("CPU"))
                    {
                        treeView1.Nodes[1].Nodes.Add($"Core #{++CPUCores} : " + ((float)sensor.Value.GetValueOrDefault()).ToString());
                    }
                }
            }
            treeView1.EndUpdate();
            computer.Close();
        }
       


        private void GetTemp(object sender, EventArgs e)
        {
            computer.Open();
            computer.Accept(new Visitor());
            richTextBox1.Text = string.Empty;

            foreach (IHardware hardware in computer.Hardware)
            {
                richTextBox1.Text += $"\tHardware: {0} {hardware.Name}\n";

                foreach (IHardware subhardware in hardware.SubHardware)
                {
                    richTextBox1.Text += $"\tSubhardware: {0} {subhardware.Name}\n";

                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        richTextBox1.Text +=$"\tSensor: {0}, value: {1} + {sensor.Name} {sensor.Value}\n";
                    }
                }

                foreach (ISensor sensor in hardware.Sensors)
                {
                    richTextBox1.Text +=$"\tSensor: {0}, value: {1} + {sensor.Name} {sensor.Value}\n";
                }
            }
            
            computer.Close();
        }

        private async Task<string> GetCPUTempAsync()
        {
            computer.Open();
            computer.Accept(new Visitor());
            tmpInfo = string.Empty;
            foreach (IHardware hardware in computer.Hardware)
            {
                
                tmpInfo +="Hardware: {0} " + hardware.Name + "\n"; 
                
                foreach (IHardware subhardware in hardware.SubHardware)
                {
                    
                    tmpInfo += "\tSubhardware: {0}" +  subhardware.Name+ "\n";

                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        tmpInfo += "\t\tSensor: {0}, value: {1} " +  $"{sensor.Name}  {sensor.Value}"+ "\n";
                    }
                }

                foreach (ISensor sensor in hardware.Sensors)
                {
                    tmpInfo += "\tSensor: {0}, value: {1} " +  $"{sensor.Name}  {sensor.Value}"+ "\n";
                }
            }
            computer.Close();
            return tmpInfo;            
        }

        private void GetHardWareInfo(string Key, System.Windows.Forms.ListView list)
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
            computer = new Computer()
            {
                CPUEnabled = true,
                //GPUEnabled= true,
                RAMEnabled = true, // uncomment for RAM reports
                MainboardEnabled = true, // uncomment for Motherboard reports
                FanControllerEnabled = true, // uncomment for FAN Reports
                HDDEnabled = true, // uncomment for HDD Report
            };
        }

        private void MainForm_AutoSizeChanged(object sender, EventArgs e)
        {
            listView1.Columns[1].Width =  Size.Width - listView1.Columns[0].Width;
        }

        private async void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            
                TreeViewNodesAdd(sender, e);
            
            /*
            while (tabControl1.SelectedTab.Text == "Temp")
            {
                richTextBox1.Text = await Task.Run(() => GetCPUTempAsync());
                await Task.Delay(1000);
            }
            //////
            //////*/
            while (tabControl1.SelectedTab.Text == "Temp")
            {
                GetTemp(sender, e);
                await Task.Delay(100);
            }
            
        }
    }
}