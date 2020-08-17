using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string UserSettingsFilename = "settings.xml";
        public string _DefaultSettingspath = AppDomain.CurrentDomain.BaseDirectory + "\\Settings\\" + UserSettingsFilename;
        Boolean isProxing = false;
        List<string> outList = new List<string>();
        MySettings Settings = new MySettings();

        public MainWindow()
        {
            InitializeComponent();
            Settings = Settings.Read(_DefaultSettingspath);
            if (Settings.client != "") clientTextBox.Text = Settings.client;
        }

        public async void startAdbClick(object sender, RoutedEventArgs e)
        {

            outList = await RunDetailCmd("adb kill-server&&adb connect "+ clientTextBox.Text + "&&adb devices");
            Settings.client = clientTextBox.Text;
            Settings.Save(_DefaultSettingspath);
            nowDeviceTextBlock.Text = outList.Last();
        }

        protected void DropToInstall(object sender, DragEventArgs e)
        {
            string filePath = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            textBlock1.Text = "安装的文件为: " + filePath;
            RunDetailCmd("adb install " + filePath);
            //using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
            //{
            //    textBlock1.Text = sr.ReadToEnd();
            //}
        }
        protected void DropToSend(object sender, DragEventArgs e)
        {
            string filePath = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            textBlock2.Text = "发送的文件为: " + filePath;
            RunDetailCmd("adb push " + filePath + " /sdcard/");
        }

        private void scrcpyClick(object sender, RoutedEventArgs e)
        {

            RunDetailCmd("scrcpy");

        }

        private async void GlobalProxyClick(object sender, RoutedEventArgs e)
        {
            List<string> ipv4_ips = GetLocalIpAddress("InterNetwork");
            string local_ipv4 = ipv4_ips.Last();
            if (!isProxing)
            {
                outList = await RunDetailCmd("adb shell settings put global http_proxy " + local_ipv4 + ":8888");
                isProxing = true;
                btGlobalProxy.Content = "代理开启成功";
            }
            else
            {
                outList = await RunDetailCmd("adb shell settings put global http_proxy :8888");
                isProxing = false;
                btGlobalProxy.Content = "代理已经关闭";
            }

        }
        /// <summary>
        /// 获取本机所有ip地址
        /// </summary>
        /// <param name="netType">"InterNetwork":ipv4地址，"InterNetworkV6":ipv6地址</param>
        /// <returns>ip地址集合</returns>
        public static List<string> GetLocalIpAddress(string netType)
        {
            string hostName = Dns.GetHostName();     //获取主机名称 
            IPAddress[] addresses = Dns.GetHostAddresses(hostName); //解析主机IP地址 
            List<string> IPList = new List<string>();
            if (netType == string.Empty)
            {
                for (int i = 0; i < addresses.Length; i++)
                {
                    IPList.Add(addresses[i].ToString());
                }
            }
            else
            {
                //List<string> ips = GetLocalIpAddress("");//获取本地所有ip
                //List<string> ipv4_ips = GetLocalIpAddress("InterNetwork");//获取ipv4类型的ip
                //List<string> ipv6_ips = GetLocalIpAddress("InterNetworkV6");//获取ipv6类型的ip
                //AddressFamily.InterNetwork表示此IP为IPv4,
                //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                for (int i = 0; i < addresses.Length; i++)
                {
                    if (addresses[i].AddressFamily.ToString() == netType)
                    {
                        IPList.Add(addresses[i].ToString());
                    }
                }
            }
            return IPList;
        }

        public async Task<List<string>> RunDetailCmd(string command)
        {
            Process cmd = new Process();
            //设置要启动的应用程序
            cmd.StartInfo.FileName = "cmd.exe";
            //是否使用操作系统shell启动
            cmd.StartInfo.UseShellExecute = false;
            // 接受来自调用程序的输入信息
            cmd.StartInfo.RedirectStandardInput = true;
            //输出信息
            cmd.StartInfo.RedirectStandardOutput = true;
            // 输出错误
            cmd.StartInfo.RedirectStandardError = true;
            //不显示程序窗口
            cmd.StartInfo.CreateNoWindow = true;
            //启动程序
            cmd.Start();
            //向cmd窗口发送输入信息
            cmd.StandardInput.WriteLine(command +" &exit");
            cmd.StandardInput.AutoFlush = true;
            //获取输出信息
            List<string> strOuputList = new List<string>();
            StreamReader reader = cmd.StandardOutput;
            string line = reader.ReadLine();
            await Task.Run(() =>
            {
                while (!reader.EndOfStream)
                {
                    strOuputList.Add(line);
                    line = reader.ReadLine();
                }

            foreach (string item in strOuputList)
            {
                Debug.WriteLine(item);
            }
            if (strOuputList.Count >= 4) strOuputList.RemoveRange(0, 4);
            else
            {
                strOuputList.RemoveRange(0, 3);
                strOuputList.Add("执行成功! ");
            }            });
            cmdTextBlock.Text = "";
            foreach (string item in strOuputList)
            {
                cmdTextBlock.Text += "\n" + item;
                Debug.WriteLine(item);
            }
            //等待程序执行完退出进程
            cmd.WaitForExit();
            cmd.Close();
            return strOuputList;
        }

        // 调整窗口大小
        private void just_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine(this.Width);
            Debug.WriteLine(this.Height);
        }
    }
}
