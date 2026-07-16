## 3. Add a migration for the column rename

From your backend folder:

cd ~/projects/arcana-ai/backend

Run:

dotnet ef migrations add RenameIdColumnsToSnakeCase \
  --project Tarot.Infrastructure \
  --startup-project Tarot.Api \
  --output-dir Persistence/Migrations

Then apply it:

dotnet ef database update \
  --project Tarot.Infrastructure \
  --startup-project Tarot.Api

Then go back into PostgreSQL:

docker exec -it arcana-ai-postgres psql -U arcana_user -d arcana_ai

Check:

\d readings
\d reading_cards