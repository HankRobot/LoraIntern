using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;

namespace LoraIntern
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private SerialDevice serialPort = null;
        DataReader dataReaderObject = null;

        //set these to false if you are running on rpi
        public bool isdesktop = true;
        public bool ejectpendrive = true;

        //these are variables for displaying the data
        public string transmission { get; set; }
        public string id { get; set; }
        public string date { get; set; }
        public string dust { get; set; }
        public string uv { get; set; }
        public string temp { get; set; }
        public string press { get; set; }
        public string hum { get; set; }
        public string RSSI { get; set; }

        //these are variables for sending data to query
        public int setter;
        public double transn = 0;
        public SqlDateTime daten;
        public double dustn = 0;
        public double uvn = 0;
        public double tempn = 0;
        public double pressn = 0;
        public double humn = 0;
        public double RSSIn = 0;

        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;
        
        public MainPage()
        {
            this.InitializeComponent();
            if (!isdesktop)
            {
                ListAvailablePorts();
                Visualize.IsEnabled = false;
                listOfDevices = new ObservableCollection<DeviceInformation>();
            }
            ///raspberry pi takes forever to load datasets
            ///and also the serial write stops 
            ///its processing when u visualize data
        }

        /// <summary>
        /// ListAvailablePorts
        /// - Use SerialDevice.GetDeviceSelector to enumerate all serial devices
        /// - Attaches the DeviceInformation to the ListBox source so that DeviceIds are displayed
        /// </summary>
        private async void ListAvailablePorts()
        {
            try
            {
                if (!isdesktop)
                {
                    await GetLogging.EmailSendLogs("Lora Gateway has started.", string.Format("Rpi Started on {0}", DateTime.Now));
                    await GetLogging.WritetoTxtFile("Lora Gateway has started",ejectpendrive);
                }
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                status.Text = "Select a device and connect";

                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

                DeviceListSource.Source = listOfDevices;
                ConnectDevices.SelectedIndex = 1;            //change this to connect to the index of the connected devices in the "Select Device:" list 
                Start_Connection();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                if (!isdesktop)
                {
                    rpiicon.Source = new BitmapImage(new Uri("ms-appx:///Assets/rpidiscon.jpeg"));
                    await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway", status.Text);
                    await GetLogging.WritetoTxtFile(status.Text,ejectpendrive);

                    ReadRestart();
                }
            }
        }

        private async void Start_Connection()
        {
            var selection = ConnectDevices.SelectedItems;

            if (selection.Count <= 0)
            {
                status.Text = "Select a device and connect";
                return;
            }

            DeviceInformation entry = (DeviceInformation)selection[0];

            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);
                if (serialPort == null) return;

                // Configure serial settings
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;

                // Display configured settings
                status.Text = "Serial port configured successfully: ";
                status.Text += serialPort.BaudRate + "-";
                status.Text += serialPort.DataBits + "-";
                status.Text += serialPort.Parity.ToString() + "-";
                status.Text += serialPort.StopBits;

                // Set the RcvdText field to invoke the TextChanged callback
                // The callback launches an async Read task to wait for data
                rcvdText.Text = "Waiting for data...";

                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();

                // Enable 'WRITE' button to allow sending data

                Listen();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                if (!isdesktop)
                {
                    rpiicon.Source = new BitmapImage(new Uri("ms-appx:///Assets/rpidiscon.jpeg"));

                    await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway", status.Text + String.Format("\n{0}", rcvdText.Text));
                    await GetLogging.WritetoTxtFile(status.Text + String.Format("\n{0}", rcvdText.Text),ejectpendrive);

                    ReadRestart();
                }
            }
        }

        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                status.Text = "Reading task was cancelled, closing device and cleaning up";
                if (!isdesktop)
                {
                    rpiicon.Source = new BitmapImage(new Uri("ms-appx:///Assets/rpidiscon.jpeg"));
                    disconnectgif.Visibility = Visibility.Visible;
                    connectgif.Visibility = Visibility.Collapsed;
                    await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway", status.Text);
                    await GetLogging.WritetoTxtFile(status.Text,ejectpendrive);

                    ReadRestart();
                }
                CloseDevice();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                if (!isdesktop)
                {
                    rpiicon.Source = new BitmapImage(new Uri("ms-appx:///Assets/rpidiscon.jpeg"));
                    disconnectgif.Visibility = Visibility.Visible;
                    connectgif.Visibility = Visibility.Collapsed;

                    await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway", status.Text + String.Format("\n{0}",rcvdText.Text));
                    await GetLogging.WritetoTxtFile(status.Text + String.Format("\n{0}", rcvdText.Text),ejectpendrive);

                    ReadRestart();
                }
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Create a task object to wait for data on the serialPort.InputStream
                loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);

                // Launch the task and wait
                UInt32 bytesRead = await loadAsyncTask;
                if (bytesRead > 0)
                {
                    connectionring.IsActive = true;
                    status.Text = "Reading Bytes...";
                    sqlstatus.Text = "Sending Data To Server...";
                    // Starts string manipulation to receive the infos
                    rcvdText.Text = dataReaderObject.ReadString(bytesRead);
                    string received = rcvdText.Text;
                    string[] transmissions = received.Split("No.",StringSplitOptions.RemoveEmptyEntries);

                    foreach (var word in transmissions)
                    {
                        Debug.WriteLine(word);
                        transmission = word.Remove(word.IndexOf('f') - 1);
                        id = word.Remove(word.IndexOf('d') - 1).Remove(0, word.IndexOf('m') + 2);
                        date = word.Remove(word.IndexOf('k') - 5).Remove(0, word.IndexOf('m') + 2 + id.Length + 2);
                        dust = word.Remove(word.IndexOf('k') + 6).Remove(0, word.IndexOf('d') + date.Length + 1);
                        uv = word.Remove(word.IndexOf('w') + 6).Remove(0, word.IndexOf('k') + 6).Replace('w', 'W');
                        temp = word.Remove(word.LastIndexOf('c') + 1).Remove(0, word.IndexOf('b') + 1).Replace("c", "°C");
                        press = word.Remove(word.IndexOf('p') + 2).Remove(0, word.LastIndexOf('c') + 2).Replace('p', 'P');
                        hum = word.Remove(word.IndexOf('%') + 1).Remove(0, word.IndexOf('p') + 3);
                        RSSI = word.Remove(0, word.IndexOf('%') + 2).Replace('m', ' ');

                        //this counter is to verify the number of transmission is changing and transmission is true for the sql to upload data
                        setter = Int32.Parse(transmission);

                        //Update all the received infos to the labels
                        transmissiont.Text = transmission;
                        idt.Text = id;
                        datet.Text = date;
                        dustt.Text = dust;
                        uvt.Text = uv;
                        tempt.Text = temp;
                        presst.Text = press;
                        humt.Text = hum;
                        RSSIt.Text = RSSI;

                        //convert all of them to numbers so I can upload them to SQL
                        transn = setter;
                        // id is a string, no need change number
                        daten = Convert.ToDateTime(date);

                        dustn = Convert.ToDouble(dust.Remove(dust.Length - 6));
                        uvn = Convert.ToDouble(uv.Remove(uv.Length - 7));
                        tempn = Convert.ToDouble(temp.Remove(temp.Length - 2));
                        pressn = Convert.ToDouble(press.Remove(press.Length - 2));
                        humn = Convert.ToDouble(hum.Remove(hum.Length - 1));
                        RSSIn = Convert.ToDouble(RSSI.Remove(RSSI.Length - 1));

                        status.Text = "Bytes read successfully!";

                        //Add a pendrive into the rpi before running this code, otherwise an exception will be thrown
                        if (!isdesktop)
                        {
                            USBLabel.Text = "Writing Logs";
                            await GetLogging.WritetoTxtFile("Sent Data: "+ transmission + " " + id + " " + date + " " + dust + " " + uv + " " + temp + " " + press + " " + hum + " " + RSSI, ejectpendrive);
                            USBLabel.Text = "Logs Written";
                        }
                        
                        SendQuerytoSql();
                        

                        connectionring.IsActive = false;
                        disconnectgif.Visibility = Visibility.Collapsed;
                        connectgif.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// CloseDevice:
        /// - Disposes SerialDevice object
        /// - Clears the enumerated device Id list
        /// </summary>
        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;

            rcvdText.Text = "";
            listOfDevices.Clear();
        }

        //send sensor data to sql server
        private async void SendQuerytoSql()
        {
            //for testign
            // INSERT INTO LORA_TABLE (ID, Trans , TimeSubmit, Dust, UV, Temp, Pressure, Humidity, Altitude) VALUES ('HANK',102,'2018/7/11 4:00',0.00,0.12,28.89,10000.67,56.78,43.26);
            string sendQuery = String.Format(
                "INSERT INTO LORA_TABLE (ID, Trans , TimeSubmit, Dust, UV, Temp, Pressure, Humidity, Altitude) " +
                "VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8});"
                ,("'"+id+"'"),transn,"'"+daten+"'",dustn,uvn,tempn,pressn,humn,RSSIn);

            if (!isdesktop)
            {
                await GetLogging.WritetoTxtFile("SQLQuery:" + sendQuery, ejectpendrive);
            }

            SqlConnectionStringBuilder sql = LoraSQLConnect.ConnectionString();

            using (SqlConnection sqlConn = new SqlConnection(sql.ConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(sendQuery, sqlConn);          //Place your query here, not the sqlConn
                try
                {
                    sqlConn.Open();
                    sqlCommand.ExecuteNonQuery();
                    sqlstatus.Text = sqlConn.State.ToString();
                    if (sqlstatus.Text == "Open")
                    {
                        sqlstatus.Text += ",Data Sent!";
                    }
                }
                catch (SqlException ex)
                {
                    LoraSQLConnect.DisplaySqlErrors(ex,isdesktop);
                    for (int i = 0; i < ex.Errors.Count; i++)
                    {
                        await GetLogging.WritetoTxtFile("Index #" + i + "\n" +
                            "Error: " + ex.Errors[i].ToString() + "\n", ejectpendrive);
                        await GetLogging.EmailSendLogs("SQL Status Exception on Lora Rpi Gateway", "Index #" + i + "\n" +
                            "Error: " + ex.Errors[i].ToString() + "\n");
                    }
                    sqlstatus.Text = "Disconnected.";
                    ReadRestart();
                }
                sqlConn.Close();
            }
        }
        
        //go to next page
        private void Visualize_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VisualData));
        }

        //ejects pendrive
        private void Eject_Click(object sender, RoutedEventArgs e)
        {
            if (ejectpendrive)
            {
                ejectpendrive = false;
                USBLabel.Text = "USB logging enabled, make sure you have a pendrive plugged in";
                Eject.Content = "Eject Pendrive";
            }
            else
            {
                ejectpendrive = true;
                USBLabel.Text = "You can now safely remove your pendrive";
                Eject.Content = "Mount Pendrive";
            }
        }

        private void ReadRestart()
        {
            CancelReadTask();
            CloseDevice();
            ListAvailablePorts();
            listOfDevices = new ObservableCollection<DeviceInformation>();
        }
    }
}
