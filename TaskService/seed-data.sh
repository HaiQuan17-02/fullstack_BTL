#!/usr/bin/env bash
set -euo pipefail

BASE="http://localhost:5002/api"

echo "=============================="
echo "  SEED DATA - TaskService"
echo "=============================="

# Đợi API sẵn sàng (tối đa 90 giây)
echo "⏳ Đợi API khởi động..."
API_READY=0
for i in $(seq 1 18); do
  STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5002/api/board 2>/dev/null || true)
  if [ "$STATUS" = "200" ]; then
    echo "✅ API sẵn sàng!"
    API_READY=1
    break
  fi
  echo "   Lần $i/18: status=$STATUS, đợi 5 giây..."
  sleep 5
done

if [ "$API_READY" -eq 0 ]; then
  echo ""
  echo "❌ API không khởi động được sau 90 giây!"
  echo "   Kiểm tra: docker ps && docker logs taskservice"
  exit 1
fi

# ─── Helpers ─────────────────────────────────────────────────────
uuid() { cat /proc/sys/kernel/random/uuid; }
future() { date -d "+${1} days" -u +%Y-%m-%dT%H:%M:%SZ; }

post() {
  local url="$1"
  local body="$2"
  curl -s -X POST "$url" -H "Content-Type: application/json" -d "$body"
}

# ─── 1. Board ────────────────────────────────────────────────────
echo ""
echo "[ 1/4 ] Kiểm tra Board..."
EXISTING_BOARDS=$(curl -s "$BASE/board" | python3 -c "import sys,json; d=json.load(sys.stdin); print(len(d))")
echo "        Hiện có $EXISTING_BOARDS board(s)"

if [ "$EXISTING_BOARDS" -eq 0 ]; then
  echo "        Tạo board mới..."
  BOARD_RAW=$(post "$BASE/board" '{"name":"Sprint Board - BTL Fullstack","description":"Bảng quản lý công việc cho đồ án fullstack semester 3."}')
  BOARD_ID=$(echo "$BOARD_RAW" | python3 -c "import sys,json; print(json.load(sys.stdin)['boardId'])")
  echo "        ✅ Board tạo: $BOARD_ID"
else
  BOARD_ID=$(curl -s "$BASE/board" | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['boardId'])")
  echo "        Dùng BoardId: $BOARD_ID"
fi

# ─── 2. Tasks ────────────────────────────────────────────────────
echo ""
echo "[ 2/4 ] Thêm Tasks..."

add_task() {
  local TITLE="$1" DESC="$2" PRIORITY="$3" COLOR="$4" STATUS="$5" DAYS="$6"
  local RESULT
  RESULT=$(post "$BASE/task" "{
    \"boardId\": \"$BOARD_ID\",
    \"title\": \"$TITLE\",
    \"description\": \"$DESC\",
    \"priority\": $PRIORITY,
    \"colorLabel\": \"$COLOR\",
    \"currentStatus\": $STATUS,
    \"assigneeId\": \"$(uuid)\",
    \"deadline\": \"$(future $DAYS)\"
  }")
  echo "$RESULT" | python3 -c "import sys,json; d=json.load(sys.stdin); print('        ✅ Task:', d['title'], '|', d['taskId'])"
  echo "$RESULT" | python3 -c "import sys,json; print(json.load(sys.stdin)['taskId'])"
}

# Status: 0=Backlog 1=ToDo 2=InProgress 3=Review 4=Done
T1=$(add_task "Thiết kế hệ thống microservice"  "Phân tích yêu cầu, vẽ diagram kiến trúc hệ thống."      3 "#EF4444" 2 10 | tail -1)
T2=$(add_task "Xây dựng TaskService API"          "Implement CRUD endpoints cho Task, Board, SubTask."      3 "#3B82F6" 2 7  | tail -1)
T3=$(add_task "Tích hợp RabbitMQ message bus"    "Publish/consume event khi task thay đổi trạng thái."    2 "#8B5CF6" 3 5  | tail -1)
T4=$(add_task "Viết unit test với xUnit"          "Coverage tối thiểu 80% cho repository và service."      2 "#F59E0B" 1 14 | tail -1)
T5=$(add_task "Deploy lên VPS với Docker"         "Cấu hình docker-compose, nginx reverse proxy."          3 "#10B981" 4 3  | tail -1)
T6=$(add_task "Thiết kế giao diện Kanban board"   "Dùng React kéo thả task giữa các cột trạng thái."      2 "#06B6D4" 2 8  | tail -1)
T7=$(add_task "Thêm authentication JWT"           "Đăng nhập, đăng ký, refresh token, phân quyền role."   3 "#F97316" 1 20 | tail -1)
T8=$(add_task "Cấu hình CI/CD GitHub Actions"     "Auto build, test và deploy khi push lên main branch."  2 "#8B5CF6" 0 30 | tail -1)

echo ""
TOTAL_TASKS=$(curl -s "$BASE/task" | python3 -c "import sys,json; print(len(json.load(sys.stdin)))")
echo "        Tổng tasks: $TOTAL_TASKS"

# ─── 3. SubTasks ─────────────────────────────────────────────────
echo ""
echo "[ 3/4 ] Thêm SubTasks..."

add_subtask() {
  local TASK_ID="$1" TITLE="$2" DONE="$3"
  post "$BASE/task/$TASK_ID/subtasks" "{\"title\": \"$TITLE\", \"isCompleted\": $DONE}" | \
    python3 -c "import sys,json; d=json.load(sys.stdin); print('        ✅ SubTask:', d['title'])" 2>/dev/null || \
    echo "        ⚠️  SubTask skip: $TITLE"
}

add_subtask "$T1" "Vẽ use case diagram"              "true"
add_subtask "$T1" "Phân tích database schema"        "true"
add_subtask "$T1" "Định nghĩa API contract"          "false"

add_subtask "$T2" "Tạo Board CRUD endpoints"         "true"
add_subtask "$T2" "Tạo Task CRUD endpoints"          "true"
add_subtask "$T2" "Tạo SubTask endpoints"            "true"
add_subtask "$T2" "Tạo WorkLog endpoints"            "false"
add_subtask "$T2" "Viết Swagger documentation"       "false"

add_subtask "$T3" "Cài đặt MassTransit"             "true"
add_subtask "$T3" "Publish TaskStatusChanged event"  "true"
add_subtask "$T3" "Test consumer nhận event"         "false"

add_subtask "$T4" "Test BoardRepository"             "false"
add_subtask "$T4" "Test TaskItemRepository"          "false"
add_subtask "$T4" "Test WorkLogRepository"           "false"

add_subtask "$T5" "Viết Dockerfile"                  "true"
add_subtask "$T5" "Cấu hình docker-compose"          "true"
add_subtask "$T5" "Deploy và kiểm tra trên VPS"      "true"

add_subtask "$T6" "Thiết kế mockup Figma"            "true"
add_subtask "$T6" "Implement drag & drop"            "false"
add_subtask "$T6" "Kết nối API thực"                 "false"

add_subtask "$T7" "Tạo bảng Users trong DB"          "false"
add_subtask "$T7" "Implement JWT middleware"         "false"

add_subtask "$T8" "Viết workflow file"               "false"
add_subtask "$T8" "Test pipeline chạy đúng"          "false"

# ─── 4. WorkLogs ─────────────────────────────────────────────────
echo ""
echo "[ 4/4 ] Thêm WorkLogs..."

add_worklog() {
  local TASK_ID="$1" HOURS="$2" NOTE="$3"
  post "$BASE/task/$TASK_ID/worklogs" "{
    \"memberId\": \"$(uuid)\",
    \"hoursSpent\": $HOURS,
    \"note\": \"$NOTE\"
  }" | python3 -c "import sys,json; d=json.load(sys.stdin); print('        ✅ WorkLog:', d['hoursSpent'],'h -', d['note'])" 2>/dev/null || \
    echo "        ⚠️  WorkLog skip"
}

add_worklog "$T1" "3.5" "Hoàn thành vẽ diagram kiến trúc hệ thống"
add_worklog "$T1" "2.0" "Review và chỉnh sửa database schema"
add_worklog "$T2" "4.0" "Implement Board và Task controller"
add_worklog "$T2" "3.0" "Fix lỗi N+1 query trong GetAll"
add_worklog "$T2" "2.5" "Thêm SubTask và WorkLog controller"
add_worklog "$T3" "2.0" "Cấu hình MassTransit với RabbitMQ"
add_worklog "$T3" "1.5" "Test publish event thành công"
add_worklog "$T5" "1.5" "Viết Dockerfile và docker-compose"
add_worklog "$T5" "2.0" "Debug lỗi Alpine ICU globalization"
add_worklog "$T5" "0.5" "Deploy thành công lên VPS"
add_worklog "$T6" "3.0" "Thiết kế mockup giao diện Kanban"
add_worklog "$T7" "1.0" "Nghiên cứu JWT authentication flow"

# ─── Summary ─────────────────────────────────────────────────────
echo ""
echo "=============================="
echo "✅ SEED HOÀN THÀNH!"
TASKS=$(curl -s "$BASE/task" | python3 -c "import sys,json; print(len(json.load(sys.stdin)))")
echo "   📋 Tasks    : $TASKS"
echo "   🗂️  SubTasks : $(curl -s "$BASE/task/$(curl -s "$BASE/task" | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['taskId'])")/subtasks" | python3 -c "import sys,json; print(len(json.load(sys.stdin)))") (task đầu tiên)"
echo "=============================="
