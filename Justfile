set dotenv-load := true

deploy:
    #!/usr/bin/env bash
    set -euxo pipefail

    dotnet new tool-manifest --force
    dotnet tool install dotnet-ef --version 9.0.2

    just db-update

    docker compose stop api
    docker compose up api --build -d

dev-setup-dependencies:
	#!/usr/bin/env bash
	set -euxo pipefail

	dotnet new tool-manifest --force
	dotnet tool install dotnet-ef --version 9.0.11

db-new-migration migration_name:
	dotnet ef migrations add {{migration_name}} \
		--project src/Infrastructure \
		--startup-project src/API

db-update:
	dotnet ef database update \
		--project src/Infrastructure \
		--startup-project src/API


db-reset:
	dotnet ef database drop \
		--project src/Infrastructure \
		--startup-project src/API

	dotnet ef database update \
		--project src/Infrastructure \
		--startup-project src/API

api-run port="9090":
	#!/usr/bin/env bash
	dotnet run --project src/API --urls "http://localhost:{{port}}"


setup-logging:
	#!/usr/bin/env bash
	set -eu

	LOG_DIR="/var/log/help-mate-backend"
	LOG_FILES=("api.json")

	sudo mkdir -p "$LOG_DIR"
	for FILE in "${LOG_FILES[@]}"; do
		FULL_PATH="$LOG_DIR/$FILE"

		if [ -d "$FULL_PATH" ]; then
			sudo rm -rf "$FULL_PATH"
		fi

		if [ ! -f "$FULL_PATH" ]; then
			echo "  Creating new file"
			sudo touch "$FULL_PATH"
		fi

		sudo chmod 666 "$FULL_PATH"
	done

	echo
	ls -l "$LOG_DIR"


search-logs tracking_id log_path="/tmp/api.json":
	cat {{log_path}} | jq 'select(.Properties.TrackingId=="{{tracking_id}}")'
