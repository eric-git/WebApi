# =========================================
# Colors
# =========================================
GREEN  := \033[32m
YELLOW := \033[33m
BLUE   := \033[34m
RED    := \033[31m
RESET  := \033[0m

# =========================================
# Helpers
# =========================================
define section
	@echo "$(BLUE)==> $(1)$(RESET)"
	@echo ""
endef

define timed
	@set -eu; \
	start=$$(date +%s); \
	{ $(1); }; \
	end=$$(date +%s); \
	echo "$(GREEN)Completed in $$((end-start))s$(RESET)"; \
	echo ""
endef

define require-secret
	@if [ ! -f "$(1)" ]; then \
		echo "$(RED)Missing secret: $(1)$(RESET)"; \
		exit 1; \
	fi
endef

# =========================================
# Compose wrapper
# =========================================
COMPOSE = docker compose

# =========================================
# Base compose file
# =========================================
BASE_FILE = ./docker/docker-compose.yml

# =========================================
# Consolidated compose stacks
# =========================================

# JSON MODE COMPOSE STACK
COMPOSE_JSON = \
  -f $(BASE_FILE) \
  -f docker/api/docker-compose.yml \
  -f docker/api/docker-compose.json.yml \
  -f docker/api/docker-compose.json.loader.yml \
  -f docker/issuer/docker-compose.yml \
  -f docker/issuer/docker-compose.json.yml \
  -f docker/issuer/docker-compose.json.loader.yml \
  -f docker/client/docker-compose.yml

# POSTGRES MODE COMPOSE STACK
COMPOSE_PG = \
  -f $(BASE_FILE) \
  -f docker/api/docker-compose.yml \
  -f docker/api/docker-compose.postgres.yml \
  -f docker/api/docker-compose.postgres.loader.yml \
  -f docker/issuer/docker-compose.yml \
  -f docker/issuer/docker-compose.postgres.yml \
  -f docker/issuer/docker-compose.postgres.loader.yml \
  -f docker/client/docker-compose.yml

.DEFAULT_GOAL := help

# =========================================
# JSON MODE
# =========================================

json-build: ## Build all JSON services (API, Issuer, Client)
	$(call section,Building JSON stack...)
	$(call timed,$(COMPOSE) $(COMPOSE_JSON) build)

json-build-api: ## Build API service (JSON mode)
	$(call section,Building API (json)...)
	$(call timed,$(COMPOSE) $(COMPOSE_JSON) build api)

json-build-issuer: ## Build Issuer service (JSON mode)
	$(call section,Building Issuer (json)...)
	$(call timed,$(COMPOSE) $(COMPOSE_JSON) build issuer)

json-build-client: ## Build Client service (JSON mode)
	$(call section,Building Client (json)...)
	$(call timed,$(COMPOSE) $(COMPOSE_JSON) build client)

json-up: ## Start JSON stack (API, Issuer, Client)
	$(call section,Starting JSON stack...)
	$(call timed,$(COMPOSE) $(COMPOSE_JSON) up -d issuer api client)

json-down: ## Stop JSON stack
	$(call section,Stopping JSON stack...)
	$(COMPOSE) $(COMPOSE_JSON) down
	@echo ""

json-restart: ## Restart JSON stack
	$(MAKE) json-down
	$(MAKE) json-up

json-ps: ## Show JSON container status
	$(COMPOSE) $(COMPOSE_JSON) ps
	@echo ""

json-load: ## Run JSON data loaders (API + Issuer)
	$(call section,Running JSON loaders...)
	$(call timed,$(COMPOSE) $(COMPOSE_JSON) up --abort-on-container-exit issuer-json-loader api-json-loader)
	$(COMPOSE) $(COMPOSE_JSON) rm -f issuer-json-loader api-json-loader
	@echo ""

json-init: ## Full JSON initialization (build + load + up)
	$(MAKE) json-build
	$(MAKE) json-load
	$(MAKE) json-up

json-init-up: ## Run loaders then start JSON stack
	$(MAKE) json-load
	$(MAKE) json-up

json-reset: ## Reset JSON mode (destroy volumes + re-init)
	$(call section,Resetting JSON mode...)
	$(COMPOSE) $(COMPOSE_JSON) down -v
	$(MAKE) json-init

json-logs: ## Tail logs for all JSON services
	$(call section,Aggregated logs (json)...)
	$(COMPOSE) $(COMPOSE_JSON) logs -f

json-sh: ## Open a shell in a JSON service (svc=name)
	@if [ -z "$(svc)" ]; then \
		echo "$(RED)Usage: make json-sh svc=api$(RESET)"; exit 1; \
	fi
	$(COMPOSE) $(COMPOSE_JSON) exec $(svc) sh

# =========================================
# POSTGRES MODE
# =========================================

postgres-build: ## Build all Postgres services (API, Issuer, Client)
	$(call section,Building Postgres stack...)
	$(call timed,$(COMPOSE) $(COMPOSE_PG) build)

postgres-build-api: ## Build API service (Postgres mode)
	$(call section,Building API (postgres)...)
	$(call timed,$(COMPOSE) $(COMPOSE_PG) build api)

postgres-build-issuer: ## Build Issuer service (Postgres mode)
	$(call section,Building Issuer (postgres)...)
	$(call timed,$(COMPOSE) $(COMPOSE_PG) build issuer)

postgres-build-client: ## Build Client service (Postgres mode)
	$(call section,Building Client (postgres)...)
	$(call timed,$(COMPOSE) $(COMPOSE_PG) build client)

postgres-up: ## Start Postgres stack (DBs + services)
	$(call section,Starting Postgres stack...)
	$(call timed,$(COMPOSE) $(COMPOSE_PG) up -d postgres-issuer postgres-api issuer api client)

postgres-down: ## Stop Postgres stack
	$(call section,Stopping Postgres stack...)
	$(COMPOSE) $(COMPOSE_PG) down
	@echo ""

postgres-restart: ## Restart Postgres stack
	$(MAKE) postgres-down
	$(MAKE) postgres-up

postgres-ps: ## Show Postgres container status
	$(COMPOSE) $(COMPOSE_PG) ps
	@echo ""

postgres-load: ## Run SQL loaders (API + Issuer)
	$(call section,Running SQL loaders...)
	$(call timed,$(COMPOSE) $(COMPOSE_PG) up --abort-on-container-exit issuer-postgres-loader api-postgres-loader)
	$(COMPOSE) $(COMPOSE_PG) rm -f issuer-postgres-loader api-postgres-loader
	@echo ""

postgres-init: ## Full Postgres initialization (DBs + loaders + services)
	$(call section,Postgres full initialization...)
	$(COMPOSE) $(COMPOSE_PG) up -d postgres-issuer postgres-api
	$(MAKE) postgres-load
	$(COMPOSE) $(COMPOSE_PG) up -d issuer api client
	@echo "$(GREEN)Postgres mode fully initialized$(RESET)"

postgres-reset: ## Reset Postgres mode (destroy volumes + re-init)
	$(call section,Resetting Postgres mode...)
	$(COMPOSE) $(COMPOSE_PG) down -v
	$(MAKE) postgres-init

postgres-logs: ## Tail logs for all Postgres services
	$(call section,Aggregated logs (postgres)...)
	$(COMPOSE) $(COMPOSE_PG) logs -f

postgres-sh: ## Open a shell in a Postgres service (svc=name)
	@if [ -z "$(svc)" ]; then \
		echo "$(RED)Usage: make postgres-sh svc=postgres-api$(RESET)"; exit 1; \
	fi
	$(COMPOSE) $(COMPOSE_PG) exec $(svc) sh

# =========================================
# UTILITIES
# =========================================

secrets-check: ## Validate required secret files exist
	$(call section,Validating secrets...)
	$(call require-secret,./secrets/db-manager-password.txt)
	$(call require-secret,./secrets/svc-issuer-password.txt)
	$(call require-secret,./secrets/svc-api-password.txt)
	@echo "$(GREEN)All required secrets are present$(RESET)"
	@echo ""

status: ## Show system status (containers, volumes, networks)
	$(call section,System status...)
	@docker ps
	@echo ""
	@docker volume ls
	@echo ""
	@docker network ls
	@echo ""
	@echo "$(GREEN)Status summary complete$(RESET)"
	@echo ""

nuke: ## Destroy all containers, volumes, images, networks for this project
	$(call section,NUKING environment...)

	@echo "$(YELLOW)Removing containers...$(RESET)"

	# Remove containers with project label
	-@docker ps -aq --filter "label=project=webapi-suite" | xargs -r docker rm -f

	# Remove containers whose NAMES start with the project prefix
	-@docker ps -a --format '{{.Names}}' | grep '^webapi-suite' | xargs -r docker rm -f

	# Remove containers whose DNS ALIASES start with the project prefix
	-@docker ps -aq | while read cid; do \
		if docker inspect "$$cid" | grep -q '"webapi-suite-'; then \
			docker rm -f "$$cid"; \
		fi; \
	done

	@echo "$(YELLOW)Removing volumes...$(RESET)"
	-@docker volume ls -q | grep '^webapi-suite_' | xargs -r docker volume rm -f

	@echo "$(YELLOW)Removing images...$(RESET)"
	-@docker images "webapi-suite/*" -q | xargs -r docker rmi -f

	@echo "$(YELLOW)Pruning dangling images volumes and build cache...$(RESET)"
	-@docker image prune -f
	-@docker volume prune -f
	-@docker builder prune -f

	@echo "$(YELLOW)Pruning unused networks...$(RESET)"
	-@docker network prune -f

	@echo "$(GREEN)Environment fully destroyed$(RESET)"
	@echo ""

# =========================================
# HELP
# =========================================

help: ## Show this help message
	@echo ""
	@echo "$(YELLOW)Available commands:$(RESET)"
	@echo ""

	@echo "$(BLUE)JSON MODE$(RESET)"
	@awk 'BEGIN {FS=":.*##"} /^json-[a-zA-Z0-9_.-]+:.*##/ {printf "  %-24s %s\n", $$1, $$2}' $(MAKEFILE_LIST)
	@echo ""

	@echo "$(BLUE)POSTGRES MODE$(RESET)"
	@awk 'BEGIN {FS=":.*##"} /^postgres-[a-zA-Z0-9_.-]+:.*##/ {printf "  %-24s %s\n", $$1, $$2}' $(MAKEFILE_LIST)
	@echo ""

	@echo "$(BLUE)UTILITIES$(RESET)"
	@awk 'BEGIN {FS=":.*##"} /^(secrets-check|status|nuke|help):.*##/ {printf "  %-24s %s\n", $$1, $$2}' $(MAKEFILE_LIST)
	@echo ""
