using System;
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
            var removableDevices = KnownFolders.RemovableDevices;
            var externalDrives = await removableDevices.GetFoldersAsync();
            var drive0 = externalDrives[0];

            var testFolder = await drive0.CreateFolderAsync("RpiLogs");
            var testFile = await testFolder.CreateFileAsync("Logs.txt");
            await FileIO.AppendTextAsync(testFile,  "[" + DateTime.Now + "] " + message + System.Environment.NewLine);
        }
    }
}
