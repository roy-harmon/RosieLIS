-- The following SQL commands are essential to the function of the Rosie Serial LIS service.
-- The syntax of this file is for MySQL.

USE lisdb; -- Change this to the name of your MySQL database schema.

CREATE TABLE SampleData(
	ID int NOT NULL AUTO_INCREMENT,
	Loadlist_ID varchar(2),
	Patient_ID varchar(27),
	Sample_No varchar(12),
	SampleType varchar(1),
	Location varchar(6),
	Priority smallint,
	DateTime varchar(12),
	Cups varchar(1),
	Dilution varchar(3),
	TestsCount varchar(2),
	DateTimeFormatted datetime AS (STR_TO_DATE(REPLACE(DateTime, " ", "0"), "%S%i%k%d%m%y")),
 CONSTRAINT PK_SampleData PRIMARY KEY CLUSTERED (ID));

CREATE TABLE SampleResults(
	Sample_ID int,
	TestName varchar(5),
	Result float AS (CAST(ResultValue AS decimal(8,3))),
	Units varchar(10),
	Error varchar(2) DEFAULT '',
	Result_ID int NOT NULL AUTO_INCREMENT,
	ResultValue varchar(10),
 CONSTRAINT PK_SampleResults PRIMARY KEY CLUSTERED (Result_ID ASC));

CREATE TABLE CalibrationResults(
	Cal_Test varchar(5),
	Cal_Units varchar(10),
	Reagent_Lot varchar(10),
	Cal_Product varchar(10),
	Cal_Prod_Lot varchar(10),
	Cal_Op varchar(10),
	Cal_DateTime varchar(12),
	Cal_Slope float,
	Cal_Intercept float,
	Coefficients_Num int,
	Coefficient_0 float,
	Coefficient_1 float,
	Coefficient_2 float,
	Coefficient_3 float,
	Coefficient_4 float,
	Bottle_Vals int,
	Val01 float,
	Res01 float,
	Val02 float,
	Res02 float,
	Val03 float,
	Res03 float,
	Val04 float,
	Res04 float,
	Val05 float,
	Res05 float,
	Val06 float,
	Res06 float,
	Val07 float,
	Res07 float,
	Val08 float,
	Res08 float,
	Val09 float,
	Res09 float,
	Val10 float,
	Res10 float,
	Val11 float,
	Res11 float,
	Val12 float,
	Res12 float,
	Val13 float,
	Res13 float,
	Val14 float,
	Res14 float,
	Val15 float,
	Res15 float,
	Val16 float,
	Res16 float,
	Val17 float,
	Res17 float,
	Val18 float,
	Res18 float,
	Val19 float,
	Res19 float,
	Val20 float,
	Res20 float,
	Calibration_ID int NOT NULL AUTO_INCREMENT,
	CalcDateTime datetime AS (STR_TO_DATE(REPLACE(Cal_DateTime, " ", "0"), "%S%i%k%d%m%y")),
	ActualDateTime datetime DEFAULT current_timestamp(),
 CONSTRAINT PK_CalibrationResults PRIMARY KEY CLUSTERED (Calibration_ID ASC));

CREATE TABLE PendingTests(
	Position varchar(2),
	PatientName varchar(27),
	SampleNo varchar(12),
	Test1 varchar(5),
	Test2 varchar(5),
	Test3 varchar(5),
	Test4 varchar(5),
	Test5 varchar(5),
	Test6 varchar(5),
	SampleType char(1) DEFAULT 1,
	ToDelete bit NOT NULL DEFAULT 0,
	Temp_ID int NOT NULL AUTO_INCREMENT,
	RejectCode char(1) DEFAULT 0,
	intPriority int DEFAULT 0,
	DilFactor int NOT NULL DEFAULT 1,
	PendingSending bit NOT NULL DEFAULT 0,
 CONSTRAINT PK_TempPendings PRIMARY KEY CLUSTERED (Temp_ID ASC));

-- The following SQL commands are not strictly required, but they should prove very helpful.

CREATE TABLE ErrorCodes(
	Error_Code varchar(2),
	Suppress_Result bit NOT NULL DEFAULT 0,
	Error_Interpretation varchar(255),
	Alt_Interpretations varchar(255),
	Explanation varchar(255),
	Error_ID int NOT NULL AUTO_INCREMENT,
 CONSTRAINT PK_ErrorCodes PRIMARY KEY CLUSTERED (Error_ID ASC));

INSERT INTO ErrorCodes 
		(Error_Code,Suppress_Result,Error_Interpretation,Alt_Interpretations,Explanation) 
VALUES
	('1',FALSE,'Temperature Out Of Range','','The cuvette temperature was out of acceptable range.'),
	('2',FALSE,'Calibration Expired','','The reagent cartridge lot for this test had an EXPIRED calibration status.'),
	('3',FALSE,'Assay Out Of Range','Assay Range/Diluted','The result for this test was out of assay range defined for the linear method.'),
	('4',FALSE,'Absorbance','Assay rng/dilu, Low ''A'' Error, High ''A'' Error','The photometric reading was out of acceptable range. These errors are also reported out as code.'),
	('5',FALSE,'Measurement System (noise, cuvette, etc.)','','During photometric measurement, the system detected some noise or variances in the absorbance.'),
	('6',TRUE,'Reagent QC','Abnormal Assay','Assay is out of the established range for the specified method.'),
	('7',TRUE,'Arithmetic Error','','The result was not able to be calculated using the current coefficients for that method.'),
	('8',TRUE,'Never Calibrated','','The reagent cartridge lot for this method was never calibrated.'),
	('9',TRUE,'No Reagent','','The system lacked sufficient reagent for this test or a hydration of a reagent failed.'),
	('10',TRUE,'Aborted Test','No Aliquots','A system action (by user or system) aborted this test.'),
	('11',TRUE,'Processing Error','','A system processing error occurred that prevented the system from the determined result.'),
	('12',TRUE,'Software Error','','Software error exists on the instrument.'),
	('13',FALSE,'"Hemoglobin"','','The sample contained enough hemoglobin to interfere with system DBIL results. However, this will not affect the TBIL results.'),
	('14',FALSE,'Abnormal Reaction','','Indicates the abnormal reaction conditions, i. e., foaming, air bubbles or turbidity problems are present in the mixture in the cuvette.'),
	('15',FALSE,'Diluted','','The test has been autodiluted by the instrument.'),
	('16',FALSE,'Below Assay Range','','Below current assay range for non-linear methods.'),
	('17',FALSE,'Above Assay Range','','Above current assay range for non-linear methods.'),
	('18',FALSE,'HIL Detected','','The amount of lipemia, hemolysis, or icterus in the specimen exceeded the threshold set in the software.'),
	('19',TRUE,'Clot Detected','','A clot was detected when the probe attempted to aspirate sample for the test.'),
	('',FALSE,'','','');

CREATE TABLE RejectReasons(
	Reason_ID int NOT NULL AUTO_INCREMENT,
	Reason varchar(50),
	ReasonCode char(1),
 CONSTRAINT PK_RejectReasons PRIMARY KEY CLUSTERED (Reason_ID ASC));

INSERT INTO RejectReasons
		(Reason, ReasonCode)
VALUES
	('',''),
	('','0'),
	('Request in process.','1'),
	('Result no longer available.','2'),
	('Sample carrier in use.','3'),
	('No memory to store request.','4'),
	('Error in test request.','5'),
	('Reserved','6'),
	('Sample Carrier full.','7'),
	('No known carriers.','8'),
	('Incorrect Fluid Type','9');

CREATE TABLE SampleTypes(
	SampleTypeCode varchar(1) NOT NULL,
	SampleTypeWord varchar(16),
 CONSTRAINT PK_SampleTypes PRIMARY KEY CLUSTERED (SampleTypeCode ASC));

INSERT INTO SampleTypes
	(SampleTypeCode,SampleTypeWord)
VALUES
	('1','Serum'),
	('2','Plasma'),
	('3','Urine'),
	('4','CSF'),
	('5','SerumQC1'),
	('6','SerumQC2'),
	('7','SerumQC3'),
	('8','UrineQC1'),
	('9','UrineQC2'),
	('W','Whole Blood');

CREATE TABLE Priorities(
	PriorityCode smallint NOT NULL,
	PriorityWord varchar(8),
	strPriority varchar(10),
 CONSTRAINT PK_Priorities PRIMARY KEY CLUSTERED (PriorityCode ASC));

INSERT INTO Priorities 
	(PriorityCode, PriorityWord, strPriority)
VALUES
	(0, 'Routine', '0'),
	(1, 'STAT', '1'),
	(2, 'ASAP', '2'),
	(3, 'QC', '3'),
	(4, 'XQC', '4');

CREATE TABLE TestCodes(
	ID int NOT NULL AUTO_INCREMENT,
	TestCode varchar(5) NOT NULL,
	TestName varchar(255),
	SampleType char(1) NOT NULL DEFAULT 1,
 CONSTRAINT PK_TestCodes PRIMARY KEY CLUSTERED (ID ASC));
 
 -- The TestCodes table should be populated with the full list of tests relevant to the installation environment.
 -- The SampleType should be populated with the default sample type for each test.
