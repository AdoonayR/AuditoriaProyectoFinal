document.addEventListener('DOMContentLoaded', function () {
    const currentDate = new Date();
    const month = String(currentDate.getMonth() + 1).padStart(2, '0');
    const day = String(currentDate.getDate()).padStart(2, '0');
    const year = currentDate.getFullYear();
    const formattedDate = `${day}/${month}/${year}`;
    document.getElementById('currentDate').textContent = formattedDate;

    const acc = document.querySelectorAll(".accordion h2");
    acc.forEach((h2) => {
        h2.addEventListener("click", function () {
            this.classList.toggle("active");
            const panel = this.nextElementSibling;
            if (panel.style.display === "block") {
                panel.style.display = "none";
            } else {
                panel.style.display = "block";
            }
        });
    });

    document.querySelectorAll('input[type="date"]').forEach(dateInput => {
        dateInput.addEventListener('change', function () {
            const selectedDate = new Date(this.value);
            const currentDate = new Date();
            if (selectedDate < currentDate) {
                this.classList.add('expired');
                this.classList.remove('expiring-soon');
            } else if (selectedDate.getMonth() === currentDate.getMonth() && selectedDate.getFullYear() === currentDate.getFullYear()) {
                this.classList.add('expiring-soon');
                this.classList.remove('expired');
            } else {
                this.classList.remove('expired', 'expiring-soon');
            }
        });
    });

    document.querySelectorAll('input[type="text"]').forEach(textInput => {
        textInput.addEventListener('input', function () {
            this.value = this.value.toUpperCase();
        });
    });

    const submitAllButton = document.getElementById('submitAll');
    const forms = document.querySelectorAll('.auditForm');

    forms.forEach(form => {
        form.addEventListener('submit', function (event) {
            event.preventDefault();
            const formData = new FormData(this);
            const sectionStatus = this.closest('.section-status');
            const header = sectionStatus.querySelector('h2');
            const packaging = formData.get('packaging' + this.id.replace('auditForm', ''));
            const expiration = new Date(formData.get('expiration' + this.id.replace('auditForm', '')));
            const fifo = formData.get('fifo' + this.id.replace('auditForm', ''));
            const mixed = formData.get('mixed' + this.id.replace('auditForm', ''));
            const qcSeal = formData.get('qcSeal' + this.id.replace('auditForm', ''));
            const clean = formData.get('clean' + this.id.replace('auditForm', ''));
            const partNumber = formData.get('partNumber' + this.id.replace('auditForm', ''));
            const comments = this.querySelector('.comments');
            const commentsTextarea = comments.querySelector('textarea');

            let isValid = true;
            let commentsText = '';
            const currentDate = new Date();

            if (packaging !== 'OK') {
                isValid = false;
                commentsText += 'Empaque en mal estado.\n';
            }
            if (expiration < currentDate) {
                isValid = false;
                const daysExpired = Math.floor((currentDate - expiration) / (1000 * 60 * 60 * 24));
                commentsText += `Químico caducado desde hace: ${daysExpired} días.\n`;
            }
            if (fifo !== 'Sí') {
                isValid = false;
                commentsText += 'No se está cumpliendo FIFO.\n';
            }
            if (mixed !== 'No') {
                isValid = false;
                commentsText += 'Químicos mezclados.\n';
            }
            if (qcSeal !== 'Sí') {
                isValid = false;
                commentsText += 'No cuenta con sello de calidad.\n';
            }
            if (clean !== 'Limpio') {
                isValid = false;
                commentsText += 'Limpieza del químico en mal estado.\n';
            }

            const result = this.querySelector('.result');
            if (isValid) {
                result.textContent = 'Aceptado';
                result.style.color = 'green';
                this.style.border = '2px solid green';
                sectionStatus.classList.add('accepted');
                sectionStatus.classList.remove('rejected', 'expiring-soon');
                header.classList.add('accepted');
                header.classList.remove('rejected', 'expiring-soon');
                comments.style.display = 'none';
            } else {
                result.textContent = 'Rechazado';
                result.style.color = 'red';
                this.style.border = '2px solid red';
                sectionStatus.classList.add('rejected');
                sectionStatus.classList.remove('accepted', 'expiring-soon');
                header.classList.add('rejected');
                header.classList.remove('accepted', 'expiring-soon');
                comments.style.display = 'block';
                commentsTextarea.value = commentsText;
            }

            const isExpiringSoon = expiration.getMonth() === currentDate.getMonth() && expiration.getFullYear() === currentDate.getFullYear();
            if (isExpiringSoon && expiration >= currentDate) {
                const daysUntilExpiration = Math.floor((expiration - currentDate) / (1000 * 60 * 60 * 24));
                commentsText += `Químico próximo a vencer en ${daysUntilExpiration} días.\n`;
                result.textContent = 'Aceptado'; 
                result.style.color = 'orange';
                this.style.border = '2px solid orange';
                sectionStatus.classList.add('expiring-soon');
                sectionStatus.classList.remove('accepted', 'rejected');
                header.classList.add('expiring-soon');
                header.classList.remove('accepted', 'rejected');
            }

            header.textContent = partNumber;

            if (Array.from(forms).every(f => f.querySelector('.result').textContent !== '')) {
                submitAllButton.style.display = 'block';
            }
        });
    });

    submitAllButton.addEventListener('click', function () {
        const quimicos = Array.from(forms).map(form => {
            const formData = new FormData(form);
            return {
                partNumber: formData.get('partNumber' + form.id.replace('auditForm', '')),
                packaging: formData.get('packaging' + form.id.replace('auditForm', '')),
                expiration: formData.get('expiration' + form.id.replace('auditForm', '')),
                lot: formData.get('lot' + form.id.replace('auditForm', '')),
                fifo: formData.get('fifo' + form.id.replace('auditForm', '')),
                mixed: formData.get('mixed' + form.id.replace('auditForm', '')),
                qcSeal: formData.get('qcSeal' + form.id.replace('auditForm', '')),
                clean: formData.get('clean' + form.id.replace('auditForm', '')),
                comments: form.querySelector('.comments textarea').value,
                result: form.querySelector('.result').textContent
            };
        });

        fetch('/api/quimicos', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(quimicos)
        })
            .then(response => response.json())
            .then(data => {
                alert('Químicos guardados exitosamente');
            })
            .catch(error => {
                console.error('Error:', error);
            });
    });
});
