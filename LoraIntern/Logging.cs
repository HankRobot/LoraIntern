using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LightBuzz.SMTP;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Streams;

namespace LoraIntern
{
    class Logging
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

        public async void WriteLogs()
        {
            var removableDevices = KnownFolders.RemovableDevices;
            var externalDrives = await removableDevices.GetFoldersAsync();
            var drive0 = externalDrives[0];

            var testFolder = await drive0.CreateFolderAsync("Logs");
            var testFile = await testFolder.CreateFileAsync("Logs.txt");

            var byteArray = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
            using (var sourceStream = new MemoryStream(byteArray).AsRandomAccessStream())
            {
                using (var destinationStream = (await testFile.OpenAsync(FileAccessMode.ReadWrite)).GetOutputStreamAt(0))
                {
                    await RandomAccessStream.CopyAndCloseAsync(sourceStream, destinationStream);
                }
            }
        }
    }
}
