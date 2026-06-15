#!/usr/bin/env bash
set -euo pipefail

cd /home/abcxyz
if [ ! -d "TaskService" ]; then
  echo "Extracting project..."
  unzip -o taskservice.zip
fi

cd /home/abcxyz/TaskService

if ! command -v docker >/dev/null 2>&1; then
  echo "Installing Docker..."
  sudo apt update
  sudo apt install -y ca-certificates curl gnupg lsb-release
  curl -fsSL https://get.docker.com | sh
  sudo usermod -aG docker "$USER"
fi

if ! docker compose version >/dev/null 2>&1; then
  echo "Installing docker compose plugin..."
  sudo apt install -y docker-compose-plugin
fi

sudo docker compose up -d --build

echo "Deployment finished."
echo "API: http://103.178.235.78:8080"
echo "RabbitMQ UI: http://103.178.235.78:15672"
