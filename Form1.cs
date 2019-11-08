using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpcDaHelper;
using OPCDAAUTO;
using System.Net;


namespace DemoForYou
{
    public partial class Form1 : Form
    {
       

        string strHostIP = "";
        string strHostName = "";

        public Form1()
        {
            InitializeComponent();
        }

        public OpcDAClientHelper opcClient = null;

        //枚举本地OPC服务器
        private void GetLocalServer()
        {
            //获取本地计算机IP,计算机名称
            IPHostEntry IPHost = Dns.GetHostEntry(Environment.MachineName);
            if (IPHost.AddressList.Length > 0)
            {
                strHostIP = IPHost.AddressList[2].ToString();
                //txtRemoteServerIP.Text = IPHost.AddressList[2].ToString();
                Server_IP.Text = IPHost.AddressList[2].ToString();
            }
            else
            {
                return;
            }
            //通过IP来获取计算机名称，可用在局域网内
            IPHostEntry ipHostEntry = Dns.GetHostEntry(strHostIP);
            strHostName = ipHostEntry.HostName.ToString();
            //获取本地计算机上的OPCServerName
            try
            {
                //创建OPC服务器对象，获得OPC服务器列表

                object serverList = OpcDAClientHelper.GetOpcServer(strHostIP);
                //显示服务器列表
                foreach (string turn in (Array)serverList)
                {
                    cmbServerName.Items.Add(turn);
                    list_INFO.Items.Add("发现服务器 " + strHostIP.ToString() + ":  " + turn.ToString());
                    if (list_INFO.Items.Count == 40)
                    {
                        list_INFO.Items.Clear();
                    }
                }
                cmbServerName.SelectedIndex = 0;
                connect_server.Enabled = true;

            }
            catch (Exception err)
            {
                MessageBox.Show("枚举本地OPC服务器出错：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }
        private void Search_Click(object sender, EventArgs e)
        {
            GetLocalServer();
        }

        //连接OPC服务器
        private bool ConnectServer(string ServerIP, string ServerName)
        {
            try
            {
         
                opcClient = new OpcDAClientHelper(ServerIP, ServerName);

                if (opcClient.Connect())
                {

                    label3.Text = "OPC 已连接";
                }
                else
                {
                    MessageBox.Show("women");
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("连接远程服务器出现错误：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        private void connect_server_Click(object sender, EventArgs e)
        {
            try
            {
                //连接服务器
                if (!ConnectServer(Server_IP.Text, cmbServerName.Text))
                {
                    return;
                }
                list_INFO.Items.Add("已连接到" + ":  " + cmbServerName.Text.ToString());
                if (list_INFO.Items.Count == 40)
                {
                    list_INFO.Items.Clear();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("初始化出错：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        Group group = null;
        private void button2_Click(object sender, EventArgs e)
        {
             group = new Group(textBox3.Text.Trim(), textBox4.Text.Trim(), textBox5.Text.Trim());
            group.IsActive = true;
            group.IsSubscribed = true;
            group.DeadBand = 0;
            group.UpdateRate = 10;
            opcClient.AddGroup(group);
            group.DataChangeEvent += group_DataChangeEvent;
            group.AsyncReadCompleteEvent +=group_AsyncReadCompleteEvent;
            group.AsyncWriteCompleteEvent +=group_AsyncWriteCompleteEvent;
            MessageBox.Show("添加组成功");
        }

        private void group_AsyncWriteCompleteEvent(Item item)
        {
            MessageBox.Show("item:"+item.Name+ "异步写完成");
        }

        private void group_AsyncReadCompleteEvent(Item item)
        {
            MessageBox.Show("异步读结果:tag:"+item.Name+",value:"+item.Value.ToString()+"");
        }

        private void group_DataChangeEvent(Item item)
        {
            list_INFO.Invoke(new Action(() => { list_INFO.Items.Insert(0,string.Format(
            "item:{0},value:{1},time:{2},quality:{3}",item.GetItemId(),item.Value.ToString(),item.Timesnamp,item.Quality)); }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            group.AddItem(new Item(textBox6.Text) { IsActive =true});
            MessageBox.Show("添加Item成功");
        }

  

        private void button5_Click(object sender, EventArgs e)
        {
             group.Items[0].Read();
             MessageBox.Show(group.Items[0].Value.ToString());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int tmp;
            group.AsyncRead(new List<Item>() {group.Items[0]}, 1, out tmp);
            MessageBox.Show("cancleID:"+tmp.ToString());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            group.Items[0].ObjValue = this.textBox7.Text.Trim();
            if (group.Items[0].Write())
            {
                MessageBox.Show("写成功");
            }
            else

            {
                MessageBox.Show("写失败");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int tmp;
            group.Items[0].ObjValue = this.textBox7.Text.Trim();
            group.AsyncWrite(new List<Item>() { group.Items[0] }, 1, out tmp);
            MessageBox.Show("cancleID:" + tmp.ToString());
        }

    

        private void disconnect_server_Click(object sender, EventArgs e)
        {
            opcClient.DisConnect();
        }
    }
}
