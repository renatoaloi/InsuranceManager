#!/usr/bin/env python3
"""
Huey Consumer Worker for Insurance Manager
Processes status change tasks from the queue.

The consumer calls the API's internal endpoint to update proposal status,
keeping all DB access through EF Core.
"""
import sys
import os
import requests

# Add Huey directory to path
sys.path.insert(0, os.path.dirname(__file__))
from huey_config import huey

# API base URL (from environment or default for local development)
API_BASE_URL = os.environ.get('API_BASE_URL', 'http://localhost:5000')


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
        # Call the internal status update endpoint
        # This keeps DB access through EF Core
        response = requests.post(
            f"{API_BASE_URL}/internal/status",
            json={
                "proposalId": proposal_id,
                "newStatus": new_status
            },
            headers={"X-Internal-Key": os.environ.get('INTERNAL_API_KEY', 'internal-secret')},
            timeout=30
        )

        if response.status_code == 200:
            print(f"Status change successful for proposal {proposal_id}")
        else:
            print(f"Status change failed: {response.status_code} - {response.text}")
            # Raise to trigger Huey retry
            raise Exception(f"Status change failed: {response.status_code}")

    except requests.RequestException as e:
        print(f"Request error processing status change: {e}")
        raise


if __name__ == '__main__':
    # Run the Huey consumer
    print("Starting Huey consumer worker...")
    huey.consume()