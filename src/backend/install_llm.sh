#!/bin/bash

VENV_DIR="venv-llm"
REQ_FILE="requirements-llm.txt"

echo "Starting backend install..."
echo "Installing ollama"
curl -fsSL https://ollama.com/install.sh | sh
ollama pull llama3.2

SCRIPT_DIR="$(pwd)"

$WHISPER_MODEL="ggml-small.bin"
if [ ! -f "./models/ggml-$WHISPER_MODEL.bin" ]; then
    echo "Downloading the '$WHISPER_MODEL' Whisper model for whisper.cpp..."
    $SCRIPT_DIR/download-ggml-model.sh $WHISPER_MODEL
else
    echo "Whisper model '$WHISPER_MODEL' already downloaded."
fi

echo "Model data pull finished successfully."
echo "Model data unpacking..."

echo "Unpack finished successfully."
echo "Creating python virtual env..."

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