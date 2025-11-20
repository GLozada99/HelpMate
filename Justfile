set dotenv-load := true

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
	dotnet ef database update --project Infrastructure
