#!/usr/bin/env bash

set -e

cd maskrcnn-benchmark
docker build -t maskrcnn-benchmark -f docker/Dockerfile .

docker tag maskrcnn-benchmark crashdetector.azurecr.io/maskrcnn:latest
docker push crashdetector.azurecr.io/maskrcnn:latest
