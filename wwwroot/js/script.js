document.addEventListener('DOMContentLoaded', function () {
    // Obtiene la fecha actual y la formatea en MM/DD/YYYY
    const currentDate = new Date();
    const month = String(currentDate.getMonth() + 1).padStart(2, '0');
    const day = String(currentDate.getDate()).padStart(2, '0');
    const year = currentDate.getFullYear();
    const formattedDate = `${month}/${day}/${year}`;
    document.getElementById('currentDate').textContent = formattedDate;

    // Inicializa el calendario Flatpickr con el formato MM/DD/YYYY
    flatpickr(".flatpickr", {
        dateFormat: "m/d/Y" // Formato de visualización MM/DD/YYYY
    });

    // Obtiene el nombre del auditor y lo coloca en el campo correspondiente
    fetch('/api/api/getAuditorName')
        .then(response => response.json())
        .then(data => {
            if (data.auditorName) {
                document.getElementById('auditor').value = data.auditorName;
            }
        });

    // Configura el comportamiento del acordeón para mostrar y ocultar paneles
    const acc = document.querySelectorAll(".accordion h2");
    acc.forEach((h2, index) => {
        h2.addEventListener("click", function () {
            this.classList.toggle("active");
            const panel = this.nextElementSibling;
            panel.style.display = panel.style.display === "block" ? "none" : "block";
        });
    });

    const submitAllButton = document.getElementById('submitAll');
    const forms = document.querySelectorAll('.audit-form');

    // Configura el comportamiento de envío para cada formulario de auditoría
    forms.forEach((form, index) => {
        form.addEventListener('submit', function (event) {
            event.preventDefault(); // Previene el envío del formulario
            const formData = new FormData(this);
            const sectionStatus = this.closest('.section-status');
            const header = sectionStatus.querySelector('h2');
            const packaging = formData.get('packaging' + this.id.replace('auditForm', ''));
            const expiration = formData.get('expiration' + this.id.replace('auditForm', ''));
            const fifo = formData.get('fifo' + this.id.replace('auditForm', ''));
            const mixed = formData.get('mixed' + this.id.replace('auditForm', ''));
            const qcSeal = formData.get('qcSeal' + this.id.replace('auditForm', ''));
            const clean = formData.get('clean' + this.id.replace('auditForm', ''));
            const partNumber = formData.get('partNumber' + this.id.replace('auditForm', ''));
            const almacen = document.getElementById('area').value;
            const commentsTextarea = this.querySelector('.comments textarea');

            let isValid = true;
            let commentsText = '';
            const currentDate = new Date();
            const expirationDate = new Date(expiration);
            let resultValue = ''; // Variable para almacenar el resultado a enviar a la base de datos

            // Verifica y agrega comentarios según los problemas encontrados
            if (packaging !== 'OK') {
                isValid = false;
                commentsText += 'Empaque en mal estado.\n';
            }
            if (expirationDate < currentDate) {
                // El químico está caducado, debe ser "Rechazado"
                isValid = false;
                const daysExpired = Math.floor((currentDate - expirationDate) / (1000 * 60 * 60 * 24));
                commentsText += `Químico caducado hace: ${daysExpired} días.\n`;
                resultValue = 'Rechazado';  // Se asegura que el valor en la base de datos será "Rechazado"
            } else {
                const isExpiringSoon = expirationDate.getMonth() === currentDate.getMonth() && expirationDate >= currentDate;
                if (isExpiringSoon) {
                    // El químico está próximo a vencer
                    const daysUntilExpiration = Math.floor((expirationDate - currentDate) / (1000 * 60 * 60 * 24));
                    commentsText += `Químico próximo a vencer en ${daysUntilExpiration} días.\n`;
                    resultValue = 'Próximo a vencer';  // Se asegura que el valor en la base de datos será "Próximo a vencer"
                } else {
                    resultValue = 'Aceptado';  // Se asegura que el valor en la base de datos será "Aceptado"
                }
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

            // Actualiza el estado y el estilo del formulario según los resultados de la auditoría
            if (isValid) {
                if (resultValue === 'Próximo a vencer') {
                    result.textContent = 'Próximo a vencer';
                    result.style.color = 'orange';
                    this.style.border = '2px solid orange';
                    sectionStatus.classList.add('expiring-soon');
                    sectionStatus.classList.remove('accepted', 'rejected');
                    header.classList.add('expiring-soon');
                    header.classList.remove('accepted', 'rejected');
                } else {
                    result.textContent = 'Aceptado';
                    result.style.color = 'green';
                    this.style.border = '2px solid green';
                    sectionStatus.classList.add('accepted');
                    sectionStatus.classList.remove('rejected', 'expiring-soon');
                    header.classList.add('accepted');
                    header.classList.remove('rejected', 'expiring-soon');
                }
                if (commentsTextarea) commentsTextarea.style.display = 'none';
            } else {
                result.textContent = 'Rechazado';
                result.style.color = 'red';
                this.style.border = '2px solid red';
                sectionStatus.classList.add('rejected');
                sectionStatus.classList.remove('accepted', 'expiring-soon');
                header.classList.add('rejected');
                header.classList.remove('accepted', 'expiring-soon');
                if (commentsTextarea) {
                    commentsTextarea.style.display = 'block';
                    commentsTextarea.value = commentsText;
                }
            }

            header.querySelector('.chemical-title').textContent = partNumber;
            header.querySelector('.chemical-comment').innerHTML = commentsText ? commentsText.replace(/\n/g, '<br>') : '';
            header.querySelector('.chemical-comment').style.display = commentsText ? 'block' : 'none';
            header.querySelector('.chemical-comment').style.marginLeft = '20px';

            // Guardar comentario en la base de datos
            if (commentsTextarea) {
                commentsTextarea.value = commentsText;
            }

            // Verifica si todos los formularios han sido completados para mostrar el botón de enviar todos
            if (Array.from(forms).every(f => f.querySelector('.result').textContent !== '')) {
                submitAllButton.style.display = 'block';
            }

            // Oculta el panel actual y muestra el siguiente
            const panel = header.nextElementSibling;
            if (panel) {
                panel.style.display = "none";
                header.classList.remove("active");
            }
            if (index + 1 < acc.length) {
                acc[index + 1].classList.add("active");
                acc[index + 1].nextElementSibling.style.display = "block";
            }

            // Guardar el valor resultante en la base de datos al enviar el formulario
            const formDetails = {
                partNumber: formData.get('partNumber' + this.id.replace('auditForm', '')),
                expirationString: formData.get('expiration' + this.id.replace('auditForm', '')), // Enviar la fecha en formato string
                result: resultValue,  // Se envía el valor correcto ("Rechazado", "Próximo a vencer", "Aceptado")
            };

            // Enviar a la base de datos
            fetch('/api/quimicos', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formDetails)
            })
                .then(response => response.json())
                .then(data => {
                    if (data.message === 'Químicos guardados exitosamente') {
                        alert('Químicos guardados exitosamente');
                        location.reload(); // Recargar la página
                    }
                })
                .catch(error => {
                    console.error('Error al guardar los químicos:', error);
                });
        });
    });

    // Configura el botón de enviar todos para guardar todos los formularios en la base de datos
    submitAllButton.addEventListener('click', function () {
        const almacen = document.getElementById('area').value;
        if (!almacen) {
            alert('Por favor seleccione un área antes de enviar.');
            return;
        }

        const quimicos = Array.from(forms).map(form => {
            const formData = new FormData(form);
            const commentsTextarea = form.querySelector('.comments textarea');
            return {
                partNumber: formData.get('partNumber' + form.id.replace('auditForm', '')),
                packaging: formData.get('packaging' + form.id.replace('auditForm', '')),
                expirationString: formData.get('expiration' + form.id.replace('auditForm', '')), // Enviar la fecha en formato string
                lot: formData.get('lot' + form.id.replace('auditForm', '')),
                fifo: formData.get('fifo' + form.id.replace('auditForm', '')),
                mixed: formData.get('mixed' + form.id.replace('auditForm', '')),
                qcSeal: formData.get('qcSeal' + form.id.replace('auditForm', '')),
                clean: formData.get('clean' + form.id.replace('auditForm', '')),
                comments: commentsTextarea ? commentsTextarea.value : '',
                result: form.querySelector('.result').textContent,
                almacen: almacen
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
                if (data.message === 'Químicos guardados exitosamente') {
                    alert('Químicos guardados exitosamente');
                    location.reload();
                }
            })
            .catch(error => {
                console.error('Error:', error);
            });
    });
});
