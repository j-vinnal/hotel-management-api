# Docker Image Build and Push Guide

This guide explains how to build a Docker image and push it to Docker Hub.

## Build the Docker Image

To create the Docker image, use the following command:

```
docker buildx build --progress=plain -t hotelx-backend:latest .
```

## Tag the Image

After building the image, create a tag for it to prepare for pushing to Docker Hub:

```
docker tag hotelx-backend <your-docker-hub-profile>/hotelx-backend:latest
```

Replace `<your-docker-hub-profile>` with your actual Docker Hub username.

## Push the Image to Docker Hub

Finally, push the tagged image to Docker Hub using the command below:

```
docker push <your-docker-hub-profile>/hotelx-backend:latest
```

Ensure you are logged into Docker Hub before executing the push command. You can log in using:

```
docker login
```

Follow the prompts to enter your Docker Hub credentials.