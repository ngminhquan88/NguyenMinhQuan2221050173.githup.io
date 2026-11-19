const modalElement = document.querySelector("#appModal");
const modalContent = document.querySelector("#appModalContent");
const modal = modalElement ? bootstrap.Modal.getOrCreateInstance(modalElement) : null;

function showMessage(message) {
	const toast = document.querySelector("#appToast");
	document.querySelector("#appToastMessage").textContent = message;
	bootstrap.Toast.getOrCreateInstance(toast).show();
}

document.addEventListener("click", async (event) => {
	const modalButton = event.target.closest("[data-modal-url]");
	if (modalButton) {
		modalContent.innerHTML = '<div class="modal-body py-5 text-center">Đang tải biểu mẫu...</div>';
		modal.show();
		const response = await fetch(modalButton.dataset.modalUrl);
		if (!response.ok) {
			modalContent.innerHTML = '<div class="modal-body">Không tải được biểu mẫu.</div>';
			return;
		}
		modalContent.innerHTML = await response.text();
		return;
	}

	const deleteButton = event.target.closest("[data-delete-url]");
	if (deleteButton && window.confirm("Bạn chắc chắn muốn xóa bản ghi này?")) {
		const token = document.querySelector('#csrfForm input[name="__RequestVerificationToken"]').value;
		const response = await fetch(deleteButton.dataset.deleteUrl, {
			method: "POST",
			headers: { RequestVerificationToken: token }
		});
		const result = await response.json();
		if (!response.ok) {
			showMessage(result.message || "Không thể xóa bản ghi.");
			return;
		}
		window.location.reload();
	}
});

document.addEventListener("submit", async (event) => {
	const form = event.target.closest("form[data-ajax-form]");
	if (!form) return;
	event.preventDefault();
	if (form.action.endsWith("/Import/Upload")) {
		const mode = form.elements.namedItem("mode").value;
		const confirmation = mode === "replace"
			? "Ghi đè sẽ xóa toàn bộ dữ liệu hiện có trong bảng trước khi nhập. Tiếp tục?"
			: "Xác nhận nối thêm các dòng trong tệp vào dữ liệu hiện có?";
		if (!window.confirm(confirmation)) return;
	}

	const button = form.querySelector('button[type="submit"]');
	button.disabled = true;
	const response = await fetch(form.action, { method: "POST", body: new FormData(form) });
	const result = await response.json();
	button.disabled = false;

	if (!response.ok) {
		const messages = result.errors
			? Object.values(result.errors).flat().join(" ")
			: result.message || "Vui lòng kiểm tra lại dữ liệu.";
		const errorBox = form.querySelector("[data-form-error]");
		errorBox.textContent = messages;
		errorBox.classList.remove("d-none");
		return;
	}

	modal.hide();
	showMessage(result.message || "Đã lưu dữ liệu.");
	window.setTimeout(() => window.location.reload(), 450);
});
