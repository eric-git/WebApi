SELECT format('REASSIGN OWNED BY %I TO db_manager', :'role_name')
WHERE EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'role_name')
\gexec

SELECT format('DROP OWNED BY %I', :'role_name')
WHERE EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'role_name')
\gexec
