// wwwroot/js/modal.js
window.showBootstrapModal = (id) => {
    const modalElement = document.getElementById(id);
    if (modalElement) {
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
};

window.hideBootstrapModal = (id) => {
    const modalElement = document.getElementById(id);
    if (modalElement) {
        const modal = bootstrap.Modal.getInstance(modalElement);
        modal?.hide();
    }
};