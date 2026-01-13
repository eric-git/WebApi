REASSIGN OWNED BY :"role_name" TO db_manager;
DROP OWNED BY :"role_name";
DROP SCHEMA IF EXISTS :"schema_name" CASCADE;

CREATE SCHEMA :"schema_name";
ALTER SCHEMA :"schema_name" OWNER TO db_manager;

GRANT USAGE  ON SCHEMA :"schema_name" TO :"role_name";
GRANT CREATE ON SCHEMA :"schema_name" TO :"role_name";

ALTER DEFAULT PRIVILEGES IN SCHEMA :"schema_name"
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO :"role_name";

ALTER DEFAULT PRIVILEGES IN SCHEMA :"schema_name"
    GRANT USAGE, SELECT ON SEQUENCES TO :"role_name";

ALTER DEFAULT PRIVILEGES IN SCHEMA :"schema_name"
    GRANT EXECUTE ON FUNCTIONS TO :"role_name";

ALTER DEFAULT PRIVILEGES IN SCHEMA :"schema_name"
    GRANT EXECUTE ON ROUTINES TO :"role_name";

ALTER DEFAULT PRIVILEGES IN SCHEMA :"schema_name"
    GRANT USAGE ON TYPES TO :"role_name";
