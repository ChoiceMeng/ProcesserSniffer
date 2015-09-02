using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ProcesserSniffer
{
    public partial class Form1 : Form
    {
        public static Form1 ins;

        BindingList<ProgramInfo> mProInfo = new BindingList<ProgramInfo>();
        public ProgramInfo mCurPro = null;
        private Timer CheckTimer = new Timer();
        string mstrCurSel = "";
        string mstrLastRun = "";
        DateTime mStarTime;
        DateTime mFinishTime;

        public Form1()
        {
            InitializeComponent();
            LoadInfoXML("Config\\Info.xml");
            listBox1.DataSource = mProInfo;
            listBox1.DisplayMember = "Name";

            this.CheckTimer.Tick += new System.EventHandler(this.CheckTimer_Tick);

            BindingList<ProgramInfo> list = (BindingList<ProgramInfo>)listBox1.DataSource;
            if (list.Count != 0)
            {
                mstrCurSel = list[0].mstrPath;
                mCurPro = list[0];
                ShowInfo(list[0]);
            }

            ins = this;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //初始化一个OpenFileDialog类
            OpenFileDialog fileDialog = new OpenFileDialog();

            //判断用户是否正确的选择了文件
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                //获取用户选择文件的后缀名
                string extension = Path.GetExtension(fileDialog.FileName);
                //声明允许的后缀名
                string[] str = {".exe", ".ink"};
                if (!str.Contains(extension))
                {
                    MessageBox.Show("非可执行程序!");
                }
                else
                {
                    for (int i = 0; i < mProInfo.Count; ++i)
                    {
                        ProgramInfo info = mProInfo[i];
                        if (info.mstrPath == fileDialog.FileName)
                        {
                            MessageBox.Show("文件已存在!");
                            return;
                        }
                    }

                    mstrCurSel = fileDialog.FileName;

                    // 添加到列表
                    ProgramInfo pro = new ProgramInfo();
                    pro.mstrPath = fileDialog.FileName;
                    CheckWorkDir(pro);
                    //listBox1.DataSource = null;
                    mProInfo.Add(pro);
                    try
                    {
                        
                        //listBox1.Items.Add(pro.Name);

                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                    SaveInfoXML("Config\\Info.xml");
                }
            }
        }

        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            if (mstrLastRun != "" && (mCurPro.mProcess == null || mCurPro.mProcess.HasExited))
            {
                this.CheckTimer.Enabled = false;
                mFinishTime = DateTime.Now;
                TimeSpan due = mFinishTime - mStarTime;

                WriteInfo(due);

                mstrLastRun = "";

                mCurPro = null;
            }
        }

        void StartProgram(string strPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(strPath);
            if (startInfo.WorkingDirectory == null || !Directory.Exists(startInfo.WorkingDirectory))
            {
                if (File.Exists(strPath))
                {
                    int nFindPos = strPath.LastIndexOf('\\');
                    if (nFindPos == -1)
                        nFindPos = strPath.LastIndexOf('/');

                    if (nFindPos >= 0)
                    {
                        startInfo.WorkingDirectory = strPath.Substring(0, nFindPos);
                    }
                }

                for (int i = 0; i < mProInfo.Count; ++i)
                {
                    ProgramInfo info = mProInfo[i];
                    if (info.mstrPath == strPath)
                        mCurPro = info;
                }

                if (mCurPro == null)
                    return;

                mCurPro.mProcess = Process.Start(startInfo);
                mstrLastRun = strPath;
                this.CheckTimer.Enabled = true;
                mStarTime = DateTime.Now;
            }
        }

        void WriteInfo(TimeSpan due)
        {
            string szCur = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;

            mCurPro.mnHours = mCurPro.mnHours + due.Hours;
            mCurPro.mnMinus = mCurPro.mnMinus + due.Minutes;
            mCurPro.mnSeconds = mCurPro.mnSeconds + due.Seconds;
            mCurPro.szLastRun = szCur;

            richTextBox1.Text = "运行时间:" + due.Hours + "小时" + due.Minutes + "分钟" + due.Seconds + "秒  累计运行:" + mCurPro.mnHours + "小时" + mCurPro.mnMinus + "分钟" + mCurPro.mnSeconds + "秒  上次运行：" + szCur;

            SaveInfoXML("Config\\Info.xml");
        }

        void ShowInfo(ProgramInfo info)
        {
            richTextBox1.Text = "运行时间:" + "0小时" + "0分钟" + "0秒" + "  累计运行:" + info.mnHours + "小时" + info.mnMinus + "分钟" + info.mnSeconds + "秒  上次运行：" + info.szLastRun;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartProgram(mstrCurSel);
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.X, e.Y);
            listBox1.SelectedIndex = index;
            if (listBox1.SelectedIndex != -1)
            {
                mCurPro = (ProgramInfo)listBox1.SelectedItem;
                mstrCurSel = mCurPro.mstrPath;
                ShowInfo(mCurPro);
            }
        }

        void LoadInfoXML(string strPath)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(strPath);

                XmlNode rootNode = xmlDoc.GetElementsByTagName("Root")[0];
                if (rootNode == null)
                    return;
                for (int nIndex = 0; nIndex < rootNode.ChildNodes.Count; ++nIndex)
                {
                    ProgramInfo info = new ProgramInfo();

                    for(int i = 0; i < rootNode.ChildNodes[nIndex].Attributes.Count; ++i)
                    {
                        XmlAttribute xa = rootNode.ChildNodes[nIndex].Attributes[i];
                        switch (xa.Name)
                        {
                            case "Path":
                                info.mstrPath = xa.Value;
                                break;
                            case "RunHour":
                                info.mnHours = int.Parse(xa.Value);
                                break;
                            case "RunMinus":
                                info.mnMinus = int.Parse(xa.Value);
                                break;
                            case "RunSeconds":
                                info.mnSeconds = int.Parse(xa.Value);
                                break;
                            case "LastRun":
                                info.szLastRun = xa.Value;
                                break;
                            case "ShowName":
                                info.szShowName = xa.Value;
                                break;
                        }
                    }


                    CheckWorkDir(info);

                    this.mProInfo.Add(info);
                    //listBox1.Items.Add(Path.GetExtension(strPath));
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void CheckWorkDir(ProgramInfo mInfo)
        {
            if (!Directory.Exists(mInfo.szWorkDir))
            {
                if (Directory.Exists(mInfo.mstrPath))
                    mInfo.szWorkDir = mInfo.mstrPath;
                else if (File.Exists(mInfo.mstrPath))
                {
                    int nFindPos = mInfo.mstrPath.LastIndexOf('\\');
                    if (nFindPos == -1)
                        nFindPos = mInfo.mstrPath.LastIndexOf('/');

                    if (nFindPos >= 0)
                    {
                        mInfo.szWorkDir = mInfo.mstrPath.Substring(0, nFindPos);
                    }
                }
            }
        }

        public void SaveInfoXML(string strPath)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                //加入XML的声明段落,<?xml version="1.0" encoding="gb2312"?>
                XmlDeclaration xmlDeclar;
                xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "gb2312", null);
                xmlDoc.AppendChild(xmlDeclar);

                //加入Employees根元素
                XmlElement xmlElement = xmlDoc.CreateElement("", "Root", "");
                xmlDoc.AppendChild(xmlElement);

                XmlNode rootNode = xmlDoc.GetElementsByTagName("Root")[0];
                if (rootNode == null)
                    return;

                for (int nIndex = 0; nIndex < mProInfo.Count; ++nIndex)
                {
                    ProgramInfo mProgram = mProInfo[nIndex];
                    if (mProgram == null)
                        continue;

                    XmlElement xe1 = xmlDoc.CreateElement("Node");
                    xe1.SetAttribute("Path", mProgram.mstrPath);
                    xe1.SetAttribute("RunHour", mProgram.mnHours + "");
                    xe1.SetAttribute("RunMinus", mProgram.mnMinus + "");
                    xe1.SetAttribute("RunSeconds", mProgram.mnSeconds + "");
                    xe1.SetAttribute("LastRun", mProgram.szLastRun + "");
                    xe1.SetAttribute("ShowName", mProgram.szShowName + "");

                    rootNode.AppendChild(xe1);
                }

                xmlDoc.Save(strPath);//保存的路径
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(mCurPro != null)
            {
                ReName rn = new ReName();
                rn.Show();
            }
        }
    }
}
