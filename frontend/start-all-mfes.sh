#!/bin/bash

echo "🚀 Starting Vendo Micro-Frontend Architecture"
echo "=============================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to check if port is in use
check_port() {
    if lsof -Pi :$1 -sTCP:LISTEN -t >/dev/null 2>&1 ; then
        echo "⚠️  Port $1 is already in use. Please free it up first."
        return 1
    fi
    return 0
}

# Check all required ports
echo "Checking ports..."
check_port 4200 || exit 1
check_port 4201 || exit 1
check_port 4202 || exit 1
check_port 4203 || exit 1

echo "✅ All ports available"
echo ""

# Start mfe-admin
echo -e "${BLUE}Starting mfe-admin on port 4201...${NC}"
cd /home/user/Vendo/frontend/mfe-admin
npm start > /tmp/mfe-admin.log 2>&1 &
MFE_ADMIN_PID=$!
echo "  PID: $MFE_ADMIN_PID"

# Start mfe-merchant
echo -e "${BLUE}Starting mfe-merchant on port 4202...${NC}"
cd /home/user/Vendo/frontend/mfe-merchant
npm start > /tmp/mfe-merchant.log 2>&1 &
MFE_MERCHANT_PID=$!
echo "  PID: $MFE_MERCHANT_PID"

# Start mfe-customer
echo -e "${BLUE}Starting mfe-customer on port 4203...${NC}"
cd /home/user/Vendo/frontend/mfe-customer
npm start > /tmp/mfe-customer.log 2>&1 &
MFE_CUSTOMER_PID=$!
echo "  PID: $MFE_CUSTOMER_PID"

echo ""
echo "⏳ Waiting for remote MFEs to start (30 seconds)..."
sleep 30

# Start shell-app
echo ""
echo -e "${BLUE}Starting shell-app (host) on port 4200...${NC}"
cd /home/user/Vendo/frontend/shell-app
npm start > /tmp/shell-app.log 2>&1 &
SHELL_APP_PID=$!
echo "  PID: $SHELL_APP_PID"

echo ""
echo -e "${GREEN}=============================================="
echo "✅ All MFEs are starting!"
echo "=============================================="
echo ""
echo "Access URLs:"
echo "  • Shell App (Host):  http://localhost:4200"
echo "  • Admin MFE:         http://localhost:4201"
echo "  • Merchant MFE:      http://localhost:4202"
echo "  • Customer MFE:      http://localhost:4203"
echo ""
echo "Test Routes (via Shell App):"
echo "  • Admin:             http://localhost:4200/admin"
echo "  • Merchant:          http://localhost:4200/merchant"
echo "  • Customer Store:    http://localhost:4200/store/example"
echo ""
echo "Process IDs:"
echo "  • mfe-admin:    $MFE_ADMIN_PID"
echo "  • mfe-merchant: $MFE_MERCHANT_PID"
echo "  • mfe-customer: $MFE_CUSTOMER_PID"
echo "  • shell-app:    $SHELL_APP_PID"
echo ""
echo "Logs:"
echo "  • tail -f /tmp/mfe-admin.log"
echo "  • tail -f /tmp/mfe-merchant.log"
echo "  • tail -f /tmp/mfe-customer.log"
echo "  • tail -f /tmp/shell-app.log"
echo ""
echo "To stop all processes:"
echo "  kill $MFE_ADMIN_PID $MFE_MERCHANT_PID $MFE_CUSTOMER_PID $SHELL_APP_PID"
echo ""
echo -e "${GREEN}Press Ctrl+C to view logs and keep processes running${NC}"
echo "=============================================="
echo -e "${NC}"

# Save PIDs to file for easy cleanup
echo "$MFE_ADMIN_PID $MFE_MERCHANT_PID $MFE_CUSTOMER_PID $SHELL_APP_PID" > /tmp/mfe-pids.txt

# Wait and show logs
sleep 5
echo ""
echo "Showing shell-app logs (Ctrl+C to exit, processes will keep running):"
tail -f /tmp/shell-app.log
