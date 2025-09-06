# Estágio 1: Build da Aplicação (usando a imagem SDK completa do .NET 9)
#----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copia e restaura as dependências primeiro para otimizar o cache do Docker
COPY ["KanbanAPI/KanbanAPI.csproj", "KanbanAPI/"]
RUN dotnet restore "KanbanAPI/KanbanAPI.csproj"

# Copia o restante do código fonte
COPY . .
WORKDIR "/src/KanbanAPI"

# Compila e publica a aplicação em modo de Release
RUN dotnet publish "KanbanAPI.csproj" -c Release -o /app/publish

# Estágio 2: Imagem Final (usando a imagem ASP.NET runtime do .NET 9, que é menor)
#----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Copia apenas os arquivos publicados do estágio de 'build'
COPY --from=build /app/publish .

# Define o comando de entrada para a aplicação
ENTRYPOINT ["dotnet", "TaskFlow.dll"]