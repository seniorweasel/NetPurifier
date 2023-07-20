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
using System.Text.Json;
using System.Text.Json.Serialization;
using static NetPurifier.Form1;
using System.Net;

namespace NetPurifier;



public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
        toolStripProgressBar1.Minimum = 0;
        toolStripProgressBar1.Maximum = 5;
        toolStripProgressBar1.Value = 0;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        LoadHostnameToListBox();
        LoadConfigValues();
        TextReader textomanic = new StreamReader(@"C:\Windows\System32\drivers\etc\hosts");
        richTextBox2.Text = textomanic.ReadToEnd();
        textomanic.Close();


        // Reset the progress bar and status label
        toolStripProgressBar1.Value = 0;
        toolStripProgressBar1.Maximum = 104; // Assuming you are downloading 5 files
        toolStripStatusLabel1.Text = "Downloading Update...";

        // Create the 'lists' directory if it doesn't exist
        string listsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filter-files");
        Directory.CreateDirectory(listsDirectory);

        // Start downloading the files
         DownloadFileAndUpdateProgressBar("light.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/light.bin");
         DownloadFileAndUpdateProgressBar("moderate.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/moderate.bin");
         DownloadFileAndUpdateProgressBar("strict.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/strict.bin");
         DownloadFileAndUpdateProgressBar("extrastrict.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/extrastrict.bin");
         DownloadFileAndUpdateProgressBar("parents.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/parents.bin");

        toolStripStatusLabel1.Text = "Download completed.";
    }
    private ConfigWrapper configWrapper;

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

    private void LoadHostnameToListBox()
    {
        string configPath = Path.Combine("config", "userConnectionConf.json");

        if (File.Exists(configPath))
        {
            try
            {
                string jsonString = File.ReadAllText(configPath);

                // Deserialize the JSON data into a wrapper object containing the entries property
                configWrapper = JsonSerializer.Deserialize<ConfigWrapper>(jsonString);

                // Check if the wrapper object and the entries list are not null
                if (configWrapper != null && configWrapper.entries != null && configWrapper.entries.Count > 0)
                {
                    // Clear the existing items in listBox1
                    listBox1.Items.Clear();

                    foreach (ConfigEntry entry in configWrapper.entries)
                    {
                        // Add each hostname to listBox1
                        listBox1.Items.Add(entry.hostname);
                    }
                }
                else
                {
                    richTextBox1.AppendText("No config entries found.\n");
                }
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log the error or show a message to the user
                richTextBox1.AppendText($"Error loading hostnames: {ex.Message}\n");
            }
        }
        else
        {
            richTextBox1.AppendText("Config file not found.\n");
        }
    }

    private void LoadConfigFromFile()
    {
        string configPath = Path.Combine("config", "userConnectionConf.json");

        if (File.Exists(configPath))
        {
            try
            {
                string jsonString = File.ReadAllText(configPath);

                // Deserialize the JSON data into a wrapper object containing the entries property
                ConfigWrapper configWrapper = JsonSerializer.Deserialize<ConfigWrapper>(jsonString);

                // Check if the wrapper object and the entries list are not null
                if (configWrapper != null && configWrapper.entries != null && configWrapper.entries.Count > 0)
                {
                    // Clear the existing items in configEntries list
                    configEntries.Clear();

                    // Add each entry to configEntries list
                    configEntries.AddRange(configWrapper.entries);
                }
                else
                {
                    richTextBox1.AppendText("No config entries found.\n");
                }
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log the error or show a message to the user
                richTextBox1.AppendText($"Error loading config entries: {ex.Message}\n");
            }
        }
        else
        {
            richTextBox1.AppendText("Config file not found.\n");
        }
    }



    // Helper class to represent the JSON wrapper with "entries" property
    public class ConfigWrapper
    {
        public List<ConfigEntry> entries { get; set; }
    }

    // List to hold the ConfigEntry objects
    private List<ConfigEntry> configEntries = new List<ConfigEntry>();
    private string configPath = "config/userConnectionConf.json"; // Update this path accordingly

    // Method to save the configuration entries to the JSON file
    private void SaveConfigToFile()
    {
        string configPath = Path.Combine("config", "userConnectionConf.json");

        try
        {
            // Create a wrapper object containing the entries list
            ConfigWrapper configWrapper = new ConfigWrapper { entries = configEntries };

            // Serialize the wrapper object to JSON
            string jsonString = JsonSerializer.Serialize(configWrapper, new JsonSerializerOptions { WriteIndented = true });

            // Write the JSON data to the file
            File.WriteAllText(configPath, jsonString);
        }
        catch (Exception ex)
        {
            // Handle the exception, e.g., log the error or show a message to the user
            richTextBox1.AppendText($"Error saving config file: {ex.Message}\n");
        }
    }

   
    public class ConfigEntry
    {
        [JsonIgnore]
        public int ClientportNum { get; set; }
        public string protocoloption { get; set; }
        public string hostname { get; set; }
        public int serverpot { get; set; }
        public string encryptionManage { get; set; }
        public string password { get; set; }
        public string plugin { get; set; }
        public string plugopts { get; set; }
        public string shadowUrl { get; set; }
    }

    private void LoadConfigValues()
    {
        string configPath = Path.Combine("config", "userConnectionConf.json");

        if (File.Exists(configPath) && listBox1.SelectedItem != null)
        {
            try
            {
                string jsonString = File.ReadAllText(configPath);
                List<ConfigEntry> configEntries = JsonSerializer.Deserialize<List<ConfigEntry>>(jsonString);

                string selectedHostname = listBox1.SelectedItem.ToString();
                ConfigEntry selectedEntry = configEntries.Find(entry => entry.hostname == selectedHostname);

                if (selectedEntry != null)
                {

                    comboBox3.Text = selectedEntry.protocoloption;
                    serveriporhost.Text = selectedEntry.hostname;
                    serverport.Text = selectedEntry.serverpot.ToString();
                    comboBox1.Text = selectedEntry.encryptionManage;
                    serverpassword.Text = selectedEntry.password;
                    pluginprogram.Text = selectedEntry.plugin;
                    plugoptions.Text = selectedEntry.plugopts;
                    shadowsocksurl.Text = selectedEntry.shadowUrl;
                }
                else
                {
                    richTextBox1.AppendText($"Config entry not found for hostname: {selectedHostname}. Creating a new entry...\n");


                }
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., log the error or show a message to the user
                richTextBox1.AppendText($"Error loading/configuring values: {ex.Message}\n");
            }
        }
        else
        {
            richTextBox1.AppendText("Config file not found\n");
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
                Invoke(new Action(() => richTextBox1.AppendText("Note: If you're trying to connect manually and it says invalid SIP002, please uncheck the \"Fast Shadowsocks Connection\" below.\n")));

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

            fastconnect.Enabled = false;

        }
        else
        {
            shadowsocksurl.Enabled = true;

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
                string title = "Oops, something went wrong!";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);
            }
        }
        else
        {
            string message = "We could not find the filter files. Please click \"Update Files\" and try again!";
            string title = "Oops, something went wrong!";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);
            return;
        }


    }

    private void button9_Click(object sender, EventArgs e)
    {

    }

    private long totalBytesDownloaded = 0;

    private async void button1_Click(object sender, EventArgs e)
    {
        // Reset the progress bar and status label
        toolStripProgressBar1.Value = 0;
        toolStripProgressBar1.Maximum = 5 * 100; // Assuming you are downloading 5 files
        toolStripStatusLabel1.Text = "Downloading...";

        // Create the 'lists' directory if it doesn't exist
        string listsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filter-files");
        Directory.CreateDirectory(listsDirectory);

        // Start downloading the files
        await DownloadFileAndUpdateProgressBar("light.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/light.bin");
        await DownloadFileAndUpdateProgressBar("moderate.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/moderate.bin");
        await DownloadFileAndUpdateProgressBar("strict.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/strict.bin");
        await DownloadFileAndUpdateProgressBar("extrastrict.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/extrastrict.bin");
        await DownloadFileAndUpdateProgressBar("parents.bin", "https://raw.githubusercontent.com/seniorweasel/netpurifier-updaterepository/main/filter-files/parents.bin");

        toolStripStatusLabel1.Text = "Download completed.";
        
    }

    private async Task DownloadFileAndUpdateProgressBar(string fileName, string url)
    {
        using (var webClient = new WebClient())
        {
            string downloadPath = Path.Combine("filter-files", fileName);

            // Initialize the total bytes to receive
            long totalBytesToReceive = 0;

            // Download progress event handler
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                // Update the ToolStripProgressBar with the progress
                toolStripProgressBar1.Value = e.ProgressPercentage;
                toolStripStatusLabel1.Text = $"Downloading {fileName}... ({FormatBytes(e.BytesReceived)}/{FormatBytes(totalBytesToReceive)})";
            };

            // Download completed event handler
            webClient.DownloadFileCompleted += (sender, e) =>
            {
                if (!e.Cancelled && e.Error == null)
                {
                    // Update the total downloaded bytes
                    totalBytesDownloaded += totalBytesToReceive;

                    // Update the status label with the actual file size
                    long fileSize = new FileInfo(downloadPath).Length;
                    toolStripStatusLabel1.Text = $"Downloaded {fileName} ({FormatBytes(fileSize)})";

                    // Update the ToolStripProgressBar value
                    toolStripProgressBar1.Value++;
                }
                else
                {
                    // Handle download error or cancellation
                    toolStripStatusLabel1.Text = $"Error downloading {fileName}";
                }
            };

            // Get the content length of the file
            using (var httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    totalBytesToReceive = response.Content.Headers.ContentLength ?? 0;
                }
                catch (HttpRequestException ex)
                {
                    toolStripStatusLabel1.Text = $"Error getting content length for {fileName}: {ex.Message}";
                    return;
                }
            }

            // Start the asynchronous download
            await webClient.DownloadFileTaskAsync(new Uri(url), downloadPath);
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int i;
        double dblSByte = bytes;
        for (i = 0; i < suffixes.Length && bytes >= 1024; i++, bytes /= 1024)
        {
            dblSByte = bytes / 1024.0;
        }

        return $"{dblSByte:0.##} {suffixes[i]}";
    }


    private void gToolStripMenuItem_Click(object sender, EventArgs e)
    {

    }

    private void sSHOceanToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string url = "https://sshocean.com/shadowsocks";

        // Open the URL in the default browser
        Process.Start("explorer.exe", url);
    }

    private void iPRaceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string url = "https://www.racevpn.com/free-shadowsocks-server";

        // Open the URL in the default browser
        Process.Start("explorer.exe", url);
    }

    private void label8_Click(object sender, EventArgs e)
    {

    }

    private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void saveConf_Click(object sender, EventArgs e)
    {
        // Create a new config entry
        ConfigEntry newEntry = new ConfigEntry
        {

            protocoloption = comboBox3.Text,
            hostname = serveriporhost.Text,
            serverpot = int.Parse(serverport.Text),
            encryptionManage = comboBox1.Text,
            password = serverpassword.Text,
            plugin = pluginprogram.Text,
            plugopts = plugoptions.Text,
            shadowUrl = shadowsocksurl.Text
        };

        // Load existing entries from the JSON file into the configEntries list
        LoadConfigFromFile();

        // Add the new entry to the list
        configEntries.Add(newEntry);

        // Save all configuration entries to JSON file
        SaveConfigToFile();

        // Reload the hostnames in listBox1
        LoadHostnameToListBox();

        richTextBox1.AppendText("New config entry created and saved.\n");
    }

    private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Get the selected item from the listBox1
        string selectedHostname = listBox1.SelectedItem?.ToString();

        // Find the corresponding entry in the config file
        ConfigEntry selectedEntry = configWrapper.entries.FirstOrDefault(entry => entry.hostname == selectedHostname);

        // Update the textboxes with the values from the selected entry
        if (selectedEntry != null)
        {

            comboBox3.Text = selectedEntry.protocoloption;
            serveriporhost.Text = selectedEntry.hostname;
            serverport.Text = selectedEntry.serverpot.ToString();
            comboBox1.Text = selectedEntry.encryptionManage;
            serverpassword.Text = selectedEntry.password;
            pluginprogram.Text = selectedEntry.plugin;
            plugoptions.Text = selectedEntry.plugopts;
            shadowsocksurl.Text = selectedEntry.shadowUrl;
        }
    }

    private void DeleteConfigurationButton_Click(object sender, EventArgs e)
    {
        if (listBox1.SelectedItem != null)
        {
            // Load existing entries from the JSON file into the configEntries list
            LoadConfigFromFile();

            // Get the selected hostname from listBox1
            string selectedHostname = listBox1.SelectedItem.ToString();

            // Find the configuration entry to delete
            ConfigEntry entryToDelete = configEntries.Find(entry => entry.hostname == selectedHostname);

            if (entryToDelete != null)
            {
                // Remove the entry from the list
                configEntries.Remove(entryToDelete);

                // Save the updated configuration entries to JSON file
                SaveConfigToFile();

                // Reload the hostnames in listBox1
                LoadHostnameToListBox();

                richTextBox1.AppendText($"Configuration for hostname '{selectedHostname}' deleted.\n");
            }
            else
            {
                richTextBox1.AppendText($"Configuration for hostname '{selectedHostname}' not found.\n");
            }
        }
        else
        {
            richTextBox1.AppendText("Please select a configuration to delete.\n");
        }
    }



    private void button4_Click(object sender, EventArgs e)
    {
        // Check if the hostname is empty, if so, set it to "A saved URL"
        if (string.IsNullOrWhiteSpace(serveriporhost.Text))
        {
            serveriporhost.Text = "A saved URL";
        }

        // Create a new config entry
        ConfigEntry newEntry = new ConfigEntry
        {
            protocoloption = comboBox3.Text,
            hostname = serveriporhost.Text,
            serverpot = 0,
            encryptionManage = comboBox1.Text,
            password = serverpassword.Text,
            plugin = pluginprogram.Text,
            plugopts = plugoptions.Text,
            shadowUrl = shadowsocksurl.Text
        };

        // Load existing entries from the JSON file into the configEntries list
        LoadConfigFromFile();

        // Add the new entry to the list
        configEntries.Add(newEntry);

        // Save all configuration entries to JSON file
        SaveConfigToFile();

        // Reload the hostnames in listBox1
        LoadHostnameToListBox();

        richTextBox1.AppendText("New config entry created and saved.\n");

        string message = "URL Saved. You can see it in \"Saved Proxies & Settings\" tab. \n Default protocol for shadowsocks URLs are http. Change it and save the url again if its a different thing.  ";
        string title = "URL Saved";
        MessageBoxButtons buttons = MessageBoxButtons.OK;
        DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Warning);


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