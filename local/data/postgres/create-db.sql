SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = :'db_name'
	AND pid <> pg_backend_pid();

DROP DATABASE IF EXISTS :"db_name";
CREATE DATABASE :"db_name" OWNER db_manager;
