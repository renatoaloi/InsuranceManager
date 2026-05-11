#!/usr/bin/env python3
"""
Huey Worker Runner
Run with: python run_worker.py
"""
import os
import sys

# Add current directory to path so huey_config can be imported
sys.path.insert(0, os.path.dirname(__file__))

if __name__ == '__main__':
    from huey.consumer import Consumer
    from huey_config import huey

    print("Starting Huey consumer worker...")
    consumer = Consumer(huey)
    consumer.run()