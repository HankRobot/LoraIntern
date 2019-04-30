using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace LoraIntern
{
    class LoraSQLConnect
    {
        public static SqlConnectionStringBuilder ConnectionString()
        {
            SqlConnectionStringBuilder sql = new SqlConnectionStringBuilder();

            /*
            sql.DataSource = "loraintern.database.windows.net";
            sql.UserID = "lora";
            sql.Password = "intern1234@";
            sql.InitialCatalog = "LoraIntern";
            */

            sql.DataSource = "lorashp.database.windows.net";
            sql.UserID = "shp";
            sql.Password = "Loraintern1234";
            sql.InitialCatalog = "lorashp";

            return sql;
        }

        public static Tuple<List<SensorDatas>,List<SensorDatas>,int,object,string> GetLoraDatabaseData(string retrieve, 
            bool sqlsearchdate, DateTime date,int counter, object lastrow)
        {
            DateTime time = DateTime.Now.Date;

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
            SqlConnectionStringBuilder sql = ConnectionString();

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
                            time = reader.GetDateTime(3);
                            if (reader.GetDateTime(3) >= date && dustrecords.Count < (31))
                            {
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
                    DisplaySqlErrors(ex,true);
                }
                sqlConn.Close();
            }
            List<SensorDatas> hankrecords = new List<SensorDatas>();
            List<SensorDatas> lorarecords = new List<SensorDatas>();

            hankrecords.Add(new SensorDatas()
            {
                dust = dustrecords,
                uv = uvrecords,
                temperature = temprecords,
                pressure = pressrecords,
                humidity = humrecords,
                rssi = RSSIrecords
            });

            lorarecords.Add(new SensorDatas()
            {
                dust = dustrecords1,
                uv = uvrecords1,
                temperature = temprecords1,
                pressure = pressrecords1,
                humidity = humrecords1,
                rssi = RSSIrecords1
            });

            return Tuple.Create(hankrecords,lorarecords,counter,lastrow,time.ToShortDateString());;
        }

        //display sql errors and display it on screen
        public async static void DisplaySqlErrors(SqlException exception,bool isdesktop)
        {
            if (isdesktop)
            {
                for (int i = 0; i < exception.Errors.Count; i++)
                {
                    MessageDialog popup = new MessageDialog("Index #" + i + "\n" +
                        "Error: " + exception.Errors[i].ToString() + "\n", "Wrong DateTime Format");
                    await popup.ShowAsync();
                }
            }
        }

        //variable and data type class for Lora Client
        public class SensorDatas
        {
            public List<SensorData> dust
            {
                get;
                set;
            }

            public List<SensorData> uv
            {
                get;
                set;
            }

            public List<SensorData> temperature
            {
                get;
                set;
            }

            public List<SensorData> pressure
            {
                get;
                set;
            }

            public List<SensorData> humidity
            {
                get;
                set;
            }

            public List<SensorData> rssi
            {
                get;
                set;
            }
        }
        
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
    }
}
