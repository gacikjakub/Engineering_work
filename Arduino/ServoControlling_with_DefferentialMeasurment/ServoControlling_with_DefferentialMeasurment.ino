//Libraries
#include <Servo.h>

//Variables
Servo servo1;
Servo servo2;
int counter1 = 0;   // keep quantity of non-overloaded values in succession for servo1
int counter2 = 0;   // keep quantity of non-overloaded values in succession for servo2
const int marginOfError = 16;
const String secretCode = "I am GOD";  // It's a Password for initialize connection with PC
int InitI = 0;

String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
 
uint16_t diffval1,diffval2;    // keep value from differential voltage measurment 

void detachServo(Servo& s) {
  s.detach();
}


void attachServo(Servo& s,int i) {
  s.attach(i);
}

uint16_t do_adc() {    // defintion of read function
  ADCSRA |= (1<<ADSC); //Start an ADC conversion by setting the ADSC bit.
  while(!(ADCSRA & (1<<ADIF)));  //Wait for the conversion to finish. The ADC signals that it's finished
  ADCSRA|=(1<<ADIF); 
return ADC;  //Return the ADC result from the ADC register.
};

void initdiff(){    // definition of initialization function
  ADCSRA=(1<<ADEN)|(1<<ADPS2)|(1<<ADPS1)|(1<<ADPS0);
  ADMUX = (1<<REFS0) |(0<<REFS1);       // set reference voltage
  ADMUX |= (1<<MUX3)|(1<<MUX1)|(1<<MUX0);  // set  ports to read voltage (particular)
};

void adc1() {
 ADCSRB = (0<<MUX5);    // set read port on A0 and A1
}

void adc2() {
 ADCSRB = (1<<MUX5);    // set read port on A8 and A9 ?? 
}

uint16_t do_adc1() {    // set port and measure voltage for servo1   
  adc1();
  delay(1);
  diffval1 = do_adc();
  return diffval1;
}

bool setServo_measure(){         //set servo based on incoming data
  bool res = false;
  if(inputString.startsWith("S1:")) {   // every line which is started by S1: concern servo1 
        res = true;
        adc1();
        inputString.remove(0,3);        //removing "S1:"  to get only value
        delay(1);
        if (inputString=="F")detachServo(servo1);  
          else
          if (inputString=="T") attachServo(servo1,2);
          else
        servo1.write(inputString.toInt());
        diffval1 = do_adc();
      } else
       if(inputString.startsWith("S2:")) {    // every line which is started by S2: concern servo2 
        res = true;
        adc2();
        inputString.remove(0,3);          //removing "S2:"  to get only value
        delay(1);
        if (inputString=="F")detachServo(servo2);  
          else
          if (inputString=="T") attachServo(servo2,3);
          else
        servo2.write(inputString.toInt());
        diffval2 = do_adc();
      }
  return res;      
}

bool setInitializeMode() {    // go to initialize()
  bool res = false;
  if (inputString.equals("Exit")) {
    res = true;
    servo1.detach();
    servo2.detach();
    setup();
  }
  return res;
}

uint16_t do_adc2() {  // set port and measure voltage for servo2
  adc2();
  delay(1);
  diffval2 = do_adc();
  return diffval2;
}

void send_diff_value(Servo* tempservo) {
  if (tempservo == &servo1) {
    Serial.print("D1:");     // every line which started by D1: concern the servo1 and after is sending overloaded value
    Serial.println(diffval1);
  }
  if (tempservo == &servo2) {
    Serial.print("D2:");
    Serial.println(diffval2);
  } 
}

void serialEvent() {     //  override function which is running on each end of loop
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    } else inputString += inChar;
  }
}

void initialize() {
  InitI ++;
  while(!Serial.available()) {
    Serial.println(secretCode);
    delay(200);
  }
  inputString = "";
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    } else inputString += inChar;
  }
    if (stringComplete) {    // check if incoming word is complete  (end by \n)
          if (!(inputString.equals(secretCode))) {
        inputString = "";
        stringComplete = false;
        initialize();
      }    
  } else initialize();
  
}

void setup() {
  Serial.begin(230400); // initialize serial communication at 9600 bits per second:
  initialize();
  Serial.println(InitI);
  initdiff();   // run initialization fucntion
  servo1.attach(2);   // set signal port to servo1 on digital port 2 
  servo2.attach(3);   // set signal port to servo2 on digital port 3 
}

// the loop routine runs over and over again forever:
void loop() {
  
    if (stringComplete) {    // check if incoming word is complete  (end by \n)
          if(!setServo_measure()) setInitializeMode();
        // clear the string:
       inputString = "";
       stringComplete = false;
  } else {    // when steering data is not receiving we must still check overloading on servos
    do_adc1();
    do_adc2();
  }
  /*   when servos standing, the measure values is not equal 0, but about 5-10 by any noise,
   *   so we must assume some margin of error .... here is marginOfError
   */  
  if (diffval1 >marginOfError) {       
    send_diff_value(&servo1);   // sending diffvalue to PC
    counter1 = 0;}   // clear counter when servo is overloaded
    else {
      if (counter1<10) counter1++;    // servo can be non-overloaded to infinity so we can increase this value all time
    }
  if (diffval2 >marginOfError) {
    send_diff_value(&servo2);
    counter2 = 0;    // clear counter when servo is overloaded
    } else {
      if (counter2<10) counter2++;    // servo can be non-overloaded to infinity so we can increase this value all time
    }
            // we send data with non-overloaded value only once time in third loop pattern because sometimes overloaded value are interlaced with small value
    if (counter1==4) {
      send_diff_value(&servo1);   // sending diffvalue to PC
    }
    if (counter2==4) {
      send_diff_value(&servo2);   // sending diffvalue to PC
    }
      delay(20);   //limit frequency of measurments
}

