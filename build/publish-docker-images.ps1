param([string] $version)

Set-Location "../"

# build docker images according to docker-compose
docker-compose -f docker-compose.yml build

# rename images with following tag
docker tag saas-sapico-sts saas-sapico-sts:$version
docker tag saas-sapico-sts.admin saas-sapico-sts.admin:$version
docker tag saas-sapico-sts.admin.api saas-sapico-sts.admin.api:$version

# push to registry
docker push saas-sapico-sts:$version
docker push saas-sapico-sts.admin:$version
docker push saas-sapico-sts.admin.api:$version