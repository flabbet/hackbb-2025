#!/bin/bash

source backend/venv/bin/activate

rm -f /tmp/emotions_feed
[ ! -p /tmp/emotions_feed ] && mkfifo /tmp/emotions_feed
python3 backend/emotions.py --mode $1 --screen $2 > /tmp/emotions_feed
