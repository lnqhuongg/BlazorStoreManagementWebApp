// Toast Configuration
const toastConfig = {
    success: {
        icon: 'bi-check-circle-fill',
        title: 'Thành công!',
        messages: [
            'Dữ liệu đã được lưu thành công',
            'Cập nhật hoàn tất!',
            'Đăng ký thành công',
            'Thanh toán đã được xử lý'
        ]
    },
    error: {
        icon: 'bi-x-circle-fill',
        title: 'Lỗi!',
        messages: [
            'Có lỗi xảy ra, vui lòng thử lại',
            'Không thể kết nối đến server',
            'Dữ liệu không hợp lệ',
            'Phiên đăng nhập đã hết hạn'
        ]
    },
    warning: {
        icon: 'bi-exclamation-triangle-fill',
        title: 'Cảnh báo!',
        messages: [
            'Bạn chưa lưu thay đổi',
            'Dung lượng sắp đầy',
            'Vui lòng kiểm tra lại thông tin',
            'Thao tác này không thể hoàn tác'
        ]
    },
    info: {
        icon: 'bi-info-circle-fill',
        title: 'Thông tin',
        messages: [
            'Có 3 tin nhắn mới',
            'Hệ thống sẽ bảo trì lúc 2h sáng',
            'Phiên bản mới đã có sẵn',
            'Nhấn vào đây để xem chi tiết'
        ]
    }
};

// Show Toast Function
function showToast(type, customMessage = null) {
    return new Promise((resolve) => {
        const config = toastConfig[type];
        const message = customMessage || config.messages[Math.floor(Math.random() * config.messages.length)];

        const toastContainer = document.getElementById('toastContainer');

        const toast = document.createElement('div');
        toast.className = `toast-item toast-${type}`;
        toast.innerHTML = `
            <div class="toast-icon">
              <i class="bi ${config.icon}"></i>
            </div>
            <div class="toast-content">
              <p class="toast-title">${config.title}</p>
              <p class="toast-message">${message}</p>
            </div>
            <button class="toast-close" onclick="closeToast(this)">
              <i class="bi bi-x"></i>
            </button>
            <div class="toast-progress"></div>
        `;

        toastContainer.appendChild(toast);

        // Resolve sớm hơn (1.5s thay vì 4s) để Blazor navigate
        // Toast vẫn hiển thị đủ 4s trên UI
        setTimeout(() => {
            resolve();
        }, 1500);

        // Remove toast sau 4 giây
        setTimeout(() => {
            removeToast(toast);
        }, 4000);
    });
}

// Close Toast
function closeToast(button) {
    const toast = button.closest('.toast-item');
    removeToast(toast);
}

// Remove Toast with Animation
function removeToast(toast) {
    toast.classList.add('removing');
    setTimeout(() => {
        toast.remove();
    }, 400);
}

// Show Random Toast
function showRandomToast() {
    const types = ['success', 'error', 'warning', 'info'];
    const randomType = types[Math.floor(Math.random() * types.length)];
    showToast(randomType);
}

//// Example: Show welcome toast on page load
//window.addEventListener('load', () => {
//    setTimeout(() => {
//        showToast('info', 'Chào mừng bạn đến với hệ thống toast notification! 👋');
//    }, 500);
//});