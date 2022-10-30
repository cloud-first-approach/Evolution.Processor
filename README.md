# Introduction 
Processor Service

dapr run --log-level debug --app-port 3000 --app-id processor --app-protocol http --dapr-http-port 3501 --components-path ../../dapr/components -- dotnet run
# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

git branch -m master main
git fetch origin
git branch -u origin/main main
git remote set-head origin -a


## Local Development

```sh
dotnet ef migrations add InitialCreate
dotnet ef database update InitialCreate

# Run SQL Server in Local using docker
docker pull mcr.microsoft.com/mssql/server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=password@1" -p 1433:1433 --name sql -d mcr.microsoft.com/mssql/server:2017-latest


dapr run --app-ssl --app-port 3000 --app-id processor --app-protocol http --dapr-http-port 3501 --components-path ../../dapr/components -- dotnet run

cd Evolution.Processor/src/Processor.Api
dapr run --log-level debug --app-port 3000 --app-id processor --dapr-http-port 3500 --components-path ../../dapr/components -- dotnet run

```

### Getting Started
```sh

#STEP 1
kind create cluster

#STEP 2
dapr init -k

#STEP 3
kubectl create secret generic mssql --from-literal=SA_PASSWORD="password@1" -n evolution

#STEP 4 (Redis)

> helm repo add bitnami https://charts.bitnami.com/bitnami
> helm repo update
> helm install redis bitnami/redis --set image.tag=6.2

#STEP 4 (step infra on kubernetes)
kubectl apply -k deploy/k8s/infra/overlays/dev

kubectl get pods -n evolution

kubectl apply -f deploy/k8s/services

kubectl get pods -n evolution

kubectl port-forward svc/uploader-api-cluster-ip 80 -n evolution



kubectl delete -f deploy/k8s/services
kubectl delete -k deploy/k8s/infra/overlays/dev
helm uninstall redis

```