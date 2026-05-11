#!/usr/bin/env python3
"""
Huey Consumer Worker for Insurance Manager
Processes status change tasks from the queue.

Run with: python Huey/huey_consumer.py
"""
import os
import sys
import json
import time
import threading
import requests

sys.path.insert(0, os.path.dirname(__file__))

huey_path = os.path.dirname(__file__)
sys.path.insert(0, os.path.dirname(huey_path))
sys.path.insert(0, os.path.join(os.path.dirname(huey_path), 'InsuranceManager.Application'))

from huey import FileHuey

huey = FileHuey(
    'insurance_huey',
    path=os.environ.get('HUEY_QUEUE_PATH', '/app/huey_data')
)

task = huey.task

API_BASE_URL = os.environ.get('API_BASE_URL', 'http://localhost:5000')
INTERNAL_API_KEY = os.environ.get('INTERNAL_API_KEY', 'internal-secret-change-me')


@task()
def process_status_change(proposal_id: str, new_status: str):
    """Process a proposal status change request."""
    print(f"Processing status change: proposal_id={proposal_id}, new_status={new_status}")

    try:
        status_map = {"Aprovada": 1, "Recusada": 2}
        new_status_int = status_map.get(new_status, 1)
        response = requests.post(
            f"{API_BASE_URL}/internal/status",
            json={"proposalId": proposal_id, "status": new_status_int},
            headers={"X-Internal-Key": INTERNAL_API_KEY},
            timeout=30
        )

        if response.status_code == 200:
            print(f"Status change successful for proposal {proposal_id}")
        else:
            print(f"Status change failed: {response.status_code} - {response.text}")
            raise Exception(f"Status change failed: {response.status_code}")

    except Exception as e:
        print(f"Request error processing status change: {e}")
        raise


def process_json_queue_with_delay(queue_dir: str):
    """Process any JSON task files in the queue directory with retry logic."""
    seen_files = set()
    retry_delay = 2

    while True:
        try:
            enqueue_files = [f for f in os.listdir(queue_dir) if f.startswith('enqueue_') and f.endswith('.json')]

            for filename in enqueue_files:
                if filename in seen_files:
                    continue

                json_file = os.path.join(queue_dir, filename)
                try:
                    with open(json_file, 'r') as f:
                        task_data = json.load(f)

                    proposal_id = task_data.get('args', [None, None])[0]
                    new_status = task_data.get('args', [None, None])[1]

                    if proposal_id and new_status:
                        print(f"Processing JSON task: proposal_id={proposal_id}, new_status={new_status}")
                        try:
                            status_map = {"Aprovada": 1, "Recusada": 2}
                            new_status_int = status_map.get(new_status, 1)
                            json_payload = {"proposalId": proposal_id, "status": new_status_int}
                            print(f"Request JSON: {json_payload}")
                            response = requests.post(
                                f"{API_BASE_URL}/internal/status",
                                json=json_payload,
                                headers={"X-Internal-Key": INTERNAL_API_KEY},
                                timeout=30
                            )

                            print(f"Response: {response.status_code} - {response.text}")

                            if response.status_code == 200:
                                print(f"Status change successful for proposal {proposal_id}")
                                os.remove(json_file)
                                print(f"Processed and removed: {json_file}")
                                seen_files.discard(filename)
                            else:
                                print(f"Failed to process {filename}: {response.status_code} - {response.text}")
                                seen_files.add(filename)
                                time.sleep(retry_delay)

                        except requests.exceptions.RequestException as e:
                            print(f"Request error processing {json_file}: {e}")
                            seen_files.add(filename)
                            time.sleep(retry_delay)

                        except Exception as e:
                            print(f"Error processing {json_file}: {e}")
                            seen_files.add(filename)
                            time.sleep(retry_delay)

                except Exception as e:
                    print(f"Error reading {json_file}: {e}")
                    time.sleep(retry_delay)

            if not enqueue_files:
                seen_files.clear()

        except Exception as e:
            print(f"Error scanning queue: {e}")
            time.sleep(retry_delay)

        time.sleep(retry_delay)


if __name__ == '__main__':
    from huey.consumer import Consumer
    print("Starting Huey consumer worker...")
    consumer = Consumer(huey)

    queue_dir = os.environ.get('HUEY_QUEUE_PATH', '/app/huey_data')

    json_processor = threading.Thread(target=process_json_queue_with_delay, args=(queue_dir,), daemon=True)
    json_processor.start()

    consumer.run()