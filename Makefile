.PHONY: restore build test format db-update run health

restore:
	dotnet restore

build:
	dotnet build --configuration Release

test:
	dotnet test --configuration Release

format:
	dotnet format --verify-no-changes --verbosity diagnostic

db-update:
	dotnet tool restore
	dotnet ef database update --project src/Identity.Infrastructure --startup-project src/Identity.Api

run:
	dotnet run --project src/Identity.Api

health:
	curl http://localhost:8081/health
