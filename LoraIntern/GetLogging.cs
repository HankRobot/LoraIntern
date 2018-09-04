using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using LightBuzz.SMTP;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace LoraIntern
{
    public class GetLogging
    {
        public async static Task EmailSendLogs(string title,string message)
        {
            try
            {
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();

                using (SmtpClient client = new SmtpClient("smtp-mail.outlook.com", 587, false, "thc1n17@soton.ac.uk", "loYhuilam0323"))
                {
                    EmailMessage emailMessage = new EmailMessage();

                    emailMessage.To.Add(new EmailRecipient("thc1n17@soton.ac.uk"));
                    emailMessage.Subject = title;
                    emailMessage.Body = message;
                    emailMessage.Importance = EmailImportance.High;

                    await client.SendMailAsync(emailMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "Email Error Status");
            }
        }

        public async static Task WritetoTxtFile(string message,bool ejectstatus)
        {
            if (ejectstatus)
            {
                int i = 0;
                StorageFolder externalDevices = KnownFolders.RemovableDevices;
                IReadOnlyList<StorageFolder> externalDrives = await externalDevices.GetFoldersAsync();
                StorageFolder drive0 = externalDrives[i];

                while (drive0.Path.Contains("E:") && i < externalDrives.Count)
                {
                    i++;
                    drive0 = externalDrives[i];
                }

                var testFolder = await drive0.CreateFolderAsync("RpiLogs", CreationCollisionOption.OpenIfExists);
                var testFile = await testFolder.CreateFileAsync("Logs.txt", CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(testFile, "[" + DateTime.Now + "] " + message + System.Environment.NewLine);
            }
        }

        public async static Task DownloadCSV(string sqlquery)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("file style", new string[] { ".csv" });
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.SuggestedFileName = "LoraGateWayDatasets";
            StorageFile file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                string retrieve = sqlquery;
                //build connenction string

                SqlConnectionStringBuilder sql = LoraSQLConnect.ConnectionString();

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

                                string dataset = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                    reader.GetString(1), reader.GetValue(2), time, reader.GetValue(4), reader.GetValue(5),
                                    reader.GetValue(6), reader.GetValue(7), reader.GetValue(8), reader.GetValue(9));

                                await FileIO.AppendTextAsync(file, dataset + System.Environment.NewLine);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        LoraSQLConnect.DisplaySqlErrors(ex);
                    }
                    sqlConn.Close();

                    MessageDialog popup = new MessageDialog("Datasets downloaded", "Your CSV file is ready");
                    await popup.ShowAsync();
                }
            }
            else
            {
                MessageDialog popup = new MessageDialog("Dataset Download Aborted", "Dataset Download Aborted");
                await popup.ShowAsync();
            }
        }
    }
}
