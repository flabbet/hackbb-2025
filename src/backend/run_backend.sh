#!/bin/bash

source backend/venv/bin/activate

python3 backend/emotions.py --mode $1 --screen $2 
