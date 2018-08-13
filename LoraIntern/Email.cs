﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightBuzz.SMTP;
using Windows.ApplicationModel.Email;

namespace LoraIntern
{
    class Email
    {
        public async static void EmailSend(string title,string message)
        {
            try
            {
                using (SmtpClient client = new SmtpClient("smtp-mail.outlook.com", 587, false, "thc1n17@soton.ac.uk", "loYhuilam0323"))
                {
                    EmailMessage emailMessage = new EmailMessage();

                    emailMessage.To.Add(new EmailRecipient("thc1n17@soton.ac.uk"));
                    emailMessage.Subject = title;
                    emailMessage.Body = message;

                    await client.SendMailAsync(emailMessage);
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex, "Email Error Status");
            }
        }
    }
}