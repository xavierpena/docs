# Scripts folder


## Contents

* `pscp.exe`: tool to upload a windows file to a linux server
* `config.bat`: contains all the private configuration variables (passwords, ip's, etc)
* `dump-mysql-locally.bat`: creates a local .sql dump of the data in the database
* `upload-sql.bat`: uploads the latest .sql dump to the linux server


## pscp.exe

Download [pscp.exe](https://the.earth.li/~sgtatham/putty/latest/w64/pscp.exe) from [putty download page](http://www.chiark.greenend.org.uk/~sgtatham/putty/latest.html).

The [pscp documentation](https://the.earth.li/~sgtatham/putty/0.60/htmldoc/Chapter5.html):

	pscp.exe path\to\your\file.extension your_user_name@XX.XX.XX.XX:/etc/hosts
	pscp.exe -r path\to\your\folder your_user_name@XX.XX.XX.XX:/etc/hosts


## config.bat

	set linux_user=your_user_name
	set linux_ip=XX.XX.XX.XX

	set mysqldump_path="C:\path\to\you\mysqldump.exe"
	set mysql_user=your_mysql_username
	set mysql_pass=your_mysql_password
	set mysql_schema=your_mysql_schema_name

	
## dump-mysql-locally.bat

	call config.bat

	@echo off

	for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
	set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
	set "HH=%dt:~8,2%" & set "Min=%dt:~10,2%" & set "Sec=%dt:~12,2%"

	set "fullstamp=%YYYY%-%MM%-%DD%_%HH%-%Min%-%Sec%"
	set "file_name_with_date=..\db-dumps\dump_%fullstamp%.sql"
	set "file_name_without_date=..\db-dumps\newest_dump.sql"

	@echo on

	%mysqldump_path% -u %mysql_user% -p%mysql_pass% %mysql_schema% > %file_name_with_date%

	:: Always copy it as `newest_dump.sql`:
	copy %file_name_with_date% %file_name_without_date% /Y

	PAUSE

	
## upload-sql.bat

	call config.bat
	:: if you upload a folder use -r before the folder path, for "recursive":
	pscp.exe ../db-dumps/newest_dump.sql %linux_user%@%linux_ip%:/home/%linux_user%
	PAUSE
