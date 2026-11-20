const modalElement = document.querySelector("#appModal");
const modalContent = document.querySelector("#appModalContent");
const modal = modalElement ? bootstrap.Modal.getOrCreateInstance(modalElement) : null;
const englishText = {
	"brand": "HUMG | Academic Management",
	"nav.students": "Students",
	"nav.departments": "Departments",
	"nav.admin": "Administration",
	"nav.privacy": "Privacy",
	"nav.register": "Register",
	"nav.login": "Sign in",
	"nav.logout": "Sign out",
	"footer.title": "HUMG | Academic Management",
	"footer.description": "Student and department management portal",
	"footer.copyright": "© 2026 StudentManagement",
	"students.title": "Students",
	"students.count": "{count} records managed",
	"students.add": "Add student",
	"students.code": "Student ID",
	"students.name": "Full name",
	"students.empty": "No students yet. Add the first record to get started.",
	"departments.eyebrow": "ACADEMIC DIRECTORY",
	"departments.subtitle": "Manage departments and their student records",
	"departments.add": "Add department",
	"departments.name": "Department name",
	"departments.students": "Students",
	"actions": "Actions",
	"actions.edit": "Edit",
	"actions.delete": "Delete",
	"import.excel": "Import Excel",
	"student.form.eyebrow": "STUDENT RECORD",
	"student.form.add": "Add student",
	"student.form.edit": "Edit student",
	"student.form.code": "Student ID",
	"student.form.keepCode": "ID stays unchanged when editing",
	"student.form.fullName": "Full name",
	"student.form.email": "Email",
	"student.form.department": "Department",
	"student.form.chooseDepartment": "Choose a department",
	"student.form.admissionYear": "Admission year",
	"student.form.programCode": "Program code",
	"student.form.majorCode": "Major code",
	"student.form.generated": "The student ID is generated from admission year, program, major and the next sequence number.",
	"department.form.eyebrow": "ACADEMIC DIRECTORY",
	"department.form.add": "Add department",
	"department.form.edit": "Edit department",
	"department.form.name": "Department name",
	"import.eyebrow": "EXCEL IMPORT",
	"import.students": "Import students",
	"import.departments": "Import departments",
	"import.file": "XLSX file",
	"import.mode": "Import mode",
	"import.append": "Append rows",
	"import.replace": "Replace the whole table",
	"import.studentColumns": "Columns: StudentCode, Name, Email, AdmissionYear, ProgramCode, MajorCode, DepartmentName. Maximum 10 MB.",
	"import.departmentColumns": "Column: DepartmentName. Maximum 10 MB.",
	"import.submit": "Import data",
	"actions.cancel": "Cancel",
	"actions.save": "Save",
	"messages.loadingForm": "Loading form...",
	"messages.formLoadFailed": "Could not load the form.",
	"messages.deleteConfirm": "Are you sure you want to delete this record?",
	"messages.deleteFailed": "Could not delete the record.",
	"messages.replaceConfirm": "Replace deletes all existing rows in this table before importing. Continue?",
	"messages.appendConfirm": "Append these rows to the existing data?",
	"messages.invalid": "Please check the entered data.",
	"messages.saved": "Changes saved."
};
const identityVietnamese = {
	"Log in": "Đăng nhập",
	"Use a local account to log in.": "Đăng nhập bằng tài khoản của bạn.",
	"Create a new account.": "Tạo tài khoản mới.",
	"Register": "Đăng ký",
	"Confirm Password": "Xác nhận mật khẩu",
	"Remember me?": "Ghi nhớ đăng nhập",
	"Forgot your password?": "Quên mật khẩu?",
	"Register as a new user": "Đăng ký tài khoản mới",
	"Resend email confirmation": "Gửi lại email xác nhận",
	"Use another service to log in.": "Đăng nhập bằng dịch vụ khác",
	"There are no external authentication services configured. See this": "Chưa cấu hình dịch vụ đăng nhập bên ngoài. Xem",
	"Email": "Email",
	"Password": "Mật khẩu"
};

function localize(root = document) {
	if (document.documentElement.lang !== "en") return;
	root.querySelectorAll("[data-i18n]").forEach((element) => {
		const key = element.dataset.i18n;
		let text = englishText[key];
		if (key === "students.count") text = text?.replace("{count}", element.dataset.count || "0");
		if (text) element.textContent = text;
	});
}

function text(key, vietnamese) {
	return document.documentElement.lang === "en" ? (englishText[key] || vietnamese) : vietnamese;
}

localize();
if (document.documentElement.lang === "vi") {
	const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
	let node;
	while ((node = walker.nextNode())) {
		const source = node.nodeValue.trim();
		if (identityVietnamese[source]) node.nodeValue = node.nodeValue.replace(source, identityVietnamese[source]);
	}
	Object.entries(identityVietnamese).forEach(([source, translated]) => {
		document.querySelectorAll(`input[placeholder="${source}"]`).forEach(input => input.setAttribute("placeholder", translated));
	});
}

const externalServiceMessage = [...document.querySelectorAll("main p")].find(paragraph =>
	paragraph.textContent.includes("There are no external authentication services configured") ||
	paragraph.textContent.includes("Chưa cấu hình dịch vụ đăng nhập bên ngoài"));
if (externalServiceMessage) {
	const externalColumn = externalServiceMessage.closest(".col-md-6") || externalServiceMessage.closest("section");
	externalColumn?.remove();
	const loginForm = document.querySelector('main form input[type="password"]')?.closest("form");
	loginForm?.closest(".col-md-4")?.classList.add("login-form-column");
}

const authForm = document.querySelector("main form#account, main form#registerForm");
if (authForm) {
	const authMain = authForm.closest("main");
	document.body.classList.add("auth-page-active");
	authMain?.classList.add("auth-layout");
	authMain?.querySelector(":scope > h1")?.classList.add("auth-page-title");
	authForm.closest(".col-lg-6")?.classList.add("auth-card");
	authForm.querySelector("h2")?.classList.add("auth-card-title");

	const isRegister = authForm.id === "registerForm";
	if (!isRegister) authForm.querySelector('a[href*="/Account/Register"]')?.closest("p")?.remove();
	const accountSwitch = document.createElement("p");
	accountSwitch.className = "auth-account-switch";
	const accountSwitchText = document.createElement("span");
	const accountSwitchLink = document.createElement("a");
	accountSwitchText.textContent = isRegister
		? (document.documentElement.lang === "en" ? "Already have an account?" : "Đã có tài khoản?")
		: (document.documentElement.lang === "en" ? "New here?" : "Chưa có tài khoản?");
	accountSwitchLink.href = isRegister ? "/Identity/Account/Login" : "/Identity/Account/Register";
	accountSwitchLink.textContent = isRegister
		? (document.documentElement.lang === "en" ? "Sign in" : "Đăng nhập")
		: (document.documentElement.lang === "en" ? "Register" : "Đăng ký");
	accountSwitch.append(accountSwitchText, " ", accountSwitchLink);
	authForm.insertAdjacentElement("afterend", accountSwitch);
}

document.querySelectorAll('input[type="password"]').forEach((input) => {
	const inputContainer = input.parentElement;
	inputContainer.classList.add("password-input-container");

	const toggle = document.createElement("button");
	toggle.type = "button";
	toggle.className = "password-visibility-toggle";
	toggle.setAttribute("aria-controls", input.id);
	toggle.setAttribute("aria-pressed", "false");
	inputContainer.appendChild(toggle);

	const isEnglish = document.documentElement.lang === "en";
	const eyeOpen = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z"/><circle cx="12" cy="12" r="3"/></svg>';
	const eyeClosed = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 3l18 18"/><path d="M10.6 10.6a2 2 0 0 0 2.8 2.8"/><path d="M9.9 5.2A10.7 10.7 0 0 1 12 5c6.4 0 10 7 10 7a15.5 15.5 0 0 1-3.1 3.9"/><path d="M6.2 6.2C3.5 8.1 2 12 2 12s3.6 7 10 7a10.7 10.7 0 0 0 2.1-.2"/></svg>';
	const showLabel = isEnglish ? "Show password" : "Hiện mật khẩu";
	const hideLabel = isEnglish ? "Hide password" : "Ẩn mật khẩu";
	const setVisible = (visible) => {
		input.type = visible ? "text" : "password";
		toggle.innerHTML = visible ? eyeOpen : eyeClosed;
		toggle.setAttribute("aria-label", visible ? hideLabel : showLabel);
		toggle.title = visible ? hideLabel : showLabel;
		toggle.setAttribute("aria-pressed", String(visible));
	};
	setVisible(false);
	toggle.addEventListener("pointerenter", (event) => {
		if (event.pointerType === "mouse") setVisible(true);
	});
	toggle.addEventListener("pointerleave", (event) => {
		if (event.pointerType === "mouse") setVisible(false);
	});
	toggle.addEventListener("pointerdown", (event) => {
		if (event.pointerType === "touch") setVisible(input.type === "password");
	});
	toggle.addEventListener("focus", () => setVisible(true));
	toggle.addEventListener("blur", () => setVisible(false));
});

function showMessage(message) {
	const toast = document.querySelector("#appToast");
	document.querySelector("#appToastMessage").textContent = message;
	bootstrap.Toast.getOrCreateInstance(toast).show();
}

document.addEventListener("click", async (event) => {
	const modalButton = event.target.closest("[data-modal-url]");
	if (modalButton) {
		modalContent.innerHTML = `<div class="modal-body py-5 text-center">${text("messages.loadingForm", "Đang tải biểu mẫu...")}</div>`;
		modal.show();
		const response = await fetch(modalButton.dataset.modalUrl);
		if (!response.ok) {
			modalContent.innerHTML = `<div class="modal-body">${text("messages.formLoadFailed", "Không tải được biểu mẫu.")}</div>`;
			return;
		}
		modalContent.innerHTML = await response.text();
		localize(modalContent);
		return;
	}

	const deleteButton = event.target.closest("[data-delete-url]");
	if (deleteButton && window.confirm(text("messages.deleteConfirm", "Bạn chắc chắn muốn xóa bản ghi này?"))) {
		const token = document.querySelector('#csrfForm input[name="__RequestVerificationToken"]').value;
		const response = await fetch(deleteButton.dataset.deleteUrl, {
			method: "POST",
			headers: { RequestVerificationToken: token }
		});
		const result = await response.json();
		if (!response.ok) {
			showMessage(result.message || text("messages.deleteFailed", "Không thể xóa bản ghi."));
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
			? text("messages.replaceConfirm", "Ghi đè sẽ xóa toàn bộ dữ liệu hiện có trong bảng trước khi nhập. Tiếp tục?")
			: text("messages.appendConfirm", "Xác nhận nối thêm các dòng trong tệp vào dữ liệu hiện có?");
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
			: result.message || text("messages.invalid", "Vui lòng kiểm tra lại dữ liệu.");
		const errorBox = form.querySelector("[data-form-error]");
		errorBox.textContent = messages;
		errorBox.classList.remove("d-none");
		return;
	}

	modal.hide();
	showMessage(result.message || text("messages.saved", "Đã lưu dữ liệu."));
	window.setTimeout(() => window.location.reload(), 450);
});
