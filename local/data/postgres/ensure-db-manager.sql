SELECT EXISTS (
    SELECT 1 FROM pg_roles WHERE rolname = 'db_manager'
) AS role_exists
\gset

\if :role_exists
    ALTER ROLE db_manager
        WITH LOGIN
             SUPERUSER
             PASSWORD :'role_password';
\else
    CREATE ROLE db_manager
        WITH LOGIN
             SUPERUSER
             PASSWORD :'role_password';
\endif
