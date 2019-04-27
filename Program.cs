using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
namespace ReadFileUploadNotification
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString = "HostName=ImageHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=oJiZcgM9UYQD/jbTnk7xq3VrdOIgY/6ZDR9C7cVDw1Y=";

        private async static void ReceiveFileUploadNotificationAsync()
        {
            var notificationReceiver = serviceClient.GetFileNotificationReceiver();

            Console.WriteLine("\nReceiving file upload notification from service");
            while (true)
            {
                var fileUploadNotification = await notificationReceiver.ReceiveAsync();
                if (fileUploadNotification == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received file upload notification: {0}",
                  string.Join(", ", new String[] { fileUploadNotification.BlobName, fileUploadNotification.BlobUri, fileUploadNotification.DeviceId }));

                Console.WriteLine("\nSending notification to device");

                //var commandMessage = new
                // Message(Encoding.ASCII.GetBytes("Cloud to device message that file is uploaded"));

                var dataToSend = new
                {
                    imgURI = fileUploadNotification.BlobUri,
                    deviceId = fileUploadNotification.DeviceId,
                    location = new
                    {
                        line1 = "100 Linn Street",
                        line2 = "unit 1",
                        city = "Seattle",
                        state = "WA",
                        zip = "98012"
                    }
                };
                var messageString = JsonConvert.SerializeObject(dataToSend);
                var commandMessage = new Message(Encoding.ASCII.GetBytes(messageString));
                Console.WriteLine("Sending {0}", commandMessage);
                await serviceClient.SendAsync("VasundharasDevice", commandMessage);

                Console.ResetColor();

                await notificationReceiver.CompleteAsync(fileUploadNotification);
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Receive file upload notifications\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            ReceiveFileUploadNotificationAsync();
            Console.WriteLine("Press Enter to exit\n");
            Console.ReadLine();
        }
    }
}
