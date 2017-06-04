# Transferring MySQL data from Windows to Linux


## Dump your data in Windows

// TODO: add the dump script


## Upload to Linux

You must have [Ubuntu on Windows 10](http://www.howtogeek.com/249966/how-to-install-and-use-the-linux-bash-shell-on-windows-10/) (otherwise use WinSCP). Your local windows files are under /mnt:

	$ cd /mnt/c/path/to/your/dump.sql

Upload to Linux (use ~ so you are sure that you have the permissions):

	$ scp your_sql_dump.sql username@remote_ip:~


## How To Install MySQL on Ubuntu 14.04

From [this guide](https://www.digitalocean.com/community/tutorials/how-to-install-mysql-on-ubuntu-14-04): 

	$ sudo apt-get update
	$ sudo apt-get install mysql-server
	$ sudo mysql_secure_installation
	$ sudo mysql_install_db
	

## Import file on Linux

First create the database:

	$ mysql -u your_user_name -p
	$ CREATE DATABASE database_name;

Then import the mysql file

	$ mysql -u <user> -p<password> <dbname> < file.sql
	
	
## Download file from linux

        $ scp remote-srv-username@remote-srv-ip:~/path/to/your/file.extension /mnt/c/???/file.extension

Warning: `/mnt/c` doesn't work due to permission issues.
	

