using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace LoraIntern
{
    public sealed partial class VisualData : Page
    {
        public int start = 0; //the first row to start iterate
        public int end = 59;  //the last row after iteration
        public int norows;    //total number of rows in the server
        public DateTime empty;
        public bool filepicked;

        SqlConnectionStringBuilder sql = new SqlConnectionStringBuilder();

        public VisualData()
        {
            this.InitializeComponent();
            readnumberofrows();
            DateTime today = DateTime.Now.Date;
            Debug.WriteLine("This is the date today!",today.ToString(""));
            findsqlDate(today,empty,false);
        }

        //variable and data type class for Lora Client
        public class SensorData
        {
            public DateTime Time
            {
                get;
                set;
            }

            public object Data
            {
                get;
                set;
            }
        }

        //number of rows the SQL table currently has
        public void readnumberofrows()
        {
            string retrieve = "SELECT COUNT(*) FROM LORA_TABLE";

            sql.DataSource = "lorawan-hank.database.windows.net";
            sql.UserID = "Hank";
            sql.Password = "Lorawan1234";
            sql.InitialCatalog = "LoraWan Database";

            using (SqlConnection sqlConn = new SqlConnection(sql.ConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(retrieve, sqlConn);
                try
                {
                    sqlConn.Open();
                    sqlCommand.ExecuteNonQuery();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            norows = reader.GetInt32(0);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    DisplaySqlErrors(ex);
                }
            }
        }

        //load the graphs of sensor data collected
        public void LoadChartContents()
        {
            string retrieve = string.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};",start,end);

            //list for client "HANK"
            List<SensorData> dustrecords = new List<SensorData>();
            List<SensorData> uvrecords = new List<SensorData>();
            List<SensorData> temprecords = new List<SensorData>();
            List<SensorData> pressrecords = new List<SensorData>();
            List<SensorData> humrecords = new List<SensorData>();
            List<SensorData> RSSIrecords = new List<SensorData>();

            //list for client "LORA"
            List<SensorData> dustrecords1 = new List<SensorData>();
            List<SensorData> uvrecords1 = new List<SensorData>();
            List<SensorData> temprecords1 = new List<SensorData>();
            List<SensorData> pressrecords1 = new List<SensorData>();
            List<SensorData> humrecords1 = new List<SensorData>();
            List<SensorData> RSSIrecords1 = new List<SensorData>();
            
            //build conenction string
            sql.DataSource = "lorawan-hank.database.windows.net";
            sql.UserID = "Hank";
            sql.Password = "Lorawan1234";
            sql.InitialCatalog = "LoraWan Database";

            using (SqlConnection sqlConn = new SqlConnection(sql.ConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(retrieve, sqlConn);
                try
                {
                    sqlConn.Open();
                    sqlCommand.ExecuteNonQuery();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DateTime time = reader.GetDateTime(3);
                            CurrentDate.Text = time.ToShortDateString();
                            CurrentDate.Text = time.ToShortDateString();
                            if (reader.GetString(1) == "HANK")
                                {
                                    dustrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(4)
                                    });

                                    uvrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(5)
                                    });

                                    temprecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(6)
                                    });

                                    pressrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(7)
                                    });

                                    humrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(8)
                                    });

                                    RSSIrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(9)
                                    });
                                }
                            if (reader.GetString(1) == "LORA")
                                {
                                    dustrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(4)
                                    });

                                    uvrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(5)
                                    });

                                    temprecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(6)
                                    });

                                    pressrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(7)
                                    });

                                    humrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(8)
                                    });

                                    RSSIrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(9)
                                    });
                                }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    DisplaySqlErrors(ex);
                }
                sqlConn.Close();
            }
            
            (dustChart.Series[0] as LineSeries).ItemsSource = dustrecords;
            (uvChart.Series[0] as LineSeries).ItemsSource = uvrecords;
            (temperatureChart.Series[0] as LineSeries).ItemsSource = temprecords;
            (pressureChart.Series[0] as LineSeries).ItemsSource = pressrecords;
            (humidityChart.Series[0] as LineSeries).ItemsSource = humrecords;
            (RSSIChart.Series[0] as LineSeries).ItemsSource = RSSIrecords;

            (dustChart.Series[1] as LineSeries).ItemsSource = dustrecords1;
            (uvChart.Series[1] as LineSeries).ItemsSource = uvrecords1;
            (temperatureChart.Series[1] as LineSeries).ItemsSource = temprecords1;
            (pressureChart.Series[1] as LineSeries).ItemsSource = pressrecords1;
            (humidityChart.Series[1] as LineSeries).ItemsSource = humrecords1;
            (RSSIChart.Series[1] as LineSeries).ItemsSource = RSSIrecords1;

        }

        //display sql errors and display it on screen
        public async static void DisplaySqlErrors(SqlException exception)
        {
            for (int i = 0; i < exception.Errors.Count; i++)
            {
                MessageDialog popup = new MessageDialog("Index #" + i + "\n" +
                    "Error: " + exception.Errors[i].ToString() + "\n", "Wrong DateTime Format");
                await popup.ShowAsync();
            }
        }
        
        //function for next page button for next extra sets of sensor data to display
        //on graph
        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            readnumberofrows();
            if (end<norows)
            {
                start += 59;
                end += 59;
                LoadChartContents();
            }
            else
            {
                MessageDialog popup = new MessageDialog("No more data to load!");
                await popup.ShowAsync();
            }
        }

        //back page button
        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            if (start>0)
            {
                start -= 59;
                end -= 59;
                LoadChartContents();
            }
            else
            {
                MessageDialog popup = new MessageDialog("You have reached the end!");
                await popup.ShowAsync();
            }
        }

        //function for searching for date and time in the sql date
        private async void SearchDate_Click(object sender, RoutedEventArgs e)
        {
            dialog.Title = "Search for a DateTime";
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";

            await dialog.ShowAsync();

            var date = datepicker.Date;
            string time = timepicker.Time.ToString();
            string formateddate;

            if (date!=null && time!=null)
            {
                DateTime datesl = date.Value.DateTime;
                formateddate = datesl.ToString("dd/MM/yyyy");  //mm stands for minute, MM stands for month, beware
                if ((formateddate + " " + time) != "")
                {
                    try
                    {
                        DateTime sqlDatetime = Convert.ToDateTime(formateddate + " " + time);
                        Debug.WriteLine(sqlDatetime.ToString(), "It worked!");
                        findsqlDate(sqlDatetime, empty, false);
                    }
                    catch (Exception ex)
                    {
                        MessageDialog popup = new MessageDialog(ex.ToString(), "Wrong DateTime Format");
                        await popup.ShowAsync();
                    }
                }
            }
        }

        //function for search date and retrieve sensor data from server
        private void findsqlDate(DateTime date, DateTime date2, bool searchbetweendates)
        {
            start = 0;
            end = 59;
            object lastrow = 0;
            int counter = 0;

            string retrieve;

            if (searchbetweendates)
            {
                retrieve = String.Format("SET ROWCOUNT 60; select * from(select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) " +
                "as Sub where TimeSubmit >= '{0}' and <='{1}';", date.ToString("MM-dd-yyyy HH:mm:ss"),date2.ToString("MM-dd-yyyy HH:mm:ss"));
                Debug.WriteLine("interesting", retrieve);
            }
            else
            {
                //string retrieve = String.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};", 0, norows);
                retrieve = String.Format("SET ROWCOUNT 60; select * from(select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) " +
                    "as Sub where TimeSubmit >= '{0}';", date.ToString("MM-dd-yyyy HH:mm:ss"));
                Debug.WriteLine("interesting", retrieve);
            }
            
            
            //list for client "HANK"
            List<SensorData> dustrecords = new List<SensorData>();
            List<SensorData> uvrecords = new List<SensorData>();
            List<SensorData> temprecords = new List<SensorData>();
            List<SensorData> pressrecords = new List<SensorData>();
            List<SensorData> humrecords = new List<SensorData>();
            List<SensorData> RSSIrecords = new List<SensorData>();

            //list for client "LORA"
            List<SensorData> dustrecords1 = new List<SensorData>();
            List<SensorData> uvrecords1 = new List<SensorData>();
            List<SensorData> temprecords1 = new List<SensorData>();
            List<SensorData> pressrecords1 = new List<SensorData>();
            List<SensorData> humrecords1 = new List<SensorData>();
            List<SensorData> RSSIrecords1 = new List<SensorData>();

            //build conenction string
            sql.DataSource = "lorawan-hank.database.windows.net";
            sql.UserID = "Hank";
            sql.Password = "Lorawan1234";
            sql.InitialCatalog = "LoraWan Database";

            using (SqlConnection sqlConn = new SqlConnection(sql.ConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand(retrieve, sqlConn);
                try
                {
                    sqlConn.Open();
                    sqlCommand.ExecuteNonQuery();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DateTime time = reader.GetDateTime(3);
                            CurrentDate.Text = time.ToShortDateString();

                            if (reader.GetDateTime(3)>=date && dustrecords.Count<(31))
                            {
                                CurrentDate.Text = time.ToShortDateString();
                                if (reader.GetString(1) == "HANK")
                                {
                                    dustrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(4)
                                    });

                                    uvrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(5)
                                    });

                                    temprecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(6)
                                    });

                                    pressrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(7)
                                    });

                                    humrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(8)
                                    });

                                    RSSIrecords.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(9)
                                    });
                                }
                                if (reader.GetString(1) == "LORA")
                                {
                                    dustrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(4)
                                    });

                                    uvrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(5)
                                    });

                                    temprecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(6)
                                    });

                                    pressrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(7)
                                    });

                                    humrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(8)
                                    });

                                    RSSIrecords1.Add(new SensorData()
                                    {
                                        Time = time,
                                        Data = reader.GetValue(9)
                                    });
                                }

                            }
                            if (reader.GetDateTime(3) >= date)
                            {
                                counter += 1;
                                lastrow = reader.GetValue(0);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    DisplaySqlErrors(ex);
                }
                sqlConn.Close();
            }

            start += Convert.ToInt32(lastrow)-counter+1;
            end += Convert.ToInt32(lastrow)-counter+1;

            (dustChart.Series[0] as LineSeries).ItemsSource = dustrecords;
            (uvChart.Series[0] as LineSeries).ItemsSource = uvrecords;
            (temperatureChart.Series[0] as LineSeries).ItemsSource = temprecords;
            (pressureChart.Series[0] as LineSeries).ItemsSource = pressrecords;
            (humidityChart.Series[0] as LineSeries).ItemsSource = humrecords;
            (RSSIChart.Series[0] as LineSeries).ItemsSource = RSSIrecords;

            (dustChart.Series[1] as LineSeries).ItemsSource = dustrecords1;
            (uvChart.Series[1] as LineSeries).ItemsSource = uvrecords1;
            (temperatureChart.Series[1] as LineSeries).ItemsSource = temprecords1;
            (pressureChart.Series[1] as LineSeries).ItemsSource = pressrecords1;
            (humidityChart.Series[1] as LineSeries).ItemsSource = humrecords1;
            (RSSIChart.Series[1] as LineSeries).ItemsSource = RSSIrecords1;
        }

        //function for going back to main page
        private void Connection_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoraIntern.MainPage));
        }

        //function for the save to csv file
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            ProgressSave.IsActive = true;

            MessageDialog dialog = new MessageDialog("Saving Lora Datasets...Press ESC to cancel");
            dialog.Title = "Saving Lora Datasets...";
            dialog.Commands.Add(new UICommand("Download This Page Only", null));
            dialog.Commands.Add(new UICommand("Download Between Dates", null));
            dialog.Commands.Add(new UICommand("Download the entire database", null));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            var cmd = await dialog.ShowAsync();

            if (cmd.Label == "Download This Page Only")
            {
                string retrieve = string.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};", start, end);
                await GetLogging.DownloadCSV(retrieve);
            }

            if (cmd.Label == "Download the entire database")
            {
                string retrieve = string.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};", 0, norows);
                await GetLogging.DownloadCSV(retrieve);
            }

            if (cmd.Label == "Download Between Dates")
            {
                dialogsave.Title = "Download between Dates";
                
                dialogsave.IsSecondaryButtonEnabled = true;
                dialogsave.PrimaryButtonText = "Ok";
                dialogsave.SecondaryButtonText = "Cancel";

                await dialogsave.ShowAsync();

                var fdate = fromdate.Date;
                var tdate = todate.Date;

                if (fdate!=null && tdate!=null)
                {
                    DateTime datesl = fdate.Value.DateTime;
                    string ffdate = datesl.ToString("dd/MM/yyyy");  //mm stands for minute, MM stands for month, beware
                    
                    DateTime dates2 = tdate.Value.DateTime;
                    string ttdate = dates2.ToString("dd/MM/yyyy");  //mm stands for minute, MM stands for month, beware
                    
                    if (ffdate != "" && ttdate != "")
                    {
                        try
                        {
                            DateTime date1 = Convert.ToDateTime(ffdate);
                            DateTime date2 = Convert.ToDateTime(ttdate);
                            Debug.WriteLine(date1, "It worked!1");
                            Debug.WriteLine(date2, "It worked!2");

                            string retrieve = string.Format("select * from(select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) " +
                                                            "as Sub where TimeSubmit >= '{0}' and TimeSubmit <='{1}';",
                                                            date1.ToString("MM-dd-yyyy HH:mm:ss"), date2.ToString("MM-dd-yyyy HH:mm:ss"));

                            await GetLogging.DownloadCSV(retrieve);
                        }
                        catch (Exception ex)
                        {
                            MessageDialog popup = new MessageDialog(ex.ToString(), "Wrong DateTime Format");
                            await popup.ShowAsync();
                        }
                    }
                }
            }

            ProgressSave.IsActive = false;
        }
    }
}
