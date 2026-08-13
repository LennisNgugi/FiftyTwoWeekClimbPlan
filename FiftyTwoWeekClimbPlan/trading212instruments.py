import requests
import json

API_KEY = "YOUR_API_KEY"
API_SECRET = "YOUR_API_SECRET"

url = "https://live.trading212.com/api/v0/equity/metadata/instruments"

response = requests.get(
    url,
    auth=(API_KEY, API_SECRET)
)

if response.ok:
    instruments = response.json()

    with open("instruments.json", "w", encoding="utf-8") as file:
        json.dump(instruments, file, indent=2, ensure_ascii=False)

    print(f"Success! {len(instruments)} instruments saved to instruments.json")

else:
    print(f"Error: {response.status_code}")
    print(response.text)
