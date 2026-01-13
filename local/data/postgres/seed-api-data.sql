TRUNCATE TABLE core.game RESTART IDENTITY CASCADE;
TRUNCATE TABLE core.relation RESTART IDENTITY CASCADE;

------------------------------------------------------------------
-- Seed core.game
------------------------------------------------------------------
MERGE INTO core.game AS g
USING (
	VALUES
		('b3f8a1d2-9d4e-4c1a-8f2a-1a2b3c4d5e6f'::uuid, 'game', 'The Legend of Zelda: Breath of the Wild', 'Link', 80),
		('f0d2b5c4-7e8f-4a9b-0c1d-2e3f4a5b6c7d'::uuid, 'game', 'Minecraft', 'Steve', 20)
) AS s(id, type, name, player_name, player_health)
ON g.id = s.id
WHEN MATCHED THEN
	UPDATE SET
		type          = s.type,
		name          = s.name,
		player_name   = s.player_name,
		player_health = s.player_health
WHEN NOT MATCHED THEN
	INSERT (id, type, name, player_name, player_health)
	VALUES (s.id, s.type, s.name, s.player_name, s.player_health);

------------------------------------------------------------------
-- Seed core.relation
------------------------------------------------------------------
MERGE INTO core.relation AS r
USING (
	VALUES
		('c7a9e2f1-4b5d-4c6e-9f7a-8b9c0d1e2f3a'::uuid,
			'b3f8a1d2-9d4e-4c1a-8f2a-1a2b3c4d5e6f'::uuid,
			'quest', 'Free the Divine Beast Vah Ruta',
			'{"status":"Completed","reward":"Mipha''s Grace"}'::jsonb),

		('d8b0f3a2-5c6d-4d7e-8f9a-0b1c2d3e4f5b'::uuid,
			'b3f8a1d2-9d4e-4c1a-8f2a-1a2b3c4d5e6f'::uuid,
			'quest', 'Defeat Calamity Ganon',
			'{"status":"In Progress","reward":"Peace in Hyrule"}'::jsonb),

		('e9c1a4b3-6d7e-4f8a-9b0c-1d2e3f4a5b6c'::uuid,
			'b3f8a1d2-9d4e-4c1a-8f2a-1a2b3c4d5e6f'::uuid,
			'equipment', 'Champion''s Gear',
			'{"weapon":"Master Sword","shield":"Hylian Shield","armor":"Champion''s Tunic"}'::jsonb),

		('a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d'::uuid,
			'f0d2b5c4-7e8f-4a9b-0c1d-2e3f4a5b6c7d'::uuid,
			'quest', 'Mine Diamonds',
			'{"status":"In Progress","reward":"Diamond Pickaxe"}'::jsonb),

		('b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e'::uuid,
			'f0d2b5c4-7e8f-4a9b-0c1d-2e3f4a5b6c7d'::uuid,
			'quest', 'Defeat the Ender Dragon',
			'{"status":"Not Started","reward":"Dragon Egg"}'::jsonb),

		('c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f'::uuid,
			'f0d2b5c4-7e8f-4a9b-0c1d-2e3f4a5b6c7d'::uuid,
			'equipment', 'Starter Gear',
			'{"weapon":"Iron Sword","tool":"Wooden Pickaxe","armor":"Leather Tunic"}'::jsonb)
) AS s(id, game_id, type, name, attributes)
ON r.id = s.id
WHEN MATCHED THEN
	UPDATE SET
		game_id   = s.game_id,
		type      = s.type,
		name      = s.name,
		attributes = s.attributes
WHEN NOT MATCHED THEN
	INSERT (id, game_id, type, name, attributes)
	VALUES (s.id, s.game_id, s.type, s.name, s.attributes);
