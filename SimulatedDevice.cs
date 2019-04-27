// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace simulated_device
{
    class SimulatedDevice
    {
        private static DeviceClient s_deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private readonly static string s_connectionString = "HostName=ImageHub.azure-devices.net;DeviceId=VasundharasDevice;SharedAccessKey=X1hnIA893PGve0zk0dq/GzJ4AzSknMBGUnTcnCCYGoA=";

        // Async method to send simulated telemetry
        private static async void SendDeviceToCloudMessagesAsync()
        {
            // Initial telemetry values
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                // Create JSON message
                var telemetryDataPoint = new
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the telemetry message
                await s_deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000);
            }
        }

        private static async void SendUploadDetailsInEvent(string msg)
        {
            Console.WriteLine("sending event to iot hub for cosmos");
            Random rand = new Random();
            // Create JSON message
            var testData = new
            {
                imgURI = msg,
                deviceId = rand.NextDouble()
            };
            var messageString = JsonConvert.SerializeObject(testData);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            // Add a custom application property to the message.
            // An IoT hub can filter on these properties without access to the message body.
            //message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

            // Send the telemetry message
            await s_deviceClient.SendEventAsync(message);
            Console.WriteLine("{0} > Sending message to cloud: {1}", DateTime.Now, messageString);

        }

        private static async void ReceiveC2dAsync()
		{
			Console.WriteLine("\nReceiving cloud to device messages from service");
			while (true)
			{
				 Message receivedMessage = await s_deviceClient.ReceiveAsync();
				 if (receivedMessage == null) continue;

				 Console.ForegroundColor = ConsoleColor.Yellow;

                string msg = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                 Console.WriteLine("Received message: {0}", msg);
				 Console.ResetColor();
                
                await s_deviceClient.CompleteAsync(receivedMessage);

                SendUploadDetailsInEvent(msg);
            }
		}

        private static async void SendToBlobAsync()
        {
            string fileName = "image1.jpg";
            Console.WriteLine("Uploading file: {0}", fileName);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (var sourceData = new FileStream(@"image.jpg", FileMode.Open))
            {
                await s_deviceClient.UploadToBlobAsync(fileName, sourceData);
            }

            watch.Stop();
            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("IoT Hub Quickstarts #1 - Simulated device. Ctrl-C to exit.\n");

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, TransportType.Mqtt);
           // SendDeviceToCloudMessagesAsync();
			
            SendToBlobAsync();

            ReceiveC2dAsync();

            
            Console.ReadLine();
        }
    }
}
