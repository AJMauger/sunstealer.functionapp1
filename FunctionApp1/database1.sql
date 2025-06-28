/* alter database [database1] 
set change_tracking = on 
(change_retention = 2 DAYS, auto_cleanup = ON);*/
/* 
alter table [table1] 
enable change_tracking;
*/

use master
go

if not exists (select name from sys.databases where name = N'database1')
  create database database1
go

drop column master key CMK
GO

create column master key [CMK]
with (
    KEY_STORE_PROVIDER_NAME = 'AZURE_KEY_VAULT',
    KEY_PATH = 'https://sunstealerkvx.vault.azure.net/keys/AlwaysEncrypted/142ec06b980d4d1c9ffd496dd96eb9dd'
)

use database1
if object_id('database1..table1', 'U') is not null
  drop table database1..table1
go

CREATE TABLE [dbo].[table1](
	[UUID] [int] IDENTITY(1,1) NOT NULL,
	[Number1] [int] NOT NULL,
	[Text1] [nvarchar](max) NOT NULL,
	[Encrypted1] [nvarchar](max) COLLATE Latin1_General_BIN2 ENCRYPTED WITH (COLUMN_ENCRYPTION_KEY = [CEK], ENCRYPTION_TYPE = Deterministic, ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') NOT NULL,
	[Date1] [date] NULL,
 CONSTRAINT [PK_table1] PRIMARY KEY CLUSTERED 
(
	[UUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

delete from database1..table1;
go

declare @Encrypted1 as nvarchar(max) = N'Encrypted';
insert into database1..table1 ([Number1], [Text1], [Encrypted1], [Date1]) values (1, N'ClearText', @Encrypted1, '01/01/2025')
go

update table1 set [Number1] = 1, [Text1] = N'ClearText' where UUID = 1
go

select * from database1..table1
go

if exists (select * from sys.views join sys.schemas on sys.views.schema_id = sys.schemas.schema_id and sys.views.name = N'View1')
  drop view View1
go
create view View1 as select [UUID], [Number1], [Text1], [Encrypted1], [Date1] from database1..table1
go

select * from database1..View1
go

/* ajm: disbale delete trigger 
create trigger trg_FilterDelete
on dbo.table1
after insert, update
AS*/

create procedure [dbo].[sp_curses] as

set nocount on;

declare @UUID as int;
declare @Number1 as int;
declare @Text1 as varchar(max);
declare @Date1 as datetime;

create table #Temp1 (
    [UUID]     int           not null,
    [Number1]  int           not null,
    [Text1]    nvarchar(max) not null,
    [Date1]    date          null,
);

declare @message as varchar(max);

declare curses cursor for select UUID, Number1, Text1, Date1 from [table1]; 
open curses;
fetch next from curses into @UUID, @Number1, @Text1, @Date1;

while @@FETCH_STATUS = 0
begin
    select @message = '-----: ' + @Text1
    print @message
	  insert into #Temp1 (UUID, Number1, Text1, Date1) values (@UUID, @Number1, @Text1, (select Text1 from table1 where UUID = @UUID), @Date1);
    fetch next from curses into @UUID, @Number1, @Text1, @Date1;
END

select * from #Temp1;

close curses;
deallocate curses;

drop table #Temp1;