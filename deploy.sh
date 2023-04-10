#!/usr/bin/env bash 

CURRENT_INSTANCE=$(docker ps -a -q --filter ancestor="$IMAGE_NAME:$IMAGE_TAG" --format="{{.ID}}")
if [ "$CURRENT_INSTANCE" ]
then
  docker rm $(docker stop $CURRENT_INSTANCE)
fi

docker pull $IMAGE_NAME:$IMAGE_TAG

CONTAINER_EXISTS=$(docker ps -a | grep $CONTAINER_NAME)
if [ "$CONTAINER_EXISTS" ] 
then
  docker rm $CONTAINER_NAME
fi

sudo cp MH-AIaaS/nginx.conf /etc/nginx/nginx.conf
sudo nginx -s reload

docker create -p $PUBLIC_PORT:$INTERNAL_PORT --name $CONTAINER_NAME $IMAGE_NAME:$IMAGE_TAG

rm .env
touch .env
echo "PORT_WEBAPI=$INTERNAL_PORT" | tee -a .env
echo "CLIENT_ORIGIN_URL=$CLIENT_ORIGIN_URL" | tee -a .env
echo "AUTH0_AUDIENCE=$AUTH0_AUDIENCE" | tee -a .env
echo "AUTH0_DOMAIN=$AUTH0_DOMAIN" | tee -a .env
docker cp .env $CONTAINER_NAME:/app/.env
rm .env

docker start $CONTAINER_NAME