#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
OUT="${1:-$ROOT/../self-test-output}"
mkdir -p "$OUT"
SETTINGS="$(mktemp)"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet build "$ROOT/src/BlinkMore.App/BlinkMore.App.csproj" -c Release

xvfb-run -a dotnet run --project "$ROOT/src/BlinkMore.App/BlinkMore.App.csproj" -c Release --no-build -- \
  --self-test \
  --settings "$SETTINGS" \
  --screenshot-dir "$OUT"

if [[ "$(cat "$OUT/result.txt")" != "ok" ]]; then
  echo "Self-test failed:"
  cat "$OUT/result.txt"
  exit 1
fi

echo "Self-test passed. Screenshots are in $OUT"
