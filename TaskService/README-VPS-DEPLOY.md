# Hướng dẫn deploy VPS Ubuntu 22

> Nếu bạn đang gặp lỗi `ssh: connect to host ... port 5002: Connection refused`, nghĩa là SSH đang không lắng nghe ở cổng 5002 hoặc firewall đang chặn.

## 1. Tạo file zip

PowerShell:

```powershell
Compress-Archive -Path .\TaskService -DestinationPath .\taskservice.zip -Force
```

## 2. Upload lên VPS

```powershell
scp -P  .\taskservice.zip abcxyz@103.178.235.78:/home/abcxyz/
```

## 3. Kết nối VPS

### Cách 1: dùng port 5002

```powershell
ssh -p 5002 abcxyz@103.178.235.78
```

### Cách 2: nếu VPS đang dùng port 22 (thường là mặc định)

```powershell
ssh abcxyz@103.178.235.78
```

Nếu bạn dùng port 22, thì lệnh upload cũng phải đổi thành:

```powershell
scp .\taskservice.zip abcxyz@103.178.235.78:/home/abcxyz/
```

## 4. Nếu VPS chưa mở SSH đúng port

Trên VPS, chạy:

```bash
sudo systemctl status ssh
sudo ss -tlnp | grep ssh
sudo ufw status numbered
```

Nếu SSH chưa chạy:

```bash
sudo systemctl start ssh
sudo systemctl enable ssh
```

Nếu muốn SSH chạy ở port 5002, sửa file:

```bash
sudo nano /etc/ssh/sshd_config
```

Đổi:

```bash
Port 22
```

thành:

```bash
Port 5002
```

Lưu rồi restart:

```bash
sudo systemctl restart ssh
```

Nếu firewall đang bật:

```bash
sudo ufw allow 5002/tcp
sudo ufw status
```

Ngoài ra, trên panel VPS/Cloud Firewall của nhà cung cấp, hãy mở port 5002 (và 8080, 5672, 15672, 1433 nếu cần).

## 5. Chạy script deploy trên VPS

```bash
cd /home/abcxyz
chmod +x deploy-vps.sh
./deploy-vps.sh
```

Nếu script chưa có sẵn trên VPS, bạn có thể chạy trực tiếp:

```bash
cd /home/abcxyz
unzip -o taskservice.zip
cd TaskService
sudo docker compose up -d --build
```
