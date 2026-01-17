DROP TABLE IF EXISTS core.client_service_scope;
DROP TABLE IF EXISTS core.client_service;
DROP TABLE IF EXISTS core.service;
DROP TABLE IF EXISTS core.client;
DROP TABLE IF EXISTS core.key;

CREATE TABLE core.service (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	name VARCHAR(255) NOT NULL
);

CREATE TABLE core.client (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	name VARCHAR(255) NOT NULL,
	email VARCHAR(255) NOT NULL
);

CREATE TABLE core.key (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	pem TEXT NOT NULL
);

CREATE TABLE core.client_service (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	client_id UUID NOT NULL REFERENCES core.client(id) ON DELETE CASCADE,
	service_id UUID NOT NULL REFERENCES core.service(id) ON DELETE CASCADE,
	key_id UUID NOT NULL REFERENCES core.key(id) ON DELETE CASCADE
);

CREATE TABLE core.client_service_scope (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	client_service_id UUID NOT NULL REFERENCES core.client_service(id) ON DELETE CASCADE,
	scope VARCHAR(255) NOT NULL
);

CREATE INDEX idx_client_service_client_id
    ON core.client_service (client_id);

CREATE INDEX idx_client_service_service_id
    ON core.client_service (service_id);

CREATE INDEX idx_client_service_key_id
    ON core.client_service (key_id);

CREATE INDEX idx_client_service_scope_client_service_id
    ON core.client_service_scope (client_service_id);