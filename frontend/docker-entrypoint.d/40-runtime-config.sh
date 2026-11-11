#!/bin/sh
set -eu

# Written at container start so API_BASE_URL can change without rebuilding the image.
api_base_url="${API_BASE_URL:-http://localhost:8080}"
escaped=$(printf '%s' "$api_base_url" | sed 's/\\/\\\\/g; s/"/\\"/g')
printf 'window.APP_CONFIG = { apiBaseUrl: "%s" };\n' "$escaped" > /usr/share/nginx/html/config.js
