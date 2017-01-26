#include <Arduino.h>
#include <Wire.h>
#include <SoftwareSerial.h>

#include <MeAuriga.h>

//Encoder Motor
MeEncoderOnBoard Encoder_1(SLOT1);
MeEncoderOnBoard Encoder_2(SLOT2);
MeBluetooth bluetooth(PORT_3);
MeBuzzer buzzer;

#define BUZZER_PORT                          45


void isr_process_encoder1(void)
{
      if(digitalRead(Encoder_1.getPortB()) == 0){
            Encoder_1.pulsePosMinus();
      }else{
            Encoder_1.pulsePosPlus();
      }
}

void isr_process_encoder2(void)
{
      if(digitalRead(Encoder_2.getPortB()) == 0){
            Encoder_2.pulsePosMinus();
      }else{
            Encoder_2.pulsePosPlus();
      }
}

void move(int direction, int speed)
{
      int leftSpeed = 0;
      int rightSpeed = 0;
      if(direction == 1){
            leftSpeed = -speed;
            rightSpeed = speed;
      }else if(direction == 2){
            leftSpeed = speed;
            rightSpeed = -speed;
      }else if(direction == 3){
            leftSpeed = -speed;
            rightSpeed = -speed;
      }else if(direction == 4){
            leftSpeed = speed;
            rightSpeed = speed;
      }
      Encoder_1.setTarPWM(leftSpeed);
      Encoder_2.setTarPWM(rightSpeed);
}
void moveDegrees(int direction,long degrees, int speed_temp)
{
      speed_temp = abs(speed_temp);
      if(direction == 1)
      {
            Encoder_1.move(-degrees,(float)speed_temp);
            Encoder_2.move(degrees,(float)speed_temp);
      }
      else if(direction == 2)
      {
            Encoder_1.move(degrees,(float)speed_temp);
            Encoder_2.move(-degrees,(float)speed_temp);
      }
      else if(direction == 3)
      {
            Encoder_1.move(-degrees,(float)speed_temp);
            Encoder_2.move(-degrees,(float)speed_temp);
      }
      else if(direction == 4)
      {
            Encoder_1.move(degrees,(float)speed_temp);
            Encoder_2.move(degrees,(float)speed_temp);
      }
}

double angle_rad = PI/180.0;
double angle_deg = 180.0/PI;

char data = 0;
int lerp(int min, int max, float t){
  return (int)(min + t*(max-min));
}
unsigned char table[128] = {0};
unsigned char cmd[2] = {0};
MeRGBLed ledas(0,12);
int ledai[12] = {0};
void setup(){
    //Set Pwm 8KHz
    TCCR1A = _BV(WGM10);
    TCCR1B = _BV(CS11) | _BV(WGM12);
    TCCR2A = _BV(WGM21) | _BV(WGM20);
    TCCR2B = _BV(CS21);
    buzzer.setpin(BUZZER_PORT);
     attachInterrupt(Encoder_1.getIntNum(), isr_process_encoder1, RISING);
    attachInterrupt(Encoder_2.getIntNum(), isr_process_encoder2, RISING);
    Serial.begin(115200);
    ledas.setpin(44);
}
int ledNumber = 0;
float percentage = 0;
float angle = 0;
void loop(){
  for(int i = 0; i < 12; i++){
    if(i%3==0){
      ledas.setColor(i+1, ledai[i],0,0);
    }
    if(i%3==1){
      ledas.setColor(i+1, 0,ledai[i],0);
    }
    if(i%3==2){
      ledas.setColor(i+1, 0,0,ledai[i]);
    }
    ledai[i] = (int)(sin((i+1)*angle)*50);
    angle += 0.00001;
    if ((i+1)*angle >= 3.14){
      angle = 0;
    }
    ledas.setColor(i+1,0,0,0);
  }
  ledas.show();
if(Serial.available() > 0)      // Send data only when you receive data:
   {
      buzzer.tone(988, 125);
      data = Serial.readBytes(cmd, 2);
      move(cmd[0], cmd[1]);
      Serial.write(cmd, 2);
   }
   

    
    _loop();
//  int readdata = 0,i = 0,count = 0;
//  char outDat;
//  if (bluetooth.available())
//  {
//    buzzer.tone(988, 125);
//    Serial.println( "bluetooth available");
//    while((readdata = bluetooth.read()) != (int)-1)
//    {      
//      buzzer.tone(988, 125);
//      Serial.println( readdata);
//      table[count] = readdata;
//      count++;
//      delay(1);
//    }
//    if(table[0] == 1){
//      move(4,100);
//      _delay(5);
//      move(1,0);
//      _loop();
//    }
//  }
}

void _delay(float seconds){
    long endTime = millis() + seconds * 1000;
    while(millis() < endTime)_loop();
}

void _loop(){
    Encoder_1.loop();
    Encoder_2.loop();
}
