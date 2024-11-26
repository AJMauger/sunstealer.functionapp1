-- Create a new database called 'DatabaseName'
-- Connect to the 'master' database to run this snippet
USE master
GO
-- Create the new database if it does not exist already
IF NOT EXISTS (
  SELECT name FROM sys.databases WHERE name = N'database1'
)
CREATE DATABASE database1
GO


USE database1
-- Create a new table called 'Table1' in schema 'Schema1'
-- Drop the table if it already exists
IF OBJECT_ID('database1..Table1', 'U') IS NOT NULL
DROP TABLE database1..Table1
GO
-- Create the table in the specified schema
CREATE TABLE database1..Table1
(
  UUID INT IDENTITY NOT NULL PRIMARY KEY,
  -- primary key column
  Number1 [INT] NOT NULL,
  Text1 [NVARCHAR](50) NOT NULL,
  [Date1] [DATE] NULL,
  -- specify more columns here
);
GO

-- Insert rows into table 'TableName'
INSERT INTO database1..Table1
( -- columns to insert data into
 [Number1], [Text1]
)
VALUES
( -- first row: values for the columns in the list above
 1, 'One'
),
( -- second row: values for the columns in the list above
 2, 'Two'
)
-- add more rows here
GO

-- Update rows in table 'TableName'
UPDATE Table1
SET
  [Number1] = 3,
  [Text1] = 'Three'
  -- add more columns and values here
WHERE UUID = 2	/* add search conditions here */
GO

-- Select rows from a Table or View DATABASE1.'TABLE1 schema 'SchemaName'
SELECT * FROM database1..Table1
-- WHERE 	/* add search conditions here */
GO

-- Create a new view called 'ViewName' in schema 'SchemaName'
-- Drop the view if it already exists
IF EXISTS (
SELECT *
  FROM sys.views
  JOIN sys.schemas
  ON sys.views.schema_id = sys.schemas.schema_id
  -- WHERE sys.schemas.name = N'SchemaName'
  AND sys.views.name = N'View1'
)
DROP VIEW View1
GO
-- Create the view in the specified schema
CREATE VIEW View1
AS
  -- body of the view
  SELECT [UUID],
    [Number1],
    [Text1],
    [Date1]
  FROM database1..table1
GO

-- Select rows from a Table or View DATABASE1.'TABLE1 schema 'SchemaName'
SELECT * FROM database1..View1
-- WHERE 	/* add search conditions here */
GO

delete from database1..Table1;
go

INSERT INTO database1..Table1 ([Number1], [Text1], [Date1]) VALUES
(1, 'One', null),
(2, 'Two', '1/1/1'),
(3, 'Three', GETDATE()),
(4, 'Four', GETDATE()),
(5, 'Five', GETDATE()),
(6, 'Six', GETDATE()),
(7, 'Seven', GETDATE()),
(8, 'Eight', GETDATE()),
(9, 'Nine', GETDATE()),
(10, 'Ten', GETDATE())
go

CREATE PROCEDURE [dbo].[sp_curses]
AS

SET NOCOUNT ON;

DECLARE @UUID as int;
DECLARE @Number1 as int;
DECLARE @Text1 as varchar(max);
DECLARE @Date1 as datetime;

CREATE TABLE #Temp1 (
    [UUID]    INT           NOT NULL,
    [Number1] INT           NOT NULL,
    [Text1]   NVARCHAR (50) NOT NULL,
    [Text2]   NVARCHAR (50) NOT NULL,
    [Date1]   DATE          NULL,
);

DECLARE @message as varchar(max);

DECLARE curses CURSOR FOR SELECT UUID, Number1, Text1, Date1 FROM [Table1]; 
OPEN curses;
FETCH NEXT FROM curses INTO @UUID, @Number1, @Text1, @Date1;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @message = '-----: ' + @Text1
    PRINT @message

	INSERT INTO #Temp1 (UUID, Number1, Text1, Text2, Date1) VALUES (@UUID, @Number1, @Text1, (select Text1 from Table1 where UUID = @UUID), @Date1);

    FETCH NEXT FROM curses INTO @UUID, @Number1, @Text1, @Date1;
END

SELECT * FROM #Temp1;

CLOSE curses;
DEALLOCATE curses;

DROP TABLE #Temp1;