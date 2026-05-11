#!/usr/bin/env python3
"""
Huey Consumer Worker for Insurance Manager
Processes status change tasks from the queue.

This file is used by huey CLI. Run with:
    huey -c huey_consumer.py worker

Or configure in Dockerfile.huey.
"""
import os
import requests
from huey import Huey, FileHuey, task

# FileHuey stores queue data as pickle files in a directory
huey = FileHuey(
    'insurance_huey',
    path=os.environ.get('HUEY_QUEUE_PATH', '/app/huey_data')
)

API_BASE_URL = os.environ.get('API_BASE_URL', 'http://localhost:5000')
INTERNAL_API_KEY = os.environ.get('INTERNAL_API_KEY', 'internal-secret-change-me')


@huey.task()
def process_status_change(proposal_id: str, new_status: str):
    """
    Process a proposal status change request.

    Args:
        proposal_id: GUID of the proposal to update
        new_status: Target status ('Aprovada' or 'Recusada')
    """
    print(f"Processing status change: proposal_id={proposal_id}, new_status={new_status}")

    try:
        response = requests.post(
            f"{API_BASE_URL}/internal/status",
            json={
                "proposalId": proposal_id,
                "newStatus": new_status
            },
            headers={"X-Internal-Key": INTERNAL_API_KEY},
            timeout=30
        )

        if response.status_code == 200:
            print(f"Status change successful for proposal {proposal_id}")
        else:
            print(f"Status change failed: {response.status_code} - {response.text}")
            raise Exception(f"Status change failed: {response.status_code}")

    except requests.RequestException as e:
        print(f"Request error processing status change: {e}")
        raise