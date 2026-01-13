DROP TABLE IF EXISTS core.relation;
DROP TABLE IF EXISTS core.game;

CREATE TABLE core.game (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	type VARCHAR(50) NOT NULL,
	name VARCHAR(500) NOT NULL,
	player_name VARCHAR(255) NOT NULL,
	player_health INTEGER NOT NULL
);

CREATE TABLE core.relation (
	id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	game_id UUID NOT NULL REFERENCES core.game(id) ON DELETE CASCADE,
	type VARCHAR(50) NOT NULL,
	name VARCHAR(500) NOT NULL,
	attributes JSONB NOT NULL
);

CREATE INDEX idx_relation_game_id
    ON core.relation (game_id);