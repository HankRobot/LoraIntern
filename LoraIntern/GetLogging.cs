using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LightBuzz.SMTP;
using Windows.ApplicationModel.Email;
using Windows.Storage;

namespace LoraIntern
{
    class GetLogging
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

        public async static Task WritetoTxtFile(string message)
        {
            int i = 0;
            StorageFolder externalDevices = KnownFolders.RemovableDevices;
            IReadOnlyList<StorageFolder> externalDrives = await externalDevices.GetFoldersAsync();
            StorageFolder drive0 = externalDrives[i];

            while (drive0.Path.Contains("E:") && i<externalDrives.Count)
            {
                i++;
                drive0 = externalDrives[i];
            }

            var testFolder = await drive0.CreateFolderAsync("RpiLogs", CreationCollisionOption.OpenIfExists);
            var testFile = await testFolder.CreateFileAsync("Logs.txt", CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(testFile,  "[" + DateTime.Now + "] " + message + System.Environment.NewLine);
        }
    }
}
