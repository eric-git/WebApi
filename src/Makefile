# -----------------------------------------
# Colors
# -----------------------------------------
GREEN  := \033[32m
YELLOW := \033[33m
BLUE   := \033[34m
RESET  := \033[0m

# -----------------------------------------
# Helpers
# -----------------------------------------
define section
	@echo "$(BLUE)==> $(1)$(RESET)"
endef

define timed
	@start=$$(date +%s); \
	$(1); \
	end=$$(date +%s); \
	echo "$(GREEN)Completed in $$((end-start))s$(RESET)"
endef

# -----------------------------------------
# Environment profiles (dev/stage/prod)
# -----------------------------------------
ENV ?= dev
COMPOSE = docker compose --profile $(ENV)

# -----------------------------------------
# Default target
# -----------------------------------------
.DEFAULT_GOAL := help

# -----------------------------------------
# Core lifecycle
# -----------------------------------------
build: ## Build all images
	$(call section,Building ($(ENV))...)
	$(call timed,$(COMPOSE) build)

build-no-cache: ## Build without cache
	$(call section,Building without cache ($(ENV))...)
	$(call timed,$(COMPOSE) build --no-cache)

up: ## Start stack
	$(call section,Starting stack ($(ENV))...)
	$(call timed,$(COMPOSE) up -d)

down: ## Stop stack
	$(call section,Stopping stack...)
	$(COMPOSE) down

restart: ## Restart stack
	$(MAKE) down
	$(MAKE) up

ps: ## Show running containers
	$(COMPOSE) ps

# -----------------------------------------
# Volume loader
# -----------------------------------------
load: ## Run volume loader (ephemeral)
	$(call section,Loading volumes...)
	$(call timed,$(COMPOSE) run --rm load-volumes)

init: ## Full onboarding: build → load → up
	$(MAKE) build
	$(MAKE) load
	$(MAKE) up

# -----------------------------------------
# Project-specific shortcuts
# -----------------------------------------
build-api: ## Build only API image
	$(call section,Building API...)
	$(call timed,$(COMPOSE) build api)

build-issuer: ## Build only Issuer image
	$(call section,Building Issuer...)
	$(call timed,$(COMPOSE) build issuer)

build-client: ## Build only Client image
	$(call section,Building Client...)
	$(call timed,$(COMPOSE) build client)

logs-api: ## Logs for API
	$(COMPOSE) logs -f api

logs-issuer: ## Logs for Issuer
	$(COMPOSE) logs -f issuer

logs-client: ## Logs for Client
	$(COMPOSE) logs -f client

sh-api: ## Shell into API
	$(COMPOSE) exec api sh

sh-issuer: ## Shell into Issuer
	$(COMPOSE) exec issuer sh

sh-client: ## Shell into Client
	$(COMPOSE) exec client sh

# -----------------------------------------
# Cleaning
# -----------------------------------------
clean: ## Remove containers + volumes
	$(call section,Cleaning containers + volumes...)
	$(COMPOSE) down -v

prune: ## Aggressive Docker cleanup
	$(call section,Pruning system...)
	docker system prune -af

reset: ## Full reset: prune + init
	$(MAKE) prune
	$(MAKE) init

# -----------------------------------------
# Utility
# -----------------------------------------
images: ## List images
	docker images

volumes: ## List volumes
	docker volume ls

networks: ## List networks
	docker network ls

# -----------------------------------------
# Help
# -----------------------------------------
help: ## Show help
	@echo ""
	@echo "$(YELLOW)Available commands:$(RESET)"
	@echo ""
	@grep -E '^[a-zA-Z0-9_-]+:.*?##' $(MAKEFILE_LIST) \
		| sed 's/:.*##/: /' \
		| column -t -s ':'
	@echo ""
	@echo "$(YELLOW)Current environment:$(RESET) $(ENV)"
	@echo "Switch with: make ENV=stage up"
	@echo ""
