# Health Check

This repository provides a small two-layer ASP.NET Core solution for validating frontend-to-backend connectivity in AWS.

## Solution structure

- `src/HealthCheck.Api` - presentation/service backend API for health status and readiness checks
- `src/HealthCheck.Frontend` - GDS-styled single-page frontend that calls the API
- `.github/workflows/ecr-publish.yml` - publishes both images to Amazon ECR after push to main
- `Jenkinsfile` - ECS deployment stub for the Jenkins environment

## Local development

Use two terminals to run the app locally.

### Prerequisite

Ensure the .NET SDK is available:

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet --version
```

### Backend API

Run the API in one terminal:

```bash
export PATH="$HOME/.dotnet:$PATH"
cd /Users/2084732/epr-calculator/health-check
dotnet run --project src/HealthCheck.Api/HealthCheck.Api.csproj --urls http://localhost:5093
```

The API will be available at:

- `http://localhost:5093`
- `https://localhost:7099`

API endpoints:

- `GET http://localhost:5093/api/health`
- `GET http://localhost:5093/api/health/detail`
- `GET http://localhost:5093/healthz`
- `GET http://localhost:5093/admin/health`

### Frontend

Run the frontend in a second terminal:

```bash
export PATH="$HOME/.dotnet:$PATH"
cd /Users/2084732/epr-calculator/health-check
dotnet run --project src/HealthCheck.Frontend/HealthCheck.Frontend.csproj --urls http://localhost:5190
```

The frontend will be available at:

- `http://localhost:5190`
- `https://localhost:7004`

The frontend calls the API using the `HealthApi:BaseUrl` setting in `src/HealthCheck.Frontend/appsettings.json`, which is set to `http://localhost:5093` for local development.

### Quick validation

Check the backend health endpoint:

```bash
curl http://localhost:5093/api/health
```

Check the frontend is responding:

```bash
curl -I http://localhost:5190
```


## GDS compliance

- uses the GOV.UK Frontend design system style classes
- follows a simple, accessible single-page layout
- includes status panels and summary lists for readable operational output

## AWS deployment

This repository is designed to publish container images to Amazon ECR. The AWS Jenkins environment is responsible for taking the ECR image and deploying it to ECS.

### GitHub Actions configuration

Add the following in GitHub:
- Settings → Secrets and variables → Actions → Secrets
- Settings → Secrets and variables → Actions → Variables

Recommended values:

- `AWS_ROLE_TO_ASSUME` -> IAM role ARN used by the GitHub workflow to push to ECR
- `AWS_REGION` -> `eu-west-1`
- `ECR_API_REPOSITORY` -> `health-check-api`
- `ECR_FRONTEND_REPOSITORY` -> `health-check-frontend`

The IAM role assumed by GitHub Actions must allow ECR login, repository inspection and creation, image upload, and image metadata operations. At minimum, grant the role the ECR actions used by the workflow, scoped to the configured repositories where practical:

- `ecr:GetAuthorizationToken`
- `ecr:DescribeRepositories`
- `ecr:CreateRepository`
- `ecr:BatchCheckLayerAvailability`
- `ecr:InitiateLayerUpload`
- `ecr:UploadLayerPart`
- `ecr:CompleteLayerUpload`
- `ecr:PutImage`

### AWS runtime configuration

Store the following in AWS Secrets Manager or SSM Parameter Store:

- `HEALTHCHECK_ENVIRONMENT`
- `HEALTHCHECK_SERVICE_NAME`
- `HEALTHCHECK_VERSION`
- `HEALTHCHECK_ALLOWED_ORIGINS`
- `HEALTHCHECK_DEPENDENCIES`
- `AWS_REGION`

Suggested AWS names:

- Secrets Manager: `health-check/api-config`
- SSM: `/health-check/environment`, `/health-check/service-name`, `/health-check/version`, `/health-check/allowed-origins`, `/health-check/dependencies`, `/health-check/aws-region`

Then expose them in the ECS task definition as environment variables or secrets.

### Local environment example

```bash
export HEALTHCHECK_ENVIRONMENT=local
export HEALTHCHECK_SERVICE_NAME=health-check-api
export HEALTHCHECK_VERSION=1.0.0
export HEALTHCHECK_ALLOWED_ORIGINS=https://localhost
export HEALTHCHECK_DEPENDENCIES=database,blob-storage,config-store
export AWS_REGION=eu-west-1
```

### Jenkins deployment

The Jenkins pipeline in [Jenkinsfile](Jenkinsfile) is the deployment entry point for the ECR images to ECS. The task definition and service details remain in the Jenkins/AWS environment configuration rather than in this repository.

## Container build example

```bash
aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin <account-id>.dkr.ecr.<region>.amazonaws.com

docker build -f Dockerfile.api -t health-check-api .
docker tag health-check-api:latest <account-id>.dkr.ecr.<region>.amazonaws.com/health-check-api:latest
docker push <account-id>.dkr.ecr.<region>.amazonaws.com/health-check-api:latest
```

## Notes

- GitHub Actions publishes the container images to ECR.
- Jenkins deploys the ECR image to ECS.
- Runtime values must come from the environment; do not hard-code secrets or service URLs in source control.
- This repo is intended as an environment connectivity test harness rather than a full business application.
