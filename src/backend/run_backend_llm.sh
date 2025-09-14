#!/bin/bash

source backend/venv-llm/bin/activate
rm /tmp/humi/summary.txt
rm /tmp/humi/transcript.txt
python3 backend/run_llm.py
