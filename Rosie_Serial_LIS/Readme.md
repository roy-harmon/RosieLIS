# Rosie Serial LIS

Rosie Serial LIS is a Windows service (written in Visual Basic.NET) that communicates over a serial port with a Siemens Dimension® analyzer, storing and retrieving results data to/from a database server. It was named after Rosie, the specific Siemens Dimension Xpand+ analyzer for which it was originally written at Auburn University.

## Installation

Rosie Serial LIS can be installed using the included setup file. Alternatively, navigate to the Rosie_Serial_LIS.exe file using the command prompt and enter the following commands:

```bash
installutil Rosie_Serial_LIS.exe
net start RosieLIS
```

Please note: The software also requires a database server in order to function properly. See the next section for details.

## Database

This Windows service is designed to connect to a database of your choice. Currently supported connection drivers include Microsoft SQL Server (for which it was originally designed), MySQL, and ODBC. It has been tested with SQL Server and MySQL databases, but it *should* be compatible with any ODBC connector. Due to the deprecation of the native Oracle client in recent versions of the .NET framework, Oracle databases are not currently supported without a third-party ODBC driver.
Whichever data source you use, just be sure to specify a valid connection string in the Rosie_Serial_LIS.exe.config file as discussed below.

While some parts of the database are fairly flexible, the Rosie Serial LIS service expects certain tables and fields to be present. To that end, several SQL "CREATE TABLE" scripts have been provided in the **sql_scripts** folder -- **sqlserver_scripts.sql** for MS SQL Server or **mysql_scripts.sql** for MySQL. Please execute these scripts in your database before attempting to start this service.

## Configuration

The "Rosie_Serial_LIS.exe.config" file contains several configuration items that should be changed according to your specific requirements. 

Serial Port Parameters
---
* portName - Change this to the COM port of whichever serial port you intend to connect to the Dimension analyzer. Default = COM1.
* baudRate - Valid parameters include 300, 600, 1200, 2400, 4800, 9600, and 19200. Default is 9600, since all Dimension EXL models have to use it.
* parity - None = 0 (default), Odd = 1, Even = 2.
* dataBits - 7 or 8 (default).
* stopBits - 1 (default) or 2.
* handshake - None = 0 (default). Other values are not recommended for Dimension systems.

Database Parameters
---
* databaseType - `SQL Server` (default), `MySQL`, or `ODBC`.
* maxTests - The maximum number of tests allowed by the PendingTests table. The default is 6; the default PendingTests table includes fields from `Test1` up to `Test6`, so if you change the maxTests value, be sure to include `Test#` fields up to (and including) the maxTests value.
* connectionString - The database connection string specific to your server/database installation. Typical values might be something like "server=127.0.0.1;database=lisdb;uid=lis_user;pwd=password123".

## Usage

The program runs as a Windows service under a Local System account. After installation, the service should automatically start on boot.

While it can be configured for several modes of operation, the typical usage is as follows: 
* The analyzer polls the computer for pending test requests.
* Pending test requests are pulled from a database (specified in the config file) and sent to the analyzer.
* The analyzer transmits result data to the computer.
* Results are stored in the database to be accessed by other software.

It also stores calibration results to the database, but due to some bugs in the Siemens Dimension software, these should not be used for anything too important. As of Dimension version 10.3, the following problems have been observed with calibration results:
* The Cal_DateTime value actually represents the timestamp from the beginning of the reagent lot's previous calibration.
* Due to the above timestamp discrepancy, the first calibration of each lot will have a Cal_DateTime value of "000019311269" (12/31/xx69 7:00 PM).
* Since the new coefficients are not yet calculated when the data is transmitted, coefficient values given are from the previous calibration.
* The actual number of coefficients may differ from the value reported by the instrument ("Coefficients_Num"). For example, it may report that there are 5 coefficients for a linear calibration, but the first two are numbers and the rest are null.
* All result ("Res##") values are calculated using the current calibration's measurements with the prior calibration's coefficients.
* The Cal_Slope value is always given as exactly "1".
* The Cal_Intercept value is always given as exactly "0". Since this holds true even when the actual intercept is >1, it seems unlikely that this is an integer rounding issue.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## Acknowledgements

This software was written using information from the Dimension Clinical Chemistry Systems Interface Specification Guide, downloaded from the Siemens Healthineers Document Library. 
Support for MySQL uses the Oracle MySQL team's MySQL Connector/.NET 8.0, used under the GPLv2 license as outlined [here](https://downloads.mysql.com/docs/licenses/connector-net-8.0-gpl-en.pdf).
Dimension, EXL, and Xpand are trademarks of Siemens Healthcare Diagnostics.

## License

Rosie Serial LIS is published under the [MIT](https://choosealicense.com/licenses/mit/) license.

MIT License
---

Copyright (c) 2019 Roy Harmon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.