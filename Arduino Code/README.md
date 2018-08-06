# This is the Arduino Code for Lora Server and Client :laughing:
This codes are to be uploaded to your Arduino, and is not part of the C# project. Please do not mix them. (Read documentation before proceeding)

# Getting Started
1. Make sure you have attached the required sensors to the Arduino clients in the documentation, otherwise your clients won't work. 
2. Client_Sensor.ino and Client_Sensor1.ino have different ids for Lora, hence they are independent clients. rf95_server.ino is the code for the Arduino Server which will be attached to the Raspberry Pi 3B+.
3. Upload the codes via Arduino IDE to the respective arduinos, and check Serial Monitor, you should see them communicating.

# Build and Test
1. The clients are meant to sent sensor data every 5 minutes, but you can edit them for debugging purposes(at the last line there is a delay. adjust that to your needs)
2. Every transmission, the red LEDs will light up, if the leds are flashing, that means the server is not receiving the transmission and the clients are attempting to send per flash.
3. Make sure your Lora_ID is in CAPSLOCK and is different than other clients.

# Contribute (Will write this when I am finishing my internship)
1. 

#ONCE AGAIN, HAPPY CODING!!! :heart:
