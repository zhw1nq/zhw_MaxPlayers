# zhw_MaxPlayers Plugin

[![CS2](https://img.shields.io/badge/CS2-Compatible-brightgreen)](https://github.com/roflmuffin/CounterStrikeSharp)
[![CounterStrikeSharp](https://img.shields.io/badge/CounterStrikeSharp-v147+-blue)](https://github.com/roflmuffin/CounterStrikeSharp)
[![Version](https://img.shields.io/badge/Version-1.0.1-orange)](https://github.com/zhw1nq/zhw_MaxPlayers)

Plugin quản lý số lượng người chơi trong server CS2, giới hạn người chơi

## 🎯 Tính năng chính

- **Giới hạn số lượng người chơi**: (Tùy chỉnh)
- **Chống 6v5**: Ngăn chặn hoàn toàn tình trạng team không cân bằng
- **Tự động cân bằng team**: Chuyển người chơi sang team còn chỗ
- **Tự động phân team**: Spectator dư thừa được tự động thêm vào team
- **Hỗ trợ custom command**: Có thể thực hiện lệnh tùy chỉnh thay vì kick
- **Cấu hình linh hoạt**: Dễ dàng thay đổi giới hạn và thông báo

## 📋 Yêu cầu

- **Counter-Strike 2 Server**
- **CounterStrikeSharp v147+**
- **Quyền admin** (để sử dụng các lệnh quản lý)

## 🚀 Cài đặt

1. **Tải xuống plugin**:
   ```bash
   git clone https://github.com/zhw1nq/zhw_MaxPlayers.git
   ```

2. **Biên dịch plugin**:
   ```bash
   dotnet build
   ```

3. **Copy file dll**:
   ```
   /csgo/addons/counterstrikesharp/plugins/zhw_MaxPlayers/zhw_MaxPlayers.dll
   ```

4. **Khởi động lại server** hoặc dùng lệnh:
   ```
   css_plugins reload zhw_MaxPlayers
   ```

## ⚙️ Cấu hình

File cấu hình sẽ được tạo tự động tại: `/csgo/addons/counterstrikesharp/plugins/zhw_MaxPlayers/zhw_MaxPlayers.json`

### Cấu hình mặc định:

```json
{
  "MaxPlayers": 12,
  "MaxPlayersPerTeam": 5,
  "MaxSpectators": 2,
  "EnableCustomCommand": true,
  "Messages": {
    "ServerFull": "Server đã đầy! Vui lòng thử lại sau.",
    "TooManySpectators": "Quá nhiều spectator! Tối đa 2 người.",
    "TeamFull": "Team đã đầy! Bạn được chuyển sang spectator.",
    "TeamBalanced": "Team đã được cân bằng!"
  },
  "CustomCommands": {
    "ServerFull": "kickid {userid} \"Server đã đầy!\"",
    "TooManySpectators": "kickid {userid} \"Quá nhiều spectator!\"",
    "TeamFull": "css_team \"{playername}\" spec",
    "TeamBalanced": "css_team \"{playername}\" ct",
    "JoinTerrorist": "css_team \"{playername}\" t",
    "JoinCounterTerrorist": "css_team \"{playername}\" ct"
  }
}
```

### Thông số cấu hình:

| Thông số | Mô tả | Giá trị mặc định |
|----------|-------|------------------|
| `MaxPlayers` | Tổng số người chơi tối đa | 12 |
| `MaxPlayersPerTeam` | Số người tối đa mỗi team | 5 |
| `MaxSpectators` | Số spectator tối đa | 2 |
| `EnableCustomCommand` | Bật/tắt custom command | true |

### Placeholder có thể dùng:

- `{playername}` - Tên người chơi
- `{steamid}` - Steam ID
- `{userid}` - User ID
- `{slot}` - Slot ID

## 🎮 Lệnh quản lý

### Lệnh Console/Chat:

| Lệnh | Quyền | Mô tả |
|------|-------|-------|
| `zhw_reload_config` | `@css/generic` | Tải lại cấu hình |
| `zhw_player_info` | `@css/generic` | Hiển thị thông tin người chơi |

### Sử dụng lệnh:

```bash
# Tải lại cấu hình
css_rcon zhw_reload_config

# Xem thông tin người chơi
css_rcon zhw_player_info
```

## 🔧 Cách hoạt động

### 1. Kiểm tra khi người chơi kết nối:
- Nếu server đầy (>12 người) → Kick
- Nếu quá nhiều spectator → Tự động thêm vào team

### 2. Kiểm tra khi thay đổi team:
- **Team T đầy** → Chuyển sang CT
- **Team CT đầy** → Chuyển sang T
- **Cả 2 team đầy** → Chuyển sang spectator
- **Không còn chỗ** → Kick

### 3. Tự động cân bằng:
- Spectator dư thừa được phân vào team ít người
- Ưu tiên team có ít người hơn
- Chọn ngẫu nhiên nếu 2 team bằng nhau

## 📊 Ví dụ tình huống

### Tình huống 1: Server 5v5 đầy
```
Trạng thái: T:5 | CT:5 | Spec:2
Người mới vào → Bị kick (server đầy)
```

### Tình huống 2: Team không cân bằng
```
Trạng thái: T:5 | CT:4 | Spec:1
Người chơi join team T → Tự động chuyển sang CT
```

### Tình huống 3: Spectator dư thừa
```
Trạng thái: T:4 | CT:5 | Spec:3
Spectator thứ 3 → Tự động join team T
```

## 🎨 Tùy chỉnh thông báo

Bạn có thể tùy chỉnh màu sắc và nội dung thông báo:

```json
{
  "Messages": {
    "ServerFull": "🔴 Server đã đầy! Vui lòng thử lại sau.",
    "TooManySpectators": "👥 Quá nhiều spectator! Tối đa 2 người.",
    "TeamFull": "⚖️ Team đã đầy! Bạn được chuyển sang spectator.",
    "TeamBalanced": "✅ Team đã được cân bằng!"
  }
}
```

## 🛠️ Khắc phục sự cố

### Lỗi thường gặp:

1. **Plugin không load**:
   - Kiểm tra CounterStrikeSharp version ≥ 147
   - Kiểm tra file dll có đúng thư mục

2. **Cấu hình không hoạt động**:
   - Sử dụng lệnh `zhw_reload_config` sau khi chỉnh sửa
   - Kiểm tra format JSON hợp lệ

3. **Lệnh không hoạt động**:
   - Kiểm tra quyền admin (`@css/generic`)
   - Đảm bảo server có CounterStrikeSharp

### Debug:

Bật console server để xem log chi tiết:
```bash
con_logfile server.log
developer 1
```

## 📝 Changelog

### Version 1.0.1
- ✅ Sửa lỗi tình trạng 6v5 tạm thời
- ✅ Thêm kiểm tra team join realtime
- ✅ Cải thiện logic cân bằng team
- ✅ Tối ưu hiệu suất

### Version 1.0.0
- 🎉 Phiên bản đầu tiên
- ✅ Giới hạn 5v5 + 2 spectator
- ✅ Tự động cân bằng team
- ✅ Hỗ trợ custom command

## 🤝 Đóng góp

Chúng tôi hoan nghênh mọi đóng góp! Vui lòng:

1. Fork repository
2. Tạo branch mới (`git checkout -b feature/amazing-feature`)
3. Commit thay đổi (`git commit -m 'Add amazing feature'`)
4. Push lên branch (`git push origin feature/amazing-feature`)
5. Tạo Pull Request

## 📜 License

Dự án này được phát hành dưới [MIT License](LICENSE).

## 📞 Hỗ trợ

- **GitHub Issues**: [Tạo issue mới](https://github.com/zhw1nq/zhw_MaxPlayers/issues)
- **Discord**: [CS2 Vietnam Community](https://discord.gg/cs2vietnam)
- **Email**: vhming@zhw1nq.com

## 🙏 Lời cảm ơn

- [CounterStrikeSharp Team](https://github.com/roflmuffin/CounterStrikeSharp) - Framework tuyệt vời
- [CS2 Community](https://discord.gg/counterstrike) - Hỗ trợ và feedback

---

**Made with ❤️ by zhw1nq | CS2 Vietnam Community**
