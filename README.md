# FitFusion.Api

Backend .NET 8 para las funciones IA/nutricion de FitFusion. Expone endpoints protegidos con Firebase Auth y usa Gemini para generar/estimar datos.

## Requisitos

- .NET SDK 8.
- Un proyecto Firebase con Authentication habilitado.
- Una API key de Gemini/Google AI Studio.

## Setup Local

1. Clona el repo y entra en la carpeta:

```bash
cd FitFusion.Api
```

2. Restaura dependencias:

```bash
dotnet restore FitFusion.Api/FitFusion.Api.csproj
```

3. Configura secretos locales. No los escribas en `appsettings.json` ni los subas a git:

```bash
dotnet user-secrets set "Gemini:ApiKey" "TU_GEMINI_API_KEY" --project FitFusion.Api/FitFusion.Api.csproj
dotnet user-secrets set "Firebase:ProjectId" "TU_FIREBASE_PROJECT_ID" --project FitFusion.Api/FitFusion.Api.csproj
```

Para el proyecto actual, el `Firebase:ProjectId` usado en `appsettings.json` es `fitfusiondiet`, pero puedes cambiarlo si usas otro Firebase.

4. Ejecuta el servidor:

```bash
dotnet run --project FitFusion.Api/FitFusion.Api.csproj
```

Por defecto levanta:

```text
http://localhost:5158
https://localhost:7228
```

5. Abre Swagger en desarrollo:

```text
http://localhost:5158/swagger
```

## Autenticacion

Los endpoints principales requieren un Firebase ID token:

```http
Authorization: Bearer <FirebaseIdToken>
```

El backend no necesita service account para validar usuarios: valida el token contra `https://securetoken.google.com/{Firebase:ProjectId}` usando las claves publicas de Google.

En Swagger pulsa **Authorize** y pega solo el token Bearer si quieres probar endpoints protegidos.

## Configuracion Disponible

Puedes configurar por `appsettings.json`, user-secrets o variables de entorno. En despliegues usa variables de entorno.

| Clave | Obligatoria | Uso |
| --- | --- | --- |
| `Gemini:ApiKey` | Si | API key de Gemini. |
| `Firebase:ProjectId` | Si | Proyecto Firebase usado para validar tokens. |
| `Gemini:Model` | No | Modelo Gemini. Default actual: `gemini-2.5-flash`. |
| `Gemini:BaseUrl` | No | URL base de Gemini. |
| `Gemini:TimeoutSeconds` | No | Timeout de llamadas a Gemini. |
| `ConnectionStrings:Default` | No | SQLite local. Default: `Data Source=fitfusion.db`. |

Formato equivalente en variables de entorno:

```bash
Gemini__ApiKey=TU_GEMINI_API_KEY
Firebase__ProjectId=fitfusiondiet
ConnectionStrings__Default="Data Source=fitfusion.db"
```

## Endpoints Principales

IA (Gemini):

- `POST /api/ai/recipe/kcal` — Calcula calorias de una receta a partir de ingredientes.
- `POST /api/ai/plate/estimate` — Estima macros de un plato descrito en texto.
- `POST /api/ai/routine/generate` — Genera una rutina de entrenamiento personalizada.
- `POST /api/ai/meal-plan/generate` — Genera un plan de comidas adaptado al usuario.
- `POST /api/ai/workout/estimate` — Estima calorias quemadas en una sesion.

Nutricion y MET:

- `POST /api/nutrition/calorie-goal` — Calcula las calorias diarias recomendadas (Mifflin-St Jeor + factor de actividad).
- `POST /api/met/estimate` — Calcula gasto energetico via tablas MET.

Usuarios:

- `GET /api/users/me` — Devuelve el perfil del usuario autenticado.

Todos devuelven JSON en `camelCase`, compatible con el cliente Android.

## Base De Datos

Usa SQLite por defecto. En local se crea/actualiza automaticamente al arrancar mediante migraciones EF Core y se rellena seed inicial si aplica.

El archivo por defecto es:

```text
FitFusion.Api/fitfusion.db
```

## Conectar Android Local

En el repo Android, apunta la app al backend local desde `local.properties`:

```properties
AI_API_BASE_URL=http://10.0.2.2:5158
```

`10.0.2.2` es la IP especial que usa el emulador Android para llegar al `localhost` del ordenador.

Si usas un movil fisico, sustituye por la IP local del PC, por ejemplo:

```properties
AI_API_BASE_URL=http://192.168.1.50:5158
```

## Despliegue En Azure App Service

1. Publica el proyecto:

```bash
dotnet publish FitFusion.Api/FitFusion.Api.csproj -c Release -o publish
```

2. Sube el contenido de `publish/` al App Service.

3. Configura app settings en Azure, no en el repo:

```text
ASPNETCORE_ENVIRONMENT=Production
Gemini__ApiKey=TU_GEMINI_API_KEY
Firebase__ProjectId=fitfusiondiet
```

4. En Android cambia `AI_API_BASE_URL` a la URL HTTPS del App Service:

```properties
AI_API_BASE_URL=https://tu-app.azurewebsites.net
```

## Comandos Utiles

```bash
dotnet build FitFusion.Api/FitFusion.Api.csproj
dotnet run --project FitFusion.Api/FitFusion.Api.csproj
dotnet publish FitFusion.Api/FitFusion.Api.csproj -c Release -o publish
```

## Notas De Seguridad

- Nunca subas `Gemini:ApiKey`, secretos Firebase, publish profiles ni archivos `.env`.
- La API key de Gemini va en user-secrets local o app settings del proveedor cloud.
- El cliente Android solo debe conocer `AI_API_BASE_URL`; no debe llamar directamente a Gemini.
