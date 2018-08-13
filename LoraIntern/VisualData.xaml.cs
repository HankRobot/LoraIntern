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

        SqlConnectionStringBuilder sql = new SqlConnectionStringBuilder();

        public VisualData()
        {
            this.InitializeComponent();
            readnumberofrows();
            DateTime today = System.DateTime.Now.Date;
            Debug.WriteLine("This is the date today!",today.ToShortDateString());
            findsqlDate(today);
        }

        //variable and data type class for Lora Client
        public class SensorData
        {
            public DateTime Name
            {
                get;
                set;
            }

            public object Amount
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
            string retrieve = String.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};",start,end);

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
                            if (reader.GetString(1) == "HANK")
                                {
                                    dustrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(4)
                                    });

                                    uvrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(5)
                                    });

                                    temprecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(6)
                                    });

                                    pressrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(7)
                                    });

                                    humrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(8)
                                    });

                                    RSSIrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(9)
                                    });
                                }
                            if (reader.GetString(1) == "LORA")
                                {
                                    dustrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(4)
                                    });

                                    uvrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(5)
                                    });

                                    temprecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(6)
                                    });

                                    pressrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(7)
                                    });

                                    humrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(8)
                                    });

                                    RSSIrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(9)
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
        private async static void DisplaySqlErrors(SqlException exception)
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

        //function for searching 
        private async void SearchDate_Click(object sender, RoutedEventArgs e)
        {
            if (TypeDate.Text!=null)
            {
                try
                {
                    DateTime sqlDatetime = Convert.ToDateTime(TypeDate.Text);
                    Debug.WriteLine(sqlDatetime.ToShortDateString(),"It worked!");
                    findsqlDate(sqlDatetime);
                }
                catch (Exception ex)
                {

                    MessageDialog popup = new MessageDialog(ex.ToString(),"Wrong DateTime Format");
                    await popup.ShowAsync();
                }
            }
            else
            {
                MessageDialog popup = new MessageDialog("Please Enter a Date or Time", "No Entry");
                await popup.ShowAsync();
            }
        }

        //function for search date and retrieve sensor data from server
        private void findsqlDate(DateTime date)
        {
            start = 0;
            end = 59;
            object lastrow = 0;
            int counter = 0;

            //string retrieve = String.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};", 0, norows);
            string retrieve = String.Format("SET ROWCOUNT 60; select * from(select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) " +
                "as Sub where TimeSubmit between '{0}' and '{0} 23:59:59';", DateTime.Parse(date.ToShortDateString()).ToString("MM-dd-yyyy"));
            Debug.WriteLine("interesting", retrieve);
            
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
                            //Debug.WriteLine("Lul",time);
                            //Debug.WriteLine("lal", date.ToShortDateString());
                            //time = time.Remove(time.LastIndexOf("/")) + "(" + reader.GetDateTime(3).ToString("HH:mm") + ")";
                            if (reader.GetDateTime(3)>=date && dustrecords.Count<(31))
                            {
                                CurrentDate.Text = time.ToShortDateString();
                                if (reader.GetString(1) == "HANK")
                                {
                                    dustrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(4)
                                    });

                                    uvrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(5)
                                    });

                                    temprecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(6)
                                    });

                                    pressrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(7)
                                    });

                                    humrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(8)
                                    });

                                    RSSIrecords.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(9)
                                    });
                                }
                                if (reader.GetString(1) == "LORA")
                                {
                                    dustrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(4)
                                    });

                                    uvrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(5)
                                    });

                                    temprecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(6)
                                    });

                                    pressrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(7)
                                    });

                                    humrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(8)
                                    });

                                    RSSIrecords1.Add(new SensorData()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(9)
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
    }
}
