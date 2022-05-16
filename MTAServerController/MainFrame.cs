using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MTAServerController;
using MTA_SDK;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MetroFramework.Controls;

namespace MTAServerController
{
    public partial class MainFrame : MetroFramework.Forms.MetroForm
    {
        public static string latestMessage;
        public MainFrame()
        {
            InitializeComponent();
        }




        private void MainFrame_Load(object sender, EventArgs e)
        {
            getPlayers();
            getResources();
            timer1.Start();
            metroGrid2.CellClick += MetroGrid2_CellClick;
            metroGrid1.CellClick += MetroGrid1_CellClick;

            this.FormClosing += MainFrame_FormClosing;
        }

        private void MainFrame_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void MetroGrid1_CellClick(object sender, DataGridViewCellEventArgs e)
        {         
            if (e.ColumnIndex == metroGrid1.Columns[0].Index && e.RowIndex >= 0)
            {
                if (metroGrid1.Rows[e.RowIndex].Cells[0].Value == null ) { return; }
                StringBuilder sb = new StringBuilder();
                string username = metroGrid1.Rows[e.RowIndex].Cells[0].Value.ToString();


                string userInfos = LoginFrame.call("RemoteControl", "getUserInfos", new MTA_LuaArgs(username));

                Console.WriteLine(userInfos);


                userInfos = userInfos.Substring(0, userInfos.Length - 2);
                userInfos = userInfos.Substring(2);
                userInfos = userInfos.Replace("\\", "");


                var jsonArray = JRaw.Parse(userInfos);

                foreach (var users in jsonArray[0])
                {
                    sb.Append(users[0].ToString() + " " + users[1].ToString() + "\n");

                }


                MessageBox.Show(sb.ToString(), "User Infos", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void MetroGrid2_CellClick(object sender, DataGridViewCellEventArgs e)
        {      
            if (e.ColumnIndex == metroGrid2.Columns[1].Index && e.RowIndex >= 0)
            {
                if (Convert.ToBoolean(metroGrid2.Rows[e.RowIndex].Cells[1].EditedFormattedValue) == true)
                {
                    string value = LoginFrame.call("RemoteControl", "startStopResource", new MTA_LuaArgs(metroGrid2.Rows[e.RowIndex].Cells[0].Value, "stop"));
                    MessageBox.Show("Stopped " + metroGrid2.Rows[e.RowIndex].Cells[0].Value);
                }
                else 
                {
                    string value = LoginFrame.call("RemoteControl", "startStopResource", new MTA_LuaArgs(metroGrid2.Rows[e.RowIndex].Cells[0].Value, "start"));
                    MessageBox.Show("Started " + metroGrid2.Rows[e.RowIndex].Cells[0].Value);
                }            
            }
        }



        public void getPlayers()
        {         
            string players = LoginFrame.call("RemoteControl", "getPlayers", new MTA_LuaArgs(""));

            players = players.Substring(0, players.Length - 2);
            players = players.Substring(2);
            players = players.Replace("\\", "");

            // JArray jsonArray = JArray.Parse(players);
            var jsonArray = JRaw.Parse(players);

            foreach (var users in jsonArray[0])
            {
                if (users[0].ToString() != "")
                {
                    metroGrid1.Rows.Add(users[0].ToString());
                    metroComboBox1.Items.Add(users[0].ToString());
                    metroComboBox2.Items.Add(users[0].ToString());
                }               
            }
        }

        public void getResources()
        {
            string resources = LoginFrame.call("RemoteControl", "getResourcen", new MTA_LuaArgs(""));


            resources = resources.Substring(0, resources.Length - 2);
            resources = resources.Substring(2);
            resources = resources.Replace("\\", "");


            int counter = 0;
            var jsonArray = JRaw.Parse(resources);
            foreach (var item in jsonArray[0])
            {
                metroGrid2.Rows.Add(item[0].ToString());
                if (item[1].ToString() == "running")
                {
                    metroGrid2.Rows[counter].Cells[1].Value = true;
                }
                counter = counter+1;
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (metroTextBox1.Text == "")
            {
                MessageBox.Show("Bitte gib einen Text ein!");
            }
            else
            {

                if (metroComboBox1.SelectedItem == null || metroComboBox1.SelectedItem.ToString() == "ALL")
                {
                    LoginFrame.call("RemoteControl", "writeMessage", new MTA_LuaArgs(metroTextBox1.Text, "None"));
                }
                else
                {
                    LoginFrame.call("RemoteControl", "writeMessage", new MTA_LuaArgs(metroTextBox1.Text, metroComboBox1.SelectedItem.ToString()));
                }
            }
        }

        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            metroGrid1.Rows.Clear();
            metroGrid2.Rows.Clear();
            metroComboBox1.Items.Clear();
            metroComboBox2.Items.Clear();
            getPlayers();
            getResources();

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            string messages = LoginFrame.call("RemoteControl", "getChatMessages", new MTA_LuaArgs(""));

            messages = messages.Substring(0, messages.Length - 2);
            messages = messages.Substring(2);
            messages = messages.Replace("\\", "");

            int counter = 0;

            var jsonArray = JRaw.Parse(messages);

            foreach (var message in jsonArray[0])
            {
                counter = counter + 1;
            }

            if (counter <= 0) 
            {
                return;
            }

          //  Console.WriteLine("Hier: " + jsonArray[0][counter - 1][1].ToString());
            if (latestMessage == jsonArray[0][counter - 1][1].ToString())
            {
             //   Console.WriteLine("message is the same");
                return;
            }
            else
            {
                listBox1.Items.Add(jsonArray[0][counter - 1][0].ToString() + ": " + jsonArray[0][counter - 1][1].ToString());
                latestMessage = jsonArray[0][counter - 1][1].ToString();
            }           
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (metroComboBox2.SelectedItem == null)
            {
                MessageBox.Show("Select a user!");
            }
            else
            {
                string returned = LoginFrame.call("RemoteControl", "kickBanPlayer", new MTA_LuaArgs(metroComboBox2.SelectedItem, "ban", metroTextBox2.Text));
                if (returned == "[true]")
                {                 
                    MessageBox.Show("You banned user: " + metroComboBox2.SelectedItem + " with reason: " + metroTextBox2.Text, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
         
                    MessageBox.Show("Error!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
   
            }
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            if (metroComboBox2.SelectedItem == null)
            {
                MessageBox.Show("Select a user!");
            }
            else
            {
                string returned = LoginFrame.call("RemoteControl", "kickBanPlayer", new MTA_LuaArgs(metroComboBox2.SelectedItem,"kick",metroTextBox2.Text));
                if (returned == "[true]")
                {                
                    MessageBox.Show("You kicked user: " + metroComboBox2.SelectedItem + " with reason: " + metroTextBox2.Text, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Error!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }         
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            string bans = LoginFrame.call("RemoteControl", "getPlayerBans", new MTA_LuaArgs(""));

            if (bans == "[]") { MessageBox.Show("Ban list is empty!", "", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }


         
            bans = bans.Substring(0, bans.Length - 2);
            bans = bans.Substring(2);
            bans = bans.Replace("\\", "");

          
            if (bans == "[ [ ] ]")
            {            
                MessageBox.Show("Ban list is empty!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                var jsonArray = JRaw.Parse(bans);

                foreach (var item in jsonArray[0])
                {
                    listBox2.Items.Add(item[0].ToString() + "  *Reason: " + item[1].ToString());
                }
            }
        }

        private void metroButton5_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null)
            {
                MessageBox.Show("Please select a player that you wanna unban!");
            }
            else
            {
                var splitted = listBox2.SelectedItem.ToString().Split(Convert.ToChar("*"));
                string username = splitted[0].ToString().Replace(" ", "");
                string result = LoginFrame.call("RemoteControl", "unbanPlayer", new MTA_LuaArgs(username));
                if (result == "[true]")
                {          
                    MessageBox.Show("User: " + username + " got unbanned!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    listBox2.Items.Remove(listBox2.SelectedItem);
                }
                else
                {
                    MessageBox.Show("Error!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

