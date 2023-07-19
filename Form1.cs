using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using System;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows.Forms;

namespace NetPurifier;



public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {

        TextReader textomanic = new StreamReader(@"C:\Windows\System32\drivers\etc\hosts");
        richTextBox2.Text = textomanic.ReadToEnd();
        textomanic.Close();

    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            DialogResult result = MessageBox.Show("Shadowsocks Proxy will remain open even if you close this window. \n To close it, you need to press \"Turn Off\" button. \n Filters also will be still active until you set it to \"OFF\". \n Are you sure you want to close this application?", "Important Message", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {

                e.Cancel = true;
            }
        }

    }

    private void tabPage1_Click(object sender, EventArgs e)
    {

    }

    private void toolStripStatusLabel1_Click(object sender, EventArgs e)
    {

    }

    private void label2_Click(object sender, EventArgs e)
    {

    }

    private void label11_Click(object sender, EventArgs e)
    {

    }

    private void button5_Click(object sender, EventArgs e)
    {
        try
        {
            File.WriteAllText(@"C:\Windows\System32\drivers\etc\hosts", richTextBox2.Text);
            string message = "Successfully saved!";
            string title = "Hosts file successfully saved!";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Information);
        }
        catch (UnauthorizedAccessException)
        {
            string message = "Well, you need close your antivirus program, etc. and try again. Your antivirus program (probably) is blocking NetPurifier's actions.";
            string title = "Oops, something is blocking NetPurifier's activities";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);
        }
    }



    private void button6_Click_1(object sender, EventArgs e)
    {
        try
        {
            string destinationFilePath = @"C:\\Windows\\System32\\drivers\\etc\\netpurifierhosts.bak";
            File.WriteAllText(destinationFilePath, richTextBox2.Text);
            string message = "Backup successfully saved to C:\\Windows\\System32\\drivers\\etc\\netpurifierhosts.bak";
            string title = "Backup Successful";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Information);
        }
        catch (UnauthorizedAccessException)
        {
            string message = "Well, you need close your antivirus program, etc. and try again. Your antivirus program (probably) is blocking NetPurifier's actions.";
            string title = "Oops, something is blocking NetPurifier's activities";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Error);
        }
    }

    private void button7_Click(object sender, EventArgs e)
    {

        try
        {
            string originHosts = @"C:\\Windows\\System32\\drivers\\etc\\hosts";
            string backuphosts = @"C:\\Windows\\System32\\drivers\\etc\\netpurifierhosts.bak";
            string fileContent = File.ReadAllText(backuphosts);
            File.WriteAllText(originHosts, fileContent);
            string message = "Backup successfully loaded to C:\\Windows\\System32\\drivers\\etc\\hosts";
            string title = "Loading Backup Successful!";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Information);
            richTextBox2.Clear();
            TextReader textomanic = new StreamReader(@"C:\Windows\System32\drivers\etc\hosts");
            richTextBox2.Text = textomanic.ReadToEnd();
            textomanic.Close();
        }
        catch (UnauthorizedAccessException)
        {
            string message = "Well, you need close your antivirus program, etc. and try again. Your antivirus program (probably) is blocking NetPurifier's actions.";
            string title = "Oops, something is blocking NetPurifier's activities";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);
        }


    }

    private void button2_Click(object sender, EventArgs e)
    {
        richTextBox1.AppendText("Attempting to connect to a Shadowsocks server...\n");
        string ClientportNum = textBox6.Text;
        string hostname = serveriporhost.Text;
        string selectedValue = comboBox1.SelectedItem?.ToString();
        string encryptionManage;

        if (!string.IsNullOrEmpty(selectedValue))
        {
            switch (selectedValue)
            {
                case "none":
                    encryptionManage = "none";
                    break;
                case "plain":
                    encryptionManage = "plain";
                    break;
                case "aes-256-gcm":
                    encryptionManage = "aes-256-gcm";
                    break;
                case "aes-192-gcm":
                    encryptionManage = "aes-192-gcm";
                    break;
                case "aes-128-gcm":
                    encryptionManage = "aes-128-gcm";
                    break;
                case "chacha20-ietf-poly1305":
                    encryptionManage = "chacha20-ietf-poly1305";
                    break;
                case "xhacha20-ietf-poly1305":
                    encryptionManage = "xhacha20-ietf-poly1305";
                    break;
                default:
                    encryptionManage = "";
                    break;
            }
        }
        else
        {
            encryptionManage = "none";
        }

        string selectedVal2forprotocol = comboBox3.SelectedItem?.ToString();
        string protocoloption;

        if (!string.IsNullOrEmpty(selectedVal2forprotocol))
        {
            switch (selectedVal2forprotocol)
            {
                case "socks5":
                    protocoloption = "socks";
                    break;
                case "http (TCP/UDP)":
                    protocoloption = "http";
                    break;
                case "tunnel":
                    protocoloption = "tunnel";
                    break;
                default:
                    protocoloption = "socks";
                    break;
            }
        }
        else
        {
            protocoloption = "http";
        }
        string plugin = pluginprogram.Text;
        string password = serverpassword.Text;
        string serverpot = serverport.Text;
        string pluginProgram = pluginprogram.Text;
        string shadowUrl = shadowsocksurl.Text;
        string plugopts = plugoptions.Text;
        void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                // Handle the standard output data as needed
                string outputData = e.Data;
                // Update UI or perform any other necessary action
                Invoke(new Action(() => richTextBox1.AppendText(outputData + Environment.NewLine)));
            }
        }

        void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                // Handle the error output data as needed
                string errorData = e.Data;
                // Update UI or perform any other necessary action
                Invoke(new Action(() => richTextBox1.AppendText(errorData + Environment.NewLine)));
            }
        }

        async Task RunShadowsocksAsync()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.RedirectStandardError = true; // Enable capturing error output
            startInfo.RedirectStandardOutput = true; // Enable capturing standard output
            startInfo.UseShellExecute = false; // Required for redirecting error and standard output
            startInfo.CreateNoWindow = true; // Prevent the command prompt window from appearing

            Invoke(new Action(() => richTextBox1.AppendText("Attempting to run Shadowsocks with selected parameters...\n")));



            if (checkBox2.Checked == false && checkBox1.Checked == false)
            {
                Invoke(new Action(() => richTextBox1.AppendText("Manual Shadowsocks Configuration Mode ON (Without Plugin Support)...\n")));
                startInfo.Arguments = $"/c cd ss && sslocal.exe --local-addr \"127.0.0.1:{ClientportNum}\" --protocol \"{protocoloption}\" -s \"{hostname}:{serverpot}\" -m \"{encryptionManage}\" -k \"{password}\" ";
            }
            else if (checkBox2.Checked == true && checkBox1.Checked == false)
            {
                Invoke(new Action(() => richTextBox1.AppendText("Fast Shadowsocks Connection (URL) Mode\n")));
                startInfo.Arguments = $"/c cd ss && sslocal.exe --local-addr \"127.0.0.1:{ClientportNum}\" --protocol \"{protocoloption}\" --server-url \"{shadowUrl}\" ";

            }
            else if (checkBox2.Checked == true && checkBox1.Checked == true)
            {
                Invoke(new Action(() => richTextBox1.AppendText("Manual Shadowsocks Configuration Mode ON (With Plugin Support)...\n")));
                startInfo.Arguments = $"/c cd ss && sslocal.exe --local-addr \"127.0.0.1:{ClientportNum}\" --protocol \"{protocoloption}\" -s \"{hostname}:{serverpot}\" -m \"{encryptionManage}\" -k \"{password}\" --plugin \"{plugin}\" --plugin-opts \"{plugopts}\" ";
            }
            else
            {
                return;
            }

            // Replace placeholder variables with actual values
            startInfo.Arguments = startInfo.Arguments.Replace("{ClientportNum}", ClientportNum.ToString());
            startInfo.Arguments = startInfo.Arguments.Replace("{encryptionManage}", encryptionManage);
            startInfo.Arguments = startInfo.Arguments.Replace("{plugin}", plugin);
            startInfo.Arguments = startInfo.Arguments.Replace("{hostname}", hostname);
            startInfo.Arguments = startInfo.Arguments.Replace("{password}", password);
            startInfo.Arguments = startInfo.Arguments.Replace("{serverpot}", serverpot);
            startInfo.Arguments = startInfo.Arguments.Replace("{protocoloption}", protocoloption);
            startInfo.Arguments = startInfo.Arguments.Replace("{shadowurl}", shadowUrl);
            startInfo.Arguments = startInfo.Arguments.Replace("{plugopts}", plugopts);

            process.StartInfo = startInfo;

            // Event handlers for capturing standard output and error
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            // Start the process and begin asynchronous reading of output and error streams
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for the process to exit asynchronously
            await Task.Run(() => process.WaitForExit());
        }

        async Task StartShadowsocksAsync()
        {
            try
            {
                await RunShadowsocksAsync();
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    richTextBox1.AppendText("An error occurred while starting the Shadowsocks local server:\n");
                    richTextBox1.AppendText(ex.Message);
                }));
            }
        }

        async Task StartAsync()
        {
            await StartShadowsocksAsync();
        }

        StartAsync();


        void EnableSystemProxy()
        {
            string proxyServer = $"127.0.0.1:{ClientportNum}"; ; // Replace with your desired proxy server (e.g., "localhost:8888")

            try
            {
                // Open the registry key for Internet Settings
                using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true))
                {
                    // Set the proxy server address
                    registryKey.SetValue("ProxyServer", proxyServer);

                    // Enable the system proxy
                    registryKey.SetValue("ProxyEnable", 1);

                    // Refresh the proxy settings
                    registryKey.SetValue("ProxyOverride", "<local>");

                    // Notify the system about the changes
                    WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                    WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
                }

                richTextBox1.AppendText("System proxy enabled and set to localhost.\n");
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("Error enabling system proxy:\n " + ex.Message);
            }
        }

        // Usage example
        EnableSystemProxy();

    }

    private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
    {

    }

    private void textBox6_TextChanged(object sender, EventArgs e)
    {

    }

    private void serveriporhost_TextChanged(object sender, EventArgs e)
    {

    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void pluginprogram_TextChanged(object sender, EventArgs e)
    {

    }

    private void serverpassword_TextChanged(object sender, EventArgs e)
    {

    }

    private void serverport_TextChanged(object sender, EventArgs e)
    {

    }

    private void button3_Click(object sender, EventArgs e)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.RedirectStandardError = true; // Enable capturing error output
        startInfo.RedirectStandardOutput = true; // Enable capturing standard output
        startInfo.UseShellExecute = false; // Required for redirecting error and standard output
        startInfo.CreateNoWindow = true; // Prevent the command prompt window from appearing

        try
        {
            Invoke(new Action(() => richTextBox1.AppendText("Attempting to shutdown Shadowsocks...\n")));
            // Find the sslocal.exe processes
            Process[] sslocalProcesses = Process.GetProcessesByName("sslocal");

            // Terminate each sslocal.exe process
            foreach (Process sslocalProcess in sslocalProcesses)
            {
                sslocalProcess.Kill();
            }
            Invoke(new Action(() => richTextBox1.AppendText("Attempt to shutdown Shadowsocks is successful!\n")));
        }
        catch (Exception ex)
        {
            richTextBox1.AppendText("Error disabling system proxy:\n " + ex.Message);
        }



        void DisableSystemProxy()
        {
            try
            {
                // Open the registry key for Internet Settings
                using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true))
                {
                    // Disable the system proxy
                    registryKey.SetValue("ProxyEnable", 0);

                    // Clear the proxy server address
                    registryKey.DeleteValue("ProxyServer", false);

                    // Refresh the proxy settings
                    registryKey.SetValue("ProxyOverride", "<local>");

                    // Notify the system about the changes
                    WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                    WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
                }

                richTextBox1.AppendText("System proxy disabled.\n");
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("Error disabling system proxy:\n " + ex.Message);
            }
        }

        // Usage example
        DisableSystemProxy();
    }



    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBox1.Checked == true)
        {

            pluginprogram.Enabled = true;
            plugoptions.Enabled = true;

        }
        else
        {

            pluginprogram.Enabled = false;
            plugoptions.Enabled = false;

        }
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBox2.Checked == false)
        {

            shadowsocksurl.Enabled = false;
            savefasturl.Enabled = false;
            fastconnect.Enabled = false;

        }
        else
        {
            shadowsocksurl.Enabled = true;
            savefasturl.Enabled = true;
            fastconnect.Enabled = true;
        }
    }



    private void shadowsocksurl_TextChanged(object sender, EventArgs e)
    {

    }

    private void fastconnect_Click(object sender, EventArgs e)
    {
        button2_Click(sender, e);
    }

    private void trackBar1_Scroll(object sender, EventArgs e)
    {

    }

    private void button8_Click(object sender, EventArgs e)
    {
        string directoryPath = "filter-files";
        string fileName = string.Empty;

        // Get the currentFilterOption value from the trackbar
        int currentFilterOption = trackBar1.Value;

        // Map the currentFilterOption to the corresponding file name
        switch (currentFilterOption)
        {
            case 0:
                fileName = "default.bin";
                break;
            case 1:
                fileName = "light.bin";
                break;
            case 2:
                fileName = "moderate.bin";
                break;
            case 3:
                fileName = "strict.bin";
                break;
            case 4:
                fileName = "extrastrict.bin";
                break;
            case 5:
                fileName = "parents.bin";
                break;
            default:
                // Handle any invalid filter option
                break;
        }

        // Combine the directory path and file name to get the complete file path
        string filePath = Path.Combine(directoryPath, fileName);

        // Check if the file exists
        if (File.Exists(filePath))
        {
            string originHosts = @"C:\\Windows\\System32\\drivers\\etc\\hosts";
            string fileContent = File.ReadAllText(filePath);

            try
            {
                File.WriteAllText(originHosts, fileContent);
                string message = "Filter rules successfully imported to your hosts file!";
                string title = "Hosts file successfully saved!";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Information);
                richTextBox2.Clear();
                TextReader textomanic = new StreamReader(@"C:\Windows\System32\drivers\etc\hosts");
                richTextBox2.Text = textomanic.ReadToEnd();
                textomanic.Close();
            }
            catch (UnauthorizedAccessException)
            {
                string message = "Well, you need close your antivirus program, etc. and try again. Your antivirus program (probably) is blocking NetPurifier's actions.";
                string title = "Oops, something is blocking NetPurifier's activities";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);
            }
        }
        else
        {
            return;
        }


    }

    private void button9_Click(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {

    }

    private void gToolStripMenuItem_Click(object sender, EventArgs e)
    {

    }

    private void sSHOceanToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string url = "https://sshocean.com/shadowsocks";

        // Open the URL in the default browser
        Process.Start("explorer.exe",url);
    }

    private void iPRaceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string url = "https://www.racevpn.com/free-shadowsocks-server";

        // Open the URL in the default browser
        Process.Start("explorer.exe", url);
    }
}
// Dictionary to map trackbar values to filenames



class WinINetInterop
{
    public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
    public const int INTERNET_OPTION_REFRESH = 37;

    [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
}


