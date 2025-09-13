#!/bin/bash

source backend/venv/bin/activate

./backend/emotions.py --mode $1 --screen $2 
