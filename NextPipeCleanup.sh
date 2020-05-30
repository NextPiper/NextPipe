kubectl delete -f nextpipe-deployment-and-service.yml
kubectl delete -f mongoDB.yml
kubectl delete pvc -l app=mongo