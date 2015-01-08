# CarManagerSystem
Final Project - Car Manager System [including Asp.Net MVC, WP8, SQL DB projects]

The project composed of number components:

* Arduino device - Device that simulates wireless immobilizer that can lock/unlock the car.
* ELM327 - Device that transfers data from car sensors to Bluetooth connection.
* Nokia Lumia 820 - Windows phone application connected to the arduino device and to the ELM327 device threw Bluetooth and sent data threw the internet.
* Azure - contained the user website and the DB server.

Code summary:
CarManagerPhoneApp: 
  - Arduino: WP8 classes to communicate with Arduino
  - Service References: CarManagerService API to send data to Azure cloud.
  - ConnectorManager: ObdConector - commuinicate with ELM327 device
  - Facebook: the users authorization using Facebook API
  - WP8 Application code.
  
CarManagerWebApplication:
  - CarManagerApi: Web Service for Car Manager data.
  - MVC web site
