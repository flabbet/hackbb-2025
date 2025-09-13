#!/bin/bash

echo "Starting dependency install..."
echo "Model train data will be pulled from kaggle..."

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

curl -L -o /tmp/fer2013.zip\\n  https://www.kaggle.com/api/v1/datasets/download/msambare/fer2013 > /dev/null

echo "Model data pull finished successfully."
echo "Model data unpacking..."

mkdir $SCRIPT_DIR/data/

unzip /tmp/fer2013.zip -d $SCRIPT_DIR/data/ > /dev/null

echo "Unpack finished successfully."
echo "Dependency install finished."
