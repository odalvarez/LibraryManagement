# Library Management System

Sistema web para registrar libros y autores, desarrollado como prueba técnica. La API está construida en .NET 9 con arquitectura en capas, base de datos SQL Server y un frontend sencillo en HTML/JS.

---

## Requisitos

- .NET 9 SDK
- Docker Desktop
- dotnet-ef tools: `dotnet tool install --global dotnet-ef`

---

## Cómo correrlo

La forma más fácil es con Docker, que levanta tanto la base de datos como la API:

```bash
docker-compose up -d --build
```

Después de unos segundos la API queda disponible en `http://localhost:5000`. El frontend está en esa misma dirección y Swagger en `http://localhost:5000/swagger`.

Si prefieres correrlo sin Docker, primero levanta solo SQL Server:

```bash
docker-compose up -d db
```

Y luego la API de forma local:

```bash
dotnet run --project src/LibraryManagement.Api
```

---

## Tests

```bash
dotnet test
```

Para correr un test específico:

```bash
dotnet test --filter "FullyQualifiedName~BookServiceTests.Create_WhenMaxReached"
```

---

## Configuración

El número máximo de libros permitidos se controla con `BookSettings:MaxAllowed` en `appsettings.json`. También se puede sobreescribir con la variable de entorno `BookSettings__MaxAllowed` sin necesidad de recompilar.

---

## Decisiones de diseño

Se usó .NET 9 interpretando "Framework 4.5 o mayor" como versión mínima. La API sigue convenciones REST estándar en lugar de MVC con vistas, lo que facilita el consumo desde el frontend y desde herramientas como Swagger.

El límite de libros se configuró en `appsettings.json` en lugar de implementar un sistema de roles o permisos, que estaría fuera del alcance de la prueba.

Los registros eliminados no se borran físicamente de la base de datos. El campo `IsDeleted` se marca como `true` y un filtro global en EF Core los excluye de todas las consultas automáticamente. Esto preserva la integridad referencial y permite recuperar datos si es necesario.

Para producción se usa Azure SQL Edge en una VM ARM, levantado con `docker-compose -f docker-compose.prod.yml up -d`. El deploy es manual para mantener control sobre los releases.

---

## Extras incluidos

El enunciado no pedía lo siguiente, pero se incluyó para reflejar prácticas de desarrollo reales:

**Manejo global de excepciones.** Las excepciones de negocio (`MaxBooksReachedException`, `AuthorNotFoundException`, `BookNotFoundException`, `DuplicateEmailException`) se capturan en un middleware centralizado que devuelve siempre la misma estructura JSON con el código HTTP correspondiente. Las excepciones 4xx se loguean como advertencia y las 5xx como error, evitando falsos positivos en los logs. Esto elimina duplicación de lógica de manejo de errores en cada controller.

**FluentValidation.** Las validaciones de los DTOs de entrada se implementaron como clases separadas usando FluentValidation, lo que permite testearlas en aislamiento sin necesidad de levantar el contexto HTTP.

**Tests unitarios.** Se incluyeron tests con xUnit para los servicios y los validadores. Los servicios se testean con un DbContext en memoria para evitar dependencias externas. En total hay 37 tests que cubren el flujo normal y los casos de error de las reglas de negocio.

**Configuración por ambiente.** Hay tres archivos de configuración: `appsettings.json` con los valores base, `appsettings.Development.json` apuntando a SQL Server local vía Docker, y `appsettings.Production.json` apuntando a Azure SQL Edge. El connection string se puede sobreescribir con variables de entorno, lo que permite usarlo en cualquier ambiente sin recompilar.

**Dos Docker Compose.** `docker-compose.yml` usa SQL Server 2022 para desarrollo local. `docker-compose.prod.yml` usa Azure SQL Edge, que corre en ARM y es la imagen compatible con la VM de producción.
