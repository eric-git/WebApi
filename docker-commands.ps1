<#
# To stop the containers, run:
& docker compose -f src/docker-compose.yml stop

# To remove the containers, run:
& docker compose -f src/docker-compose.yml down

# To view the logs, run:
& docker compose -f src/docker-compose.yml logs -f

# To view the status of the containers, run:
& docker compose -f src/docker-compose.yml ps

# To rebuild the containers, run:
& docker compose -f src/docker-compose.yml down
& docker compose -f src/docker-compose.yml up -d --build

# To build the containers without starting them, run:
& docker compose -f src/docker-compose.yml build

# To build only the API container, run:
& docker compose -f src/docker-compose.yml build api
#>