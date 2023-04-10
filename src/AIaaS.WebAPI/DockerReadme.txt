from the solution:
docker build -f src/AIaaS.WebAPI/Dockerfile .

clean
docker image prune -a

how to debug
docker logs guid
docker commit guid testimg
docker run -ti --entrypoint=sh testimg