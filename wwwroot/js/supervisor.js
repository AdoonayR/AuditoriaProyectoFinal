document.addEventListener("DOMContentLoaded", function () {
    const approveButtons = document.querySelectorAll(".approve-btn");

    approveButtons.forEach(button => {
        button.addEventListener("click", function () {
            const date = this.getAttribute("data-date");
            const role = this.getAttribute("data-role");

            fetch(`/Supervisor/ApproveIncoming`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({ date })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.message) {
                        alert(data.message);
                        window.location.reload();
                    }
                })
                .catch(error => console.error("Error:", error));
        });
    });
});
