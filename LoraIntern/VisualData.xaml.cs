using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace LoraIntern
{
    public sealed partial class VisualData : Page
    {
        public int start = 0; //the first row to start iterate
        public int end = 30;  //the last row after iteration
        public int norows;    //total number of rows in the server

        SqlConnectionStringBuilder cb = new SqlConnectionStringBuilder();

        public VisualData()
        {
            this.InitializeComponent();
            readnumberofrows();
            LoadChartContents();
        }

        //variable class for Hank client
        public class dustRecords
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
        public class uvRecords
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
        public class tempRecords
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
        public class pressRecords
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
        public class humRecords
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
        public class RSSIRecords
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

        //variable class for LORA client
        public class dustRecords1
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
        public class uvRecords1
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
        public class tempRecords1
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
        public class pressRecords1
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
        public class humRecords1
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
        public class RSSIRecords1
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


        public void readnumberofrows()
        {
            string retrieve = "SELECT COUNT(*) FROM LORA_TABLE";

            cb.DataSource = "lorawan-hank.database.windows.net";
            cb.UserID = "Hank";
            cb.Password = "Lorawan1234";
            cb.InitialCatalog = "LoraWan Database";

            using (SqlConnection sqlConn = new SqlConnection(cb.ConnectionString))
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

        public void LoadChartContents()
        {
            string retrieve = String.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};",start,end*2);

            //list for client "HANK"
            List<dustRecords> dustrecords = new List<dustRecords>();
            List<uvRecords> uvrecords = new List<uvRecords>();
            List<tempRecords> temprecords = new List<tempRecords>();
            List<pressRecords> pressrecords = new List<pressRecords>();
            List<humRecords> humrecords = new List<humRecords>();
            List<RSSIRecords> RSSIrecords = new List<RSSIRecords>();

            //list for client "LORA"
            List<dustRecords1> dustrecords1 = new List<dustRecords1>();
            List<uvRecords1> uvrecords1 = new List<uvRecords1>();
            List<tempRecords1> temprecords1 = new List<tempRecords1>();
            List<pressRecords1> pressrecords1 = new List<pressRecords1>();
            List<humRecords1> humrecords1 = new List<humRecords1>();
            List<RSSIRecords1> RSSIrecords1 = new List<RSSIRecords1>();

            //build conenction string
            cb.DataSource = "lorawan-hank.database.windows.net";
            cb.UserID = "Hank";
            cb.Password = "Lorawan1234";
            cb.InitialCatalog = "LoraWan Database";

            using (SqlConnection sqlConn = new SqlConnection(cb.ConnectionString))
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
                            if (dustrecords.Count<31)
                            {
                                CurrentDate.Text = time.ToShortDateString();
                                if (reader.GetString(1) == "HANK")
                                {
                                    dustrecords.Add(new dustRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(4)
                                    });

                                    uvrecords.Add(new uvRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(5)
                                    });

                                    temprecords.Add(new tempRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(6)
                                    });

                                    pressrecords.Add(new pressRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(7)
                                    });

                                    humrecords.Add(new humRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(8)
                                    });

                                    RSSIrecords.Add(new RSSIRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(9)
                                    });
                                }
                                if (reader.GetString(1) == "LORA")
                                {
                                    dustrecords1.Add(new dustRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(4)
                                    });

                                    uvrecords1.Add(new uvRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(5)
                                    });

                                    temprecords1.Add(new tempRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(6)
                                    });

                                    pressrecords1.Add(new pressRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(7)
                                    });

                                    humrecords1.Add(new humRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(8)
                                    });

                                    RSSIrecords1.Add(new RSSIRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(9)
                                    });
                                }
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

        private async static void DisplaySqlErrors(SqlException exception)
        {
            for (int i = 0; i < exception.Errors.Count; i++)
            {
                MessageDialog popup = new MessageDialog("Index #" + i + "\n" +
                    "Error: " + exception.Errors[i].ToString() + "\n", "Wrong DateTime Format");
                await popup.ShowAsync();
            }
        }

        private void Connection_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoraIntern.MainPage));
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            readnumberofrows();
            if (end<norows)
            {
                start += 28;
                end += 28;
                LoadChartContents();
            }
            else
            {
                MessageDialog popup = new MessageDialog("No more data to load!");
                await popup.ShowAsync();
            }
        }

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            if (start>0)
            {
                start -= 28;
                end -= 28;
                LoadChartContents();
            }
            else
            {
                MessageDialog popup = new MessageDialog("You have reached the end!");
                await popup.ShowAsync();
            }
        }

        private async void SearchDate_Click(object sender, RoutedEventArgs e)
        {
            if (TypeDate.Text!=null)
            {
                try
                {
                    DateTime sqlDatetime = Convert.ToDateTime(TypeDate.Text);
                    //Debug.WriteLine(sqlDatetime.ToShortDateString(),"It worked!");
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

        private void findsqlDate(DateTime date)
        {
            start = 0;
            end = 30;
            object lastrow = 0;
            int counter = 0;

            string retrieve = String.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};", 0, norows);

            //list for client "HANK"
            List<dustRecords> dustrecords = new List<dustRecords>();
            List<uvRecords> uvrecords = new List<uvRecords>();
            List<tempRecords> temprecords = new List<tempRecords>();
            List<pressRecords> pressrecords = new List<pressRecords>();
            List<humRecords> humrecords = new List<humRecords>();
            List<RSSIRecords> RSSIrecords = new List<RSSIRecords>();

            //list for client "LORA"
            List<dustRecords1> dustrecords1 = new List<dustRecords1>();
            List<uvRecords1> uvrecords1 = new List<uvRecords1>();
            List<tempRecords1> temprecords1 = new List<tempRecords1>();
            List<pressRecords1> pressrecords1 = new List<pressRecords1>();
            List<humRecords1> humrecords1 = new List<humRecords1>();
            List<RSSIRecords1> RSSIrecords1 = new List<RSSIRecords1>();

            //build conenction string
            cb.DataSource = "lorawan-hank.database.windows.net";
            cb.UserID = "Hank";
            cb.Password = "Lorawan1234";
            cb.InitialCatalog = "LoraWan Database";

            using (SqlConnection sqlConn = new SqlConnection(cb.ConnectionString))
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
                                    dustrecords.Add(new dustRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(4)
                                    });

                                    uvrecords.Add(new uvRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(5)
                                    });

                                    temprecords.Add(new tempRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(6)
                                    });

                                    pressrecords.Add(new pressRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(7)
                                    });

                                    humrecords.Add(new humRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(8)
                                    });

                                    RSSIrecords.Add(new RSSIRecords()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(9)
                                    });
                                }
                                if (reader.GetString(1) == "LORA")
                                {
                                    dustrecords1.Add(new dustRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(4)
                                    });

                                    uvrecords1.Add(new uvRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(5)
                                    });

                                    temprecords1.Add(new tempRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(6)
                                    });

                                    pressrecords1.Add(new pressRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(7)
                                    });

                                    humrecords1.Add(new humRecords1()
                                    {
                                        Name = time,
                                        Amount = reader.GetValue(8)
                                    });

                                    RSSIrecords1.Add(new RSSIRecords1()
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
    }
}
