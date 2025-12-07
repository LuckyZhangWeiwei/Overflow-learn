dotnet user-secrets set "Parameters:typesense-api-key" "xyz"
dotnet user-secrets list

https://bfritscher.github.io/typesense-dashboard

dotnet tool install -g aspire.cli
aspire publish -o infra

docker compose up -d keycloak
docker volume ls

docker run --rm -v keycloak-data:/opt/keycloak/data -v ${PWD}/realms:/opt/keycloak/export quay.io/keycloak/keycloak:26.4 export --realm overflow-learn --dir /opt/keycloak/export --users realm_file

keycloak - section6
username: prod-admin
password: !QA2ws3e8

D:\dotnet\Overflow-learn [main ≡ +1 ~0 -0 | +3 ~7 -0 !]>

dotnet publish .\SearchService\SearchService.csproj -t:PublishContainer
dotnet publish .\QuestionService\QuestionService.csproj -t:PublishContainer