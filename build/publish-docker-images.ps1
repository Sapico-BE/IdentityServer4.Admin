param([string] $version)

Set-Location "../"

# build docker images according to docker-compose
docker-compose -f docker-compose.yml build

# rename images with following tag
docker tag sts sts:$version
docker tag sts.admin sts.admin:$version
docker tag sts.admin.api sts.admin.api:$version

# push to registry
docker push sts:$version
docker push sts.admin:$version
docker push sts.admin.api:$version