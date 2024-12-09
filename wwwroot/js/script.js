'use strict';

document.addEventListener('DOMContentLoaded', function () {
    let chemicalCount = 0;

    const currentDate = new Date();
    const month = String(currentDate.getMonth() + 1).padStart(2, '0');
    const day = String(currentDate.getDate()).padStart(2, '0');
    const year = currentDate.getFullYear();
    document.getElementById('currentDate').textContent = `${month}/${day}/${year}`;

    fetch('/api/api/getAuditorName')
        .then(r => r.json())
        .then(data => {
            if (data.auditorName) {
                document.getElementById('auditor').value = data.auditorName;
            }
        });

    function initializeFlatpickr(container) {
        const dateInputs = container.querySelectorAll('.flatpickr');
        dateInputs.forEach(input => {
            flatpickr(input, {
                dateFormat: "m/d/Y",
                allowInput: false // Deshabilita ingreso manual, solo seleccionar en calendario
            });
        });
    }

    function assignIDs(newSection) {
        chemicalCount++;
        const form = newSection.querySelector('form.audit-form');
        form.id = 'auditForm' + chemicalCount;

        // Obtenemos todos los inputs de texto menos los flatpickr para poder identificar partNumber y lot
        const allTextInputs = newSection.querySelectorAll('input[type="text"]');
        const flatpickrInput = newSection.querySelector('input.flatpickr');
        // Filtrar el flatpickr de los textInputs
        let textArr = Array.from(allTextInputs).filter(el => el !== flatpickrInput);

        const selects = newSection.querySelectorAll('select');

        // Orden esperado:
        // textArr[0] = partNumber
        // flatpickrInput = expiration
        // selects[0] = fifo
        // selects[1] = qcSeal
        // selects[2] = packaging
        // textArr[1] = lot
        // selects[3] = mixed
        // selects[4] = clean

        textArr[0].id = 'partNumber' + chemicalCount;
        textArr[0].name = 'partNumber' + chemicalCount;

        flatpickrInput.id = 'expiration' + chemicalCount;
        flatpickrInput.name = 'expiration' + chemicalCount;

        selects[0].id = 'fifo' + chemicalCount;
        selects[0].name = 'fifo' + chemicalCount;

        selects[1].id = 'qcSeal' + chemicalCount;
        selects[1].name = 'qcSeal' + chemicalCount;

        selects[2].id = 'packaging' + chemicalCount;
        selects[2].name = 'packaging' + chemicalCount;

        textArr[1].id = 'lot' + chemicalCount;
        textArr[1].name = 'lot' + chemicalCount;

        selects[3].id = 'mixed' + chemicalCount;
        selects[3].name = 'mixed' + chemicalCount;

        selects[4].id = 'clean' + chemicalCount;
        selects[4].name = 'clean' + chemicalCount;

        const header = newSection.querySelector('h2.chemical-header .chemical-title');
        header.textContent = 'Químico ' + chemicalCount;

        const commentSpan = newSection.querySelector('h2.chemical-header .chemical-comment');
        commentSpan.id = 'comment' + chemicalCount;
    }

    function addAccordionBehavior(header) {
        header.addEventListener("click", function () {
            this.classList.toggle("active");
            const panel = this.nextElementSibling;
            panel.style.display = panel.style.display === "block" ? "none" : "block";
        });
    }

    function bindListoButtons() {
        const listoButtons = document.querySelectorAll('.listo-button');
        listoButtons.forEach(button => {
            button.removeEventListener('click', listoClickHandler);
            button.addEventListener('click', listoClickHandler);
        });
    }

    function listoClickHandler(e) {
        e.preventDefault();
        e.stopPropagation();
        handleListoClick(e.target);
    }

    function handleListoClick(button) {
        const form = button.closest('form');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const sectionStatus = form.closest('.section-status');
        const header = sectionStatus.querySelector('h2');
        const formNumber = form.id.replace('auditForm', '');
        const formData = new FormData(form);

        const partNumber = formData.get('partNumber' + formNumber) || 'Sin nombre';
        const lot = formData.get('lot' + formNumber) || '';
        const packaging = formData.get('packaging' + formNumber);
        const expiration = formData.get('expiration' + formNumber) || '';
        const fifo = formData.get('fifo' + formNumber);
        const mixed = formData.get('mixed' + formNumber);
        const qcSeal = formData.get('qcSeal' + formNumber);
        const clean = formData.get('clean' + formNumber);
        const commentsTextarea = form.querySelector('.comments textarea');

        let isValid = true;
        let commentsText = '';
        let resultValue = '';

        const [monthStr, dayStr, yearStr] = expiration.split('/');
        const expMonth = parseInt(monthStr, 10);
        const expDay = parseInt(dayStr, 10);
        const expYear = parseInt(yearStr, 10);

        const expirationDate = new Date(expYear, expMonth - 1, expDay);
        const now = new Date();
        const todayMidnight = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        const msInOneDay = 1000 * 60 * 60 * 24;
        const diffMs = expirationDate - todayMidnight;
        const daysUntilExpiration = Math.ceil(diffMs / msInOneDay);

        if (diffMs < 0) {
            isValid = false;
            const daysExpired = Math.abs(Math.floor((now - expirationDate) / msInOneDay));
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

        resultValue = resultValue || (isValid ? 'Aceptado' : 'Rechazado');
        const result = form.querySelector('.result');

        if (isValid) {
            if (resultValue === 'Próximo a vencer') {
                result.textContent = 'Próximo a vencer';
                result.style.color = 'orange';
                form.style.border = '2px solid orange';
                sectionStatus.classList.add('expiring-soon');
                sectionStatus.classList.remove('accepted', 'rejected');
                header.classList.add('expiring-soon');
                header.classList.remove('accepted', 'rejected');
            } else {
                result.textContent = 'Aceptado';
                result.style.color = 'green';
                form.style.border = '2px solid green';
                sectionStatus.classList.add('accepted');
                sectionStatus.classList.remove('rejected', 'expiring-soon');
                header.classList.add('accepted');
                header.classList.remove('rejected', 'expiring-soon');
            }
            if (commentsTextarea) commentsTextarea.style.display = 'none';
        } else {
            result.textContent = 'Rechazado';
            result.style.color = 'red';
            form.style.border = '2px solid red';
            sectionStatus.classList.add('rejected');
            sectionStatus.classList.remove('accepted', 'expiring-soon');
            header.classList.add('rejected');
            header.classList.remove('accepted', 'expiring-soon');
            if (commentsTextarea) {
                commentsTextarea.style.display = 'block';
                commentsTextarea.value = commentsText;
            }
        }

        let titleContainer = header.querySelector('.info-container');
        if (!titleContainer) {
            titleContainer = document.createElement('div');
            titleContainer.classList.add('info-container');

            const infoLeft = document.createElement('div');
            infoLeft.classList.add('info-left');
            titleContainer.appendChild(infoLeft);

            const infoRight = document.createElement('div');
            infoRight.classList.add('info-right');
            titleContainer.appendChild(infoRight);

            while (header.firstChild) {
                header.removeChild(header.firstChild);
            }
            header.appendChild(titleContainer);
        }

        const infoContainer = header.querySelector('.info-container');
        const infoLeft = infoContainer.querySelector('.info-left');
        const infoRight = infoContainer.querySelector('.info-right');

        let existingTitle = infoLeft.querySelector('.chemical-title');
        if (!existingTitle) {
            existingTitle = document.createElement('span');
            existingTitle.classList.add('chemical-title');
            existingTitle.style.color = '#fff';
            infoLeft.appendChild(existingTitle);
        }
        existingTitle.textContent = partNumber;

        let existingLot = infoLeft.querySelector('.chemical-lot');
        if (!existingLot) {
            existingLot = document.createElement('span');
            existingLot.classList.add('chemical-lot');
            existingLot.style.display = 'block';
            existingLot.style.fontSize = '0.9em';
            existingLot.style.color = '#fff';
            infoLeft.appendChild(existingLot);
        }
        existingLot.textContent = lot;

        let commentElement = infoRight.querySelector('.chemical-comment');
        if (!commentElement) {
            commentElement = document.createElement('span');
            commentElement.classList.add('chemical-comment');
            commentElement.style.color = '#fff';
            infoRight.appendChild(commentElement);
        }
        commentElement.innerHTML = commentsText ? commentsText.replace(/\n/g, '<br>') : '';
        commentElement.style.display = commentsText ? 'block' : 'none';

        const panel = header.nextElementSibling;
        if (panel) {
            panel.style.display = "none";
            header.classList.remove('active');
        }

        showNextOrFinishOptions();
    }

    function showNextOrFinishOptions() {
        const optionsContainer = document.getElementById('optionsContainer');
        optionsContainer.innerHTML = '';

        const nextBtn = document.createElement('button');
        nextBtn.textContent = 'Siguiente Químico';
        nextBtn.style.marginRight = '10px';
        nextBtn.addEventListener('click', addNewChemical);

        const finishBtn = document.createElement('button');
        finishBtn.textContent = 'Terminar Auditoría';
        finishBtn.addEventListener('click', finishAuditoria);

        optionsContainer.appendChild(nextBtn);
        optionsContainer.appendChild(finishBtn);
    }

    function addNewChemical() {
        const template = document.getElementById('chemicalTemplate');
        const clone = template.content.cloneNode(true);
        const container = document.getElementById('chemicalsContainer');

        assignIDs(clone);
        const header = clone.querySelector('h2.chemical-header');
        addAccordionBehavior(header);
        initializeFlatpickr(clone);

        container.appendChild(clone);

        const optionsContainer = document.getElementById('optionsContainer');
        optionsContainer.innerHTML = '';

        bindListoButtons();
    }

    function finishAuditoria() {
        const almacen = document.getElementById('area').value;
        if (!almacen) {
            alert('Por favor seleccione un área antes de enviar.');
            return;
        }

        const allForms = document.querySelectorAll('.audit-form');
        const quimicos = Array.from(allForms).map(form => {
            const formNumber = form.id.replace('auditForm', '');
            const formData = new FormData(form);
            const commentsTextarea = form.querySelector('.comments textarea');
            const expirationUI = formData.get('expiration' + formNumber);

            return {
                partNumber: formData.get('partNumber' + formNumber),
                packaging: formData.get('packaging' + formNumber),
                expirationString: expirationUI,
                lot: formData.get('lot' + formNumber),
                fifo: formData.get('fifo' + formNumber),
                mixed: formData.get('mixed' + formNumber),
                qcSeal: formData.get('qcSeal' + formNumber),
                clean: formData.get('clean' + formNumber),
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
                    alert('Auditoría Guardada');
                    location.reload();
                } else {
                    alert('Error al guardar la Auditoría');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Error al guardar la Auditoría');
            });
    }

    // Asignar IDs al primer químico ya presente
    const initialSection = document.querySelector('.section-status');
    assignIDs(initialSection);

    bindListoButtons();

    const initialHeader = initialSection.querySelector('h2.chemical-header');
    if (initialHeader) {
        addAccordionBehavior(initialHeader);
    }

    // Inicializar flatpickr en el químico inicial también
    initializeFlatpickr(document);
});
