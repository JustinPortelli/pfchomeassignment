using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.Json;
using System.Web;
using WebApplication1.Models;
using System.Net;

namespace WebApplication1.DataAccess
{
    public class PubSubRepository
    {
        TopicName tn ;
        SubscriptionName sn;
        public PubSubRepository()
        {
            tn  = new TopicName("justinportellimsd63b", "HomeAssignmentTopic");  //A Queue/Topic will be created to hold the emails to be sent.  It will always have the same name DemoTopic, which you can change
            sn = new SubscriptionName("justinportellimsd63b", "HomeAssignmentSubscription");  //A Subscription will be created to hold which messages were read or not.  It will always have the same name DemoSubscription, which you can change
        }
        private Topic CreateGetTopic()
        {
            PublisherServiceApiClient client = PublisherServiceApiClient.Create();   //We check if Topic exists, if no we create it and return it
            TopicName tn = new TopicName("justinportellimsd63b", "HomeAssignmentTopic");
            try
            {
                 return client.GetTopic(tn);
            }
            catch
            {
                return client.CreateTopic(tn);
            }
          

        }

        /// <summary>
        /// Publish method: uploads a message to the queue
        /// </summary>
        /// <param name="p"></param>
        public void AddToEmailQueue(FileUpload fu) 
        {
            PublisherServiceApiClient client = PublisherServiceApiClient.Create();
            var t = CreateGetTopic();


            string serialized = JsonSerializer.Serialize(fu, typeof(FileUpload));
            string encryptedFile = KeyRepository.Encrypt(serialized);
            List<PubsubMessage> messagesToAddToQueue = new List<PubsubMessage>(); // the method takes a list, so you can upload more than 1 message/item/product at a time
            PubsubMessage msg = new PubsubMessage();
            msg.Data = ByteString.CopyFromUtf8(serialized);

            messagesToAddToQueue.Add(msg);


            client.Publish(t.TopicName, messagesToAddToQueue); //committing to queue

        }


        private Subscription CreateGetSubscription()
        {
            SubscriberServiceApiClient client = SubscriberServiceApiClient.Create();  //We check if Subscription exists, if no we create it and return it
 
            try
            {
               return client.GetSubscription(sn);
            }
            catch
            {
                return  client.CreateSubscription(sn, tn, null, 30);
            }

        }

        public void DownloadEmailFromQueueAndSend()
        {
            SubscriberServiceApiClient client = SubscriberServiceApiClient.Create();

            var s = CreateGetSubscription(); //This must be called before being able to read messages from Topic/Queue
            var pullResponse = client.Pull(s.SubscriptionName, true, 1); //Reading the message on top; You can read more than just 1 at a time
            if(pullResponse != null)
            {
                string toDeserialize = pullResponse.ReceivedMessages[0].Message.Data.ToStringUtf8(); //extracting the first message since in the previous line it was specified to read one at a time. if you decide to read more then a loop is needed
                string decryptedFile = KeyRepository.Decrypt(toDeserialize);
                FileUpload deserialized = JsonSerializer.Deserialize<FileUpload>(toDeserialize); //Deserializing since when we published it we serialized it

                MailMessage mm = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                mm.To.Add(deserialized.userEmail);
                mm.From = new MailAddress("noreplyjustinportelli@gmail.com");
                mm.Subject = "A new file has been shared with you";
                mm.IsBodyHtml = true;
                mm.Body = "New Message From";
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("noreplyjustinportelli@gmail.com", "noreply1234");

                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(mm);

                List<string> acksIds = new List<string>();
                acksIds.Add(pullResponse.ReceivedMessages[0].AckId);
                client.Acknowledge(s.SubscriptionName, acksIds.AsEnumerable());


                //SmtpClient mailserver = new SmtpClient("smtp.gmail.com", 587);
                //mailserver.Credentials =

                //mailserver.Send(mm);


                //Send Email with deserialized. Documentation: https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient?view=netframework-4.8
                /*
                 * 
                 *       MailMessage message = new MailMessage();  
                            SmtpClient smtp = new SmtpClient();  
                            message.From = new MailAddress("FromMailAddress");  
                            message.To.Add(new MailAddress("ToMailAddress"));  
                            message.Subject = "Test";  
                            message.IsBodyHtml = true; //to make message body as html  
                            message.Body = htmlString;  
                            smtp.Port = 587;  
                            smtp.Host = "smtp.gmail.com"; //for gmail host  
                            smtp.EnableSsl = true;  
                            smtp.UseDefaultCredentials = false;  
                            smtp.Credentials = new NetworkCredential("noreply-programmingforthecloud@gmail.com", "password");  

                            //go on google while you are logged in in this account > search for lesssecureapps > turn it to on

                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;  
                            smtp.Send(message);  
                 * 
                 *


                List<string> acksIds = new List<string>();
                acksIds.Add(pullResponse.ReceivedMessages[0].AckId); //after the email is sent successfully you acknolwedge the message so it is confirmed that it was processed
               
                client.Acknowledge(s.SubscriptionName, acksIds.AsEnumerable());*/
            }

        }
    }
}