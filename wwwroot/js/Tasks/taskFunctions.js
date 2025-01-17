function editTask(taskId) {
    $.ajax({
        url: '/Task/GetDetails',
        method: 'GET',
        data: { taskId: taskId },
        success: function (response) {
            if (response.success) {
                const estimatedTime = response.task.estimatedTime;
                const hours = Math.floor(estimatedTime / 60);
                const minutes = estimatedTime % 60;


                let hoursOptions = '';
                for (let i = 0; i < 24; i++) {
                    const selected = i === hours ? 'selected' : '';
                    hoursOptions += `<option value="${i}" ${selected}>${String(i).padStart(2, '0')}</option>`;
                }

                let minutesOptions = '';
                for (let i = 0; i < 60; i += 5) {
                    const selected = i === minutes ? 'selected' : '';
                    minutesOptions += `<option value="${i}" ${selected}>${String(i).padStart(2, '0')}</option>`;
                }

                const editTaskHTML = `
                    <div class="task-edit-modal">
                        <form id="editTaskForm">
                            <input type="hidden" name="id" value="${response.task.id}">
                            <div class="mb-3">
                                <label for="taskName" class="form-label">Nombre de la Tarea</label>
                                <input type="text" class="form-control" id="taskName" name="name" value="${response.task.name}" required>
                            </div>
                            <div class="mb-3">
                                <label for="taskDescription" class="form-label">Descripción</label>
                                <textarea class="form-control" id="taskDescription" name="description">${response.task.description || ''}</textarea>
                            </div>
                            <div class="mb-3">
                                <label for="taskDueDate" class="form-label">Fecha de Entrega</label>
                                <input type="datetime-local" class="form-control" id="taskDueDate" name="dueDate" value="${response.task.dueDate}" required>
                            </div>
                            <div class="mb-3">
                                <label for="taskEstimatedTime" class="form-label">Duración estimada</label>
                                <div class="d-flex justify-content-center">
                                    <div class="me-2 text-center">
                                        <label for="taskEstimatedTimeHours" class="form-label d-block">Horas</label>
                                        <select class="form-control form-control-sm" id="taskEstimatedTimeHours" name="estimatedTimeHours" required>
                                            ${hoursOptions}
                                        </select>
                                    </div>
                                    <div class="text-center">
                                        <label for="taskEstimatedTimeMinutes" class="form-label d-block">Minutos</label>
                                        <select class="form-control form-control-sm" id="taskEstimatedTimeMinutes" name="estimatedTimeMinutes" required>
                                            ${minutesOptions}
                                        </select>
                                    </div>
                                </div>
                            </div>
                            <div class="mb-3">
                                <label for="taskPriority" class="form-label">Prioridad</label>
                                <select class="form-control" id="taskPriority" name="priority">
                                    <option value="1" ${response.task.priority === 'Baja' ? 'selected' : ''}>Baja</option>
                                    <option value="2" ${response.task.priority === 'Media' ? 'selected' : ''}>Media</option>
                                    <option value="3" ${response.task.priority === 'Alta' ? 'selected' : ''}>Alta</option>
                                </select>
                            </div>
                        </form>
                    </div>
                `;

                Swal.fire({
                    title: 'Editar Tarea',
                    html: editTaskHTML,
                    showCancelButton: true,
                    confirmButtonText: 'Guardar',
                    cancelButtonText: 'Cancelar',
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    preConfirm: () => {
                        const hours = parseInt($('#taskEstimatedTimeHours').val());
                        const minutes = parseInt($('#taskEstimatedTimeMinutes').val());
                        const estimatedTimeInMinutes = (hours * 60) + minutes;

                        const formData = $('#editTaskForm').serializeArray();
                        formData.push({ name: "estimatedTime", value: estimatedTimeInMinutes });
                        formData.push({ name: "remainingTime", value: estimatedTimeInMinutes });
                        $.ajax({
                            url: '/Task/Edit',
                            method: 'POST',
                            data: formData,
                            success: function (response) {
                                if (response.success) {
                                    Swal.fire({
                                        title: 'Tarea Editada',
                                        text: 'La tarea ha sido actualizada correctamente.',
                                        icon: 'success',
                                        timer: 5000,
                                        didClose: () => {
                                            location.reload();
                                        }
                                    });
                                } else {
                                    Swal.fire('Error', response.message, 'error');
                                }
                            },
                            error: function () {
                                Swal.fire('Error', 'No se pudo actualizar la tarea', 'error');
                            }
                        });
                    }
                });
            } else {
                showErrorToast(response.message);
            }
        },
        error: function () {
            showErrorToast('Error al cargar los detalles de la tarea');
        }
    });
}
function addTask() {
    const addTaskHTML = `
    <div class="task-add-modal">
        <form id="addTaskForm">
            <div class="alert alert-info d-flex align-items-center" role="alert" style="font-size: 14px; background-color: transparent; border: none;">
                <i class="fas fa-info-circle" style="color: red; font-size: 18px;"></i>
                <span class="ms-2" style="color: red; font-size: 14px;">Si añades una tarea se te organizará el horario de nuevo.</span>
            </div>
            <div class="mb-3">
                <label for="taskName" class="form-label">Nombre de la Tarea</label>
                <input type="text" class="form-control" id="taskName" name="name" required>
            </div>
            <div class="mb-3">
                <label for="taskDescription" class="form-label">Descripción</label>
                <textarea class="form-control" id="taskDescription" name="description"></textarea>
            </div>
            <div class="mb-3">
                <label for="taskDueDate" class="form-label">Fecha de Entrega</label>
                <input type="datetime-local" class="form-control" id="taskDueDate" name="dueDate" required>
            </div>
            <div class="mb-3">
                <label for="taskEstimatedTime" class="form-label">Duración estimada</label>
                <input type="time" class="form-control form-control-sm" id="taskEstimatedTime" name="estimatedTime" required>
            </div>
            <div class="mb-3">
                <label for="taskPriority" class="form-label">Prioridad</label>
                <select class="form-control" id="taskPriority" name="priority">
                    <option value="1">Baja</option>
                    <option value="2">Media</option>
                    <option value="3">Alta</option>
                </select>
            </div>
        </form>
    </div>
    `;

    Swal.fire({
        title: 'Nueva Tarea',
        html: addTaskHTML,
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        preConfirm: () => {
            // Obtenemos el valor del input "time"
            const estimatedTime = $('#taskEstimatedTime').val();
            const [hours, minutes] = estimatedTime.split(':');  // Extraemos horas y minutos

            // Convertimos el tiempo estimado en minutos
            const estimatedTimeInMinutes = (parseInt(hours) * 60) + parseInt(minutes);

            const formData = $('#addTaskForm').serializeArray();
            formData.forEach((field) => {
                if (field.name === 'estimatedTime') {
                    field.value = estimatedTimeInMinutes;
                }
            });
            formData.push({ name: "remainingTime", value: estimatedTimeInMinutes });

            return $.ajax({
                url: '/Task/Add',
                method: 'POST',
                data: formData,
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            title: 'Tarea Creada',
                            text: 'La tarea ha sido creada correctamente.',
                            icon: 'success',
                            timer: 5000,
                            didClose: () => {
                                location.reload();
                            }
                        });
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error', 'No se pudo crear la tarea', 'error');
                }
            });
        }
    });
}

function deleteTask(taskId) {
    Swal.fire({
        title: '¿Estás seguro?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, cancelar',
        cancelButtonText: 'Atrás',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        preConfirm: () => {
            $.ajax({
                url: '/Task/Delete',
                type: 'POST',
                cache: false,
                data: { taskId: taskId },
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            title: 'Tarea Cancelada',
                            text: 'La tarea ha sido cancelada correctamente.',
                            icon: 'success',
                            timer: 5000,
                            didClose: () => {
                                location.reload();
                            }
                        });
                    } else {
                        Swal.fire('Error', response.message, 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error', 'No se pudo Cancelar la tarea', 'error');
                }
            });
        }
    });
}
function updateState(taskId) {
    $.ajax({
        url: '/Task/GetDetails',
        method: 'GET',
        data: { taskId: taskId },
        success: function (response) {
            if (response.success) {
                const currentState = response.task.state;
                const remainingTime = response.task.remainingTime;
                const workedHours = Math.floor(remainingTime / 60);
                const workedMinutes = remainingTime % 60;
                const timeValue = `${String(workedHours).padStart(2, '0')}:${String(workedMinutes).padStart(2, '0')}`;

                // Mostrar el campo de horas trabajadas solo si el estado no es "Completada"
                const hoursSection = currentState !== 'Completada' ? `
                    <div class="mb-3" id="workedTimeSection">
                        <label class="form-label">Introduce el tiempo que has trabajado en la tarea</label>
                        <div class="d-flex justify-content-center">
                            <div class="me-2 text-center">
                                <label for="workedTime" class="form-label d-block">Tiempo trabajado</label>
                                <input type="time" class="form-control form-control-sm" id="workedTime" name="workedTime" value="${timeValue}" required>
                            </div>
                        </div>
                    </div>
                ` : '';

                const updateStateHTML = `
                    <div class="task-update-modal">
                        <form id="updateStateForm">
                            <input type="hidden" name="id" value="${response.task.id}">
                            <div class="mb-3">
                                <label for="taskState" class="form-label">Estado Actual</label>
                                <select class="form-control" id="taskState" name="state">
                                    <option value="En proceso" ${currentState === 'En Proceso' ? 'selected' : ''}>En Proceso</option>
                                    <option value="Finalizado" ${currentState === 'Completada' ? 'selected' : ''}>Completada</option>
                                </select>
                            </div>
                            ${hoursSection}
                        </form>
                    </div>
                `;

                Swal.fire({
                    title: 'Actualizar Estado',
                    html: updateStateHTML,
                    showCancelButton: true,
                    confirmButtonText: 'Guardar',
                    cancelButtonText: 'Cancelar',
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    preConfirm: () => {
                        const timeValue = $('#workedTime').val();
                        const formData = $('#updateStateForm').serializeArray();
                        if (timeValue) {
                            const [hours, minutes] = timeValue.split(':').map(num => parseInt(num));
                            const workedTimeInMinutes = (hours * 60) + minutes;
                            formData.push({ name: "remainingTime", value: workedTimeInMinutes });
                        }
                        $.ajax({
                            url: '/Task/UpdateState',
                            method: 'POST',
                            data: formData,
                            success: function (response) {
                                if (response.success) {
                                    Swal.fire({
                                        title: 'Estado Actualizado',
                                        text: 'El estado de la tarea ha sido actualizado correctamente.',
                                        icon: 'success',
                                        timer: 5000,
                                        didClose: () => {
                                            location.reload();
                                        }
                                    });
                                } else {
                                    Swal.fire('Error', response.message, 'error');
                                }
                            },
                            error: function () {
                                Swal.fire('Error', 'No se pudo actualizar el estado de la tarea', 'error');
                            }
                        });
                    }
                });

                $(document).on('change', '#taskState', function () {
                    const selectedState = $(this).val();
                    if (selectedState === 'Finalizado') {
                        $('#workedTimeSection').remove(); // Elimina la sección de tiempo trabajado
                    } else {
                        if (!$('#workedTimeSection').length) {
                            const newHoursSection = `
                                <div class="mb-3" id="workedTimeSection">
                                    <label class="form-label">Introduce el tiempo que has trabajado en la tarea</label>
                                    <div class="d-flex justify-content-center">
                                        <div class="me-2 text-center">
                                            <label for="workedTime" class="form-label d-block">Tiempo trabajado</label>
                                            <input type="time" class="form-control form-control-sm" id="workedTime" name="workedTime" required>
                                        </div>
                                    </div>
                                </div>
                            `;
                            $('#updateStateForm').append(newHoursSection);
                        }
                    }
                });
            } else {
                showErrorToast(response.message);
            }
        },
        error: function () {
            showErrorToast('Error al cargar los detalles de la tarea');
        }
    });
}
function showTaskDetails(taskId) {
    $.ajax({
        url: '/Task/GetDetails',
        method: 'GET',
        data: { taskId: taskId },
        success: function (response) {
            if (response.success) {

                const taskDetailsHTML = `
                    <div class="task-details p-4">
                        <h3 class="task-details__title mb-3">${response.task.name}</h3>
                        <div class="task-details__info mb-4">
                            <p><strong>Fecha de entrega:</strong> ${formatDate(response.task.dueDate)}</p>
                            <p><strong>Estado:</strong> 
                                <span class="badge ${response.task.isCompleted ? 'bg-success' : 'bg-warning'}">
                                    ${response.task.state == 'Finalizado' ? 'Completada' : 'En proceso'}
                                </span>
                            </p>
                            <p><strong>Prioridad:</strong>
                                <span class="badge ${response.task.priority === 'Alta' ? 'bg-danger' : response.task.priority === 'Media' ? 'bg-warning' : response.task.priority === 'Baja' ? 'bg-success' : 'bg-secondary'}">
                                    ${response.task.priority === 'Alta' ? 'Alta' : response.task.priority === 'Media' ? 'Media' : response.task.priority === 'Baja' ? 'Baja' : 'No especificada'}
                                </span>
                            </p>
                            <p><strong>Tiempo estimado:</strong> ${formatEstimatedTime(response.task.estimatedTime)}</p>
                            <p><strong>Descripción:</strong></p>
                            <p>${response.task.description || 'Sin descripción'}</p>
                            ${response.task.location ? `<p><strong>Ubicación:</strong> ${response.task.location}</p>` : ''}
                        </div>
                        <div class="task-details__actions text-center">
                            <button class="btn btn-primary btn-sm mr-2" 
                                    onclick="editTask(${response.task.id})" 
                                    title="Editar">
                                <i class="fas fa-edit"></i> Editar
                            </button>
                            <button class="btn btn-primary btn-sm mr-2"
                                    onclick="updateState(${response.task.id})" 
                                    title="Estado">
                                <i class="fas fa-sync-alt"></i> Actualizar estado
                            </button>
                            <button class="btn btn-danger btn-sm" 
                                    onclick="deleteTask(${response.task.id})" 
                                    title="Cancelar">
                                <i class="fas fa-trash"></i> Cancelar
                            </button>
                        </div>
                    </div>
                `;


                Swal.fire({
                    html: taskDetailsHTML,
                    showCloseButton: true,
                    showCancelButton: false,
                    confirmButtonText: 'Cerrar',
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    width: '600px',
                    customClass: {
                        popup: 'border-radius-custom',
                    }
                });
            } else {
                showErrorToast(response.message);
            }
        },
        error: function (error) {
            showErrorToast('Error al cargar los detalles de la tarea');
        }
    });
}
function showErrorToast(message) {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message,
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000
    });
}
function formatTime(timeString) {
    const date = new Date(timeString);
    return date.toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit'
    });
}
function formatEstimatedTime(timeString) {
    const timeInMinutes = parseInt(timeString);
    let hours = Math.floor(timeInMinutes / 60);
    let minutes = timeInMinutes % 60;

    if (minutes == 0) {
        return `${hours} h`;
    } else if (hours > 0) {
        return `${hours} h ${minutes} m`
    }
    else {
        return `${minutes} m`;
    }
}
function formatDate(dateString) {
    const date = new Date(dateString);

    return date.toLocaleString('es-ES', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
    }).replace(',', ' - ');
}
function getPriorityClass(priority) {
    const classes = {
        'Alta': 'priority-high',
        'Media': 'priority-medium',
        'Baja': 'priority-low'
    };
    return classes[priority] || 'priority-medium';
}
function getPriorityBadge(priority) {
    switch (priority) {
        case 'high':
            return 'bg-primary';
        case 'medium':
            return 'bg-secondary';
        case 'low':
            return 'bg-warning';
        default:
            return 'bg-light';
    }
}