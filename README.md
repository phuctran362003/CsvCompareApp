# 🎉 CSV Compare App - UTF-8 Optimized

## ✨ Các cải thiện đã thực hiện:

### 🔧 **Encoding Support:**
- ✅ Auto-detect encoding (UTF-8, UTF-16, Windows-1258, ASCII)
- ✅ Hỗ trợ tiếng Việt với Windows-1258 và UTF-8
- ✅ Xử lý BOM (Byte Order Mark)
- ✅ Console UTF-8 output cho tiếng Việt

### 🚀 **Tính năng chính:**
- ✅ So sánh 2 cột đơn (A vs B)
- ✅ So sánh nhóm cột (ID + Amount)
- ✅ Auto-detect header trong CSV
- ✅ Xử lý input validation với timeout
- ✅ Error handling tốt hơn

### 🎯 **User Experience:**
- ✅ Giao diện tiếng Việt với emoji
- ✅ Thông báo encoding detection
- ✅ Input validation với retry logic
- ✅ Clear error messages
- ✅ Exit option với 'exit' command

## 📊 **Test Files có sẵn:**
1. `Files/test_utf8.csv` - UTF-8 với tiếng Việt
2. `Files/so_sanh_utf8.csv` - File so sánh nhóm cột
3. `Files/Book1.csv` - File dữ liệu thực tế

## 🚀 **Cách sử dụng:**
```bash
dotnet run
```

### Ví dụ input:
- File path: `D:\FPT\projects\CsvCompareApp\Files\test_utf8.csv`
- Comparison type: `1` (Two columns) hoặc `2` (Group columns)
- Chọn cột theo số thứ tự hoặc tên cột
- Continue: `2` để thoát

## 🔥 **Các encoding được hỗ trợ:**
- UTF-8 (with/without BOM)
- UTF-16 LE/BE
- Windows-1258 (Vietnamese)
- ASCII
- Windows-1252 (Western European)

## 📝 **Notes:**
- Chương trình tự động detect encoding
- Hỗ trợ đầy đủ ký tự tiếng Việt
- Error handling với retry mechanism
- Clean console output với UTF-8