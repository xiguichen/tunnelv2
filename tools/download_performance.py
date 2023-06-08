import sys
import requests
import time

def measure_url_performance(base_url, byte_count):
    url = f"{base_url}/{byte_count}"
    start_time = time.time()
    response = requests.get(url)
    received_bytes = len(response.content)
    end_time = time.time()

    time_diff = (int) (end_time - start_time)
    if time_diff != 0:
        return (int)(received_bytes / time_diff / 1000)
    else:
        return received_bytes / 1000

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Usage: python script.py base_url byte_count")
        sys.exit(1)

    base_url = sys.argv[1]
    byte_count = int(sys.argv[2])

    received_bytes = measure_url_performance(base_url, byte_count)
    print(f"Received {received_bytes} bytes in one second.")
