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
        public int end = 11;  //the last row after iteration
        public int norows;    //total number of rows in the server

        SqlConnectionStringBuilder cb = new SqlConnectionStringBuilder();

        public VisualData()
        {
            this.InitializeComponent();
            readnumberofrows();
            LoadChartContents();
        }

        public class dustRecords
        {
            public string Name
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
            public string Name
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
            public string Name
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
            public string Name
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
            public string Name
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
        public class altRecords
        {
            public string Name
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
            string retrieve = String.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};",start,end);

            List<dustRecords> dustrecords = new List<dustRecords>();
            List<uvRecords> uvrecords = new List<uvRecords>();
            List<tempRecords> temprecords = new List<tempRecords>();
            List<pressRecords> pressrecords = new List<pressRecords>();
            List<humRecords> humrecords = new List<humRecords>();
            List<altRecords> altrecords = new List<altRecords>();

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
                            string time = reader.GetDateTime(3).ToShortDateString();
                            time = time.Remove(time.LastIndexOf("/")) + "(" + reader.GetDateTime(3).ToString("HH:mm")+")";

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

                            altrecords.Add(new altRecords()
                            {
                                Name = time,
                                Amount = reader.GetValue(9)
                            });
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
            (altitudeChart.Series[0] as LineSeries).ItemsSource = altrecords;

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
                start += 12;
                end += 12;
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
                start -= 12;
                end -= 12;
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
            end = 11;
            object lastrow = 0;
            int counter = 0;

            string retrieve = String.Format("select * from (select Row_Number() over (order by TIMESUBMIT) as RowIndex, * from LORA_TABLE) as Sub Where Sub.RowIndex >= {0} and Sub.RowIndex <= {1};", 0, norows);

            List<dustRecords> dustrecords = new List<dustRecords>();
            List<uvRecords> uvrecords = new List<uvRecords>();
            List<tempRecords> temprecords = new List<tempRecords>();
            List<pressRecords> pressrecords = new List<pressRecords>();
            List<humRecords> humrecords = new List<humRecords>();
            List<altRecords> altrecords = new List<altRecords>();

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
                            string time = reader.GetDateTime(3).ToShortDateString();
                            //Debug.WriteLine("Lul",time);
                            //Debug.WriteLine("lal", date.ToShortDateString());
                            time = time.Remove(time.LastIndexOf("/")) + "(" + reader.GetDateTime(3).ToString("HH:mm") + ")";
                            if (reader.GetDateTime(3)>=date && dustrecords.Count<13)
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

                                altrecords.Add(new altRecords()
                                {
                                    Name = time,
                                    Amount = reader.GetValue(9)
                                });
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
            (altitudeChart.Series[0] as LineSeries).ItemsSource = altrecords;
        }
    }
}
