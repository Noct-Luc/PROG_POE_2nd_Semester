# PROG_POE_2nd_Semester
## YouTube
https://youtu.be/Rre0CA1Fvw0?si=_IYhLI3drS4w4vfa
## YT part 2
https://youtu.be/nuYTemGQsAE?si=Q3Bdlq0ZFe8KfX07 
## YT part 3 Just no
https://youtu.be/wIzyt8f9tVs?si=LexgZ7z-fnY8o-GQ 
## CI/CD workflow
<img width="1277" height="269" alt="image" src="https://github.com/user-attachments/assets/e2fcf130-ce86-41e1-b609-0f85c7e28479" />

## ERD and API endpoint
<img width="1101" height="527" alt="image" src="https://github.com/user-attachments/assets/b4cb9024-2e1e-4a50-a015-98df82de37b9" />
<img width="1421" height="840" alt="image" src="https://github.com/user-attachments/assets/77cfc3ca-8537-4ea7-8fcf-5f260d739444" />
Anny was not ok with this POE

## Docker Setup

### Prerequisites
- Docker Desktop (v20.10+)
- Docker Compose (v1.29+)

### Build and Run with Docker

#### Option 1: Using Docker Compose (Recommended)

1. Create a `.env` file in the project root (copy from `.env.example`):

3. Access the application:
- MVC Application: `http://localhost:8080`
- API Service: `https://localhost:7176`

#### Option 2: Running MVC Container Only

1. Build the MVC image:


### Environment Variables

Configure the following environment variables when running the container:

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | ASP.NET Core environment (Production/Development/Staging) |
| `ApiSettings__BaseUrl` | `https://raceday-api:7176` | API base URL |
| `ApiSettings__AllowedOrigins` | `https://raceday-api:7176` | Allowed CORS origins |

### Health Checks

The container includes a health check that verifies the application is running:


Look for the `(healthy)` status in the output.

### Docker Networking

When using Docker Compose, both services are connected via the `raceday-network` bridge network. Services can communicate using their container names (e.g., `raceday-api`, `raceday-mvc`).

### Troubleshooting

**Container exits immediately:**

**Cannot reach API from MVC:**
- Ensure both containers are on the same network
- Verify environment variables are correctly set
- Check firewall/network policies

**Port already in use:**
# Using Docker Compose (recommended)
docker-compose up --build

# Standalone container
docker build -f RaceDay.MVC/Dockerfile -t raceday-mvc:latest .
docker run -p 8080:8080 -e ApiSettings__BaseUrl=https://localhost:7176 raceday-mvc:latest
