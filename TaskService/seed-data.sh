#!/usr/bin/env bash
set -euo pipefail

API="http://localhost:5002/api/task"

echo "Fetching BoardId..."
BOARD_ID=$(curl -s "$API" | python3 -c "
import sys, json
tasks = json.load(sys.stdin)
if tasks:
    print(tasks[0]['boardId'])
else:
    print('')
")

if [ -z "$BOARD_ID" ]; then
  echo "ERROR: Không lấy được BoardId. Kiểm tra API có chạy không: curl $API"
  exit 1
fi

echo "BoardId: $BOARD_ID"
echo ""

add_task() {
  local TITLE="$1"
  local DESC="$2"
  local PRIORITY="$3"
  local COLOR="$4"
  local STATUS="$5"
  local DAYS="$6"
  local DEADLINE
  DEADLINE=$(date -d "+${DAYS} days" -u +%Y-%m-%dT%H:%M:%SZ)
  local ASSIGNEE
  ASSIGNEE=$(cat /proc/sys/kernel/random/uuid)

  curl -s -X POST "$API" \
    -H "Content-Type: application/json" \
    -d "{
      \"boardId\": \"$BOARD_ID\",
      \"title\": \"$TITLE\",
      \"description\": \"$DESC\",
      \"priority\": $PRIORITY,
      \"colorLabel\": \"$COLOR\",
      \"currentStatus\": $STATUS,
      \"assigneeId\": \"$ASSIGNEE\",
      \"deadline\": \"$DEADLINE\"
    }" > /dev/null

  echo "  ✅ $TITLE"
}

echo "Thêm tasks..."
# Status: 0=Backlog, 1=ToDo, 2=InProgress, 3=Review, 4=Done

add_task "Triển khai CI/CD pipeline"        "Cấu hình GitHub Actions tự động build và deploy lên VPS."   3 "#EF4444" 1 7
add_task "Viết unit test TaskRepository"    "Coverage tối thiểu 80% cho các hàm CRUD."                  2 "#8B5CF6" 2 4
add_task "Tích hợp Swagger UI đầy đủ"      "Thêm annotation mô tả cho từng endpoint."                   1 "#10B981" 4 1
add_task "Docker Compose production"        "Tách môi trường dev/prod, thêm health check."               3 "#F59E0B" 3 10
add_task "Thiết kế database schema v2"      "Thêm bảng Comment và Attachment cho TaskItem."              2 "#06B6D4" 0 14
add_task "Tối ưu query N+1 EF Core"        "Dùng Include() và AsNoTracking() đúng chỗ."                 2 "#EF4444" 2 3
add_task "Thêm authentication JWT"          "Bảo vệ các endpoint với Bearer token."                      3 "#F97316" 1 20
add_task "Viết tài liệu API"               "Mô tả đầy đủ request/response cho từng endpoint."           1 "#10B981" 4 2
add_task "Load testing với k6"             "Kiểm tra API chịu tải 1000 concurrent users."               2 "#8B5CF6" 0 30
add_task "Review pull request RabbitMQ"    "Kiểm tra logic xử lý consumer và error handling."           1 "#06B6D4" 3 1
add_task "Xử lý lỗi global middleware"     "Thêm exception handler trả về chuẩn ProblemDetails."        2 "#F59E0B" 2 5
add_task "Cấu hình logging Serilog"        "Ghi log ra file và console theo cấu trúc JSON."             1 "#3B82F6" 1 8
add_task "Refactor repository pattern"     "Tách interface rõ ràng, áp dụng generic repository."        2 "#8B5CF6" 0 15
add_task "Thêm validation FluentValidation" "Validate request body trước khi xử lý business logic."     3 "#EF4444" 1 6
add_task "Setup môi trường staging"        "Tạo VPS staging riêng biệt với data test."                  2 "#06B6D4" 2 12
add_task "Cập nhật README dự án"           "Hướng dẫn setup local và deploy production."                1 "#10B981" 4 1
add_task "Thêm pagination cho GET all"     "Hỗ trợ ?page=1&pageSize=20 cho danh sách task."             2 "#3B82F6" 3 9
add_task "Kiểm tra bảo mật SQL injection"  "Rà soát toàn bộ query, đảm bảo dùng parameterized."        3 "#F97316" 0 25

echo ""
TOTAL=$(curl -s "$API" | python3 -c "import sys,json; print(len(json.load(sys.stdin)))")
echo "============================="
echo "✅ Hoàn thành! Tổng số task: $TOTAL"
echo "============================="
