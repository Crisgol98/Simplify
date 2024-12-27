$(document).ready(function () {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    $('#calendar').fullCalendar({
        locale: 'es',
        defaultView: 'month',
        events: '/Task/GetTasksForCalendar',
        timeFormat: 'H:mm',
        eventClick: function (event, jsEvent, view) {
            showTaskDetails(event.id);
        },
        height: 'auto'
    });
    $.ajax({
        url: '/Task/GetTasksData',
        method: 'GET',
        success: function (response) {
            $('#totalTasks').text(response.total);
            $('#completedTasks').text(response.completed);
            $('#pendingTasks').text(response.pending);
            $('#overdueTasks').text(response.overdue);
        },
        error: function (error) {
            console.error('Error loading dashboard data:', error);
            showErrorToast('No se pudieron cargar los datos del dashboard');
        }
    });

    $('.schedule-task').click(function () {
        const taskId = $(this).data('task-id');
        showTaskDetails(taskId);
    });
});