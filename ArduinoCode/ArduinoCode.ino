#include <JsonParser.h>
#include <JsonGenerator.h>
using namespace ArduinoJson::Generator;
//#include <HashMap.h>


#include <DateTime.h>
#include <DateTimeStrings.h>

#define MAX_STRING_LEN 30
#define CAR_ID 1455543
size_t Psize = 0, Pindex = 0;
/****----------------------------------------------------ALLWAYS ALLOWED TO DRIVE- DRIVERS LIST--------------------------****/
struct permanentAutorizationDriver *headPDriver = NULL;
struct permanentAutorizationDriver *tailPDriver = NULL;

/****----------------------------------------------------EMERGENCY CASES ALLOWED TO DRIVE- DRIVERS LIST------------------****/
struct emergencyAutorizationDriver *headEDriver = NULL;
struct emergencyAutorizationDriver *tailEDriver = NULL;

/****----------------------------------------------------POINTERS FOR RIDE LIST------------------------------------------****/
struct ride *ridesHead = NULL;
struct ride *ridesTail = NULL;

/****----------------------------------------------------COUNTERS OF DRIVERS---------------------------------------------****/
int numOfPDrivers;
int numOfEDrivers;
/****----------------------------------------------------COUNTER OF RIDES------------------------------------------------****/
int numOfRides;
//char * endPackageDelimiter;
int endPackageDelimiter;
int startPackageDelimiter;
char * package;


/****----------------------------------------------------DEBUG PARAMETERS------------------------------------------------****/
long val;         // variable to receive data from the serial port
int ledpin = 2;  // LED connected to pin 2 (on-board LED)


/****----------------------------------------------------THE STRUCTS-----------------------------------------------------****/

typedef struct permanentAutorizationDriver
{
	char driverID[37] ;
	struct permanentAutorizationDriver *nextDriver;
};
typedef struct emergencyAutorizationDriver
{
	char driverID[37];
	int emergencyRidesCount;
	struct emergencyAutorizationDriver *nextDriver;
};

typedef struct ride
{
	char driverID[37];
	char carID[37];
	char* time;
	struct ride *next;
};
/****----------------------------------------------------METHODS---------------------------------------------------------****/
struct permanentAutorizationDriver* FindDriverInPermanentList(char* driverId)
{
	struct permanentAutorizationDriver* currentDriver = (permanentAutorizationDriver*)malloc(sizeof(permanentAutorizationDriver*));
	struct permanentAutorizationDriver* result = NULL;
	currentDriver = headPDriver;
	bool notFound = true;
	int i;

	for (i = 1; i <= numOfPDrivers && notFound; i++)
	{
		//if (currentDriver->driverID == driverId /*&&*/)
                if (strcmp(currentDriver->driverID, driverId) == 0)
		{
			result = (permanentAutorizationDriver*)malloc(sizeof(permanentAutorizationDriver*));
			result = currentDriver;
			notFound = false;
		}
		else

		{
                        Serial.println(currentDriver->driverID);
			currentDriver = currentDriver->nextDriver;
		}
	}
//	if (result == NULL)
//	{
//		Serial.println("Driver was'nt found ");
//	}
//	else 
//	{
//		Serial.println("Permanent ride id allowed");
//	}
	return result;
}

struct emergencyAutorizationDriver* FindDriverInEmergencyList(char *driverId)
{
	struct emergencyAutorizationDriver* currentDriver = (emergencyAutorizationDriver*)malloc(sizeof(emergencyAutorizationDriver*));
	struct emergencyAutorizationDriver* result = NULL;
	currentDriver = headEDriver;
	bool notFound = true;
        int i;

        for (i = 1; i <= numOfEDrivers && notFound; i++)
	{
		//if (currentDriver->driverID == driverId /*&&*/)
                if (strcmp(currentDriver->driverID , driverId) == 0)
		{
			result = (emergencyAutorizationDriver*)malloc(sizeof(emergencyAutorizationDriver*));
			result = currentDriver;
			notFound = false;
		}
		else
		{
                        Serial.println(currentDriver->driverID);
			currentDriver = currentDriver->nextDriver;
		}
	}
//	if (result == NULL)
//	{
//		Serial.println("Driver was'nt found ");
//	}
	return result;
}

void AddNewDriverToPermanentList(char *driverId)
{
	permanentAutorizationDriver *newDriver;
	newDriver = (permanentAutorizationDriver*)calloc(1, sizeof(struct permanentAutorizationDriver));
	//newDriver->driverID = driverId;
	strcpy(newDriver->driverID, driverId);
        newDriver->nextDriver = NULL;

	if (headPDriver == NULL)
	{
		headPDriver = tailPDriver = newDriver;
	}
	else
	{
		tailPDriver->nextDriver = newDriver;
		tailPDriver = newDriver;
	}
	numOfPDrivers++;
        Serial.println("OK");
}

void AddNewDriverToEmergencyList(char *driverId, int count)
{
	emergencyAutorizationDriver *newDriver;

	newDriver = (emergencyAutorizationDriver*)calloc(1, sizeof(struct emergencyAutorizationDriver));
	//newDriver->driverID = driverId;
        strcpy(newDriver->driverID , driverId);
	newDriver->emergencyRidesCount = count;
	newDriver->nextDriver = NULL;

	if (headEDriver == NULL)
	{
		headEDriver = tailEDriver = newDriver;
	}
	else
	{
		tailEDriver->nextDriver = newDriver;

		tailEDriver = newDriver;
	}
	numOfEDrivers++;
        Serial.println("OK");
}

int GetNumberOfPermanentDrivers()
{
	return numOfPDrivers;
}
int GetNumberOfEmergencyDrivers()
{
	return numOfEDrivers;
}
int GetNumberOfRides()
{
	return numOfRides;
}

void PrintPermanentDriversList()
{
	struct permanentAutorizationDriver* currentDriver = (permanentAutorizationDriver*)malloc(sizeof(permanentAutorizationDriver*));
	currentDriver = headPDriver;
	char id[37];
	for (int i = 1; i <= numOfPDrivers; i++)
	{
		Serial.println("driver id37 is :");
		//id = currentDriver->driverID;
                strcpy(id , currentDriver->driverID);
		Serial.println(id);
		currentDriver = currentDriver->nextDriver;
	}
}

void PrinteEmergencyDriversList()
{
	struct emergencyAutorizationDriver* currentDriver = (emergencyAutorizationDriver*)malloc(sizeof(emergencyAutorizationDriver*));
	currentDriver = headEDriver;
	char id[37];
	for (int i = 1; i <= numOfEDrivers; i++)
	{
		Serial.println("driver id is :");
		//id = currentDriver->driverID;
                strcpy(id , currentDriver->driverID);
		Serial.println(id);
		currentDriver = currentDriver->nextDriver;
	}
}

void DeletePermanentDriversList()
{
	int count = numOfPDrivers;
	struct permanentAutorizationDriver* currentDriver = (permanentAutorizationDriver*)malloc(sizeof(permanentAutorizationDriver*));
	currentDriver = headPDriver;
//    Serial.println("Inside delete");
	for (int i = 1; i <= count; i++){
		headPDriver = currentDriver->nextDriver;
		
		Serial.println(currentDriver->driverID);
		free(currentDriver);
		currentDriver = headPDriver;
		numOfPDrivers--;
	}
        Serial.println("Done deleting");
}

void DeleteEmergencyDriversList()
{
	int count = numOfEDrivers;
	struct emergencyAutorizationDriver* currentDriver = (emergencyAutorizationDriver*)malloc(sizeof(emergencyAutorizationDriver*));
	currentDriver = headEDriver;
	for (int i = 1; i <= count; i++){
		headEDriver = currentDriver->nextDriver;
		Serial.println("delete driver id :");
		Serial.println(currentDriver->driverID);
		free(currentDriver);
		currentDriver = headEDriver;
		numOfEDrivers--;
	}
        Serial.println("Done deleting");
}

bool isPermanentAllowedEoDrive(char *driverId)
{       
	permanentAutorizationDriver *currentDriver = FindDriverInPermanentList(driverId);
	if (currentDriver == NULL)
	{
		return false;
	}
	return true;
}
bool isEmergencyAllowedEoDrive(char *driverId)
{
	emergencyAutorizationDriver *currentDriver = FindDriverInEmergencyList(driverId);
	if (currentDriver == NULL /* && currentDriver->count>0 */)
	{
		return false;
	}
	currentDriver->emergencyRidesCount--;
	return true;
}

void UpdatePermanentallowedList()
{
	if (true)
	{
		DeletePermanentDriversList();
	}
}
void RideRequest(char *id)
{
  bool pa= isPermanentAllowedEoDrive(id);
  if(!pa)
  {
    Serial.println("not exist in permanent list");
    pa =isEmergencyAllowedEoDrive(id);

  }
  if(pa)
  {
    Serial.println("allowed");
  }
  else
  {
    Serial.println("Not allowed");
  }
  
}
/****---------------------------------------------------- COMMON METHODS---------------------------------------------------------****/
void AddNewRideToList(char *dId, char *cId, char* time)
{
	ride *newRide;
	newRide = (ride*)calloc(1, sizeof(struct ride));
        strcpy(newRide->driverID, dId);
        strcpy(newRide->carID, cId);
        newRide->time = time;
	if (ridesHead == NULL){
		ridesHead = ridesTail = newRide;
	}
	else{
		ridesTail->next = newRide;
		ridesTail = newRide;
	}
	numOfRides++;
        Serial.println("");
}
//void PrintRidesList()
//{
//	struct ride*  currentRide = (ride*)malloc(sizeof(ride*));
//	currentRide = ridesHead;
//	for (int i = 1; i <= numOfERides; i++)
//	{
//		Serial.print("driver id is :");
//		Serial.print(currentRide->driverID);
//		Serial.print(currentRide->carID);
//		Serial.print(currentRide->time);
//
//		currentRide = currentRide->next;
//	}
//}

char *ReadString()
{
	char *line = NULL, *tmp = NULL;
	size_t size = 0, index = 0;
	int ch = EOF;
	while (Serial.available() > 0 && ch != '_'){
		//ch = getc(stdin);
		ch = Serial.read();

		if (ch != '_' /*&& ch != EOT && ch != SO*/){
			/* Check if we need to expand. */
			if (size <= index) {
				size += 1;
				tmp = (char*)realloc(line, size);
				if (!tmp) {
					free(line);
					line = NULL;
					break;
				}
				line = tmp;
			}

			/* Actually store the thing. */
			line[index++] = ch;
		}
	}

	return line;
}


long ReadInteger()
{
	if (Serial.available() > 0)
	{
		long result = 0;
		char ch = 0;
		int temp = 0;
		Serial.println("start reading int");

		while (Serial.available() > 0 && ch != '_') {

			ch = Serial.read();
			if (ch != '_')
			{
				result *= 10;
				temp = ch - '0';
				result += (temp);
			}
		}
		//PrintInteger(result);
		return result;
	}
	return  NULL;
}
//  char* result;
//  char currentChar = Serial.read();
//  Serial.println("start reading int");
//  delay(500);                  // waits for a second
//  while (Serial.available() > 0 && currentChar!='_') {
//            result += '1';
////            result += (Serial.read() - '0');
//            delay(10);
//        }
////  PrintInteger(result);
//  return result;        
//  }
//  return  NULL;
//}
void PrintInteger(int val)
{
	Serial.print(val);
	Serial.println();
}

/**************************************************************************************************************************************/
/**************************************************************************************************************************************/

void ResetPackage()
{
	free(package);
	package = (char*)malloc(512);
	Psize = 0;
	startPackageDelimiter = 0;
	endPackageDelimiter = 0;
}

void setup()
{
	pinMode(ledpin = 13, OUTPUT);  // pin 13 (on-board LED) as OUTPUT
	ResetPackage();

	Serial.begin(115200);       // start serial communication at 115200bps
	Serial.println("Started: ");
	Serial.println("Started: ");
	Serial.println("Started: ");

}


void AddCharToPackage(char *package, char val)
{
	Psize += 1;
	package[Psize - 1] = val;
}
void CreateRidesList()
{
  
  	struct ride*  currentRide = (ride*)malloc(sizeof(ride*));
	currentRide = ridesHead;
        JsonArray<10> array;
        
	for (int i = 1; i <= numOfRides; i++)
	{
                JsonObject<3> ride; 
                ride["dId"] = currentRide->driverID;
                ride["cId"] = currentRide->carID;
                ride["time"] = currentRide->time;
                array.add(ride);
		currentRide = currentRide->next;
	}

       JsonObject<1> root; 
                root["rides"] = array;

    Serial.print(root); 
}

ArduinoJson::Parser::JsonParser<16> parser;

void loop()
{
	char val;

	while (Serial.available() > 0)
	{
		// val = ch;

		val = Serial.read();

		if (val == '&')
		{
  		    startPackageDelimiter++;
		}
		else if (val == '$' /*&&*/)
	        {
		    endPackageDelimiter++;
	        }
	        else if (startPackageDelimiter == 3)
		{
			AddCharToPackage(package, val);
			endPackageDelimiter = 0;
		}

//		Serial.print("Val: ");
//		Serial.print(val);
//		Serial.print(", start count: ");
//		Serial.print(startPackageDelimiter);
//		Serial.print(", end count: ");
//		Serial.println(endPackageDelimiter);

		if (endPackageDelimiter == 3 && startPackageDelimiter == 3)
		{//that is the third time
//			Serial.print("In Main method: ");
//			Serial.println(Psize);
			if (Psize > 0)
			{
				AddCharToPackage(package, '\0');
				//Serial.print("Package is ready");
				//Serial.println(package);
				ArduinoJson::Parser::JsonObject root = parser.parse(package);
//				Serial.println("Deserialized!!!");
                                  
				if (!root.success())
				{
					Serial.println("Too bad");
				}
                                 
				else
				{
					char*  functionName = root["FunctionName"];
                                        char*  initString = root["InitString"];
                                if (strcmp(functionName, "PerCount") == 0)
				{
                                                  Serial.println("                                          ");
                                }
                                 else
                                 {
                                        //Serial.println("     START");
                                        Serial.println(">>>>>");
//					Serial.print("Function: ");
				//	Serial.println(functionName);
                                        Serial.println(initString);
					if (strcmp(functionName, "AddPerDr") == 0)
					{
//						Serial.println("Adding new driver");
						char *id = root["driverId"];
						AddNewDriverToPermanentList(id);
						//	AddNewDriver(root);
					}
					else if (strcmp(functionName, "AddEmerDr") == 0)
					{
						char *id = root["driverId"];
                                                long count = root["count"];
 						AddNewDriverToEmergencyList(id,count);
						//	AddNewDriver(root);
					}
					else if (strcmp(functionName, "AddRide") == 0)
					{
						char *id = root["dId"];
                                                char *carId = root["cId"];
                                                char *time = root["time"];
                                                AddNewRideToList(id, carId, time);
						//	AddNewDriver(root);
					}
				        else if (strcmp(functionName, "GetRides") == 0)
					{
                                                CreateRidesList();
					}
					else if (strcmp(functionName, "DelPerList") == 0)
					{
//						Serial.println("Deleting permanent list");
                                                DeletePermanentDriversList();
					}
					else if (strcmp(functionName, "DelEmerList") == 0)
					{
						Serial.println("Deleting emergency list");
                                                DeleteEmergencyDriversList();
					}
				        else if (strcmp(functionName, "RideRequest") == 0)
					{
						char *id = root["driverId"];
                                                RideRequest(id);
					}
                                        else if (strcmp(functionName, "PerCount") == 0)
					{
                                                 Serial.println( "                          " );
                                                //Serial.println(GetNumberOfPermanentDrivers());
					}
                                        else if (strcmp(functionName, "EmerCount") == 0)
					{
                                                Serial.println(GetNumberOfEmergencyDrivers());
					}
                                        else if (strcmp(functionName, "IsPerAllowed") == 0)
					{
                                                char *id = root["driverId"];
                                                if(isPermanentAllowedEoDrive(id))
                                                {
                                                  Serial.println("Approved");
                                                }
                                                else
                                                {
                                                  Serial.println("Rejected");
                                                }
                                        }
                                        else if (strcmp(functionName, "IsEmerAllowed") == 0)
					{
                                                char *id = root["driverId"];
                                                if(isEmergencyAllowedEoDrive(id))
                                                {
                                                  Serial.println("Approved");
                                                }
                                                else
                                                {
                                                  Serial.println("Rejected");
                                                }
					}
                                        else if (strcmp(functionName, "GetCarId") == 0)
					{
                                                Serial.println(1455543);
					}
                                   Serial.print("<<<<<");
                              //  Serial.print("END");
				}
                            }

			}

			ResetPackage();
		}
	}
}


