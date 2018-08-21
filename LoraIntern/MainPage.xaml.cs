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

namespace LoraIntern
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private SerialDevice serialPort = null;
        DataReader dataReaderObject = null;

        //set this to false if you are running on rpi
        public bool isdesktop = false;

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

        SqlConnectionStringBuilder sql = new SqlConnectionStringBuilder();

        public MainPage()
        {
            this.InitializeComponent();
            comPortInput.IsEnabled = false;
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();

            ///Visualize.IsEnabled = false;
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
                    await GetLogging.EmailSendLogs("Lora Gateway has started.", String.Format("Rpi Started on {0}", DateTime.Now));
                    GetLogging.WriteLogs("This is a test");
                }
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                status.Text = "Select a device and connect";

                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

                DeviceListSource.Source = listOfDevices;
                comPortInput.IsEnabled = true;
                ConnectDevices.SelectedIndex = 1;            //change this to connect to the index of the connected devices in the "Select Device:" list 
                start_connection();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                if (!isdesktop)
                {
                    await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway, the Rpi will try restarting the App", status.Text);
                    await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");      
                }
            }
        }

        /// <summary>
        /// comPortInput_Click: Action to take when 'Connect' button is clicked
        /// - Get the selected device index and use Id to create the SerialDevice object
        /// - Configure default settings for the serial port
        /// - Create the ReadCancellationTokenSource token
        /// - Start listening on the serial port input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void comPortInput_Click(object sender, RoutedEventArgs e)
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

                // Disable the 'Connect' button 
                comPortInput.IsEnabled = false;

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
                comPortInput.IsEnabled = true;
                GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway, the Rpi will try restarting the App", status.Text);
                await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");
            }
        }

        private async void start_connection()
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

                // Disable the 'Connect' button 
                comPortInput.IsEnabled = false;

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
                comPortInput.IsEnabled = true;
                if (!isdesktop)
                {
                    await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway, the Rpi will try restarting the App", status.Text + String.Format("\n{0}", rcvdText.Text));
                    await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");
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
                    await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway", status.Text);
                }
                CloseDevice();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                if (!isdesktop)
                {
                    await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway, the Rpi will try restarting the App", status.Text + String.Format("\n{0}",rcvdText.Text));
                    await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");
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

                        status.Text = "bytes read successfully!";
                        sendQuerytoSql();
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

            comPortInput.IsEnabled = true;
            rcvdText.Text = "";
            listOfDevices.Clear();
        }

        /// <summary>
        /// closeDevice_Click: Action to take when 'Disconnect and Refresh List' is clicked on
        /// - Cancel all read operations
        /// - Close and dispose the SerialDevice object
        /// - Enumerate connected devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void closeDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                status.Text = "";
                CancelReadTask();
                CloseDevice();
                ListAvailablePorts();
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                if (!isdesktop)
                {
                    Console.WriteLine("bool");
                    await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway, the Rpi will try restarting the App", status.Text);
                    await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");
                }
            }
        }

        //send sensor data to sql server
        private void sendQuerytoSql()
        {
            //for testign
            // INSERT INTO LORA_TABLE (ID, Trans , TimeSubmit, Dust, UV, Temp, Pressure, Humidity, Altitude) VALUES ('HANK',102,'2018/7/11 4:00',0.00,0.12,28.89,10000.67,56.78,43.26);
            string sendQuery = String.Format(
                "INSERT INTO LORA_TABLE (ID, Trans , TimeSubmit, Dust, UV, Temp, Pressure, Humidity, Altitude) " +
                "VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8});"
                ,("'"+id+"'"),transn,"'"+daten+"'",dustn,uvn,tempn,pressn,humn,RSSIn);

            Debug.WriteLine(sendQuery,"This is the test for query");

            sql.DataSource = "lorawan-hank.database.windows.net";
            sql.UserID = "Hank";
            sql.Password = "Lorawan1234";
            sql.InitialCatalog = "LoraWan Database";

            using (SqlConnection sqlConn = new SqlConnection(sql.ConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(sendQuery, sqlConn);          //Place your query here, not the sqlConn
                try
                {
                    sqlConn.Open();
                    sqlCommand.ExecuteNonQuery();
                    label1.Text = sqlConn.State.ToString();
                }
                catch (SqlException ex)
                {
                    DisplaySqlErrors(ex);
                }
            }
        }

        //This is for catching sql errors in case they show up :)
        private async void DisplaySqlErrors(SqlException exception)
        {
            for (int i = 0; i < exception.Errors.Count; i++)
            {
                label1.Text += "Index #" + i + "\n" +
                    "Error: " + exception.Errors[i].ToString() + "\n";
                Console.WriteLine("Index #" + i + "\n" +
                    "Error: " + exception.Errors[i].ToString() + "\n");
            }
            if (!isdesktop)
            {
                await GetLogging.EmailSendLogs("Status Exception on Lora Rpi Gateway, the Rpi will try restarting the App", label1.Text);
                await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("-fastInit -level 1 -foo");
            }
        }

        //go to next page
        private void Visualize_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoraIntern.VisualData));
        }
    }
}
