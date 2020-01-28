﻿using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameNightSerialCommunications
{
    public partial class MainForm : Form
    {
        readonly SerialHandler sc = new SerialHandler();
        DateTime setDate;
        readonly string sessionFileLocation = Path.Combine(Directory.GetCurrentDirectory(), "currentSession.gns");
        readonly string defaultSaveLocation = Path.Combine(Directory.GetCurrentDirectory(), "savedSessions");
        Session session;

        public MainForm()
        {
            InitializeComponent();

            session = new Session();
            session.team1 = new Models.Team();
            session.team2 = new Models.Team();
            var ports = SerialPort.GetPortNames();
            cboTeam1.DataSource = ports;
            var ports2 = SerialPort.GetPortNames();
            cboTeam2.DataSource = ports2;
            setDate = DateTime.Now;
        }

        private void btnSendAll_Click(object sender, EventArgs e)
        {
            sc.SendToAll(txtCommand.Text);
            txtCommand.Text = "";
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            sc.CloseAllPorts();
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, SerialDataReceivedEventArgs>(serialPort1_DataReceived), new object[] { sender,e });
                return;
            }

            if (!String.IsNullOrEmpty(txtTimeSinceSet1.Text))
            {
                sc.sendMessageToTeam(serialTeam1, txtLastAnswerTeam1.Text.Split(':')[1].Substring(0,1));
                return;
            }
            TimeSpan span = DateTime.Now - setDate;
            int ms = (int)span.TotalMilliseconds;
            double seconds = Convert.ToDouble(ms) / 1000;
            txtTimeSinceSet1.Text = seconds.ToString() + " s.";
            if (String.IsNullOrEmpty(txtTimeSinceSet2.Text))
            {
                chkFastest1.Checked = true;
                sc.sendMessageToTeam(serialTeam1, "L:255");
                sc.sendMessageToTeam(serialTeam1, "S:200");
            }
            setDataReceived("TEAM1-" + serialTeam1.ReadLine());
        }

        private void serialTeam2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, SerialDataReceivedEventArgs>(serialTeam2_DataReceived), new object[] { sender, e });
                return;
            }
            if (!String.IsNullOrEmpty(txtTimeSinceSet2.Text))
            {
                sc.sendMessageToTeam(serialTeam2, txtLastAnswerTeam2.Text.Split(':')[1].Substring(0, 1));
                return;
            }
            TimeSpan span = DateTime.Now - setDate;
            int ms = (int)span.TotalMilliseconds;
            double seconds = Convert.ToDouble(ms)/1000;
            txtTimeSinceSet2.Text = seconds.ToString() + " s.";           if (String.IsNullOrEmpty(txtTimeSinceSet1.Text))
            {
                chkFastest2.Checked = true;
                sc.sendMessageToTeam(serialTeam2, "L:255");
                sc.sendMessageToTeam(serialTeam2, "S:200");
            }
            setDataReceived("TEAM2-" + serialTeam2.ReadLine());
        }

        private void setDataReceived(string dataReceived)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(setDataReceived), new object[] { dataReceived });
                return;
            }
            var team = dataReceived.Split('-');
            if (team[0] == "TEAM1")
            {
                txtLastAnswerTeam1.Text = team[1];

            }
            if (team[0] == "TEAM2")
            {
                txtLastAnswerTeam2.Text = team[1];
            }
        }

        private void btnFault1_Click(object sender, EventArgs e)
        {
            Task.Run(() => sc.SendFault(serialTeam1));
        }

        private void btnGood1_Click(object sender, EventArgs e)
        {
            Task.Run(() => sc.SendGood(serialTeam1));
        }

        private void btnSerial1Open_Click(object sender, EventArgs e)
        {
            if (!serialTeam1.IsOpen)
            {
                serialTeam1.PortName = cboTeam1.Text;
                if (serialTeam1.PortName == serialTeam2.PortName)
                {
                    MessageBox.Show("Kies een andere port, deze is al in gebruik");
                    return;
                }
                sc.OpenPort(serialTeam1);
                btnSerial1Open.Text = "Close";
                session.team1.comPortUsed = serialTeam1.PortName;
            }
            else
            {
                sc.ClosePort(serialTeam1);
                btnSerial1Open.Text = "Open";
                session.team1.comPortUsed = "";
            }
        }

        private void btnSerial2Open_Click(object sender, EventArgs e)
        {
            if (!serialTeam2.IsOpen)
            {
                serialTeam2.PortName = cboTeam2.Text;
                if (serialTeam1.PortName == serialTeam2.PortName)
                {
                    MessageBox.Show("Kies een andere port, deze is al in gebruik");
                    return;
                }
                sc.OpenPort(serialTeam2);
                btnSerial2Open.Text = "Close";
                session.team2.comPortUsed = serialTeam2.PortName;
            }
            else
            {
                sc.ClosePort(serialTeam2);
                btnSerial2Open.Text = "Open";
                session.team2.comPortUsed = "";
            }
        }



        private void btnFaultAll_Click(object sender, EventArgs e)
        {
            Task.Run(() => sc.SendFault(serialTeam1));
            Task.Run(() => sc.SendFault(serialTeam2));
        }

        private void btnGoodAll_Click(object sender, EventArgs e)
        {
            Task.Run(() => sc.SendGood(serialTeam1));
            Task.Run(() => sc.SendGood(serialTeam2));
        }

        private void btnFault2_Click(object sender, EventArgs e)
        {
            Task.Run(() => sc.SendFault(serialTeam2));
        }

        private void btnGood2_Click(object sender, EventArgs e)
        {
            Task.Run(() => sc.SendGood(serialTeam2));
        }

        private void btnSendScores_Click(object sender, EventArgs e)
        {
            sc.sendMessageToTeam(serialTeam1, numScore1.Value.ToString());
            sc.sendMessageToTeam(serialTeam2, numScore2.Value.ToString());
        }

        private void btnSend1_Click(object sender, EventArgs e)
        {
            sc.sendMessageToTeam(serialTeam1, txtCommand.Text);
        }

        private void btnSend2_Click(object sender, EventArgs e)
        {
            sc.sendMessageToTeam(serialTeam2, txtCommand.Text);
        }

        private void btnStartTimer_Click(object sender, EventArgs e)
        {
            resetAnswerFields();
            setDate = DateTime.Now;
        }

        private void btnAdd11_Click(object sender, EventArgs e)
        {
            numScore1.Value += 1;
        }

        private void btnAdd15_Click(object sender, EventArgs e)
        {
            numScore1.Value += 5;
        }

        private void btnAdd21_Click(object sender, EventArgs e)
        {
            numScore2.Value += 1;
        }

        private void btnAdd25_Click(object sender, EventArgs e)
        {
            numScore2.Value += 5;
        }

        void resetAnswerFields()
        {
            sc.SendToAll("");
            txtLastAnswerTeam1.Text = "";
            txtLastAnswerTeam2.Text = "";
            txtTimeSinceSet1.Text = "";
            txtTimeSinceSet2.Text = "";
            sc.SendToAll("L:0");
            chkFastest1.Checked = false;
            chkFastest2.Checked = false;
        }

        private void btnSaveSession_Click(object sender, EventArgs e)
        {
            if (!File.Exists(sessionFileLocation))
            {
                MessageBox.Show("Geen sessie gevonden, start eerst het spel voordat je een sessie kunt opslaan");
                return;
            }
            if (!Directory.Exists(defaultSaveLocation))
            {
                Directory.CreateDirectory(defaultSaveLocation);
            }
            if (sfdSaveSession.ShowDialog() == DialogResult.Yes)
            {
                // The data was already saved in a file, so we only have to copy it
                File.Copy(sessionFileLocation, sfdSaveSession.FileName, true);
            }
        }

        private void btnSessionLoad_Click(object sender, EventArgs e)
        {
            ofdSessionLoad.InitialDirectory = defaultSaveLocation;
            if (!Directory.Exists(defaultSaveLocation))
            {
                Directory.CreateDirectory(defaultSaveLocation);
            }
            if (ofdSessionLoad.ShowDialog() == DialogResult.Yes)
            {
                File.Copy(ofdSessionLoad.FileName, sessionFileLocation, true);
                reloadSession();
            }
        }

        private void reloadSession()
        {
            throw new NotImplementedException();
        }

        private void btnAdd12_Click(object sender, EventArgs e)
        {
            numScore1.Value += 2;
        }

        private void btnAdd13_Click(object sender, EventArgs e)
        {
            numScore1.Value += 3;
        }

        private void btnAdd14_Click(object sender, EventArgs e)
        {
            numScore1.Value += 4;
        }

        private void btnAdd22_Click(object sender, EventArgs e)
        {
            numScore2.Value += 2;
        }

        private void btnAdd23_Click(object sender, EventArgs e)
        {
            numScore2.Value += 3;
        }

        private void btnAdd24_Click(object sender, EventArgs e)
        {
            numScore2.Value += 4;
        }

        private void txtTeamName1_TextChanged(object sender, EventArgs e)
        {
            session.team1.teamName = txtTeamName1.Text;
        }

        private void txtTeamName2_TextChanged(object sender, EventArgs e)
        {
            session.team2.teamName = txtTeamName2.Text;
        }
    }
}
