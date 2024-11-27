# BT2202A TAP Plugin Development

### Overview

This project focuses on the development of a plugin for Keysight’s BT2202A Charge-Discharge, Li-ion Cell Formation, and Test Solution, using the OpenTAP framework. The plugin enables users to design and execute test sequences for energy storage cells, enhancing the BT2202A’s usability for testing and research purposes. The development includes basic functionalities, advanced features for running parallel tests, and solutions to overcome hardware limitations.

### Video Demonstration
- https://youtu.be/3TpPh6u8YSQ


## Features

### Basic Plugin (Branch: main)

	•	Detects the BT2202A instrument within TAP.
	•	Drag-and-drop steps for common test operations (charge, discharge, measure).
	•	Basic Pass/Fail logging.
	•	Modular and open-source design for future development.

### Advanced Plugin (Branch: json-testing)

	•	Enables parallel test execution by running separate TAP instances in sync.
	•	Introduces JSON-based communication for coordination between test steps.
	•	Flags and logic adjustments to manage device constraints.
	•	Overcomes SCPI command limitations for true parallel testing.

## Requirements

### Software

	•	Visual Studio: The project is designed to be developed and run within Visual Studio.
Open the solution file located at:

/YushiProject/bt2202a.sln


	•	OpenTAP: Install OpenTAP to integrate the plugin into your testing environment.
Refer to OpenTAP Documentation for installation steps.

### Dependencies

	•	Keysight BT2202A instrument (real or simulated for testing).
	•	SCPI command support.

## Branches

### main

Contains the basic plugin, which provides fundamental test steps for the BT2202A:
	•	Charge
	•	Discharge
	•	Measure

### json-testing

Contains the advanced plugin, which includes:
	•	Parallel test execution features.
	•	JSON-based communication to address reporting and measurement limitations.

Usage

	1.	Clone the repository:

git clone https://github.com/ricardo1slas/BT2202A_Plugin.git
cd BT2202A_Plugin


	2.	Switch to the desired branch:
	•	For the basic plugin:

git checkout main


	•	For the advanced plugin:

git checkout json-testing


	3.	Open the solution file in Visual Studio:

/YushiProject/bt2202a.sln


	4.	Build and deploy the plugin within the OpenTAP environment.

### Known Limitations

	•	The current implementation does not distinguish active and inactive cells, which may cause errors during measurements.
	•	Pass/Fail verdicts are not directly linked to the TAP test sequence during parallel execution.
	•	Requires additional configuration files (e.g., JSON) for improved scalability and control.

### Future Steps

	•	Implement a configuration file for better cell tracking and centralized control.
	•	Improve Pass/Fail reporting to directly integrate with TAP’s sequence control.
	•	Extend support for more complex test sequences.

## Contributing

We welcome contributions to further improve this project. For collaboration:
	•	Fork the repository and create pull requests.
	•	Report issues or suggest enhancements via GitHub Issues.

### Acknowledgments

This project was developed by Ricardo Islas Guerra, Germán Alvarado De los Santos, and Cheyenne Rigel De Jesús González as part of the course TE3004B - Desarrollo de Telecomunicaciones y Sistemas Energéticos at Tec de Monterrey.

Special thanks to Pablo and the team at Keysight for their support and guidance.

License

This project is open source and follows the principles of the OpenTAP framework.
