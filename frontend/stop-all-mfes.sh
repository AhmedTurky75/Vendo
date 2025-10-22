#!/bin/bash

echo "🛑 Stopping all Vendo MFEs..."

if [ -f /tmp/mfe-pids.txt ]; then
    PIDS=$(cat /tmp/mfe-pids.txt)
    for PID in $PIDS; do
        if ps -p $PID > /dev/null 2>&1; then
            echo "  Stopping process $PID..."
            kill $PID 2>/dev/null
        fi
    done
    rm /tmp/mfe-pids.txt
    echo "✅ All MFEs stopped"
else
    echo "⚠️  No PID file found. MFEs may not be running."
    echo "Checking for processes on MFE ports..."

    # Try to kill processes on MFE ports
    for PORT in 4200 4201 4202 4203; do
        PID=$(lsof -ti:$PORT)
        if [ ! -z "$PID" ]; then
            echo "  Stopping process on port $PORT (PID: $PID)..."
            kill $PID 2>/dev/null
        fi
    done
fi

# Clean up log files
rm -f /tmp/mfe-*.log /tmp/shell-app.log

echo "✅ Cleanup complete"
