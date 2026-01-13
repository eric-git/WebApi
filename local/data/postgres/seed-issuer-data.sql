TRUNCATE TABLE core.service RESTART IDENTITY CASCADE;
TRUNCATE TABLE core.client RESTART IDENTITY CASCADE;
TRUNCATE TABLE core.client_service RESTART IDENTITY CASCADE;
TRUNCATE TABLE core.client_service_scope RESTART IDENTITY CASCADE;

------------------------------------------------------------------
-- Seed core.service
------------------------------------------------------------------
MERGE INTO core.service AS t
USING (
	VALUES
		('af2157c0-3280-42e7-b327-33e5aa3ec76e'::uuid, 'WebApi.Service')
) AS s(id, name)
ON t.id = s.id
WHEN MATCHED THEN
	UPDATE SET name = s.name
WHEN NOT MATCHED THEN
	INSERT (id, name)
	VALUES (s.id, s.name);

------------------------------------------------------------------
-- Seed core.client
------------------------------------------------------------------
MERGE INTO core.client AS t
USING (
	VALUES
		('088f6a2e-8f00-4340-b32d-49de4acde03c'::uuid, 'WebApi.Client')
) AS s(id, name)
ON t.id = s.id
WHEN MATCHED THEN
	UPDATE SET name = s.name
WHEN NOT MATCHED THEN
	INSERT (id, name)
	VALUES (s.id, s.name);

------------------------------------------------------------------
-- Seed core.key
------------------------------------------------------------------
MERGE INTO core.key AS t
USING (
	VALUES
		('dc6f9ca5-1337-4bcb-952c-f6df8ff9d528'::uuid,
			:'client_public_signing_key')
) AS s(id, pem)
ON t.id = s.id
WHEN MATCHED THEN
	UPDATE SET pem = s.pem
WHEN NOT MATCHED THEN
	INSERT (id, pem)
	VALUES (s.id, s.pem);

------------------------------------------------------------------
-- Seed core.client_service
------------------------------------------------------------------
MERGE INTO core.client_service AS t
USING (
	VALUES
		('cdf539f1-2dcd-4510-b011-b63d20f125e3'::uuid,
			'088f6a2e-8f00-4340-b32d-49de4acde03c'::uuid,
			'af2157c0-3280-42e7-b327-33e5aa3ec76e'::uuid,
			'dc6f9ca5-1337-4bcb-952c-f6df8ff9d528'::uuid)
) AS s(id, client_id, service_id, key_id)
ON t.id = s.id
WHEN MATCHED THEN
	UPDATE SET
		client_id  = s.client_id,
		service_id = s.service_id,
		key_id     = s.key_id
WHEN NOT MATCHED THEN
	INSERT (id, client_id, service_id, key_id)
	VALUES (s.id, s.client_id, s.service_id, s.key_id);

------------------------------------------------------------------
-- Seed core.client_service_scope
------------------------------------------------------------------
MERGE INTO core.client_service_scope AS t
USING (
	VALUES
		('88ab3352-bcc2-46d5-ae0c-f745ea4b44c0'::uuid,
			'cdf539f1-2dcd-4510-b011-b63d20f125e3'::uuid,
			'api.read'),
		('22521e4e-93a3-4830-a45b-b21d02059b95'::uuid,
			'cdf539f1-2dcd-4510-b011-b63d20f125e3'::uuid,
			'api.write')
) AS s(id, client_service_id, scope)
ON t.id = s.id
WHEN MATCHED THEN
	UPDATE SET
		client_service_id = s.client_service_id,
		scope             = s.scope
WHEN NOT MATCHED THEN
	INSERT (id, client_service_id, scope)
	VALUES (s.id, s.client_service_id, s.scope);
