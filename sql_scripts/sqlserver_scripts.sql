-- The following SQL commands are essential to the function of the Rosie Serial LIS service.
-- Execute the following commands on the SQL Server database of your choice.

USE lisdb -- Change this to the name of your SQL Server database.
GO

CREATE TABLE [SampleData](
	[ID] int IDENTITY(1,1) NOT NULL,
	[Loadlist_ID] varchar(2) NULL,
	[Patient_ID] varchar(27) NULL,
	[Sample_No] varchar(12) NULL,
	[SampleType] varchar(1) NULL,
	[Location] varchar(6) NULL,
	[Priority] smallint NULL,
	[DateTime] varchar(12) NULL,
	[Cups] varchar(1) NULL,
	[Dilution] varchar(3) NULL,
	[TestsCount] varchar(2) NULL,
	[DateTimeFormatted]  AS (datetimefromparts(try_parse('20'+substring([DateTime],(11),(2))  AS int),substring([DateTime],(9),(2)),try_parse(ltrim(substring([DateTime],(7),(2)))  AS int),substring([DateTime],(5),(2)),substring([DateTime],(3),(2)),substring([DateTime],(1),(2)),(0))),
 CONSTRAINT [PK_SampleData] PRIMARY KEY CLUSTERED ([ID])) ON [PRIMARY]
GO

CREATE TABLE [SampleResults](
	[Sample_ID] int NULL,
	[TestName] varchar(5) NULL,
	[Result]  AS (TRY_CAST([ResultValue] AS float)),
	[Units] varchar(10) NULL,
	[Error] varchar(2) DEFAULT '',
	[Result_ID] int IDENTITY(1,1) NOT NULL,
	[ResultValue] varchar(10) NULL,
 CONSTRAINT [PK_SampleResults] PRIMARY KEY CLUSTERED ([Result_ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [CalibrationResults](
	[Cal_Test] varchar(5) NULL,
	[Cal_Units] varchar(10) NULL,
	[Reagent_Lot] varchar(10) NULL,
	[Cal_Product] varchar(10) NULL,
	[Cal_Prod_Lot] varchar(10) NULL,
	[Cal_Op] varchar(10) NULL,
	[Cal_DateTime] varchar(12) NULL,
	[Cal_Slope] float NULL,
	[Cal_Intercept] float NULL,
	[Coefficients_Num] int NULL,
	[Coefficient_0] float NULL,
	[Coefficient_1] float NULL,
	[Coefficient_2] float NULL,
	[Coefficient_3] float NULL,
	[Coefficient_4] float NULL,
	[Bottle_Vals] int NULL,
	[Val01] float NULL,
	[Res01] float NULL,
	[Val02] float NULL,
	[Res02] float NULL,
	[Val03] float NULL,
	[Res03] float NULL,
	[Val04] float NULL,
	[Res04] float NULL,
	[Val05] float NULL,
	[Res05] float NULL,
	[Val06] float NULL,
	[Res06] float NULL,
	[Val07] float NULL,
	[Res07] float NULL,
	[Val08] float NULL,
	[Res08] float NULL,
	[Val09] float NULL,
	[Res09] float NULL,
	[Val10] float NULL,
	[Res10] float NULL,
	[Val11] float NULL,
	[Res11] float NULL,
	[Val12] float NULL,
	[Res12] float NULL,
	[Val13] float NULL,
	[Res13] float NULL,
	[Val14] float NULL,
	[Res14] float NULL,
	[Val15] float NULL,
	[Res15] float NULL,
	[Val16] float NULL,
	[Res16] float NULL,
	[Val17] float NULL,
	[Res17] float NULL,
	[Val18] float NULL,
	[Res18] float NULL,
	[Val19] float NULL,
	[Res19] float NULL,
	[Val20] float NULL,
	[Res20] float NULL,
	[Calibration_ID] int IDENTITY(1,1) NOT NULL,
	[CalcDateTime]  AS (datetimefromparts(try_parse('20'+substring([Cal_DateTime],(11),(2))  AS int),substring([Cal_DateTime],(9),(2)),try_parse(ltrim(substring([Cal_DateTime],(7),(2)))  AS int),substring([Cal_DateTime],(5),(2)),substring([Cal_DateTime],(3),(2)),substring([Cal_DateTime],(1),(2)),(0))),
	[ActualDateTime] [datetime2](7) DEFAULT GETDATE(),
 CONSTRAINT [PK_CalibrationResults] PRIMARY KEY CLUSTERED ([Calibration_ID] ASC)) ON [PRIMARY]
GO

CREATE TABLE [PendingTests](
	[Position] varchar(2) NULL,
	[PatientName] varchar(27) NULL,
	[SampleNo] varchar(12) NULL,
	[Test1] varchar(5) NULL,
	[Test2] varchar(5) NULL,
	[Test3] varchar(5) NULL,
	[Test4] varchar(5) NULL,
	[Test5] varchar(5) NULL,
	[Test6] varchar(5) NULL,
	[SampleType] char(1) DEFAULT 1,
	[ToDelete] bit NOT NULL DEFAULT 0,
	[Temp_ID] int IDENTITY(1,1) NOT NULL,
	[RejectCode] char(1) DEFAULT 0,
	[intPriority] int DEFAULT 0,
	[DilFactor] int NOT NULL DEFAULT 1,
	[PendingSending] bit NOT NULL DEFAULT 0,
 CONSTRAINT [PK_TempPendings] PRIMARY KEY CLUSTERED ([Temp_ID] ASC)) ON [PRIMARY]
GO

-- The following SQL commands are not strictly required, but they should prove very helpful.

CREATE TABLE [ErrorCodes](
	[Error_Code] varchar(2) NULL,
	[Suppress_Result] bit NOT NULL DEFAULT 0,
	[Error_Interpretation] varchar(255) NULL,
	[Alt_Interpretations] varchar(255) NULL,
	[Explanation] varchar(255) NULL,
	[Error_ID] int IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_ErrorCodes] PRIMARY KEY CLUSTERED ([Error_ID] ASC)) ON [PRIMARY]
GO

INSERT INTO [ErrorCodes] 
		([Error_Code],[Suppress_Result],[Error_Interpretation],[Alt_Interpretations],[Explanation]) 
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
	('',FALSE,'','','')
GO

CREATE TABLE [RejectReasons](
	[Reason_ID] int IDENTITY(1,1) NOT NULL,
	[Reason] varchar(50) NULL,
	[ReasonCode] char(1) NULL,
 CONSTRAINT [PK_RejectReasons] PRIMARY KEY CLUSTERED ([Reason_ID] ASC)) ON [PRIMARY]
GO

INSERT INTO [RejectReasons]
		([Reason], [ReasonCode])
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
	('Incorrect Fluid Type','9')
GO

CREATE TABLE [SampleTypes](
	[SampleTypeCode] varchar(1) NOT NULL,
	[SampleTypeWord] varchar(16) NULL,
 CONSTRAINT [PK_SampleTypes] PRIMARY KEY CLUSTERED ([SampleTypeCode] ASC)) ON [PRIMARY]
GO

INSERT INTO [SampleTypes]
	([SampleTypeCode],[SampleTypeWord])
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
	('W','Whole Blood')
GO

CREATE TABLE [Priorities](
	[PriorityCode] smallint NOT NULL,
	[PriorityWord] varchar(8) NULL,
	[strPriority] varchar(10) NULL,
 CONSTRAINT [PK_Priorities] PRIMARY KEY CLUSTERED ([PriorityCode] ASC)) ON [PRIMARY]
GO

INSERT INTO [Priorities] 
	([PriorityCode], [PriorityWord], [strPriority])
VALUES
	(0, 'Routine', '0'),
	(1, 'STAT', '1'),
	(2, 'ASAP', '2'),
	(3, 'QC', '3'),
	(4, 'XQC', '4')
GO

CREATE TABLE [TestCodes](
	[ID] int IDENTITY(1,1) NOT NULL,
	[TestCode] varchar(5) NOT NULL,
	[TestName] varchar(255) NULL,
	[SampleType] char(1) NOT NULL DEFAULT 1,
 CONSTRAINT [PK_TestCodes] PRIMARY KEY CLUSTERED ([ID] ASC))
 GO
 
 -- The TestCodes table should be populated with the full list of tests relevant to the installation environment.
 -- The SampleType should be populated with the default sample type for each test.
