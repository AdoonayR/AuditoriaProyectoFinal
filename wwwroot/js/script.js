// Evento que espera a que el documento esté cargado completamente
document.addEventListener('DOMContentLoaded', function () {
    // Obtiene la fecha actual y la formatea en mm/dd/yyyy
    const currentDate = new Date();
    const month = String(currentDate.getMonth() + 1).padStart(2, '0');
    const day = String(currentDate.getDate()).padStart(2, '0');
    const year = currentDate.getFullYear();
    const formattedDate = `${month}/${day}/${year}`;
    document.getElementById('currentDate').textContent = formattedDate;

    // Configuración de Flatpickr para formatear los campos de fecha
    flatpickr(".flatpickr", {
        dateFormat: "m/d/Y"
    });

    // Llamada a la API para obtener el nombre del auditor y asignarlo al input de auditor
    fetch('/api/api/getAuditorName')
        .then(response => response.json())
        .then(data => {
            if (data.auditorName) {
                document.getElementById('auditor').value = data.auditorName;
            }
        });

    // Configuración de cada acordeón del formulario para expandirse/colapsarse al hacer clic
    const acc = document.querySelectorAll(".accordion h2");
    acc.forEach((h2, index) => {
        h2.addEventListener("click", function () {
            this.classList.toggle("active");
            const panel = this.nextElementSibling;
            panel.style.display = panel.style.display === "block" ? "none" : "block";
        });
    });

    // Configuración de los formularios y el botón para enviar todos
    const submitAllButton = document.getElementById('submitAll');
    const forms = document.querySelectorAll('.audit-form');

    // Procesa cada formulario al hacer submit
    forms.forEach((form, index) => {
        form.addEventListener('submit', function (event) {
            event.preventDefault(); // Evita el envío por defecto
            const formData = new FormData(this); // Extrae datos del formulario
            const sectionStatus = this.closest('.section-status'); // Contenedor de estado de la sección
            const header = sectionStatus.querySelector('h2'); // Encabezado del formulario para los cambios de estado
            const partNumber = formData.get('partNumber' + this.id.replace('auditForm', ''));
            const lot = formData.get('lot' + this.id.replace('auditForm', ''));
            const packaging = formData.get('packaging' + this.id.replace('auditForm', ''));
            const expiration = formData.get('expiration' + this.id.replace('auditForm', ''));
            const fifo = formData.get('fifo' + this.id.replace('auditForm', ''));
            const mixed = formData.get('mixed' + this.id.replace('auditForm', ''));
            const qcSeal = formData.get('qcSeal' + this.id.replace('auditForm', ''));
            const clean = formData.get('clean' + this.id.replace('auditForm', ''));
            const almacen = document.getElementById('area').value;
            const commentsTextarea = this.querySelector('.comments textarea');

            let isValid = true; // Para definir si el químico cumple con las condiciones
            let commentsText = ''; // Texto para los comentarios de errores
            const currentDate = new Date();
            const expirationDate = new Date(expiration);
            let resultValue = '';

            // Cálculo de días hasta el vencimiento ajustado para casos especiales
            const msInOneDay = 1000 * 60 * 60 * 24;
            const daysUntilExpiration = Math.ceil((expirationDate - currentDate) / msInOneDay);

            // Validación de la fecha de vencimiento y otros campos
            if (expirationDate < currentDate) {
                isValid = false;
                const daysExpired = Math.floor((currentDate - expirationDate) / msInOneDay);
                commentsText += `Químico caducado hace: ${daysExpired} días.\n`;
                resultValue = 'Rechazado';
            } else {
                if (daysUntilExpiration > 30) {
                    resultValue = 'Aceptado';
                } else {
                    commentsText += `Químico próximo a vencer en ${daysUntilExpiration} días.\n`;
                    resultValue = 'Próximo a vencer';
                }
            }

            // Validación de cada campo
            if (packaging !== 'OK') {
                isValid = false;
                commentsText += 'Empaque en mal estado.\n';
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

            // Mostrar el resultado visual en el formulario
            const result = this.querySelector('.result');
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

            // Creación del contenedor de información en el encabezado
            let titleContainer = header.querySelector('.info-container');
            if (!titleContainer) {
                titleContainer = document.createElement('div');
                titleContainer.classList.add('info-container');
                header.innerHTML = '';
                header.appendChild(titleContainer);
            }

            // Actualización de campos de nombre y lote de químico
            let partNumberElement = titleContainer.querySelector('.chemical-title');
            if (!partNumberElement) {
                partNumberElement = document.createElement('span');
                partNumberElement.classList.add('chemical-title');
                partNumberElement.style.color = '#fff';
                titleContainer.appendChild(partNumberElement);
            }
            partNumberElement.textContent = partNumber;

            let lotElement = titleContainer.querySelector('.chemical-lot');
            if (!lotElement) {
                lotElement = document.createElement('span');
                lotElement.classList.add('chemical-lot');
                lotElement.style.display = 'block';
                lotElement.style.fontSize = '0.9em';
                lotElement.style.color = '#fff';
                titleContainer.appendChild(lotElement);
            }
            lotElement.textContent = lot;

            // Configuración de comentarios y su visualización
            let commentElement = header.querySelector('.chemical-comment');
            if (!commentElement) {
                commentElement = document.createElement('span');
                commentElement.classList.add('chemical-comment');
                commentElement.style.color = '#fff';
                commentElement.style.marginLeft = 'auto';
                header.appendChild(commentElement);
            }
            commentElement.innerHTML = commentsText ? commentsText.replace(/\n/g, '<br>') : '';
            commentElement.style.display = commentsText ? 'block' : 'none';

            if (commentsTextarea) {
                commentsTextarea.value = commentsText;
            }

            if (Array.from(forms).every(f => f.querySelector('.result').textContent !== '')) {
                submitAllButton.style.display = 'block';
            }

            const panel = header.nextElementSibling;
            if (panel) {
                panel.style.display = "none";
                header.classList.remove("active");
            }
            if (index + 1 < acc.length) {
                acc[index + 1].classList.add("active");
                acc[index + 1].nextElementSibling.style.display = "block";
            }

            // Creación del objeto de detalles del formulario y su envío
            const formDetails = {
                partNumber: partNumber,
                lot: lot,
                expirationString: expiration,
                result: resultValue,
            };

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
                        alert('Auditoria Guardada');
                        location.reload();
                    }
                })
                .catch(error => {
                    console.error('Error al guardar la Auditoria', error);
                });
        });
    });

    // Envío de todos los datos de los químicos en un solo envío al presionar "submitAllButton"
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
                expirationString: formData.get('expiration' + form.id.replace('auditForm', '')),
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
                    alert('Auditoria Guardada');
                    location.reload();
                }
            })
            .catch(error => {
                console.error('Error:', error);
            });
    });
});
