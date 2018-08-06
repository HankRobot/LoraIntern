// LoRa 9x_TX
// -*- mode: C++ -*-
// Example sketch showing how to create a simple messaging client (transmitter)
// with the RH_RF95 class. RH_RF95 class does not provide for addressing or
// reliability, so you should only use RH_RF95 if you do not need the higher
// level messaging abilities.
// It is designed to work with the other example LoRa9x_RX

//RTC modules
#include <Wire.h>
#include "RTClib.h"

//Environmental sensor Modules
#include <DFRobot_BME280.h>
#define SEA_LEVEL_PRESSURE  1013.25f
#define BME_CS 10

//Lora Modules
#include <SPI.h>
#include <RH_RF95.h>
 
#define RFM95_CS 10
#define RFM95_RST 7
#define RFM95_INT 2
#define node_id "HANK"   //change the name of this if u wanna add new clients
 
// Change to 434.0 or other frequency, must match RX's freq!
#define RF95_FREQ 915.0
 
// Singleton instance of the radio driver
RH_RF95 rf95(RFM95_CS, RFM95_INT);
int16_t packetnum = 0;  // packet counter, we increment per xmission

//.............................................................................Sensor variables.......................................................................................//

//UV sensor variables
int ReadUVintensityPin = A0; //Output from the UV sensor 
String UVreading;

//Dust sensor
int measurePin = A2;
int ledPower = 12;

unsigned int samplingTime = 280;
unsigned int deltaTime = 40;
unsigned int sleepTime = 9680;

float voMeasured = 0;
float calcVoltage = 0;
float dustDensity = 0;

String dustreading;

//RTC sensor
RTC_DS1307 rtc;
char daysOfTheWeek[7][12] = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};
String rtcreading;

//Environment sensor
DFRobot_BME280 bme; //I2C
float temp, pa, hum, alt;
String BMEreading;

void setup() 
{
  //LED indicator
  pinMode(3,OUTPUT);
//.............................................................................setup for sensor........................................................................................//
  // UV sensor setup
  pinMode(ReadUVintensityPin, INPUT);
  
  // Dust sensor setup
  pinMode(ledPower,OUTPUT);

  // RTC sensor setup
  if (! rtc.begin()) {
    Serial.println("Couldn't find RTC");
    while (1);
  }
  
  if (! rtc.isrunning()) {
    Serial.println("RTC is NOT running!");
    // following line sets the RTC to the date & time this sketch was compiled
    rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));
    // This line sets the RTC with an explicit date & time, for example to set
    // January 21, 2014 at 3am you would call:
    //rtc.adjust(DateTime(2018, 7, 26, 10, 30, 0));
  }
  //Environemental sensor setup
  if (!bme.begin()) {
        Serial.println("No sensor device found, check line or address!");
        while (1);
    }
    
    Serial.println("-- BME280 DEMO --");

  //Lora setup
  pinMode(RFM95_RST, OUTPUT);
  digitalWrite(RFM95_RST, HIGH);
 
  while (!Serial);
  Serial.begin(9600);
  delay(100);
 
  Serial.println("Arduino LoRa TX Test!");
 
  // manual reset
  digitalWrite(RFM95_RST, LOW);
  delay(10);
  digitalWrite(RFM95_RST, HIGH);
  delay(10);
 
  while (!rf95.init()) {
    Serial.println("LoRa radio init failed");
    while (1);
  }
  Serial.println("LoRa radio init OK!");
 
  // Defaults after init are 434.0MHz, modulation GFSK_Rb250Fd250, +13dbM
  if (!rf95.setFrequency(RF95_FREQ)) {
    Serial.println("setFrequency failed");
    while (1);
  }
  Serial.print("Set Freq to: "); Serial.println(RF95_FREQ);
  
  // Defaults after init are 434.0MHz, 13dBm, Bw = 125 kHz, Cr = 4/5, Sf = 128chips/symbol, CRC on
 
  // The default transmitter power is 13dBm, using PA_BOOST.
  // If you are using RFM95/96/97/98 modules which uses the PA_BOOST transmitter pin, then 
  // you can set transmitter powers from 5 to 23 dBm:
  rf95.setTxPower(23, false);
}

//................................................................................functions for UV reading values.............................................................................

//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||UV sensor||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
void UVread()
{
  int uvLevel = averageAnalogRead(ReadUVintensityPin);
  float outputVoltage = 5.0 * uvLevel/1024;
  float uvIntensity = mapfloat(outputVoltage, 0.99, 2.9, 0.0, 15.0);   //mW/cm^2
  UVreading = String(uvIntensity) + "mw/cm^2";
}

int averageAnalogRead(int pinToRead)
{
  byte numberOfReadings = 8;
  unsigned int runningValue = 0; 

  for(int x = 0 ; x < numberOfReadings ; x++)
    runningValue += analogRead(pinToRead);
  runningValue /= numberOfReadings;

  return(runningValue);  

}

float mapfloat(float x, float in_min, float in_max, float out_min, float out_max)
{
  return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||dust sensor||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
void dustread()
{
  digitalWrite(ledPower,LOW);  // power on the LED 
  delayMicroseconds(samplingTime);

  voMeasured = analogRead(measurePin);  // read the dust value

  delayMicroseconds(deltaTime);
  digitalWrite(ledPower,HIGH);  // turn the LED off 
  delayMicroseconds(sleepTime);

  // 0 - 5.0V mapped to 0 - 1023 integer values  
  calcVoltage = voMeasured*(5.0/1024);

  // linear eqaution taken from http://www.howmuchsnow.com/arduino/airquality/
  // Chris Nafis (c) 2012 
  dustDensity = 0.17*calcVoltage-0.1;

  if ( dustDensity < 0)
  {
    dustDensity = 0.00;
  }
  dustreading = String(dustDensity) + "kg/m^3";

  delay(2000);
}

//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||RTC|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||| 
void rtcread()
{
  DateTime now = rtc.now();
  rtcreading = "d" + String(now.year()) + "/" + String(now.month()) + "/" + String(now.day()) + " " + String(now.hour()) + ":" + String(now.minute());
}

//|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||Environmental Sensor||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
void BMEread()
{
	temp = bme.temperatureValue();
	pa = bme.pressureValue();
	hum = bme.humidityValue();
	alt = bme.altitudeValue(SEA_LEVEL_PRESSURE);
	BMEreading = "b" + String(temp) + "c " + String(pa) + "pa " + String(hum) + "%";
}

void loop()
{
  digitalWrite(3,HIGH);
	bool send_status = false;
	Serial.println("Sending to rf95_server");
	UVread();
	BMEread();
	dustread();
	rtcread();
  
  //if NAN condition occurs
  while(isnan(temp)||isnan(pa)||isnan(hum))
  {
    BMEread();
  }

  // Send a message to rf95_server
	String radiopacket = "No.";
	radiopacket += String(packetnum++);   //number of successful transmissions
	radiopacket += " from ";
	radiopacket += node_id;
	radiopacket += " ";
	radiopacket += rtcreading;
	radiopacket += " "; 
	radiopacket += dustreading;
	radiopacket += " ";
	radiopacket += UVreading;
	radiopacket += " ";
	radiopacket += BMEreading;
  
	Serial.print("Sending "); 
	Serial.println(radiopacket); delay(10);
	rf95.send((uint8_t*)(radiopacket.c_str()), radiopacket.length()+1);
 
	Serial.println("Waiting for packet to complete..."); delay(10);
	rf95.waitPacketSent();
  // Now wait for a reply
	uint8_t buf[RH_RF95_MAX_MESSAGE_LEN];
	uint8_t len = sizeof(buf);
 
	Serial.println("Waiting for reply..."); 
	delay(10);
	while (send_status==false)
	{
    digitalWrite(3,HIGH);
		if (rf95.waitAvailableTimeout(1000))
		{
			// Should be a reply message for us now   
			if (rf95.recv(buf, &len))
			{
				Serial.print("Got reply: ");
				Serial.println((char*)buf);
				Serial.print("RSSI: ");
				Serial.println(rf95.lastRssi(), DEC);
				send_status = true;
        digitalWrite(3,LOW); 
			}
			else
			{
				Serial.println("Receive failed,sending again");
				Serial.println(radiopacket); delay(10);
				rf95.send((uint8_t*)(radiopacket.c_str()), radiopacket.length() + 1);
				Serial.println("Waiting for packet to complete..."); delay(10);
				rf95.waitPacketSent();
        digitalWrite(3,LOW);
        delay(1000);
			}
		}
		else
		{
			Serial.println("Lets send again, and see if there is a reply");
			Serial.println(radiopacket); delay(10);
			rf95.send((uint8_t*)(radiopacket.c_str()), radiopacket.length() + 1);
			Serial.println("Waiting for packet to complete..."); delay(10);
			rf95.waitPacketSent();
      digitalWrite(3,LOW);
      delay(1000);
		}
	}
	delay(300000); 
}
