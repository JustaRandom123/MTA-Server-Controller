using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTA_SDK;
using Newtonsoft.Json.Linq;

namespace MTAServerController
{
    public partial class LoginFrame : MetroFramework.Forms.MetroForm
    {
        public static MTA server;

        public LoginFrame()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadPresets();
        }


        public static string refreshServerPresets(string presetName, string ip, string port, string username, string passwort)
        {
            string temp = @"{""username"":""#username#"",""ip"":""#ip#"",""port"":""#port#"",""passwort"":""#passwort#""}";
            string currentlist = MTAServerController.Properties.Settings.Default.presetsTable;
            JObject foodJsonObj = JObject.Parse(currentlist);
            foodJsonObj.Add(presetName, JObject.Parse(temp.Replace("#username#", username).Replace("#passwort#", passwort).Replace("#port#", port).Replace("#ip#", ip)));
            Console.WriteLine(foodJsonObj.ToString());
            return foodJsonObj.ToString();
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (metroComboBox1.SelectedItem != null && metroComboBox1.SelectedItem.ToString() != "None")
            {
                var data = getPresetCredentials(metroComboBox1.SelectedItem.ToString());             
                server = new MTA(data.Item1, Convert.ToInt32(data.Item2), data.Item3, data.Item4);
                if (call("RemoteControl", "connected", new MTA_LuaArgs("")) == "[true]")
                {          
                    MessageBox.Show("Connected!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();
                    MainFrame frmMain = new MainFrame();
                    frmMain.Text = JArray.Parse(call("RemoteControl", "servername", new MTA_LuaArgs("")))[0].ToString();
                    frmMain.Show();
                }
            }
            else
            {
                if (metroTextBox4.Text == "")
                {
                    MessageBox.Show("Bitte gib eine IP addresse an!");
                }
                else
                { }
                if (metroTextBox3.Text == "")
                {
                    MessageBox.Show("Bitte gib einen Port an!");
                }
                else
                {
                    if (metroTextBox2.Text == "")
                    {
                        MessageBox.Show("Bitte gib einen username an!");
                    }
                    else
                    {
                        if (metroTextBox1.Text == "")
                        {
                            MessageBox.Show("Bitte gib ein passwort ein!");
                        }
                        else
                        {
                            if (metroComboBox1.SelectedItem == null || metroComboBox1.SelectedItem.ToString() == "None")
                            {
                                server = new MTA(metroTextBox4.Text, Convert.ToInt32(metroTextBox3.Text), metroTextBox2.Text, metroTextBox1.Text);
                                if (call("RemoteControl", "connected", new MTA_LuaArgs("")) == "[true]")
                                {
                                    MessageBox.Show("Connected!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Hide();
                                    MainFrame frmMain = new MainFrame();
                                    frmMain.Text = JArray.Parse(call("RemoteControl", "servername", new MTA_LuaArgs("")))[0].ToString();
                                    frmMain.Show();
                                }
                            }
                            else
                            {
                                var data = getPresetCredentials(metroComboBox1.SelectedItem.ToString());                           

                                server = new MTA(data.Item1, Convert.ToInt32(data.Item2), data.Item3, data.Item4);
                                if (call("RemoteControl", "connected", new MTA_LuaArgs("")) == "[true]")
                                {
                                    MessageBox.Show("Connected!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Hide();
                                    MainFrame frmMain = new MainFrame();
                                    frmMain.Text = JArray.Parse(call("RemoteControl", "servername", new MTA_LuaArgs("")))[0].ToString();
                                    frmMain.Show();
                                }
                            }
                        }
                    }
                }
            }
        }

        public Tuple <string,string,string,string> getPresetCredentials(string presetname)
        {
            var jsonObject = JObject.Parse(MTAServerController.Properties.Settings.Default.presetsTable);
            string ip = String.Empty;
            string port = String.Empty;
            string passwort = String.Empty;
            string username = String.Empty;

            foreach (var root in jsonObject)
            {
                if (root.Key.ToString() == presetname)
                {
                    ip = root.Value["ip"].ToString();
                    port = root.Value["port"].ToString();
                    username = root.Value["username"].ToString();
                    passwort = root.Value["passwort"].ToString();
                    break;
                }
            }
            return Tuple.Create(ip,port,username,passwort);
        }



        public void loadPresets()
        {
            var jsonObject = JObject.Parse(MTAServerController.Properties.Settings.Default.presetsTable);
            foreach (var root in jsonObject)
            {
                string presetName = root.Key;              
                metroComboBox1.Items.Add(presetName);
            }
        }


        public void removePreset(string presetname)
        {
            string currentlist = MTAServerController.Properties.Settings.Default.presetsTable;
            JObject foodJsonObj = JObject.Parse(currentlist);
            foodJsonObj.Remove(presetname);
            MTAServerController.Properties.Settings.Default.presetsTable = foodJsonObj.ToString();
            MTAServerController.Properties.Settings.Default.Save();
        }


        public static string call(string resource,string functioname, MTA_LuaArgs args)
        {
            string returned = server.CallFunction(resource, functioname, args);
            Console.WriteLine(returned);
            return returned;
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            if (metroTextBox4.Text == "")
            {
                MessageBox.Show("Bitte gib eine IP addresse an!");
            }
            else
            {
                if (metroTextBox3.Text == "")
                {
                    MessageBox.Show("Bitte gib einen Port an!");
                }
                else
                {
                    if (metroTextBox2.Text == "")
                    {
                        MessageBox.Show("Bitte gib einen username an!");
                    }
                    else
                    {
                        if (metroTextBox1.Text == "")
                        {
                            MessageBox.Show("Bitte gib ein passwort ein!");
                        }
                        else
                        {
                            if (metroTextBox5.Text == "")
                            {
                                MessageBox.Show("Please enter a preset name");
                            }
                            else
                            {
                                MTAServerController.Properties.Settings.Default.presetsTable = refreshServerPresets(metroTextBox5.Text,metroTextBox4.Text, metroTextBox3.Text, metroTextBox2.Text, metroTextBox1.Text);
                                MTAServerController.Properties.Settings.Default.Save();
                                MessageBox.Show("Preset Saved!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // MessageBox.Show("Preset Saved!");
                                loadPresets();
                            }                         
                        }
                    }
                }
            }                             
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (metroComboBox1.SelectedItem == null || metroComboBox1.SelectedItem.ToString() == "None")
            {
                MessageBox.Show("Please select the preset that you want to remove!");
            }
            else
            {
                removePreset(metroComboBox1.SelectedItem.ToString());
                MessageBox.Show("You removed the preset called: " + metroComboBox1.SelectedItem.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //  MessageBox.Show("You removed the preset called: " + metroComboBox1.SelectedItem.ToString());
                metroComboBox1.Items.Clear();
                metroComboBox1.Items.Add("None");
                loadPresets();
            }
        }
    }
}
