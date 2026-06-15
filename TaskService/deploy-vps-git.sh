#!/usr/bin/env bash
set -euo pipefail

REPO_DIR="/home/abcxyz/fullstack_BTL"
REPO_URL="https://github.com/HaiQuan17-02/fullstack_BTL.git"

mkdir -p /home/abcxyz

if [ -d "$REPO_DIR/.git" ]; then
  echo "Updating existing repository..."
  cd "$REPO_DIR"
  git pull origin main
else
  echo "Cloning repository..."
  git clone "$REPO_URL" "$REPO_DIR"
  cd "$REPO_DIR"
fi

cd "$REPO_DIR/TaskService"

if ! command -v docker >/dev/null 2>&1; then
  echo "Installing Docker..."
  sudo apt update
  sudo apt install -y ca-certificates curl gnupg lsb-release
  curl -fsSL https://get.docker.com | sh
  sudo usermod -aG docker "$USER"
fi

if ! docker compose version >/dev/null 2>&1; then
  echo "Installing Docker Compose plugin..."
  sudo apt install -y docker-compose-plugin
fi

sudo docker compose up -d --build

echo "Deployment finished."
echo "API: http://103.178.235.78:8080"
echo "RabbitMQ UI: http://103.178.235.78:15672"
