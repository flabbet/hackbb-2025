#!/bin/bash

source backend/venv/bin/activate

[ ! -p /tmp/my_pipe ] && mkfifo /tmp/my_pipe
python3 backend/emotions.py --mode $1 --screen $2 > /tmp/emotions_feed
