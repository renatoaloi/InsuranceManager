import os
from huey import Huey, FileHuey

# FileHuey stores queue data as pickle files in a directory
# This is Windows and Docker compatible (no Redis required)
huey = FileHuey(
    'insurance_huey',
    path=os.path.join(os.path.dirname(__file__), '../../../huey_data')
)