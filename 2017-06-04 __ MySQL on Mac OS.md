# MySQL on Mac OS

	brew install mysql
	brew services start mysql 
	export PATH=$PATH:/usr/local/mysql/bin/
	$(brew --prefix mysql)/bin/mysqladmin -u root password NEWPASS 
	mysql -u root -p
