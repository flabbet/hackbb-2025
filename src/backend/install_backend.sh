#!/bin/bash

VENV_DIR="venv"
REQ_FILE="requirements.txt"

echo "Starting backend install..."
echo "Model train data will be pulled from kaggle..."

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

curl -L -o /tmp/fer2013.zip\\n  https://www.kaggle.com/api/v1/datasets/download/msambare/fer2013 > /dev/null

echo "Model data pull finished successfully."
echo "Model data unpacking..."

mkdir $SCRIPT_DIR/data/

unzip /tmp/fer2013.zip -d $SCRIPT_DIR/data/ > /dev/null

echo "Unpack finished successfully."
echo "Creating python virtual env..."

if ! command -v python3 &> /dev/null
then
    echo "Python3 could not be found. Please install it first."
    exit 1
fi

if [ ! -d "$VENV_DIR" ]; then
    echo "Creating virtual environment in $VENV_DIR..."
    python3 -m venv "$VENV_DIR"
else
    echo "Virtual environment $VENV_DIR already exists."
fi

echo "Virtualenv creation finished successfully."
echo "Pulling pip dependencies..."

source "$VENV_DIR/bin/activate"
if [ -f "$REQ_FILE" ]; then
    echo "Installing dependencies from $REQ_FILE..."
    pip install -r "$REQ_FILE"
else
    echo "No requirements.txt found. Skipping dependency installation."
fi

echo "Pulling pip dependencies finished." 
echo "Backend install finished!"
