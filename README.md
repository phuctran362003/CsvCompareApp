# Công cụ So sánh CSV

Đây là một công cụ dòng lệnh đơn giản giúp bạn tìm thấy sự khác biệt trong các cột của một file CSV.

## Tính năng chính

Công cụ này cung cấp hai chế độ so sánh chính:

1.  **So sánh hai cột đơn lẻ:**
    *   So sánh giá trị trên cùng một hàng giữa hai cột bạn chọn.
    *   Đếm và so sánh số lần xuất hiện của mỗi giá trị trong hai cột.
    *   Tìm ra các giá trị chỉ tồn tại ở một trong hai cột.

2.  **So sánh theo nhóm (ID & Số lượng):**
    *   So sánh hai cặp cột "ID" và "Số lượng" (Amount).
    *   Tìm các ID khớp nhau nhưng có "Số lượng" khác nhau.
    *   Liệt kê các ID chỉ xuất hiện ở một trong hai nhóm.

## Cách sử dụng

1.  **Chạy ứng dụng:** Mở ứng dụng (file `.exe`).
2.  **Nhập đường dẫn file:** Cung cấp đường dẫn đầy đủ đến file CSV bạn muốn phân tích.
    *   *Ví dụ:* `D:\DuLieu\BaoCao.csv`
3.  **Chọn loại so sánh:**
    *   Nhập `1` để so sánh hai cột đơn.
    *   Nhập `2` để so sánh theo nhóm.
4.  **Chọn cột:**
    *   Làm theo hướng dẫn để chọn các cột bạn muốn so sánh. Bạn có thể chọn bằng cách nhập số thứ tự hoặc tên cột.
5.  **Xem kết quả:** Công cụ sẽ in kết quả phân tích ra màn hình, bao gồm tóm tắt ở cuối.
6.  **Tiếp tục hoặc thoát:** Sau khi hoàn tất, bạn có thể chọn thực hiện một so sánh khác hoặc thoát khỏi chương trình.

## Yêu cầu

*   Máy tính chạy hệ điều hành Windows.
*   [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) (Thường đã được cài đặt sẵn trên Windows).
